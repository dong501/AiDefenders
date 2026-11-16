using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class PlacementAndUiUpgrade
{
    [MenuItem("Tools/AI Defenders/Placement/City Rooftops Sidewalks And Canyon Rules")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode first.");
        foreach(string path in new[]{"Assets/Scenes/GameStages/City/City_1.unity","Assets/Scenes/GameStages/Desert/Desert_1.unity"})
        {
            var scene=SceneManager.GetSceneByPath(path);bool opened=!scene.isLoaded;
            if(!opened && scene.isDirty) throw new InvalidOperationException("Unsaved scene: "+path);
            Directory.CreateDirectory("Docs/PlacementUiUpgrade/Before");
            string backup="Docs/PlacementUiUpgrade/Before/"+Path.GetFileName(path);
            if(!File.Exists(backup)) File.Copy(path,backup);
            if(opened) scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
            var original=SceneManager.GetActiveScene();SceneManager.SetActiveScene(scene);
            try
            {
                string signature=EnvironmentExpansionBuilder.GameplaySignature(scene);
                var roots=scene.GetRootGameObjects();
                if(path.Contains("Desert"))
                {
                    var terrain=roots.SelectMany(g=>g.GetComponentsInChildren<Terrain>()).First();
                    var rules=terrain.GetComponent<TurretPlacementRules>();if(!rules)rules=terrain.gameObject.AddComponent<TurretPlacementRules>();
                    rules.canyonTerrain=terrain;rules.minimumCanyonHeight=4.8f;
                }
                else
                {
                    var skyline=roots.First(g=>g.name=="City_Diverse_Skyline");
                    int mask=roots.SelectMany(g=>g.GetComponentsInChildren<TurretPlatformPlacer>()).First().groundMask.value;
                    int layer=0;while(layer<31 && mask!=0 && (mask&(1<<layer))==0)layer++;
                    foreach(Transform building in skyline.transform)
                    {
                        // The distant skyline remains scenery, beyond the playable city.
                        if(Mathf.Abs(building.position.x)>170)continue;
                        var mesh=building.GetComponent<MeshFilter>();if(!mesh)continue;
                        var collider=building.GetComponent<MeshCollider>();if(!collider)collider=building.gameObject.AddComponent<MeshCollider>();
                        collider.sharedMesh=mesh.sharedMesh;building.gameObject.layer=layer;
                        if(!building.GetComponent<TurretBuildSurface>())building.gameObject.AddComponent<TurretBuildSurface>();
                        if(PrefabUtility.IsPartOfPrefabInstance(building))PrefabUtility.RecordPrefabInstancePropertyModifications(building.gameObject);
                    }
                    CityPavementClearance.Rebuild(scene);
                }
                if(signature!=EnvironmentExpansionBuilder.GameplaySignature(scene))throw new InvalidOperationException("Gameplay routes changed.");
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            finally{SceneManager.SetActiveScene(original);if(opened)EditorSceneManager.CloseScene(scene,true);}
        }
        AssetDatabase.SaveAssets();
    }
}
