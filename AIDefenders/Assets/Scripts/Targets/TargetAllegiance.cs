using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Dataset-scoped roles. Species/class IDs stay stable when a role is edited.
public static class TargetAllegiance
{
    public const string FileName = "allegiances.json";
    [Serializable] public class Entry { public string label; public bool isEnemy; }
    [Serializable] public class Data { public List<Entry> entries = new List<Entry>(); }
    static readonly Dictionary<string, bool> active = new Dictionary<string, bool>(StringComparer.Ordinal);

    public static Data Read(string folder)
    {
        string path = Path.Combine(folder, FileName);
        if (!File.Exists(path)) return new Data(); // Legacy datasets retain prefab defaults.
        var data = JsonUtility.FromJson<Data>(File.ReadAllText(path));
        if (data == null || data.entries == null) throw new InvalidDataException("Invalid allegiance file");
        foreach (var entry in data.entries)
            if (entry == null || string.IsNullOrWhiteSpace(entry.label)) throw new InvalidDataException("Missing class name");
        return data;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Clear() { active.Clear(); }
    public static bool TryGet(string prefabName, out bool enemy)
    { return active.TryGetValue(prefabName.Replace("(Clone)", "").Trim(), out enemy); }

    // Retained for compatibility; learned roles must never override a stage's actual faction.
    public static void Configure(PythonRuntime.YoloClassMap map) { Clear(); }
}
