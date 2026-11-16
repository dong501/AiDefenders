using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CityGreeneryUpgrade
{
    const string RootName = "City_Greenery_Upgrade";
    const string ScenePath = "Assets/Scenes/GameStages/City/City_1.unity";
    const string Folder = "Assets/Art/EnvironmentExpansion/Generated/";

    [MenuItem("Tools/AI Defenders/Environment Expansion/4 Upgrade City Trees")]
    public static void Apply()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode first.");
        var scene = SceneManager.GetSceneByPath(ScenePath);
        bool opened = !scene.isLoaded;
        if (!opened && scene.isDirty) throw new InvalidOperationException("Save city scene edits first.");
        if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
        try
        {
            Directory.CreateDirectory("Docs/CityFourWay/BeforeTrees");
            if (!File.Exists("Docs/CityFourWay/BeforeTrees/City_1.unity")) File.Copy(ScenePath, "Docs/CityFourWay/BeforeTrees/City_1.unity");
            ApplyToScene(scene);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        }
        finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
    }

    public static void ApplyToScene(Scene scene)
    {
        string before = EnvironmentExpansionBuilder.GameplaySignature(scene);
        var all = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
        var originalTrees = all.First(t => t.name == "Street_Trees");
        var old = scene.GetRootGameObjects().FirstOrDefault(r => r.name == RootName);
        if (old) UnityEngine.Object.DestroyImmediate(old);
        var root = new GameObject(RootName);
        SceneManager.MoveGameObjectToScene(root, scene);
        int index = 0;
        foreach (Transform source in originalTrees)
        {
            string name = index % 5 == 0 ? "GrasslandBirchTree" : index % 3 == 0 ? "TreeOak" : "StreetTree";
            float scale = name == "GrasslandBirchTree" ? 1.5f : name == "TreeOak" ? 2f : 1.95f;
            scale *= .94f + (index % 4) * .04f;
            Add(root.transform, name, source.position.x, source.position.z, scale, index * 47);
            index++;
        }
        originalTrees.gameObject.SetActive(false);
        if (PrefabUtility.IsPartOfPrefabInstance(originalTrees)) PrefabUtility.RecordPrefabInstancePropertyModifications(originalTrees.gameObject);

        // Additional planted trees along pavements, outside the central compound and four lanes.
        foreach (float x in new[] { -124f, -24f, 24f, 124f })
            foreach (float z in new[] { -165f, -85f, -5f })
            {
                if (Mathf.Abs(x) < 32 && z > -130 && z < -55) continue;
                Add(root.transform, "ResidentialTreePlanter", x, z, 1.65f, x < 0 ? 90 : -90);
            }
        foreach (float x in new[] { -76f, -24f, 24f, 76f })
            foreach (float z in new[] { -207f, -47f, 33f })
                Add(root.transform, "BushWide", x + 4, z, 1.6f, 0);
        if (before != EnvironmentExpansionBuilder.GameplaySignature(scene)) throw new InvalidOperationException("Gameplay layout changed.");
        Debug.Log("City greenery: replaced " + index + " trees, added 10 planted trees and 12 shrub clusters. Gameplay unchanged.");
    }

    static void Add(Transform root, string name, float x, float z, float scale, float yaw)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + name + ".prefab");
        if (!prefab) throw new InvalidOperationException("Missing greenery prefab: " + name);
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root);
        var mesh = prefab.GetComponent<MeshFilter>().sharedMesh;
        go.transform.position = new Vector3(x, .34f - mesh.bounds.min.y * scale, z);
        go.transform.localScale = Vector3.one * scale;
        go.transform.rotation = Quaternion.Euler(0, yaw, 0);
    }
}
