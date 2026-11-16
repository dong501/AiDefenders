using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class CityPipeBarricadeUpgrade
{
    const string City="Assets/Scenes/GameStages/City/City_1.unity";
    const string Art="Assets/Art/StageDetails/";
    const string MeshPath=Art+"Shelter_Striped_Pipe_Barricade.asset";
    static readonly List<Vector3> vertices=new List<Vector3>();
    static readonly List<Vector3> normals=new List<Vector3>();
    static readonly List<int>[] indices={new List<int>(),new List<int>()};

    [MenuItem("Tools/AI Defenders/City/Replace Shelter Pipe Barricades")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play mode first.");
        var scene=SceneManager.GetSceneByPath(City);bool opened=!scene.isLoaded;
        if(!opened&&scene.isDirty)throw new InvalidOperationException("Unsaved city scene edits.");
        Directory.CreateDirectory("Docs/CityPipeBarricade/Before");string backup="Docs/CityPipeBarricade/Before/City_1.unity";
        if(!File.Exists(backup))File.Copy(City,backup);
        if(opened)scene=EditorSceneManager.OpenScene(City,OpenSceneMode.Additive);
        try
        {
            string before=EnvironmentExpansionBuilder.GameplaySignature(scene);int count=0;
            var checkpoints=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true))
                .Where(t=>new[]{"Checkpoint_South","Checkpoint_North","Checkpoint_West","Checkpoint_East"}.Contains(t.name)).ToArray();
            if(checkpoints.Length!=4)throw new InvalidOperationException("Expected four shelter entrances.");
            foreach(var checkpoint in checkpoints)
            {
                // The old X frames were baked into each checkpoint mesh. Strip only
                // their local bounds, retaining the guardhouses, boom and road blocks.
                var filter=checkpoint.GetComponent<MeshFilter>();var source=filter.sharedMesh;var mesh=UnityEngine.Object.Instantiate(source);var points=source.vertices;
                for(int sub=0;sub<source.subMeshCount;sub++)
                {
                    var triangles=source.GetTriangles(sub);var kept=new List<int>();
                    for(int i=0;i<triangles.Length;i+=3)
                    {
                        var p=(points[triangles[i]]+points[triangles[i+1]]+points[triangles[i+2]])/3;
                        if(Mathf.Abs(Mathf.Abs(p.x)-4.2f)<1.9f&&p.z> -3.95f&&p.z< -2.05f&&p.y<2.6f&&p.y>-.1f)continue;
                        kept.Add(triangles[i]);kept.Add(triangles[i+1]);kept.Add(triangles[i+2]);
                    }
                    mesh.SetTriangles(kept,sub);
                }
                mesh.RecalculateBounds();string path=Art+checkpoint.name+"_Without_X_Frames.asset";
                var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if(saved){EditorUtility.CopySerialized(mesh,saved);UnityEngine.Object.DestroyImmediate(mesh);EditorUtility.SetDirty(saved);}else{saved=mesh;AssetDatabase.CreateAsset(saved,path);}
                filter.sharedMesh=saved;
                foreach(Transform child in checkpoint.Cast<Transform>().Where(t=>t.name=="X_Frame_Barricade"||t.name=="Striped_Pipe_Barricade").ToArray())UnityEngine.Object.DestroyImmediate(child.gameObject);
                foreach(float x in new[]{-4.2f,4.2f}){Create(checkpoint,new Vector3(x,0,-3),0);count++;}
            }
            if(before!=EnvironmentExpansionBuilder.GameplaySignature(scene))throw new InvalidOperationException("Gameplay transforms changed.");
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            File.WriteAllText("Docs/CityPipeBarricade/Verification.txt",count+" barricades replaced; original placements retained; center lane clear; gameplay signature unchanged.");
        }
        finally{if(opened)EditorSceneManager.CloseScene(scene,true);}
    }
    internal static void Create(Transform parent,Vector3 position,float yaw)
    {
        var root=new GameObject("Striped_Pipe_Barricade");root.layer=2;root.transform.SetParent(parent,false);root.transform.localPosition=position;root.transform.localRotation=Quaternion.Euler(0,yaw,0);
        root.AddComponent<MeshFilter>().sharedMesh=EnsureMesh();root.AddComponent<MeshRenderer>().sharedMaterials=new[]{
            StageDetailPolish.Mat("Barricade_Pipe_Black","#161D29"),StageDetailPolish.Mat("Barricade_Pipe_Yellow","#F4BD16")};
        // Decorative only, matching the old checkpoint and leaving invasion routes open.
    }
    static Mesh EnsureMesh()
    {
        var saved=AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath);
        vertices.Clear();normals.Clear();foreach(var list in indices)list.Clear();
        Pipe(new Vector3(-1.78f,2.22f,0),new Vector3(1.78f,2.22f,0),.105f,10);
        foreach(float z in new[]{-.64f,.64f})Pipe(new Vector3(-1.65f,.55f,z),new Vector3(1.65f,.55f,z),.09f,10);
        foreach(float x in new[]{-1.5f,-.75f,0,.75f,1.5f})foreach(float side in new[]{-1,1})
            Pipe(new Vector3(x*1.1f,.035f,side*.88f),new Vector3(x,2.22f,0),.087f,8);
        foreach(float x in new[]{-1.63f,1.63f})Pipe(new Vector3(x,.48f,-.70f),new Vector3(x,.48f,.70f),.075f,4);
        var mesh=new Mesh{name="Shelter_Striped_Pipe_Barricade"};mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.subMeshCount=2;
        for(int i=0;i<2;i++)mesh.SetTriangles(indices[i],i);mesh.RecalculateBounds();
        if(saved){EditorUtility.CopySerialized(mesh,saved);UnityEngine.Object.DestroyImmediate(mesh);EditorUtility.SetDirty(saved);return saved;}
        AssetDatabase.CreateAsset(mesh,MeshPath);return mesh;
    }
    static void Pipe(Vector3 a,Vector3 b,float radius,int bands)
    {
        const int sides=12;Vector3 axis=(b-a).normalized;var u=Vector3.Cross(axis,Mathf.Abs(axis.y)>.9f?Vector3.right:Vector3.up).normalized;var v=Vector3.Cross(axis,u);
        for(int band=0;band<bands;band++)
        {
            int start=vertices.Count;var first=Vector3.Lerp(a,b,(float)band/bands);var last=Vector3.Lerp(a,b,(float)(band+1)/bands);
            for(int end=0;end<2;end++)for(int s=0;s<=sides;s++)
            {float angle=s*Mathf.PI*2/sides;var normal=u*Mathf.Cos(angle)+v*Mathf.Sin(angle);vertices.Add((end==0?first:last)+normal*radius);normals.Add(normal);}
            var tris=indices[band%2];
            for(int s=0;s<sides;s++){int n=start+s,m=n+sides+1;tris.AddRange(new[]{n,n+1,m,n+1,m+1,m});}
        }
        foreach(int end in new[]{0,1})
        {
            int center=vertices.Count;var p=end==0?a:b;var normal=end==0?-axis:axis;vertices.Add(p);normals.Add(normal);
            for(int s=0;s<=sides;s++){float angle=s*Mathf.PI*2/sides;vertices.Add(p+(u*Mathf.Cos(angle)+v*Mathf.Sin(angle))*radius);normals.Add(normal);}
            for(int s=0;s<sides;s++)indices[0].AddRange(end==0?new[]{center,center+s+2,center+s+1}:new[]{center,center+s+1,center+s+2});
        }
    }
}
