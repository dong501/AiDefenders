using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;

// Scene-only decoration; no changes to spawn paths, targets or combat rules.
public static class BaseCampDetailUpgrade
{
    const string Art="Assets/Art/StageDetails/";
    const string Props="Assets/Art/EnvironmentExpansion/Generated/";
    const string City="Assets/Scenes/GameStages/City/City_1.unity";
    const string Desert="Assets/Scenes/GameStages/Desert/Desert_1.unity";
    static Material steel,white,glass,olive,yellow,dark,concrete;
    static TMP_FontAsset font;
    static Transform root;
    static Transform[] All(Scene s) => s.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();

    [MenuItem("Tools/AI Defenders/Environment Expansion/10 Base And Shelter Details")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop play mode first.");
        foreach(var path in new[]{City,Desert}){var s=SceneManager.GetSceneByPath(path);if(s.isLoaded&&s.isDirty)throw new InvalidOperationException("Save scene edits first: "+path);}
        Directory.CreateDirectory("Docs/BaseCampUpgrade/Before");
        foreach(var path in new[]{City,Desert,"Assets/Scenes/GameStages/Desert/Terrain_Desert_Day.asset"})
        {string dest="Docs/BaseCampUpgrade/Before/"+Path.GetFileName(path);if(!File.Exists(dest))File.Copy(path,dest);}
        steel=StageDetailPolish.Mat("Camp_Grey_Steel","#9DA79F");white=StageDetailPolish.Mat("Camp_Trim","#DBDED3");
        glass=StageDetailPolish.Mat("Camp_Glass","#284951");olive=StageDetailPolish.Mat("Camp_Olive","#485540");
        yellow=StageDetailPolish.Mat("Camp_Safety_Yellow","#E9B929");dark=StageDetailPolish.Mat("Camp_Dark","#242D29");concrete=StageDetailPolish.Mat("Camp_Concrete","#969988");
        if(AssetDatabase.FindAssets("DesertTank t:Prefab").Length==0)AIDefenders.Assets.TankPrefabBuilder.Build();
        var original=SceneManager.GetActiveScene();
        foreach(var path in new[]{City,Desert})
        {
            var s=SceneManager.GetSceneByPath(path);bool opened=!s.isLoaded;if(opened)s=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
            SceneManager.SetActiveScene(s);
            try
            {
                string signature=EnvironmentExpansionBuilder.GameplaySignature(s);
                string name=path==City?"City_Shelter_Living_Upgrade":"Desert_Military_Base_Upgrade";
                var old=s.GetRootGameObjects().FirstOrDefault(g=>g.name==name);if(old)UnityEngine.Object.DestroyImmediate(old);
                var all=All(s);font=all.Select(t=>t.GetComponent<TMP_Text>()).Where(t=>t&&t.font).Select(t=>t.font).FirstOrDefault();
                root=new GameObject(name).transform;
                if(path==City)CityDetails(s,all);else DesertDetails(s,all);
                if(signature!=EnvironmentExpansionBuilder.GameplaySignature(s))throw new InvalidOperationException("Gameplay transforms changed.");
                EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);
            }
            finally{SceneManager.SetActiveScene(original);if(opened)EditorSceneManager.CloseScene(s,true);}
        }
        AssetDatabase.SaveAssets();
    }
    static void Hide(Transform t){t.gameObject.SetActive(false);if(PrefabUtility.IsPartOfPrefabInstance(t))PrefabUtility.RecordPrefabInstancePropertyModifications(t.gameObject);}
    static Transform Group(string name,Vector3 p,float yaw=0)
    {var t=new GameObject(name).transform;t.SetParent(root,false);t.position=p;t.rotation=Quaternion.Euler(0,yaw,0);return t;}
    static void Box(Transform p,string name,Vector3 v,Vector3 size,Material m,Quaternion? q=null)
    {StageDetailPolish.Shape(p,name,PrimitiveType.Cube,v,size,m,q);}
    static void Rod(Transform p,string name,Vector3 a,Vector3 b,float width,Material m)=>StageDetailPolish.Line(p,name,a,b,width,m);
    static void Label(Transform p,string name,string text,Vector3 pos,Vector2 size,float fontSize)
    {
        if(!font)return;var g=new GameObject(name,typeof(RectTransform),typeof(TextMeshPro));g.transform.SetParent(p,false);g.transform.localPosition=pos;g.layer=2;
        var tmp=g.GetComponent<TextMeshPro>();tmp.font=font;tmp.text=text;tmp.fontSize=fontSize;tmp.color=Color.white;tmp.alignment=TextAlignmentOptions.Center;tmp.rectTransform.sizeDelta=size;
    }
    static GameObject Prop(string name,Vector3 position,float scale=1,float yaw=0)
    {
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Props+name+".prefab");if(!prefab)throw new InvalidOperationException("Missing prop: "+name);
        var g=(GameObject)PrefabUtility.InstantiatePrefab(prefab,root);g.transform.localScale=Vector3.one*scale;g.transform.rotation=Quaternion.Euler(0,yaw,0);g.transform.position=position;
        var b=g.GetComponentsInChildren<Renderer>().First().bounds;g.transform.position+=Vector3.up*(position.y-b.min.y);return g;
    }
    static void CityDetails(Scene s,Transform[] all)
    {
        foreach(var t in all.Where(t=>t.name.Contains("_Gate_Post")||t.name.EndsWith("_GateHeader")||t.name.EndsWith("_GateTitle")||t.name.EndsWith("_SafetyStripe")||t.name.EndsWith("_WarningBeacon")||t.name=="QuarantineNotice"))Hide(t);
        // Small wall plaque supplied by the user, next to the main bunker entrance.
        var plaque=Group("Shelter_Red_Plaque",new Vector3(3.8f,3.5f,-107.65f));
        Box(plaque,"Plaque_Frame",Vector3.zero,new Vector3(1.65f,1.95f,.12f),white);
        var material=StageDetailPolish.Mat("Shelter_Plaque_Image","#FFFFFF");material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"Shelter_Plaque.png"));EditorUtility.SetDirty(material);
        StageDetailPolish.Shape(plaque,"Shelter_Plaque_Face",PrimitiveType.Quad,new Vector3(0,0,-.071f),new Vector3(1.52f,1.82f,1),material);
        CleanBakedGate(all);
        Container(new Vector3(-11,.34f,-69),0,"숙소 A");Container(new Vector3(11,.34f,-69),0,"숙소 B");
        Checkpoint("South",new Vector3(0,.34f,-120),0);
        Checkpoint("North",new Vector3(0,.34f,-63),180);
        Checkpoint("West",new Vector3(-25,.34f,-100),90);
        Checkpoint("East",new Vector3(25,.34f,-100),-90);
    }
    static void CleanBakedGate(Transform[] all)
    {
        var f=all.First(t=>t.name=="Evacuation_Shelter").GetComponent<MeshFilter>();
        var source=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Art/EnvironmentExpansion/Generated/CityShelterFourWay.asset");
        var copy=UnityEngine.Object.Instantiate(source);var vertices=source.vertices;
        for(int sub=0;sub<source.subMeshCount;sub++)
        {
            var triangles=source.GetTriangles(sub);var keep=new System.Collections.Generic.List<int>();
            for(int i=0;i<triangles.Length;i+=3)
            {
                var point=f.transform.TransformPoint((vertices[triangles[i]]+vertices[triangles[i+1]]+vertices[triangles[i+2]])/3);
                if(point.z < -114 && Mathf.Abs(point.x)<10 && point.y>.35f)continue;
                keep.Add(triangles[i]);keep.Add(triangles[i+1]);keep.Add(triangles[i+2]);
            }
            copy.SetTriangles(keep,sub);
        }
        copy.RecalculateBounds();string path=Art+"Shelter_Without_Old_Gantry.asset";
        var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(saved){EditorUtility.CopySerialized(copy,saved);UnityEngine.Object.DestroyImmediate(copy);EditorUtility.SetDirty(saved);}else{saved=copy;AssetDatabase.CreateAsset(saved,path);}
        f.sharedMesh=saved;
    }
    static void Container(Vector3 p,float yaw,string title)
    {
        var c=Group("Container_Residence_"+title,p,yaw);
        Box(c,"Foundation",new Vector3(0,.12f,0),new Vector3(10.6f,.24f,4.8f),concrete);
        Box(c,"Container_Shell",new Vector3(0,1.95f,0),new Vector3(10,3.5f,4.2f),steel);
        foreach(float z in new[]{-2.15f,2.15f})for(int i=0;i<29;i++)Box(c,"Corrugation",new Vector3(-4.9f+i*.35f,1.95f,z),new Vector3(.08f,3.35f,.1f),white);
        Box(c,"Roof_Overhang",new Vector3(0,3.8f,0),new Vector3(10.4f,.22f,4.5f),white);
        foreach(float x in new[]{-2.9f,0f})
        {
            Box(c,"Window_Frame",new Vector3(x,2.25f,-2.25f),new Vector3(2.15f,1.65f,.16f),white);
            Box(c,"Window_Glass",new Vector3(x,2.25f,-2.35f),new Vector3(1.91f,1.4f,.08f),glass);
            for(int i=-1;i<=1;i++)Box(c,"Window_Bars",new Vector3(x+i*.52f,2.25f,-2.43f),new Vector3(.045f,1.5f,.045f),white);
            Box(c,"Window_Crossbar",new Vector3(x,2.25f,-2.44f),new Vector3(2,.05f,.04f),white);
            Box(c,"Window_Awning",new Vector3(x,3.16f,-2.42f),new Vector3(2.4f,.12f,.65f),dark,Quaternion.Euler(-8,0,0));
        }
        Box(c,"Door_Frame",new Vector3(3.2f,1.68f,-2.27f),new Vector3(1.55f,2.95f,.14f),white);
        Box(c,"Door",new Vector3(3.2f,1.65f,-2.36f),new Vector3(1.3f,2.7f,.1f),olive);
        Box(c,"Door_Handle",new Vector3(3.66f,1.6f,-2.46f),new Vector3(.08f,.27f,.1f),dark);
        Box(c,"Step",new Vector3(3.2f,.2f,-2.9f),new Vector3(1.8f,.4f,.9f),concrete);
        Box(c,"Residence_Nameplate",new Vector3(1.6f,3.38f,-2.24f),new Vector3(2.1f,.43f,.12f),olive);
        Label(c,"Residence_Label",title,new Vector3(1.6f,3.38f,-2.32f),new Vector2(2,.4f),1.4f);
        StageDetailPolish.Combine(c,"Residence_"+Mathf.RoundToInt(p.x));
    }
    static void Checkpoint(string name,Vector3 p,float yaw)
    {
        var c=Group("Checkpoint_"+name,p,yaw);
        foreach(float x in new[]{-8.5f,8.5f})
        {
            Box(c,"Guardhouse_Base",new Vector3(x,1.1f,0),new Vector3(3.5f,2.2f,3.2f),concrete);
            // Brick joints and roof overhang echo the reference checkpoint.
            for(int row=0;row<5;row++)Box(c,"Brick_Seam",new Vector3(x,.35f+row*.38f,-1.61f),new Vector3(3.4f,.035f,.02f),white);
            Box(c,"Guardhouse_Window",new Vector3(x,2.66f,-.1f),new Vector3(3.1f,1.1f,2.6f),glass);
            foreach(float dx in new[]{-1.55f,1.55f})foreach(float dz in new[]{-1.35f,1.35f})Box(c,"Guardhouse_Post",new Vector3(x+dx,2.72f,dz),new Vector3(.18f,1.4f,.18f),olive);
            Box(c,"Guardhouse_Roof",new Vector3(x,3.52f,0),new Vector3(4.2f,.38f,3.9f),olive);
            Box(c,"Barrier_Pivot",new Vector3(x>0?6.2f:-6.2f,1,1),new Vector3(.65f,2,.7f),yellow);
        }
        // Raised barrier arm leaves the four existing invasion lanes open.
        Rod(c,"Raised_Boom",new Vector3(-6.2f,1.7f,1),new Vector3(-4.9f,6.7f,1),.19f,white);
        for(int i=0;i<5;i++)Box(c,"Boom_Red_Band",new Vector3(-6.2f+i*.26f,1.7f+i,1),new Vector3(.24f,.34f,.24f),yellow,Quaternion.Euler(0,0,-15));
        Barricade(c,new Vector3(-4.2f,0,-3),0);Barricade(c,new Vector3(4.2f,0,-3),0);
        foreach(float x in new[]{-4.2f,4.2f})Box(c,"Concrete_Road_Block",new Vector3(x,.5f,-5),new Vector3(2.5f,1,1.1f),concrete);
        StageDetailPolish.Combine(c,"Checkpoint_"+name);
    }
    static void Barricade(Transform p,Vector3 pos,float yaw)
    {
        CityPipeBarricadeUpgrade.Create(p,pos,yaw);
    }
    static void DesertDetails(Scene s,Transform[] all)
    {
        var terrain=all.Select(t=>t.GetComponent<Terrain>()).First(t=>t);var data=terrain.terrainData;
        // Grow the usable compound, gently blending the outside back into the dunes.
        Flatten(terrain,new Vector2(0,69),new Vector2(31,28),0,8);
        var boundary=all.First(t=>t.name=="Desert_Defense_Outpost");
        foreach(var t in boundary.Cast<Transform>().Where(t=>t.name.StartsWith("Hesco_")).ToArray())Hide(t);
        var template=boundary.Cast<Transform>().First(t=>t.name=="Hesco_1");
        for(int side=-1;side<=1;side+=2)for(int i=0;i<14;i++)Hesco(template,new Vector3(side*30,0,43+i*4),0);
        for(int i=0;i<15;i++)Hesco(template,new Vector3(-28+i*4,0,97),90);
        foreach(int side in new[]{-1,1})for(int i=0;i<6;i++)Hesco(template,new Vector3(side*(8+i*4),0,41),90);
        foreach(var t in all.Where(t=>t.gameObject.activeInHierarchy&&t.name.StartsWith("Defense_")&&t.name!="Defense_Supply_Camp"))
        {if(t.name=="Defense_Watchtower")MoveGrounded(t,new Vector3(-25,0,83),terrain);if(t.name=="Defense_WaterTank")MoveGrounded(t,new Vector3(25,0,86),terrain);}
        Tank(new Vector3(-20,0,57),165);Tank(new Vector3(20,0,66),195);
        Prop("BaseFieldGenerator",new Vector3(-24,0,74),1.6f,90);Prop("BaseSupplyPallet",new Vector3(24,0,77),1.5f);
        Prop("SandbagStraight",new Vector3(-20,0,47),1.6f);Prop("SandbagStraight",new Vector3(20,0,47),1.6f);
        // Put the ruin cluster on one coherent terrace, rather than straddling the new slope.
        Vector2 center=new Vector2(-82,20);float height=terrain.SampleHeight(new Vector3(center.x,0,center.y))+terrain.transform.position.y;
        Flatten(terrain,center,new Vector2(15,11),height,7);
        string[] names={"RuinGate","BrokenColumn","FallenColumn","BrokenRuinWall"};
        Vector3[] placements={new Vector3(-82,0,20),new Vector3(-91,0,24),new Vector3(-76,0,27),new Vector3(-89,0,15)};
        for(int i=0;i<names.Length;i++)
        {var t=all.FirstOrDefault(a=>a.name==names[i]&&a.parent&&a.parent.name=="Environment_Asset_Expansion");if(t)MoveGrounded(t,placements[i],terrain);}
        // Re-seat the small surrounding plants touched by either terrain patch.
        foreach(var t in all.Where(t=>t.gameObject.activeInHierarchy&&(t.name.StartsWith("Dry_Shrub_")||t.name.StartsWith("Rock_Cluster_")||t.name=="Outpost_Palm")))
        {var p=t.position;if((Mathf.Abs(p.x)<40&&p.z>35&&p.z<105)||Vector2.Distance(new Vector2(p.x,p.z),center)<24)MoveGrounded(t,p,terrain);}
        foreach(var rock in all.Where(t=>t.gameObject.activeInHierarchy&&t.parent&&(t.parent.name=="Scattered_Sandstone"||t.parent.name=="Sandstone_Outcrops")))
        {if(Vector2.Distance(new Vector2(rock.position.x,rock.position.z),center)<13)Hide(rock);}
        EditorUtility.SetDirty(data);
    }
    static void MoveGrounded(Transform t,Vector3 p,Terrain terrain)
    {
        t.position=new Vector3(p.x,0,p.z);float lowest=t.GetComponentsInChildren<Renderer>().Min(r=>r.bounds.min.y);
        p.y=terrain.SampleHeight(p)+terrain.transform.position.y-lowest-.035f;t.position=p;
        if(PrefabUtility.IsPartOfPrefabInstance(t))PrefabUtility.RecordPrefabInstancePropertyModifications(t);
    }
    static void Hesco(Transform template,Vector3 p,float yaw)
    {var g=UnityEngine.Object.Instantiate(template.gameObject,root);g.name="Expanded_Hesco";g.SetActive(true);g.transform.position=p;g.transform.rotation=template.rotation*Quaternion.Euler(0,yaw,0);}
    static void Tank(Vector3 p,float yaw)
    {
        string path=AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("DesertTank t:Prefab").First());var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
        var g=(GameObject)PrefabUtility.InstantiatePrefab(prefab,root);g.name="Base_DesertTank";g.transform.localScale=Vector3.one*1.6f;g.transform.rotation=Quaternion.Euler(0,yaw,0);g.transform.position=p;
        g.transform.position+=Vector3.up*(p.y-g.GetComponentsInChildren<Renderer>().Min(r=>r.bounds.min.y));
        foreach(var t in g.GetComponentsInChildren<Transform>())t.gameObject.layer=2;
        foreach(var c in g.GetComponentsInChildren<Collider>())UnityEngine.Object.DestroyImmediate(c);
    }
    static void Flatten(Terrain terrain,Vector2 center,Vector2 half,float height,float blend)
    {
        var d=terrain.terrainData;int n=d.heightmapResolution;var h=d.GetHeights(0,0,n,n);var o=terrain.transform.position;
        for(int z=0;z<n;z++)for(int x=0;x<n;x++)
        {
            float wx=o.x+x*d.size.x/(n-1),wz=o.z+z*d.size.z/(n-1);
            float outside=Mathf.Max(Mathf.Abs(wx-center.x)-half.x,Mathf.Abs(wz-center.y)-half.y);if(outside>=blend)continue;
            float weight=1-Mathf.SmoothStep(0,1,Mathf.Clamp01(outside/blend));h[z,x]=Mathf.Lerp(h[z,x],(height-o.y)/d.size.y,weight);
        }
        d.SetHeights(0,0,h);
    }
}
