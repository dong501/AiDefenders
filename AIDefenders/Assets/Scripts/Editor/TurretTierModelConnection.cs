using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

// Imports the supplied mesh data only; scripts inside the archive are not executed.
public static class TurretTierModelConnection
{
    const string Source="Docs/TurretTierConnection/Sources/";
    const string Art="Assets/Art/TurretUpgradeModels";
    const string Prefabs="Assets/Turrets/Turret_Test/";
    public static readonly string[] Types={"MachineGun","Precision","Missile","Laser","Plasma"};
    public static readonly string[] Names={"기관총 포탑","정밀 사격 포탑","미사일 포탑","레이저 포탑","플라즈마 포탑"};
    public static readonly string[] Files={"Turret_MG","Turret_PrecisionShot","Turret_Test Missile","Turret_Test Laser","Turret_Test_Bolt"};
    [Serializable] class Part { public string name,group,color;public bool emissive;public Vector3[] vertices;public int[] triangles; }
    [Serializable] class Model { public Part[] parts; }

    [MenuItem("Tools/AI Defenders/Turrets/Connect Uploaded Fifteen Upgrade Models")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play mode first.");
        Directory.CreateDirectory("Docs/TurretTierConnection/Before");
        Directory.CreateDirectory(Art);AssetDatabase.Refresh();
        for(int type=0;type<Types.Length;type++)Connect(type,Files[type]);
        // Legacy machine-gun references exist in the shared game-manager prefabs.
        Connect(0,"Turret_Test MG");
        foreach(var path in new[]{"Assets/Waves/GameManager.prefab","Assets/Scenes/GameStages/GameManager.prefab"})
        {
            Backup(path);var root=PrefabUtility.LoadPrefabContents(path);
            try{foreach(var placer in root.GetComponentsInChildren<TurretPlatformPlacer>(true))Catalog(placer);PrefabUtility.SaveAsPrefabAsset(root,path);}
            finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        foreach(var path in new[]{"Assets/Scenes/TurretLab/TurretLab.unity","Assets/Scenes/GameStages/GrassField/Grassfield_1.unity","Assets/Scenes/GameStages/GrassField/Grassfield_2.unity","Assets/Scenes/GameStages/GrassField/Grassfield_3.unity","Assets/Scenes/GameStages/Desert/Desert_1.unity","Assets/Scenes/GameStages/City/City_1.unity"})
        {
            var scene=SceneManager.GetSceneByPath(path);bool opened=!scene.isLoaded;
            if(!opened&&scene.isDirty)throw new InvalidOperationException("Unsaved scene: "+path);
            Backup(path);if(opened)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
            try
            {
                foreach(var placer in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<TurretPlatformPlacer>(true)))
                {Catalog(placer);EditorUtility.SetDirty(placer);PrefabUtility.RecordPrefabInstancePropertyModifications(placer);}
                foreach(var lab in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<MenuManager_TurretLab>(true)))
                {
                    var entries=(lab.turretCatalog??new TurretLabEntry[0]).ToList();
                    for(int i=0;i<Types.Length;i++)if(!entries.Any(e=>e.prefab==Prefab(i)))entries.Add(new TurretLabEntry{displayName=Names[i],prefab=Prefab(i),researches=new TurretLabResearch[0]});
                    lab.turretCatalog=entries.ToArray();EditorUtility.SetDirty(lab);
                }
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            finally{if(opened)EditorSceneManager.CloseScene(scene,true);}
        }
        AssetDatabase.SaveAssets();
    }
    public static GameObject Prefab(int type)=>AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs+Files[type]+".prefab");
    public static void UpdatePaintableModels()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play mode first.");
        for(int type=0;type<Types.Length;type++)Connect(type,Files[type]);
        Connect(0,"Turret_Test MG");AssetDatabase.SaveAssets();
    }
    static bool Paintable(Part part)
    {
        if(part.emissive)return false;
        string n=part.name;
        return new[]{"Foundation","SquareFoundation","UpperPlinth","UpperArmor","Housing","FrontRim","TopRim","BaseFoot","BaseShoulder","BaseTopBevel","ArmorTop","ArmorBody","ArmorBevel","FaceGasket","SideArmor","LauncherHousing","RearArmor","BlueFrontRim","TopArmor","Chamber","RearCasing","RearReinforcement","RearCap","FootArmor","FootTop","SquareTop","UpperDeck","InnerDeck","RaisedInnerDeck","WideReactorHull","ReactorRearArmor"}
            .Any(prefix=>n.StartsWith(prefix,StringComparison.Ordinal)) && n.IndexOf("Gold",StringComparison.OrdinalIgnoreCase)<0;
    }
    static void Backup(string path)
    {string dest="Docs/TurretTierConnection/Before/"+path.Replace('/','_');if(!File.Exists(dest))File.Copy(path,dest);}
    static void Catalog(TurretPlatformPlacer placer)
    {
        var options=(placer.turretOptions??new TurretBuildOption[0]).ToList();
        for(int i=0;i<Types.Length;i++)
        {
            var prefab=Prefab(i);var option=options.FirstOrDefault(o=>o.prefab==prefab||(i==0&&o.prefab!=null&&o.prefab.name=="Turret_Test MG"));
            if(option==null){option=new TurretBuildOption{prefab=prefab,cost=i==1?60:50,sellPrice=i==1?30:25,buildTime=3};options.Add(option);}
            option.displayName=Names[i];
        }
        placer.turretOptions=options.ToArray();
    }
    static void Connect(int type,string file)
    {
        string path=Prefabs+file+".prefab";Backup(path);var root=PrefabUtility.LoadPrefabContents(path);
        try
        {
            var turret=root.GetComponent<TurretController>();
            if(turret.upgradeTiers==null||turret.upgradeTiers.Length!=3)throw new InvalidOperationException(path+" needs three existing stat tiers.");
            var old=turret.upgradeTiers.Select(t=>t.visualRoot).Where(g=>g!=null&&g!=root).Distinct().ToArray();
            foreach(var g in old)UnityEngine.Object.DestroyImmediate(g);
            for(int level=1;level<=3;level++)
            {
                Transform pitch,fire;var visual=BuildModel(Types[type],level,root.transform,out pitch,out fire);
                var tier=turret.upgradeTiers[level-1];tier.visualRoot=visual;tier.barrel=pitch;tier.firePoint=fire;tier.label=level+"단계";
                visual.SetActive(level==1);
            }
            turret.firePoint=turret.upgradeTiers[0].firePoint;turret.upgradeLevel=1;turret.displayName=Names[type];
            PrefabUtility.SaveAsPrefabAsset(root,path);
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}
    }
    static Transform Child(string name,Transform parent,Vector3 position)
    {var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=position;return t;}
    static Vector3 GroupOffset(string type,int level,string group)
    {
        if(group=="ChargeOrb")return new Vector3(0,0,level==1?.94f:level==2?1.17f:1.25f);
        if(type=="Missile"&&group.StartsWith("Missile_"))
        {
            bool left=group.StartsWith("Missile_L");if(level==2)return new Vector3(left?-.44f:.44f,0,0);
            int index=int.Parse(group.Substring(group.Length-1))-1;
            return new Vector3((left?-.84f:.84f)+(index%2==0?-.205f:.205f),index<2?.19f:-.19f,0);
        }
        return Vector3.zero;
    }
    static Vector3 Muzzle(string type,int level)
    {
        if(type=="Precision")return new Vector3(0,0,level==1?1.91f:level==2?2.48f:3.24f);
        if(type=="Laser")return new Vector3(0,0,1.60f);
        if(type=="Plasma")return new Vector3(0,0,level==1?1.25f:level==2?1.64f:1.89f);
        if(type=="Missile")return level==1?new Vector3(0,0,.96f):level==2?new Vector3(-.44f,0,1):new Vector3(-1.045f,.19f,.84f);
        return level==1?new Vector3(-.15f,.11f,1.61f):level==2?new Vector3(-.62f,.11f,1.61f):new Vector3(-.72f,-.08f,1.67f);
    }
    static GameObject BuildModel(string type,int level,Transform parent,out Transform pitch,out Transform fire)
    {
        var model=JsonUtility.FromJson<Model>(File.ReadAllText(Source+type+level+".json"));
        var root=Child(type+"_Level"+level+"_Skin",parent,Vector3.zero);
        var baseRoot=Child("Base",root,Vector3.zero);var yaw=Child("YawPivot",root,Vector3.zero);
        pitch=Child("PitchPivot",yaw,new Vector3(0,type=="Precision"?1.40f:type=="Plasma"?(level==3?1.65f:1.53f):1.55f,0));
        var groups=new Dictionary<string,Transform>{{"Base",baseRoot},{"Yaw",yaw},{"Pitch",pitch}};
        foreach(var group in model.parts.Select(p=>p.group).Distinct())if(!groups.ContainsKey(group))groups[group]=Child(group,pitch,GroupOffset(type,level,group));
        // Merge by articulation group and material to avoid hundreds of renderers.
        foreach(var group in model.parts.GroupBy(p=>p.group))
        {
            var vertices=new List<Vector3>();var submeshes=new List<int[]>();var materials=new List<Material>();var paintSlots=new List<int>();
            foreach(var material in group.GroupBy(p=>p.color+"_"+p.emissive+"_"+Paintable(p)))
            {
                var triangles=new List<int>();
                foreach(var part in material)
                {
                    if(part.triangles.Any(i=>i<0||i>=part.vertices.Length))throw new InvalidDataException(part.name);
                    int offset=vertices.Count;vertices.AddRange(part.vertices);triangles.AddRange(part.triangles.Select(i=>i+offset));
                }
                if(Paintable(material.First()))paintSlots.Add(materials.Count);
                submeshes.Add(triangles.ToArray());materials.Add(MaterialFor(material.First()));
            }
            var mesh=new Mesh{name=type+level+"_"+group.Key,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(vertices);mesh.subMeshCount=submeshes.Count;
            for(int i=0;i<submeshes.Count;i++)mesh.SetTriangles(submeshes[i],i);mesh.RecalculateNormals();mesh.RecalculateBounds();
            string path=Art+"/"+mesh.name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(existing){EditorUtility.CopySerialized(mesh,existing);UnityEngine.Object.DestroyImmediate(mesh);mesh=existing;EditorUtility.SetDirty(mesh);}else AssetDatabase.CreateAsset(mesh,path);
            var target=groups[group.Key];target.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;target.gameObject.AddComponent<MeshRenderer>().sharedMaterials=materials.ToArray();
            target.gameObject.AddComponent<TurretPaintSlots>().materialSlots=paintSlots.ToArray();
        }
        fire=Child("FirePoint",pitch,Muzzle(type,level));
        var baseCollider=baseRoot.gameObject.AddComponent<BoxCollider>();baseCollider.center=new Vector3(0,.25f,0);baseCollider.size=new Vector3(2.15f,.5f,2.15f);
        var head=pitch.gameObject.AddComponent<BoxCollider>();head.center=new Vector3(0,0,-.1f);head.size=new Vector3(type=="Missile"&&level==3?2.7f:1.4f,1.1f,1.2f);
        foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=parent.gameObject.layer;
        return root.gameObject;
    }
    static Material MaterialFor(Part part)
    {
        string key=part.color.TrimStart('#')+(part.emissive?"_Glow":"");string path=Art+"/Material_"+key+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(mat)return mat;
        mat=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=key};Color color;ColorUtility.TryParseHtmlString(part.color,out color);mat.color=color;
        mat.SetFloat("_Metallic",.12f);mat.SetFloat("_Smoothness",.28f);
        if(part.emissive){mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",color.linear*1.6f);}
        AssetDatabase.CreateAsset(mat,path);return mat;
    }
}
