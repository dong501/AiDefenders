using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;

public static class CityGuideSignsUpgrade
{
    const string City="Assets/Scenes/GameStages/City/City_1.unity";
    static Material green,white,steel,blue;
    static TMP_FontAsset font;
    [MenuItem("Tools/AI Defenders/City/Guide Signs And Blinking Amber Signals")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play mode first.");
        var scene=SceneManager.GetSceneByPath(City);bool opened=!scene.isLoaded;
        if(!opened&&scene.isDirty)throw new InvalidOperationException("Unsaved city edits.");
        Directory.CreateDirectory("Docs/CityGuideSigns/Before");string backup="Docs/CityGuideSigns/Before/City_1.unity";if(!File.Exists(backup))File.Copy(City,backup);
        if(opened)scene=EditorSceneManager.OpenScene(City,OpenSceneMode.Additive);
        var original=SceneManager.GetActiveScene();SceneManager.SetActiveScene(scene);
        try
        {
            string signature=EnvironmentExpansionBuilder.GameplaySignature(scene);
            var old=scene.GetRootGameObjects().FirstOrDefault(g=>g.name=="City_Green_Guide_Signs");if(old)UnityEngine.Object.DestroyImmediate(old);
            var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();
            font=all.Select(t=>t.GetComponent<TMP_Text>()).Where(t=>t&&t.font).Select(t=>t.font).FirstOrDefault();
            if(!font)throw new InvalidOperationException("Korean sign font missing.");
            green=StageDetailPolish.Mat("Guide_Sign_Green","#126C51");white=StageDetailPolish.Mat("Guide_Sign_White","#F1F3E9");steel=StageDetailPolish.Mat("Guide_Sign_Steel","#748083");blue=StageDetailPolish.Mat("Guide_Route_Blue","#1C508A");
            var root=new GameObject("City_Green_Guide_Signs").transform;
            // Ordinary streets, ahead of intersections rather than around the shelter.
            Sign(root,new Vector3(-100,.34f,-145),0,"영등포역 · 여의도","영등포시장 · 신길동","영등포로 / Yeongdeungpo-ro","서울");
            Sign(root,new Vector3(100,.34f,-55),180,"국회의사당 · 여의도","당산역 · 양평동","국회대로 / Gukhoe-daero","서울");
            Sign(root,new Vector3(-155,.34f,-180),90,"선유도공원 · 당산역","양평동 · 양화대교","양평로 / Yangpyeong-ro","서울");
            Sign(root,new Vector3(155,.34f,-20),-90,"여의도 · 마포대교","여의도공원 · 서울교","여의대로 / Yeoui-daero","서울");
            Sign(root,new Vector3(-55,.34f,-20),90,"신길동 · 대방역","여의도 · 신길역","여의대방로 / Yeouidaebang-ro","서울");
            Sign(root,new Vector3(55,.34f,-180),-90,"양평동 · 선유도공원","영등포구청 · 문래동","선유로 / Seonyu-ro","서울");
            var traffic=all.First(t=>t.name=="City_Traffic_Details");
            foreach(var label in traffic.GetComponentsInChildren<TMP_Text>(true).Where(t=>t.name=="Road_Name"))
            {
                var p=label.transform.position;
                label.text=p.x<-50?"양평로 / Yangpyeong-ro":p.x>50?"여의대로 / Yeoui-daero":p.z<-100?"영등포로 / Yeongdeungpo-ro":"국회대로 / Gukhoe-daero";
                label.enableAutoSizing=true;label.fontSizeMin=1.5f;label.fontSizeMax=3f;EditorUtility.SetDirty(label);
            }
            var controller=traffic.GetComponent<CityTrafficAmberBlink>();if(!controller)controller=traffic.gameObject.AddComponent<CityTrafficAmberBlink>();
            var amber=StageDetailPolish.Mat("Signal_Amber_Blink","#FFAC13",true);var darkGreen=StageDetailPolish.Mat("Signal_Green_Off","#14392F");
            var bindings=new List<CityTrafficAmberBlink.Signal>();
            foreach(var renderer in traffic.GetComponentsInChildren<MeshRenderer>())
            {
                var materials=renderer.sharedMaterials;
                for(int i=0;i<materials.Length;i++)
                {
                    if(materials[i].name.StartsWith("Signal_Amber")){materials[i]=amber;bindings.Add(new CityTrafficAmberBlink.Signal{renderer=renderer,materialIndex=i});}
                    else if(materials[i].name.StartsWith("Signal_Green"))materials[i]=darkGreen;
                }
                renderer.sharedMaterials=materials;
                if(renderer.name.StartsWith("Junction_"))RemoveGreenArrows(renderer);
            }
            controller.signals=bindings.ToArray();controller.onSeconds=.5f;controller.offSeconds=.5f;EditorUtility.SetDirty(controller);
            if(bindings.Count!=14)throw new InvalidOperationException("Expected 14 merged signal junctions, got "+bindings.Count);
            if(signature!=EnvironmentExpansionBuilder.GameplaySignature(scene))throw new InvalidOperationException("Gameplay layout changed.");
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            File.WriteAllText("Docs/CityGuideSigns/Changes.txt","Six green directional signs and junction road-name plates use Yeongdeungpo destinations and real road names. Ordinary roads ahead of intersections, no shelter-front guide signs. 14 junction batches / 56 amber lamps; .5 second on / .5 second off; green lamps dark; gameplay unchanged.");
        }
        finally{SceneManager.SetActiveScene(original);if(opened)EditorSceneManager.CloseScene(scene,true);}
    }
    static void RemoveGreenArrows(MeshRenderer renderer)
    {
        var markers=renderer.GetComponentsInChildren<Transform>().Where(t=>t.name=="Green_Arrow").Select(t=>t.position).ToArray();if(markers.Length==0)return;
        var filter=renderer.GetComponent<MeshFilter>();var mesh=UnityEngine.Object.Instantiate(filter.sharedMesh);var vertices=mesh.vertices;
        for(int sub=0;sub<mesh.subMeshCount;sub++)
        {
            if(!renderer.sharedMaterials[sub].name.StartsWith("Reflective_White"))continue;
            var triangles=mesh.GetTriangles(sub);var kept=new List<int>();
            for(int i=0;i<triangles.Length;i+=3)
            {
                var p=renderer.transform.TransformPoint((vertices[triangles[i]]+vertices[triangles[i+1]]+vertices[triangles[i+2]])/3);
                if(markers.Any(m=>(p-m).sqrMagnitude<.2f))continue;
                kept.AddRange(new[]{triangles[i],triangles[i+1],triangles[i+2]});
            }
            mesh.SetTriangles(kept,sub);
        }
        string path="Assets/Art/StageDetails/Amber_"+renderer.name+".asset";var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(saved){EditorUtility.CopySerialized(mesh,saved);UnityEngine.Object.DestroyImmediate(mesh);EditorUtility.SetDirty(saved);}else{saved=mesh;AssetDatabase.CreateAsset(saved,path);}filter.sharedMesh=saved;
    }
    static void Box(Transform parent,string name,Vector3 position,Vector3 size,Material material)=>StageDetailPolish.Shape(parent,name,PrimitiveType.Cube,position,size,material);
    static void Sign(Transform parent,Vector3 position,float yaw,string destination,string other,string roadName,string routeNumber)
    {
        var root=new GameObject("Green_Direction_Sign").transform;root.SetParent(parent,false);root.position=position;root.rotation=Quaternion.Euler(0,yaw,0);
        StageDetailPolish.Shape(root,"Cantilever_Post",PrimitiveType.Cylinder,new Vector3(17,5,0),new Vector3(.4f,5,.4f),steel);
        Box(root,"Cantilever_Arm",new Vector3(5.5f,10,0),new Vector3(23,.3f,.35f),steel);
        Box(root,"Board_Back",new Vector3(0,9,0),new Vector3(16,5,.3f),steel);
        Box(root,"Green_Board",new Vector3(0,9,-.18f),new Vector3(15.8f,4.8f,.12f),green);
        foreach(float y in new[]{6.7f,11.3f})Box(root,"White_Border",new Vector3(0,y,-.25f),new Vector3(15.5f,.07f,.02f),white);
        foreach(float x in new[]{-7.72f,7.72f})Box(root,"White_Border",new Vector3(x,9,-.25f),new Vector3(.07f,4.65f,.02f),white);
        Label(root,"Primary_Destination",destination,new Vector3(-2.6f,10.25f,-.28f),new Vector2(9,1.1f),6);
        Label(root,"Secondary_Destination",other,new Vector3(-2.6f,8.95f,-.28f),new Vector2(9,1.1f),4.7f);
        Label(root,"Road_Name",roadName,new Vector3(-1.3f,7.4f,-.28f),new Vector2(13,1),3.1f);
        // Straight-ahead arrow on the right, visible without relying on font glyphs.
        StageDetailPolish.Line(root,"Direction_Arrow",new Vector3(5.7f,8,-.3f),new Vector3(5.7f,10.7f,-.3f),.23f,white);
        StageDetailPolish.Line(root,"Direction_Arrow",new Vector3(5.7f,10.7f,-.3f),new Vector3(4.9f,9.8f,-.3f),.23f,white);
        StageDetailPolish.Line(root,"Direction_Arrow",new Vector3(5.7f,10.7f,-.3f),new Vector3(6.5f,9.8f,-.3f),.23f,white);
        Box(root,"Route_Badge",new Vector3(-6.7f,10.35f,-.3f),new Vector3(1.25f,.85f,.05f),blue);
        Label(root,"Route_Number",routeNumber,new Vector3(-6.7f,10.35f,-.35f),new Vector2(1.2f,.8f),3.2f);
        root.localScale=Vector3.one*.78f;
        StageDetailPolish.Combine(root,"City_Green_Guide_"+Mathf.RoundToInt(position.x)+"_"+Mathf.RoundToInt(position.z));
    }
    static void Label(Transform parent,string name,string text,Vector3 position,Vector2 size,float fontSize)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshPro));go.transform.SetParent(parent,false);go.transform.localPosition=position;go.layer=2;
        var label=go.GetComponent<TextMeshPro>();label.font=font;label.text=text;label.fontSize=fontSize;label.alignment=TextAlignmentOptions.Center;label.color=Color.white;label.rectTransform.sizeDelta=size;
    }
}
