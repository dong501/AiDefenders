using System.Collections;
using UnityEngine;

/// <summary>Articulated chapter units. Movement and combat remain owned by Target_Base.</summary>
public sealed class ChapterUnitVisual : MonoBehaviour
{
    public Transform body;
    public Transform[] limbs = new Transform[0];
    public Transform[] wheels = new Transform[0];
    public Renderer[] warningLamps = new Renderer[0];
    public Light[] headlights = new Light[0];
    public Renderer[] headlightLenses = new Renderer[0];
    public Transform[] breakawayPanels = new Transform[0];
    public GameObject wreckBurst;
    public SfxEvent wreckSound;
    public bool zombie;
    public bool vehicle;
    public float gait = 1f;
    Quaternion[] rest;
    Vector3 bodyRest;
    Target_Base target;
    Vector3 previous;
    float phase;

    public bool CanPlayDefeat => body != null;

    void Awake()
    {
        target = GetComponent<Target_Base>();
        rest = new Quaternion[limbs.Length];
        for (int i = 0; i < limbs.Length; i++) if (limbs[i]) rest[i] = limbs[i].localRotation;
        if (body) bodyRest = body.localPosition;
        previous = transform.position;
        phase = Random.value * Mathf.PI * 2f;
        if (vehicle && gameObject.scene.name == "Desert_1")
            TurnOffHeadlights();
    }

    void LateUpdate()
    {
        if (target == null || target.isSample || !target.IsActiveInPlay) return;
        float distance = Vector3.Distance(previous, transform.position);
        previous = transform.position;
        if (Time.timeScale <= 0f) return;
        phase += Mathf.Min(distance, 1f) * (vehicle ? 2f : 4.2f) * gait;
        Pose(phase, distance > .0001f ? 1f : .08f);
        foreach (var wheel in wheels) if (wheel) wheel.Rotate(Vector3.right, distance * 170f, Space.Self);
        // Renderer enable toggling keeps sirens readable without allocating material instances.
        for (int i = 0; i < warningLamps.Length; i++)
            if (warningLamps[i]) warningLamps[i].enabled = Mathf.Sin(Time.time * 8f + i * Mathf.PI) > -.25f;
    }

    void Pose(float value, float strength)
    {
        if (rest == null) Awake();
        for (int i = 0; i < limbs.Length; i++) if (limbs[i])
        {
            float sign = (i == 0 || i == 3) ? 1f : -1f;
            float swing = Mathf.Sin(value) * (zombie ? 19f : 30f) * strength * sign;
            limbs[i].localRotation = rest[i] * Quaternion.Euler(swing, 0, zombie ? Mathf.Sin(value * .5f + i) * 3f : 0);
        }
        if (body) body.localPosition = bodyRest + Vector3.up * Mathf.Abs(Mathf.Sin(value)) * (vehicle ? .015f : .035f) * strength;
    }

    public void RandomizeSamplePose()
    {
        if (rest == null) Awake();
        Pose(Random.value * Mathf.PI * 2f, 1f);
    }

    public IEnumerator PlayDefeat(float holdTime)
    {
        float side = Mathf.Sin(phase) < 0f ? -1f : 1f;
        float duration = vehicle ? .48f : .72f;
        if (vehicle)
            yield return Wreck(duration, side);
        else
            yield return Fall(duration, side);

        float restTime = Mathf.Max(0f, holdTime - duration);
        if (restTime > 0f)
            yield return new WaitForSeconds(restTime);

        const float sinkTime = .6f;
        Vector3 settled = body.localPosition;
        float depth = vehicle ? 2.8f : .85f;
        for (float elapsed = 0f; elapsed < sinkTime; elapsed += Time.deltaTime)
        {
            body.localPosition = settled + Vector3.down * depth * (elapsed / sinkTime);
            yield return null;
        }
    }

    IEnumerator Fall(float duration, float side)
    {
        Quaternion startRotation = body.localRotation;
        Vector3 startPosition = body.localPosition;
        Quaternion[] startLimbs = new Quaternion[limbs.Length];
        for (int i = 0; i < limbs.Length; i++)
            if (limbs[i]) startLimbs[i] = limbs[i].localRotation;

        Quaternion recoil = startRotation * Quaternion.Euler(-12f, 0f, side * 8f);
        Quaternion fallen = startRotation * Quaternion.Euler(86f, side * 12f, side * 17f);
        Vector3 staggered = startPosition + new Vector3(side * .08f, -.13f, 0f);
        Vector3 grounded = bodyRest + new Vector3(side * .2f, .32f, zombie ? .62f : .37f);

        for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
        {
            float t = elapsed / duration;
            float impact = Mathf.SmoothStep(0f, 1f, t / .23f);
            float fall = Mathf.Clamp01((t - .23f) / .77f);
            fall *= fall;
            body.localRotation = Quaternion.Slerp(Quaternion.Slerp(startRotation, recoil, impact), fallen, fall);
            body.localPosition = Vector3.Lerp(Vector3.Lerp(startPosition, staggered, impact), grounded, fall);
            SetFallenLimbs(startLimbs, side, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        body.localRotation = fallen;
        body.localPosition = grounded;
        SetFallenLimbs(startLimbs, side, 1f);
    }

    void SetFallenLimbs(Quaternion[] start, float side, float t)
    {
        for (int i = 0; i < limbs.Length; i++)
        {
            if (!limbs[i]) continue;
            bool arm = i < 2;
            float outward = i % 2 == 0 ? -1f : 1f;
            Quaternion pose = Quaternion.Euler(
                arm ? -35f + 17f * outward * side : -16f * outward * side,
                0f, outward * (arm ? 32f : 9f));
            limbs[i].localRotation = Quaternion.Slerp(start[i], pose, t);
        }
    }

    IEnumerator Wreck(float duration, float side)
    {
        Vector3 origin = body.position + Vector3.up * .9f;
        float scale = 1f;
        if (target.TryGetRenderBounds(out Bounds bounds))
        {
            origin = bounds.center;
            scale = Mathf.Clamp(Mathf.Max(bounds.size.x, bounds.size.z) / 4f, .85f, 1.35f);
        }
        GameObject burst = CombatVfx.Play(wreckBurst, origin, Quaternion.identity, null, 2.8f);
        if (burst != null) burst.transform.localScale *= scale;
        GameSfx.PlayWorld(wreckSound, origin);
        foreach (var lamp in warningLamps)
            if (lamp) lamp.enabled = false;
        ScorchBody();
        TurnOffHeadlights();

        Quaternion startRotation = body.localRotation;
        Quaternion wrecked = startRotation * Quaternion.Euler(8f, side * 18f, -side * 12f);
        Vector3 startPosition = body.localPosition;
        Vector3 endPosition = bodyRest + new Vector3(side * .28f, .08f, .63f);
        var panelPositions = new Vector3[breakawayPanels.Length];
        var panelRotations = new Quaternion[breakawayPanels.Length];
        for (int i = 0; i < breakawayPanels.Length; i++)
        {
            Transform panel = breakawayPanels[i];
            if (!panel) continue;
            panelPositions[i] = panel.localPosition;
            panelRotations[i] = panel.localRotation;
        }

        for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
        {
            float t = elapsed / duration;
            float settle = 1f - (1f - t) * (1f - t);
            body.localRotation = Quaternion.Slerp(startRotation, wrecked, settle);
            body.localPosition = Vector3.Lerp(startPosition, endPosition, settle)
                + Vector3.up * Mathf.Sin(t * Mathf.PI) * .2f;
            SetWreckPanels(panelPositions, panelRotations, t);
            yield return null;
        }

        body.localRotation = wrecked;
        body.localPosition = endPosition;
        SetWreckPanels(panelPositions, panelRotations, 1f);
    }

    void SetWreckPanels(Vector3[] start, Quaternion[] rotations, float t)
    {
        for (int i = 0; i < breakawayPanels.Length; i++)
        {
            Transform panel = breakawayPanels[i];
            if (!panel) continue;
            float side = Mathf.Sign(start[i].x);
            panel.localPosition = start[i] + new Vector3(side * t * .8f,
                Mathf.Sin(t * Mathf.PI) * .85f - t * .5f, -t * .14f);
            panel.localRotation = rotations[i] * Quaternion.Euler(48f * t, side * 70f * t, side * 32f * t);
        }
    }

    void TurnOffHeadlights()
    {
        foreach (var headlight in headlights)
            if (headlight) headlight.enabled = false;

        if (headlightLenses.Length == 0) return;
        var properties = new MaterialPropertyBlock();
        var unlitGlass = new Color(.03f, .033f, .035f);
        foreach (var lens in headlightLenses)
        {
            if (!lens) continue;
            lens.GetPropertyBlock(properties);
            properties.SetColor("_BaseColor", unlitGlass);
            properties.SetColor("_Color", unlitGlass);
            lens.SetPropertyBlock(properties);
        }
    }

    void ScorchBody()
    {
        var block = new MaterialPropertyBlock();
        int baseColor = Shader.PropertyToID("_BaseColor");
        int color = Shader.PropertyToID("_Color");
        int emission = Shader.PropertyToID("_EmissionColor");
        foreach (var renderer in target.renderers)
        {
            if (renderer == null || renderer.sharedMaterial == null) continue;
            Material material = renderer.sharedMaterial;
            int property = material.HasProperty(baseColor) ? baseColor : color;
            if (!material.HasProperty(property)) continue;
            renderer.GetPropertyBlock(block);
            Color paint = material.GetColor(property);
            block.SetColor(property, new Color(paint.r * .42f, paint.g * .38f, paint.b * .34f, paint.a));
            if (material.HasProperty(emission)) block.SetColor(emission, Color.black);
            renderer.SetPropertyBlock(block);
        }
    }
}
