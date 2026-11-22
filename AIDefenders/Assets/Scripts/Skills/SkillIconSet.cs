using System;
using System.Collections.Generic;
using UnityEngine;

// Extracts runtime sprites from the intact source sheet. Clip regions remove neighboring icons.
public static class SkillIconSet
{
    [Serializable] class Sheet { public Entry[] entries; }
    [Serializable] class Entry
    {
        public string name;
        public int x, y, width, height;
        public Vector2[] vertices;
    }
    static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
    static bool loaded;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset() { loaded = false; sprites.Clear(); }

    public static Sprite Load(string resource)
    {
        if (string.IsNullOrEmpty(resource)) return null;
        if (!loaded) Initialize();
        string name = resource.Substring(resource.LastIndexOf('/') + 1);
        Sprite sprite;
        return sprites.TryGetValue(name, out sprite) ? sprite : null;
    }

    static void Initialize()
    {
        loaded = true;
        var texture = Resources.Load<Texture2D>("UI/SkillIcons/SkillIconSet");
        var json = Resources.Load<TextAsset>("UI/SkillIcons/SkillIconSet");
        if (texture != null && json != null)
        {
            var sheet = JsonUtility.FromJson<Sheet>(json.text);
            Color32[] source = texture.GetPixels32();
            foreach (var entry in sheet.entries)
            {
                var pixels = new Color32[entry.width * entry.height];
                // Each four-vertex region is an allowed rectangle in the source icon.
                for (int k = 0; k < entry.vertices.Length; k += 4)
                {
                    int left = Mathf.RoundToInt(entry.vertices[k].x * 100 + entry.width * .5f);
                    int bottom = Mathf.RoundToInt(entry.vertices[k].y * 100 + entry.height * .5f);
                    int right = Mathf.RoundToInt(entry.vertices[k+2].x * 100 + entry.width * .5f);
                    int top = Mathf.RoundToInt(entry.vertices[k+2].y * 100 + entry.height * .5f);
                    for (int y = bottom; y < top; y++)
                        Array.Copy(source, (entry.y + y) * texture.width + entry.x + left, pixels, y * entry.width + left, right - left);
                }
                var isolated = new Texture2D(entry.width, entry.height, TextureFormat.RGBA32, false);
                isolated.name = entry.name; isolated.wrapMode = TextureWrapMode.Clamp;
                isolated.SetPixels32(pixels); isolated.Apply(false, true);
                var sprite = Sprite.Create(isolated, new Rect(0, 0, entry.width, entry.height), new Vector2(.5f,.5f), 100, 0, SpriteMeshType.FullRect);
                sprite.name = entry.name;
                sprites[entry.name] = sprite;
            }
        }
        var missile = Resources.Load<Sprite>("UI/SkillIcons/BearTrapMissileModel");
        if (missile != null) sprites["BearTrapMissile"] = missile;
    }
}
