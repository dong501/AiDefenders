using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public static class CityCommandPostFlightRepair
{
    [MenuItem("Tools/AI Defenders/City/Restore Command Post And Expand Flight")]
    public static void Apply()
    {
        const string path="Assets/Scenes/GameStages/City/City_1.unity";
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play first");
        var s=SceneManager.GetSceneByPath(path);bool opened=!s.isLoaded;
        if(!opened&&s.isDirty)throw new InvalidOperationException("Save city edits first");
        Directory.CreateDirectory("Docs/CityCommandPostFlightRepair/Before");
        if(!File.Exists("Docs/CityCommandPostFlightRepair/Before/City_1.unity"))File.Copy(path,"Docs/CityCommandPostFlightRepair/Before/City_1.unity");
        if(opened)s=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
        try
        {
            Func<string,string> combat=signature=>string.Join("|",signature.Split('|').Where(x=>!x.StartsWith("CCTVMoveLimit")).ToArray());
            string before=combat(EnvironmentExpansionBuilder.GameplaySignature(s));
            var all=s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();
            var gp=all.First(t=>t.name=="Shelter_GP_Enlarged");
            var goal=all.First(t=>t.name=="Target Goal");
            gp.position=new Vector3(goal.position.x,.34f,goal.position.z);Record(gp);
            var limit=all.Select(t=>t.GetComponent<CCTVMoveLimit>()).First(l=>l);
            var box=limit.GetComponent<BoxCollider>();box.transform.position=new Vector3(0,121,-60);
            box.center=Vector3.zero;box.size=new Vector3(1560,238,1400);box.isTrigger=true;limit.hardBoundary=true;
            Record(box.transform);Record(box);Record(limit);
            foreach(var drone in all.Select(t=>t.GetComponent<CCTVDroneController>()).Where(d=>d))
            {drone.minPitch=-85;drone.maxPitch=85;drone.moveLimit=limit;Record(drone);}
            if(before!=combat(EnvironmentExpansionBuilder.GameplaySignature(s)))throw new InvalidOperationException("Combat routes changed");
            EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);AssetDatabase.SaveAssets();
            File.WriteAllText("Docs/CityCommandPostFlightRepair/Changes.txt","GP restored to "+gp.position+". Pitch [-85,85]. Flight X[-779.5,779.5], Z[-759.5,639.5], Y[2.5,239.5]. Combat routes preserved. Ground clearance retained.");
        }
        finally{if(opened)EditorSceneManager.CloseScene(s,true);}
    }
    static void Record(UnityEngine.Object value)
    {EditorUtility.SetDirty(value);if(PrefabUtility.IsPartOfPrefabInstance(value))PrefabUtility.RecordPrefabInstancePropertyModifications(value);}
}
