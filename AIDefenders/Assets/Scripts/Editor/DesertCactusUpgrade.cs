using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class DesertCactusUpgrade
{
    const string ScenePath="Assets/Scenes/GameStages/Desert/Desert_1.unity";
    const string Folder="Assets/Art/EnvironmentExpansion/Generated/";
    const string RootName="Desert_Cactus_Upgrade";
    [MenuItem("Tools/AI Defenders/Environment Expansion/6 Upgrade Desert Cacti")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play mode first.");
        var scene=SceneManager.GetSceneByPath(ScenePath);bool opened=!scene.isLoaded;
        if(!opened&&scene.isDirty)throw new InvalidOperationException("Save desert scene edits first.");
        if(opened)scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Additive);
        try
        {
            Directory.CreateDirectory("Docs/DesertCactus/Before");
            if(!File.Exists("Docs/DesertCactus/Before/Desert_1.unity"))File.Copy(ScenePath,"Docs/DesertCactus/Before/Desert_1.unity");
            string before=EnvironmentExpansionBuilder.GameplaySignature(scene);
            var all=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
            var original=all.First(t=>t.name=="Cacti");
            var terrain=all.Select(t=>t.GetComponent<Terrain>()).First(t=>t);
            var paths=all.Select(t=>t.GetComponent<EnemyPath>()).Where(p=>p).Select(p=>p.GetWorldPoints()).ToArray();
            var old=scene.GetRootGameObjects().FirstOrDefault(r=>r.name==RootName);
            if(old)UnityEngine.Object.DestroyImmediate(old);
            var root=new GameObject(RootName);SceneManager.MoveGameObjectToScene(root,scene);
            int index=0;
            foreach(Transform source in original)
            {
                string model=index%5==0?"DesertYoungCactus":index%3==0?"DesertTallCactus":"DesertRibbedCactus";
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+model+".prefab");
                var mesh=prefab.GetComponent<MeshFilter>().sharedMesh;
                float height=source.GetComponent<Renderer>().bounds.size.y;
                float scale=height/mesh.bounds.size.y;
                if(model=="DesertYoungCactus")scale*=.65f;
                Add(root.transform,terrain,model,source.position.x,source.position.z,scale,source.eulerAngles.y);
                if(index%2==0)
                {
                    float x=source.position.x+3.5f,z=source.position.z+2.2f;
                    if(RouteDistance(paths,x,z)>10)Add(root.transform,terrain,"DesertYoungCactus",x,z,1.2f+(index%3)*.2f,index*37);
                }
                if(index%3==0)
                {
                    float x=source.position.x-3,z=source.position.z+2.8f;
                    if(RouteDistance(paths,x,z)>10)Add(root.transform,terrain,"DesertDryShrubs",x,z,1.4f,index*23);
                }
                index++;
            }
            original.gameObject.SetActive(false);
            if(PrefabUtility.IsPartOfPrefabInstance(original))PrefabUtility.RecordPrefabInstancePropertyModifications(original.gameObject);
            if(before!=EnvironmentExpansionBuilder.GameplaySignature(scene))throw new InvalidOperationException("Gameplay changed.");
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            Debug.Log("Desert cacti upgraded: "+index+" originals replaced; "+root.transform.childCount+" total decorative plants.");
        }
        finally{if(opened)EditorSceneManager.CloseScene(scene,true);}
    }
    static void Add(Transform root,Terrain terrain,string name,float x,float z,float scale,float yaw)
    {
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+name+".prefab");
        if(!prefab)throw new InvalidOperationException(name);
        float y=terrain.SampleHeight(new Vector3(x,0,z))+terrain.transform.position.y;
        var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,root);
        go.transform.position=new Vector3(x,y-prefab.GetComponent<MeshFilter>().sharedMesh.bounds.min.y*scale-.025f,z);
        go.transform.localScale=Vector3.one*scale;go.transform.rotation=Quaternion.Euler(0,yaw,0);
    }
    static float RouteDistance(Vector3[][] paths,float x,float z)
    {
        float distance=float.MaxValue;var point=new Vector2(x,z);
        foreach(var path in paths)for(int i=1;i<path.Length;i++)
        {
            var a=new Vector2(path[i-1].x,path[i-1].z);var b=new Vector2(path[i].x,path[i].z);var delta=b-a;
            float t=delta.sqrMagnitude>0?Mathf.Clamp01(Vector2.Dot(point-a,delta)/delta.sqrMagnitude):0;
            distance=Mathf.Min(distance,Vector2.Distance(point,a+delta*t));
        }
        return distance;
    }
}
