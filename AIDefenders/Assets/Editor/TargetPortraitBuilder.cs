#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class TargetPortraitBuilder
{
    [MenuItem("AI Defenders/UI/Build Target Portraits")]
    public static void Build() => Build(null);

    public static void Build(string targetName)
    {
        const string folder = "Assets/Resources/UI/TargetPortraits";
        Directory.CreateDirectory(folder);
        var targets = new List<Target_Base>();
        foreach (var entry in DatasetStageCatalog.Stages)
            DatasetStageCatalog.CollectTargets(entry, targets);
        if (!string.IsNullOrEmpty(targetName))
            targets.RemoveAll(target => target.name != targetName);
        foreach (var target in targets)
        {
            var preview = new PreviewRenderUtility();
            try
            {
                var instance = preview.InstantiatePrefabInScene(target.gameObject);
                var visual = instance.GetComponent<ChapterUnitVisual>();
                if (visual != null)
                    foreach (var light in visual.headlights)
                        if (light != null) light.enabled = false;
                var renderers = instance.GetComponentsInChildren<Renderer>();
                var bounds = renderers[0].bounds;
                foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
                preview.ambientColor = new Color(.48f, .48f, .48f);
                preview.lights[0].intensity = 1.7f;
                preview.lights[0].transform.rotation = Quaternion.Euler(40f, -35f, 0f);
                preview.lights[1].intensity = .8f;
                preview.camera.fieldOfView = 32f;
                preview.camera.nearClipPlane = .03f;
                preview.camera.farClipPlane = 500f;
                preview.camera.backgroundColor = new Color32(27, 44, 58, 255);
                preview.camera.clearFlags = CameraClearFlags.SolidColor;
                float distance = bounds.extents.magnitude / Mathf.Sin(16f * Mathf.Deg2Rad) * 1.08f;
                preview.camera.transform.position = bounds.center + new Vector3(.7f, .30f, 1f).normalized * distance;
                preview.camera.transform.LookAt(bounds.center);
                for (int frame = 0; frame < 3; frame++)
                {
                    preview.BeginStaticPreview(new Rect(0, 0, 192, 192));
                    preview.Render(true);
                    var texture = preview.EndStaticPreview();
                    if (frame == 2) File.WriteAllBytes(folder + "/" + target.name + ".png", texture.EncodeToPNG());
                    Object.DestroyImmediate(texture);
                }
            }
            finally { preview.Cleanup(); }
        }
        AssetDatabase.Refresh();
        foreach (var target in targets)
        {
            string path = folder + "/" + target.name + ".png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 256;
            importer.SaveAndReimport();
        }
        Debug.Log("Built " + targets.Count + " target portraits.");
    }
}
#endif
