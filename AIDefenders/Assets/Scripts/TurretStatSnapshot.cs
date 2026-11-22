using UnityEngine;

// Match the tier/research calculations used by TurretController and WeaponSystem.
public sealed class TurretStatSnapshot
{
    public readonly float[] values = new float[6];
    public bool beam;
    public string role;
    public static TurretStatSnapshot Read(TurretLabEntry entry, int level)
    {
        if (entry == null || entry.isCctv || entry.prefab == null) return null;
        var turret = entry.prefab.GetComponentInChildren<TurretController>(true);
        if (turret == null) return null;
        var weapon = turret.weapon != null ? turret.weapon : turret.GetComponent<WeaponSystem>();
        if (weapon == null || weapon.ammo == null) return null;
        var ammo = weapon.ammo;
        var tiers = turret.upgradeTiers;
        var tier = tiers != null && tiers.Length > 0 ? tiers[Mathf.Clamp(level-1,0,tiers.Length-1)] : null;
        var result = new TurretStatSnapshot();
        result.beam = ammo.fireMode == WeaponFireMode.Beam;
        result.values[0] = ammo.damage * Mathf.Max(.01f,tier != null ? tier.damageScale : 1f);
        result.values[1] = 1f / Mathf.Max(.01f,(tier != null ? tier.fireDelay : weapon.fireDelay)/(1f+TurretResearch.FireRateBonus(turret)));
        result.values[2] = (tier != null && tier.range > 0 ? tier.range : turret.range) + TurretResearch.RangeBonus(turret);
        result.values[3] = (tier != null ? tier.armorPierce : turret.armorPierce) + TurretResearch.ArmorPierceBonus(turret);
        result.values[4] = ammo.HasExplosion ? ammo.ExplosionRadius(turret) : 0;
        result.values[5] = Mathf.Max(.01f,((tier != null ? tier.reloadTime : weapon.reloadTime)+TurretResearch.ReloadBonus(turret))*TurretResearch.ReloadTimeScale(turret));
        if(result.beam) result.values[5] /= 1f+TurretResearch.OverheatCoolBonus(turret);
        switch(TurretResearch.GroupOf(turret))
        {
            case TurretResearch.MachineGun: result.role="빠른 연사로 비장갑 다수 제압. 중장갑에는 관통력 부족에 주의.";break;
            case TurretResearch.PrecisionShot: result.role="먼 거리의 단일 표적 정밀 사격. [연구] 식별 신뢰도에 비례한 고정 피해.";break;
            case TurretResearch.Laser: result.role="탄체 없이 즉시 빔 명중. [연구] 화상·과열 냉각·열 피드백 강화.";break;
            case TurretResearch.Missile: result.role="높은 단발 피해와 폭발. [연구] 유도·상부 공격·대전차 강화.";break;
            case TurretResearch.Plasma: result.role="넓은 폭발로 밀집 표적 대응. [연구] 장갑 약화·끌어당김.";break;
            default: result.role="공격 방식과 영구 연구를 함께 비교하세요.";break;
        }
        if(ammo.HasExplosion) result.role += "\n폭발 추가 피해 최대 " + ammo.explosionDamage.ToString("0.#") + " · 중심에서 멀수록 감소";
        return result;
    }
}
