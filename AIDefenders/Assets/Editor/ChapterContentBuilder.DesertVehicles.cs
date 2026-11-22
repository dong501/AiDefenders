#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static partial class ChapterContentBuilder
{
    const string VehicleArt = "Assets/Art/ChapterVehicles";
    const string ImportedModels = VehicleArt + "/Models";

    [Serializable]
    sealed class VehicleMeshData
    {
        public VehicleMeshPart[] parts;
    }

    [Serializable]
    sealed class VehicleMeshPart
    {
        public string name;
        public string group;
        public string color;
        public string material;
        public Vector3[] vertices;
        public int[] triangles;
    }

    static void VehiclePalette()
    {
        ColorMat("VehicleOlive", "4C5339");
        ColorMat("VehicleOliveLight", "62684B");
        ColorMat("VehicleOliveShade", "3D4431");
        ColorMat("HeadlampLens", "FFF0D9", unlit: true);
    }

    static void BuildDesertVehicleModel(ChapterUnitVisual visual, BoxCollider collider, string kind)
    {
        bool truck = kind == "truck";
        bool tank = kind == "rover";
        bool jeep = kind == "buggy";
        bool friendlyArmor = kind == "crawler";
        string assetName = truck ? "MilitaryTruck" : tank ? "Tank" : jeep ? "Jeep" : "ArmoredCar";
        string source = truck ? ImportedModels + "/MilitaryTruckMeshData.json"
            : tank ? "Assets/CreatedAssets/Tank/Assets/AIDefendersTank/Editor/TankMeshData.json"
            : jeep ? "Assets/CreatedAssets/Desert_Jeep/Assets/AIDefendersJeep/Editor/JeepMeshData.json"
            : "Assets/CreatedAssets/Armored_Car/Assets/AIDefendersArmoredCar/Editor/ArmoredCarMeshData.json";

        var data = AssetDatabase.LoadAssetAtPath<TextAsset>(source);
        if (data == null) throw new InvalidOperationException("Missing vehicle model: " + source);
        var parts = JsonUtility.FromJson<VehicleMeshData>(data.text).parts;
        string lensPrefix = truck ? "HeadlightLens" : tank ? "Headlamp" : jeep ? "Headlight" : "LampLens";
        string[] lenses = tank ? new[] { "HeadlampL", "HeadlampR" } : new[] { lensPrefix + "-1", lensPrefix + "1" };
        string[] panels = truck ? new[] { "CargoSide-1", "CargoSide1" }
            : tank ? new[] { "SkirtL2", "SkirtR2" }
            : jeep ? new[] { "DoorL0", "DoorR0" }
            : new[] { "SideDoor-1", "SideDoor1" };
        var batches = parts.GroupBy(part => lenses.Contains(part.name) || panels.Contains(part.name)
            ? part.group + "_" + part.name : part.group + "_" + part.color.TrimStart('#')).ToArray();
        var meshesByName = LoadVehicleMeshes(assetName, batches, lenses, panels);

        UnityEngine.Object.DestroyImmediate(visual.body.gameObject);
        visual.body = Node(visual.transform, "DesertVehicle", Vector3.zero);
        Transform model = Node(visual.body, assetName, Vector3.zero);
        model.localScale = Vector3.one * (truck ? .82f : tank ? 1.08f : jeep ? .88f : .92f);

        var groups = new Dictionary<string, Transform>();
        var wheels = new List<Transform>();
        if (tank)
        {
            groups["Hull"] = model;
            groups["TrackL"] = model;
            groups["TrackR"] = model;
            groups["Yaw"] = Node(model, "Turret", new Vector3(0f, 1.25f, -.22f));
            groups["Pitch"] = Node(groups["Yaw"], "GunMount", new Vector3(0f, .32f, .88f));
            groups["Recoil"] = groups["Pitch"];
        }
        else
        {
            groups["Body"] = model;
            if (!truck && !jeep)
            {
                groups["Yaw"] = Node(model, "Turret", new Vector3(0f, 2.08f, .03f));
                groups["Pitch"] = Node(groups["Yaw"], "GunMount", new Vector3(0f, .32f, .39f));
            }
            foreach (char axle in truck ? "FMR" : "FR")
            foreach (char side in "LR")
            {
                float x = truck ? 1.085f : jeep ? .98f : 1.10f;
                float z = truck ? axle == 'F' ? 2.25f : axle == 'M' ? -1.20f : -2.55f
                    : (jeep ? 1.20f : 1.35f) * (axle == 'F' ? 1f : -1f);
                string group = "Wheel" + axle + side;
                var wheel = Node(model, group, new Vector3(side == 'L' ? -x : x, .51f, z));
                groups.Add(group, wheel);
                wheels.Add(wheel);
            }
        }

        var lensRenderers = new List<Renderer>(2);
        var breakaway = new List<Transform>(2);
        var hullRenderers = new List<Renderer>();
        foreach (var batch in batches)
        {
            VehicleMeshPart part = batch.First();
            Transform piece = Node(groups[part.group], batch.Key, Vector3.zero);
            Mesh mesh = meshesByName[batch.Key];
            piece.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = piece.gameObject.AddComponent<MeshRenderer>();
            bool lens = lenses.Contains(part.name);
            bool panel = panels.Contains(part.name);
            if (lens || panel)
                piece.localPosition = PartCenter(part);
            if (lens)
                lensRenderers.Add(renderer);
            if (panel)
                breakaway.Add(piece);
            renderer.sharedMaterial = lens ? mats["HeadlampLens"] : ArmedVehicleMaterial(part, truck || friendlyArmor);
            if (!tank || part.group != "Recoil")
                hullRenderers.Add(renderer);
        }

        if (lensRenderers.Count != 2 || breakaway.Count != 2)
            throw new InvalidOperationException("Missing lamps or side panels on " + assetName);
        Bounds bounds = hullRenderers[0].bounds;
        foreach (var renderer in hullRenderers)
            bounds.Encapsulate(renderer.bounds);
        collider.center = visual.transform.InverseTransformPoint(bounds.center);
        collider.size = bounds.size;

        visual.headlightLenses = lensRenderers.ToArray();
        visual.breakawayPanels = breakaway.ToArray();
        visual.wheels = wheels.ToArray();
        visual.warningLamps = Array.Empty<Renderer>();
        SetVehicleWreckEffect(visual);
        visual.GetComponent<Target_Base>().displayName = truck ? "수송 트럭" : tank ? "사막 전차" : jeep ? "사막 지프"
            : friendlyArmor ? "구조 장갑차" : "사막 장갑차";
    }

    static Dictionary<string, Mesh> LoadVehicleMeshes(string modelName, IGrouping<string, VehicleMeshPart>[] batches,
        string[] lenses, string[] panels)
    {
        Folder(VehicleArt + "/Geometry");
        string path = VehicleArt + "/Geometry/" + modelName + ".asset";
        var saved = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Mesh>().ToDictionary(mesh => mesh.name);
        foreach (var batch in batches)
        {
            if (saved.ContainsKey(batch.Key))
                continue;
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            foreach (var part in batch)
            {
                int offset = vertices.Count;
                Vector3 pivot = lenses.Contains(part.name) || panels.Contains(part.name) ? PartCenter(part) : Vector3.zero;
                foreach (Vector3 vertex in part.vertices)
                    vertices.Add(vertex - pivot);
                foreach (int index in part.triangles)
                    triangles.Add(offset + index);
            }
            var mesh = new Mesh { name = batch.Key };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            if (saved.Count == 0) AssetDatabase.CreateAsset(mesh, path);
            else AssetDatabase.AddObjectToAsset(mesh, path);
            saved.Add(batch.Key, mesh);
        }
        return saved;
    }

    static Vector3 PartCenter(VehicleMeshPart part)
    {
        var bounds = new Bounds(part.vertices[0], Vector3.zero);
        foreach (var vertex in part.vertices)
            bounds.Encapsulate(vertex);
        return bounds.center;
    }

    static Material ArmedVehicleMaterial(VehicleMeshPart part, bool friendly)
    {
        if (friendly)
            switch (part.material)
            {
                case "Olive": case "Sand": return mats["VehicleOlive"];
                case "OliveLight": case "LightSand": return mats["VehicleOliveLight"];
                case "OliveDark": case "ShadeSand": return mats["VehicleOliveShade"];
            }

        string hex = part.color.TrimStart('#').ToUpperInvariant();
        return ImportedVehicleMaterial(hex);
    }

    static Material ImportedVehicleMaterial(string hex)
    {
        string name = "VehiclePaint_" + hex;
        if (mats.TryGetValue(name, out Material material))
            return material;
        Folder(VehicleArt + "/Materials");
        string path = VehicleArt + "/Materials/" + name + ".mat";
        material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out Color color);
            material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = color };
            material.SetFloat("_Smoothness", .22f);
            AssetDatabase.CreateAsset(material, path);
        }
        mats.Add(name, material);
        return material;
    }
}
#endif
