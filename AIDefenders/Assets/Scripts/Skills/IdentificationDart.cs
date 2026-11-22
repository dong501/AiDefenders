using UnityEngine;

public class IdentificationDart : Projectile_Base
{
    CCTVController cctv;
    bool asEnemy;
    Target_Base homingTarget;
    float homingTurnRate;

    public void SetGuidance(Target_Base target, float turnRate)
    {homingTarget=target;homingTurnRate=turnRate;}

    protected override void TickHoming(float deltaTime)
    {velocity=SkillGuidance.Steer(velocity,homingTarget,transform.position,homingTurnRate,deltaTime);}

    protected override void TickMove(float deltaTime)
    {
        velocity+=Vector3.down*gravity*deltaTime;
        AdvanceWithSweep(velocity*deltaTime);
        if(velocity.sqrMagnitude>.0001f)transform.rotation=Quaternion.LookRotation(velocity);
    }

    public void Init(
        Vector3 initialVelocity,
        float gravityValue,
        float life,
        CCTVController controller,
        bool markAsEnemy,
        Transform ignore)
    {
        cctv = controller;
        asEnemy = markAsEnemy;
        homingTarget = null;
        Launch(initialVelocity, gravityValue, life, 0f, ignore);
    }

    protected override void OnLaunched()
    {
        ApplyTint();
    }

    protected override void OnHitTarget(Target_Base target, Vector3 point)
    {
        if (target != null && cctv != null)
            cctv.ForceIdentify(target, asEnemy);
    }

    void ApplyTint()
    {
        Color color = asEnemy
            ? new Color(0.95f, 0.2f, 0.15f, 1f)
            : new Color(0.2f, 0.55f, 1f, 1f);
        // Only the identification core changes colour; silver casing and blue fins remain intact.
        foreach (Renderer mesh in GetComponentsInChildren<Renderer>())
        {
            var materials = mesh.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] == null || (!materials[i].name.StartsWith("Dart_Core") && materials.Length != 1))
                    continue;
                if (mesh is TrailRenderer) continue;
                var properties = new MaterialPropertyBlock();
                mesh.GetPropertyBlock(properties, i);
                properties.SetColor("_BaseColor", color);
                properties.SetColor("_Color", color);
                properties.SetColor("_EmissionColor", color * 1.4f);
                mesh.SetPropertyBlock(properties, i);
            }
        }
        foreach (TrailRenderer trail in GetComponentsInChildren<TrailRenderer>())
        {
            trail.startColor = new Color(color.r, color.g, color.b, .65f);
            trail.endColor = new Color(color.r, color.g, color.b, 0f);
        }
    }
}
