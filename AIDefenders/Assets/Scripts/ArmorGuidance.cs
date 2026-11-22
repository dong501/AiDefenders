using UnityEngine;

public static class ArmorGuidance
{
    public static float BuildPierce(TurretController turret)
    {
        if (turret == null) return 0f;
        var tiers = turret.upgradeTiers;
        float basic = tiers != null && tiers.Length > 0 && tiers[0] != null ? tiers[0].armorPierce : turret.armorPierce;
        return basic + TurretResearch.ArmorPierceBonus(turret);
    }

    public static string CardLine(GameObject prefab)
    {
        var turret = prefab != null ? prefab.GetComponent<TurretController>() : null;
        if (turret == null) return "";
        float ap = BuildPierce(turret);
        return "관통 " + ap.ToString("0.##") + (ap < 1f ? " · 경장갑 이하" : " · 장갑 대응");
    }

    public static float WaveArmor(WaveDirector director)
    {
        if (director == null || director.stage == null || director.stage.waves == null) return 0f;
        int index = director.CurrentWaveIndex;
        if (index < 0 || index >= director.stage.waves.Length) return 0f;
        var wave = director.stage.waves[index]; float armor = 0f;
        if (wave == null || wave.groups == null) return 0f;
        foreach (var group in wave.groups)
        {
            if (group == null || group.targets == null) continue;
            foreach (var spawn in group.targets)
            {
                if (spawn == null || spawn.prefab == null || spawn.count <= 0) continue;
                var target = spawn.prefab.GetComponent<Target_Base>();
                if (target != null && target.IsEnemy) armor = Mathf.Max(armor, target.armorValue);
            }
        }
        return armor;
    }

    public static string Recommendation(float armor, TurretBuildOption[] options)
    {
        string best = "관통력이 더 높은 포탑 또는 관통력 연구 필요";
        int level = int.MaxValue; float penetration = -1f;
        if (options == null) return best;
        foreach (var option in options)
        {
            if (option == null || option.prefab == null) continue;
            var turret = option.prefab.GetComponent<TurretController>(); if (turret == null) continue;
            var tiers = turret.upgradeTiers;
            int count = tiers != null && tiers.Length > 0 ? tiers.Length : 1;
            for (int i = 0; i < count; i++)
            {
                float ap = tiers != null && tiers.Length > 0 && tiers[i] != null ? tiers[i].armorPierce + TurretResearch.ArmorPierceBonus(turret) : BuildPierce(turret);
                if (ap <= armor + .0001f || i + 1 > level || (i + 1 == level && ap <= penetration)) continue;
                level = i + 1; penetration = ap;
                best = option.Label + " " + level + "단계 (관통 " + ap.ToString("0.##") + ")";
            }
        }
        return best;
    }
}
