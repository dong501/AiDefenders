using UnityEngine;

public static class TurretPaint
{
    public static readonly Color[] Colors = BuildPalette();
    public static readonly string[] Names = BuildNames();
    static Color[] BuildPalette()
    {
        var colors=new Color[65];colors[0]=Color.white;
        for(int row=0;row<8;row++)for(int column=0;column<8;column++)
            colors[1+row*8+column]=row==0?Color.Lerp(new Color(.08f,.08f,.08f),Color.white,column/7f):Color.HSVToRGB(column/8f,row<=4?.85f:Mathf.Lerp(.65f,.22f,(row-5)/2f),row<=4?Mathf.Lerp(.35f,1f,(row-1)/3f):1f);
        return colors;
    }
    static string[] BuildNames()
    {
        var names=new string[65];names[0]="기본";
        for(int i=1;i<names.Length;i++)names[i]="#"+ColorUtility.ToHtmlStringRGB(Colors[i]);return names;
    }
    static string Key(TurretController turret)
    {
        var source = turret.sourcePrefab != null ? turret.sourcePrefab : turret.gameObject;
        string group = TurretResearch.GroupOfPrefab(source);
        return "TurretPaint_" + (string.IsNullOrEmpty(group) ? source.name.Replace("(Clone)", "").Trim() : group);
    }
    public static int Selected(TurretController turret)
    {
        if(PlayerPrefs.HasKey(Key(turret)+"_Palette64"))return Mathf.Clamp(PlayerPrefs.GetInt(Key(turret)+"_Palette64"),0,64);
        int old=PlayerPrefs.GetInt(Key(turret),0);if(old<=0)return 0;
        var legacy=new[]{Color.white,new Color(.16f,.48f,.85f),new Color(.25f,.52f,.30f),new Color(.75f,.23f,.20f),new Color(.52f,.28f,.72f)};
        Color target=legacy[Mathf.Clamp(old,0,4)];int closest=1;float distance=float.MaxValue;
        for(int i=1;i<Colors.Length;i++){float d=((Vector4)(Colors[i]-target)).sqrMagnitude;if(d<distance){distance=d;closest=i;}}
        return closest;
    }
    public static void Select(TurretController turret, int index)
    {
        PlayerPrefs.SetInt(Key(turret)+"_Palette64", Mathf.Clamp(index,0,64));
        PlayerPrefs.Save();
        Apply(turret);
    }
    public static void Apply(TurretController turret)
    {
        int selected = Selected(turret);
        foreach (var renderer in turret.GetComponentsInChildren<MeshRenderer>(true))
        {
            var materials = renderer.sharedMaterials;
            var slots=renderer.GetComponent<TurretPaintSlots>();
            for (int i = 0; i < materials.Length; i++)
            {
                var material = materials[i];
                if (material == null || !material.HasProperty("_BaseColor")) continue;
                Color original = material.GetColor("_BaseColor");
                var block = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(block, i);
                bool paintable=slots!=null && slots.materialSlots!=null && System.Array.IndexOf(slots.materialSlots,i)>=0;
                Color color=selected==0 || !paintable?original:Colors[selected]*Mathf.Lerp(.65f,1f,original.maxColorComponent);
                color.a=original.a;
                block.SetColor("_BaseColor",color);
                renderer.SetPropertyBlock(block, i);
            }
        }
    }
}
