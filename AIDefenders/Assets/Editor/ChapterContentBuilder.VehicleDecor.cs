#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static partial class ChapterContentBuilder
{
    static void ApplyAmbulanceModel(ChapterUnitVisual visual, BoxCollider collider)
    {
        const string modelName = "Ambulance";
        var source = AssetDatabase.LoadAssetAtPath<TextAsset>(ImportedModels + "/AmbulanceMeshData.json");
        if (source == null) throw new InvalidOperationException("Missing ambulance model data.");
        var parts = JsonUtility.FromJson<VehicleMeshData>(source.text).parts;
        string[] lenses = { "Headlight-1", "Headlight1" };
        string[] beacons = { "Beacon-1", "Beacon1" };
        var batches = parts.GroupBy(part => part.group + "_" +
            (lenses.Contains(part.name) || beacons.Contains(part.name) ? part.name : part.color.TrimStart('#'))).ToArray();
        var modelMeshes = LoadVehicleMeshes(modelName, batches, lenses, Array.Empty<string>());

        Transform oldBody = visual.body;
        visual.body = Node(visual.transform, "Chassis", Vector3.zero);
        Transform model = Node(visual.body, modelName, Vector3.zero);
        model.localScale = Vector3.one * ((collider.size.z - .3f) / 5.3675f);
        var groups = new Dictionary<string, Transform> { ["Body"] = model };
        var wheels = new List<Transform>(4);
        foreach (char axle in "FR")
        foreach (char side in "LR")
        {
            string name = "Wheel" + axle + side;
            var wheel = Node(model, name, new Vector3(side == 'L' ? -.96f : .96f, .39f, axle == 'F' ? 1.52f : -1.48f));
            groups.Add(name, wheel);
            wheels.Add(wheel);
        }
        var headlightLenses = new List<Renderer>(2);
        var warningLamps = new List<Renderer>(2);
        foreach (var batch in batches)
        {
            VehicleMeshPart part = batch.First();
            bool isLens = lenses.Contains(part.name);
            Transform piece = Node(groups[part.group], batch.Key, isLens ? PartCenter(part) : Vector3.zero);
            piece.gameObject.AddComponent<MeshFilter>().sharedMesh = modelMeshes[batch.Key];
            var renderer = piece.gameObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = isLens ? mats["HeadlampLens"] : ImportedVehicleMaterial(part.color.TrimStart('#').ToUpperInvariant());
            if (isLens) headlightLenses.Add(renderer);
            if (beacons.Contains(part.name)) warningLamps.Add(renderer);
        }
        if (headlightLenses.Count != 2 || warningLamps.Count != 2)
            throw new InvalidOperationException("Ambulance must include two headlights and two beacons.");
        visual.headlightLenses = headlightLenses.ToArray();
        visual.warningLamps = warningLamps.ToArray();
        visual.wheels = wheels.ToArray();
        visual.breakawayPanels = Array.Empty<Transform>();
        SetVehicleWreckEffect(visual);
        UnityEngine.Object.DestroyImmediate(oldBody.gameObject);
    }

    static void ApplyCivilianBusModel(ChapterUnitVisual visual, BoxCollider collider)
    {
        GameObject source = PrepareVehicleProp("CivilianBus");
        GameObject model = UnityEngine.Object.Instantiate(source);
        model.name = "CivilianBus";
        Transform oldBody = visual.body;
        visual.body = Node(visual.transform, "Chassis", Vector3.zero);
        model.transform.SetParent(visual.body, false);
        var mesh = model.GetComponentInChildren<MeshFilter>();
        float scale = (collider.size.z - .3f) / mesh.sharedMesh.bounds.size.z;
        model.transform.localScale = Vector3.one * scale;

        visual.headlightLenses = new Renderer[2];
        for (int i = 0; i < visual.headlightLenses.Length; i++)
        {
            float x = i == 0 ? -.82f : .78f;
            visual.headlightLenses[i] = P(visual.body, "Headlight", x * scale, 1.125f * scale,
                3.959f * scale, .25f * scale, .13f * scale, .035f * scale, "HeadlampLens").GetComponent<Renderer>();
        }
        visual.wheels = Array.Empty<Transform>();
        visual.warningLamps = Array.Empty<Renderer>();
        visual.breakawayPanels = Array.Empty<Transform>();
        SetVehicleWreckEffect(visual);
        UnityEngine.Object.DestroyImmediate(oldBody.gameObject);
    }

    [MenuItem("AI Defenders/Chapters/Apply Stage Vehicle Decorations")]
    public static void ApplyStageVehicleDecorations()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play before editing stage decorations.");

        VehiclePalette();
        GameObject bus = PrepareVehicleProp("CivilianBus");
        GameObject scooter = PrepareVehicleProp("DeliveryScooter");
        for (int stage = 1; stage <= 3; stage++)
        {
            EditVehicleStage("City", stage, scene => AddCityVehicleProps(scene, bus, scooter));
            EditVehicleStage("Desert", stage, RepaintDesertBaseVehicles);
        }
        AssetDatabase.SaveAssets();
    }

    static GameObject PrepareVehicleProp(string modelName)
    {
        Folder(VehicleArt + "/Props");
        string path = VehicleArt + "/Props/" + modelName + ".prefab";
        var saved = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (saved != null) return saved;

        var model = AssetDatabase.LoadAssetAtPath<GameObject>(ImportedModels + "/" + modelName + ".obj");
        if (model == null) throw new InvalidOperationException("Missing city prop: " + modelName);
        var prop = new GameObject(modelName);
        try
        {
            foreach (var source in model.GetComponentsInChildren<MeshRenderer>())
            {
                Transform part = source.transform == model.transform ? prop.transform : Node(prop.transform, source.name, Vector3.zero);
                part.localPosition = model.transform.InverseTransformPoint(source.transform.position);
                part.localRotation = Quaternion.Inverse(model.transform.rotation) * source.transform.rotation;
                part.gameObject.AddComponent<MeshFilter>().sharedMesh = source.GetComponent<MeshFilter>().sharedMesh;
                Material[] materials = source.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    // Unity's OBJ importer converts MTL diffuse values to sRGB. Match the supplied palette.
                    materials[i] = ImportedVehicleMaterial(ColorUtility.ToHtmlStringRGB(materials[i].color.linear));
                }
                part.gameObject.AddComponent<MeshRenderer>().sharedMaterials = materials;
            }
            return PrefabUtility.SaveAsPrefabAsset(prop, path);
        }
        finally { UnityEngine.Object.DestroyImmediate(prop); }
    }

    static void EditVehicleStage(string chapter, int stage, Func<Scene, bool> edit)
    {
        string path = $"Assets/Scenes/GameStages/{chapter}/{chapter}_{stage}.unity";
        Scene scene = SceneManager.GetSceneByPath(path);
        bool openedHere = !scene.isLoaded;
        if (openedHere) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        else if (scene.isDirty) throw new InvalidOperationException("Save the open stage before upgrading decorations: " + scene.name);
        try
        {
            if (edit(scene) && !EditorSceneManager.SaveScene(scene))
                throw new InvalidOperationException("Could not save stage decorations: " + path);
        }
        finally
        {
            if (openedHere) EditorSceneManager.CloseScene(scene, true);
        }
    }

    static bool AddCityVehicleProps(Scene scene, GameObject bus, GameObject scooter)
    {
        const string groupName = "Parked_City_Vehicles";
        var group = Array.Find(scene.GetRootGameObjects(), root => root.name == groupName);
        if (group == null)
        {
            group = new GameObject(groupName);
            SceneManager.MoveGameObjectToScene(group, scene);
        }

        bool added = PlaceCityProp(group.transform, bus, "Bus_West_Stop", new Vector3(-17.2f, .30f, -138f), 0f);
        added |= PlaceCityProp(group.transform, bus, "Bus_East_Stop", new Vector3(17.2f, .30f, -58f), 180f);
        added |= PlaceCityProp(group.transform, scooter, "Scooter_West_Stop", new Vector3(-19.5f, .30f, -126.5f), 6f);
        added |= PlaceCityProp(group.transform, scooter, "Scooter_East_Stop", new Vector3(19.5f, .30f, -46.5f), 162f);
        return added;
    }

    static bool PlaceCityProp(Transform group, GameObject source, string objectName, Vector3 point, float yaw)
    {
        Transform prop = group.Find(objectName);
        Quaternion rotation = Quaternion.Euler(0f, yaw, 0f);
        if (prop != null && prop.position == point && prop.rotation == rotation) return false;
        if (prop == null)
        {
            prop = ((GameObject)PrefabUtility.InstantiatePrefab(source, group)).transform;
            prop.name = objectName;
        }
        prop.SetPositionAndRotation(point, rotation);
        PrefabUtility.RecordPrefabInstancePropertyModifications(prop);
        return true;
    }

    static bool RepaintDesertBaseVehicles(Scene scene)
    {
        bool changed = false;
        foreach (GameObject root in scene.GetRootGameObjects())
        foreach (Transform vehicle in root.GetComponentsInChildren<Transform>(true))
        {
            if (vehicle.name != "Base_DesertTank" && vehicle.name != "Defense_Jeep") continue;
            foreach (var renderer in vehicle.GetComponentsInChildren<MeshRenderer>(true))
            {
                Material original = renderer.sharedMaterial;
                if (original == null) continue;
                string color;
                switch (original.name)
                {
                    case "Material_b4a77b": case "Material_c5b98d": color = "VehicleOlive"; break;
                    case "Material_d0c39a": case "Material_ded3ab": color = "VehicleOliveLight"; break;
                    case "Material_8c805c": case "Material_9c906b": color = "VehicleOliveShade"; break;
                    default: continue;
                }
                renderer.sharedMaterial = mats[color];
                PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
                changed = true;
            }
        }
        return changed;
    }
}
#endif
