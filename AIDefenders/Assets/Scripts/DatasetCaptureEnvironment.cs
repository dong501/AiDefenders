using UnityEngine;

/// <summary>A capture preset references shared terrain geometry plus a stage's atmosphere.</summary>
public sealed class DatasetCaptureEnvironment : MonoBehaviour
{
    public string stageId;
    public Material daySky;

    public static string Label(GameObject terrain)
    {
        if (terrain == null) return "";
        var preset = terrain.GetComponent<DatasetCaptureEnvironment>();
        if (preset == null) return terrain.name;
        switch (preset.stageId)
        {
            case "Desert_1": return "사막 1 · 낮";
            case "Desert_2": return "사막 2 · 황혼 · 모래폭풍";
            case "Desert_3": return "사막 3 · 달밤";
            case "City_1": return "도시 1 · 낮";
            case "City_2": return "도시 2 · 비 오는 저녁";
            case "City_3": return "도시 3 · 안개 낀 새벽";
            default: return terrain.name;
        }
    }

    public void Apply(Light sun)
    {
        var atmosphere = GetComponent<StageAtmosphere>();
        if (atmosphere != null)
        {
            atmosphere.ApplyEnvironment();
            if (sun != null) sun.intensity *= Random.Range(.9f, 1.1f);
            return;
        }
        RenderSettings.skybox = daySky;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(.58f, .66f, .71f);
        RenderSettings.ambientEquatorColor = new Color(.43f, .49f, .48f);
        RenderSettings.ambientGroundColor = new Color(.25f, .26f, .24f);
        RenderSettings.reflectionIntensity = .65f;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fog = true;
        bool desert = stageId.StartsWith("Desert");
        RenderSettings.fogColor = desert ? new Color(.77f,.67f,.47f) : new Color(.55f,.65f,.68f);
        RenderSettings.fogDensity = desert ? .0012f : .0018f;
        if (sun != null)
        {
            sun.intensity = Random.Range(1.05f,1.35f);
            sun.color = desert ? new Color(1f,.91f,.75f) : new Color(.93f,.98f,1f);
            sun.transform.rotation = Quaternion.Euler(Random.Range(36,60),Random.Range(-70,-25),0);
            RenderSettings.sun = sun;
        }
    }

    public void ApplyForAutomaticCapture(Light sun)
    {
        Apply(sun);

        // The simplified capture sets lack the light bouncing off the battlefield and its local lamps.
        // Lift the shadows while retaining each preset's colors and fog.
        bool daylight = stageId == "Desert_1" || stageId == "City_1";
        float ambientLift = daylight ? .18f : .38f;
        var fill = new Color(.80f, .82f, .85f);
        RenderSettings.ambientSkyColor = Color.Lerp(RenderSettings.ambientSkyColor, fill, ambientLift);
        RenderSettings.ambientEquatorColor = Color.Lerp(RenderSettings.ambientEquatorColor, fill, ambientLift);
        RenderSettings.ambientGroundColor = Color.Lerp(RenderSettings.ambientGroundColor, fill, ambientLift);
        if (sun != null)
        {
            sun.intensity *= daylight ? 1.12f : 1.30f;
            RenderSettings.sun = sun;
        }
    }
}
