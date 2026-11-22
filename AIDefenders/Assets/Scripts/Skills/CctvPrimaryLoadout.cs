using UnityEngine;

public class CctvPrimaryDef
{
    public string id;
    public string displayName;
    public string description;
    public string iconResource;

    public Sprite Icon { get { return SkillIconSet.Load(iconResource) ?? Resources.Load<Sprite>(iconResource); } }

    public CctvPrimaryDef(string id, string displayName, string description, string iconResource)
    {
        this.id = id;
        this.displayName = displayName;
        this.description = description;
        this.iconResource = iconResource;
    }
}

public static class CctvPrimaryLoadout
{
    public const string Tesla = "tesla";
    public const string EnergyParticle = "energy_particle";
    public const string FocusFire = "focus_fire";
    public const string DefaultId = Tesla;

    public static readonly CctvPrimaryDef[] All =
    {
        new CctvPrimaryDef(
            Tesla,
            "테슬라 발사기",
            "좌클릭으로 사거리 안의 식별된 적 전원에게 전격을 가합니다. 짧게 감속시킵니다.",
            "UI/SkillIcons/TeslaLauncher"
        ),
        new CctvPrimaryDef(
            EnergyParticle,
            "에너지 입자탄",
            "사거리 안 가장 가까운 적에게 유도 입자탄을 1초마다 자동 발사합니다.",
            "UI/SkillIcons/EnergyParticle"
        ),
        new CctvPrimaryDef(
            FocusFire,
            "집중 지정",
            "좌클릭으로 식별된 적을 지정하면 포탑이 우선 공격합니다. 우클릭으로 해제합니다.",
            "UI/SkillIcons/FocusFire"
        )
    };

    public static CctvPrimaryDef Find(string id)
    {
        string resolved = ResolveId(id);
        for (int i = 0; i < All.Length; i++)
        {
            if (All[i].id == resolved)
                return All[i];
        }

        return All[0];
    }

    public static string ResolveId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return DefaultId;
        for (int i = 0; i < All.Length; i++)
        {
            if (All[i].id == id)
                return id;
        }

        return DefaultId;
    }
}
