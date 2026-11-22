#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class DatasetStagePresetBuilder
{
    [MenuItem("AI Defenders/Chapters/Build Dataset Stage Presets")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Stop Play first.");
        for(int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)
            if(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)throw new System.InvalidOperationException("Save open scenes before rebuilding presets.");
        var setup=EditorSceneManager.GetSceneManagerSetup();
        try
        {
            foreach(string chapter in new[]{"Desert","City"})
            {
                var basePrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/ChapterTrainingTerrains/"+chapter+".prefab");
                if(basePrefab==null)throw new System.InvalidOperationException("Build chapter training terrain first.");
                for(int stage=1;stage<=3;stage++)
                {
                    string id=chapter+"_"+stage;
                    var scene=EditorSceneManager.OpenScene("Assets/Scenes/GameStages/"+chapter+"/"+id+".unity");
                    Material sky=RenderSettings.skybox;
                    var source=Object.FindFirstObjectByType<StageAtmosphere>();
                    var root=(GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
                    root.SetActive(false);
                    root.name=(chapter=="Desert"?"사막 ":"도시 ")+stage+" · "+(stage==1?"낮":chapter=="Desert"?(stage==2?"황혼 · 모래폭풍":"달밤"):(stage==2?"비 오는 저녁":"안개 낀 새벽"));
                    var preset=root.AddComponent<DatasetCaptureEnvironment>();preset.stageId=id;preset.daySky=sky;
                    if(source!=null)
                    {
                        var atmosphere=root.AddComponent<StageAtmosphere>();atmosphere.weather=source.weather;atmosphere.skyMaterial=source.skyMaterial;
                    }
                    PrefabUtility.SaveAsPrefabAsset(root,"Assets/Resources/ChapterTrainingTerrains/"+id+".prefab");
                    Object.DestroyImmediate(root);
                }
            }
            AssetDatabase.SaveAssets();
        }
        finally {EditorSceneManager.RestoreSceneManagerSetup(setup);}
    }
}
#endif
