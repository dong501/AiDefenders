using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

// Imports only the supplied static mesh data. No scripts from the ZIP are executed.
public static class EnvironmentExpansionBuilder
{
    const string Folder = "Assets/Art/EnvironmentExpansion";
    const string RootName = "Environment_Asset_Expansion";
    [Serializable] public class Part { public string name, material, color; public Vector3[] vertices; public int[] triangles; }
    [Serializable] public class Item { public string name, source; public Part[] parts; }
    [Serializable] public class Model { public Item[] assets; }
    [Serializable] public class SceneReport { public string scene; public int instances, renderers, colliders; public string gameplayBefore, gameplayAfter; }
    [Serializable] public class Report { public SceneReport[] scenes; }
    static readonly Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
    static readonly List<Vector2[]> routes = new List<Vector2[]>();
    static Terrain terrain;
    static Transform parent;
    static bool city;

    [MenuItem("Tools/AI Defenders/Environment Expansion/1 Import Selected Assets")]
    public static void Import()
    {
        EnsureFolder(Folder + "/Generated");
        var model = JsonUtility.FromJson<Model>(File.ReadAllText(Folder + "/Source/SelectedEnvironmentMeshData.json"));
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (!shader) throw new InvalidOperationException("URP Lit shader missing.");
        var materials = new Dictionary<string, Material>();
        foreach (var item in model.assets)
        {
            var vertices = new List<Vector3>();
            var groups = new Dictionary<string, List<int>>();
            foreach (var part in item.parts)
            {
                if (part.triangles.Length % 3 != 0) throw new InvalidDataException(part.name);
                string key = part.color.TrimStart('#');
                if (!groups.ContainsKey(key)) groups.Add(key, new List<int>());
                if (!materials.ContainsKey(key))
                {
                    string path = Folder + "/Generated/Color_" + key + ".mat";
                    var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (!mat) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, path); }
                    Color color;
                    if (!ColorUtility.TryParseHtmlString(part.color, out color)) throw new InvalidDataException(part.color);
                    mat.SetColor("_BaseColor", color); mat.color = color;
                    mat.SetFloat("_Smoothness", .15f); mat.SetFloat("_Metallic", 0);
                    EditorUtility.SetDirty(mat); materials.Add(key, mat);
                }
                foreach (int index in part.triangles)
                {
                    if (index < 0 || index >= part.vertices.Length) throw new InvalidDataException(part.name);
                    groups[key].Add(vertices.Count); vertices.Add(part.vertices[index]);
                }
            }
            string meshPath = Folder + "/Generated/" + item.name + ".asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if (!mesh) { mesh = new Mesh(); AssetDatabase.CreateAsset(mesh, meshPath); }
            mesh.Clear(); mesh.name = item.name;
            mesh.indexFormat = vertices.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.SetVertices(vertices); mesh.subMeshCount = groups.Count;
            int sub = 0; foreach (var group in groups) mesh.SetTriangles(group.Value, sub++);
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); EditorUtility.SetDirty(mesh);
            var go = new GameObject(item.name);
            try
            {
                go.layer = 2; // Decorative objects cannot intercept construction raycasts.
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = go.AddComponent<MeshRenderer>();
                renderer.sharedMaterials = groups.Keys.Select(k => materials[k]).ToArray();
                PrefabUtility.SaveAsPrefabAsset(go, Folder + "/Generated/" + item.name + ".prefab");
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Environment Expansion: imported " + model.assets.Length + " static decorative prefabs.");
    }

    [MenuItem("Tools/AI Defenders/Environment Expansion/2 Apply To Chapters")]
    public static void Apply()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode first.");
        var paths = new[] {
            "Assets/Scenes/GameStages/GrassField/Grassfield_1.unity",
            "Assets/Scenes/GameStages/GrassField/Grassfield_2.unity",
            "Assets/Scenes/GameStages/GrassField/Grassfield_3.unity",
            "Assets/Scenes/GameStages/Desert/Desert_1.unity",
            "Assets/Scenes/GameStages/City/City_1.unity" };
        // Check every open scene before making any saved changes.
        foreach (string path in paths)
        { var s = SceneManager.GetSceneByPath(path); if (s.isLoaded && s.isDirty) throw new InvalidOperationException("Save scene edits first: " + path); }
        var original = SceneManager.GetActiveScene();
        var reports = new List<SceneReport>();
        prefabs.Clear();
        foreach (string path in paths)
        {
            var scene = SceneManager.GetSceneByPath(path); bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(scene);
                string before = GameplaySignature(scene);
                foreach (var root in scene.GetRootGameObjects()) if (root.name == RootName) UnityEngine.Object.DestroyImmediate(root);
                terrain = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Terrain>(true)).First();
                city = scene.name.StartsWith("City"); CollectRoutes(scene);
                var rootNew = new GameObject(RootName); parent = rootNew.transform;
                if (city) BuildCity(); else if (scene.name.StartsWith("Desert")) BuildDesert(); else BuildGrassland();
                string after = GameplaySignature(scene);
                if (before != after) throw new InvalidOperationException("Gameplay layout changed: " + scene.name);
                reports.Add(new SceneReport { scene = scene.name, instances = parent.childCount,
                    renderers = rootNew.GetComponentsInChildren<Renderer>().Length,
                    colliders = rootNew.GetComponentsInChildren<Collider>().Length, gameplayBefore = before, gameplayAfter = after });
                EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            }
            finally { SceneManager.SetActiveScene(original); if (opened) EditorSceneManager.CloseScene(scene, true); }
        }
        Directory.CreateDirectory("Docs/AssetVisualUpgrade");
        File.WriteAllText("Docs/AssetVisualUpgrade/scene_validation.json", JsonUtility.ToJson(new Report { scenes = reports.ToArray() }, true));
        Debug.Log("Environment Expansion applied to all 5 stages. Gameplay transforms preserved; no decorative colliders.");
    }

    static void EnsureFolder(string path)
    { if (AssetDatabase.IsValidFolder(path)) return; EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/')); AssetDatabase.CreateFolder(Path.GetDirectoryName(path).Replace('\\', '/'), Path.GetFileName(path)); }
    static IEnumerable<Transform> Transforms(Scene scene) { return scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)); }
    public static string GameplaySignature(Scene scene)
    {
        return string.Join("|", Transforms(scene).Where(t => t.name.StartsWith("Enemy spawner") || t.name.StartsWith("Path_") || (t.parent && t.parent.name.StartsWith("Path_")) || t.name == "Target Goal" || t.name.StartsWith("CCTV"))
            .Select(t => t.name + ":" + t.position.ToString("F4") + ":" + t.rotation.ToString("F4") + ":" + t.localScale.ToString("F4")).ToArray());
    }
    static void CollectRoutes(Scene scene)
    {
        routes.Clear();
        foreach (var t in Transforms(scene).Where(t => t.name.StartsWith("Path_") && t.childCount > 0))
        {
            var points = new List<Vector2>();
            if (t.parent) points.Add(new Vector2(t.parent.position.x, t.parent.position.z));
            foreach (Transform c in t) points.Add(new Vector2(c.position.x, c.position.z));
            routes.Add(points.ToArray());
        }
    }
    static float RouteDistance(float x, float z)
    {
        float distance = float.MaxValue; var p = new Vector2(x, z);
        foreach (var route in routes) for (int i = 1; i < route.Length; i++)
        { var v = route[i] - route[i - 1]; float t = v.sqrMagnitude > .0001f ? Mathf.Clamp01(Vector2.Dot(p - route[i - 1], v) / v.sqrMagnitude) : 0; distance = Mathf.Min(distance, Vector2.Distance(p, route[i - 1] + t * v)); }
        return distance;
    }
    static GameObject Place(string name, float x, float z, float scale = 1, float yaw = 0, bool routeGuard = true)
    {
        GameObject prefab;
        if (!prefabs.TryGetValue(name, out prefab))
        { prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/Generated/" + name + ".prefab"); if (!prefab) throw new InvalidDataException(name); prefabs.Add(name, prefab); }
        var mesh = prefab.GetComponent<MeshFilter>().sharedMesh;
        var size = mesh.bounds.size * scale; float radius = Mathf.Max(size.x, size.z) * .5f;
        if (routeGuard && RouteDistance(x, z) < 9 + radius) return null;
        float y = city ? .34f : terrain.SampleHeight(new Vector3(x, 0, z)) + terrain.transform.position.y;
        if (!city && y < -.15f) return null;
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        go.transform.position = new Vector3(x, y - mesh.bounds.min.y * scale, z);
        go.transform.localScale = Vector3.one * scale; go.transform.rotation = Quaternion.Euler(0, yaw, 0);
        return go;
    }
    static void BuildGrassland()
    {
        Place("GrasslandFeedTrough", -12, 65, 1.5f, 0, false);
        Place("GrasslandHayBales", 13, 64, 1.4f, 15, false);
        Place("FarmWheelbarrow", 21, 73, 1.4f, -35, false);
        Place("FarmWateringCan", 23, 74, 1.4f, 35, false);
        Place("GrasslandAppleCrates", 25, 78, 1.4f, 0, false);
        Place("GrasslandOrchardWagon", 25, 92, 1.7f, 30);
        Place("FarmToolShed", 12, 100, 2.5f, 90);
        Place("Windmill", 12, 114, 3, 20);
        Place("GrasslandScarecrow", 29, 110, 1.5f, 45);
        for (int i = 0; i < 5; i++)
        {
            Place("GrasslandAppleTree", -36 + (i % 3) * 8, 100 + (i / 3) * 9, 2, i * 43);
            Place("FarmPumpkinPatch", 26, 99 + i * 3.5f, 1.8f, i * 20);
        }
        Place("FarmBeehive", -25, 95, 1.5f, 30);
        Place("FarmBeehive", -22, 95, 1.5f, 30);
        var centers = new[] { new Vector2(-54, 8), new Vector2(73, 10), new Vector2(-76, -55), new Vector2(13, -95), new Vector2(83, -89) };
        foreach (var c in centers)
        {
            Place("GrasslandBirchTree", c.x, c.y, 2.2f, c.x);
            Place("GrasslandBirchTree", c.x + 5, c.y - 3, 1.8f, 120);
            Place("ForestFallenLog", c.x - 4, c.y + 3, 1.5f, 35);
            Place("ForestMushroomCluster", c.x - 3, c.y + 5, 1.3f, 0);
            Place("ForestFern", c.x - 6, c.y + 4, 2, 90);
        }
    }
    static void BuildDesert()
    {
        // The original Defense_Supply_Camp already contains the outpost tent.
        Place("SupplyCrate", -18, 57, 2, 12, false);
        Place("SupplyCrate", -18, 60, 1.6f, -12, false);
        Place("DrumPallet", 17, 65, 1.7f, 0, false);
        Place("DesertClayJars", 16, 57, 1.8f, 0, false);
        Place("RuinGate", -59, -3, 3.2f, 35);
        Place("BrokenColumn", -70, 3, 3, 5);
        Place("FallenColumn", -53, 10, 2.2f, 55);
        Place("BrokenRuinWall", -75, -9, 2.7f, 10);
        Place("RuinObelisk", 85, 20, 3.2f, -20);
        Place("BrokenColumn", 92, 28, 2.5f, 15);
        Place("FallenColumn", 84, 31, 2.1f, -40);
        Place("DesertMarketStall", -60, 66, 2.6f, 110);
        Place("DesertCarpetRack", -66, 72, 2, 90);
        Place("DesertClayJars", -54, 70, 2.3f, -15);
        Place("DesertWaterCistern", -70, 60, 2.5f, 15);
        Place("DesertDatePalm", -71, 77, 3, 20);
        Place("DesertDatePalm", -51, 78, 2.5f, -50);
    }
    static void BuildCity()
    {
        var goal = parent.gameObject.scene.GetRootGameObjects().FirstOrDefault(r => r.name == "Target Goal");
        if (goal && goal.transform.position.z < -50)
        {
            float z = goal.transform.position.z;
            Place("ParkInformationBooth", 18, z - 10, 1.8f, 180);
            Place("ParkSlattedBench", -18, z + 12, 1.6f, 90);
            Place("ParkCoveredBin", -18, z + 16, 1.4f);
            Place("SupplyCrate", -18, z + 21, 1.8f, 15);
            Place("DrumPallet", 18, z + 21, 1.4f);
            Place("ParkPicnicTable", -11, z + 28, 1.6f, 90);
            Place("ParkPicnicTable", 11, z + 28, 1.6f, 90);
            Place("ParkShrubPlanter", -17, z - 12, 1.7f);
            Place("ParkShrubPlanter", 17, z - 12, 1.7f);
        }
        else
        {
        Place("ParkInformationBooth", 18, 51, 1.8f, 180);
        Place("ParkSlattedBench", -18, 55, 1.6f, 90);
        Place("ParkSlattedBench", 18, 58, 1.6f, -90);
        Place("ParkCoveredBin", -18, 59, 1.4f);
        Place("SupplyCrate", -18, 64, 1.8f, 15);
        Place("SupplyCrate", -18, 67, 1.4f, -15);
        Place("DrumPallet", 18, 66, 1.4f);
        Place("ParkPicnicTable", -11, 85, 1.6f, 90);
        Place("ParkPicnicTable", 11, 85, 1.6f, 90);
        Place("ParkShrubPlanter", -17, 48, 1.7f, 0);
        Place("ParkShrubPlanter", 17, 45, 1.7f, 0);
        }
        foreach (float x in new[] { -50f, 50f })
        {
            Place("ParkSlattedBench", x, 5, 1.7f, 180);
            Place("ParkCoveredBin", x + 4, 5, 1.4f);
            Place("ParkShrubPlanter", x, -2, 2);
            Place("ParkPicnicTable", x, 33, 1.8f);
        }
        // Damaged landmarks sit in gaps between existing blocks, clear of the roads.
        Place("RuinsAbandonedCafe", -50, -116, 3.2f, 180);
        Place("RuinsStandingFacade", 50, -116, 3, 180);
        Place("RuinsCollapsedWarehouse", 150, -116, 2.8f, 90);
    }

    public static void RefreshCityDecor(Scene scene)
    {
        var original = SceneManager.GetActiveScene();
        SceneManager.SetActiveScene(scene);
        try
        {
            foreach (var root in scene.GetRootGameObjects()) if (root.name == RootName) UnityEngine.Object.DestroyImmediate(root);
            terrain = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Terrain>(true)).First();
            city = true; CollectRoutes(scene); prefabs.Clear();
            parent = new GameObject(RootName).transform;
            BuildCity();
        }
        finally { SceneManager.SetActiveScene(original); }
    }

    public static string Capture(string path, string name, Vector3 position, Vector3 target)
    {
        var scene = SceneManager.GetSceneByPath(path);
        bool opened = !scene.isLoaded;
        if (opened) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        var original = SceneManager.GetActiveScene();
        var hidden = new List<GameObject>();
        GameObject cameraObject = null; RenderTexture rt = null; Texture2D texture = null;
        var previous = RenderTexture.active;
        try
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var other = SceneManager.GetSceneAt(i);
                if (other == scene) continue;
                foreach (var root in other.GetRootGameObjects().Where(r => r.activeSelf)) { root.SetActive(false); hidden.Add(root); }
            }
            SceneManager.SetActiveScene(scene);
            cameraObject = new GameObject("Expansion_Preview_Camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.transform.position = position; camera.transform.LookAt(target);
            camera.fieldOfView = 48; camera.nearClipPlane = .2f; camera.farClipPlane = 1600;
            camera.cullingMask = ~(1 << 5); camera.allowHDR = false;
            rt = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
            camera.targetTexture = rt; camera.Render();
            RenderTexture.active = rt; texture = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0); texture.Apply();
            string output = "Docs/AssetVisualUpgrade/" + name + ".png";
            File.WriteAllBytes(output, texture.EncodeToPNG()); return output;
        }
        finally
        {
            RenderTexture.active = previous;
            if (cameraObject) UnityEngine.Object.DestroyImmediate(cameraObject);
            if (texture) UnityEngine.Object.DestroyImmediate(texture);
            if (rt) UnityEngine.Object.DestroyImmediate(rt);
            SceneManager.SetActiveScene(original);
            foreach (var root in hidden) root.SetActive(true);
            if (opened) EditorSceneManager.CloseScene(scene, true);
        }
    }
}
