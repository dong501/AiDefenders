using UnityEngine;

public class CityTrafficAmberBlink : MonoBehaviour
{
    [System.Serializable] public class Signal { public Renderer renderer; public int materialIndex; }
    public Signal[] signals;
    [Min(.1f)] public float onSeconds=.5f;
    [Min(.1f)] public float offSeconds=.5f;
    public Color onColor=new Color(1f,.63f,.025f,1f);
    public Color offColor=new Color(.18f,.13f,.035f,1f);
    MaterialPropertyBlock properties;
    bool initialized,lastState;
    static readonly int BaseColor=Shader.PropertyToID("_BaseColor"),Emission=Shader.PropertyToID("_EmissionColor");
    public bool IsAmberOn(float elapsed)=>Mathf.Repeat(elapsed,Mathf.Max(.1f,onSeconds)+Mathf.Max(.1f,offSeconds))<Mathf.Max(.1f,onSeconds);
    void OnEnable(){initialized=false;}
    void Update()
    {
        bool state=IsAmberOn(Time.time);
        if(!initialized||state!=lastState)ApplyState(state);
    }
    public void ApplyState(bool on)
    {
        if(properties==null)properties=new MaterialPropertyBlock();
        if(signals!=null)foreach(var signal in signals)
        {
            if(signal==null||signal.renderer==null)continue;
            properties.Clear();signal.renderer.GetPropertyBlock(properties,signal.materialIndex);
            properties.SetColor(BaseColor,on?onColor:offColor);
            properties.SetColor(Emission,on?onColor*3f:Color.black);
            signal.renderer.SetPropertyBlock(properties,signal.materialIndex);
        }
        initialized=true;lastState=on;
    }
    void OnDisable(){if(signals!=null)foreach(var signal in signals)if(signal!=null&&signal.renderer!=null)signal.renderer.SetPropertyBlock(null,signal.materialIndex);}
}
