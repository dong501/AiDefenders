using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 데이터셋 생성 UI의 게임 스테이지 목록입니다.
/// 스테이지를 추가할 때는 여기에 한 줄을 넣고, 등장 대상은 해당 StageData의 웨이브 spawn prefab에서 읽습니다.
/// </summary>
public static class DatasetStageCatalog
{
    public class Entry
    {
        public string displayName;
        public string stageDataPath;
    }

    public static readonly Entry[] Stages =
    {
        new Entry { displayName = "초원 1", stageDataPath = "Assets/Waves/StageData_GrassField_1.asset" },
        new Entry { displayName = "초원 2", stageDataPath = "Assets/Waves/StageData_GrassField_2.asset" },
        new Entry { displayName = "초원 3", stageDataPath = "Assets/Waves/StageData_GrassField_3.asset" },
        new Entry { displayName = "사막 1", stageDataPath = "Assets/Waves/StageData_Desert_1.asset" },
        new Entry { displayName = "사막 2", stageDataPath = "Assets/Waves/StageData_Desert_2.asset" },
        new Entry { displayName = "사막 3", stageDataPath = "Assets/Waves/StageData_Desert_3.asset" },
        new Entry { displayName = "도시 1", stageDataPath = "Assets/Waves/StageData_City_1.asset" },
        new Entry { displayName = "도시 2", stageDataPath = "Assets/Waves/StageData_City_2.asset" },
        new Entry { displayName = "도시 3", stageDataPath = "Assets/Waves/StageData_City_3.asset" }
    };

    public static StageData LoadStageData(Entry entry)
    {
        if (entry == null || string.IsNullOrEmpty(entry.stageDataPath))
            return null;

#if UNITY_EDITOR
        var editorData = UnityEditor.AssetDatabase.LoadAssetAtPath<StageData>(entry.stageDataPath);
        if (editorData != null) return editorData;
#endif
        return Resources.Load<StageData>("CampaignStages/" + System.IO.Path.GetFileNameWithoutExtension(entry.stageDataPath));
    }

    public static void CollectTargets(Entry entry, List<Target_Base> results)
    {
        StageData data = LoadStageData(entry);
        if (data != null)
            data.CollectSpawnTargets(results);
    }
}
