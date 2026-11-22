using UnityEngine;
using UnityEngine.UI;

// Cyan bars use one shared scale across all turret types and tiers; white ticks mark the comparison.
public sealed class TurretStatComparison
{
    readonly TurretLabEntry[] catalog;
    readonly GameObject root;
    readonly Text title, compare, role;
    readonly Image[] fills=new Image[6];
    readonly RectTransform[] markers=new RectTransform[6];
    readonly Text[] numbers=new Text[6];
    readonly float[] maxima=new float[6];
    readonly string[] labels={"1회 공격력","초당 공격 횟수","사거리","관통력","폭발 반경","재장전 / 완전 냉각"};
    readonly string[] units={"","회/초","m","","m","초"};
    int selected=-1, level=1, baseline;
    public TurretStatComparison(Transform parent,TurretLabEntry[] catalog)
    {
        this.catalog=catalog ?? new TurretLabEntry[0];
        var panel=UiFont.AddImage(parent,"TurretStatComparison",new Color(.035f,.10f,.19f,.95f));
        root=panel.gameObject;UiSkin.Surface(panel);
        var rt=panel.rectTransform;rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);rt.anchoredPosition=new Vector2(24,-110);rt.sizeDelta=new Vector2(600,610);
        title=TextAt(rt,"Title","포탑 능력치",26,18,14,564,36,Color.white);
        compare=TextAt(rt,"Comparison","",19,64,58,472,32,UiSkin.Muted);
        Arrow(rt,"Previous","‹",18,-58,()=>Cycle(-1));Arrow(rt,"Next","›",542,-58,()=>Cycle(1));
        TextAt(rt,"Legend","청록: 선택 포탑   흰 눈금: 비교 포탑 · 같은 단계",16,18,98,564,24,UiSkin.Muted);
        for(int i=0;i<6;i++)
        {
            float top=136+i*54;
            TextAt(rt,"StatLabel_"+i,labels[i],19,18,top,230,25,Color.white);
            numbers[i]=TextAt(rt,"StatValue_"+i,"",18,240,top,342,25,Color.white);numbers[i].alignment=TextAnchor.UpperRight;
            var track=UiFont.AddImage(rt,"Track_"+i,new Color(.12f,.20f,.28f));
            var tr=track.rectTransform;tr.anchorMin=tr.anchorMax=new Vector2(0,1);tr.pivot=new Vector2(0,1);tr.anchoredPosition=new Vector2(18,-top-29);tr.sizeDelta=new Vector2(564,12);track.raycastTarget=false;
            fills[i]=UiFont.AddImage(tr,"Fill",new Color(.08f,.80f,.91f));fills[i].raycastTarget=false;
            var tick=UiFont.AddImage(tr,"CompareTick",Color.white);tick.raycastTarget=false;markers[i]=tick.rectTransform;
        }
        role=TextAt(rt,"Role","",18,18,466,564,90,UiSkin.Gold);role.horizontalOverflow=HorizontalWrapMode.Wrap;
        TextAt(rt,"Note","영구 연구 반영 · 공격력은 장갑 적용 전 수치\n재장전은 짧을수록 긴 게이지 · 범위 피해·과열 등은 별도",15,18,568,564,38,UiSkin.Muted);
        for(int i=0;i<this.catalog.Length;i++) if(this.catalog[i]!=null && this.catalog[i].prefab!=null && TurretResearch.GroupOfPrefab(this.catalog[i].prefab)==TurretResearch.MachineGun) baseline=i;
        root.SetActive(false);
    }
    static Text TextAt(Transform parent,string name,string text,int size,float x,float top,float width,float height,Color color)
    {
        var t=UiFont.AddText(parent,name,text,size,TextAnchor.UpperLeft,color,UiType.Body);
        t.raycastTarget=false;var rt=t.rectTransform;rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);rt.anchoredPosition=new Vector2(x,-top);rt.sizeDelta=new Vector2(width,height);return t;
    }
    static void Arrow(Transform parent,string name,string text,float x,float y,UnityEngine.Events.UnityAction action)
    {
        var im=UiFont.AddImage(parent,name,UiSkin.Blue);var rt=im.rectTransform;rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);rt.anchoredPosition=new Vector2(x,y);rt.sizeDelta=new Vector2(40,34);
        var b=im.gameObject.AddComponent<Button>();b.targetGraphic=im;b.onClick.AddListener(action);
        var t=UiFont.AddText(rt,"Label",text,24,TextAnchor.MiddleCenter,Color.white,UiType.Body);UiFont.Stretch(t.rectTransform);t.raycastTarget=false;
    }
    void Cycle(int direction)
    {
        for(int i=0;i<catalog.Length;i++) {baseline=(baseline+direction+catalog.Length)%catalog.Length;if(TurretStatSnapshot.Read(catalog[baseline],level)!=null)break;}
        Show(selected,level);
    }
    public void Show(int index,int tier)
    {
        selected=index;level=tier;
        var current=index>=0&&index<catalog.Length ? TurretStatSnapshot.Read(catalog[index],level) : null;
        root.SetActive(current!=null);if(current==null)return;
        var reference=baseline>=0&&baseline<catalog.Length ? TurretStatSnapshot.Read(catalog[baseline],level) : null;
        if(reference==null){baseline=index;reference=current;}
        for(int j=0;j<6;j++) maxima[j]=.01f;
        foreach(var entry in catalog)for(int k=1;k<=3;k++) {var snapshot=TurretStatSnapshot.Read(entry,k);if(snapshot==null)continue;for(int j=0;j<6;j++)maxima[j]=Mathf.Max(maxima[j],snapshot.values[j]);}
        title.text=catalog[index].Label+" · "+level+"단계";
        compare.text="비교: "+catalog[baseline].Label+" · "+level+"단계";
        role.text=current.role;
        for(int j=0;j<6;j++)
        {
            float a=current.values[j],b=reference.values[j],delta=a-b;
            float ratio=j==5?1f-a/(maxima[j]*1.1f):a/maxima[j];
            float tick=j==5?1f-b/(maxima[j]*1.1f):b/maxima[j];
            var fr=fills[j].rectTransform;fr.anchorMin=Vector2.zero;fr.anchorMax=new Vector2(Mathf.Clamp01(ratio),1);fr.offsetMin=fr.offsetMax=Vector2.zero;
            markers[j].anchorMin=markers[j].anchorMax=new Vector2(Mathf.Clamp01(tick),.5f);markers[j].anchoredPosition=Vector2.zero;markers[j].sizeDelta=new Vector2(3,20);
            string change="";
            if(Mathf.Abs(delta)>.005f){bool better=j==5?delta<0:delta>0;change=" <color="+(better?"#76E8A0":"#FF8B82")+">("+(delta>0?"+":"")+delta.ToString("0.##")+")</color>";}
            numbers[j].text=a.ToString("0.##")+units[j]+change;
        }
    }
}
