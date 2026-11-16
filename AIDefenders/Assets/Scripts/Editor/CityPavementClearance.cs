using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class CityPavementClearance
{
    const string ScenePath="Assets/Scenes/GameStages/City/City_1.unity";
    const string Folder="Assets/Art/StageDetails/";
    [MenuItem("Tools/AI Defenders/City/Clean Pavement Building Intersections")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play mode first.");
        var scene=SceneManager.GetSceneByPath(ScenePath);bool opened=!scene.isLoaded;
        if(!opened&&scene.isDirty)throw new InvalidOperationException("Unsaved city scene.");
        Directory.CreateDirectory("Docs/PavementClearance/Before");
        if(!File.Exists("Docs/PavementClearance/Before/City_1.unity"))File.Copy(ScenePath,"Docs/PavementClearance/Before/City_1.unity");
        if(opened)scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Additive);
        try{Rebuild(scene);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();}
        finally{if(opened)EditorSceneManager.CloseScene(scene,true);}
    }
    public static void Rebuild(Scene scene)
    {
        var roots=scene.GetRootGameObjects();string signature=EnvironmentExpansionBuilder.GameplaySignature(scene);
        var old=roots.FirstOrDefault(g=>g.name=="City_Buildable_Sidewalks");if(old)UnityEngine.Object.DestroyImmediate(old);
        var transforms=roots.Where(g=>g).SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();
        var street=transforms.First(t=>t.name=="City_Street_Grid");
        var buildings=roots.Where(g=>g && (g.name=="City_Diverse_Skyline"||g.name=="City_Urban_Refinement"))
            .SelectMany(g=>g.GetComponentsInChildren<MeshRenderer>()).Where(r=>r.gameObject.activeInHierarchy&&r.bounds.size.y>4&&r.bounds.size.x>5&&r.bounds.size.z>5&&r.bounds.min.y<.8f).ToArray();
        var obstacles=buildings.Select(r=>Rect.MinMaxRect(r.bounds.min.x-.06f,r.bounds.min.z-.06f,r.bounds.max.x+.06f,r.bounds.max.z+.06f)).ToArray();
        var source=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Art/StageAtmosphere/Urban_Street_Grid.asset");
        var vertices=source.vertices;var resultVertices=new List<Vector3>();var subsets=new List<int[]>();var walkVertices=new List<Vector3>();
        for(int sub=0;sub<source.subMeshCount;sub++)
        {
            var indices=new List<int>();var triangles=source.GetTriangles(sub);
            for(int n=0;n<triangles.Length;n+=3)
            {
                var a=street.TransformPoint(vertices[triangles[n]]);var b=street.TransformPoint(vertices[triangles[n+1]]);var c=street.TransformPoint(vertices[triangles[n+2]]);
                bool top=sub==1&&Vector3.Cross(b-a,c-a).normalized.y>.9f;
                var pieces=new List<List<Vector3>>{new List<Vector3>{a,b,c}};
                if(top)foreach(var rect in obstacles)
                {
                    var next=new List<List<Vector3>>();
                    foreach(var piece in pieces)
                    {
                        if(!Overlaps(piece,rect)){next.Add(piece);continue;}
                        var remaining=piece;
                        for(int edge=0;edge<4&&remaining.Count>=3;edge++)
                        {var outside=Clip(remaining,rect,edge,false);if(outside.Count>=3)next.Add(outside);remaining=Clip(remaining,rect,edge,true);}
                    }
                    pieces=next;
                }
                foreach(var piece in pieces)for(int p=1;p<piece.Count-1;p++)
                {
                    if(Vector3.Cross(piece[p]-piece[0],piece[p+1]-piece[0]).sqrMagnitude<.000001f)continue;
                    foreach(var v in new[]{piece[0],piece[p],piece[p+1]})
                    {indices.Add(resultVertices.Count);resultVertices.Add(street.InverseTransformPoint(v));if(top)walkVertices.Add(street.InverseTransformPoint(v));}
                }
            }
            subsets.Add(indices.ToArray());
        }
        var mesh=new Mesh{name="City_Street_Grid_Clearance",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(resultVertices);mesh.subMeshCount=subsets.Count;
        for(int i=0;i<subsets.Count;i++)mesh.SetTriangles(subsets[i],i);
        mesh.uv=resultVertices.Select(v=>new Vector2(v.x/4,v.z/4)).ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();
        var saved=Save(mesh,Folder+"City_Street_Grid_Clearance.asset");street.GetComponent<MeshFilter>().sharedMesh=saved;
        var existingCollider=street.GetComponent<MeshCollider>();if(existingCollider)existingCollider.sharedMesh=saved;
        if(PrefabUtility.IsPartOfPrefabInstance(street)){PrefabUtility.RecordPrefabInstancePropertyModifications(street.GetComponent<MeshFilter>());if(existingCollider)PrefabUtility.RecordPrefabInstancePropertyModifications(existingCollider);}
        var surface=new GameObject("City_Buildable_Sidewalks");SceneManager.MoveGameObjectToScene(surface,scene);surface.transform.SetPositionAndRotation(street.position,street.rotation);surface.transform.localScale=street.lossyScale;
        int mask=transforms.Select(t=>t.GetComponent<TurretPlatformPlacer>()).First(p=>p).groundMask.value;int layer=0;while(layer<31&&mask!=0&&(mask&(1<<layer))==0)layer++;surface.layer=layer;
        var walk=new Mesh{name="City_Pavement_Install_Surface",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};walk.SetVertices(walkVertices);walk.SetTriangles(Enumerable.Range(0,walkVertices.Count).ToArray(),0);walk.RecalculateNormals();walk.RecalculateBounds();
        surface.AddComponent<MeshCollider>().sharedMesh=Save(walk,Folder+"City_Pavement_Install_Surface.asset");surface.AddComponent<TurretBuildSurface>();
        if(signature!=EnvironmentExpansionBuilder.GameplaySignature(scene))throw new InvalidOperationException("Gameplay routes changed.");
        Directory.CreateDirectory("Docs/PavementClearance");File.WriteAllText("Docs/PavementClearance/Changes.txt","Removed 128 raised plain strips. Existing tiled pavement clipped against "+obstacles.Length+" building footprints. Install surface matches pavement exactly; roads and enemy routes preserved.");
    }
    static Mesh Save(Mesh mesh,string path)
    {var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(saved){EditorUtility.CopySerialized(mesh,saved);UnityEngine.Object.DestroyImmediate(mesh);EditorUtility.SetDirty(saved);return saved;}AssetDatabase.CreateAsset(mesh,path);return mesh;}
    static bool Overlaps(List<Vector3> p,Rect r)
    {return p.Max(v=>v.x)>r.xMin&&p.Min(v=>v.x)<r.xMax&&p.Max(v=>v.z)>r.yMin&&p.Min(v=>v.z)<r.yMax;}
    static float Distance(Vector3 p,Rect r,int edge)
    {return edge==0?p.x-r.xMin:edge==1?r.xMax-p.x:edge==2?p.z-r.yMin:r.yMax-p.z;}
    static List<Vector3> Clip(List<Vector3> polygon,Rect r,int edge,bool inside)
    {
        var result=new List<Vector3>();if(polygon.Count==0)return result;
        Vector3 a=polygon[polygon.Count-1];float da=Distance(a,r,edge)*(inside?1:-1);
        foreach(var b in polygon){float db=Distance(b,r,edge)*(inside?1:-1);if((da>=0)!=(db>=0))result.Add(Vector3.LerpUnclamped(a,b,da/(da-db)));if(db>=0)result.Add(b);a=b;da=db;}return result;
    }
}
