using System;
using UnityEngine;

public static class SkillGuidance
{
    public static bool Valid(Target_Base target)
    {return target && target.isActiveAndEnabled && target.IsActiveInPlay;}

    public static bool Visible(Target_Base target, Vector3 origin, Transform caster)
    {
        Vector3 delta=target.GetAimPoint(origin)-origin;
        var hits=Physics.RaycastAll(origin,delta.normalized,delta.magnitude,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Collide);
        Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
        foreach(var hit in hits)
        {
            var collider=hit.collider;
            if(caster&&(collider.transform==caster||collider.transform.IsChildOf(caster)))continue;
            if(collider.GetComponentInParent<Projectile_Base>()||collider.GetComponentInParent<CCTVDroneController>())continue;
            var body=collider.GetComponentInParent<Target_Base>();
            if(body==target)return true;
            if(!collider.isTrigger)return false;
        }
        return true;
    }

    public static float Score(Target_Base target,Vector3 origin,Vector3 forward,float range,float angle)
    {
        if(!Valid(target))return float.NegativeInfinity;
        Vector3 delta=target.GetAimPoint(origin)-origin;
        float distance=delta.magnitude;
        float cosine=Vector3.Dot(forward.normalized,delta.normalized);
        if(distance>range||cosine<Mathf.Cos(angle*Mathf.Deg2Rad))return float.NegativeInfinity;
        return cosine*1000-distance*.1f;
    }

    public static Target_Base AcquireDart(Vector3 origin,Vector3 direction,Transform caster,float range,float angle)
    {
        Target_Base best=null;float score=float.NegativeInfinity;
        foreach(var target in UnityEngine.Object.FindObjectsByType<Target_Base>(FindObjectsSortMode.None))
        {
            if(caster&&target.gameObject.scene!=caster.gameObject.scene)continue;
            float candidate=Score(target,origin,direction,range,angle);
            if(candidate<=score||!Visible(target,origin,caster))continue;
            best=target;score=candidate;
        }
        return best;
    }

    public static Vector3 Steer(Vector3 velocity,Target_Base target,Vector3 position,float rate,float deltaTime)
    {
        if(!Valid(target)||velocity.sqrMagnitude<.0001f)return velocity;
        Vector3 aim=target.GetAimPoint(position);
        var body=target.rigidbody?target.rigidbody:target.GetComponent<Rigidbody>();
        if(body)aim+=Vector3.ClampMagnitude(body.linearVelocity,20)*Mathf.Min(.35f,Vector3.Distance(aim,position)/velocity.magnitude*.65f);
        Vector3 delta=aim-position;if(delta.sqrMagnitude<.00001f)return velocity;
        return Vector3.RotateTowards(velocity.normalized,delta.normalized,Mathf.Max(0,rate)*Mathf.Deg2Rad*deltaTime,0)*velocity.magnitude;
    }
}
