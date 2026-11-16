using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Static curved artwork with blue/red gradients and metallic highlights.
public class UiSkillSymbol : MaskableGraphic
{
    public bool missile;
    [System.NonSerialized] public float previewPhase=-1;
    static readonly Color Blue=new Color32(51,188,255,255),Red=new Color32(255,78,112,255);
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();if(missile)Rocket(vh);else Identification(vh);
    }
    void Identification(VertexHelper vh)
    {
        Ring(vh,new Vector2(-.07f,.02f),.35f,.012f,1,.22f);
        Ellipse(vh,new Vector2(-.075f,.245f),new Vector2(.105f,.123f),0,new Color(0,.02f,.08f,.4f));
        Ellipse(vh,new Vector2(-.09f,.267f),new Vector2(.102f,.12f),1,Color.white);
        var torso=new List<Vector2>{new Vector2(-.16f,.13f)};
        Curve(torso,new Vector2(-.32f,.115f),new Vector2(-.37f,.025f),new Vector2(-.37f,-.10f));
        Curve(torso,new Vector2(-.37f,-.17f),new Vector2(-.27f,-.18f),new Vector2(-.235f,-.12f));
        Curve(torso,new Vector2(-.23f,-.20f),new Vector2(-.235f,-.29f),new Vector2(-.21f,-.34f));
        Curve(torso,new Vector2(-.12f,-.365f),new Vector2(.02f,-.365f),new Vector2(.05f,-.32f));
        Curve(torso,new Vector2(.055f,-.25f),new Vector2(.045f,-.18f),new Vector2(.055f,-.12f));
        Curve(torso,new Vector2(.11f,-.18f),new Vector2(.20f,-.16f),new Vector2(.19f,-.09f));
        Curve(torso,new Vector2(.18f,.03f),new Vector2(.13f,.11f),new Vector2(-.02f,.13f));
        Curve(torso,new Vector2(-.04f,.09f),new Vector2(-.13f,.09f),new Vector2(-.16f,.13f));
        Fill(vh,torso,new Vector2(-.085f,-.09f),1,Color.white);
        Stroke(vh,new Vector2(-.26f,.035f),new Vector2(-.29f,-.08f),.014f,0,new Color(1,1,1,.55f));
        Ellipse(vh,new Vector2(-.118f,.31f),new Vector2(.044f,.023f),0,new Color(1,1,1,.45f));
        Ring(vh,new Vector2(-.065f,-.065f),.08f,.014f,0,.9f);
        Ring(vh,new Vector2(-.065f,-.065f),.115f,.006f,0,.38f);
        Vector2 tip=new Vector2(-.065f,-.065f),end=new Vector2(.29f,.22f);
        Stroke(vh,tip+new Vector2(.01f,-.02f),end+new Vector2(.01f,-.02f),.045f,0,new Color(0,.02f,.08f,.6f));
        Stroke(vh,tip,end,.031f,2,Color.white);
        Stroke(vh,tip+new Vector2(0,.007f),end+new Vector2(0,.007f),.008f,0,Color.white);
        var fin=new List<Vector2>{new Vector2(.16f,.16f)};
        Curve(fin,new Vector2(.21f,.32f),new Vector2(.27f,.40f),new Vector2(.40f,.405f));
        Curve(fin,new Vector2(.39f,.30f),new Vector2(.32f,.24f),new Vector2(.29f,.22f));
        Curve(fin,new Vector2(.38f,.20f),new Vector2(.43f,.17f),new Vector2(.44f,.10f));
        Curve(fin,new Vector2(.32f,.08f),new Vector2(.23f,.10f),new Vector2(.16f,.16f));
        Fill(vh,fin,new Vector2(.29f,.23f),1,Color.white);
        Stroke(vh,new Vector2(.17f,.16f),new Vector2(.38f,.35f),.012f,0,new Color(1,1,1,.85f));
        Ellipse(vh,tip,new Vector2(.022f,.022f),3,Color.white);
    }
    void Rocket(VertexHelper vh)
    {
        var body=new List<Vector2>{new Vector2(-.095f,-.26f)};
        Curve(body,new Vector2(-.12f,-.08f),new Vector2(-.105f,.12f),new Vector2(-.085f,.26f));
        Curve(body,new Vector2(-.06f,.29f),new Vector2(.06f,.29f),new Vector2(.085f,.26f));
        Curve(body,new Vector2(.105f,.12f),new Vector2(.12f,-.08f),new Vector2(.095f,-.26f));
        Fill(vh,body,Vector2.zero,2,Color.white);
        var nose=new List<Vector2>{new Vector2(-.085f,.26f)};
        Curve(nose,new Vector2(-.09f,.32f),new Vector2(-.035f,.41f),new Vector2(0,.46f));
        Curve(nose,new Vector2(.035f,.41f),new Vector2(.09f,.32f),new Vector2(.085f,.26f));
        Fill(vh,nose,new Vector2(0,.32f),5,Color.white);
        Stroke(vh,new Vector2(-.088f,.245f),new Vector2(.088f,.245f),.035f,3,Color.white);
        var casing=new List<Vector2>{new Vector2(-.09f,-.22f),new Vector2(-.09f,.09f),new Vector2(.09f,-.02f),new Vector2(.09f,-.22f)};
        Fill(vh,casing,new Vector2(0,-.10f),6,Color.white);
        Stroke(vh,new Vector2(-.06f,-.18f),new Vector2(-.06f,.07f),.014f,0,new Color(1,1,1,.65f));
        Ellipse(vh,new Vector2(.01f,.12f),new Vector2(.034f,.047f),4,Color.white);
        Ellipse(vh,new Vector2(.005f,.13f),new Vector2(.018f,.025f),0,new Color32(101,242,255,255));
        for(int side=-1;side<=1;side+=2)
        {
            var fin=new List<Vector2>{new Vector2(side*.045f,-.10f)};
            Curve(fin,new Vector2(side*.14f,-.25f),new Vector2(side*.27f,-.32f),new Vector2(side*.32f,-.28f));
            Curve(fin,new Vector2(side*.36f,-.22f),new Vector2(side*.35f,-.08f),new Vector2(side*.31f,.005f));
            Curve(fin,new Vector2(side*.27f,-.02f),new Vector2(side*.23f,-.055f),new Vector2(side*.20f,-.09f));
            Curve(fin,new Vector2(side*.26f,-.09f),new Vector2(side*.26f,-.18f),new Vector2(side*.245f,-.21f));
            Curve(fin,new Vector2(side*.19f,-.20f),new Vector2(side*.10f,-.13f),new Vector2(side*.045f,-.10f));
            Fill(vh,fin,new Vector2(side*.245f,-.17f),4,Color.white);
            Stroke(vh,new Vector2(side*.30f,-.25f),new Vector2(side*.32f,-.08f),.012f,3,Color.white);
        }
        Stroke(vh,new Vector2(-.10f,-.28f),new Vector2(.10f,-.28f),.07f,4,Color.white);
        Fill(vh,new List<Vector2>{new Vector2(-.055f,-.32f),new Vector2(0,-.45f),new Vector2(.055f,-.32f)},new Vector2(0,-.35f),3,Color.white);
    }
    static void Curve(List<Vector2> points,Vector2 a,Vector2 b,Vector2 end)
    {
        Vector2 start=points[points.Count-1];
        for(int i=1;i<=18;i++){float t=i/18f,u=1-t;points.Add(u*u*u*start+3*u*u*t*a+3*u*t*t*b+t*t*t*end);}
    }
    Color Shade(Vector2 p,int mode,Color tint)
    {
        Color c=tint;
        if(mode==1)
        {
            float t=Mathf.SmoothStep(0,1,Mathf.InverseLerp(-.30f,.20f,p.x));Color violet=new Color32(157,105,255,255);
            c=(t<.5f?Color.Lerp(Blue,violet,t*2):Color.Lerp(violet,Red,(t-.5f)*2))*Mathf.Lerp(.72f,1.12f,Mathf.InverseLerp(-.4f,.4f,p.y));
        }
        if(mode==2)c=Color.Lerp(new Color32(51,76,96,255),new Color32(236,249,252,255),Mathf.Clamp01(1-Mathf.Abs(p.x+.035f)*8));
        if(mode==3)c=Color.Lerp(new Color32(214,132,20,255),new Color32(255,238,146,255),Mathf.Clamp01(.7f-p.x*2+p.y));
        if(mode==4)c=Color.Lerp(new Color32(16,33,53,255),new Color32(118,157,183,255),Mathf.Clamp01(1-Mathf.Abs(p.x)*2.8f+p.y));
        if(mode==5)c=Color.Lerp(new Color32(153,28,49,255),new Color32(255,141,102,255),Mathf.Clamp01(.6f-p.x*5));
        if(mode==6)c=Color.Lerp(new Color32(18,52,99,255),new Color32(65,162,224,255),Mathf.Clamp01(.4f-p.x*5));
        if(mode!=0)c.a=tint.a;return c;
    }
    Vector2 P(Vector2 p)
    {
        if(missile)p=new Vector2(p.x*.866f+p.y*.5f,-p.x*.5f+p.y*.866f);
        return rectTransform.rect.center+p*Mathf.Min(rectTransform.rect.width,rectTransform.rect.height);
    }
    void Tri(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,int mode,Color tint)
    {int n=vh.currentVertCount;vh.AddVert(P(a),Shade(a,mode,tint),Vector2.zero);vh.AddVert(P(b),Shade(b,mode,tint),Vector2.zero);vh.AddVert(P(c),Shade(c,mode,tint),Vector2.zero);vh.AddTriangle(n,n+1,n+2);}
    void Fill(VertexHelper vh,List<Vector2> outline,Vector2 center,int mode,Color tint)
    {for(int i=0;i<outline.Count;i++)Tri(vh,center,outline[i],outline[(i+1)%outline.Count],mode,tint);}
    void Ellipse(VertexHelper vh,Vector2 center,Vector2 radius,int mode,Color tint)
    {for(int i=0;i<64;i++){float a=i*Mathf.PI/32,b=(i+1)*Mathf.PI/32;Tri(vh,center,center+Vector2.Scale(new Vector2(Mathf.Cos(a),Mathf.Sin(a)),radius),center+Vector2.Scale(new Vector2(Mathf.Cos(b),Mathf.Sin(b)),radius),mode,tint);}}
    void Stroke(VertexHelper vh,Vector2 a,Vector2 b,float width,int mode,Color tint)
    {Vector2 n=new Vector2(-(b-a).y,(b-a).x).normalized*width*.5f;Tri(vh,a+n,b+n,b-n,mode,tint);Tri(vh,a+n,b-n,a-n,mode,tint);}
    void Ring(VertexHelper vh,Vector2 center,float radius,float thickness,int mode,float alpha)
    {
        Color tint=new Color(1,1,1,alpha);
        for(int i=0;i<96;i++){float a=i*Mathf.PI/48,b=(i+1)*Mathf.PI/48;Vector2 x=new Vector2(Mathf.Cos(a),Mathf.Sin(a)),y=new Vector2(Mathf.Cos(b),Mathf.Sin(b));Tri(vh,center+x*radius,center+y*radius,center+y*(radius-thickness),mode,tint);Tri(vh,center+x*radius,center+y*(radius-thickness),center+x*(radius-thickness),mode,tint);}
    }
}
