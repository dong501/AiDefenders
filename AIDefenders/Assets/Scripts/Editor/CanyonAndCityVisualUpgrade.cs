using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Deterministic scene authoring. Imported models use only the supplied mesh data.
public static class CanyonAndCityVisualUpgrade
{
    const string Folder = "Assets/Art/ChapterVisualUpgrade";
    const string DesertPath = "Assets/Scenes/GameStages/Desert/Desert_1.unity";
    const string CityPath = "Assets/Scenes/GameStages/City/City_1.unity";
    static readonly string[] BandColors = { "#975034", "#B86A41", "#D68C58", "#A95B3C", "#E2AB73", "#BB714B", "#D79866", "#AD6545", "#EBC28D", "#DFAE79" };
    static readonly float[] Levels = { 0, .15f, .19f, .36f, .4f, .57f, .61f, .77f, .81f, .96f, 1 };
    static Vector3[][] routes;
    static Transform canyonRoot;
    static Material[] bands;

    [MenuItem("Tools/AI Defenders/Environment Expansion/7 Canyon And City Skyline")]
    public static void Apply()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode first.");
        foreach (string p in new[] { DesertPath, CityPath })
        { var s = SceneManager.GetSceneByPath(p); if (s.isLoaded && s.isDirty) throw new InvalidOperationException("Unsaved scene: " + p); }
        Directory.CreateDirectory("Docs/CanyonCityUpgrade/Before");
        foreach (string p in new[] { DesertPath, CityPath, "Assets/Scenes/GameStages/Desert/Terrain_Desert_Day.asset" })
        { string dest = "Docs/CanyonCityUpgrade/Before/" + Path.GetFileName(p); if (!File.Exists(dest)) File.Copy(p, dest); }
        EnsureFolder(Folder + "/Generated");
        ImportBuildings();
        Edit(DesertPath, UpgradeDesert);
        Edit(CityPath, UpgradeCity);
        AssetDatabase.SaveAssets();
    }

    static void Edit(string path, Action<Scene> edit)
    {
        var scene = SceneManager.GetSceneByPath(path); bool opened = !scene.isLoaded;
        if (opened) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        var previous = SceneManager.GetActiveScene(); SceneManager.SetActiveScene(scene);
        try { edit(scene); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); }
        finally { SceneManager.SetActiveScene(previous); if (opened) EditorSceneManager.CloseScene(scene, true); }
    }

    static Transform[] All(Scene scene) { return scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray(); }
    static void EnsureFolder(string p)
    { if (AssetDatabase.IsValidFolder(p)) return; var parent = Path.GetDirectoryName(p).Replace('\\', '/'); EnsureFolder(parent); AssetDatabase.CreateFolder(parent, Path.GetFileName(p)); }
    static Material Material(string name, string hex)
    {
        string p = Folder + "/Generated/" + name + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(p);
        if (!m) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m, p); }
        Color c; ColorUtility.TryParseHtmlString(hex, out c); m.SetColor("_BaseColor", c); m.SetFloat("_Smoothness", .08f);
        EditorUtility.SetDirty(m); return m;
    }
    static Mesh SaveMesh(string name, Mesh mesh)
    {
        string p = Folder + "/Generated/" + name + ".asset"; mesh.name = name;
        var existing = AssetDatabase.LoadAssetAtPath<Mesh>(p);
        if (existing) { EditorUtility.CopySerialized(mesh, existing); UnityEngine.Object.DestroyImmediate(mesh); mesh = existing; EditorUtility.SetDirty(mesh); }
        else AssetDatabase.CreateAsset(mesh, p);
        return mesh;
    }

    public static void ImportBuildings()
    {
        EnsureFolder(Folder + "/Generated");
        var model = JsonUtility.FromJson<EnvironmentExpansionBuilder.Model>(File.ReadAllText(Folder + "/Source/CityBuildings.json"));
        foreach (var item in model.assets)
        {
            var vertices = new List<Vector3>(); var groups = new Dictionary<string, List<int>>();
            foreach (var part in item.parts)
            {
                string key = part.color.TrimStart('#');
                if (!groups.ContainsKey(key)) groups[key] = new List<int>();
                foreach (int index in part.triangles) { groups[key].Add(vertices.Count); vertices.Add(part.vertices[index]); }
            }
            var mesh = new Mesh(); mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(vertices); mesh.subMeshCount = groups.Count;
            int sub = 0; foreach (var group in groups) mesh.SetTriangles(group.Value, sub++);
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); mesh = SaveMesh(item.name, mesh);
            var go = new GameObject(item.name); go.layer = 2;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterials = groups.Keys.Select(k => Material("Building_" + k, "#" + k)).ToArray();
            PrefabUtility.SaveAsPrefabAsset(go, Folder + "/Generated/" + item.name + ".prefab");
            UnityEngine.Object.DestroyImmediate(go);
        }
    }

    static float RouteDistance(float x, float z)
    {
        var p = new Vector2(x, z); float distance = float.MaxValue;
        foreach (var path in routes) for (int i = 1; i < path.Length; i++)
        { var a = new Vector2(path[i-1].x, path[i-1].z); var v = new Vector2(path[i].x, path[i].z)-a; float t = v.sqrMagnitude > 0 ? Mathf.Clamp01(Vector2.Dot(p-a,v)/v.sqrMagnitude) : 0; distance = Mathf.Min(distance, Vector2.Distance(p,a+t*v)); }
        return distance;
    }
    // The two firing rims flank the active paths; the outpost stays in the open basin.
    static float Inner(float z, int side)
    {
        float edge = side < 0 ? float.MaxValue : float.MinValue;
        foreach (var path in routes) for (int i=1;i<path.Length;i++)
        {
            var a=path[i-1];var b=path[i]; if(z<Mathf.Min(a.z,b.z)||z>Mathf.Max(a.z,b.z)||Mathf.Abs(b.z-a.z)<.01f)continue;
            float x=Mathf.Lerp(a.x,b.x,(z-a.z)/(b.z-a.z)); edge=side<0?Mathf.Min(edge,x):Mathf.Max(edge,x);
        }
        if (edge==float.MaxValue || edge==float.MinValue) edge=side<0?-80:85;
        return edge+side*(18+1.4f*Mathf.Sin(z*.12f+side));
    }
    static void UpgradeDesert(Scene scene)
    {
        var all=All(scene); string before=EnvironmentExpansionBuilder.GameplaySignature(scene);
        var spawners=all.Select(t=>t.GetComponent<EnemySpawner>()).Where(p=>p).ToArray();
        routes=spawners.SelectMany(p=>p.paths.Select(path=>new[]{p.transform.position}.Concat(path.GetWorldPoints()).ToArray())).ToArray();
        var old=scene.GetRootGameObjects().FirstOrDefault(r=>r.name=="Desert_Grand_Canyon");if(old)UnityEngine.Object.DestroyImmediate(old);
        canyonRoot=new GameObject("Desert_Grand_Canyon").transform;
        bands=BandColors.Select((c,i)=>Material("Canyon_Band_"+i,c)).ToArray();
        var sandstone=SandstoneTexture();foreach(var material in bands){material.SetTexture("_BaseMap",sandstone);EditorUtility.SetDirty(material);}
        BuildRim(-1,-72,35,14); BuildRim(1,-32,35,17);
        // Broken, varied mesas recede into the background instead of identical rock walls.
        Butte("NorthWest_Mesa",new Vector3(-138,0,-105),52,43,47,1);
        Butte("North_Monument",new Vector3(-26,0,-150),48,35,64,2);
        Butte("NorthEast_Mesa",new Vector3(139,0,-121),52,37,52,3);
        Butte("West_Far_Mesa",new Vector3(-192,0,-5),42,62,58,4);
        Butte("East_Far_Mesa",new Vector3(196,0,14),45,58,62,5);
        Butte("Distant_Spire",new Vector3(91,0,-190),18,22,74,6);

        var terrain=all.Select(t=>t.GetComponent<Terrain>()).First(t=>t);var td=terrain.terrainData;
        // Lower only terrain buried inside the new rock masses; active route terrain is untouched.
        int res=td.heightmapResolution;var heights=td.GetHeights(0,0,res,res);var size=td.size;var origin=terrain.transform.position;
        for(int j=0;j<res;j++)for(int i=0;i<res;i++)
        {
            float x=origin.x+i*size.x/(res-1),z=origin.z+j*size.z/(res-1);
            if(z < -72 || z>35 || RouteDistance(x,z)<9)continue;
            foreach(int side in new[]{-1,1})
            {
                if(side>0&&z < -32)continue;
                float edge=Inner(z,side),width=RimWidth(z,side);
                if((x-edge)*side>-.1f&&(x-edge)*side<width+1)
                { float cap=(side<0?12f:15f);heights[j,i]=Mathf.Min(heights[j,i],(cap-origin.y)/size.y); }
            }
        }
        td.SetHeights(0,0,heights);EditorUtility.SetDirty(td);
        // Hide the earlier low outcrops that would intersect the stratified formations.
        var outcrops=all.FirstOrDefault(t=>t.name=="Sandstone_Outcrops");if(outcrops)outcrops.gameObject.SetActive(false);
        for(int side=-1;side<=1;side+=2)foreach(float z in new[]{-10f,19f})
        {
            float x=Inner(z,side)-side*2;
            Pad("HighGround_"+side+"_"+z,new Vector3(x,(side<0?14:17)+.18f,z),terrain.gameObject.layer);
        }
        foreach(var root in scene.GetRootGameObjects().Where(r=>r.name=="Desert_Cactus_Upgrade"||r.name=="Environment_Asset_Expansion"))
        foreach(Transform prop in root.transform)
        {
            float x=prop.position.x,z=prop.position.z;
            foreach(int side in new[]{-1,1})
            {
                if(z<(side<0?-72:-32)||z>35)continue;
                float d=(x-Inner(z,side))*side;
                if(d>1&&d<RimWidth(z,side)-1)
                {
                    var mf=prop.GetComponent<MeshFilter>();if(!mf)continue;
                    var pos=prop.position;pos.y=(side<0?14:17)-mf.sharedMesh.bounds.min.y*prop.localScale.y;prop.position=pos;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(prop);break;
                }
            }
        }
        // Start above the rim, looking down the canyon toward the approaching enemies.
        var rig=scene.GetRootGameObjects().First(r=>r.name=="CCTVSystem").transform;
        rig.position=new Vector3(.37f,32,65);rig.rotation=Quaternion.Euler(38,180,0);
        foreach(var cam in rig.GetComponentsInChildren<Camera>(true))cam.transform.localRotation=Quaternion.identity;
        foreach(var component in all.Where(t=>t&&PrefabUtility.IsPartOfPrefabInstance(t)))PrefabUtility.RecordPrefabInstancePropertyModifications(component);
        // Only the drone viewpoint is intentionally different in the gameplay signature.
        File.WriteAllText("Docs/CanyonCityUpgrade/desert_layout_before.txt",before);
        Debug.Log("Grand Canyon: two buildable firing rims, four level high-ground sites and six distant mesas. Enemy paths and goal preserved.");
    }
    static float RimWidth(float z,int side) { return 48+12*Mathf.Sin(z*.035f+side)+6*Mathf.Cos(z*.09f); }
    static void BuildRim(int side,float start,float end,float height)
    {
        const int count=29;var ring=new List<Vector3>();
        for(int i=0;i<count;i++){float z=Mathf.Lerp(start,end,(float)i/(count-1));ring.Add(new Vector3(Inner(z,side),0,z));}
        for(int i=count-1;i>=0;i--){float z=Mathf.Lerp(start,end,(float)i/(count-1));ring.Add(new Vector3(Inner(z,side)+side*RimWidth(z,side),0,z));}
        Rock(side<0?"West_Firing_Rim":"East_Firing_Rim",ring.ToArray(),height,true);
    }
    static void Butte(string name,Vector3 center,float rx,float rz,float height,int seed)
    {
        var ring=new Vector3[19];for(int i=0;i<ring.Length;i++)
        {float a=i*Mathf.PI*2/ring.Length;float r=1+.12f*Mathf.Sin(i*2.7f+seed)+.05f*Mathf.Cos(i*4.2f);ring[i]=center+new Vector3(Mathf.Cos(a)*rx*r,0,Mathf.Sin(a)*rz*r);}
        Rock(name,ring,height,false);
    }
    static void Rock(string name,Vector3[] ring,float height,bool rim)
    {
        var center=ring.Aggregate(Vector3.zero,(s,v)=>s+v)/ring.Length;
        var vs=new List<Vector3>();var uv=new List<Vector2>();var ts=Enumerable.Range(0,10).Select(i=>new List<int>()).ToArray();
        Action<Vector3,Vector3,Vector3,int,Vector3> tri=(a,b,c,sub,normal)=>
        {if(Vector3.Dot(Vector3.Cross(b-a,c-a),normal)<0){var tmp=b;b=c;c=tmp;}int n=vs.Count;vs.Add(a);vs.Add(b);vs.Add(c);foreach(var p in new[]{a,b,c})uv.Add(normal.y>.5f?new Vector2(p.x*.1f,p.z*.1f):new Vector2((p.x+p.z)*.09f,p.y*.15f));ts[sub].AddRange(new[]{n,n+1,n+2});};
        Func<int,int,Vector3> point=(i,l)=>
        {var p=ring[i%ring.Length];float f=Levels[l];float inset=rim?(1-f)*2:(1-f)*7;var direction=(p-center).normalized; p+=direction*(inset+(l==0||l==10?0:Mathf.Sin(i*2.3f+l*.7f)*.6f));p.y=Mathf.Lerp(-5,height,f)+(l==0||l==10?0:Mathf.Sin(i*1.8f+l)*.18f);return p;};
        for(int l=0;l<10;l++)for(int i=0;i<ring.Length;i++)
        {var a=point(i,l);var b=point((i+1)%ring.Length,l);var c=point((i+1)%ring.Length,l+1);var d=point(i,l+1);var normal=(a+b)*.5f-center;normal.y=0;tri(a,b,c,l,normal);tri(a,c,d,l,normal);}
        if(rim)
        {int count=ring.Length/2;for(int i=0;i<count-1;i++){var a=point(i,10);var b=point(i+1,10);var c=point(ring.Length-2-i,10);var d=point(ring.Length-1-i,10);tri(a,b,c,9,Vector3.up);tri(a,c,d,9,Vector3.up);}}
        else {center.y=height;for(int i=0;i<ring.Length;i++)tri(center,point(i,10),point((i+1)%ring.Length,10),9,Vector3.up);}
        var mesh=new Mesh();mesh.SetVertices(vs);mesh.SetUVs(0,uv);mesh.subMeshCount=10;for(int i=0;i<10;i++)mesh.SetTriangles(ts[i],i);mesh.RecalculateNormals();mesh.RecalculateBounds();mesh=SaveMesh(name,mesh);
        var go=new GameObject(name);go.transform.SetParent(canyonRoot,false);go.layer=rim?0:2;go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterials=bands;
        if(rim)go.AddComponent<MeshCollider>().sharedMesh=mesh;
    }
    static void Pad(string name,Vector3 pos,int layer)
    {
        var ring=new Vector3[13];for(int i=0;i<ring.Length;i++)
        {float a=i*Mathf.PI*2/ring.Length;float r=1+.07f*Mathf.Sin(i*2.4f);ring[i]=new Vector3(pos.x+Mathf.Cos(a)*4.5f*r,0,pos.z+Mathf.Sin(a)*4*r);}
        Rock(name,ring,pos.y+.2f,false);
        var go=canyonRoot.Find(name).gameObject;go.layer=layer;go.AddComponent<MeshCollider>().sharedMesh=go.GetComponent<MeshFilter>().sharedMesh;
    }

    static Texture2D SandstoneTexture()
    {
        string path=Folder+"/Generated/Sandstone_Weathering.png";
        var texture=new Texture2D(256,256,TextureFormat.RGB24,true);var colors=new Color[256*256];
        for(int y=0;y<256;y++)for(int x=0;x<256;x++)
        {
            float grain=Mathf.PerlinNoise(x*.39f,y*.37f)*.1f;
            float strata=Mathf.PerlinNoise(x*.025f,y*.38f)*.08f;
            float erosion=Mathf.Pow(Mathf.PerlinNoise(x*.16f,y*.012f),5)*.16f;
            float v=.81f+grain+strata-erosion;colors[y*256+x]=new Color(v,v,v,1);
        }
        texture.SetPixels(colors);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.wrapMode=TextureWrapMode.Repeat;importer.mipmapEnabled=true;importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    static void UpgradeCity(Scene scene)
    {
        string before=EnvironmentExpansionBuilder.GameplaySignature(scene);var all=All(scene);
        var originals=all.First(t=>t.name=="City_Blocks");
        var old=scene.GetRootGameObjects().FirstOrDefault(r=>r.name=="City_Diverse_Skyline");if(old)UnityEngine.Object.DestroyImmediate(old);
        var root=new GameObject("City_Diverse_Skyline").transform;
        var names=new[]{"CityOfficeTower","CityHotel","CityHospital","CityBank","CityCinema","CityOfficetel","CityRestaurant","CityTownhouseRow","CityDepartmentStore","CityBroadcastingStation","CityLibrary","CityPoliceStation","CitySchool","CityShoppingMall","CityArtGallery","CityMotel","CityResearchCenter","CityFireStation","CityCourthouse","CitySupermarket","CityCommunityCenter","CityParkingBuilding","CityBookstore","CityGuesthouse","CityTownHall","CityConcertHall","CityPostOffice","CitySportsHall","CitySmallFactory","CityBusTerminal","CityAutoRepairShop","CityVeterinaryClinic"};
        int index=0;
        foreach(Transform source in originals)
        {
            var sourceBounds=source.GetComponent<Renderer>().bounds;string name=names[index%names.Length];
            float width=index%3==0?23:21;float height=name=="CityOfficeTower"?62:name=="CityOfficetel"?49:0;
            Building(root,name,source.position.x,source.position.z,width,height,source.position.x<0?-90:90);
            index++;
        }
        originals.gameObject.SetActive(false);
        if(PrefabUtility.IsPartOfPrefabInstance(originals))PrefabUtility.RecordPrefabInstancePropertyModifications(originals.gameObject);
        // Tall buildings form an outer skyline and do not occupy the four approach roads.
        for(int side=-1;side<=1;side+=2)for(int i=0;i<4;i++)
        {
            string name=i%2==0?"CityOfficeTower":"CityOfficetel";
            Building(root,name,side*(i%2==0?237:266),-220+i*80,25,64+i*6+(side>0?7:0),side<0?-90:90);
        }
        foreach(var component in root.GetComponentsInChildren<Transform>())if(PrefabUtility.IsPartOfPrefabInstance(component))PrefabUtility.RecordPrefabInstancePropertyModifications(component);
        if(before!=EnvironmentExpansionBuilder.GameplaySignature(scene))throw new InvalidOperationException("City gameplay transforms changed.");
        Debug.Log("City skyline: 32 supplied building types placed in existing blocks; 8 high-rise buildings added, 64–89m tall. Four shelter routes preserved.");
    }
    static GameObject Building(Transform parent,string name,float x,float z,float width,float height,float yaw)
    {
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"/Generated/"+name+".prefab");
        var mesh=prefab.GetComponent<MeshFilter>().sharedMesh;float scale=width/Mathf.Max(mesh.bounds.size.x,mesh.bounds.size.z);
        var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,parent);go.name=name+"_"+parent.childCount;
        float sy=height>0?height/mesh.bounds.size.y:scale;
        go.transform.localScale=new Vector3(scale,sy,scale);go.transform.position=new Vector3(x,.34f-mesh.bounds.min.y*sy,z);go.transform.rotation=Quaternion.Euler(0,yaw,0);return go;
    }

    public static string Capture(string scenePath,string name,Vector3 position,Vector3 target)
    {
        var scene=SceneManager.GetSceneByPath(scenePath);bool opened=!scene.isLoaded;
        if(opened)scene=EditorSceneManager.OpenScene(scenePath,OpenSceneMode.Additive);
        var original=SceneManager.GetActiveScene();var hidden=new List<GameObject>();GameObject cameraObject=null;RenderTexture rt=null;Texture2D texture=null;var previous=RenderTexture.active;
        try
        {
            for(int i=0;i<SceneManager.sceneCount;i++)
            {var other=SceneManager.GetSceneAt(i);if(other==scene)continue;foreach(var root in other.GetRootGameObjects().Where(r=>r.activeSelf)){root.SetActive(false);hidden.Add(root);}}
            foreach(var drone in scene.GetRootGameObjects().Where(r=>r.name.StartsWith("CCTV")&&r.activeSelf)){drone.SetActive(false);hidden.Add(drone);}
            SceneManager.SetActiveScene(scene);cameraObject=new GameObject("Chapter_Photo_Camera");var camera=cameraObject.AddComponent<Camera>();
            camera.transform.position=position;camera.transform.LookAt(target);camera.fieldOfView=52;camera.nearClipPlane=.2f;camera.farClipPlane=1700;camera.cullingMask=~(1<<5);camera.allowHDR=false;
            cameraObject.AddComponent<Skybox>().material=RenderSettings.skybox;
            cameraObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>().renderShadows=true;
            rt=new RenderTexture(1920,1080,24,RenderTextureFormat.ARGB32);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
            texture=new Texture2D(1920,1080,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1920,1080),0,0);texture.Apply();
            Directory.CreateDirectory("Docs/CanyonCityUpgrade/Photos");string output="Docs/CanyonCityUpgrade/Photos/"+name+".png";File.WriteAllBytes(output,texture.EncodeToPNG());return output;
        }
        finally
        {RenderTexture.active=previous;if(cameraObject)UnityEngine.Object.DestroyImmediate(cameraObject);if(texture)UnityEngine.Object.DestroyImmediate(texture);if(rt)UnityEngine.Object.DestroyImmediate(rt);SceneManager.SetActiveScene(original);foreach(var root in hidden)root.SetActive(true);if(opened)EditorSceneManager.CloseScene(scene,true);}
    }
}
