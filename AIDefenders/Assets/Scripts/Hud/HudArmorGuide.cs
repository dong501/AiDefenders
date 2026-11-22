using UnityEngine;
using UnityEngine.UI;

/// <summary>Shows counterplay before combat and contextual help on a blocked shot.</summary>
public sealed class HudArmorGuide
{
    readonly WaveDirector director;
    readonly TurretPlatformPlacer placer;
    readonly GameManager manager;
    readonly GameObject root;
    readonly Text title;
    readonly Text body;
    float blockedUntil;
    float blockedArmor;
    int lastWave = -2;
    bool wasPrep;

    public HudArmorGuide(RectTransform parent, WaveDirector director, TurretPlatformPlacer placer, GameManager manager)
    {
        this.director = director; this.placer = placer; this.manager = manager;
        var panel = UiFont.AddImage(parent, "ArmorGuide", new Color(.09f,.16f,.24f,.96f));
        panel.raycastTarget = false; root = panel.gameObject;
        UiSkin.Place(panel.rectTransform, new Vector2(.43f,1f), new Vector2(0,-178), new Vector2(730,112));
        title = UiFont.AddText(panel.transform,"Title","",23,TextAnchor.UpperLeft,UiSkin.Gold,UiType.Title);
        title.raycastTarget = false;
        UiFont.Stretch(title.rectTransform);title.rectTransform.offsetMin = new Vector2(18,75);title.rectTransform.offsetMax = new Vector2(-18,-10);
        body = UiFont.AddText(panel.transform,"Body","",19,TextAnchor.UpperLeft,Color.white,UiType.Body);
        body.raycastTarget = false;body.horizontalOverflow = HorizontalWrapMode.Wrap;
        UiFont.Stretch(body.rectTransform);body.rectTransform.offsetMin = new Vector2(18,8);body.rectTransform.offsetMax = new Vector2(-18,-41);
        root.SetActive(false);HudFx.ArmorBlocked += OnBlocked;
    }

    void OnBlocked(float armor)
    {
        blockedArmor = armor; blockedUntil = Time.unscaledTime + 6f;
    }

    public void Tick()
    {
        if (director == null || (manager != null && manager.IsModelPromptOpen) || director.Phase == WavePhase.Complete || director.Phase == WavePhase.Idle)
        { root.SetActive(false);return; }
        bool prep = director.Phase == WavePhase.Prep;
        if (prep)
        {
            if (!wasPrep || lastWave != director.CurrentWaveIndex)
            {
                float armor = ArmorGuidance.WaveArmor(director);
                root.SetActive(armor > 0f);
                title.text = "장갑 대상 등장 · 최대 장갑 " + armor.ToString("0.##");
                body.text = "관통력이 장갑보다 높아야 피해를 줍니다. 같거나 낮으면 피해 0.\n대응 추천: " + ArmorGuidance.Recommendation(armor, placer != null ? placer.turretOptions : null);
                lastWave = director.CurrentWaveIndex;
            }
        }
        else
        {
            bool show = Time.unscaledTime < blockedUntil;root.SetActive(show);
            if (show)
            {
                title.text = "관통력 부족 · 장갑 " + blockedArmor.ToString("0.##") + "에 공격이 막혔습니다";
                body.text = "현재 포탑의 관통력이 부족합니다. 고관통 포탑으로 교체하거나 업그레이드하세요.\n대응 추천: " + ArmorGuidance.Recommendation(blockedArmor, placer != null ? placer.turretOptions : null);
            }
        }
        wasPrep = prep;
    }

    public void Dispose() { HudFx.ArmorBlocked -= OnBlocked; }
}
