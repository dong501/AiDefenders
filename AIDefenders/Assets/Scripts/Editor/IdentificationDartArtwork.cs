using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class IdentificationDartArtwork
{
    const string Folder="Assets/Art/IdentificationDart";
    static readonly List<Vector3> vertices=new List<Vector3>();
    static readonly List<int>[] triangles=Enumerable.Range(0,5).Select(i=>new List<int>()).ToArray();
    static void Face(Vector3 a,Vector3 b,Vector3 c,int sub)
    {int n=vertices.Count;vertices.AddRange(new[]{a,b,c});triangles[sub].AddRange(new[]{n,n+1,n+2});}
    static void Ring(float z0,float r0,float z1,float r1,int material)
    {
        for(int i=0;i<24;i++)
        {
            float a=i*Mathf.PI/12,b=(i+1)*Mathf.PI/12;
            var p=new Vector3(Mathf.Cos(a)*r0,Mathf.Sin(a)*r0,z0);
            var q=new Vector3(Mathf.Cos(b)*r0,Mathf.Sin(b)*r0,z0);
            var r=new Vector3(Mathf.Cos(b)*r1,Mathf.Sin(b)*r1,z1);
            var t=new Vector3(Mathf.Cos(a)*r1,Mathf.Sin(a)*r1,z1);
            Face(p,r,q,material);Face(p,t,r,material);
        }
    }
    static Material Material(string name,string hex,float metal,float smooth,bool emission=false)
    {
        string path=Folder+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!m){var shader=Shader.Find("Universal Render Pipeline/Lit");if(!shader)shader=Shader.Find("Standard");m=new Material(shader);AssetDatabase.CreateAsset(m,path);}
        Color color;ColorUtility.TryParseHtmlString(hex,out color);m.color=color;
        if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",color);
        m.SetFloat("_Metallic",metal);m.SetFloat("_Smoothness",smooth);
        if(emission){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*1.4f);}
        EditorUtility.SetDirty(m);return m;
    }
    [MenuItem("Tools/AI Defenders/Skills/Apply Supplied Icons And Dart Model")]
    public static void Apply()
    {
        System.IO.Directory.CreateDirectory(Folder);AssetDatabase.Refresh();
        foreach(string icon in new[]{"BearTrapMissile","IdentificationDart"})
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/UI/SkillIcons/"+icon+".png");
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.filterMode=FilterMode.Bilinear;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=2048;importer.SaveAndReimport();
        }
        var mats=new[]{Material("Dart_Silver","#C6D3DF",.7f,.65f),Material("Dart_Graphite","#263340",.55f,.5f),Material("Dart_Core","#06DDED",.25f,.75f,true),Material("Dart_Fins","#087CDE",.4f,.5f),Material("Dart_Fin_Edge","#35C9FF",.25f,.6f)};
        vertices.Clear();foreach(var t in triangles)t.Clear();
        // +Z is the flight direction. Needle, bevelled casing, luminous core, tail spindle.
        Ring(1.65f,0,1.12f,.052f,0);Ring(1.12f,.052f,.89f,.085f,0);
        Ring(.89f,.085f,.77f,.15f,1);Ring(.77f,.15f,.65f,.25f,0);
        Ring(.65f,.25f,.46f,.285f,0);Ring(.46f,.285f,.40f,.29f,1);
        Ring(.40f,.27f,-.35f,.27f,2);Ring(-.35f,.29f,-.42f,.29f,1);
        Ring(-.42f,.285f,-.74f,.255f,0);Ring(-.74f,.255f,-.90f,.16f,0);
        Ring(-.90f,.16f,-1.08f,.14f,1);Ring(-1.08f,.14f,-1.53f,.12f,1);
        Ring(-1.53f,.12f,-1.58f,0,1);
        for(int fin=0;fin<3;fin++)
        {
            var rotation=Quaternion.Euler(0,0,fin*120+30);
            var profile=new[]{new Vector3(.12f,0,-.83f),new Vector3(.54f,0,-1.05f),new Vector3(.62f,0,-1.51f),new Vector3(.17f,0,-1.45f)};
            var a=profile.Select(p=>rotation*(p+Vector3.up*.035f)).ToArray();
            var b=profile.Select(p=>rotation*(p-Vector3.up*.035f)).ToArray();
            Face(a[0],a[1],a[2],3);Face(a[0],a[2],a[3],3);
            Face(b[2],b[1],b[0],3);Face(b[3],b[2],b[0],3);
            for(int i=0;i<4;i++){int j=(i+1)%4;Face(a[i],b[i],b[j],4);Face(a[i],b[j],a[j],4);}
        }
        var mesh=new Mesh{name="IdentificationDart_Detailed"};mesh.SetVertices(vertices);mesh.subMeshCount=5;
        for(int i=0;i<5;i++)mesh.SetTriangles(triangles[i],i);mesh.RecalculateNormals();mesh.RecalculateBounds();
        string meshPath=Folder+"/IdentificationDart_Detailed.asset";var saved=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
        if(saved){EditorUtility.CopySerialized(mesh,saved);UnityEngine.Object.DestroyImmediate(mesh);EditorUtility.SetDirty(saved);}else{saved=mesh;AssetDatabase.CreateAsset(saved,meshPath);}
        const string path="Assets/Ammo/Identification Dart.prefab";var root=PrefabUtility.LoadPrefabContents(path);
        try
        {
            root.GetComponent<MeshFilter>().sharedMesh=saved;root.GetComponent<MeshRenderer>().sharedMaterials=mats;
            var trail=root.GetComponent<TrailRenderer>();if(!trail)trail=root.AddComponent<TrailRenderer>();
            var trailMaterial=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Dart_Trail.mat");
            if(!trailMaterial){trailMaterial=new Material(Shader.Find("Sprites/Default"));AssetDatabase.CreateAsset(trailMaterial,Folder+"/Dart_Trail.mat");}
            trail.sharedMaterial=trailMaterial;trail.time=.10f;trail.startWidth=.035f;trail.endWidth=0;trail.minVertexDistance=.08f;
            trail.startColor=new Color(0,.85f,1,.65f);trail.endColor=new Color(0,.85f,1,0);trail.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            PrefabUtility.SaveAsPrefabAsset(root,path);
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}
        AssetDatabase.SaveAssets();
    }
}
