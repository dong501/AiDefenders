using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public static class CityRoofVehicleRepair
{
    const string Path="Assets/Scenes/GameStages/City/City_1.unity";
    static readonly HashSet<string> Vehicles=new HashSet<string>{"CitySedan","CitySUV","CityMini","CityHatchback","CityTaxi","CityMinivan","CityWagon","CityCoupe","CityPickup","DeliveryVan","CityBoxTruck"};
    static Bounds BoundsOf(Transform t)
    {
        var rr=t.GetComponentsInChildren<Renderer>().Where(r=>r.gameObject.activeInHierarchy).ToArray();
        var b=rr[0].bounds;foreach(var r in rr.Skip(1))b.Encapsulate(r.bounds);return b;
    }
    static void Record(Transform t)
    {EditorUtility.SetDirty(t);if(PrefabUtility.IsPartOfPrefabInstance(t))PrefabUtility.RecordPrefabInstancePropertyModifications(t);}
    [MenuItem("Tools/AI Defenders/City/Align Roofs And Park Vehicles")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play first");
        var s=SceneManager.GetSceneByPath(Path);bool opened=!s.isLoaded;
        if(!opened&&s.isDirty)throw new InvalidOperationException("Save city edits first");
        Directory.CreateDirectory("Docs/CityRoofVehicleRepair/Before");
        if(!File.Exists("Docs/CityRoofVehicleRepair/Before/City_1.unity"))File.Copy(Path,"Docs/CityRoofVehicleRepair/Before/City_1.unity");
        if(opened)s=EditorSceneManager.OpenScene(Path,OpenSceneMode.Additive);
        try
        {
            string sig=EnvironmentExpansionBuilder.GameplaySignature(s);var roots=s.GetRootGameObjects();
            var urban=roots.First(g=>g.name=="City_Urban_Refinement").transform;int roofs=0,cars=0;
            foreach(var body in urban.Cast<Transform>().Where(t=>t.name.StartsWith("Urban_Block_")||t.name.StartsWith("Skyline_")).ToArray())
            {
                if(body.name.Contains("_Roof"))continue;
                var lid=urban.Cast<Transform>().FirstOrDefault(t=>t.name==body.name+"_Roof");if(!lid)continue;
                var delta=new Vector3(body.position.x-lid.position.x,0,body.position.z-lid.position.z);
                foreach(var part in urban.Cast<Transform>().Where(t=>t.name.StartsWith(body.name+"_Roof",StringComparison.Ordinal)).ToArray())
                {part.position+=delta;part.SetParent(body,true);Record(part);roofs++;}
            }
            var street=roots.First(g=>g.name=="City_FourWay_Streetscape").transform;
            foreach(var t in street.Cast<Transform>().Where(t=>t.gameObject.activeInHierarchy&&Vehicles.Contains(t.name)).ToArray())
            {
                var p=t.position;
                // The shelter's dedicated emergency parking bays are handled separately.
                if(Mathf.Abs(p.x)<27&&p.z>-125&&p.z<-60)continue;
                float ns=Mathf.Round(p.x/100)*100,ew=Mathf.Round((p.z+100)/80)*80-100;
                bool vertical=Mathf.Abs(p.x-ns)<Mathf.Abs(p.z-ew);
                float yaw;
                if(vertical)
                {
                    float side=p.x<ns?-1:1;p.x=ns+side*6.2f;yaw=side>0?180:0;
                    float nearest=Mathf.Round((p.z+100)/80)*80-100;
                    float distance=p.z-nearest;
                    if(Mathf.Abs(distance)<24)p.z=nearest+(distance>=0?24:-24);
                }
                else
                {
                    float side=p.z<ew?-1:1;p.z=ew+side*6.2f;yaw=side>0?90:270;
                    float nearest=Mathf.Round(p.x/100)*100,distance=p.x-nearest;
                    if(Mathf.Abs(distance)<25)p.x=nearest+(distance>=0?25:-25);
                }
                t.rotation=Quaternion.Euler(0,yaw,0);t.position=p;
                var b=BoundsOf(t);t.position+=Vector3.up*(.045f-b.min.y);Record(t);cars++;
            }
            if(sig!=EnvironmentExpansionBuilder.GameplaySignature(s))throw new InvalidOperationException("Gameplay changed");
            EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);AssetDatabase.SaveAssets();
            File.WriteAllText("Docs/CityRoofVehicleRepair/Changes.txt","Roof components attached and realigned: "+roofs+". Road vehicles aligned and grounded: "+cars+". Gameplay signature preserved.");
        }
        finally{if(opened)EditorSceneManager.CloseScene(s,true);}
    }
}
