using UnityEngine;

[DisallowMultipleComponent]
public class TurretBuildZone : MonoBehaviour
{
    public const string RootName = "BuildZones";

    void Awake()
    {
        PrepareColliders(transform);
    }

    public static void PrepareColliders(Transform root)
    {
        if (root == null)
            return;

        Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (collider == null)
                continue;

            collider.isTrigger = true;
        }
    }

    public static bool IsBuildZonesRootName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;

        return name.Replace(" ", "").Equals(RootName, System.StringComparison.OrdinalIgnoreCase);
    }

    public static bool BelongsToBuildZone(Collider collider)
    {
        if (collider == null)
            return false;

        if (collider.GetComponentInParent<TurretBuildZone>() != null)
            return true;

        Transform node = collider.transform;
        while (node != null)
        {
            if (IsBuildZonesRootName(node.name))
                return true;
            node = node.parent;
        }

        return false;
    }
}
