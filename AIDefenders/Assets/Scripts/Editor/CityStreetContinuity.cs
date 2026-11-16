using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

// Matches the original 100 x 80 metre city blocks, including their road centres.
public static class CityStreetContinuity
{
    const string Path="Assets/Scenes/GameStages/City/City_1.unity";
    const string Art="Assets/Art/StageDetails/";
    static readonly List<Vector3> verts=new List<Vector3>();
    static readonly List<int>[] indices=Enumerable.Range(0,5).Select(i=>new List<int>()).ToArray();
    static Vector3 Clearance(Bounds b)
    {
        float cx=(Mathf.Floor(b.center.x/100)+.5f)*100;
        float cz=(Mathf.Floor((b.center.z+100)/80)+.5f)*80-100;
        return new Vector3(Mathf.Clamp(b.center.x,cx-33+b.extents.x,cx+33-b.extents.x)-b.center.x,0,
            Mathf.Clamp(b.center.z,cz-23+b.extents.z,cz+23-b.extents.z)-b.center.z);
    }
    static void Quad(float x0,float x1,float z0,float z1,float y,int mat)
    {
        if(x1-x0<.001f||z1-z0<.001f)return;
        int n=verts.Count;verts.AddRange(new[]{new Vector3(x0,y,z0),new Vector3(x0,y,z1),new Vector3(x1,y,z1),new Vector3(x1,y,z0)});
        indices[mat].AddRange(new[]{n,n+1,n+2,n,n+2,n+3});
    }
    static void Outside(float x0,float x1,float z0,float z1,float y,int mat)
    {
        // Preserve the original street mesh and all shelter roads inside this rectangle.
        Quad(x0,Mathf.Min(x1,-205),z0,z1,y,mat);Quad(Mathf.Max(x0,205),x1,z0,z1,y,mat);
        float a=Mathf.Max(x0,-205),b=Mathf.Min(x1,205);
        Quad(a,b,z0,Mathf.Min(z1,-235),y,mat);Quad(a,b,Mathf.Max(z0,95),z1,y,mat);
    }
    static Mesh Save(Mesh m,string name)
    {
        var old=AssetDatabase.LoadAssetAtPath<Mesh>(Art+name+".asset");
        if(old){EditorUtility.CopySerialized(m,old);UnityEngine.Object.DestroyImmediate(m);EditorUtility.SetDirty(old);return old;}
        AssetDatabase.CreateAsset(m,Art+name+".asset");return m;
    }
    [MenuItem("Tools/AI Defenders/City/Repair And Extend Streets")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play first");
        var s=SceneManager.GetSceneByPath(Path);bool opened=!s.isLoaded;
        if(!opened&&s.isDirty)throw new InvalidOperationException("Save city edits first");
        Directory.CreateDirectory("Docs/CityStreetRepair/Before");
        if(!File.Exists("Docs/CityStreetRepair/Before/City_1.unity"))File.Copy(Path,"Docs/CityStreetRepair/Before/City_1.unity");
        if(opened)s=EditorSceneManager.OpenScene(Path,OpenSceneMode.Additive);
        var active=SceneManager.GetActiveScene();SceneManager.SetActiveScene(s);
        try
        {
            string sig=EnvironmentExpansionBuilder.GameplaySignature(s);var roots=s.GetRootGameObjects();int moved=0,outerMoved=0;
            var facade=roots.FirstOrDefault(g=>g.name=="City_Facade_Window_Upgrade");
            foreach(var root in roots.Where(g=>g.name=="City_Diverse_Skyline"||g.name=="City_Urban_Refinement"))
            foreach(Transform building in root.transform)
            {
                // The command post is fixed to the gameplay goal, not a city block.
                if(root.name=="City_Urban_Refinement" &&
                    !(building.name.StartsWith("Urban_Block_",StringComparison.Ordinal)||building.name.StartsWith("Skyline_",StringComparison.Ordinal)))continue;
                var rr=building.GetComponentsInChildren<MeshRenderer>().Where(r=>r.gameObject.activeInHierarchy).ToArray();
                if(rr.Length==0)continue;var bounds=rr[0].bounds;foreach(var r in rr.Skip(1))bounds.Encapsulate(r.bounds);
                if(bounds.size.y<4||bounds.min.y>1||bounds.size.x>64||bounds.size.z>44)continue;
                var delta=Clearance(bounds);if(delta.sqrMagnitude<.0001f)continue;
                building.position+=delta;EditorUtility.SetDirty(building);PrefabUtility.RecordPrefabInstancePropertyModifications(building);
                if(facade){var f=facade.transform.Find(building.name+"_Facade_Windows");if(f)f.position+=delta;}
                moved++;
            }
            var horizon=roots.First(g=>g.name=="City_Outer_Background");
            foreach(Transform t in horizon.transform)
            {
                if(t.name.StartsWith("Outer_EW_")||t.name.StartsWith("Outer_NS_")||t.name.StartsWith("Outer_Road_Closure"))t.gameObject.SetActive(false);
                if(!t.name.StartsWith("Background_Ring_"))continue;
                var filter=t.GetComponent<MeshFilter>();var mesh=UnityEngine.Object.Instantiate(filter.sharedMesh);var v=mesh.vertices;Vector3 lastDelta=Vector3.zero;
                for(int n=0;n+19<v.Length;n+=20)
                {
                    var b=new Bounds(t.TransformPoint(v[n]),Vector3.zero);for(int j=1;j<20;j++)b.Encapsulate(t.TransformPoint(v[n+j]));
                    if(b.min.y<1){lastDelta=Clearance(b);if(lastDelta.sqrMagnitude>.0001f)outerMoved++;}
                    var d=t.InverseTransformVector(lastDelta);for(int j=0;j<20;j++)v[n+j]+=d;
                }
                mesh.vertices=v;mesh.RecalculateBounds();filter.sharedMesh=Save(mesh,t.name+"_StreetClearance");
            }
            var old=roots.FirstOrDefault(g=>g.name=="City_Continuous_Outer_Streets");if(old)UnityEngine.Object.DestroyImmediate(old);
            verts.Clear();foreach(var list in indices)list.Clear();
            for(int ix=-8;ix<=8;ix++)
            {
                float x=ix*100;Outside(x-9,x+9,-775,655,.045f,0);
                for(int iz=-8;iz<=9;iz++)
                {
                    float z=-100+iz*80;
                    Outside(x-15,x-9,z+9,z+71,.3f,1);Outside(x+9,x+15,z+9,z+71,.3f,1);
                    Outside(x-9,x-8.75f,z+10,z+70,.055f,3);Outside(x+8.75f,x+9,z+10,z+70,.055f,3);
                    for(float q=z+12;q<z+70;q+=10)Outside(x-.13f,x+.13f,q,q+5,.06f,4);
                }
            }
            for(int iz=-8;iz<=9;iz++)
            {
                float z=-100+iz*80;Outside(-815,815,z-9,z+9,.046f,0);
                for(int ix=-8;ix<8;ix++)
                {
                    float x=ix*100;Outside(x+9,x+91,z-15,z-9,.3f,1);Outside(x+9,x+91,z+9,z+15,.3f,1);
                    Outside(x+10,x+90,z-9,z-8.75f,.06f,3);Outside(x+10,x+90,z+8.75f,z+9,.06f,3);
                    for(float q=x+12;q<x+90;q+=10)Outside(q,q+5,z-.13f,z+.13f,.065f,4);
                }
            }
            // Crossing stripes stop at the junction instead of continuing lane lines through it.
            for(int ix=-8;ix<=8;ix++)for(int iz=-8;iz<=9;iz++)
            {
                float x=ix*100,z=-100+iz*80;
                foreach(int side in new[]{-1,1})for(int stripe=0;stripe<8;stripe++)
                {
                    float q=-7.6f+stripe*2;
                    Outside(x+q,x+q+1.1f,z+side*12-2,z+side*12+2,.07f,3);
                    Outside(x+side*12-2,x+side*12+2,z+q,z+q+1.1f,.071f,3);
                }
            }
            var m=new Mesh{name="City_Continuous_Outer_Streets",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};m.SetVertices(verts);m.subMeshCount=5;
            for(int i=0;i<5;i++)m.SetTriangles(indices[i],i);m.uv=verts.Select(v=>new Vector2(v.x/4,v.z/4)).ToArray();m.RecalculateNormals();m.RecalculateBounds();
            var go=new GameObject(m.name);go.layer=2;go.AddComponent<MeshFilter>().sharedMesh=Save(m,m.name);
            var street=s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).First(t=>t.name=="City_Street_Grid");
            go.AddComponent<MeshRenderer>().sharedMaterials=street.GetComponent<MeshRenderer>().sharedMaterials;
            CityPavementClearance.Rebuild(s);
            if(sig!=EnvironmentExpansionBuilder.GameplaySignature(s))throw new InvalidOperationException("Gameplay changed");
            EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);AssetDatabase.SaveAssets();
            File.WriteAllText("Docs/CityStreetRepair/Changes.txt","Repositioned buildings: "+moved+"; background footprints: "+outerMoved+". Continuous marked outer street grid, 1630m wide. Gameplay signature preserved.");
        }
        finally{SceneManager.SetActiveScene(active);if(opened)EditorSceneManager.CloseScene(s,true);}
    }
}
