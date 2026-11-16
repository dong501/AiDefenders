using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;

public static class StageAtmosphereRefinement
{
    const string Folder="Assets/Art/StageAtmosphere";
    const string Desert="Assets/Scenes/GameStages/Desert/Desert_1.unity";
    const string City="Assets/Scenes/GameStages/City/City_1.unity";
    const string TerrainPath="Assets/Scenes/GameStages/Desert/Terrain_Desert_Day.asset";
    static Transform root;
    static Material concrete,roof,glass,metal,olive,trim;
    static TMP_FontAsset font;

    [MenuItem("Tools/AI Defenders/Environment Expansion/8 Natural Canyon And Urban GP")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop play mode first.");
        foreach(string path in new[]{Desert,City})
        {var s=SceneManager.GetSceneByPath(path);if(s.isLoaded&&s.isDirty)throw new InvalidOperationException("Unsaved scene: "+path);}
        Directory.CreateDirectory("Docs/StageAtmosphere/Before");
        foreach(string path in new[]{Desert,City,TerrainPath})
        {string backup="Docs/StageAtmosphere/Before/"+Path.GetFileName(path);if(!File.Exists(backup))File.Copy(path,backup);}
        Ensure(Folder);
        // Restore the original heightmap while retaining its GUID and references.
        File.Copy("Docs/CanyonCityUpgrade/Before/Terrain_Desert_Day.asset",TerrainPath,true);
        AssetDatabase.ImportAsset(TerrainPath,ImportAssetOptions.ForceUpdate);
        Edit(Desert,RefineDesert);Edit(City,RefineCity);AssetDatabase.SaveAssets();
    }
    static void Ensure(string p)
    {if(AssetDatabase.IsValidFolder(p))return;string parent=Path.GetDirectoryName(p).Replace('\\','/');Ensure(parent);AssetDatabase.CreateFolder(parent,Path.GetFileName(p));}
    static void Edit(string path,Action<Scene> edit)
    {
        var s=SceneManager.GetSceneByPath(path);bool opened=!s.isLoaded;
        if(opened)s=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
        var previous=SceneManager.GetActiveScene();SceneManager.SetActiveScene(s);
        try{edit(s);EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);}
        finally{SceneManager.SetActiveScene(previous);if(opened)EditorSceneManager.CloseScene(s,true);}
    }
    static Transform[] All(Scene s){return s.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();}
    static void Remember(Transform t)
    {if(PrefabUtility.IsPartOfPrefabInstance(t))PrefabUtility.RecordPrefabInstancePropertyModifications(t);}

    static float Distance(Vector3[][] paths,float x,float z)
    {
        float distance=float.MaxValue;var p=new Vector2(x,z);
        foreach(var path in paths)for(int i=1;i<path.Length;i++)
        {var a=new Vector2(path[i-1].x,path[i-1].z);var v=new Vector2(path[i].x,path[i].z)-a;float t=v.sqrMagnitude>0?Mathf.Clamp01(Vector2.Dot(p-a,v)/v.sqrMagnitude):0;distance=Mathf.Min(distance,Vector2.Distance(p,a+v*t));}
        return distance;
    }
    static void RefineDesert(Scene s)
    {
        string signature=EnvironmentExpansionBuilder.GameplaySignature(s);var all=All(s);
        var angular=s.GetRootGameObjects().FirstOrDefault(r=>r.name=="Desert_Grand_Canyon");if(angular)angular.SetActive(false);
        var outcrops=all.First(t=>t.name=="Sandstone_Outcrops");outcrops.gameObject.SetActive(true);
        var terrain=all.Select(t=>t.GetComponent<Terrain>()).First(t=>t);var data=terrain.terrainData;
        var paths=all.Select(t=>t.GetComponent<EnemySpawner>()).Where(p=>p).SelectMany(p=>p.paths.Select(q=>new[]{p.transform.position}.Concat(q.GetWorldPoints()).ToArray())).ToArray();
        int n=data.heightmapResolution;var heights=data.GetHeights(0,0,n,n);var origin=terrain.transform.position;var size=data.size;
        var banks=new[]{new Vector3(-49,8,-10),new Vector3(-33,8,19),new Vector3(22,10,-10),new Vector3(3,10,19)};
        // Broad rounded banks blend into the original eroded valley. No mesh cliffs,
        // straight cut faces or isolated circular platforms remain active.
        for(int j=0;j<n;j++)for(int i=0;i<n;i++)
        {
            float x=origin.x+i*size.x/(n-1),z=origin.z+j*size.z/(n-1);
            if(x < -85||x>60||z < -45||z>50)continue;
            float distance=Distance(paths,x,z);if(distance<=9)continue;
            float y=origin.y+heights[j,i]*size.y;float guard=Mathf.SmoothStep(0,1,Mathf.InverseLerp(9,20,distance));
            foreach(var bank in banks)
            {
                float r=Vector2.Distance(new Vector2(x,z),new Vector2(bank.x,bank.z));
                float blend=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(5,24,r));
                y=Mathf.Lerp(y,Mathf.Max(y,bank.y),blend*guard);
            }
            heights[j,i]=(y-origin.y)/size.y;
        }
        data.SetHeights(0,0,heights);EditorUtility.SetDirty(data);
        // Re-anchor supplied plants and props to the restored terrain.
        foreach(var group in s.GetRootGameObjects().Where(r=>r.name=="Environment_Asset_Expansion"||r.name=="Desert_Cactus_Upgrade"))
        foreach(Transform prop in group.transform)
        {
            var mf=prop.GetComponent<MeshFilter>();if(!mf)continue;var p=prop.position;
            p.y=terrain.SampleHeight(p)+origin.y-mf.sharedMesh.bounds.min.y*prop.lossyScale.y-.025f;prop.position=p;Remember(prop);
        }
        if(signature!=EnvironmentExpansionBuilder.GameplaySignature(s))throw new InvalidOperationException("Desert gameplay layout changed.");
        Debug.Log("Desert refined: original winding terrain restored; rounded high banks blended in; angular canyon meshes disabled.");
    }

    static Material Mat(string name,string hex,Texture2D texture=null)
    {
        string path=Folder+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
        Color c;ColorUtility.TryParseHtmlString(hex,out c);m.SetColor("_BaseColor",c);m.SetFloat("_Smoothness",.15f);m.enableInstancing=true;
        if(texture)m.SetTexture("_BaseMap",texture);EditorUtility.SetDirty(m);return m;
    }
    static Texture2D WindowTexture()
    {
        string path=Folder+"/Office_Window_Cell.png";var tex=new Texture2D(128,128,TextureFormat.RGB24,true);var pixels=new Color[128*128];
        for(int y=0;y<128;y++)for(int x=0;x<128;x++)
        {
            float grain=(Mathf.PerlinNoise(x*.47f,y*.43f)-.5f)*.04f;var c=new Color(.83f+grain,.84f+grain,.84f+grain);
            bool frame=x>=19&&x<=108&&y>=19&&y<=110;bool window=x>=24&&x<=103&&y>=26&&y<=105;
            if(frame)c=new Color(.53f,.56f,.57f);
            if(window){float v=.24f+.07f*y/128f+grain;c=new Color(v*.9f,v,v*1.04f);if(x==62||x==63)c=new Color(.42f,.46f,.48f);}
            if(y<4)c*=.88f;pixels[y*128+x]=c;
        }
        tex.SetPixels(pixels);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(path);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.wrapMode=TextureWrapMode.Repeat;importer.mipmapEnabled=true;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
    static void RefineCity(Scene s)
    {
        string signature=EnvironmentExpansionBuilder.GameplaySignature(s);var all=All(s);
        foreach(var cross in all.Where(t=>t.name.StartsWith("Shelter_MedicalCross_")))cross.gameObject.SetActive(false);
        var old=s.GetRootGameObjects().FirstOrDefault(r=>r.name=="City_Urban_Refinement");if(old)UnityEngine.Object.DestroyImmediate(old);
        root=new GameObject("City_Urban_Refinement").transform;
        concrete=Mat("GP_Concrete","#777E74");roof=Mat("Roof_Concrete","#969A98");glass=Mat("GP_Glass","#20353E");metal=Mat("GP_Metal","#303C38");olive=Mat("GP_Olive","#515D45");trim=Mat("GP_Trim","#B3AEA0");
        font=all.Select(t=>t.GetComponent<TMP_Text>()).Where(t=>t&&t.font).Select(t=>t.font).FirstOrDefault();
        BuildGP();
        var texture=WindowTexture();var facades=new[]{Mat("Office_Concrete","#A6AAA8",texture),Mat("Office_Brown","#A68C77",texture),Mat("Office_Brick","#957966",texture),Mat("Office_Slate","#798995",texture),Mat("Office_Pale","#C1BFB3",texture),Mat("Office_Grey","#8F9395",texture)};
        var buildings=all.First(t=>t.name=="City_Diverse_Skyline");int index=0,added=0;
        foreach(Transform building in buildings)
        {
            // Record an absolute original position, so subsequent runs do not drift.
            var marker=building.name.Split('_');int id;int.TryParse(marker[marker.Length-1],out id);
            if(id>32)
            {
                float h=building.GetComponent<MeshFilter>().sharedMesh.bounds.size.y*building.lossyScale.y;building.gameObject.SetActive(false);
                Tower("Skyline_"+id,new Vector3(building.position.x,.34f,building.position.z),25,23,h,facades[id%facades.Length]);continue;
            }
            float blockZ=-220+Mathf.Round((building.position.z+220)/80)*80;
            var pos=building.position;pos.z=blockZ+(Mathf.Abs(pos.x)<45&&blockZ==-60?15:-15);building.position=pos;Remember(building);
            // Commercial blocks have a second row of taller office and residential slabs.
            if(!(Mathf.Abs(pos.x)<45 && (blockZ==-60||blockZ==-140)))
            {
                float height=new[]{31f,43f,55f,37f,64f,48f}[index%6];
                Tower("Urban_Block_"+id,new Vector3(pos.x,.34f,blockZ+19),22+(index%2)*2,22,height,facades[index%facades.Length]);added++;
            }
            index++;
        }
        // Less saturated light and a grey-blue haze tie the varied assets together.
        RenderSettings.fogColor=new Color(.55f,.59f,.61f);RenderSettings.fogDensity=.0027f;
        RefineStreets(all);
        if(signature!=EnvironmentExpansionBuilder.GameplaySignature(s))throw new InvalidOperationException("City gameplay layout changed.");
        File.WriteAllText("Docs/StageAtmosphere/city_summary.txt","Original models: 32\nAdded urban towers: "+added+"\nRetextured skyline towers: 8\nCentral GP: 1\nGround cross: hidden\nGameplay layout signature unchanged.");
        Debug.Log("Urban refinement: "+added+" dense window-grid towers, eight revised skyline towers and a central GP building. Four approach routes preserved.");
    }
    static void Box(string name,Vector3 p,Vector3 scale,Material m,Quaternion? rotation=null)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(root,false);go.transform.position=p;go.transform.localScale=scale;if(rotation.HasValue)go.transform.rotation=rotation.Value;
        go.layer=2;UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());go.GetComponent<Renderer>().sharedMaterial=m;
    }
    static Texture2D StreetTexture(string name,bool pavement)
    {
        string path=Folder+"/"+name+".png";var tex=new Texture2D(128,128,TextureFormat.RGB24,true);var colors=new Color[128*128];
        for(int y=0;y<128;y++)for(int x=0;x<128;x++)
        {
            float v=.86f+(Mathf.PerlinNoise(x*.67f,y*.63f)-.5f)*.17f;
            if(pavement&&(x%32<2||y%32<2))v*=.77f;
            colors[y*128+x]=new Color(v,v,v);
        }
        tex.SetPixels(colors);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(path);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.wrapMode=TextureWrapMode.Repeat;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
    static void RefineStreets(Transform[] all)
    {
        var street=all.First(t=>t.name=="City_Street_Grid");var filter=street.GetComponent<MeshFilter>();
        var copy=UnityEngine.Object.Instantiate(filter.sharedMesh);copy.name="Urban_Street_Grid";
        copy.uv=copy.vertices.Select(v=>new Vector2(v.x/4,v.z/4)).ToArray();
        string path=Folder+"/Urban_Street_Grid.asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(mesh){EditorUtility.CopySerialized(copy,mesh);UnityEngine.Object.DestroyImmediate(copy);EditorUtility.SetDirty(mesh);}else{mesh=copy;AssetDatabase.CreateAsset(mesh,path);}filter.sharedMesh=mesh;
        var renderer=street.GetComponent<Renderer>();var materials=renderer.sharedMaterials;
        materials[0]=Mat("Urban_Asphalt","#62676B",StreetTexture("Asphalt_Grain",false));
        materials[1]=Mat("Urban_Pavement","#B4B6B2",StreetTexture("Pavement_Tiles",true));renderer.sharedMaterials=materials;
        if(PrefabUtility.IsPartOfPrefabInstance(filter)){PrefabUtility.RecordPrefabInstancePropertyModifications(filter);PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);}
    }
    static void BuildGP()
    {
        Vector3 c=new Vector3(0,0,-100);
        Box("GP_Foundation",c+Vector3.up*.65f,new Vector3(12, .6f,12),concrete);
        Box("GP_Main_Bunker",c+Vector3.up*3.2f,new Vector3(10.8f,5,10.8f),concrete);
        Box("GP_Roof_Slab",c+Vector3.up*5.85f,new Vector3(11.8f,.45f,11.8f),olive);
        Box("GP_Observation_Room",c+Vector3.up*7.3f,new Vector3(6.3f,2.5f,6.3f),concrete);
        Box("GP_Upper_Roof",c+Vector3.up*8.75f,new Vector3(7.3f,.4f,7.3f),olive);
        Box("GP_Entrance",c+new Vector3(0,2.1f,-5.46f),new Vector3(1.9f,2.8f,.12f),metal);
        Box("GP_Door_Trim",c+new Vector3(0,3.65f,-5.5f),new Vector3(2.5f,.25f,.2f),trim);
        Box("GP_Entrance_Canopy",c+new Vector3(0,4.4f,-6.2f),new Vector3(3.7f,.25f,1.8f),olive);
        for(int side=0;side<4;side++)
        {
            var rot=Quaternion.Euler(0,side*90,0);
            foreach(float x in new[]{-3.4f,3.4f})Box("GP_Lower_Observation_Slit",c+rot*new Vector3(x,3.65f,-5.44f),new Vector3(1.9f,.65f,.12f),glass,rot);
            Box("GP_Upper_Observation_Window",c+rot*new Vector3(0,7.5f,-3.19f),new Vector3(4.9f,.9f,.12f),glass,rot);
            foreach(float x in new[]{-2.4f,0,2.4f})Box("GP_Window_Divider",c+rot*new Vector3(x,7.5f,-3.27f),new Vector3(.12f,1.05f,.1f),olive,rot);
            for(int j=0;j<7;j++)Box("GP_Roof_Sandbag",c+rot*new Vector3(-4.8f+j*1.6f,6.35f,-5.25f),new Vector3(1.45f,.48f,.8f),olive,rot);
        }
        for(int j=0;j<3;j++)Box("GP_Entrance_Step",c+new Vector3(0,.35f+j*.18f,-7+j*.45f),new Vector3(2.8f,.25f+j*.36f,1),concrete);
        Box("GP_Antenna_Mast",c+new Vector3(1.8f,11.2f,1.8f),new Vector3(.12f,4.6f,.12f),metal);
        Box("GP_Antenna_Crossbar",c+new Vector3(1.8f,12.5f,1.8f),new Vector3(2.5f,.08f,.08f),metal);
        Box("GP_Radio_Cabinet",c+new Vector3(-2,9.25f,2),new Vector3(1.1f,.8f,.8f),metal);
        for(int side=-1;side<=1;side+=2)
        {
            Box("GP_Floodlight_Arm",c+new Vector3(side*3.4f,8.9f,-3.4f),new Vector3(.15f,.8f,.15f),metal);
            Box("GP_Floodlight",c+new Vector3(side*3.4f,9.3f,-3.45f),new Vector3(.65f,.38f,.3f),trim);
        }
        if(font)
        {
            var go=new GameObject("GP_Command_Sign",typeof(RectTransform),typeof(TextMeshPro));go.transform.SetParent(root);go.transform.position=c+new Vector3(0,4.95f,-5.48f);go.layer=2;
            var label=go.GetComponent<TextMeshPro>();label.font=font;label.text="경계 지휘소  /  GP-01";label.fontSize=2.3f;label.alignment=TextAlignmentOptions.Center;label.color=new Color(.9f,.89f,.79f);label.rectTransform.sizeDelta=new Vector2(9,.8f);
        }
    }
    static void Tower(string name,Vector3 pos,float width,float depth,float height,Material facade)
    {
        var vs=new List<Vector3>();var uv=new List<Vector2>();var tris=new List<int>();
        Action<Vector3,Vector3,Vector3,Vector3,float> wall=(a,b,c,d,length)=>
        {int n=vs.Count;vs.AddRange(new[]{a,b,c,d});uv.AddRange(new[]{Vector2.zero,new Vector2(length/3.4f,0),new Vector2(length/3.4f,height/3.3f),new Vector2(0,height/3.3f)});tris.AddRange(new[]{n,n+2,n+1,n,n+3,n+2});};
        float x=width*.5f,z=depth*.5f;
        wall(new Vector3(-x,0,-z),new Vector3(x,0,-z),new Vector3(x,height,-z),new Vector3(-x,height,-z),width);
        wall(new Vector3(x,0,z),new Vector3(-x,0,z),new Vector3(-x,height,z),new Vector3(x,height,z),width);
        wall(new Vector3(-x,0,z),new Vector3(-x,0,-z),new Vector3(-x,height,-z),new Vector3(-x,height,z),depth);
        wall(new Vector3(x,0,-z),new Vector3(x,0,z),new Vector3(x,height,z),new Vector3(x,height,-z),depth);
        var mesh=new Mesh();mesh.name=name;mesh.SetVertices(vs);mesh.SetUVs(0,uv);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
        string path=Folder+"/"+name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(existing){EditorUtility.CopySerialized(mesh,existing);UnityEngine.Object.DestroyImmediate(mesh);mesh=existing;EditorUtility.SetDirty(mesh);}else AssetDatabase.CreateAsset(mesh,path);
        var go=new GameObject(name);go.transform.SetParent(root);go.transform.position=pos;go.layer=2;go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=facade;
        Box(name+"_Roof",pos+Vector3.up*(height+.15f),new Vector3(width+.25f,.3f,depth+.25f),roof);
        Box(name+"_Rooftop_Plant",pos+new Vector3(width*.18f,height+1.1f,depth*.12f),new Vector3(width*.25f,1.9f,depth*.23f),concrete);
        foreach(int side in new[]{-1,1})
        {
            Box(name+"_Roof_Parapet",pos+new Vector3(side*x,height+.5f,0),new Vector3(.28f,.75f,depth),roof);
            Box(name+"_Roof_Parapet",pos+new Vector3(0,height+.5f,side*z),new Vector3(width,.75f,.28f),roof);
        }
        // Keep the complete tower together when its street clearance is adjusted.
        foreach(var part in root.Cast<Transform>().Where(t=>t.name.StartsWith(name+"_Roof",StringComparison.Ordinal)).ToArray())
            part.SetParent(go.transform,true);
    }
}
