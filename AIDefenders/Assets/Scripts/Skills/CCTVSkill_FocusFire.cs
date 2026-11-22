using UnityEngine;

public class CCTVSkill_FocusFire : CCTVActiveSkill
{
    public static Target_Base Designated { get; private set; }

    public float aimCone = 14f;
    public float aimRange = 120f;
    public SfxEvent markSfx;
    public SfxEvent clearSfx;

    CCTVController cctv;

    public override CCTVPrimaryUseMode PrimaryUseMode
    {
        get { return CCTVPrimaryUseMode.LeftRight; }
    }

    protected override string DefaultIconResource
    {
        get { return CctvPrimaryLoadout.Find(LoadoutId).iconResource; }
    }

    public override string LoadoutId
    {
        get { return CctvPrimaryLoadout.FocusFire; }
    }

    public static bool IsDesignated(Target_Base target)
    {
        return target != null && target == Designated;
    }

    void Start()
    {
        cctv = GetComponent<CCTVController>();
        if (cctv == null)
            cctv = FindFirstObjectByType<CCTVController>();
    }

    public override bool TryPrimaryFire(CCTVSkillContext context, int button)
    {
        if (button == 1)
        {
            if (Designated == null)
                return false;
            ClearDesignate();
            GameSfx.PlayWorld(clearSfx, transform.position);
            return true;
        }

        if (!isActiveAndEnabled || !IsReady)
            return false;
        if (context == null || !CanUse(context))
            return false;

        Target_Base target = FindTargetNearCrosshair(context);
        if (target == null)
            return false;

        Designated = target;
        GameSfx.PlayWorld(markSfx, target.GetAimPoint(transform.position));
        StartCooldown();
        return true;
    }

    protected override void OnUse(CCTVSkillContext context)
    {
    }

    protected override void Update()
    {
        base.Update();
        if (Designated == null)
            return;
        if (Designated.isDead || !Designated.isActiveAndEnabled)
            ClearDesignate();
    }

    void OnDisable()
    {
        ClearDesignate();
    }

    void ClearDesignate()
    {
        Designated = null;
    }

    Target_Base FindTargetNearCrosshair(CCTVSkillContext context)
    {
        Vector3 origin = transform.position;
        Vector3 aimDir = transform.forward;
        if (context != null && context.ViewCamera != null)
        {
            origin = context.AimRay.origin;
            aimDir = context.AimRay.direction;
        }
        else if (context != null && context.AimRay.direction.sqrMagnitude > 0.0001f)
        {
            origin = context.AimRay.origin;
            aimDir = context.AimRay.direction;
        }

        if (aimDir.sqrMagnitude < 0.0001f)
            aimDir = transform.forward;
        aimDir.Normalize();

        Target_Base[] targets = Object.FindObjectsByType<Target_Base>(FindObjectsSortMode.None);
        Target_Base best = null;
        float cone = Mathf.Max(1f, aimCone);
        float maxRange = aimRange > 0.01f ? aimRange : 120f;
        float bestAngle = cone;
        float bestDist = maxRange;

        for (int i = 0; i < targets.Length; i++)
        {
            Target_Base target = targets[i];
            if (target == null || target.isDead || target.isSample)
                continue;
            if (!IsKnownEnemy(target))
                continue;

            Vector3 toTarget = target.GetAimPoint(origin) - origin;
            float dist = toTarget.magnitude;
            if (dist < 0.4f || dist > maxRange)
                continue;

            float angle = Vector3.Angle(aimDir, toTarget);
            if (angle > cone)
                continue;
            if (best != null && angle > bestAngle + 0.01f)
                continue;
            if (best != null && angle >= bestAngle - 0.01f && dist >= bestDist)
                continue;

            best = target;
            bestAngle = angle;
            bestDist = dist;
        }

        return best;
    }

    bool IsKnownEnemy(Target_Base target)
    {
        if (cctv == null || cctv.enemies == null)
            return false;

        for (int i = 0; i < cctv.enemies.Count; i++)
        {
            YoloSighting sighting = cctv.enemies[i];
            if (sighting != null && sighting.target == target)
                return true;
        }

        return false;
    }
}
