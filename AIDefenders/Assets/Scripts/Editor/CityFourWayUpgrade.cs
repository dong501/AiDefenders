using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class CityFourWayUpgrade
{
    const string ScenePath = "Assets/Scenes/GameStages/City/City_1.unity";
    const string Folder = "Assets/Art/EnvironmentExpansion/Generated/";
    const float CenterZ = -100f;
    static readonly string[] Directions = { "West", "East", "North", "South" };
    static readonly Vector3[] Axes = { Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
    static readonly Vector3 Center = new Vector3(0, .35f, CenterZ);
    static Transform decor;

    [MenuItem("Tools/AI Defenders/Environment Expansion/3 Upgrade City Four Way Defense")]
    public static void Apply()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode first.");
        var scene = SceneManager.GetSceneByPath(ScenePath);
        bool opened = !scene.isLoaded;
        if (!opened && scene.isDirty) throw new InvalidOperationException("Save City scene edits first.");
        if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
        var original = SceneManager.GetActiveScene();
        SceneManager.SetActiveScene(scene);
        try
        {
            Directory.CreateDirectory("Docs/CityFourWay/Before");
            if (!File.Exists("Docs/CityFourWay/Before/City_1.unity")) File.Copy(ScenePath, "Docs/CityFourWay/Before/City_1.unity");
            var roots = scene.GetRootGameObjects();
            var all = roots.SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
            var goal = roots.First(r => r.name == "Target Goal").transform;
            goal.position = new Vector3(0, 1.22f, CenterZ);
            var trigger = goal.GetComponent<BoxCollider>();
            trigger.size = new Vector3(14, 8, 14); trigger.center = new Vector3(0, 3, 0);

            var shelter = all.First(t => t.name == "Evacuation_Shelter");
            shelter.position = new Vector3(0, 0, CenterZ - 58.9f);
            // Replace the one-entrance perimeter with four open gates.
            var oldPerimeter = shelter.Find("Shelter_Perimeter");
            if (oldPerimeter) oldPerimeter.gameObject.SetActive(false);
            var oldSign = shelter.Find("Shelter_Gate_Sign");
            if (oldSign) oldSign.gameObject.SetActive(false);
            OpenShelterCorridors(shelter.GetComponent<MeshFilter>());

            var director = all.SelectMany(t => t.GetComponents<WaveDirector>()).First();
            var previousSpawners = all.SelectMany(t => t.GetComponents<EnemySpawner>()).ToArray();
            var spawners = new EnemySpawner[4];
            for (int i = 0; i < 4; i++)
            {
                var existing = previousSpawners.FirstOrDefault(sp => sp.spawnerId == "City_" + Directions[i]);
                var spawner = existing ? existing : (i < previousSpawners.Length ? previousSpawners[i] : new GameObject().AddComponent<EnemySpawner>());
                spawner.name = "Enemy spawner City " + Directions[i];
                spawner.spawnerId = "City_" + Directions[i];
                spawner.transform.position = Center + Axes[i] * 135f;
                spawner.spawnRange = 3; spawner.destination = goal; spawner.destinationRadius = 1.5f;
                foreach (var renderer in spawner.GetComponents<Renderer>()) renderer.enabled = false;
                foreach (Transform child in spawner.transform.Cast<Transform>().ToArray()) UnityEngine.Object.DestroyImmediate(child.gameObject);
                var pathObject = new GameObject("Path_City_" + Directions[i]);
                pathObject.transform.SetParent(spawner.transform, false);
                var path = pathObject.AddComponent<EnemyPath>(); path.waypointRadius = 1.5f;
                path.gizmoColor = new[] { Color.cyan, Color.yellow, Color.green, new Color(1,.5f,.2f) }[i];
                var waypoints = new List<Transform>();
                foreach (float distance in new[] { 125f, 90f, 55f, 30f, 12f, 0f })
                {
                    var point = new GameObject("Waypoint_" + waypoints.Count).transform;
                    point.SetParent(path.transform); point.position = Center + Axes[i] * distance;
                    waypoints.Add(point);
                }
                path.waypoints = waypoints.ToArray(); spawner.paths = new[] { path }; spawners[i] = spawner;
            }
            director.spawner = spawners[0]; director.spawners = spawners;
            UpdateWaves(director.stage);
            all = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
            director.preparationCamera = all.First(t => t.name == "Camera_GamePreparation").GetComponent<Camera>();
            director.waveCamera = all.First(t => t.name == "Camera_GameOnWave").GetComponent<Camera>();
            director.preparationCamera.transform.position = new Vector3(0, 155, CenterZ);
            director.preparationCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
            var drone = roots.First(r => r.name == "CCTVSystem");
            drone.transform.position = new Vector3(0, 30, CenterZ + 30);
            // The cameras sit in front of the drone along its local +Z axis.
            // Rotate the rig, keeping cameras aligned, so its body cannot block the view.
            drone.transform.rotation = Quaternion.Euler(40, 180, 0);
            foreach (var camera in drone.GetComponentsInChildren<Camera>(true)) camera.transform.localRotation = Quaternion.identity;
            var limit = all.First(t => t.name.StartsWith("CCTVMoveLimit")).GetComponent<BoxCollider>();
            limit.transform.position = new Vector3(0, 45, CenterZ); limit.center = Vector3.zero; limit.size = new Vector3(340, 110, 340);

            EnvironmentExpansionBuilder.RefreshCityDecor(scene);
            var oldDecor = scene.GetRootGameObjects().FirstOrDefault(r => r.name == "City_FourWay_Streetscape");
            if (oldDecor) UnityEngine.Object.DestroyImmediate(oldDecor);
            decor = new GameObject("City_FourWay_Streetscape").transform;
            BuildShelter(); BuildVehicles(); BuildStreetProps();
            CityGreeneryUpgrade.ApplyToScene(scene);
            if (oldSign)
            {
                for (int i=0;i<4;i++)
                {
                    var sign = UnityEngine.Object.Instantiate(oldSign.gameObject, decor);
                    sign.name = "Shelter_Entrance_Sign_" + Directions[i]; sign.SetActive(true);
                    sign.layer = 2;
                    var axis = Axes[i]; float distance=i==2?37:(i==3?20:25);
                    sign.transform.position=Center+axis*distance+Vector3.up*5.65f;
                    sign.transform.rotation=Quaternion.LookRotation(axis);
                }
            }
            CityShelterVisualUpgrade.ApplyToScene(scene);
            foreach (var component in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Component>(true)))
                if (component && PrefabUtility.IsPartOfPrefabInstance(component))
                    PrefabUtility.RecordPrefabInstancePropertyModifications(component);
            EditorUtility.SetDirty(director);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("City four-way defense upgraded: 4 routes, balanced wave totals, vehicles and street props.");
        }
        finally { SceneManager.SetActiveScene(original); if (opened) EditorSceneManager.CloseScene(scene, true); }
    }

    static void UpdateWaves(StageData stage)
    {
        string path = AssetDatabase.GetAssetPath(stage);
        string backup = "Docs/CityFourWay/Before/StageData_City_1.asset";
        if (!File.Exists(backup)) File.Copy(path, backup);
        // Reapplying the editor menu must not multiply or redistribute existing groups.
        if (stage.waves.SelectMany(w => w.groups).All(g => Directions.Any(d => g.spawnerId == "City_" + d))) return;
        for (int waveIndex = 0; waveIndex < stage.waves.Length; waveIndex++)
        {
            var wave = stage.waves[waveIndex]; var groups = new List<SpawnGroup>(); int cursor = waveIndex % 4;
            int before = wave.groups.Sum(g => g.targets.Sum(e => e.count));
            foreach (var group in wave.groups)
            {
                var buckets = new List<SpawnTargetEntry>[4]; for (int i=0;i<4;i++) buckets[i]=new List<SpawnTargetEntry>();
                foreach (var entry in group.targets)
                {
                    var counts = new int[4]; for (int i=0;i<entry.count;i++) counts[(cursor++)%4]++;
                    for(int i=0;i<4;i++) if(counts[i]>0) buckets[i].Add(new SpawnTargetEntry {prefab=entry.prefab,count=counts[i]});
                }
                for(int i=0;i<4;i++) if(buckets[i].Count>0) groups.Add(new SpawnGroup {spawnTime=group.spawnTime,spawnerId="City_"+Directions[i],targets=buckets[i].ToArray()});
            }
            wave.groups=groups.ToArray();
            if (before != wave.groups.Sum(g=>g.targets.Sum(e=>e.count))) throw new InvalidOperationException("Wave total changed");
        }
        EditorUtility.SetDirty(stage);
    }

    static void OpenShelterCorridors(MeshFilter filter)
    {
        const string path = Folder + "CityShelterFourWay.asset";
        var source = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Scenes/GameStages/City/City_EvacuationShelter.asset");
        var mesh = UnityEngine.Object.Instantiate(source); mesh.name = "CityShelterFourWay";
        var vertices = mesh.vertices;
        for(int sub=0;sub<mesh.subMeshCount;sub++)
        {
            var indices=mesh.GetTriangles(sub); var keep=new List<int>();
            for(int i=0;i<indices.Length;i+=3)
            {
                var center=(vertices[indices[i]]+vertices[indices[i+1]]+vertices[indices[i+2]])/3f;
                bool lane = (Mathf.Abs(center.z-58.9f)<5.5f && Mathf.Abs(center.x)>8 && center.y<2.5f)
                    || (Mathf.Abs(center.x)<3 && center.z>65 && center.y<2.5f);
                if(!lane) {keep.Add(indices[i]);keep.Add(indices[i+1]);keep.Add(indices[i+2]);}
            }
            mesh.SetTriangles(keep,sub);
        }
        var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(saved) {EditorUtility.CopySerialized(mesh,saved);UnityEngine.Object.DestroyImmediate(mesh);} else {AssetDatabase.CreateAsset(mesh,path);saved=mesh;}
        filter.sharedMesh=saved; EditorUtility.SetDirty(saved);
    }

    static GameObject Prop(string name,float x,float z,float scale=1,float yaw=0)
    {
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+name+".prefab");
        if(!prefab) throw new InvalidOperationException("Missing prefab: "+name);
        var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,decor);
        var mesh=prefab.GetComponent<MeshFilter>().sharedMesh;
        go.transform.position=new Vector3(x,.34f-mesh.bounds.min.y*scale,z);
        go.transform.localScale=Vector3.one*scale;go.transform.rotation=Quaternion.Euler(0,yaw,0);return go;
    }
    static Material Material(string name, Color color)
    {
        string path=Folder+"City_"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!mat) {mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}
        mat.color=color;mat.SetColor("_BaseColor",color);mat.SetFloat("_Smoothness",.1f);EditorUtility.SetDirty(mat);return mat;
    }
    static void Box(string name, Vector3 position, Vector3 size, Material material)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(decor);
        go.transform.position=position;go.transform.localScale=size;go.layer=2;
        UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());go.GetComponent<Renderer>().sharedMaterial=material;
    }
    static void BuildShelter()
    {
        var concrete=Material("ShelterConcrete",new Color(.38f,.43f,.43f));
        var yellow=Material("SafetyYellow",new Color(.95f,.66f,.12f));
        var blue=Material("ShelterMarking",new Color(.18f,.52f,.67f));
        // Raised only 2cm above the road: no competing coplanar surfaces.
        Box("Shelter_Plaza",new Vector3(0,.32f,CenterZ+8),new Vector3(50,.04f,54),concrete);
        Box("Shelter_MedicalCross_H",new Vector3(0,.355f,CenterZ),new Vector3(8,.02f,1.8f),blue);
        Box("Shelter_MedicalCross_V",new Vector3(0,.356f,CenterZ),new Vector3(1.8f,.02f,8),blue);
        for(int side=0;side<4;side++)
        {
            for(int sign=-1;sign<=1;sign+=2)
            {
                Vector3 rotated, size;
                if (side % 2 == 0)
                {
                    rotated = new Vector3(sign*17,.95f,side==0?37:-20);
                    size = new Vector3(16,1.2f,1);
                }
                else
                {
                    rotated = new Vector3(side==1?25:-25,.95f,sign<0?-14:22.5f);
                    size = new Vector3(1,1.2f,sign<0?12:29);
                }
                Box("Gate_"+side+"_Barrier",new Vector3(rotated.x,rotated.y,CenterZ+rotated.z),size,concrete);
                var stripePos=new Vector3(rotated.x,1.6f,CenterZ+rotated.z);
                Box("Gate_"+side+"_YellowCap",stripePos,new Vector3(size.x+.05f,.12f,size.z+.05f),yellow);
            }
        }
    }
    static void BuildVehicles()
    {
        string[] cars={"CitySedan","CitySUV","CityMini","CityHatchback","CityTaxi","CityMinivan","CityWagon","CityCoupe","CityPickup","DeliveryVan"};
        int index=0;
        foreach(float x in new[]{-115.5f,-84.5f,84.5f,115.5f})
            foreach(float z in new[]{-214f,-174f,-134f,-54f,-14f,26f})
                Prop(cars[(index++)%cars.Length],x,z,1.7f,x<0?0:180);
        foreach(float x in new[]{-150f,-65f,65f,150f})
        {
            Prop("CityTaxi",x,-35.5f,1.7f,90);
            Prop("CityMinivan",x,-195.5f,1.7f,-90);
        }
        Prop("PoliceCar",-19,CenterZ-11,1.8f,20);
        Prop("CityMinivan",18,CenterZ+11,1.8f,180);
        Prop("CityBoxTruck",-52,-150,2,90);
        Prop("DeliveryVan",52,43,1.8f,180);
    }
    static void BuildStreetProps()
    {
        foreach(float x in new[]{-78f,-22f,22f,78f})
        {
            Prop("FireHydrant",x,-176,1.5f);
            Prop("ResidentialRecyclingBins",x,-53,1.5f);
            Prop("ResidentialMailboxBank",x,30,1.5f);
        }
        Prop("VendingMachine",-46,3,1.8f,180);Prop("ATMTerminal",-42,3,1.8f,180);
        Prop("PhoneBooth",46,3,1.8f,180);Prop("PostBox",42,3,1.5f);
        Prop("BicycleRack",-54,27,1.8f);Prop("BicycleRack",54,27,1.8f);
        Prop("ConstructionScaffold",-67,-151,2,0);
        Prop("ConstructionCementMixer",-47,-151,1.8f,45);
        Prop("ConstructionConcretePipe",-52,-167,2,90);
        Prop("ConstructionCableReel",-60,-166,1.8f,30);
        Prop("ConstructionRoadworkFence",-42,-163,2,90);
        Prop("ConstructionWorkLight",-40,-156,2);
    }
}
