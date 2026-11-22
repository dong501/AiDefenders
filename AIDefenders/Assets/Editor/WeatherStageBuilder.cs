using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class WeatherStageBuilder
{
    const string Art = "Assets/Art/StageAtmospheres";
    const string Previews = "Assets/Resources/UI/Previews/";
    public static void Build()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop Play mode before building");
        var setup=EditorSceneManager.GetSceneManagerSetup();
        foreach(var item in setup)if(item.isLoaded && SceneManager.GetSceneByPath(item.path).isDirty)throw new Exception("Unsaved scene: "+item.path);
        Directory.CreateDirectory(Art);AssetDatabase.Refresh();
        try
        {
            Variant("Desert",2,StageWeather.SandstormSunset);
            Variant("Desert",3,StageWeather.DesertMoonlight);
            Variant("City",2,StageWeather.RainyEvening);
            Variant("City",3,StageWeather.FoggyDawn);
            CaptureOriginal("Desert");CaptureOriginal("City");
            var list=new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach(string chapter in new[]{"Desert","City"})for(int i=1;i<=3;i++)
            {
                string path="Assets/Scenes/GameStages/"+chapter+"/"+chapter+"_"+i+".unity";
                if(!list.Any(s=>s.path==path))list.Add(new EditorBuildSettingsScene(path,true));
                else list.First(s=>s.path==path).enabled=true;
            }
            EditorBuildSettings.scenes=list.ToArray();AssetDatabase.SaveAssets();AssetDatabase.Refresh();
            Debug.Log("WEATHER_STAGES_BUILT: four stage variants, six current-map previews, build entries");
        }
        finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
    }

    static void Variant(string chapter,int index,StageWeather profile)
    {
        string source="Assets/Scenes/GameStages/"+chapter+"/"+chapter+"_1.unity";
        string path=source.Replace("_1.unity","_"+index+".unity");
        if(!File.Exists(path) && !AssetDatabase.CopyAsset(source,path))throw new Exception("Copy failed: "+path);
        var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
        string stagePath="Assets/Waves/StageData_"+chapter+"_"+index+".asset";
        if(!File.Exists(stagePath))AssetDatabase.CopyAsset("Assets/Waves/StageData_"+chapter+"_1.asset",stagePath);
        var director=UnityEngine.Object.FindFirstObjectByType<WaveDirector>();
        var serialized=new SerializedObject(director);
        var stageProperty=serialized.FindProperty("stageData");
        if(stageProperty==null)stageProperty=serialized.FindProperty("stage");
        if(stageProperty==null)throw new Exception("Missing stage data property");
        stageProperty.objectReferenceValue=AssetDatabase.LoadAssetAtPath<StageData>(stagePath);serialized.ApplyModifiedPropertiesWithoutUndo();
        PrefabUtility.RecordPrefabInstancePropertyModifications(director);
        var old=UnityEngine.Object.FindFirstObjectByType<StageAtmosphere>();
        var atmosphere=old!=null?old:new GameObject("StageAtmosphere").AddComponent<StageAtmosphere>();
        atmosphere.weather=profile;
        string materialPath=Art+"/"+chapter+"_"+index+"_Sky.mat";
        var sky=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if(sky==null){sky=new Material(Shader.Find("AIDefenders/AtmosphereSky"));AssetDatabase.CreateAsset(sky,materialPath);}
        sky.SetColor("_Top",atmosphere.SkyTop);sky.SetColor("_Horizon",atmosphere.SkyHorizon);
        sky.SetFloat("_Stars",profile==StageWeather.DesertMoonlight?1:0);sky.SetFloat("_Moon",profile==StageWeather.DesertMoonlight?1:0);
        EditorUtility.SetDirty(sky);atmosphere.skyMaterial=sky;atmosphere.ApplyEnvironment();
        AddPracticalLights(profile);
        if(profile==StageWeather.RainyEvening)WetRoads();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        Capture(chapter,index,atmosphere);
    }

    static void AddPracticalLights(StageWeather profile)
    {
        if(GameObject.Find("AtmosphereBaseLights")!=null)return;
        var goal=UnityEngine.Object.FindFirstObjectByType<TargetGoal>();
        var root=new GameObject("AtmosphereBaseLights");root.transform.position=goal.transform.position;
        Material pole=AssetDatabase.LoadAssetAtPath<Material>(Art+"/LampPole.mat");
        if(pole==null){pole=new Material(Shader.Find("Universal Render Pipeline/Lit"));pole.color=new Color(.08f,.11f,.15f);AssetDatabase.CreateAsset(pole,Art+"/LampPole.mat");}
        Material bulb=AssetDatabase.LoadAssetAtPath<Material>(Art+"/LampGlow.mat");
        if(bulb==null){bulb=new Material(Shader.Find("Universal Render Pipeline/Lit"));bulb.color=new Color(1,.83f,.48f);bulb.EnableKeyword("_EMISSION");bulb.SetColor("_EmissionColor",new Color(1,.64f,.22f)*3);AssetDatabase.CreateAsset(bulb,Art+"/LampGlow.mat");}
        foreach(var offset in new[]{new Vector3(-25,0,-20),new Vector3(25,0,-20),new Vector3(-25,0,20),new Vector3(25,0,20)})
        {
            Vector3 grounded=offset;RaycastHit hit;int mask=LayerMask.GetMask("Ground");
            if(Physics.Raycast(root.transform.position+offset+Vector3.up*100,Vector3.down,out hit,200,mask))grounded.y=hit.point.y-root.transform.position.y;
            else grounded.y=-1.2f;
            var post=GameObject.CreatePrimitive(PrimitiveType.Cylinder);post.name="BaseLampPost";post.transform.SetParent(root.transform,false);post.transform.localPosition=grounded+Vector3.up*2;post.transform.localScale=new Vector3(.14f,2,.14f);UnityEngine.Object.DestroyImmediate(post.GetComponent<Collider>());post.GetComponent<Renderer>().sharedMaterial=pole;
            var glow=GameObject.CreatePrimitive(PrimitiveType.Sphere);glow.name="BaseLamp";glow.transform.SetParent(root.transform,false);glow.transform.localPosition=grounded+Vector3.up*4;glow.transform.localScale=Vector3.one*.5f;UnityEngine.Object.DestroyImmediate(glow.GetComponent<Collider>());glow.GetComponent<Renderer>().sharedMaterial=bulb;
            var light=glow.AddComponent<Light>();light.type=LightType.Point;light.range=19;light.intensity=profile==StageWeather.DesertMoonlight?5:3;light.color=new Color(1,.72f,.4f);light.shadows=LightShadows.None;
        }
    }

    static void WetRoads()
    {
        var copies=new Dictionary<Material,Material>();
        foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
        {
            if(!r.name.ToLowerInvariant().Contains("road") && !r.name.ToLowerInvariant().Contains("asphalt"))continue;
            var materials=r.sharedMaterials;bool changed=false;
            for(int i=0;i<materials.Length;i++)
            {
                var original=materials[i];if(original==null||!original.HasProperty("_Smoothness"))continue;
                if(!copies.TryGetValue(original,out var wet))
                {
                    string path=Art+"/Wet_"+original.name.Replace("/","_")+".mat";
                    wet=AssetDatabase.LoadAssetAtPath<Material>(path);
                    if(wet==null){wet=new Material(original);AssetDatabase.CreateAsset(wet,path);}
                    wet.SetFloat("_Smoothness",.72f);if(wet.HasProperty("_BaseColor"))wet.SetColor("_BaseColor",original.GetColor("_BaseColor")*.85f);
                    copies.Add(original,wet);
                }
                materials[i]=wet;changed=true;
            }
            if(changed)r.sharedMaterials=materials;
        }
    }

    static void CaptureOriginal(string chapter)
    {
        EditorSceneManager.OpenScene("Assets/Scenes/GameStages/"+chapter+"/"+chapter+"_1.unity",OpenSceneMode.Single);
        Capture(chapter,1,null);
    }

    static void Capture(string chapter,int index,StageAtmosphere atmosphere)
    {
        var goal=UnityEngine.Object.FindFirstObjectByType<TargetGoal>();
        var root=new GameObject("PreviewCaptureCamera");var camera=root.AddComponent<Camera>();
        camera.fieldOfView=55;camera.nearClipPlane=.3f;camera.farClipPlane=1400;camera.clearFlags=CameraClearFlags.Skybox;
        Vector3 focus=goal.transform.position+(chapter=="Desert"?new Vector3(0,0,-35):new Vector3(0,4,0));
        root.transform.position=goal.transform.position+(chapter=="Desert"?new Vector3(85,72,100):new Vector3(85,125,115));
        root.transform.LookAt(focus);
        // Hide UI and cameras from scene previews. All environment is the actual saved map.
        var canvases=UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach(var c in canvases)c.enabled=false;
        if(atmosphere!=null)
        {
            atmosphere.CreateWeatherEffects();
            var effects=atmosphere.transform.Find("WeatherParticles");if(effects!=null)effects.position=focus;
            foreach(var ps in atmosphere.GetComponentsInChildren<ParticleSystem>())ps.Simulate(6,true,true,true);
        }
        var rt=RenderTexture.GetTemporary(1280,720,24,RenderTextureFormat.ARGB32);
        var previous=RenderTexture.active;
        Texture2D image=null;
        try
        {
            camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
            image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();
            string path=Previews+chapter+"_"+index+".png";File.WriteAllBytes(path,image.EncodeToPNG());
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Default;importer.mipmapEnabled=false;importer.maxTextureSize=2048;importer.SaveAndReimport();
        }
        finally
        {
            camera.targetTexture=null;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);
            if(image!=null)UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(root);
            foreach(var c in canvases)if(c!=null)c.enabled=true;
        }
    }
}
