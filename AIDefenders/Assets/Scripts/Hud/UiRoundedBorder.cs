using UnityEngine;
using UnityEngine.UI;

public class UiRoundedBorder : MaskableGraphic
{
    public float thickness=4.5f;
    public float radius=18f;
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();Rect r=rectTransform.rect;float rad=Mathf.Min(radius,r.width*.5f,r.height*.5f);
        const int steps=10;
        for(int corner=0;corner<4;corner++)
        {
            Vector2 center=new Vector2(corner==0||corner==3?r.xMax-rad:r.xMin+rad,corner<2?r.yMax-rad:r.yMin+rad);
            for(int step=0;step<=steps;step++)
            {
                float angle=(corner*90+step*90f/steps)*Mathf.Deg2Rad;Vector2 normal=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                vh.AddVert(center+normal*rad,color,Vector2.zero);vh.AddVert(center+normal*Mathf.Max(0,rad-thickness),color,Vector2.zero);
            }
        }
        int pairs=4*(steps+1);
        for(int i=0;i<pairs;i++){int a=i*2,b=((i+1)%pairs)*2;vh.AddTriangle(a,b,a+1);vh.AddTriangle(a+1,b,b+1);}
    }
}
