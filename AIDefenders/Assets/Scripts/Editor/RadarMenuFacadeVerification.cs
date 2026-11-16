using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public static class RadarMenuFacadeVerification
{
    const string Output="Docs/RadarMenuFacadeUpgrade/";
    public static void VerifyLayout()
    {
        var report=new List<string>();
        foreach(var resolution in new[]{new Vector2(1920,1080),new Vector2(1280,720),new Vector2(1024,768),new Vector2(960,600)})
        foreach(int mode in new[]{0,1,2})
        {
            var canvas=new Rect(0,0,resolution.x,resolution.y);var panels=new List<Rect>();
            panels.Add(new Rect(resolution.x-384,resolution.y-176,360,52));
            if(mode>0)panels.Add(new Rect(resolution.x-384,168,360,resolution.y-358));
            if(mode>1)panels.Add(new Rect(resolution.x-600,resolution.y-450,340,220));
            var radar=HudRadarLayout.Resolve(canvas,240,24,panels);
            if(!canvas.Contains(radar.min)||!canvas.Contains(radar.max)||panels.Any(r=>r.Overlaps(radar)))throw new InvalidOperationException("Radar overlaps at "+resolution+" mode "+mode);
            report.Add(resolution+" mode="+mode+": "+radar+" clear");
        }
        File.WriteAllLines(Output+"Radar_layout_checks.txt",report);
    }
    public static void CaptureUi(string mode)
    {
        Texture2D result=null;HudWorldOverlay overlay=null;GameObject root=null,cameraRoot=null;RenderTexture target=null;var previous=RenderTexture.active;
        try
        {
            cameraRoot=new GameObject("UiReviewCamera");var camera=cameraRoot.AddComponent<Camera>();camera.enabled=false;target=new RenderTexture(1920,1080,24);camera.targetTexture=target;camera.orthographic=true;camera.orthographicSize=540;camera.transform.position=new Vector3(20000,20000,-10);camera.transform.rotation=Quaternion.identity;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.07f,.13f,.18f);camera.nearClipPlane=.01f;camera.farClipPlane=1000;camera.allowHDR=false;camera.cullingMask=~0;
            root=new GameObject("ReviewCanvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));root.transform.position=new Vector3(20000,20000,0);
            var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=camera;((RectTransform)root.transform).sizeDelta=new Vector2(1920,1080);
            var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
            if(mode=="Title")TitleScreenUi.Create(root.transform,()=>{});
            else if(mode=="MainMenu")
            {var manager=root.AddComponent<MenuManager_MainMenu>();typeof(MenuManager_MainMenu).GetMethod("BuildStyledMainMenu",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(manager,null);}
            else
            {
                var components=new GameObject("PreviewComponents");components.transform.SetParent(root.transform,false);
                var placer=components.AddComponent<TurretPlatformPlacer>();placer.turretOptions=new TurretBuildOption[0];
                var wave=components.AddComponent<WaveDirector>();typeof(WaveDirector).GetProperty("Phase").SetValue(wave,WavePhase.Combat);
                var radar=components.AddComponent<CCTVRadar>();radar.size=240;typeof(CCTVRadar).GetField("waveDirector",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(radar,wave);
                var build=new HudBuildUi((RectTransform)root.transform,placer,wave);
                overlay=new HudWorldOverlay((RectTransform)root.transform,build);
                typeof(HudWorldOverlay).GetField("radar",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(overlay,radar);
                var popup=(GameObject)typeof(HudBuildUi).GetField("inspectRoot",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(build);
                var rt=popup.GetComponent<RectTransform>();rt.anchoredPosition=new Vector2(350,100);
                Canvas.ForceUpdateCanvases();overlay.Tick();
                var radarRt=(RectTransform)typeof(HudWorldOverlay).GetField("radarRt",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(overlay);
                var panels=new List<RectTransform>();build.CollectRadarObstacles(panels);var corners=new Vector3[4];radarRt.GetWorldCorners(corners);var rect=Rect.MinMaxRect(corners[0].x,corners[0].y,corners[2].x,corners[2].y);
                foreach(var panel in panels){panel.GetWorldCorners(corners);if(rect.Overlaps(Rect.MinMaxRect(corners[0].x,corners[0].y,corners[2].x,corners[2].y)))throw new InvalidOperationException("Actual UI overlap: "+panel.name);}
            }
            foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=5;
            Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;result=new Texture2D(1920,1080,TextureFormat.RGB24,false);result.ReadPixels(new Rect(0,0,1920,1080),0,0);result.Apply();File.WriteAllBytes(Output+mode+".png",result.EncodeToPNG());
            var previews=root.GetComponentsInChildren<RawImage>().Where(i=>i.name.StartsWith("Preview_Menu_Tier1_")).ToArray();
            if(mode!="Radar"&&previews.Length!=5)throw new InvalidOperationException("Expected five tier-one previews in "+mode+", got "+previews.Length);
        }
        finally{RenderTexture.active=previous;if(overlay!=null)overlay.Dispose();if(result)UnityEngine.Object.DestroyImmediate(result);if(root)UnityEngine.Object.DestroyImmediate(root);if(cameraRoot)UnityEngine.Object.DestroyImmediate(cameraRoot);if(target)UnityEngine.Object.DestroyImmediate(target);}
    }
}
