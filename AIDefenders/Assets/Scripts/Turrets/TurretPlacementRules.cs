using UnityEngine;

// Explicit roof surfaces and canyon high ground share the same footprint checks.
public class TurretPlacementRules : MonoBehaviour
{
    const float CanyonSupportHeightTolerance = .75f;
    const float CanyonInnerFootprintScale = .30f;
    const int CanyonMinimumOuterSupports = 3;

    public Terrain canyonTerrain;
    public float minimumCanyonHeight = 4.8f;
    public float maximumSlope = 32f;

    public static bool Allows(RaycastHit hit, Vector3 extents)
    {
        if (hit.collider == null) return false;
        var scene = hit.collider.gameObject.scene;
        TurretPlacementRules rules = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            rules = root.GetComponentInChildren<TurretPlacementRules>();
            if (rules != null) break;
        }
        if (rules != null && rules.canyonTerrain != null)
        {
            if (hit.collider != rules.canyonTerrain.GetComponent<TerrainCollider>()
                || hit.normal.y < Mathf.Cos(rules.maximumSlope * Mathf.Deg2Rad)
                || !rules.HasCanyonSupport(hit.point, extents))
                return false;
        }
        else if (hit.normal.y < Mathf.Cos(22f * Mathf.Deg2Rad))
            return false;
        var surface = hit.collider.GetComponent<TurretBuildSurface>();
        if (surface != null)
        {
            // Stay clear of roof and pavement edges; all corners must be supported.
            for (int x = -1; x <= 1; x += 2) for (int z = -1; z <= 1; z += 2)
            {
                var ray = new Ray(hit.point + new Vector3(x * extents.x, .3f, z * extents.z), Vector3.down);
                RaycastHit corner;
                if (!hit.collider.Raycast(ray, out corner, .7f) || corner.normal.y < .92f) return false;
            }
        }
        return true;
    }

    bool HasCanyonSupport(Vector3 center, Vector3 extents)
    {
        if (!SupportsCanyonPoint(center, center.y, out _))
            return false;

        // A canyon lip may cross two sides of the base when approached diagonally.
        int outerSupports = 0;
        for (int x = -1; x <= 1; x++)
        for (int z = -1; z <= 1; z++)
        {
            if (x == 0 && z == 0)
                continue;
            Vector3 point = center + new Vector3(x * extents.x, 0f, z * extents.z);
            if (SupportsCanyonPoint(point, center.y, out float height))
                outerSupports++;
            else if (height >= center.y)
                return false;
        }

        if (outerSupports < CanyonMinimumOuterSupports)
            return false;
        if (outerSupports == 8)
            return true;

        // Keep the center and the inner footprint on level high ground.
        Vector3 innerExtents = extents * CanyonInnerFootprintScale;
        for (int x = -1; x <= 1; x++)
        for (int z = -1; z <= 1; z++)
        {
            if (x == 0 && z == 0)
                continue;
            Vector3 point = center + new Vector3(x * innerExtents.x, 0f, z * innerExtents.z);
            if (!SupportsCanyonPoint(point, center.y, out _))
                return false;
        }

        return true;
    }

    bool SupportsCanyonPoint(Vector3 point, float platformHeight, out float height)
    {
        Vector3 terrainOrigin = canyonTerrain.transform.position;
        TerrainData data = canyonTerrain.terrainData;
        Vector3 local = point - terrainOrigin;
        float x = local.x / data.size.x;
        float z = local.z / data.size.z;
        height = platformHeight;
        if (x < 0f || x > 1f || z < 0f || z > 1f || data.IsHole(
            Mathf.Min((int)(x * data.holesResolution), data.holesResolution - 1),
            Mathf.Min((int)(z * data.holesResolution), data.holesResolution - 1)))
            return false;

        height = canyonTerrain.SampleHeight(point) + terrainOrigin.y;
        return height >= minimumCanyonHeight
            && Mathf.Abs(height - platformHeight) <= CanyonSupportHeightTolerance
            && data.GetSteepness(x, z) <= maximumSlope;
    }
}
