using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class TurretTierModelVerification
{
    const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    const string Output="Docs/TurretTierConnection/";
    public static void Verify()
    {
        var report=new List<string>();
        foreach(var file in TurretTierModelConnection.Files.Concat(new[]{"Turret_Test MG"}))
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Turrets/Turret_Test/"+file+".prefab");
            var instance=UnityEngine.Object.Instantiate(prefab);instance.name="TierVerification";
            try
            {
                var turret=instance.GetComponent<TurretController>();turret.enabled=false;turret.weapon.enabled=false;
                turret.constructStart=turret.constructComplete=turret.upgradeStart=turret.upgradeComplete=null;
                turret.BeginConstruction(0,true,null);
                for(int level=1;level<=3;level++)
                {
                    if(level>1)
                    {
                        turret.BeginUpgrade(false);
                        Check(turret.IsUpgrading,"Timed upgrade did not begin: "+file);
                        typeof(TurretController).GetField("upgradeElapsed",Private).SetValue(turret,1000f);
                        typeof(TurretController).GetMethod("TickUpgrade",Private).Invoke(turret,null);
                    }
                    VerifyLevel(turret,level);report.Add(file+" timed level "+level+": model, muzzle, aiming, stats and collider PASS");
                }
                Check(!turret.CanUpgrade(),"Level 3 must be final");
                // Stored-turret restoration and lab previews both use this construction path.
                turret.upgradeLevel=2;turret.BeginConstruction(0,true,null);VerifyLevel(turret,2);
                turret.upgradeLevel=3;turret.BeginConstruction(0,true,null);VerifyLevel(turret,3);
                report.Add(file+" restored levels 2/3: PASS");
            }
            finally{UnityEngine.Object.DestroyImmediate(instance);}
        }
        File.WriteAllLines(Output+"Upgrade_checks.txt",report);
    }
    static void Check(bool value,string message){if(!value)throw new InvalidOperationException(message);}
    static void VerifyLevel(TurretController turret,int level)
    {
        Check(turret.upgradeLevel==level&&turret.IsReady,"Wrong completed level");
        var tier=turret.upgradeTiers[level-1];
        Check(turret.upgradeTiers.Count(t=>t.visualRoot.activeSelf)==1&&tier.visualRoot.activeSelf,"Overlapping tier models");
        Check(turret.firePoint==tier.firePoint&&turret.firePoint.IsChildOf(tier.barrel),"Wrong muzzle for tier");
        Check((Transform)typeof(TurretController).GetField("aimPivot",Private).GetValue(turret)==tier.barrel,"Wrong aiming pivot");
        Check(Vector3.Dot(tier.barrel.forward,tier.firePoint.forward)>.999f,"Muzzle direction mismatch");
        Check(tier.visualRoot.GetComponentsInChildren<Collider>().Length>=2,"Missing selection colliders");
        Check(Mathf.Approximately(turret.weapon.fireDelay,tier.fireDelay)&&turret.weapon.maxAmmo==tier.maxAmmo,"Weapon stats not switched");
        Check(Mathf.Approximately((float)typeof(WeaponSystem).GetField("damageScale",Private).GetValue(turret.weapon),tier.damageScale),"Damage scaling not switched");
        var renderers=tier.visualRoot.GetComponentsInChildren<Renderer>();Check(renderers.Length>2,"Missing model mesh groups");
        var bounds=renderers[0].bounds;foreach(var r in renderers.Skip(1))bounds.Encapsulate(r.bounds);
        Check(bounds.size.y>1.8f&&bounds.size.y<5f,"Unexpected model scale "+bounds.size);
        // Both head and muzzle must follow rotation while the base stays fixed.
        var baseRoot=tier.visualRoot.transform.Find("Base");var basePosition=baseRoot.position;var rotation=tier.barrel.localRotation;
        tier.barrel.localRotation=Quaternion.Euler(-15,40,0);Check(baseRoot.position==basePosition,"Base moved during aiming");
        Check(Vector3.Dot(tier.barrel.forward,tier.firePoint.forward)>.999f,"Muzzle does not follow aiming");tier.barrel.localRotation=rotation;
    }
    public static void CaptureModels()
    {
        Directory.CreateDirectory(Output+"Previews");var sheet=new Texture2D(1200,1500,TextureFormat.RGB24,false);
        try
        {
            for(int type=0;type<5;type++)for(int level=1;level<=3;level++)
            {
                var preview=new PreviewRenderUtility();Texture2D texture=null;
                try
                {
                    var model=UnityEngine.Object.Instantiate(TurretTierModelConnection.Prefab(type));preview.AddSingleGO(model);
                    var turret=model.GetComponent<TurretController>();turret.enabled=false;turret.weapon.enabled=false;
                    foreach(var t in turret.upgradeTiers)t.visualRoot.SetActive(t==turret.upgradeTiers[level-1]);
                    var renderers=model.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers.Skip(1))bounds.Encapsulate(r.bounds);
                    var camera=preview.camera;camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.12f,.19f,.25f);
                    camera.transform.position=bounds.center+new Vector3(5,3.5f,7).normalized*15;camera.transform.LookAt(bounds.center);
                    camera.orthographicSize=2.35f;camera.nearClipPlane=.01f;camera.farClipPlane=100;
                    preview.ambientColor=Color.gray;preview.lights[0].intensity=1.3f;preview.lights[0].transform.rotation=Quaternion.Euler(35,210,0);preview.lights[1].intensity=.8f;
                    preview.BeginStaticPreview(new Rect(0,0,400,300));preview.Render(true);texture=preview.EndStaticPreview();
                    File.WriteAllBytes(Output+"Previews/"+TurretTierModelConnection.Types[type]+level+".png",texture.EncodeToPNG());
                    sheet.SetPixels((level-1)*400,(4-type)*300,400,300,texture.GetPixels());
                }
                finally{if(texture)UnityEngine.Object.DestroyImmediate(texture);preview.Cleanup();}
            }
            sheet.Apply();File.WriteAllBytes(Output+"All15Models.png",sheet.EncodeToPNG());
        }
        finally{UnityEngine.Object.DestroyImmediate(sheet);}
    }
}
