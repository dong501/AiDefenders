using UnityEngine;

// Explicit roof surfaces and canyon high ground share the same footprint checks.
public class TurretPlacementRules : MonoBehaviour
{
    public Terrain canyonTerrain;
    public float minimumCanyonHeight = 4.8f;
    public float maximumSlope = 22f;

    public static bool Allows(RaycastHit hit, Vector3 extents)
    {
        if (hit.collider == null || hit.normal.y < Mathf.Cos(22f * Mathf.Deg2Rad)) return false;
        var scene = hit.collider.gameObject.scene;
        TurretPlacementRules rules = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            rules = root.GetComponentInChildren<TurretPlacementRules>();
            if (rules != null) break;
        }
        if (rules != null && rules.canyonTerrain != null)
        {
            // The low channel and the base approach are not firing positions.
            if (hit.collider != rules.canyonTerrain.GetComponent<TerrainCollider>()) return false;
            for (int x = -1; x <= 1; x++) for (int z = -1; z <= 1; z++)
            {
                var point = hit.point + new Vector3(x * extents.x, 0, z * extents.z);
                var terrain = rules.canyonTerrain;
                float height = terrain.SampleHeight(point) + terrain.transform.position.y;
                var uv = point - terrain.transform.position;
                if (height < rules.minimumCanyonHeight || Mathf.Abs(height-hit.point.y) > .45f ||
                    terrain.terrainData.GetSteepness(uv.x / terrain.terrainData.size.x, uv.z / terrain.terrainData.size.z) > rules.maximumSlope) return false;
            }
        }
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
}
