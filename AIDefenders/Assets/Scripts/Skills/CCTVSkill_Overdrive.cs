using System.Collections.Generic;
using UnityEngine;

public class CCTVSkill_Overdrive : CCTVActiveSkill
{
    public float duration = 10f;
    public float attackSpeedMultiplier = 1.5f;
    public float projectileSpeedMultiplier = 1.3f;
    static CCTVSkill_Overdrive active;
    float endsAt;
    float nextScan;
    Material auraMaterial;
    readonly Dictionary<TurretController, LineRenderer> auras = new Dictionary<TurretController, LineRenderer>();

    public static bool IsActive => active != null && active.isActiveAndEnabled && Time.time < active.endsAt;
    public static float AttackSpeedScale => IsActive ? Mathf.Max(1f, active.attackSpeedMultiplier) : 1f;
    public static float ProjectileSpeedScale => IsActive ? Mathf.Max(1f, active.projectileSpeedMultiplier) : 1f;
    public float Remaining => active == this && IsActive ? Mathf.Max(0f, endsAt - Time.time) : 0f;
    protected override string DefaultIconResource => "UI/SkillIcons/Overdrive";

    void Reset()
    {
        displayName = "오버드라이브";
        cooldown = 60f;
    }

    protected override bool CanUse(CCTVSkillContext context)
    {
        return !IsActive;
    }

    protected override void OnUse(CCTVSkillContext context)
    {
        active = this;
        endsAt = Time.time + Mathf.Max(.1f, duration);
        nextScan = 0f;
        ScanTurrets();
    }

    protected override void Update()
    {
        base.Update();
        if (active != this || !IsActive)
        {
            if (auras.Count > 0) ClearAuras();
            return;
        }
        if (Time.time >= nextScan) ScanTurrets();
        float pulse = .5f + .5f * Mathf.Sin(Time.time * 7f);
        foreach (var item in auras)
        {
            if (item.Key == null || item.Value == null) continue;
            item.Value.enabled = item.Key.isActiveAndEnabled;
            Color color = Color.Lerp(new Color(.05f,.95f,1f,.65f),new Color(1f,.8f,.15f,1f),pulse);
            item.Value.startColor = item.Value.endColor = color;
            item.Value.widthMultiplier = Mathf.Lerp(.05f,.1f,pulse);
        }
    }

    void ScanTurrets()
    {
        nextScan = Time.time + .5f;
        if (auraMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) return;
            auraMaterial = new Material(shader);
        }
        foreach (var turret in FindObjectsByType<TurretController>(FindObjectsSortMode.None))
        {
            if (auras.ContainsKey(turret)) continue;
            var go = new GameObject("OverdriveAura");
            go.transform.SetParent(turret.transform,false);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = auraMaterial;
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = 48;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            for (int i=0;i<48;i++)
            {
                float angle = i * Mathf.PI * 2f / 48;
                line.SetPosition(i,new Vector3(Mathf.Cos(angle)*1.2f,.15f,Mathf.Sin(angle)*1.2f));
            }
            auras.Add(turret,line);
        }
    }

    void OnDisable()
    {
        if (active == this) active = null;
        ClearAuras();
    }

    void ClearAuras()
    {
        foreach(var item in auras)
            if(item.Value != null) Destroy(item.Value.gameObject);
        auras.Clear();
        if (auraMaterial != null) Destroy(auraMaterial);
        auraMaterial = null;
    }
}
