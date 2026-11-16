using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

// Scene authoring only. Re-running uses the original backup, so terrain never accumulates edits.
public static class CanyonShelterRevision
{
    const string City="Assets/Scenes/GameStages/City/City_1.unity";
    const string Desert="Assets/Scenes/GameStages/Desert/Desert_1.unity";
    const string TerrainPath="Assets/Scenes/GameStages/Desert/Terrain_Desert_Day.asset";
    const string Output="Docs/CanyonShelterRevision/";
    static readonly List<Vector3[]> routes=new List<Vector3[]>();
    static readonly List<Vector3> terraces=new List<Vector3>();
    static Transform[] All(Scene scene)=>scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();
    static Bounds BoundsOf(Transform t)
    {var renderers=t.GetComponentsInChildren<Renderer>();var b=renderers[0].bounds;foreach(var r in renderers.Skip(1))b.Encapsulate(r.bounds);return b;}
    static void Remember(UnityEngine.Object o){EditorUtility.SetDirty(o);if(PrefabUtility.IsPartOfPrefabInstance(o))PrefabUtility.RecordPrefabInstancePropertyModifications(o);}
    static void Hide(Transform t){t.gameObject.SetActive(false);Remember(t.gameObject);}
    static void Place(Transform t,float x,float z,float y=.34f)
    {t.position=new Vector3(x,0,z);t.position+=Vector3.up*(y-BoundsOf(t).min.y);Remember(t);}

    [MenuItem("Tools/AI Defenders/Environment Expansion/12 Winding Shallow Canyon And Shelter Clearance")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play mode first.");
        foreach(var path in new[]{City,Desert}){var s=SceneManager.GetSceneByPath(path);if(s.isLoaded&&s.isDirty)throw new InvalidOperationException("Unsaved scene: "+path);}
        Directory.CreateDirectory(Output+"Before");
        foreach(var path in new[]{City,Desert,TerrainPath}){string backup=Output+"Before/"+Path.GetFileName(path);if(!File.Exists(backup))File.Copy(path,backup);}
        Edit(City,FixCity);Edit(Desert,FixDesert);AssetDatabase.SaveAssets();
    }
    static void Edit(string path,Action<Scene> work)
    {
        var s=SceneManager.GetSceneByPath(path);bool opened=!s.isLoaded;if(opened)s=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
        var original=SceneManager.GetActiveScene();SceneManager.SetActiveScene(s);
        try{work(s);EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);}
        finally{SceneManager.SetActiveScene(original);if(opened)EditorSceneManager.CloseScene(s,true);}
    }
    static void FixCity(Scene s)
    {
        string signature=EnvironmentExpansionBuilder.GameplaySignature(s);var all=All(s);
        foreach(var t in all.Where(t=>t.gameObject.activeInHierarchy))
        {
            // Legacy scenery was not part of the previous rearranged-object-only check.
            if(t.parent&&t.parent.name=="City_Extra_Decor")
            {
                if(t.name=="Planter_10")Place(t,-33,-78);
                if(t.name=="Planter_15")Place(t,33,-78);
                if(t.name=="Parked_Car_13")Place(t,-16,-54);
                if(t.name=="Parked_Car_14")Place(t,16,-54);
            }
            if(t.parent&&t.parent.name=="City_Emergency_Shelter_Upgrade")
            {
                if(t.name=="Watchtower")Place(t,-21,-72);
                if(t.name=="CrateStack")Place(t,19.5f,-112);
                if(t.name=="JerryCan")Place(t,23,-112);
            }
            if(t.parent&&t.parent.name=="City_Greenery_Upgrade"&&Mathf.Abs(t.position.x)<28&&t.position.z<-119&&t.position.z>-126)
                Place(t,t.position.x<0?-33:33,-129);
        }
        // This legacy mesh spans the whole city, so its bounds hide individual
        // traffic poles embedded in the shelter. Remove only its shelter triangles.
        var oldLights=all.First(t=>t.name=="Street_Lights_Signals_Checkpoint");
        var filter=oldLights.GetComponent<MeshFilter>();var mesh=UnityEngine.Object.Instantiate(filter.sharedMesh);var vertices=mesh.vertices;
        for(int sub=0;sub<mesh.subMeshCount;sub++)
        {
            var triangles=mesh.GetTriangles(sub);var kept=new List<int>();
            for(int i=0;i<triangles.Length;i+=3)
            {
                var center=oldLights.TransformPoint((vertices[triangles[i]]+vertices[triangles[i+1]]+vertices[triangles[i+2]])/3);
                if(Mathf.Abs(center.x)<29&&center.z>-127&&center.z<-59)continue;
                kept.Add(triangles[i]);kept.Add(triangles[i+1]);kept.Add(triangles[i+2]);
            }
            mesh.SetTriangles(kept,sub);
        }
        mesh.RecalculateBounds();mesh.name="City_Lights_Outside_Shelter";
        string meshPath="Assets/Art/StageDetails/City_Lights_Outside_Shelter.asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
        if(existing){EditorUtility.CopySerialized(mesh,existing);UnityEngine.Object.DestroyImmediate(mesh);Remember(existing);}else{existing=mesh;AssetDatabase.CreateAsset(mesh,meshPath);}
        filter.sharedMesh=existing;Remember(filter);
        if(signature!=EnvironmentExpansionBuilder.GameplaySignature(s))throw new InvalidOperationException("City gameplay changed.");
        ValidateCity(s);
    }
    public static void ValidateCity(Scene s)
    {
        var all=All(s);
        var units=all.Where(t=>t.gameObject.activeInHierarchy&&Mathf.Abs(t.position.x)<27&&t.position.z>-120&&t.position.z<-63&&t.GetComponent<Renderer>()&&
          (t.parent&&(t.parent.name=="Environment_Asset_Expansion"||t.parent.name=="City_Extra_Decor"||t.parent.name=="City_Emergency_Shelter_Upgrade")||t.name.StartsWith("Container_Residence_")))
          .Where(t=>t.name!="BarbedFence").ToList();
        units.Add(all.First(t=>t.name=="Shelter_GP_Enlarged"));
        var errors=new List<string>();
        for(int i=0;i<units.Count;i++)for(int j=i+1;j<units.Count;j++)
        {var a=BoundsOf(units[i]);var b=BoundsOf(units[j]);float dx=Mathf.Min(a.max.x,b.max.x)-Mathf.Max(a.min.x,b.min.x),dz=Mathf.Min(a.max.z,b.max.z)-Mathf.Max(a.min.z,b.min.z);if(dx>.04f&&dz>.04f)errors.Add(units[i].name+" / "+units[j].name+" overlap="+dx+","+dz);}
        File.WriteAllLines(Output+"City_clearance.txt",new[]{"Checked all loose shelter props, legacy planters/cars, containers, lights, sandbags, watchtower and GP: "+units.Count,"Overlapping footprints: "+errors.Count}.Concat(errors));
        if(errors.Count>0)throw new InvalidOperationException("Shelter overlap: "+string.Join(";",errors.ToArray()));
    }
    static Vector3[] Smooth(Vector3[] p)
    {
        var points=new List<Vector3>();
        for(int i=0;i<p.Length-1;i++)
        {
            Vector3 a=p[Mathf.Max(0,i-1)],b=p[i],c=p[i+1],d=p[Mathf.Min(p.Length-1,i+2)];int samples=Mathf.CeilToInt(Vector3.Distance(b,c)/3);
            for(int j=0;j<samples;j++){float t=(float)j/samples;points.Add(.5f*((2*b)+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t));}
        }
        points.Add(p.Last());return points.ToArray();
    }
    static Vector3 V(float x,float z)=>new Vector3(x,0,z);
    static float SmoothRange(float a,float b,float v)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,v));
    static float Distance(float x,float z)
    {
        float best=float.MaxValue;var q=new Vector2(x,z);
        foreach(var route in routes)for(int i=1;i<route.Length;i++)
        {var a=new Vector2(route[i-1].x,route[i-1].z);var v=new Vector2(route[i].x,route[i].z)-a;float t=Mathf.Clamp01(Vector2.Dot(q-a,v)/Mathf.Max(.001f,v.sqrMagnitude));best=Mathf.Min(best,Vector2.Distance(q,a+t*v));}return best;
    }
    static void FixDesert(Scene s)
    {
        var all=All(s);var terrain=all.Select(t=>t.GetComponent<Terrain>()).First(t=>t);var data=terrain.terrainData;var origin=terrain.transform.position;
        // Load immutable original data for blending outside the new valley.
        string temp="Assets/Art/StageDetails/Canyon_OriginalTerrain.asset";
        if(!AssetDatabase.LoadAssetAtPath<TerrainData>(temp)){File.Copy(Output+"Before/Terrain_Desert_Day.asset",temp);AssetDatabase.ImportAsset(temp);}
        var original=AssetDatabase.LoadAssetAtPath<TerrainData>(temp);
        routes.Clear();terraces.Clear();
        routes.Add(Smooth(new[]{V(-64,-104),V(-72,-80),V(-48,-62),V(-30,-40),V(-42,-18),V(-25,0),V(-9,16),V(-18,28),V(-4,36),V(0,42),V(.2f,58.9f)}));
        routes.Add(Smooth(new[]{V(76,-97),V(69,-77),V(46,-62),V(20,-54),V(9,-34),V(-4,-15),V(-25,0),V(-9,16),V(-18,28),V(-4,36),V(0,42),V(.2f,58.9f)}));
        foreach(var route in routes)
        foreach(int index in new[]{8,17,26,35})
        {if(index>=route.Length-1)continue;var center=route[index];if(center.z>25)continue;var forward=(route[index+1]-route[index-1]).normalized;var normal=new Vector3(-forward.z,0,forward.x);foreach(int side in new[]{-1,1}){var p=center+normal*side*15;if(Distance(p.x,p.z)>12)terraces.Add(p);}}
        int n=data.heightmapResolution;var heights=original.GetHeights(0,0,n,n);var size=data.size;
        for(int j=0;j<n;j++)for(int i=0;i<n;i++)
        {
            float x=origin.x+i*size.x/(n-1),z=origin.z+j*size.z/(n-1);if(x<-125||x>125||z<-145||z>50)continue;
            if(Mathf.Abs(x)<34&&z>=38)continue;
            float distance=Distance(x,z),floor=.4f*(1-SmoothRange(20,40,z));
            float edge=SmoothRange(0,20,Mathf.Min(x+125,125-x,z+145,50-z));
            float irregular=(Mathf.PerlinNoise(x*.05f+14,z*.05f+31)-.5f)*.85f;
            float bank=6.1f*SmoothRange(5.5f,12.2f,distance+irregular)*(1-SmoothRange(28,40,z));
            float y=floor+bank;
            y+=(Mathf.PerlinNoise(x*.08f+20,z*.08f+9)-.5f)*.35f*SmoothRange(7,13,distance);
            float old=origin.y+heights[j,i]*size.y;
            y=Mathf.Lerp(old,y,edge*(1-SmoothRange(29,53,distance)));
            foreach(var terrace in terraces){float r=Vector2.Distance(new Vector2(x,z),new Vector2(terrace.x,terrace.z));if(r<6.5f)y=Mathf.Lerp(y,6.5f,1-SmoothRange(4,6.5f,r));}
            heights[j,i]=(y-origin.y)/size.y;
        }
        data.SetHeights(0,0,heights);EditorUtility.SetDirty(data);terrain.Flush();
        var oldRoot=s.GetRootGameObjects().FirstOrDefault(g=>g.name=="Desert_Shallow_Canyon");if(oldRoot)UnityEngine.Object.DestroyImmediate(oldRoot);
        all=All(s);
        var root=new GameObject("Desert_Shallow_Canyon").transform;
        var angular=s.GetRootGameObjects().FirstOrDefault(g=>g.name=="Desert_Grand_Canyon");if(angular)Hide(angular.transform);
        var spawners=all.Select(t=>t.GetComponent<EnemySpawner>()).Where(t=>t).OrderBy(t=>t.transform.position.x).ToArray();
        for(int k=0;k<2;k++)
        {
            var route=routes[k];var routeObject=new GameObject(k==0?"Canyon_West_Winding":"Canyon_East_Winding");routeObject.transform.SetParent(root);var path=routeObject.AddComponent<EnemyPath>();
            path.waypointRadius=2.4f;path.waypoints=new Transform[route.Length];
            for(int i=0;i<route.Length;i++){var p=route[i];p.y=terrain.SampleHeight(p)+origin.y+.05f;route[i]=p;var waypoint=new GameObject("Point_"+i.ToString("D2")).transform;waypoint.SetParent(routeObject.transform);waypoint.position=p;path.waypoints[i]=waypoint;}
            var spawner=spawners[k];spawner.transform.position=route[0]+Vector3.up*.15f;spawner.paths=new[]{path};spawner.spawnRange=2.3f;spawner.pickNearestPath=true;Remember(spawner.transform);Remember(spawner);
        }
        // Inactive old paths cannot be selected accidentally, but remain available in backups.
        foreach(var path in all.Select(t=>t.GetComponent<EnemyPath>()).Where(t=>t))Hide(path.transform);
        int number=0;foreach(var p in terraces){var marker=new GameObject("Natural_Firing_Terrace_"+(++number)).transform;marker.SetParent(root);marker.position=new Vector3(p.x,terrain.SampleHeight(p)+origin.y,p.z);}
        RegroundScenery(s,terrain);
        PaintFloor(data,origin);
        var drone=all.First(t=>t.name=="CCTVSystem");drone.position=new Vector3(9,33,49);drone.rotation=Quaternion.Euler(44,190,0);Remember(drone);
        foreach(var camera in drone.GetComponentsInChildren<Camera>(true)){camera.transform.localRotation=Quaternion.identity;Remember(camera.transform);}
        Physics.SyncTransforms();ValidateDesert(s);
    }
    static void RegroundScenery(Scene s,Terrain terrain)
    {
        foreach(var group in All(s).Where(t=>t.name=="Scattered_Sandstone"||t.name=="Sandstone_Outcrops"||t.name=="Desert_Extra_Decor"||t.name=="Environment_Asset_Expansion"||t.name=="Desert_Cactus_Upgrade"))
        foreach(Transform t in group)
        {
            if(!t.gameObject.activeInHierarchy||t.GetComponentsInChildren<Renderer>().Length==0)continue;
            var p=t.position;if(Mathf.Abs(p.x)<34&&p.z>38&&p.z<101)continue;
            if(p.x<-135||p.x>135||p.z<-150||p.z>60)continue;
            float distance=Distance(p.x,p.z);
            // Keep the canyon floor and firing terraces clear of decorative obstacles.
            if(distance<13||terraces.Any(q=>Vector2.Distance(new Vector2(q.x,q.z),new Vector2(p.x,p.z))<7)){Hide(t);continue;}
            float floor=terrain.SampleHeight(p)+terrain.transform.position.y;
            var bounds=BoundsOf(t);
            if(t.name.Contains("Rock")||t.name.Contains("Boulder")||t.name.Contains("Mesa"))
                foreach(float dx in new[]{-.4f,.4f})foreach(float dz in new[]{-.4f,.4f})
                {var q=bounds.center+new Vector3(dx*bounds.size.x,0,dz*bounds.size.z);floor=Mathf.Min(floor,terrain.SampleHeight(q)+terrain.transform.position.y);}
            Place(t,p.x,p.z,floor-.12f);
        }
    }
    static void PaintFloor(TerrainData data,Vector3 origin)
    {
        int n=data.alphamapResolution;var alpha=data.GetAlphamaps(0,0,n,n);int layers=alpha.GetLength(2);
        for(int j=0;j<n;j++)for(int i=0;i<n;i++)
        {
            float x=origin.x+i*data.size.x/(n-1),z=origin.z+j*data.size.z/(n-1);if(x<-120||x>120||z<-135||z>38)continue;
            float d=Distance(x,z);if(d>28)continue;float floor=1-SmoothRange(4,7,d);
            for(int l=0;l<layers;l++)alpha[j,i,l]=l==0?1-floor:l==1?floor:0;
        }
        data.SetAlphamaps(0,0,alpha);EditorUtility.SetDirty(data);
    }
    public static void ValidateDesert(Scene s)
    {
        var all=All(s);var terrain=all.Select(t=>t.GetComponent<Terrain>()).First(t=>t);var spawners=all.Select(t=>t.GetComponent<EnemySpawner>()).Where(t=>t).ToArray();
        var report=new List<string>();int errors=0;float minCoverage=999;
        foreach(var sp in spawners)
        foreach(var path in sp.paths)
        {
            var centers=path.GetWorldPoints();float length=0,direct=Vector3.Distance(centers.First(),centers.Last());
            for(int i=1;i<centers.Length;i++)length+=Vector3.Distance(centers[i-1],centers[i]);
            report.Add(sp.Id+": waypoints="+centers.Length+", route="+length.ToString("F1")+"m, direct="+direct.ToString("F1")+"m");
            foreach(float lane in new[]{-path.waypointRadius,0,path.waypointRadius})
            {var points=path.GetLaneOffsetPoints(lane);for(int i=1;i<points.Length;i++)for(int step=0;step<=4;step++){var p=Vector3.Lerp(points[i-1],points[i],step/4f);float y=terrain.SampleHeight(p)+terrain.transform.position.y;if(y>p.y+.55f)errors++;}}
        }
        var marks=all.Where(t=>t.name.StartsWith("Natural_Firing_Terrace_")).ToArray();
        foreach(var marker in marks)
        {
            Vector3 closest=spawners.SelectMany(sp=>sp.paths).SelectMany(p=>p.GetWorldPoints()).OrderBy(p=>(new Vector2(p.x-marker.position.x,p.z-marker.position.z)).sqrMagnitude).First();
            float distance=Vector3.Distance(marker.position+Vector3.up*1.5f,closest+Vector3.up);minCoverage=Mathf.Min(minCoverage,distance);
            var local=marker.position-terrain.transform.position;float slope=terrain.terrainData.GetSteepness(local.x/terrain.terrainData.size.x,local.z/terrain.terrainData.size.z);
            RaycastHit hit;bool collider=Physics.Raycast(marker.position+Vector3.up*3,Vector3.down,out hit,6,1<<terrain.gameObject.layer);
            bool blocked=Physics.Linecast(marker.position+Vector3.up*2.2f,closest+Vector3.up*1.1f,1<<terrain.gameObject.layer,QueryTriggerInteraction.Ignore);
            report.Add(marker.name+": range="+distance.ToString("F1")+"m, height advantage="+(marker.position.y-closest.y).ToString("F1")+"m, slope="+slope.ToString("F1")+", ground collider="+collider+", firing line clear="+!blocked);
            if(distance>24||slope>8||!collider||blocked)errors++;
        }
        report.Add("Validation errors: "+errors);File.WriteAllLines(Output+"Desert_validation.txt",report);
        if(errors>0)throw new InvalidOperationException("Canyon validation errors: "+errors);
    }
}
