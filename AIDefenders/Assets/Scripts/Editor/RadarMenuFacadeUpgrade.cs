using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class RadarMenuFacadeUpgrade
{
    const string Output="Docs/RadarMenuFacadeUpgrade/";
    const string Art="Assets/Art/StageDetails/";
    const string City="Assets/Scenes/GameStages/City/City_1.unity";
    static readonly string[] Models={"MachineGun","Precision","Missile","Laser","Plasma"};
    static readonly string[] Stages={"Assets/Scenes/GameStages/GrassField/Grassfield_1.unity","Assets/Scenes/GameStages/GrassField/Grassfield_2.unity","Assets/Scenes/GameStages/GrassField/Grassfield_3.unity","Assets/Scenes/GameStages/Desert/Desert_1.unity",City};
    [Serializable] class MenuPart : EnvironmentExpansionBuilder.Part { public string group; }
    [Serializable] class MenuModel { public MenuPart[] parts; }

    [MenuItem("Tools/AI Defenders/UI/Refresh Radar Tier One Menu And Building Windows")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play mode first.");
        foreach(var path in Stages){var s=SceneManager.GetSceneByPath(path);if(s.isLoaded&&s.isDirty)throw new InvalidOperationException("Unsaved edits: "+path);}
        Directory.CreateDirectory(Output+"Before");
        foreach(var path in Stages){var dest=Output+"Before/"+Path.GetFileName(path);if(!File.Exists(dest))File.Copy(path,dest);}
        foreach(var model in Models)RenderTierOne(model);
        foreach(var path in Stages)Edit(path,s=>
        {
            foreach(var radar in s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<CCTVRadar>(true)))
            {radar.size=CCTVRadar.DefaultSize;Record(radar);}
            if(path==City)Facades(s);
        });
        AssetDatabase.SaveAssets();
    }
    static void Record(UnityEngine.Object obj){EditorUtility.SetDirty(obj);if(PrefabUtility.IsPartOfPrefabInstance(obj))PrefabUtility.RecordPrefabInstancePropertyModifications(obj);}
    static void Edit(string path,Action<Scene> action)
    {
        var s=SceneManager.GetSceneByPath(path);bool opened=!s.isLoaded;if(opened)s=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
        var before=SceneManager.GetActiveScene();SceneManager.SetActiveScene(s);
        try{action(s);EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);}
        finally{SceneManager.SetActiveScene(before);if(opened)EditorSceneManager.CloseScene(s,true);}
    }
    static void RenderTierOne(string model)
    {
        // These five source meshes come from the uploaded upgrade pack, including
        // Plasma, which has no matching finished gameplay prefab yet.
        var source=JsonUtility.FromJson<MenuModel>(File.ReadAllText(Output+"Sources/"+model+".json"));
        var mesh=new Mesh();mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
        var vertices=new List<Vector3>();var groups=new Dictionary<string,List<int>>();
        foreach(var part in source.parts)
        {
            if(!groups.ContainsKey(part.color))groups[part.color]=new List<int>();int offset=vertices.Count;
            // The pack stores head/barrel vertices relative to their pitch pivots.
            var position=part.group=="Base"||part.group=="Yaw"?Vector3.zero:new Vector3(0,model=="Precision"?1.40f:model=="Plasma"?1.53f:1.55f,0);
            if(part.group=="ChargeOrb")position.z+=.94f;
            vertices.AddRange(part.vertices.Select(v=>v+position));groups[part.color].AddRange(part.triangles.Select(i=>i+offset));
        }
        mesh.SetVertices(vertices);mesh.subMeshCount=groups.Count;int sub=0;foreach(var group in groups)mesh.SetTriangles(group.Value,sub++);mesh.RecalculateNormals();mesh.RecalculateBounds();
        var preview=new PreviewRenderUtility();Texture2D output=null,white=null;
        try
        {
            var instance=new GameObject(model+"_Level1");instance.AddComponent<MeshFilter>().sharedMesh=mesh;
            instance.AddComponent<MeshRenderer>().sharedMaterials=groups.Keys.Select(c=>StageDetailPolish.Mat("Menu_Tier1_"+c.TrimStart('#'),c)).ToArray();
            preview.AddSingleGO(instance);
            instance.transform.position=Vector3.zero;instance.transform.rotation=Quaternion.identity;
            var renderers=instance.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers.Skip(1))bounds.Encapsulate(r.bounds);
            var camera=preview.camera;camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.allowHDR=false;
            camera.transform.position=bounds.center+new Vector3(5,3.5f,7).normalized*bounds.size.magnitude*3;camera.transform.LookAt(bounds.center);
            float halfWidth=0,halfHeight=0;
            foreach(int x in new[]{-1,1})foreach(int y in new[]{-1,1})foreach(int z in new[]{-1,1})
            {var point=camera.transform.InverseTransformPoint(bounds.center+Vector3.Scale(bounds.extents,new Vector3(x,y,z)));halfWidth=Mathf.Max(halfWidth,Mathf.Abs(point.x));halfHeight=Mathf.Max(halfHeight,Mathf.Abs(point.y));}
            camera.orthographicSize=Mathf.Max(halfHeight,halfWidth/(4f/3))*1.13f;camera.nearClipPlane=.01f;camera.farClipPlane=2000;
            preview.ambientColor=new Color(.65f,.65f,.65f,1);
            preview.lights[0].intensity=1.3f;preview.lights[0].transform.rotation=Quaternion.Euler(35,210,0);
            preview.lights[1].intensity=.85f;preview.lights[1].transform.rotation=Quaternion.Euler(325,35,0);
            // Static previews flatten alpha in this render pipeline. Two native renders
            // recover coverage, keeping the menu background visible around the model.
            camera.backgroundColor=Color.black;
            preview.BeginStaticPreview(new Rect(0,0,768,576));preview.Render(true);output=preview.EndStaticPreview();
            camera.backgroundColor=Color.white;
            preview.BeginStaticPreview(new Rect(0,0,768,576));preview.Render(true);white=preview.EndStaticPreview();
            var darkPixels=output.GetPixels();var lightPixels=white.GetPixels();
            for(int i=0;i<darkPixels.Length;i++)
            {
                var b=darkPixels[i];var w=lightPixels[i];float alpha=Mathf.Clamp01(1-(w.r-b.r+w.g-b.g+w.b-b.b)/3);
                darkPixels[i]=alpha>.001f?new Color(b.r/alpha,b.g/alpha,b.b/alpha,alpha):Color.clear;
            }
            UnityEngine.Object.DestroyImmediate(output);output=new Texture2D(768,576,TextureFormat.RGBA32,false);output.SetPixels(darkPixels);output.Apply();
            string path="Assets/Resources/UI/Previews/Menu_Tier1_"+model+".png";
            File.WriteAllBytes(path,output.EncodeToPNG());AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=1024;importer.SaveAndReimport();
        }
        finally{if(output)UnityEngine.Object.DestroyImmediate(output);if(white)UnityEngine.Object.DestroyImmediate(white);preview.Cleanup();UnityEngine.Object.DestroyImmediate(mesh);}
    }
    class Face
    {
        public int axis,sign;public float coordinate,area,glass;
        public float minU=float.MaxValue,maxU=float.MinValue,minY=float.MaxValue,maxY=float.MinValue;
        public List<Vector2[]> triangles=new List<Vector2[]>();
    }
    static bool Glass(Material material)
    {var c=material.color;return c.b>c.r*1.12f&&c.b>c.g*.95f;}
    static List<Face> Faces(Transform transform,Mesh mesh,Material[] materials)
    {
        var vertices=mesh.vertices.Select(transform.TransformPoint).ToArray();var faces=new Dictionary<string,Face>();
        for(int sub=0;sub<mesh.subMeshCount;sub++)
        {
            var indices=mesh.GetTriangles(sub);bool glass=sub<materials.Length&&Glass(materials[sub]);
            for(int i=0;i<indices.Length;i+=3)
            {
                var a=vertices[indices[i]];var b=vertices[indices[i+1]];var c=vertices[indices[i+2]];var cross=Vector3.Cross(b-a,c-a);float area=cross.magnitude*.5f;if(area<.001f)continue;
                var normal=cross.normalized;int axis=Mathf.Abs(normal.x)>.985f?0:Mathf.Abs(normal.z)>.985f?2:-1;if(axis<0)continue;
                int sign=normal[axis]>0?1:-1;float coordinate=(a[axis]+b[axis]+c[axis])/3;
                string key=axis+":"+sign+":"+Mathf.RoundToInt(coordinate*20);
                Face f;if(!faces.TryGetValue(key,out f)){f=new Face{axis=axis,sign=sign,coordinate=coordinate};faces.Add(key,f);}
                f.area+=area;if(glass)f.glass+=area;
                int u=axis==0?2:0;var tri=new[]{new Vector2(a[u],a.y),new Vector2(b[u],b.y),new Vector2(c[u],c.y)};
                f.triangles.Add(tri);foreach(var p in tri){f.minU=Mathf.Min(f.minU,p.x);f.maxU=Mathf.Max(f.maxU,p.x);f.minY=Mathf.Min(f.minY,p.y);f.maxY=Mathf.Max(f.maxY,p.y);}
            }
        }
        return faces.Values.ToList();
    }
    static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
    static bool Contains(Face f,Vector2 p)
    {
        return f.triangles.Any(t=>{float a=Cross(t[1]-t[0],p-t[0]),b=Cross(t[2]-t[1],p-t[1]),c=Cross(t[0]-t[2],p-t[2]);return a>=-.001f&&b>=-.001f&&c>=-.001f||a<=.001f&&b<=.001f&&c<=.001f;});
    }
    static Vector3 Point(Face f,float u,float y,float offset)=>f.axis==0?new Vector3(f.coordinate+f.sign*offset,y,u):new Vector3(u,y,f.coordinate+f.sign*offset);
    static void Quad(Face f,float u,float y,float w,float h,float offset,List<Vector3> vertices,List<int> triangles)
    {
        int n=vertices.Count;var a=Point(f,u-w/2,y-h/2,offset);var b=Point(f,u+w/2,y-h/2,offset);var c=Point(f,u+w/2,y+h/2,offset);var d=Point(f,u-w/2,y+h/2,offset);
        vertices.AddRange(new[]{a,b,c,d});var normal=Vector3.zero;normal[f.axis]=f.sign;
        if(Vector3.Dot(Vector3.Cross(b-a,c-a),normal)>0)triangles.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});else triangles.AddRange(new[]{n,n+2,n+1,n,n+3,n+2});
    }
    static void Facades(Scene scene)
    {
        string signature=EnvironmentExpansionBuilder.GameplaySignature(scene);
        var old=scene.GetRootGameObjects().FirstOrDefault(g=>g.name=="City_Facade_Window_Upgrade");if(old)UnityEngine.Object.DestroyImmediate(old);
        var root=new GameObject("City_Facade_Window_Upgrade").transform;
        var buildings=scene.GetRootGameObjects().First(g=>g.name=="City_Diverse_Skyline");
        var report=new List<string>();int total=0;
        var mats=new[]{StageDetailPolish.Mat("Facade_Window_Frame","#9BA7A7"),StageDetailPolish.Mat("Facade_Window_Dark","#304852"),StageDetailPolish.Mat("Facade_Window_Blue","#48616B"),StageDetailPolish.Mat("Facade_Window_Light","#66808A")};
        foreach(Transform building in buildings.transform)
        {
            if(!building.gameObject.activeInHierarchy)continue;var filter=building.GetComponent<MeshFilter>();if(!filter)continue;
            var faces=Faces(building,filter.sharedMesh,building.GetComponent<Renderer>().sharedMaterials);
            var vertices=new List<Vector3>();var triangles=Enumerable.Range(0,4).Select(i=>new List<int>()).ToArray();int windows=0,blankSides=0;
            foreach(var f in faces.GroupBy(f=>f.axis+":"+f.sign).Select(g=>g.OrderByDescending(f=>f.area).First()))
            {
                float width=f.maxU-f.minU,height=f.maxY-f.minY;
                if(f.area<35||width<5||height<7)continue;
                float glazing=faces.Where(q=>q.axis==f.axis&&Mathf.Abs(q.coordinate-f.coordinate)<.4f).Sum(q=>q.glass);
                if(glazing>f.area*.06f)continue;
                int added=0,cols=Mathf.Max(1,Mathf.FloorToInt((width-2)/2.8f));float spacing=(width-1.8f)/cols;
                for(int row=0;f.minY+3.8f+row*3.2f<f.maxY-1.2f;row++)for(int col=0;col<cols;col++)
                {
                    float u=f.minU+.9f+spacing*(col+.5f),y=f.minY+3.8f+row*3.2f;
                    bool fits=true;foreach(float dx in new[]{-.8f,.8f})foreach(float dy in new[]{-.95f,.95f})if(!Contains(f,new Vector2(u+dx,y+dy)))fits=false;
                    if(!fits)continue;
                    Quad(f,u,y,1.6f,1.9f,.04f,vertices,triangles[0]);Quad(f,u,y,1.36f,1.65f,.055f,vertices,triangles[1+(row+col)%3]);added++;
                }
                if(added>0)blankSides++;windows+=added;
            }
            if(windows==0)continue;
            var mesh=new Mesh();mesh.name=building.name+"_Windows";mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;mesh.SetVertices(vertices);mesh.subMeshCount=4;
            for(int i=0;i<4;i++)mesh.SetTriangles(triangles[i],i);mesh.RecalculateNormals();mesh.RecalculateBounds();
            string path=Art+mesh.name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(existing){EditorUtility.CopySerialized(mesh,existing);UnityEngine.Object.DestroyImmediate(mesh);Record(existing);}else{existing=mesh;AssetDatabase.CreateAsset(mesh,path);}
            var g=new GameObject(building.name+"_Facade_Windows");g.layer=2;g.transform.SetParent(root,false);g.AddComponent<MeshFilter>().sharedMesh=existing;g.AddComponent<MeshRenderer>().sharedMaterials=mats;
            report.Add(building.name+": blank facades="+blankSides+", windows="+windows);total+=windows;
        }
        report.Add("Added windows: "+total);File.WriteAllLines(Output+"Building_windows.txt",report);
        if(signature!=EnvironmentExpansionBuilder.GameplaySignature(scene))throw new InvalidOperationException("City gameplay changed.");
    }
}
