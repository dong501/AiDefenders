using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;

public static class CampLayoutCleanup
{
    const string Art="Assets/Art/StageDetails/";
    const string Props="Assets/Art/EnvironmentExpansion/Generated/";
    const string City="Assets/Scenes/GameStages/City/City_1.unity";
    const string Desert="Assets/Scenes/GameStages/Desert/Desert_1.unity";
    static Transform root;
    static readonly List<Transform> placed=new List<Transform>();
    static Transform[] All(Scene s)=>s.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
    static void Hide(Transform t){t.gameObject.SetActive(false);if(PrefabUtility.IsPartOfPrefabInstance(t))PrefabUtility.RecordPrefabInstancePropertyModifications(t.gameObject);}
    static Bounds BoundsOf(Transform t)
    {var rs=t.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs.Skip(1))b.Encapsulate(r.bounds);return b;}
    static void Put(Transform t,float x,float z,float floor,float? scale=null,float? yaw=null)
    {
        if(scale.HasValue)t.localScale=Vector3.one*scale.Value;
        if(yaw.HasValue)t.rotation=Quaternion.Euler(0,yaw.Value,0);
        t.position=new Vector3(x,0,z);t.position+=Vector3.up*(floor-BoundsOf(t).min.y);
        if(PrefabUtility.IsPartOfPrefabInstance(t))PrefabUtility.RecordPrefabInstancePropertyModifications(t);
        placed.Add(t);
    }
    [MenuItem("Tools/AI Defenders/Environment Expansion/11 Organize Shelter And Supply Base")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop play mode first.");
        foreach(var path in new[]{City,Desert}){var s=SceneManager.GetSceneByPath(path);if(s.isLoaded&&s.isDirty)throw new InvalidOperationException("Save edits first: "+path);}
        Directory.CreateDirectory("Docs/CampLayoutCleanup/Before");
        foreach(var path in new[]{City,Desert}){string dest="Docs/CampLayoutCleanup/Before/"+Path.GetFileName(path);if(!File.Exists(dest))File.Copy(path,dest);}
        ImportCampProps();var original=SceneManager.GetActiveScene();
        foreach(var path in new[]{City,Desert})
        {
            var s=SceneManager.GetSceneByPath(path);bool opened=!s.isLoaded;if(opened)s=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
            SceneManager.SetActiveScene(s);
            try
            {
                string signature=EnvironmentExpansionBuilder.GameplaySignature(s);
                var old=s.GetRootGameObjects().FirstOrDefault(g=>g.name=="Camp_Layout_Cleanup");if(old)UnityEngine.Object.DestroyImmediate(old);
                var all=All(s);root=new GameObject("Camp_Layout_Cleanup").transform;placed.Clear();
                if(path==City)CityLayout(all);else DesertLayout(all);
                if(signature!=EnvironmentExpansionBuilder.GameplaySignature(s))throw new InvalidOperationException("Gameplay layout changed.");
                var overlaps=new List<string>();
                for(int i=0;i<placed.Count;i++)for(int j=i+1;j<placed.Count;j++)
                {
                    var a=BoundsOf(placed[i]);var b=BoundsOf(placed[j]);
                    float dx=Mathf.Min(a.max.x,b.max.x)-Mathf.Max(a.min.x,b.min.x),dz=Mathf.Min(a.max.z,b.max.z)-Mathf.Max(a.min.z,b.min.z);
                    if(dx>.08f&&dz>.08f)overlaps.Add(placed[i].name+" / "+placed[j].name+" : "+dx+", "+dz);
                }
                File.WriteAllLines("Docs/CampLayoutCleanup/"+s.name+"_overlap_check.txt",overlaps.Count==0?new[]{"No footprint overlaps among arranged objects."}:overlaps.ToArray());
                Debug.Log(s.name+": arranged "+placed.Count+" objects; footprint overlaps: "+overlaps.Count);
                EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);
            }
            finally{SceneManager.SetActiveScene(original);if(opened)EditorSceneManager.CloseScene(s,true);}
        }
        AssetDatabase.SaveAssets();
    }
    static void CityLayout(Transform[] all)
    {
        // The old combined shelter mesh includes buildings, furniture and signs.
        // Retire that legacy set so the individually placed facilities have real space.
        var legacy=all.First(t=>t.name=="Evacuation_Shelter");Hide(legacy);
        foreach(var t in all.Where(t=>Mathf.Abs(t.position.x)<27&&t.position.z>-125&&t.position.z<-60))
        {
            var text=t.GetComponent<TMP_Text>();
            if(text&&!text.text.Contains("대피소"))Hide(t);
            if(t.name.StartsWith("Triage")||t.name=="GP_Command_Sign"||t.name=="CheckpointSign"||t.name=="GuardBooth")Hide(t);
        }
        var emergency=all.First(t=>t.name=="City_Emergency_Shelter_Upgrade");
        foreach(var t in all.Where(t=>t.parent==emergency&&t.name=="FieldTent"))Put(t,-15.5f,-113,.34f,1,180);
        foreach(var t in all.Where(t=>t.name.StartsWith("MedicalTent_Cross_")))Hide(t);
        // Vehicles get separate bays rather than sharing space with medical/supply tents.
        var ambulances=all.Where(t=>t.parent==emergency&&t.name=="Ambulance").OrderBy(t=>t.position.z).ToArray();
        Put(ambulances[0],15,-113,.34f,1.4f,180);Put(ambulances[1],20,-86,.34f,1.4f,180);
        var police=all.Where(t=>t.name=="PoliceCar"&&Mathf.Abs(t.position.x)<27&&t.position.z>-125&&t.position.z<-60&&t.gameObject.activeInHierarchy).OrderBy(t=>t.position.z).ToArray();
        Put(police[0],-21,-111,.34f,1.1f,180);Put(police[1],-21,-86,.34f,1.1f,180);
        foreach(var t in all.Where(t=>t.name.StartsWith("Container_Residence_")&&t.gameObject.activeInHierarchy))
        {float x=t.position.x<0?-13:13;Put(t,x,-74,.34f);}
        int table=0;
        foreach(var t in all.Where(t=>t.parent&&t.parent.name=="Environment_Asset_Expansion"&&t.gameObject.activeInHierarchy&&Mathf.Abs(t.position.x)<27&&t.position.z>-120&&t.position.z<-63))
        {
            if(t.name=="ParkPicnicTable")Put(t,table++==0?-12:12,-81.5f,.34f,1.25f);
            else if(t.name=="SupplyCrate")Put(t,22,-73,.34f,1.3f);
            else if(t.name=="DrumPallet")Put(t,22,-78,.34f,1.1f);
            else if(t.name=="ParkSlattedBench")Put(t,-17,-81.5f,.34f,1.2f,0);
            else if(t.name=="ParkCoveredBin")Put(t,-18.5f,-79,.34f,1.1f);
        }
        int pallet=0;
        foreach(var t in emergency.Cast<Transform>().Where(t=>t.gameObject.activeSelf))
        {
            if(t.name=="BaseSupplyPallet")Put(t,pallet++==0?13:16,-88,.34f,1.1f);
            else if(t.name=="CrateStack")Put(t,21,-116,.34f,1.1f);
            else if(t.name=="JerryCan")Put(t,23,-116,.34f,1.3f);
            else if(t.name=="BaseFieldGenerator")Put(t,-21,-79,.34f,1.2f,90);
            else if(t.name=="Watchtower")Put(t,-22,-68.5f,.34f,1);
        }
        // Preserve the supplied red shelter plaque on the GP wall.
    }
    static void DesertLayout(Transform[] all)
    {
        var terrain=all.Select(t=>t.GetComponent<Terrain>()).First(t=>t);
        var expansion=all.First(t=>t.name=="Desert_Military_Base_Upgrade");
        foreach(var t in expansion.Cast<Transform>().Where(t=>t.name=="Expanded_Hesco"))
        {
            // 1.3m-high thin panels become a substantial two-tier perimeter.
            var scale=t.localScale;scale.x=1;scale.y=1.9f;scale.z=2;t.localScale=scale;
            var p=t.position;Put(t,p.x,p.z,0);placed.Remove(t);
        }
        var tanks=expansion.Cast<Transform>().Where(t=>t.name=="Base_DesertTank").OrderBy(t=>t.position.x).ToArray();
        Put(tanks[0],-20,54,0,null,165);Put(tanks[1],20,54,0,null,195);
        foreach(var t in all.Where(t=>t.gameObject.activeInHierarchy))
        {
            if(t.name=="Defense_Supply_Camp")Put(t,0,85,0);
            else if(t.name=="Defense_Jeep")Put(t,-12,65,0,null,155);
            else if(t.name=="Defense_Solar")Put(t,-23,92,0);
            else if(t.name=="Defense_Watchtower")Put(t,-25,83,0);
            else if(t.name=="Defense_WaterTank")Put(t,25,92,0);
            else if(t.name=="Outpost_Palm")Hide(t);
            else if(t.parent==expansion&&t.name=="BaseFieldGenerator")Put(t,-24,77,0);
            else if(t.parent==expansion&&t.name=="BaseSupplyPallet")Put(t,23,78,0);
            else if(t.parent&&t.parent.name=="Environment_Asset_Expansion"&&Mathf.Abs(t.position.x)<29&&t.position.z>41&&t.position.z<98)
            {
                if(t.name=="SupplyCrate")Hide(t);
                else if(t.name=="DrumPallet")Put(t,23,84,0,1.4f);
                else if(t.name=="DesertClayJars")Hide(t);
            }
        }
        Truck(-18,70,90);Truck(18,70,-90);
        CampProp("SupportPrefabFieldOffice",-17,86,8,0);
        CampProp("BaseSatelliteDish",-11,94,3.8f,25);
        CampProp("BaseRadioRack",-12,78,1.7f,180);
        CampProp("BaseFuelTank",23,87,3,90);
        CampProp("PowerCabinet",-25,73,1.3f,90);
        CampProp("CableReel",-20,80,1.7f,0);
        CampProp("BaseLockerBank",12,89,3.2f,90);
        Prop("CrateStack",20,79,1.2f);
        Prop("SupplyCrate",17,79,1.6f);
        Prop("JerryCan",25,81,1.5f);
        Prop("BaseSupplyPallet",14,77,1.5f);
        // Re-ground every independent rock/prop affected by the enlarged compound.
        // Rocks inside the fence are removed; outside rocks sit on the actual terrain.
        foreach(var t in all.Where(t=>t.gameObject.activeInHierarchy&&t.parent&&(t.parent.name=="Scattered_Sandstone"||t.parent.name=="Sandstone_Outcrops"||t.parent.name=="Desert_Cactus_Upgrade"||t.name.StartsWith("Rock_Cluster_")||t.name.StartsWith("Dry_Shrub_"))))
        {
            var p=t.position;
            if(t.parent.name=="Desert_Cactus_Upgrade"&&Mathf.Abs(p.x)<46&&p.z>35&&p.z<116)
            {
                // Keep upright cacti on the rim rather than halfway down a steep cut.
                if(p.x<-33)p.x=-50;
                else if(p.x>33)p.x=50;
                else p.z=122;
            }
            if(Mathf.Abs(p.x)<33&&p.z>38&&p.z<100)Hide(t);
            else
            {
                var b=BoundsOf(t);float floor=terrain.SampleHeight(b.center)+terrain.transform.position.y;
                foreach(float dx in new[]{-.35f,.35f})foreach(float dz in new[]{-.35f,.35f})
                {var q=b.center+new Vector3(dx*b.size.x,0,dz*b.size.z);floor=Mathf.Min(floor,terrain.SampleHeight(q)+terrain.transform.position.y);}
                Put(t,p.x,p.z,floor-.2f);placed.Remove(t);
            }
        }
    }
    static void Truck(float x,float z,float yaw)
    {
        var g=Prop("CityBoxTruck",x,z,1.45f,yaw);g.name="Military_Supply_Truck";
        var mats=g.GetComponent<Renderer>().sharedMaterials;
        for(int i=0;i<mats.Length;i++)
        {
            var c=mats[i].color;if((c.r>.65f&&c.g>.65f&&c.b>.6f)||i==3||i==7||i==8)mats[i]=StageDetailPolish.Mat("Army_Truck_Olive_"+i,i%2==0?"#596248":"#72755B");
        }
        g.GetComponent<Renderer>().sharedMaterials=mats;
        PrefabUtility.RecordPrefabInstancePropertyModifications(g.GetComponent<Renderer>());
        PrefabUtility.RecordPrefabInstancePropertyModifications(g);
    }
    static GameObject Prop(string name,float x,float z,float scale,float yaw=0)
    {
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Props+name+".prefab");
        var g=(GameObject)PrefabUtility.InstantiatePrefab(prefab,root);Put(g.transform,x,z,0,scale,yaw);return g;
    }
    static void CampProp(string name,float x,float z,float width,float yaw)
    {
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Art+name+".prefab");var mesh=prefab.GetComponent<MeshFilter>().sharedMesh;
        var g=(GameObject)PrefabUtility.InstantiatePrefab(prefab,root);Put(g.transform,x,z,0,width/Mathf.Max(mesh.bounds.size.x,mesh.bounds.size.z),yaw);
    }
    static void ImportCampProps()
    {
        var data=JsonUtility.FromJson<EnvironmentExpansionBuilder.Model>(File.ReadAllText(Art+"CampPropsSource.json"));
        foreach(var item in data.assets)
        {
            if(AssetDatabase.LoadAssetAtPath<GameObject>(Art+item.name+".prefab"))continue;
            var vertices=new List<Vector3>();var groups=new Dictionary<string,List<int>>();
            foreach(var part in item.parts)
            {
                if(!groups.ContainsKey(part.color))groups[part.color]=new List<int>();
                int offset=vertices.Count;vertices.AddRange(part.vertices);groups[part.color].AddRange(part.triangles.Select(i=>i+offset));
            }
            var mesh=new Mesh();mesh.name=item.name;mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;mesh.SetVertices(vertices);mesh.subMeshCount=groups.Count;
            int sub=0;var materials=new List<Material>();foreach(var g in groups){mesh.SetTriangles(g.Value,sub++);materials.Add(StageDetailPolish.Mat("Camp_Imported_"+g.Key.TrimStart('#'),g.Key));}
            mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,Art+item.name+".asset");
            var go=new GameObject(item.name);go.layer=2;go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterials=materials.ToArray();
            PrefabUtility.SaveAsPrefabAsset(go,Art+item.name+".prefab");UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
