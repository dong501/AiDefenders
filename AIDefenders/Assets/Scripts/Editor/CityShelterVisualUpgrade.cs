using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CityShelterVisualUpgrade
{
    const string ScenePath = "Assets/Scenes/GameStages/City/City_1.unity";
    const string RootName = "City_Emergency_Shelter_Upgrade";
    const string Folder = "Assets/Art/EnvironmentExpansion/Generated/";
    static Transform root;
    static TMP_FontAsset font;
    static Material steel, yellow, white, blue, red, dark;

    [MenuItem("Tools/AI Defenders/Environment Expansion/5 Upgrade Emergency Shelter")]
    public static void Apply()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode first.");
        var scene=SceneManager.GetSceneByPath(ScenePath); bool opened=!scene.isLoaded;
        if (!opened && scene.isDirty) throw new InvalidOperationException("Save city edits first.");
        if (opened) scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Additive);
        try
        {
            Directory.CreateDirectory("Docs/CityFourWay/BeforeShelter");
            if (!File.Exists("Docs/CityFourWay/BeforeShelter/City_1.unity")) File.Copy(ScenePath,"Docs/CityFourWay/BeforeShelter/City_1.unity");
            ApplyToScene(scene); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        }
        finally { if(opened) EditorSceneManager.CloseScene(scene,true); }
    }

    public static void ApplyToScene(Scene scene)
    {
        string before=EnvironmentExpansionBuilder.GameplaySignature(scene);
        var all=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
        font=all.Select(t=>t.GetComponent<TMP_Text>()).Where(t=>t&&t.font).First().font;
        var old=scene.GetRootGameObjects().FirstOrDefault(r=>r.name==RootName);
        if(old) UnityEngine.Object.DestroyImmediate(old);
        root=new GameObject(RootName).transform; SceneManager.MoveGameObjectToScene(root.gameObject,scene);
        steel=Mat("CheckpointSteel",new Color(.17f,.23f,.24f));
        yellow=Mat("CheckpointYellow",new Color(.93f,.65f,.15f));
        white=Mat("TriageWhite",new Color(.8f,.85f,.82f));
        blue=Mat("MedicalBlue",new Color(.1f,.48f,.65f));
        red=Mat("EmergencyRed",new Color(.9f,.12f,.09f),true);
        dark=Mat("CheckpointBlack",new Color(.07f,.085f,.09f));

        // Replace signs and the van occupying the emergency medical parking bay.
        foreach(var t in all.Where(t=>t && t.parent && (t.parent.name=="City_FourWay_Streetscape" || t.parent.name=="Environment_Asset_Expansion")))
        {
            bool hide=t.name.StartsWith("Shelter_Entrance_Sign_")
                || (t.name=="CityMinivan" && Vector3.Distance(t.position,new Vector3(18,t.position.y,-89))<1)
                || (t.name=="ParkInformationBooth" && Mathf.Abs(t.position.x-18)<1 && Mathf.Abs(t.position.z+110)<1)
                || (t.name=="ParkShrubPlanter" && Mathf.Abs(t.position.z+112)<1);
            if(hide) { t.gameObject.SetActive(false); if(PrefabUtility.IsPartOfPrefabInstance(t)) PrefabUtility.RecordPrefabInstancePropertyModifications(t.gameObject); }
        }

        Prop("Ambulance",18,-88,1.8f,180);
        Prop("Ambulance",14,-112,1.8f,8);
        Prop("PoliceCar",-19,-91,1.8f,-15);
        // Existing police car at (-19,-111) remains at the other checkpoint.
        var tent=Prop("FieldTent",-12,-112,1.2f,180);
        var tentBounds=tent.GetComponent<Renderer>().bounds;
        Box("MedicalTent_Cross_H",new Vector3(-12,tentBounds.max.y+.03f,-112),new Vector3(3.2f,.06f,.8f),blue);
        Box("MedicalTent_Cross_V",new Vector3(-12,tentBounds.max.y+.05f,-112),new Vector3(.8f,.06f,3.2f),blue);
        Box("Triage_Signboard",new Vector3(-12,3.3f,-116.85f),new Vector3(10,1.5f,.18f),steel);
        Box("Triage_SignPost_L",new Vector3(-16,1.3f,-116.85f),new Vector3(.15f,2.6f,.15f),steel);
        Box("Triage_SignPost_R",new Vector3(-8,1.3f,-116.85f),new Vector3(.15f,2.6f,.15f),steel);
        Label("Triage", "응급 진료 / MEDICAL",new Vector3(-12,3.3f,-117),Quaternion.identity,10,1.4f,white.color,3.4f);
        Prop("BaseSupplyPallet",-12,-119,1.1f);
        Prop("CrateStack",-19,-118,1.4f,10);
        Prop("JerryCan",-21,-118,1.3f);
        Prop("BaseFieldGenerator",-20,-78,1.6f,90);
        Prop("BaseSupplyPallet",-18,-83,1.3f);
        Prop("Watchtower",-21,-69,1.25f,180);
        Prop("GuardBooth",21,-114,1.2f,-90);
        Prop("CheckpointSign",22,-119,1.2f,180);
        Prop("CheckpointSign",-22,-81,1.2f,90);

        // Four 16m openings leave the existing approach routes clear.
        Gate("South",new Vector3(0,0,-120),Vector3.back,"중앙 대피소 / SHELTER");
        Gate("North",new Vector3(0,0,-63),Vector3.forward,"생존자 대피 구역 / SAFE ZONE");
        Gate("West",new Vector3(-25,0,-100),Vector3.left,"검문소 / CHECKPOINT");
        Gate("East",new Vector3(25,0,-100),Vector3.right,"의료 지원 / MEDICAL AID");
        foreach(float z in new[]{-120f,-63f}) foreach(float x in new[]{-17f,17f}) FenceLine(new Vector3(x-8,1.55f,z),Vector3.right,16);
        foreach(float x in new[]{-25f,25f})
        {
            FenceLine(new Vector3(x,1.55f,-120),Vector3.forward,12);
            FenceLine(new Vector3(x,1.55f,-92),Vector3.forward,29);
        }
        foreach(float x in new[]{-21f,21f}) foreach(float z in new[]{-117f,-67f})
        {
            Prop("SandbagStraight",x,z,1.2f,x<0?15:-15);
            var lamp=Prop("GroundSearchlight",x,z+2,1.6f,x<0?90:-90);
            var lightObject=new GameObject("Shelter_Floodlight"); lightObject.transform.SetParent(root);
            lightObject.transform.position=lamp.transform.position+Vector3.up*4.6f;
            lightObject.transform.LookAt(new Vector3(0,0,-100));
            var light=lightObject.AddComponent<Light>(); light.type=LightType.Spot; light.color=new Color(.8f,.91f,1);
            light.intensity=350;light.range=30;light.spotAngle=80;light.shadows=LightShadows.None;
        }
        Label("QuarantineNotice","출입 전 검역  ·  감염 의심자 신고",new Vector3(0,4.8f,-120.15f),Quaternion.identity,16,1,Color.white,2.5f);
        if(before!=EnvironmentExpansionBuilder.GameplaySignature(scene)) throw new InvalidOperationException("Gameplay layout changed.");
        Debug.Log("Emergency shelter visual upgrade applied. Four approaches and gameplay transforms preserved.");
    }

    static GameObject Prop(string name,float x,float z,float scale=1,float yaw=0)
    {
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+name+".prefab");
        if(!prefab) throw new InvalidOperationException("Missing shelter asset: "+name);
        var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,root);
        var mesh=prefab.GetComponent<MeshFilter>().sharedMesh;
        go.transform.position=new Vector3(x,.34f-mesh.bounds.min.y*scale,z);
        go.transform.localScale=Vector3.one*scale;go.transform.rotation=Quaternion.Euler(0,yaw,0);return go;
    }
    static Material Mat(string name,Color color,bool emission=false)
    {
        string path=Folder+"Shelter_"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!mat){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}
        mat.color=color;mat.SetColor("_BaseColor",color);mat.SetFloat("_Smoothness",.15f);
        if(emission){mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",color*2);}
        EditorUtility.SetDirty(mat);return mat;
    }
    static void Box(string name,Vector3 position,Vector3 size,Material material,Quaternion? rotation=null)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(root);
        go.transform.position=position;go.transform.localScale=size;go.layer=2;
        if(rotation.HasValue)go.transform.rotation=rotation.Value;
        UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());go.GetComponent<Renderer>().sharedMaterial=material;
    }
    static void Label(string name,string text,Vector3 position,Quaternion rotation,float width,float height,Color color,float size)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshPro));go.transform.SetParent(root);go.layer=2;
        go.transform.position=position;go.transform.rotation=rotation;
        var label=go.GetComponent<TextMeshPro>();label.font=font;label.text=text;label.fontSize=size;
        label.enableAutoSizing=true;label.fontSizeMin=size;label.fontSizeMax=size*3f;
        label.alignment=TextAlignmentOptions.Center;label.color=color;label.rectTransform.sizeDelta=new Vector2(width,height);
        label.overflowMode=TextOverflowModes.Overflow;
        label.ForceMeshUpdate();
    }
    static void Gate(string name,Vector3 center,Vector3 outward,string title)
    {
        var side=Vector3.Cross(Vector3.up,outward);
        Quaternion rotation=Quaternion.LookRotation(-outward);
        foreach(float sign in new[]{-1f,1f})
        {
            Box(name+"_Gate_Post",center+side*8.5f*sign+Vector3.up*3.2f,new Vector3(.65f,6.4f,.65f),steel);
            Box(name+"_WarningBeacon",center+side*8.5f*sign+Vector3.up*6.7f,new Vector3(.5f,.4f,.5f),red);
        }
        Box(name+"_GateHeader",center+Vector3.up*6.15f,new Vector3(18,1.5f,.5f),steel,rotation);
        Label(name+"_GateTitle",title,center+Vector3.up*6.15f+outward*.3f,rotation,17,1.3f,Color.white,3.8f);
        Box(name+"_SafetyStripe",center+Vector3.up*5.32f,new Vector3(18,.15f,.6f),yellow,rotation);
    }
    static void FenceLine(Vector3 start,Vector3 axis,float length)
    {
        int count=Mathf.FloorToInt(length/3.2f);
        for(int i=0;i<count;i++)
        {
            var center=start+axis*(1.6f+i*3.2f);
            var fence=Prop("BarbedFence",center.x,center.z,1,axis.z>.5f?90:0);
            fence.transform.position+=Vector3.up*1.25f;
        }
    }
}
