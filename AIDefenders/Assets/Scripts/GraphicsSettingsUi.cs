using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class GraphicsSettingsUi
{
    const string WidthKey = "Graphics.Width";
    const string HeightKey = "Graphics.Height";
    const string ModeKey = "Graphics.ScreenMode";
    const string QualityKey = "Graphics.Quality";

    static readonly FullScreenMode[] Modes = {
        FullScreenMode.FullScreenWindow, FullScreenMode.Windowed, FullScreenMode.ExclusiveFullScreen
    };
    static readonly string[] ModeLabels = { "테두리 없는 전체 화면", "창 모드", "전체 화면" };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Load()
    {
        string[] qualityNames = QualitySettings.names;
        string savedQuality = PlayerPrefs.GetString(QualityKey, "");
        int quality = Array.IndexOf(qualityNames, savedQuality);
        if (quality >= 0) QualitySettings.SetQualityLevel(quality, true);
        int width = PlayerPrefs.GetInt(WidthKey, 0);
        int height = PlayerPrefs.GetInt(HeightKey, 0);
        var mode = (FullScreenMode)PlayerPrefs.GetInt(ModeKey, (int)Screen.fullScreenMode);
        if (width > 0 && height > 0 && Array.IndexOf(Modes, mode) >= 0)
            Screen.SetResolution(width, height, mode);
    }

    public static GameObject Create(Transform parent)
    {
        var panel = new GameObject("GraphicsTab", typeof(RectTransform));
        panel.transform.SetParent(parent, false);
        UiFont.Stretch(panel.GetComponent<RectTransform>());

        Vector2Int current = new Vector2Int(Screen.width, Screen.height);
        var sizes = new List<Vector2Int> { current };
        foreach (Resolution resolution in Screen.resolutions)
        {
            var size = new Vector2Int(resolution.width, resolution.height);
            if (!sizes.Contains(size)) sizes.Add(size);
        }
        sizes.Sort((left, right) => {
            int byWidth = left.x.CompareTo(right.x);
            return byWidth != 0 ? byWidth : left.y.CompareTo(right.y);
        });
        string[] resolutions = sizes.ConvertAll(size => size.x + " × " + size.y).ToArray();
        string[] qualityNames = QualitySettings.names;
        string[] qualityLabels = Array.ConvertAll(qualityNames, name => name == "Mobile" ? "낮음" : name == "PC" ? "높음" : name);
        var status = UiFont.AddText(panel.transform, "Status", "", 21, TextAnchor.MiddleCenter, UiSkin.Muted, UiType.Body);
        UiSkin.Place(status.rectTransform, new Vector2(.5f, .23f), Vector2.zero, new Vector2(760, 40));
        Action selectionChanged = () => status.text = "";
        var getMode = AddChoice(panel.transform, "ScreenMode", "화면 모드", .60f, ModeLabels,
            Mathf.Max(0, Array.IndexOf(Modes, Screen.fullScreenMode)), selectionChanged);
        var getResolution = AddChoice(panel.transform, "Resolution", "해상도", .48f, resolutions, sizes.IndexOf(current), selectionChanged);
        var getQuality = AddChoice(panel.transform, "Quality", "그래픽 품질", .36f, qualityLabels, QualitySettings.GetQualityLevel(), selectionChanged);
        UiSkin.Button(panel.transform, "ApplyGraphics", "적용", UiSkin.Blue, new Vector2(.5f, .16f),
            Vector2.zero, new Vector2(300, 54)).onClick.AddListener(() =>
        {
            Vector2Int size = sizes[getResolution()];
            FullScreenMode mode = Modes[getMode()];
            int quality = getQuality();
            QualitySettings.SetQualityLevel(quality, true);
            Screen.SetResolution(size.x, size.y, mode);
            PlayerPrefs.SetInt(WidthKey, size.x);
            PlayerPrefs.SetInt(HeightKey, size.y);
            PlayerPrefs.SetInt(ModeKey, (int)mode);
            PlayerPrefs.SetString(QualityKey, qualityNames[quality]);
            PlayerPrefs.Save();
            status.text = "그래픽 설정을 저장했습니다.";
        });
        return panel;
    }

    static Func<int> AddChoice(Transform panel, string name, string title, float y,
        string[] options, int initial, Action selectionChanged)
    {
        var anchor = new Vector2(.5f, y);
        var caption = UiFont.AddText(panel, name + "Label", title, 27, TextAnchor.MiddleLeft, Color.white, UiType.BodyBold);
        UiSkin.Place(caption.rectTransform, anchor, new Vector2(-265f, 0f), new Vector2(275f, 55f));
        var value = UiFont.AddText(panel, name + "Value", "", 24, TextAnchor.MiddleCenter, Color.white, UiType.Body);
        UiSkin.Place(value.rectTransform, anchor, new Vector2(145f, 0f), new Vector2(350f, 55f));
        int index = Mathf.Clamp(initial, 0, options.Length - 1);
        value.text = options[index];
        void Select(int direction)
        {
            index = (index + direction + options.Length) % options.Length;
            value.text = options[index];
            selectionChanged();
        }
        var previous = UiSkin.Button(panel, name + "Previous", "‹", UiSkin.Navy, anchor, new Vector2(-63f, 0f), new Vector2(58f, 54f));
        var next = UiSkin.Button(panel, name + "Next", "›", UiSkin.Navy, anchor, new Vector2(353f, 0f), new Vector2(58f, 54f));
        previous.onClick.AddListener(() => Select(-1));
        next.onClick.AddListener(() => Select(1));
        previous.interactable = next.interactable = options.Length > 1;
        return () => index;
    }
}
