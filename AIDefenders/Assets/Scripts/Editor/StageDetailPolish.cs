using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public static class StageDetailPolish
{
    const string Folder="Assets/Art/StageDetails";
    const string City="Assets/Scenes/GameStages/City/City_1.unity";
    const string Desert="Assets/Scenes/GameStages/Desert/Desert_1.unity";
    static Material steel,black,white,blue,red,yellow,green,lid;
    static TMP_FontAsset font;

    [MenuItem("Tools/AI Defenders/Environment Expansion/9 Traffic Canyon And Larger GP")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop play mode first.");
        foreach(string path in new[]{City,Desert})
        {var s=SceneManager.GetSceneByPath(path);if(s.isLoaded&&s.isDirty)throw new InvalidOperationException("Unsaved scene: "+path);}
        Directory.CreateDirectory("Docs/StageDetailPolish/Before");
        foreach(string path in new[]{City,Desert,"Assets/Scenes/GameStages/Desert/Terrain_Desert_Day.asset"})
        {string dest="Docs/StageDetailPolish/Before/"+Path.GetFileName(path);if(!File.Exists(dest))File.Copy(path,dest);}
        Ensure(Folder);Edit(City,CityDetails);Edit(Desert,DesertDetails);AssetDatabase.SaveAssets();
    }
    static void Ensure(string path)
    {if(AssetDatabase.IsValidFolder(path))return;string parent=Path.GetDirectoryName(path).Replace('\\','/');Ensure(parent);AssetDatabase.CreateFolder(parent,Path.GetFileName(path));}
    static Transform[] All(Scene s){return s.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();}
    static void Edit(string path,Action<Scene> action)
    {
        var s=SceneManager.GetSceneByPath(path);bool opened=!s.isLoaded;if(opened)s=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
        var previous=SceneManager.GetActiveScene();SceneManager.SetActiveScene(s);
        try{action(s);EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);}finally{SceneManager.SetActiveScene(previous);if(opened)EditorSceneManager.CloseScene(s,true);}
    }
    internal static Material Mat(string name,string hex,bool emissive=false)
    {
        string path=Folder+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
        Color color;ColorUtility.TryParseHtmlString(hex,out color);m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",.22f);m.enableInstancing=true;
        if(emissive){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*1.8f);}EditorUtility.SetDirty(m);return m;
    }
    internal static GameObject Shape(Transform parent,string name,PrimitiveType type,Vector3 p,Vector3 scale,Material mat,Quaternion? rotation=null)
    {
        var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=p;go.transform.localScale=scale;if(rotation.HasValue)go.transform.localRotation=rotation.Value;
        go.layer=2;UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());go.GetComponent<Renderer>().sharedMaterial=mat;return go;
    }
    internal static void Line(Transform parent,string name,Vector3 a,Vector3 b,float width,Material mat)
    {Shape(parent,name,PrimitiveType.Cube,(a+b)*.5f,new Vector3(width,Vector3.Distance(a,b),width),mat,Quaternion.FromToRotation(Vector3.up,b-a));}
    static void Label(Transform parent,string name,string text,Vector3 p,Vector2 size,float fontSize,Material unused=null,Color? color=null)
    {
        if(!font)return;var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshPro));go.transform.SetParent(parent,false);go.transform.localPosition=p;go.layer=2;
        var tmp=go.GetComponent<TextMeshPro>();tmp.font=font;tmp.text=text;tmp.fontSize=fontSize;tmp.alignment=TextAlignmentOptions.Center;tmp.color=color??Color.white;tmp.rectTransform.sizeDelta=size;tmp.overflowMode=TextOverflowModes.Overflow;
    }
    static void CityDetails(Scene scene)
    {
        var all=All(scene);string before=EnvironmentExpansionBuilder.GameplaySignature(scene);
        var old=scene.GetRootGameObjects().FirstOrDefault(r=>r.name=="City_Traffic_Details");if(old)UnityEngine.Object.DestroyImmediate(old);
        all=All(scene);
        var root=new GameObject("City_Traffic_Details").transform;
        steel=Mat("Galvanized_Poles","#697679");black=Mat("Signal_Housing","#172022");white=Mat("Reflective_White","#E9ECE5");blue=Mat("Road_Sign_Blue","#15589D");
        red=Mat("Signal_Red_Off","#5D2522");yellow=Mat("Signal_Amber_Off","#66521D");green=Mat("Signal_Green_On","#33EAA5",true);lid=Mat("Manhole_Cast_Iron","#353E40");
        font=all.Select(t=>t.GetComponent<TMP_Text>()).Where(t=>t&&t.font).Select(t=>t.font).FirstOrDefault();
        int count=0;
        foreach(float x in new[]{-200f,-100f,0f,100f,200f})foreach(float z in new[]{-180f,-100f,-20f})
        {
            if(x==0&&z==-100)continue;
            var junction=new GameObject("Junction_"+count).transform;junction.SetParent(root);junction.position=new Vector3(x,.34f,z);
            SignalAssembly(junction,0);SignalAssembly(junction,90);Combine(junction,"Traffic_Junction_"+count);count++;
        }
        var covers=new GameObject("Manhole_Covers").transform;covers.SetParent(root);
        foreach(float x in new[]{-150f,-50f,50f,150f})foreach(float z in new[]{-184f,-104f,-24f})Manhole(covers,new Vector3(x,.355f,z));
        foreach(float x in new[]{-104f,4f,104f})foreach(float z in new[]{-220f,-155f,-45f,20f})Manhole(covers,new Vector3(x,.355f,z));
        Combine(covers,"City_Manholes");

        var gpParent=all.First(t=>t.name=="City_Urban_Refinement");var group=gpParent.Find("Shelter_GP_Enlarged");
        if(group)group.localScale=Vector3.one;
        else
        {
            group=new GameObject("Shelter_GP_Enlarged").transform;group.SetParent(gpParent);group.position=new Vector3(0,.34f,-100);
            foreach(var part in gpParent.Cast<Transform>().Where(t=>t.name.StartsWith("GP_")).ToArray())part.SetParent(group,true);
        }
        group.localScale=new Vector3(1.4f,1.3f,1.4f);
        // Keep targets stopping outside the enlarged bunker instead of inside its walls.
        var goal=all.First(t=>t.name=="Target Goal").GetComponent<BoxCollider>();goal.size=new Vector3(18,11,18);goal.center=new Vector3(0,4,0);
        foreach(var part in all.Where(t=>t.name=="FieldTent"&&t.parent&&t.parent.name=="City_Emergency_Shelter_Upgrade"))
        {var p=part.position;p.x=-15;part.position=p;if(PrefabUtility.IsPartOfPrefabInstance(part))PrefabUtility.RecordPrefabInstancePropertyModifications(part);}
        foreach(var part in all.Where(t=>t.name.StartsWith("MedicalTent_Cross_")||t.name.StartsWith("Triage")))
        {var p=part.position;p.x=part.name=="Triage_SignPost_L"?-19:part.name=="Triage_SignPost_R"?-11:-15;part.position=p;}
        if(before!=EnvironmentExpansionBuilder.GameplaySignature(scene))throw new InvalidOperationException("City gameplay transforms changed.");
        Debug.Log("Traffic detail: 28 horizontal signal assemblies, road signs, 24 manholes; GP enlarged 40% horizontally / 30% vertically.");
    }
    static void SignalAssembly(Transform parent,float yaw)
    {
        var assembly=new GameObject("Overhead_Signals_"+yaw).transform;assembly.SetParent(parent,false);assembly.localPosition=Quaternion.Euler(0,yaw,0)*new Vector3(0,0,-14);assembly.localRotation=Quaternion.Euler(0,yaw,0);
        // The horizontal arm carries lane-facing signal heads, as in the reference.
        Shape(assembly,"Pole",PrimitiveType.Cylinder,new Vector3(-15,4.2f,0),new Vector3(.3f,4.2f,.3f),steel);
        Shape(assembly,"Pole_Base",PrimitiveType.Cylinder,new Vector3(-15,.3f,0),new Vector3(.7f,.3f,.7f),steel);
        Shape(assembly,"Cross_Arm",PrimitiveType.Cube,new Vector3(-1,8.15f,0),new Vector3(28,.22f,.25f),steel);
        foreach(float lane in new[]{-7f,6f})
        {
            Shape(assembly,"Signal_Hanger",PrimitiveType.Cube,new Vector3(lane,7.9f,0),new Vector3(.13f,.6f,.13f),steel);
            Shape(assembly,"Horizontal_Head",PrimitiveType.Cube,new Vector3(lane,7.4f,-.1f),new Vector3(4.2f,1,.7f),black);
            for(int i=0;i<4;i++)
            {
                var p=new Vector3(lane-1.45f+i*.97f,7.4f,-.49f);Material lamp=i==0?red:i==1?yellow:green;
                Shape(assembly,"Signal_Lens",PrimitiveType.Cylinder,p,new Vector3(.65f,.035f,.65f),lamp,Quaternion.Euler(90,0,0));
                if(i==2)
                {
                    var q=p+new Vector3(0,0,-.06f);Line(assembly,"Green_Arrow",q+Vector3.right*.23f,q-Vector3.right*.23f,.09f,white);
                    Line(assembly,"Green_Arrow",q-Vector3.right*.23f,q+new Vector3(-.05f,.18f,0),.09f,white);Line(assembly,"Green_Arrow",q-Vector3.right*.23f,q+new Vector3(-.05f,-.18f,0),.09f,white);
                }
            }
        }
        var sign=new GameObject("U_Turn_Sign").transform;sign.SetParent(assembly,false);sign.localPosition=new Vector3(-7,9.55f,0);
        Shape(sign,"Blue_Round_Sign",PrimitiveType.Cylinder,Vector3.zero,new Vector3(1.9f,.045f,1.9f),blue,Quaternion.Euler(90,0,0));
        var curve=new List<Vector3>{new Vector3(-.42f,-.55f,-.07f),new Vector3(-.42f,.15f,-.07f)};
        for(int i=0;i<=12;i++){float a=Mathf.PI-i*Mathf.PI/12;curve.Add(new Vector3(Mathf.Cos(a)*.42f,.15f+Mathf.Sin(a)*.42f,-.07f));}curve.Add(new Vector3(.42f,-.25f,-.07f));
        for(int i=1;i<curve.Count;i++)Line(sign,"U_Turn_Arrow",curve[i-1],curve[i],.13f,white);
        Line(sign,"U_Turn_Arrow",new Vector3(.42f,-.25f,-.07f),new Vector3(.2f,-.02f,-.07f),.13f,white);Line(sign,"U_Turn_Arrow",new Vector3(.42f,-.25f,-.07f),new Vector3(.64f,-.02f,-.07f),.13f,white);
        Shape(assembly,"Road_Name_Plate",PrimitiveType.Cube,new Vector3(-15,5.6f,-.45f),new Vector3(4.5f,1.1f,.16f),blue);
        Label(assembly,"Road_Name","영등포로 / Yeongdeungpo-ro",new Vector3(-15,5.6f,-.55f),new Vector2(4.3f,1),3);
        Shape(assembly,"Speed_Sign_Rim",PrimitiveType.Cylinder,new Vector3(-15,3.7f,-.3f),new Vector3(1.55f,.05f,1.55f),Mat("Speed_Sign_Red","#C94332"),Quaternion.Euler(90,0,0));
        Shape(assembly,"Speed_Sign_Face",PrimitiveType.Cylinder,new Vector3(-15,3.7f,-.37f),new Vector3(1.3f,.025f,1.3f),white,Quaternion.Euler(90,0,0));
        Label(assembly,"Speed_Limit","40",new Vector3(-15,3.7f,-.42f),new Vector2(1.2f,1.1f),6,null,Color.black);
    }
    static void Manhole(Transform parent,Vector3 p)
    {
        Shape(parent,"Manhole_Rim",PrimitiveType.Cylinder,p,new Vector3(1.4f,.018f,1.4f),steel);
        Shape(parent,"Manhole_Lid",PrimitiveType.Cylinder,p+Vector3.up*.022f,new Vector3(1.23f,.012f,1.23f),lid);
        for(int i=-3;i<=3;i++)Shape(parent,"Manhole_Grating",PrimitiveType.Cube,p+new Vector3(i*.14f,.04f,0),new Vector3(.025f,.014f,Mathf.Sqrt(.32f-i*i*.0196f)*2),steel);
        foreach(float x in new[]{-.45f,.45f})Shape(parent,"Manhole_Key",PrimitiveType.Cube,p+new Vector3(x,.042f,0),new Vector3(.09f,.015f,.19f),black);
    }
    internal static void Combine(Transform group,string name)
    {
        var filters=group.GetComponentsInChildren<MeshFilter>().Where(f=>f.GetComponent<TMP_Text>()==null).ToArray();var mats=new List<Material>();var batches=new List<List<CombineInstance>>();
        foreach(var f in filters)
        {
            var renderer=f.GetComponent<MeshRenderer>();if(!renderer)continue;
            for(int sub=0;sub<f.sharedMesh.subMeshCount;sub++)
            {var material=renderer.sharedMaterials[sub];int index=mats.IndexOf(material);if(index<0){index=mats.Count;mats.Add(material);batches.Add(new List<CombineInstance>());}batches[index].Add(new CombineInstance{mesh=f.sharedMesh,subMeshIndex=sub,transform=group.worldToLocalMatrix*f.transform.localToWorldMatrix});}
        }
        var meshes=new List<Mesh>();foreach(var batch in batches){var m=new Mesh();m.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;m.CombineMeshes(batch.ToArray(),true,true);meshes.Add(m);}
        var combined=new Mesh();combined.name=name;combined.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;combined.CombineMeshes(meshes.Select(m=>new CombineInstance{mesh=m,transform=Matrix4x4.identity}).ToArray(),false,true);combined.RecalculateBounds();
        string path=Folder+"/"+name+".asset";var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(saved){EditorUtility.CopySerialized(combined,saved);UnityEngine.Object.DestroyImmediate(combined);EditorUtility.SetDirty(saved);}else{saved=combined;AssetDatabase.CreateAsset(saved,path);}
        group.gameObject.layer=2;group.gameObject.AddComponent<MeshFilter>().sharedMesh=saved;group.gameObject.AddComponent<MeshRenderer>().sharedMaterials=mats.ToArray();
        foreach(var f in filters){UnityEngine.Object.DestroyImmediate(f.GetComponent<MeshRenderer>());UnityEngine.Object.DestroyImmediate(f);}
        foreach(var m in meshes)UnityEngine.Object.DestroyImmediate(m);
    }
    static float RouteDistance(Vector3[][] paths,float x,float z)
    {
        float d=float.MaxValue;var point=new Vector2(x,z);foreach(var path in paths)for(int i=1;i<path.Length;i++)
        {var a=new Vector2(path[i-1].x,path[i-1].z);var v=new Vector2(path[i].x,path[i].z)-a;float t=v.sqrMagnitude>0?Mathf.Clamp01(Vector2.Dot(point-a,v)/v.sqrMagnitude):0;d=Mathf.Min(d,Vector2.Distance(point,a+t*v));}return d;
    }
    static void DesertDetails(Scene scene)
    {
        string before=EnvironmentExpansionBuilder.GameplaySignature(scene);var all=All(scene);
        var extra=all.FirstOrDefault(t=>t.name=="FieldTent"&&t.parent&&t.parent.name=="Environment_Asset_Expansion");
        if(extra){extra.gameObject.SetActive(false);if(PrefabUtility.IsPartOfPrefabInstance(extra))PrefabUtility.RecordPrefabInstancePropertyModifications(extra.gameObject);}
        var terrain=all.Select(t=>t.GetComponent<Terrain>()).First(t=>t);var data=terrain.terrainData;
        var paths=all.Select(t=>t.GetComponent<EnemySpawner>()).Where(p=>p).SelectMany(p=>p.paths.Select(q=>new[]{p.transform.position}.Concat(q.GetWorldPoints()).ToArray())).ToArray();
        int n=data.heightmapResolution;var heights=data.GetHeights(0,0,n,n);var size=data.size;var origin=terrain.transform.position;
        for(int j=0;j<n;j++)for(int i=0;i<n;i++)
        {
            float x=origin.x+i*size.x/(n-1),z=origin.z+j*size.z/(n-1);
            if(x < -160||x>180||z < -150||z>65)continue;
            float d=RouteDistance(paths,x,z);if(d<10)continue;
            float basin=Vector2.Distance(new Vector2(x,z),new Vector2(0,65));float preserveBase=Mathf.SmoothStep(0,1,Mathf.InverseLerp(34,65,basin));
            float edge=Mathf.SmoothStep(0,1,Mathf.InverseLerp(0,35,Mathf.Min(x+160,180-x,z+150,65-z)));
            float shelf=12*Mathf.SmoothStep(0,1,Mathf.InverseLerp(11,23,d))+11*Mathf.SmoothStep(0,1,Mathf.InverseLerp(31,44,d));
            shelf+=(Mathf.PerlinNoise(x*.028f+17,z*.026f+29)-.5f)*1.4f;
            float oldY=origin.y+heights[j,i]*size.y;float y=Mathf.Lerp(oldY,Mathf.Max(oldY,shelf),preserveBase*edge);
            heights[j,i]=(y-origin.y)/size.y;
        }
        data.SetHeights(0,0,heights);EditorUtility.SetDirty(data);
        // Re-seat rocks and plants after sculpting; the defended compound remains level.
        foreach(var mf in all.Select(t=>t.GetComponent<MeshFilter>()).Where(m=>m&&m.gameObject.activeInHierarchy))
        {
            var t=mf.transform;bool decor=t.IsChildOf(all.First(a=>a.name=="Desert_Environment"))||t.parent&&t.parent.name=="Desert_Cactus_Upgrade"||t.parent&&t.parent.name=="Environment_Asset_Expansion";
            if(!decor||Vector2.Distance(new Vector2(t.position.x,t.position.z),new Vector2(0,65))<38)continue;
            if(t.name.StartsWith("Oasis_")||t.name=="Defense_Supply_Camp")continue;
            var p=t.position;p.y=terrain.SampleHeight(p)+origin.y-mf.sharedMesh.bounds.min.y*t.lossyScale.y-.025f;t.position=p;
            if(PrefabUtility.IsPartOfPrefabInstance(t))PrefabUtility.RecordPrefabInstancePropertyModifications(t);
        }
        if(before!=EnvironmentExpansionBuilder.GameplaySignature(scene))throw new InvalidOperationException("Desert gameplay transforms changed.");
        Debug.Log("Desert: winding low canyon with softly eroded two-level shoulders, original camp retained, overlapping extra tent hidden.");
    }
}
