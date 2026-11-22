using UnityEngine;

public enum StageWeather { SandstormSunset, DesertMoonlight, RainyEvening, FoggyDawn }

// Stage-local atmosphere. No changes to terrain, collision, AI or shared materials.
public class StageAtmosphere : MonoBehaviour
{
    public StageWeather weather;
    public Material skyMaterial;
    GameObject effects;
    Material particleMaterial;
    Texture2D particleTexture;
    Transform weatherAnchor;

    void Awake() { ApplyEnvironment(); CreateWeatherEffects(); }

    public Color SkyTop => weather == StageWeather.SandstormSunset ? new Color(.35f,.22f,.14f)
        : weather == StageWeather.DesertMoonlight ? new Color(.015f,.035f,.13f)
        : weather == StageWeather.RainyEvening ? new Color(.035f,.065f,.13f) : new Color(.12f,.24f,.31f);
    public Color SkyHorizon => weather == StageWeather.SandstormSunset ? new Color(.85f,.51f,.25f)
        : weather == StageWeather.DesertMoonlight ? new Color(.12f,.2f,.37f)
        : weather == StageWeather.RainyEvening ? new Color(.23f,.26f,.35f) : new Color(.52f,.66f,.69f);

    public void ApplyEnvironment()
    {
        bool storm = weather == StageWeather.SandstormSunset;
        bool moon = weather == StageWeather.DesertMoonlight;
        bool rain = weather == StageWeather.RainyEvening;
        RenderSettings.skybox = skyMaterial;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = moon ? new Color(.22f,.3f,.48f) : rain ? new Color(.29f,.33f,.44f) : storm ? new Color(.55f,.4f,.27f) : new Color(.43f,.55f,.58f);
        RenderSettings.ambientEquatorColor = moon ? new Color(.16f,.21f,.32f) : rain ? new Color(.23f,.28f,.36f) : storm ? new Color(.45f,.32f,.19f) : new Color(.32f,.43f,.45f);
        RenderSettings.ambientGroundColor = moon ? new Color(.1f,.14f,.23f) : new Color(.2f,.23f,.25f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = storm ? new Color(.64f,.43f,.25f) : moon ? new Color(.11f,.17f,.29f) : rain ? new Color(.24f,.29f,.38f) : new Color(.46f,.6f,.62f);
        RenderSettings.fogDensity = storm ? .0062f : moon ? .0018f : rain ? .0028f : .0048f;
        foreach(var light in FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if(light.gameObject.scene != gameObject.scene || light.type != LightType.Directional)continue;
            light.color = storm ? new Color(1f,.66f,.34f) : moon ? new Color(.53f,.68f,1f) : rain ? new Color(.62f,.7f,.9f) : new Color(.66f,.83f,.86f);
            light.intensity = storm ? .85f : moon ? .7f : rain ? .55f : .75f;
            light.transform.rotation = Quaternion.Euler(storm ? 12f : moon ? 32f : rain ? 18f : 9f,storm ? -65f : moon ? -35f : 125f,0);
            RenderSettings.sun = light;
        }
        RenderSettings.reflectionIntensity = rain ? .8f : .4f;
    }

    public void CreateWeatherEffects()
    {
        if(effects != null)return;
        effects = new GameObject("WeatherParticles");effects.transform.SetParent(transform,false);
        Shader shader=Shader.Find("Sprites/Default");
        if(shader==null)return;
        particleMaterial=new Material(shader);
        particleTexture=new Texture2D(32,32,TextureFormat.RGBA32,false);
        for(int y=0;y<32;y++)for(int x=0;x<32;x++)
        {
            float r=new Vector2((x-15.5f)/15.5f,(y-15.5f)/15.5f).magnitude;
            particleTexture.SetPixel(x,y,new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-r),2)));
        }
        particleTexture.Apply();particleMaterial.mainTexture=particleTexture;
        bool storm=weather==StageWeather.SandstormSunset;
        bool rain=weather==StageWeather.RainyEvening;
        if(storm)
        {
            CreateParticles("WindblownSand",1400,8f,.08f,new Color(.88f,.64f,.35f,.52f),new Vector3(24,-1,7),new Vector3(160,35,160),false);
            CreateParticles("DustBanks",65,10f,6f,new Color(.73f,.5f,.27f,.07f),new Vector3(12,0,4),new Vector3(180,12,180),false);
        }
        else if(rain)
            CreateParticles("Rain",1100,2.5f,.035f,new Color(.68f,.83f,.96f,.48f),new Vector3(-4,-29,2),new Vector3(110,12,110),true);
        else if(weather==StageWeather.FoggyDawn)
            CreateParticles("LowMist",24,15f,10f,new Color(.63f,.78f,.8f,.035f),new Vector3(1,0,.3f),new Vector3(180,10,180),false);
    }

    void CreateParticles(string label,float rate,float life,float size,Color color,Vector3 velocity,Vector3 volume,bool streak)
    {
        var go=new GameObject(label);go.transform.SetParent(effects.transform,false);
        go.transform.localPosition=new Vector3(0,streak?35f:2f,0);
        var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        var main=ps.main;main.loop=true;main.prewarm=true;main.startLifetime=life;main.startSpeed=0;main.startSize=size;
        main.startColor=color;main.maxParticles=16000;main.simulationSpace=ParticleSystemSimulationSpace.World;
        main.useUnscaledTime=false;main.cullingMode=ParticleSystemCullingMode.AlwaysSimulate;
        var emission=ps.emission;emission.rateOverTime=rate;
        var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=volume;
        var speed=ps.velocityOverLifetime;speed.enabled=true;speed.space=ParticleSystemSimulationSpace.World;
        speed.x=velocity.x;speed.y=velocity.y;speed.z=velocity.z;
        var alpha=ps.colorOverLifetime;alpha.enabled=true;var g=new Gradient();
        g.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,.15f),new GradientAlphaKey(.8f,.75f),new GradientAlphaKey(0,1)});alpha.color=g;
        var renderer=go.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=particleMaterial;
        renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
        if(streak){renderer.renderMode=ParticleSystemRenderMode.Stretch;renderer.velocityScale=.06f;renderer.lengthScale=4f;}
        ps.Play();
    }

    void LateUpdate()
    {
        if(effects==null)return;
        if(weatherAnchor==null)
        {
            var drone=FindFirstObjectByType<CCTVDroneController>();
            weatherAnchor=drone!=null?drone.transform:Camera.main!=null?Camera.main.transform:null;
        }
        if(weatherAnchor!=null)effects.transform.position=weatherAnchor.position;
    }

    void OnDestroy()
    {
        if(particleMaterial!=null){if(Application.isPlaying)Destroy(particleMaterial);else DestroyImmediate(particleMaterial);}
        if(particleTexture!=null){if(Application.isPlaying)Destroy(particleTexture);else DestroyImmediate(particleTexture);}
    }
}
