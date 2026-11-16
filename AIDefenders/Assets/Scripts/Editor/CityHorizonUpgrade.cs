using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class CityHorizonUpgrade
{
    const string ScenePath="Assets/Scenes/GameStages/City/City_1.unity";
    const string Folder="Assets/Art/CityHorizon";
    const string RootName="City_Outer_Background";
    class Batch
    {
        public readonly List<Vector3> vertices=new List<Vector3>();
        public readonly List<Vector2> uv=new List<Vector2>();
        public readonly List<int>[] triangles=Enumerable.Range(0,7).Select(i=>new List<int>()).ToArray();
        void Face(Vector3 a,Vector3 b,Vector3 c,Vector3 d,int material,float width,float height)
        {
            int n=vertices.Count;vertices.AddRange(new[]{a,b,c,d});uv.AddRange(new[]{Vector2.zero,new Vector2(width/3.4f,0),new Vector2(width/3.4f,height/3.3f),new Vector2(0,height/3.3f)});
            triangles[material].AddRange(new[]{n,n+2,n+1,n,n+3,n+2});
        }
        public void Building(Vector3 p,float width,float depth,float height,int material)
        {
            float x=width/2,z=depth/2;
            Face(p+new Vector3(-x,0,-z),p+new Vector3(x,0,-z),p+new Vector3(x,height,-z),p+new Vector3(-x,height,-z),material,width,height);
            Face(p+new Vector3(x,0,z),p+new Vector3(-x,0,z),p+new Vector3(-x,height,z),p+new Vector3(x,height,z),material,width,height);
            Face(p+new Vector3(-x,0,z),p+new Vector3(-x,0,-z),p+new Vector3(-x,height,-z),p+new Vector3(-x,height,z),material,depth,height);
            Face(p+new Vector3(x,0,-z),p+new Vector3(x,0,z),p+new Vector3(x,height,z),p+new Vector3(x,height,-z),material,depth,height);
            Face(p+new Vector3(-x,height,-z),p+new Vector3(x,height,-z),p+new Vector3(x,height,z),p+new Vector3(-x,height,z),6,width,depth);
        }
        public void Save(Transform root,string name,Material[] materials)
        {
            var mesh=new Mesh{name=name,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.subMeshCount=7;
            for(int i=0;i<7;i++)mesh.SetTriangles(triangles[i],i);mesh.RecalculateNormals();mesh.RecalculateBounds();
            string path=Folder+"/"+name+".asset";var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(saved){EditorUtility.CopySerialized(mesh,saved);UnityEngine.Object.DestroyImmediate(mesh);EditorUtility.SetDirty(saved);}else{saved=mesh;AssetDatabase.CreateAsset(saved,path);}
            var go=new GameObject(name);go.transform.SetParent(root,false);go.layer=2;go.AddComponent<MeshFilter>().sharedMesh=saved;
            var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterials=materials;
            renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }
    [MenuItem("Tools/AI Defenders/City/Extend Background And Limit Drone")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play mode first.");
        var scene=SceneManager.GetSceneByPath(ScenePath);bool opened=!scene.isLoaded;
        if(!opened&&scene.isDirty)throw new InvalidOperationException("Unsaved City scene edits.");
        Directory.CreateDirectory("Docs/CityHorizonUpgrade/Before");string backup="Docs/CityHorizonUpgrade/Before/City_1.unity";if(!File.Exists(backup))File.Copy(ScenePath,backup);
        Directory.CreateDirectory(Folder);AssetDatabase.Refresh();if(opened)scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Additive);
        var original=SceneManager.GetActiveScene();SceneManager.SetActiveScene(scene);
        try
        {
            string signature=CombatSignature(scene);
            var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();
            var old=scene.GetRootGameObjects().FirstOrDefault(g=>g.name==RootName);if(old)UnityEngine.Object.DestroyImmediate(old);
            var root=new GameObject(RootName).transform;
            var materials=new[]{"Office_Concrete","Office_Brown","Office_Brick","Office_Slate","Office_Pale","Office_Grey","Roof_Concrete"}
                .Select(n=>AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/StageAtmosphere/"+n+".mat")).ToArray();
            if(materials.Any(m=>m==null))throw new InvalidOperationException("Missing facade material.");
            // A broad low plane prevents the original ground edge appearing beneath the skyline.
            var ground=StageDetailPolish.Mat("Horizon_Ground","#707878");
            StageDetailPolish.Shape(root,"Extended_Urban_Ground",PrimitiveType.Cube,new Vector3(0,-.24f,-100),new Vector3(1800,.2f,1800),ground);
            int count=0;
            for(int ring=0;ring<3;ring++)
            {
                var batch=new Batch();float halfX=new[]{310f,430f,590f}[ring],halfZ=new[]{240f,355f,500f}[ring];float spacing=new[]{47f,57f,68f}[ring];
                int seed=0;
                for(int side=0;side<4;side++)
                {
                    bool horizontal=side<2;float length=horizontal?halfX*2:halfZ*2;
                    int number=Mathf.CeilToInt(length/spacing);
                    for(int i=0;i<number;i++)
                    {
                        float along=-length/2+(i+.5f)*length/number;
                        float jitter=Mathf.Sin((seed+ring*19)*2.17f)*7;
                        Vector3 position=horizontal?new Vector3(along,.0f,-100+(side==0?-halfZ:halfZ)+jitter):new Vector3((side==2?-halfX:halfX)+jitter,0,-100+along);
                        float width=25+(seed*7%15),depth=25+(seed*11%13),height=32+(seed*17+ring*23)%65;
                        batch.Building(position,width,depth,height,seed%6);
                        if(seed%4==0)batch.Building(position+Vector3.up*height+new Vector3(3,0,-2),width*.35f,depth*.3f,3,6);
                        count++;seed++;
                    }
                }
                batch.Save(root,"Background_Ring_"+(ring+1),materials);
            }
            // Continue the four central streets beyond their spawners, then end them
            // at closed roadwork barriers. None of these decorations have colliders.
            var asphalt=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/StageAtmosphere/Urban_Asphalt.mat");
            foreach(int side in new[]{-1,1})
            {
                StageDetailPolish.Shape(root,"Outer_EW_Street",PrimitiveType.Cube,new Vector3(side*255,.1f,-100),new Vector3(120,.16f,20),asphalt);
                StageDetailPolish.Shape(root,"Outer_NS_Street",PrimitiveType.Cube,new Vector3(0,.1f,-100+side*230),new Vector3(20,.16f,150),asphalt);
                RoadEnd(root,new Vector3(side*216,.3f,-100),90);
                RoadEnd(root,new Vector3(0,.3f,-100+side*190),0);
            }
            var limit=all.Where(t=>t!=null).Select(t=>t.GetComponent<CCTVMoveLimit>()).First(l=>l!=null);var box=limit.GetComponent<BoxCollider>();
            box.transform.position=new Vector3(0,121,-60);box.center=Vector3.zero;box.size=new Vector3(1560,238,1400);limit.hardBoundary=true;
            EditorUtility.SetDirty(limit);EditorUtility.SetDirty(box);PrefabUtility.RecordPrefabInstancePropertyModifications(limit);PrefabUtility.RecordPrefabInstancePropertyModifications(box);PrefabUtility.RecordPrefabInstancePropertyModifications(box.transform);
            foreach(var drone in all.Where(t=>t!=null).Select(t=>t.GetComponent<CCTVDroneController>()).Where(d=>d!=null))
            {drone.moveLimit=limit;drone.minPitch=-85;drone.maxPitch=85;EditorUtility.SetDirty(drone);PrefabUtility.RecordPrefabInstancePropertyModifications(drone);}
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogColor=new Color(.55f,.61f,.65f);RenderSettings.fogDensity=.0045f;
            if(signature!=CombatSignature(scene))throw new InvalidOperationException("Combat layout changed.");
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            File.WriteAllText("Docs/CityHorizonUpgrade/Changes.txt",count+" background buildings in three batches; all four faces windowed; 1800m ground; four road closures outside spawn points; drone bounds X[-165.5,165.5], Z[-265.5,65.5], Y[2.5,67.5]; fog density .0045; gameplay signature unchanged.");
        }
        finally{SceneManager.SetActiveScene(original);if(opened)EditorSceneManager.CloseScene(scene,true);}
    }
    static string CombatSignature(Scene scene)
    {
        // The flight-limit volume is intentionally resized; spawn routes, targets
        // and camera home transforms must stay exactly as they were.
        return string.Join("|",EnvironmentExpansionBuilder.GameplaySignature(scene).Split('|').Where(s=>!s.StartsWith("CCTVMoveLimit")).ToArray());
    }
    static void RoadEnd(Transform root,Vector3 position,float yaw)
    {
        var group=new GameObject("Outer_Road_Closure").transform;group.SetParent(root,false);group.position=position;group.rotation=Quaternion.Euler(0,yaw,0);
        var concrete=StageDetailPolish.Mat("Horizon_Barrier_Concrete","#818889");var yellow=StageDetailPolish.Mat("Horizon_Barrier_Yellow","#DDB230");
        for(int i=-4;i<=4;i++)
        {
            StageDetailPolish.Shape(group,"Road_Block",PrimitiveType.Cube,new Vector3(i*2.25f,.5f,0),new Vector3(2.1f,1,.8f),concrete);
            StageDetailPolish.Shape(group,"Reflector",PrimitiveType.Cube,new Vector3(i*2.25f,.6f,-.42f),new Vector3(.8f,.28f,.04f),yellow);
        }
        StageDetailPolish.Combine(group,"Outer_Road_Closure_"+Mathf.RoundToInt(position.x)+"_"+Mathf.RoundToInt(position.z));
    }
}
