using UnityEngine;
using UnityEngine.UI;

public class UiAmbientMotion : MonoBehaviour
{
    RectTransform rect;
    Vector2 home;
    Vector3 scale;
    Quaternion rotation;
    float phase, amplitude, tilt, pulse, speed;

    public static void Apply(RectTransform target,float offset,float bob=16,float angle=2.2f,float breathe=.025f,float frequency=1.5f)
    {
        var motion=target.gameObject.AddComponent<UiAmbientMotion>();
        motion.rect=target;motion.home=target.anchoredPosition;motion.scale=target.localScale;motion.rotation=target.localRotation;
        motion.phase=offset;motion.amplitude=bob;motion.tilt=angle;motion.pulse=breathe;motion.speed=frequency;
    }
    void LateUpdate()
    {
        if(!rect)return;
        float t=Time.unscaledTime*speed+phase;
        rect.anchoredPosition=home+new Vector2(Mathf.Sin(t*.65f)*amplitude*.25f,Mathf.Sin(t)*amplitude);
        rect.localRotation=rotation*Quaternion.Euler(0,0,Mathf.Sin(t*.8f)*tilt);
        rect.localScale=scale*(1+Mathf.Sin(t)*pulse);
    }
}

public class MenuEnergyParticles : MaskableGraphic
{
    public static void Create(Transform parent)
    {
        var go=new GameObject("Floating_Energy",typeof(RectTransform),typeof(CanvasRenderer),typeof(MenuEnergyParticles));
        go.transform.SetParent(parent,false);UiFont.Stretch(go.GetComponent<RectTransform>());go.GetComponent<MenuEnergyParticles>().raycastTarget=false;
    }
    void Update(){SetVerticesDirty();}
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();var r=rectTransform.rect;float time=Time.unscaledTime;
        for(int i=0;i<32;i++)
        {
            float seed=Mathf.Repeat(i*.618034f,1);float y=Mathf.Repeat(i*.173f+time*(.025f+(i%4)*.006f),1);
            float x=seed+Mathf.Sin(time*.6f+i)*.018f;
            var p=new Vector2(r.xMin+x*r.width,r.yMin+y*r.height);
            float size=4+(i%4)*1.7f;
            float alpha=Mathf.Sin(y*Mathf.PI)*(.35f+.18f*Mathf.Sin(time*1.8f+i));
            var c=i%4==0?new Color(1,.8f,.25f,alpha):new Color(.45f,.95f,1,alpha);
            int n=vh.currentVertCount;
            vh.AddVert(p+Vector2.up*size,c,Vector2.zero);vh.AddVert(p+Vector2.right*size*.5f,c,Vector2.zero);
            vh.AddVert(p-Vector2.up*size,c,Vector2.zero);vh.AddVert(p-Vector2.right*size*.5f,c,Vector2.zero);
            vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
        }
    }
}
