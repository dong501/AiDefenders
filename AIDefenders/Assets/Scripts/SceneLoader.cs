using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    static SceneLoader instance;
    static bool loading;
    public static string PreviousSceneName { get; private set; } = "MainMenu";

    CanvasGroup overlay;
    Image fill;
    Text label;
    Text percent;

    public static void Load(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("불러올 씬 이름이 비어 있습니다.");
            return;
        }

        if (loading)
            return;

        string current = SceneManager.GetActiveScene().name;
        if (current != sceneName)
            PreviousSceneName = current;
        EnsureInstance();
        instance.StartCoroutine(instance.LoadRoutine(sceneName));
    }

    static void EnsureInstance()
    {
        if (instance != null)
            return;

        GameObject host = new GameObject("SceneLoader");
        instance = host.AddComponent<SceneLoader>();
        DontDestroyOnLoad(host);
        instance.BuildOverlay();
    }

    IEnumerator LoadRoutine(string sceneName)
    {
        loading = true;
        ShowOverlay(sceneName);
        SetProgress(0f);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        if (op == null)
        {
            Debug.LogError("씬을 찾지 못했습니다: " + sceneName + " (Build Settings에 추가되어 있는지 확인하세요.)");
            HideOverlay();
            loading = false;
            yield break;
        }

        op.allowSceneActivation = false;
        while (op.progress < 0.9f)
        {
            SetProgress(Mathf.Clamp01(op.progress / 0.9f));
            yield return null;
        }

        SetProgress(1f);
        yield return new WaitForSecondsRealtime(0.2f);
        op.allowSceneActivation = true;
        while (!op.isDone)
            yield return null;

        HideOverlay();
        loading = false;
    }

    void ShowOverlay(string sceneName)
    {
        if (overlay == null)
            BuildOverlay();

        overlay.gameObject.SetActive(true);
        overlay.alpha = 1f;
        overlay.blocksRaycasts = true;
        if (label != null)
            label.text = LoadingLabel(sceneName);
        SetProgress(0f);
    }

    public static string LoadingLabel(string sceneName)
    {
        if(string.IsNullOrWhiteSpace(sceneName))return "로딩 중";
        string name=System.IO.Path.GetFileNameWithoutExtension(sceneName).ToLowerInvariant();
        if(name.Contains("grassfield") || name.Contains("grassfiled") || name.Contains("grassland"))return "초원 로딩 중";
        if(name.Contains("desert"))return "사막 로딩 중";
        if(name.Contains("city"))return "도시 로딩 중";
        if(name.Contains("turretlab"))return "포탑 연구소 로딩 중";
        if(name.Contains("dataset"))return "데이터셋 팩토리 로딩 중";
        if(name.Contains("mainmenu"))return "메인 화면 로딩 중";
        return "로딩 중";
    }

    void HideOverlay()
    {
        if (overlay == null)
            return;

        overlay.alpha = 0f;
        overlay.blocksRaycasts = false;
        overlay.gameObject.SetActive(false);
    }

    void SetProgress(float t)
    {
        t = Mathf.Clamp01(t);
        if (fill != null)
            fill.fillAmount = t;
        if (percent != null)
            percent.text = Mathf.RoundToInt(t * 100f) + "%";
    }

    void BuildOverlay()
    {
        GameObject canvasGo = new GameObject("LoadingOverlay");
        canvasGo.transform.SetParent(transform, false);

        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32000;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        overlay = canvasGo.AddComponent<CanvasGroup>();

        Image dim = UiFont.AddImage(canvasGo.transform, "Dim", UiSkin.Navy);
        UiFont.Stretch(dim.rectTransform);

        GameObject center = new GameObject("Center");
        RectTransform centerRt = center.AddComponent<RectTransform>();
        centerRt.SetParent(canvasGo.transform, false);
        centerRt.anchorMin = new Vector2(0.5f, 0.5f);
        centerRt.anchorMax = new Vector2(0.5f, 0.5f);
        centerRt.sizeDelta = new Vector2(640f, 180f);

        label = UiFont.AddText(centerRt, "Label", "로딩 중", 36, TextAnchor.MiddleCenter, Color.white, UiType.Title);
        label.rectTransform.anchorMin = new Vector2(0f, 0.55f);
        label.rectTransform.anchorMax = new Vector2(1f, 1f);
        label.rectTransform.offsetMin = Vector2.zero;
        label.rectTransform.offsetMax = Vector2.zero;

        percent = UiFont.AddText(centerRt, "Percent", "0%", 22, TextAnchor.MiddleCenter, new Color(0.85f, 0.88f, 0.92f, 1f), UiType.BodyMedium);
        percent.rectTransform.anchorMin = new Vector2(0f, 0.32f);
        percent.rectTransform.anchorMax = new Vector2(1f, 0.55f);
        percent.rectTransform.offsetMin = Vector2.zero;
        percent.rectTransform.offsetMax = Vector2.zero;

        Image track = UiFont.AddImage(centerRt, "Track", new Color(1f, 1f, 1f, 0.18f));
        track.rectTransform.anchorMin = new Vector2(0f, 0.08f);
        track.rectTransform.anchorMax = new Vector2(1f, 0.28f);
        track.rectTransform.offsetMin = Vector2.zero;
        track.rectTransform.offsetMax = Vector2.zero;

        fill = UiFont.AddImage(track.transform, "Fill", UiSkin.Gold);
        UiFont.Stretch(fill.rectTransform);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = 0;
        fill.fillAmount = 0f;

        HideOverlay();
    }
}
