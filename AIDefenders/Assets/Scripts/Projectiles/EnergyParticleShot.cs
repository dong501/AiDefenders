using UnityEngine;

public class EnergyParticleShot : Projectile_Base
{
    public float homingTurnRate = 260f;

    Target_Base homingTarget;
    TrailRenderer trail;

    protected override void Awake()
    {
        if (GetComponent<Rigidbody>() == null)
        {
            Rigidbody body = gameObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
        }

        base.Awake();
    }

    public void Init(
        Vector3 initialVelocity,
        float life,
        float damageValue,
        float pierce,
        Target_Base target,
        Transform ignore,
        float turnRate)
    {
        homingTarget = target;
        homingTurnRate = turnRate;
        Launch(initialVelocity, 0f, life, damageValue, ignore, pierce);
    }

    protected override void OnLaunched()
    {
        EnsureTrail();
        int bullet = LayerMask.NameToLayer("Bullet");
        if (bullet >= 0)
            gameObject.layer = bullet;
    }

    protected override void TickHoming(float deltaTime)
    {
        if (!IsTargetValid())
            return;

        Vector3 aim = homingTarget.GetAimPoint(transform.position);
        Vector3 toTarget = aim - transform.position;
        if (toTarget.sqrMagnitude < 0.0001f)
            return;

        float speed = velocity.magnitude;
        if (speed <= 0.01f)
            return;

        Vector3 newDir = Vector3.RotateTowards(
            velocity.normalized,
            toTarget.normalized,
            homingTurnRate * Mathf.Deg2Rad * deltaTime,
            0f
        );
        velocity = newDir * speed;
    }

    protected override bool ShouldIgnore(Collider other)
    {
        if (base.ShouldIgnore(other))
            return true;

        Target_Base target = other.GetComponentInParent<Target_Base>();
        if (target == null)
            return false;

        return !IsTargetValid() || target != homingTarget;
    }

    bool IsTargetValid()
    {
        return homingTarget != null && !homingTarget.isDead && homingTarget.isActiveAndEnabled;
    }

    void EnsureTrail()
    {
        if (trail != null)
            return;

        trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = 0.22f;
        trail.minVertexDistance = 0.08f;
        trail.widthMultiplier = 1f;
        trail.startWidth = 0.12f;
        trail.endWidth = 0.02f;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trail.receiveShadows = false;
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader != null)
            trail.material = new Material(shader);
        Color start = new Color(1f, 0.82f, 0.28f, 0.95f);
        Color end = new Color(1f, 0.55f, 0.1f, 0f);
        trail.startColor = start;
        trail.endColor = end;
    }
}
