using UnityEngine;

[System.Serializable]
public class StoredTurret
{
    public GameObject prefab;
    public string displayName;
    public int sellPrice;
    public float buildTime = 3f;
    public int upgradeLevel = 1;

    public string Label
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(displayName))
                return TurretUiPresentation.Label(displayName, prefab);
            return TurretUiPresentation.Label(null, prefab);
        }
    }
}

[System.Serializable]
public class TurretBuildOption
{
    public string displayName;
    public GameObject prefab;
    public int cost = 50;
    public int sellPrice = 25;
    public float buildTime = 3f;

    public string Label
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(displayName))
                return TurretUiPresentation.Label(displayName, prefab);

            return TurretUiPresentation.Label(null, prefab);
        }
    }
}

// Resolve presentation independently of legacy prefab file names.
public static class TurretUiPresentation
{
    public static string Kind(GameObject prefab)
    {
        string n = prefab != null ? prefab.name.ToLowerInvariant() : "";
        if (n.Contains("missile")) return "Missile";
        if (n.Contains("laser")) return "Laser";
        if (n.Contains("plasma") || n.Contains("bolt")) return "Plasma";
        if (n.Contains("precision") || n.Contains("sniper")) return "Precision";
        if (n.Contains("machine") || n.Contains("mg")) return "MachineGun";
        return "";
    }

    public static string Label(string configured, GameObject prefab)
    {
        // Preserve deliberate user-facing names, replace internal prefab identifiers.
        if (!string.IsNullOrWhiteSpace(configured) &&
            !configured.StartsWith("Turret", System.StringComparison.OrdinalIgnoreCase))
            return configured;
        switch (Kind(prefab))
        {
            case "Missile": return "미사일 포탑";
            case "Laser": return "레이저 포탑";
            case "Plasma": return "플라즈마 포탑";
            case "Precision": return "정밀 사격 포탑";
            case "MachineGun": return "기관총 포탑";
            default: return prefab != null ? prefab.name : "포탑";
        }
    }
}
