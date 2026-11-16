using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public static class GuidedSkillsUpgrade
{
    static void Configure(GameObject root)
    {
        foreach(var d in root.GetComponentsInChildren<CCTVDroneController>(true)){d.minPitch=-85;d.maxPitch=85;Record(d);}
        foreach(var d in root.GetComponentsInChildren<CCTVSkill_IdentificationDart>(true))
        {d.homingTurnRate=300;d.homingRange=180;d.homingConeAngle=14;Record(d);}
        foreach(var m in root.GetComponentsInChildren<CCTVSkill_TrapMissile>(true))
        {m.homingTurnRate=360;m.homingRange=180;m.homingConeAngle=70;Record(m);}
    }
    static void Record(UnityEngine.Object o)
    {EditorUtility.SetDirty(o);if(PrefabUtility.IsPartOfPrefabInstance(o))PrefabUtility.RecordPrefabInstancePropertyModifications(o);}
    [MenuItem("Tools/AI Defenders/Skills/Enable Guidance And All Chapter Sky View")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play first");
        Directory.CreateDirectory("Docs/GuidedSkillsUpgrade/Before");var report=new List<string>();
        const string path="Assets/DefenderBase/CCTVSystem 1.prefab";
        if(!File.Exists("Docs/GuidedSkillsUpgrade/Before/CCTVSystem.prefab"))File.Copy(path,"Docs/GuidedSkillsUpgrade/Before/CCTVSystem.prefab");
        var prefab=PrefabUtility.LoadPrefabContents(path);
        try{Configure(prefab);PrefabUtility.SaveAsPrefabAsset(prefab,path);}finally{PrefabUtility.UnloadPrefabContents(prefab);}
        var original=SceneManager.GetActiveScene();
        foreach(string scenePath in AssetDatabase.FindAssets("t:Scene",new[]{"Assets/Scenes/GameStages"}).Select(AssetDatabase.GUIDToAssetPath))
        {
            var scene=SceneManager.GetSceneByPath(scenePath);bool opened=!scene.isLoaded;
            if(!opened&&scene.isDirty)throw new InvalidOperationException("Unsaved scene: "+scenePath);
            string backup="Docs/GuidedSkillsUpgrade/Before/"+System.IO.Path.GetFileName(scenePath);
            if(!File.Exists(backup))File.Copy(scenePath,backup);
            if(opened)scene=EditorSceneManager.OpenScene(scenePath,OpenSceneMode.Additive);
            try
            {
                string signature=EnvironmentExpansionBuilder.GameplaySignature(scene);
                foreach(var root in scene.GetRootGameObjects())Configure(root);
                if(signature!=EnvironmentExpansionBuilder.GameplaySignature(scene))throw new InvalidOperationException("Gameplay routes changed");
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
                report.Add(scenePath+": pitch [-85,85]; missile turn 360deg/s; dart turn 300deg/s; unchanged routes and flight boundaries.");
            }
            finally{if(opened)EditorSceneManager.CloseScene(scene,true);}
        }
        SceneManager.SetActiveScene(original);AssetDatabase.SaveAssets();File.WriteAllLines("Docs/GuidedSkillsUpgrade/Configuration.txt",report.ToArray());
    }
}
