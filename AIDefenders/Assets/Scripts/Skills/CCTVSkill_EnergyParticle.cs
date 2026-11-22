using UnityEngine;

public class CCTVSkill_EnergyParticle : CCTVActiveSkill
{
    public float range = 70f;
    public float damage = 160f;
    public float armorPierce = 2f;
    public float projectileSpeed = 34f;
    public float projectileLifetime = 4f;
    public float homingTurnRate = 280f;
    public float muzzleOffset = 1.6f;
    public float projectileScale = 0.22f;
    public SfxEvent fireSfx;

    CCTVController cctv;

    public override CCTVPrimaryUseMode PrimaryUseMode
    {
        get { return CCTVPrimaryUseMode.Passive; }
    }

    protected override string DefaultIconResource
    {
        get { return CctvPrimaryLoadout.Find(LoadoutId).iconResource; }
    }

    public override string LoadoutId
    {
        get { return CctvPrimaryLoadout.EnergyParticle; }
    }

    void Start()
    {
        cctv = GetComponent<CCTVController>();
        if (cctv == null)
            cctv = FindFirstObjectByType<CCTVController>();
    }

    public override void TickPrimary(CCTVSkillContext context)
    {
        TryUse(context);
    }

    protected override bool CanUse(CCTVSkillContext context)
    {
        return FindNearestEnemy(OriginOf(context)) != null;
    }

    protected override void OnUse(CCTVSkillContext context)
    {
        Vector3 origin = OriginOf(context);
        Target_Base target = FindNearestEnemy(origin);
        if (target == null)
            return;

        Vector3 aim = target.GetAimPoint(origin);
        Vector3 direction = aim - origin;
        if (direction.sqrMagnitude < 0.0001f)
            direction = transform.forward;
        direction.Normalize();
        origin += direction * muzzleOffset;

        GameObject spawned = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        spawned.name = "EnergyParticle";
        spawned.transform.position = origin;
        spawned.transform.rotation = Quaternion.LookRotation(direction);
        spawned.transform.localScale = Vector3.one * Mathf.Max(0.08f, projectileScale);
        spawned.GetComponent<Collider>().isTrigger = true;

        Renderer renderer = spawned.GetComponent<Renderer>();
        if (renderer != null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
                shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader != null)
                renderer.material = new Material(shader);
            renderer.material.color = new Color(1f, 0.78f, 0.22f, 1f);
        }

        EnergyParticleShot shot = spawned.AddComponent<EnergyParticleShot>();
        shot.Init(
            direction * projectileSpeed,
            projectileLifetime,
            damage,
            armorPierce,
            target,
            transform,
            homingTurnRate
        );
        GameSfx.PlayWorld(fireSfx, origin);
    }

    Target_Base FindNearestEnemy(Vector3 origin)
    {
        if (cctv == null || cctv.enemies == null)
            return null;

        float maxRange = Mathf.Max(0.1f, range);
        Target_Base best = null;
        float bestDist = maxRange;

        for (int i = 0; i < cctv.enemies.Count; i++)
        {
            YoloSighting sighting = cctv.enemies[i];
            if (sighting == null || sighting.target == null || sighting.target.isDead || sighting.target.isSample)
                continue;

            float dist = Vector3.Distance(origin, sighting.target.GetAimPoint(origin));
            if (dist > bestDist)
                continue;

            best = sighting.target;
            bestDist = dist;
        }

        return best;
    }

    Vector3 OriginOf(CCTVSkillContext context)
    {
        if (context != null && context.ViewCamera != null)
            return context.ViewCamera.transform.position;
        if (context != null && context.Origin != null)
            return context.Origin.position;
        return transform.position;
    }
}
