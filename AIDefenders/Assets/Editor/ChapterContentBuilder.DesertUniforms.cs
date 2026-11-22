#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

public static partial class ChapterContentBuilder
{
    static void DesertUniformPalette()
    {
        ColorMat("DesertUniformShade", "907450");
    }

    [MenuItem("AI Defenders/Chapters/Apply Desert Uniforms")]
    public static void ApplyDesertUniforms()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play before editing desert uniforms.");

        VehiclePalette();
        DesertUniformPalette();
        foreach (string name in new[] { "Sand", "White" })
        {
            mats[name] = AssetDatabase.LoadAssetAtPath<Material>(Art + "/Materials/" + name + ".mat");
            if (!mats[name]) throw new InvalidOperationException("Missing chapter material: " + name);
        }

        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { Targets + "/Desert" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var rig = prefab.GetComponent<ChapterUnitVisual>();
            if (rig == null || rig.vehicle) continue;

            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var target = root.GetComponent<Target_Base>();
                ApplyDesertUniform(root.GetComponent<ChapterUnitVisual>().body, target.IsFriendly);
                if (root.name == "Target_Friendly_DesertRescueWorker")
                    target.displayName = "사막 아군 군인";
                target.renderers = root.GetComponentsInChildren<Renderer>();
                target.RefreshAllegianceLayer();
                PrefabUtility.SaveAsPrefabAsset(root, path, out bool saved);
                if (!saved) throw new InvalidOperationException("Could not save uniform: " + path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        AssetDatabase.SaveAssets();
    }

    static void ApplyDesertUniform(Transform body, bool friendly)
    {
        string uniform = friendly ? "VehicleOlive" : "Sand";
        string equipment = friendly ? "VehicleOliveShade" : "DesertUniformShade";
        foreach (var renderer in body.GetComponentsInChildren<MeshRenderer>())
        {
            switch (renderer.name)
            {
                case "Jacket": case "Sleeve": case "Trouser": case "Helmet":
                    renderer.sharedMaterial = mats[uniform];
                    break;
                case "HelmetBrim": case "ChestArmor": case "AmmoPouch": case "RescueBackpack":
                case "ShoulderArmorL": case "ShoulderArmorR": case "ReflectiveShoulder":
                    renderer.sharedMaterial = mats[equipment];
                    break;
            }
        }

        if (!friendly || body.Find("SoldierGear") != null) return;
        Transform head = body.Find("Head");
        if (head.Find("Helmet") == null)
        {
            P(head, "Helmet", 0, .22f, 0, .52f, .21f, .46f, uniform);
            P(head, "HelmetBrim", 0, .16f, .08f, .58f, .055f, .57f, equipment);
        }

        Transform gear = Node(body, "SoldierGear", Vector3.zero);
        P(gear, "Vest", 0, 1.42f, .229f, .53f, .46f, .11f, equipment);
        P(gear, "UnitPatch", 0, 1.53f, .297f, .22f, .085f, .022f, "White");
        for (int side = -1; side <= 1; side += 2)
            P(gear, "UtilityPouch", side * .14f, 1.25f, .308f, .19f, .16f, .11f, uniform);
    }
}
#endif
