#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Rebuildable, self-contained art and campaign content; no imported model dependency.</summary>
public static partial class ChapterContentBuilder
{
    const string Art = "Assets/Art/ChapterUnits";
    const string Targets = "Assets/Resources/ChapterTargets";
    const string FarmerTruckPath = "Assets/Targets/Farmer/Target_Friendly_FarmerTruck.prefab";
    static Dictionary<string, Material> mats = new Dictionary<string, Material>();
    static Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();
    static Dictionary<string, GameObject> units = new Dictionary<string, GameObject>();
    static List<Transform> limbs, wheels;
    static List<Renderer> lamps;

    [MenuItem("AI Defenders/Chapters/Build Desert and City Content")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play before building content.");
        mats.Clear(); meshes.Clear(); units.Clear();
        Folder(Art + "/Materials"); Folder(Art + "/Meshes");
        Folder(Targets + "/Desert"); Folder(Targets + "/City");
        Folder("Assets/Resources/CampaignStages"); Folder("Assets/Resources/ChapterTrainingTerrains");
        Palette();
        Human("Desert", "RescueWorker", "사막 아군 군인", false, "rescue", 260, 3.8f);
        Human("Desert", "Medic", "사막 의무병", false, "medic", 240, 4.1f);
        Human("Desert", "Raider", "사막 약탈자", true, "raider", 270, 3.5f);
        Vehicle("Desert", "SupplyTruck", "수송 트럭", false, "truck", 720, 4.4f);
        Vehicle("Desert", "RescueCrawler", "구조 장갑차", false, "crawler", 950, 3.4f);
        Vehicle("Desert", "RaiderBuggy", "사막 지프", true, "buggy", 460, 5.4f);
        Vehicle("Desert", "ArmoredBuggy", "사막 장갑차", true, "armored", 850, 4.4f);
        Vehicle("Desert", "AttackRover", "사막 전차", true, "rover", 1500, 3.5f);
        Human("City", "Survivor", "생존자", false, "survivor", 220, 3.8f);
        Human("City", "PoliceRescuer", "구조 경찰", false, "police", 320, 4.2f);
        Human("City", "Firefighter", "소방 구조대원", false, "fire", 420, 3.6f);
        Human("City", "HazmatRescuer", "방역 구조대원", false, "hazmat", 380, 3.5f);
        Human("City", "WalkerZombie", "일반 좀비", true, "walker", 300, 2.9f);
        Human("City", "RunnerZombie", "질주 좀비", true, "runner", 270, 5.6f);
        Human("City", "WorkerZombie", "작업복 좀비", true, "worker", 530, 3.3f);
        Human("City", "BruteZombie", "거대 좀비", true, "brute", 1200, 2.5f);
        Human("City", "HazmatZombie", "감염 방역 좀비", true, "infected", 720, 3.8f);
        Human("City", "ArmoredZombie", "장갑 좀비", true, "riot", 1700, 2.7f);
        Vehicle("City", "Ambulance", "구급차", false, "ambulance", 850, 4.5f);
        Vehicle("City", "EvacBus", "대피 버스", false, "bus", 1250, 3.8f);
        for (int stage = 1; stage <= 3; stage++) { Campaign("Desert", stage); Campaign("City", stage); }
        foreach (string chapter in new[] { "GrassField", "Desert", "City" }) for (int i = 1; i <= 3; i++)
        {
            string name = "StageData_" + chapter + "_" + i;
            var source = AssetDatabase.LoadAssetAtPath<StageData>("Assets/Waves/" + name + ".asset");
            if (source == null) continue;
            string path = "Assets/Resources/CampaignStages/" + name + ".asset";
            var copy = AssetDatabase.LoadAssetAtPath<StageData>(path);
            if (copy == null) { copy = ScriptableObject.CreateInstance<StageData>(); AssetDatabase.CreateAsset(copy, path); }
            EditorUtility.CopySerialized(source, copy); copy.name = name; EditorUtility.SetDirty(copy);
        }
        TrainingGround("Desert"); TrainingGround("City");
        AssetDatabase.SaveAssets();
        Debug.Log("Chapter content built: animated units, six distinct campaigns, two training environments, nine dataset stage presets.");
    }

    static void Folder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        Folder(Path.GetDirectoryName(path).Replace('\\', '/'));
        AssetDatabase.CreateFolder(Path.GetDirectoryName(path).Replace('\\', '/'), Path.GetFileName(path));
    }
    static void Palette()
    {
        ColorMat("Steel", "6E8392"); ColorMat("Dark", "202D39"); ColorMat("Rubber", "17202B");
        ColorMat("Glass", "376E83"); ColorMat("Window", "8CC3CE"); ColorMat("White", "DCE4DE");
        ColorMat("Sand", "BB9D68"); ColorMat("Olive", "667A4A"); ColorMat("Orange", "ED873D");
        ColorMat("Rescue", "309DAF"); ColorMat("Red", "B63D36"); ColorMat("Skin", "D5A078");
        ColorMat("Hair", "4E382E"); ColorMat("Navy", "294F7C"); ColorMat("Denim", "456981");
        ColorMat("Yellow", "EDC44C"); ColorMat("Zombie", "849A75"); ColorMat("Wound", "624F4A");
        ColorMat("Purple", "675373"); ColorMat("Concrete", "909A9A"); ColorMat("Road", "414B50");
        ColorMat("GlowBlue", "55DDE9", true); ColorMat("GlowRed", "F25D45", true);
        VehiclePalette();
        DesertUniformPalette();
    }
    static void ColorMat(string name, string hex, bool glow = false, bool unlit = false)
    {
        string path = Art + "/Materials/" + name + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = Shader.Find(unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit")
            ?? Shader.Find(unlit ? "Unlit/Color" : "Standard");
        if (mat == null) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, path); }
        if (unlit && mat.shader != shader)
        {
            mat.DisableKeyword("_EMISSION");
            mat.shader = shader;
        }
        Color color; ColorUtility.TryParseHtmlString("#" + hex, out color); mat.color = color;
        if (!unlit) mat.SetFloat("_Smoothness", name == "Glass" || name == "Window" ? .62f : .22f);
        if (glow) { mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor", color * 1.5f); }
        EditorUtility.SetDirty(mat); mats[name] = mat;
    }
    static Mesh Shape(string key)
    {
        Mesh mesh;
        if (meshes.TryGetValue(key, out mesh)) return mesh;
        string path = Art + "/Meshes/" + key + ".asset";
        mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (mesh == null)
        {
            if (key == "Panel") mesh = BeveledBox();
            else
            {
                var temp = GameObject.CreatePrimitive(key == "Round" ? PrimitiveType.Sphere : key == "Cylinder" ? PrimitiveType.Cylinder : PrimitiveType.Cube);
                mesh = UnityEngine.Object.Instantiate(temp.GetComponent<MeshFilter>().sharedMesh);
                UnityEngine.Object.DestroyImmediate(temp);
            }
            mesh.name = key; AssetDatabase.CreateAsset(mesh, path);
        }
        meshes[key] = mesh; return mesh;
    }
    static Mesh BeveledBox()
    {
        // An octagonal cross section gives armor and clothing a readable low-poly silhouette.
        Vector2[] ring = { new Vector2(-.38f,-.5f),new Vector2(.38f,-.5f),new Vector2(.5f,-.38f),new Vector2(.5f,.38f),new Vector2(.38f,.5f),new Vector2(-.38f,.5f),new Vector2(-.5f,.38f),new Vector2(-.5f,-.38f) };
        var v = new List<Vector3>(); var t = new List<int>();
        for (int i=0;i<8;i++)
        {
            int n=(i+1)%8, s=v.Count;
            v.Add(new Vector3(ring[i].x,ring[i].y,-.5f));v.Add(new Vector3(ring[i].x,ring[i].y,.5f));
            v.Add(new Vector3(ring[n].x,ring[n].y,.5f));v.Add(new Vector3(ring[n].x,ring[n].y,-.5f));
            t.AddRange(new[]{s,s+2,s+1,s,s+3,s+2});
        }
        for(int side=0;side<2;side++) for(int i=0;i<8;i++)
        {
            int s=v.Count,n=(i+1)%8;float z=side==0?-.5f:.5f;
            v.Add(new Vector3(0,0,z));v.Add(new Vector3(ring[i].x,ring[i].y,z));v.Add(new Vector3(ring[n].x,ring[n].y,z));
            t.AddRange(side==0?new[]{s,s+2,s+1}:new[]{s,s+1,s+2});
        }
        var m=new Mesh();m.SetVertices(v);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds();return m;
    }
    static Transform Node(Transform parent, string name, Vector3 p)
    { var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.localPosition = p; return go.transform; }
    static Transform Part(Transform parent, string name, Vector3 p, Vector3 size, string mat, string shape = "Panel")
    {
        var tr=Node(parent,name,p);tr.localScale=size;
        tr.gameObject.AddComponent<MeshFilter>().sharedMesh=Shape(shape);
        tr.gameObject.AddComponent<MeshRenderer>().sharedMaterial=mats[mat];return tr;
    }
    static Transform P(Transform p,string n,float x,float y,float z,float sx,float sy,float sz,string m,string shape="Panel")
    { return Part(p,n,new Vector3(x,y,z),new Vector3(sx,sy,sz),m,shape); }
    static void Bar(Transform root,string n,Vector3 a,Vector3 b,float radius,string mat)
    { var tr=Part(root,n,(a+b)*.5f,new Vector3(radius,(b-a).magnitude*.5f,radius),mat,"Cylinder");tr.rotation=root.rotation*Quaternion.FromToRotation(Vector3.up,b-a); }
    static void Cross(Transform root, Vector3 p, float size)
    { Part(root,"MedicalCrossH",p,new Vector3(size,.10f,size*.32f),"Red");Part(root,"MedicalCrossV",p,new Vector3(size*.32f,.11f,size),"Red"); }
    static void Badge(Transform root,string mat)
    { P(root,"ChestPatch",.18f,1.55f,.231f,.16f,.16f,.025f,mat); }
    static GameObject Root(string chapter, string id, string label, bool enemy, float hp, float speed, bool vehicle)
    {
        var root = new GameObject("Target_"+(enemy?"Enemy_":"Friendly_")+chapter+id);
        Target_Base target=enemy?(Target_Base)root.AddComponent<Target_Enemy>():root.AddComponent<Target_Friendly>();
        target.displayName=label;target.maxHP=hp;target.moveSpeed=speed;target.groundMask=LayerMask.GetMask("Ground");
        target.groundRayHeight=18;target.maxGroundDistance=55;target.turnSpeed=vehicle?5f:8f;
        target.waypointRadius=vehicle?2.5f:1.8f;target.scoreCoefficient=enemy?20:25;
        if(enemy) { var e=(Target_Enemy)target;e.killFunds=hp>=900?45:hp>=450?24:16;e.baseDamage=hp>=1100?3:hp>=650?2:1; }
        else { var f=(Target_Friendly)target;f.passFunds=vehicle?45:25;f.friendlyFireFunds=vehicle?60:35; }
        target.rigidbody=root.GetComponent<Rigidbody>();target.rigidbody.useGravity=false;target.rigidbody.constraints=RigidbodyConstraints.FreezeRotation;
        target.rigidbody.mass=vehicle?6:1;target.rigidbody.interpolation=RigidbodyInterpolation.Interpolate;
        limbs=new List<Transform>();wheels=new List<Transform>();lamps=new List<Renderer>();return root;
    }
    static void Save(GameObject root,string chapter,string id,Transform body,bool zombie,bool vehicle,float gait=1,string vehicleKind=null)
    {
        var target=root.GetComponent<Target_Base>();
        var visual=root.AddComponent<ChapterUnitVisual>();visual.body=body;visual.limbs=limbs.ToArray();visual.wheels=wheels.ToArray();
        visual.warningLamps=lamps.ToArray();visual.zombie=zombie;visual.vehicle=vehicle;visual.gait=gait;
        if (vehicle)
        {
            BoxCollider collider = root.GetComponent<BoxCollider>();
            if (chapter == "Desert")
                BuildDesertVehicleModel(visual, collider, vehicleKind);
            else if (vehicleKind == "bus")
                ApplyCivilianBusModel(visual, collider);
            else if (vehicleKind == "ambulance")
                ApplyAmbulanceModel(visual, collider);
            else
                AddVehicleDetails(visual, collider, vehicleKind);
            AddVehicleHeadlights(visual, chapter == "Desert" || vehicleKind == "bus" || vehicleKind == "ambulance");
        }
        target.renderers=root.GetComponentsInChildren<Renderer>();
        target.RefreshAllegianceLayer();
        var prefab=PrefabUtility.SaveAsPrefabAsset(root,Targets+"/"+chapter+"/"+root.name+".prefab");
        units[id]=prefab;UnityEngine.Object.DestroyImmediate(root);
    }
    static void Human(string chapter,string id,string label,bool enemy,string kind,float hp,float speed)
    {
        bool zombie=chapter=="City"&&enemy, big=kind=="brute"||kind=="heavy"||kind=="riot";
        bool suit=kind=="hazmat"||kind=="infected";
        var root=Root(chapter,id,label,enemy,hp,speed,false);
        var body=Node(root.transform,"ArticulatedBody",Vector3.zero);
        string coat=kind=="rescue"?"Rescue":kind=="medic"?"White":kind=="police"||kind=="riot"?"Navy":kind=="fire"||kind=="worker"?"Orange":suit?"Yellow":kind=="survivor"?"Purple":kind=="runner"?"Red":enemy?"Sand":"White";
        string pants=zombie?"Dark":kind=="survivor"?"Denim":"Dark";
        P(body,"Jacket",0,1.36f,0,.67f,.72f,.43f,coat);
        P(body,"Belt",0,1.05f,0,.64f,.1f,.45f,"Dark");
        P(body,"BeltBuckle",0,1.05f,.24f,.12f,.08f,.035f,"Steel");
        var head=Node(body,"Head",new Vector3(0,1.99f,0));
        P(head,"Face",0,0,0,.43f,.48f,.40f,zombie?"Zombie":"Skin");
        P(head,"Nose",0,-.02f,.22f,.09f,.10f,.08f,zombie?"Zombie":"Skin");
        for(int i=-1;i<=1;i+=2)P(head,"Eye",i*.106f,.065f,.205f,.062f,.05f,.025f,"Dark","Cube");
        P(head,"Mouth",0,-.13f,.207f,.18f,zombie?.07f:.025f,.022f,zombie?"Wound":"Hair","Cube");
        for(int i=0;i<4;i++)
        {
            bool arm=i<2;float side=(i%2==0)?-1:1;
            var limb=Node(body,arm?"Shoulder":"Hip",new Vector3(side*(arm?.43f:.185f),arm?1.62f:1.01f,0));limbs.Add(limb);
            P(limb,arm?"Sleeve":"Trouser",0,arm?-.19f:-.27f,0,arm?.24f:.26f,arm?.42f:.56f,arm?.29f:.31f,arm?coat:pants);
            P(limb,arm?"Glove":"Boot",0,arm?-.47f:-.69f,arm?.01f:.07f,arm?.19f:.28f,arm?.17f:.24f,arm?.23f:.45f,arm&&kind=="survivor"?"Skin":"Dark");
            if(zombie&&arm)limb.localRotation=Quaternion.Euler(-52,side*9,side*12);
        }
        if(kind=="raider"||kind=="assault"||kind=="heavy")
        {
            P(head,"Bandanna",0,-.12f,.17f,.45f,.20f,.16f,"Red");
            P(body,"ChestArmor",0,1.41f,.23f,.53f,.50f,.12f,kind=="heavy"?"Steel":"Dark");
            for(int i=-1;i<=1;i+=2)P(body,"AmmoPouch",i*.16f,1.24f,.32f,.18f,.21f,.1f,"Olive");
            P(body,"RifleStock",.49f,1.2f,.22f,.13f,.20f,.3f,"Dark");
            P(body,"Rifle",.49f,1.24f,.49f,.12f,.11f,.62f,"Steel");
            P(body,"RifleMuzzle",.49f,1.24f,.84f,.08f,.08f,.15f,"Dark");
        }
        else if(!zombie)
        {
            P(body,"RescueBackpack",0,1.42f,-.31f,.46f,.54f,.23f,kind=="survivor"?"Olive":"Rescue");
            Badge(body,kind=="medic"?"Red":"White");
            for(int i=-1;i<=1;i+=2)P(body,"ReflectiveShoulder",i*.20f,1.65f,.225f,.08f,.17f,.02f,"White");
        }
        if(kind=="rescue"||kind=="worker"||kind=="fire"||big)
        {
            P(head,"Helmet",0,.22f,0,.52f,.21f,.46f,kind=="fire"||kind=="worker"?"Yellow":enemy?"Dark":"White");
            P(head,"HelmetBrim",0,.16f,.08f,.58f,.055f,.57f,kind=="fire"||kind=="worker"?"Yellow":enemy?"Dark":"White");
        }
        else if(!suit)
        { P(head,"Hair",0,.20f,-.025f,.46f,.16f,.43f,kind=="police"?"Navy":"Hair");if(kind=="police")P(head,"CapBrim",0,.16f,.24f,.45f,.05f,.25f,"Navy"); }
        if(suit)
        {
            P(head,"Hood",0,.12f,-.09f,.57f,.55f,.43f,"Yellow");
            P(head,"Respirator",0,-.09f,.24f,.29f,.2f,.17f,"Dark");
            P(head,"Visor",0,.10f,.224f,.40f,.19f,.04f,"Glass");
            P(body,"AirTank",0,1.40f,-.36f,.40f,.57f,.28f,"Steel");
            if(zombie){ P(body,"TornSuit",-.2f,1.38f,.232f,.18f,.30f,.03f,"Wound");P(head,"CrackedVisor",.09f,.12f,.253f,.07f,.15f,.01f,"Dark"); }
        }
        if(kind=="medic") {P(body,"MedicalBag",-.58f,1.13f,.03f,.30f,.30f,.24f,"White");Cross(body,new Vector3(-.58f,1.30f,.03f),.21f);}
        if(kind=="fire") {P(body,"ReflectiveStripe",0,1.22f,.23f,.67f,.08f,.02f,"Yellow");P(body,"AxeHandle",.54f,1.15f,.17f,.06f,.72f,.06f,"Hair");P(body,"AxeHead",.54f,1.51f,.17f,.27f,.16f,.07f,"Steel");}
        if(zombie)
        {
            P(body,"TornHem",-.18f,1.16f,.229f,.16f,.25f,.02f,"Wound");
            P(body,"ExposedPatch",.15f,1.52f,.229f,.2f,.16f,.02f,"Zombie");
            head.localRotation=Quaternion.Euler(0,8,kind=="runner"?-10:12);
            body.localRotation=Quaternion.Euler(kind=="runner"?16:8,0,kind=="brute"?2:4);
        }
        if(big)
        {
            P(body,"ShoulderArmorL",-.48f,1.68f,0,.35f,.28f,.48f,kind=="brute"?"Zombie":"Steel");
            P(body,"ShoulderArmorR",.48f,1.68f,0,.35f,.28f,.48f,kind=="brute"?"Zombie":"Steel");
            if(kind=="riot") {P(body,"RiotChestPlate",0,1.4f,.25f,.61f,.6f,.12f,"Steel");P(head,"RiotVisor",0,0,.25f,.50f,.33f,.06f,"Glass");}
            root.transform.localScale=Vector3.one*(kind=="brute"?1.35f:1.14f);
            root.GetComponent<Target_Base>().armorValue=kind=="brute"?1:kind=="riot"?1.8f:1.25f;
        }
        var col=root.AddComponent<CapsuleCollider>();col.height=2.35f;col.radius=big?.5f:.38f;col.center=new Vector3(0,1.16f,0);
        if (chapter == "Desert") ApplyDesertUniform(body, !enemy);
        Save(root,chapter,id,body,zombie,false,kind=="runner"?1.45f:1);
    }

    static void Vehicle(string chapter,string id,string label,bool enemy,string kind,float hp,float speed)
    {
        bool bus=kind=="bus",truck=kind=="truck",ambulance=kind=="ambulance",crawler=kind=="crawler",rover=kind=="rover";
        var root=Root(chapter,id,label,enemy,hp,speed,true);
        var body=Node(root.transform,"Chassis",Vector3.zero);
        float length=bus?5.7f:truck?4.7f:ambulance?4.6f:crawler?3.7f:rover?4.4f:3.5f;
        string color=enemy?"Sand":bus?"Rescue":ambulance?"White":"Olive";
        P(body,"LowerChassis",0,.57f,0,1.85f,.30f,length,"Dark");
        P(body,"Bodywork",0,.94f,0,1.88f,.60f,length-.2f,color);
        P(body,"FrontBumper",0,.64f,length*.5f,2.03f,.22f,.22f,"Steel");
        P(body,"RearBumper",0,.64f,-length*.5f,2.03f,.22f,.22f,"Steel");
        if(bus||ambulance||truck)
        {
            float upperLength=bus?length-.2f:truck?1.65f:length-.5f;
            float upperZ=truck?length*.5f-1.1f:0;
            P(body,"Cabin",0,1.72f,upperZ,1.83f,1.1f,upperLength,color);
            P(body,"Roof",0,2.31f,upperZ,1.9f,.15f,upperLength+.08f,"White");
            P(body,"Windshield",0,1.94f,upperZ+upperLength*.5f+.015f,1.57f,.54f,.035f,"Glass");
            P(body,"WindshieldDivider",0,1.94f,upperZ+upperLength*.5f+.04f,.04f,.55f,.03f,"Steel");
            for(int side=-1;side<=1;side+=2)
            {
                int count=bus?6:truck?1:3;
                for(int i=0;i<count;i++)
                { float z=upperZ+upperLength*.5f-.45f-i*(upperLength-.7f)/Mathf.Max(1,count-1);P(body,"SideWindow",side*.93f,1.97f,z,.025f,.51f,bus?.63f:truck?.95f:.66f,"Glass"); }
                P(body,"DoorHandle",side*.96f,1.45f,upperZ+.15f,.04f,.06f,.22f,"Steel");
                P(body,"MirrorArm",side*1.05f,1.86f,length*.5f-.45f,.23f,.06f,.06f,"Dark");
                P(body,"Mirror",side*1.17f,1.86f,length*.5f-.45f,.08f,.26f,.18f,"Steel");
                P(body,"SideStripe",side*.95f,1.24f,0,.035f,.13f,length-.2f,ambulance?"Red":bus?"White":"Rescue");
            }
            if(truck)
            {
                P(body,"CanvasCargo",0,1.52f,-.95f,1.76f,1.03f,2.4f,"Sand");
                for(int i=0;i<4;i++)P(body,"CargoRib",0,2.06f,-1.9f+i*.65f,1.80f,.045f,.045f,"Olive");
                P(body,"CargoTailgate",0,1.10f,-length*.5f,1.80f,.65f,.08f,"Olive");
            }
            if(ambulance) {Cross(body,new Vector3(0,2.40f,.05f),.85f); Siren(body,2.48f,.9f);}
            if(bus){ P(body,"DestinationBoard",0,2.17f,length*.5f,.95f,.15f,.04f,"Dark");for(int i=0;i<3;i++)P(body,"BoardLight",-.27f+i*.27f,2.17f,length*.5f+.03f,.14f,.04f,.02f,"GlowBlue"); }
        }
        else
        {
            P(body,"SlopedHood",0,1.13f,1.02f,1.72f,.42f,.86f,color);
            P(body,"CabBase",0,1.27f,-.24f,1.66f,.35f,1.68f,color);
            if(kind=="buggy")
            {
                for(int side=-1;side<=1;side+=2)
                {Bar(body,"RollCage",new Vector3(side*.76f,1.2f,.6f),new Vector3(side*.76f,2f,-.1f),.075f,"Dark");Bar(body,"RearCage",new Vector3(side*.76f,2f,-.1f),new Vector3(side*.76f,1.2f,-1.25f),.075f,"Dark");}
                P(body,"RollCageRoof",0,2f,-.2f,1.70f,.10f,1.1f,"Dark");
                P(body,"DriverSeat",-.43f,1.55f,-.4f,.46f,.56f,.5f,"Dark");
                P(body,"DriverHead",-.43f,1.87f,-.20f,.27f,.30f,.27f,"Skin");
            }
            else
            {
                P(body,"ArmoredCab",0,1.63f,-.25f,1.7f,.71f,1.85f,color);
                P(body,"VisionSlit",0,1.75f,.70f,1.30f,.16f,.05f,"Glass");
                for(int side=-1;side<=1;side+=2)P(body,"SideArmor",side*.93f,1.17f,-.2f,.20f,.55f,2.4f,crawler?"Rescue":"Steel");
            }
            if(enemy)
            {
                P(body,"TurretBase",0,2.01f,-.55f,.86f,.16f,.86f,"Dark","Cylinder");
                P(body,"WeaponPod",0,2.18f,-.55f,.64f,.32f,.86f,"Dark");
                P(body,"WeaponBarrel",0,2.22f,.21f,.13f,.13f,.85f,"Steel");
                for(int side=-1;side<=1;side+=2)P(body,"RaiderMark",side*.96f,1.12f,.72f,.025f,.31f,.31f,"Red");
            }
            else {Cross(body,new Vector3(0,2.04f,-.15f),.7f);Siren(body,2.15f,-.6f);}
        }
        int axles=bus||rover||truck?3:2;
        for(int side=-1;side<=1;side+=2)for(int i=0;i<axles;i++)
        {
            float z=length*.31f-i*(length*.62f)/(axles-1);
            var pivot=Node(body,"WheelAxle",new Vector3(side*1.0f,.43f,z));wheels.Add(pivot);
            var tire=P(pivot,"Tire",0,0,0,.80f,.15f,.80f,"Rubber","Cylinder");tire.localRotation=Quaternion.Euler(0,0,90);
            var hub=P(pivot,"Hub",side*.16f,0,0,.43f,.018f,.43f,"Steel","Cylinder");hub.localRotation=Quaternion.Euler(0,0,90);
        }
        if(crawler)for(int side=-1;side<=1;side+=2)
        {P(body,"TrackBelt",side*1.0f,.43f,0,.36f,.65f,3.28f,"Rubber");for(int i=0;i<12;i++)P(body,"TrackTread",side*1f,.12f,-1.5f+i*.27f,.40f,.12f,.15f,"Steel");}
        for(int side=-1;side<=1;side+=2)
        {P(body,"Headlight",side*.64f,1.05f,length*.5f+.02f,.30f,.20f,.04f,"White");P(body,"TailLight",side*.7f,.96f,-length*.5f-.02f,.22f,.14f,.05f,"Red");}
        P(body,"Grille",0,.9f,length*.5f+.03f,.70f,.24f,.03f,"Dark");
        for(int i=0;i<4;i++)P(body,"GrilleSlat",-.25f+i*.17f,.9f,length*.5f+.05f,.045f,.19f,.02f,"Steel");
        var col=root.AddComponent<BoxCollider>();col.center=new Vector3(0,1.2f,0);col.size=new Vector3(2.14f,2.3f,length+.3f);
        root.GetComponent<Target_Base>().armorValue=enemy?(rover?1.8f:kind=="armored"?1.05f:.4f):0;
        Save(root,chapter,id,body,false,true,vehicleKind:kind);
    }

    [MenuItem("AI Defenders/Chapters/Upgrade Chapter Vehicle Visuals")]
    public static void UpgradeVehicleVisuals()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play before upgrading vehicle prefabs.");

        foreach (string name in new[] { "Steel", "Dark", "Rubber", "White", "Sand", "Olive", "Rescue", "Yellow",
            "Glass", "Red", "GlowBlue", "GlowRed" })
        {
            mats[name] = AssetDatabase.LoadAssetAtPath<Material>(Art + "/Materials/" + name + ".mat");
            if (!mats[name]) throw new InvalidOperationException("Missing chapter material: " + name);
        }
        VehiclePalette();

        var variants = new[]
        {
            ("Desert/Target_Friendly_DesertSupplyTruck", "truck"),
            ("Desert/Target_Friendly_DesertRescueCrawler", "crawler"),
            ("Desert/Target_Enemy_DesertRaiderBuggy", "buggy"),
            ("Desert/Target_Enemy_DesertArmoredBuggy", "armored"),
            ("Desert/Target_Enemy_DesertAttackRover", "rover"),
            ("City/Target_Friendly_CityAmbulance", "ambulance"),
            ("City/Target_Friendly_CityEvacBus", "bus")
        };
        foreach (var (name, kind) in variants)
        {
            string path = Targets + "/" + name + ".prefab";
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var visual = root.GetComponent<ChapterUnitVisual>();
                if (!visual || !visual.vehicle || !visual.body)
                    throw new InvalidOperationException("Missing vehicle rig: " + path);
                bool desert = name.StartsWith("Desert/", StringComparison.Ordinal);
                bool bus = kind == "bus";
                bool ambulance = kind == "ambulance";
                bool needsBus = bus && visual.body.Find("CivilianBus") == null;
                bool needsAmbulance = ambulance && visual.body.Find("Ambulance") == null;
                bool needsModel = desert && (visual.body.name != "DesertVehicle" || visual.headlightLenses.Length != 2);
                bool needsDetails = !desert && !bus && !ambulance && visual.body.Find("BodyDetail") == null;
                bool needsHeadlights = visual.body.Find("Headlights") == null;
                bool needsLens = visual.headlightLenses.Length != 2;
                if (!needsAmbulance && !needsBus && !needsModel && !needsDetails && !needsHeadlights && !needsLens)
                    continue;
                BoxCollider collider = root.GetComponent<BoxCollider>();
                if (needsModel)
                    BuildDesertVehicleModel(visual, collider, kind);
                else if (needsBus)
                    ApplyCivilianBusModel(visual, collider);
                else if (needsAmbulance)
                    ApplyAmbulanceModel(visual, collider);
                else if (needsDetails)
                    AddVehicleDetails(visual, collider, kind);
                if (needsHeadlights || needsModel || needsBus || needsAmbulance)
                    AddVehicleHeadlights(visual, desert || bus || ambulance);
                else if (needsLens)
                    SetVehicleHeadlightLenses(visual);
                var target = root.GetComponent<Target_Base>();
                target.renderers = root.GetComponentsInChildren<Renderer>();
                target.RefreshAllegianceLayer();
                PrefabUtility.SaveAsPrefabAsset(root, path, out bool saved);
                if (!saved) throw new InvalidOperationException("Could not save " + path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Updated chapter vehicle details, headlights and wreck effects.");
    }

    static void AddVehicleHeadlights(ChapterUnitVisual visual, bool useModelLenses)
    {
        var farmerTruck = AssetDatabase.LoadAssetAtPath<GameObject>(FarmerTruckPath);
        var source = farmerTruck != null ? farmerTruck.GetComponent<Target_FarmerTruck>() : null;
        if (source == null || source.headlights == null || source.headlights.Length == 0
            || source.headlights[0] == null)
            throw new InvalidOperationException("Missing Farmer Truck headlight reference.");

        Light reference = source.headlights[0];
        Transform mount = Node(visual.body, "Headlights", Vector3.zero);
        var headlights = new List<Light>(2);
        if (!useModelLenses)
            SetVehicleHeadlightLenses(visual);
        foreach (Renderer lensRenderer in visual.headlightLenses)
        {
            Transform lens = lensRenderer.transform;
            Vector3 point = visual.body.InverseTransformPoint(lensRenderer.bounds.center);
            if (!useModelLenses)
                Part(mount, "LensBezel", point + Vector3.back * .008f,
                    new Vector3(lens.localScale.x + .055f, lens.localScale.y + .04f, .033f), "Steel");

            string side = point.x < 0f ? "Left" : "Right";
            Transform emitter = Node(mount, side + "Spotlight", point + Vector3.forward * .04f);
            emitter.localRotation = Quaternion.Euler(24f, 0f, 0f);
            Light light = emitter.gameObject.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = reference.color;
            light.intensity = reference.intensity;
            light.range = reference.range;
            light.spotAngle = reference.spotAngle;
            light.innerSpotAngle = reference.innerSpotAngle;
            light.shadows = LightShadows.None;
            headlights.Add(light);
        }
        visual.headlights = headlights.ToArray();
    }

    static void SetVehicleHeadlightLenses(ChapterUnitVisual visual)
    {
        var lenses = new List<Renderer>(2);
        foreach (Transform part in visual.body)
        {
            if (part.name != "Headlight") continue;
            var lens = part.GetComponent<Renderer>();
            lens.sharedMaterial = mats["HeadlampLens"];
            lenses.Add(lens);
        }
        if (lenses.Count != 2)
            throw new InvalidOperationException("Expected two headlight lenses on " + visual.name);
        visual.headlightLenses = lenses.ToArray();
    }

    static void AddVehicleDetails(ChapterUnitVisual visual, BoxCollider collider, string kind)
    {
        SetVehicleWreckEffect(visual);
        Transform details = Node(visual.body, "BodyDetail", Vector3.zero);
        float length = collider.size.z - .3f;
        float front = length * .5f;
        bool armored = kind == "armored" || kind == "rover";
        bool enclosed = kind == "truck" || kind == "ambulance" || kind == "bus";
        bool crawler = kind == "crawler";
        var breakaway = new Transform[2];

        P(details, "FrontLowerFascia", 0, .64f, front + .1f, 1.48f, .14f, .08f, "Dark");
        P(details, "RearHitch", 0, .67f, -front - .12f, .23f, .15f, .17f, "Steel");
        for (int side = -1; side <= 1; side += 2)
        {
            P(details, "BumperEnd", side * .86f, .65f, front + .05f, .25f, .19f, .14f, "Rubber");
            P(details, "LampHousing", side * .64f, 1.05f, front - .018f, .40f, .25f, .065f, "Dark");
            P(details, "MarkerLight", side * .88f, 1.08f, front + .013f, .11f, .11f, .03f, "Yellow");

            string panelColor = armored ? "Steel" : crawler || kind == "bus" ? "Rescue" :
                kind == "ambulance" ? "White" : kind == "truck" ? "Olive" : "Sand";
            Transform panel = Node(details, side < 0 ? "LeftBodyPanel" : "RightBodyPanel",
                new Vector3(side * .96f, enclosed ? 1.48f : 1.1f, enclosed ? -.15f : .14f));
            breakaway[side < 0 ? 0 : 1] = panel;
            float panelLength = armored ? 1.48f : enclosed ? .82f : .9f;
            P(panel, "BeveledPanel", 0, 0, 0, .13f, enclosed ? .34f : .27f, panelLength, panelColor);
            P(panel, "Trim", side * .065f, .047f, 0, .022f, .064f, panelLength * .72f, "Dark");

            if (enclosed)
                P(details, "SillStep", side * 1.01f, .76f, front - 1.12f, .24f, .12f, 1.14f, "Steel");
            else if (crawler)
                P(details, "TrackGuard", side * 1.01f, .85f, 0, .45f, .14f, length * .88f, "Steel");
            else
                P(details, "SideGuard", side * .95f, .80f, 0, .22f, .22f, length * .57f, "Dark");
        }

        if (enclosed)
            P(details, "WindshieldCowl", 0, 1.64f, front - .19f, 1.65f, .13f, .30f, "Dark");
        else if (armored)
            P(details, "SlopedArmor", 0, 1.19f, front - .36f, 1.71f, .22f, .56f, "Steel")
                .localRotation = Quaternion.Euler(14f, 0f, 0f);
        else if (!crawler)
            P(details, "HoodInset", 0, 1.36f, 1.10f, .72f, .08f, .58f, "Dark");

        if (!crawler)
        {
            foreach (Transform wheel in visual.wheels)
            {
                float side = Mathf.Sign(wheel.localPosition.x);
                P(wheel, "HubCap", side * .18f, 0, 0, .27f, .045f, .27f, "Dark", "Cylinder")
                    .localRotation = Quaternion.Euler(0f, 0f, 90f);
                P(details, "WheelFender", wheel.localPosition.x, .86f, wheel.localPosition.z,
                    .29f, .16f, .88f, enclosed ? "Rubber" : armored ? "Steel" : "Dark");
            }
        }
        visual.breakawayPanels = breakaway;
    }

    static void SetVehicleWreckEffect(ChapterUnitVisual visual)
    {
        const string missileAmmo = "Assets/Ammo/AmmoData_Missile.asset";
        var ammo = AssetDatabase.LoadAssetAtPath<AmmoData>(missileAmmo);
        if (!ammo || !ammo.impactEffect)
            throw new InvalidOperationException("Missing missile impact VFX: " + missileAmmo);
        visual.wreckBurst = ammo.impactEffect;
        visual.wreckSound = ammo.explosionSfx;
    }

    static void Siren(Transform p,float y,float z)
    {
        P(p,"LightBar",0,y,z,1.14f,.10f,.26f,"Dark");
        for(int i=-1;i<=1;i+=2)lamps.Add(P(p,"Siren",i*.38f,y+.08f,z,.36f,.17f,.28f,i<0?"GlowBlue":"GlowRed").GetComponent<Renderer>());
    }

    static void Campaign(string chapter,int stage)
    {
        string path="Assets/Waves/StageData_"+chapter+"_"+stage+".asset";
        var data=AssetDatabase.LoadAssetAtPath<StageData>(path);
        if(data==null){data=ScriptableObject.CreateInstance<StageData>();AssetDatabase.CreateAsset(data,path);}
        bool city=chapter=="City";
        data.firstPrepTime=stage==1?40:35;data.startingFunds=city?650+(stage-1)*125:500+(stage-1)*125;data.startingBaseHP=25;
        int waveCount=stage+3;
        var waves=new List<WaveData>();
        for(int w=0;w<waveCount;w++)
        {
            var groups=new List<SpawnGroup>();
            int streams=city?(stage==1&&w<2?2:4):2;
            string[] routes=city?new[]{"City_North","City_South","City_East","City_West"}:new[]{"Enemy spawner1","Enemy spawner2"};
            // Repeated raider slots preserve the spawn pattern of the retired variants.
            string[] enemies=city?(stage==1?new[]{"WalkerZombie","RunnerZombie"}:stage==2?new[]{"WalkerZombie","RunnerZombie","WalkerZombie","BruteZombie"}:new[]{"WalkerZombie","RunnerZombie","WalkerZombie","RunnerZombie","BruteZombie","ArmoredZombie"}):(stage==1?new[]{"Raider","RaiderBuggy"}:stage==2?new[]{"Raider","RaiderBuggy","Raider","ArmoredBuggy"}:new[]{"Raider","Raider","RaiderBuggy","ArmoredBuggy","Raider","AttackRover"});
            string[] friends=city?(stage==1?new[]{"Survivor","PoliceRescuer"}:stage==2?new[]{"Survivor","PoliceRescuer","PoliceRescuer","Ambulance"}:new[]{"Survivor","PoliceRescuer","PoliceRescuer","Ambulance","Survivor","EvacBus"}):(stage==1?new[]{"RescueWorker","SupplyTruck"}:stage==2?new[]{"RescueWorker","SupplyTruck","RescueCrawler"}:new[]{"RescueWorker","SupplyTruck","RescueCrawler","Medic"});
            int enemyCount=(city?12:10)+(stage-1)*5+w*(stage+2);
            // Each small packet has its own time and lane. Civilians are interleaved, never a separate safe wave.
            for(int i=0;i<enemyCount;i++)
            {
                int maxKind=Mathf.Min(enemies.Length,2+w+Mathf.Max(0,stage-2));
                int index=i%Mathf.Min(3,maxKind);
                // Only one heavy per eight targets; the final wave introduces the chapter's armored finale.
                if(maxKind>3&&i%8==7)index=3+(i/8+w)%(maxKind-3);
                if(stage==3&&w==waveCount-1&&i>=enemyCount-2)index=enemies.Length-1;
                groups.Add(Group(i*(stage==1?2.1f:stage==2?1.8f:1.6f),routes[(i+w)%streams],enemies[index]));
            }
            int friendlyCount=4+stage+w/2;
            for(int i=0;i<friendlyCount;i++)groups.Add(Group(3+i*6.3f,routes[(i+w+1)%streams],friends[(i+w)%friends.Length]));
            groups.Sort((a,b)=>a.spawnTime.CompareTo(b.spawnTime));
            waves.Add(new WaveData{waveName=(w==waveCount-1?"최종 방어선":"방어 웨이브 "+(w+1)),duration=180f+stage*10,nextWavePrepTime=stage==1?25:stage==2?22:20,groups=groups.ToArray()});
        }
        data.waves=waves.ToArray();EditorUtility.SetDirty(data);
    }
    static SpawnGroup Group(float time,string route,string id)
    {return new SpawnGroup{spawnTime=time,spawnerId=route,targets=new[]{new SpawnTargetEntry{prefab=units[id],count=1}}};}

    static void TrainingGround(string chapter)
    {
        var root=new GameObject(chapter=="Desert"?"사막 · 협곡 훈련장":"도시 · 도로 훈련장");
        // Capture center remains clear; unlike scene-only terrain these grounds ship in builds too.
        var floor=P(root.transform,"TrainingFloor",0,-.15f,0,110,.3f,110,chapter=="Desert"?"Sand":"Road","Cube");
        floor.gameObject.AddComponent<BoxCollider>();
        if(chapter=="Desert")for(int side=-1;side<=1;side+=2)for(int i=0;i<9;i++)
        {P(root.transform,"CanyonRock",side*(25+i%3),3f+i%3,-40+i*10,9,6+i%3*2,13,"Sand","Round");}
        else
        {
            for(int side=-1;side<=1;side+=2)
            {
                var walk=P(root.transform,"Sidewalk",side*21,.06f,0,10,.4f,100,"Concrete","Cube");walk.gameObject.AddComponent<BoxCollider>();
                for(int i=0;i<5;i++)
                {
                    float height=8+i%3*5,z=-40+i*20;var b=P(root.transform,"CityBlock",side*36,height*.5f,z,15,height,15,"Concrete","Cube");
                    for(int row=0;row<(int)height/2;row++)for(int col=0;col<5;col++)P(root.transform,"Windows",side*28.4f,1.4f+row*2,z-5+col*2.5f,.04f,1.2f,1.5f,"Glass","Cube");
                }
            }
            for(int i=0;i<15;i++)P(root.transform,"LaneMark",0,.015f,-45+i*6,.16f,.03f,2.5f,"Yellow","Cube");
        }
        int layer=LayerMask.NameToLayer("Ground");foreach(var t in root.GetComponentsInChildren<Transform>())t.gameObject.layer=layer<0?0:layer;
        PrefabUtility.SaveAsPrefabAsset(root,"Assets/Resources/ChapterTrainingTerrains/"+chapter+".prefab");UnityEngine.Object.DestroyImmediate(root);
    }
}
#endif
