using System.Collections.Generic;
using UnityEngine;

public class CCTVSkill_TeslaLauncher : CCTVActiveSkill
{
    public float range = 70f;
    public float damage = 80f;
    public float armorPierce = 1.6f;
    public float slowDuration = 0.5f;
    public float slowSpeedScale = 0.7f;
    public SfxEvent fireSfx;
    public float boltDuration = 0.14f;
    public Color boltGlow = new Color(0.45f, 0.85f, 1f, 0.92f);
    public Color boltCore = new Color(0.92f, 0.98f, 1f, 1f);

    CCTVController cctv;

    public override CCTVPrimaryUseMode PrimaryUseMode
    {
        get { return CCTVPrimaryUseMode.Click; }
    }

    protected override string DefaultIconResource
    {
        get { return CctvPrimaryLoadout.Find(LoadoutId).iconResource; }
    }

    public override string LoadoutId
    {
        get { return CctvPrimaryLoadout.Tesla; }
    }

    void Start()
    {
        cctv = GetComponent<CCTVController>();
        if (cctv == null)
            cctv = FindFirstObjectByType<CCTVController>();
    }

    public override bool TryPrimaryFire(CCTVSkillContext context, int button)
    {
        if (button != 0)
            return false;
        return TryUse(context);
    }

    protected override bool CanUse(CCTVSkillContext context)
    {
        return CollectTargets(OriginOf(context)).Count > 0;
    }

    protected override void OnUse(CCTVSkillContext context)
    {
        Vector3 origin = OriginOf(context);
        List<Target_Base> targets = CollectTargets(origin);
        for (int i = 0; i < targets.Count; i++)
        {
            Target_Base target = targets[i];
            if (target == null || target.isDead)
                continue;

            Vector3 hit = target.GetAimPoint(origin);
            target.TakeDamage(damage, armorPierce);
            target.ApplyStatus(new StatusEffect_TeslaSlow(slowDuration, slowSpeedScale));
            SpawnBolt(origin, hit);
        }

        GameSfx.PlayWorld(fireSfx, origin);
    }

    List<Target_Base> CollectTargets(Vector3 origin)
    {
        var list = new List<Target_Base>();
        if (cctv == null || cctv.enemies == null)
            return list;

        float maxRange = Mathf.Max(0.1f, range);
        for (int i = 0; i < cctv.enemies.Count; i++)
        {
            YoloSighting sighting = cctv.enemies[i];
            if (sighting == null || sighting.target == null || sighting.target.isDead || sighting.target.isSample)
                continue;

            float dist = Vector3.Distance(origin, sighting.target.GetAimPoint(origin));
            if (dist > maxRange)
                continue;
            if (!list.Contains(sighting.target))
                list.Add(sighting.target);
        }

        return list;
    }

    Vector3 OriginOf(CCTVSkillContext context)
    {
        if (context != null && context.ViewCamera != null)
            return context.ViewCamera.transform.position;
        if (context != null && context.Origin != null)
            return context.Origin.position;
        return transform.position;
    }

    void SpawnBolt(Vector3 from, Vector3 to)
    {
        GameObject root = new GameObject("TeslaBolt");
        root.transform.position = from;
        LineRenderer glow = CreateBoltLine(root.transform, "Glow", 6, 0.18f, 0.08f, boltGlow);
        LineRenderer core = CreateBoltLine(root.transform, "Core", 10, 0.07f, 0.03f, boltCore);
        Vector3[] points = BuildArc(from, to, 7);
        glow.positionCount = points.Length;
        core.positionCount = points.Length;
        glow.SetPositions(points);
        core.SetPositions(points);
        Object.Destroy(root, Mathf.Max(0.05f, boltDuration));
    }

    static Vector3[] BuildArc(Vector3 from, Vector3 to, int segments)
    {
        int count = Mathf.Max(3, segments);
        var points = new Vector3[count];
        Vector3 delta = to - from;
        Vector3 dir = delta.sqrMagnitude > 0.0001f ? delta.normalized : Vector3.forward;
        Vector3 side = Vector3.Cross(dir, Vector3.up);
        if (side.sqrMagnitude < 0.0001f)
            side = Vector3.Cross(dir, Vector3.right);
        side.Normalize();
        Vector3 up = Vector3.Cross(side, dir).normalized;
        float length = delta.magnitude;

        points[0] = from;
        points[count - 1] = to;
        for (int i = 1; i < count - 1; i++)
        {
            float t = i / (float)(count - 1);
            Vector3 point = Vector3.Lerp(from, to, t);
            float amp = length * 0.045f * (1f - Mathf.Abs(t * 2f - 1f));
            point += side * Random.Range(-amp, amp);
            point += up * Random.Range(-amp, amp);
            points[i] = point;
        }

        return points;
    }

    static LineRenderer CreateBoltLine(Transform parent, string name, int caps, float startWidth, float endWidth, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        LineRenderer line = go.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.numCapVertices = caps;
        line.alignment = LineAlignment.View;
        line.material = CreateBoltMaterial();
        line.startWidth = startWidth;
        line.endWidth = endWidth;
        line.startColor = color;
        line.endColor = color;
        return line;
    }

    static Material CreateBoltMaterial()
    {
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
            shader = Shader.Find("Hidden/InternalErrorShader");
        return new Material(shader);
    }
}
