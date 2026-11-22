using System;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class TurretLabResearch
{
    public string displayName;
    public string description;
    public int cost;
}

[System.Serializable]
public class TurretLabEntry
{
    public string displayName;
    public GameObject prefab;
    public GameObject previewPrefab;
    public bool isCctv;
    public TurretLabResearch[] researches;

    public string Label
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(displayName))
                return displayName;
            return prefab != null ? prefab.name : "포탑";
        }
    }
}

public class MenuManager_TurretLab : MonoBehaviour
{
    public TurretPlatform previewPlatform;
    public TurretLabEntry[] turretCatalog;

    readonly Color panelColor = UiSkin.Panel;
    readonly Color rowColor = UiSkin.Blue;
    readonly Color selectedColor = UiSkin.Sky;
    readonly Color textColor = new Color(0.94f, 0.95f, 0.97f, 1f);
    readonly Color mutedColor = UiSkin.Muted;

    TurretStatComparison statComparison;
    Transform turretListContent;
    Transform researchListContent;
    Transform appearanceContent;
    Text pointsText;
    Text detailTitle;
    Text detailBody;
    Image detailArtwork;
    Button detailAction;
    Text detailActionLabel;
    Transform previewLevelRow;
    Image[] turretRowImages = new Image[0];
    int selectedIndex = -1;
    int previewLevel = 1;
    string selectedResearchId;
    string selectedPrimaryId;
    GameObject previewInstance;
    GameObject confirmRoot;
    Action pendingConfirm;

    void Start()
    {
        EnsureDefaultCatalog();
        if (previewPlatform == null)
            previewPlatform = FindFirstObjectByType<TurretPlatform>();

        ConfigureCanvas();
        StripLegacyHud();
        CreateNavButtons();
        BuildLabPanel();
        var statCanvas = FindCanvas();
        if (statCanvas != null) statComparison = new TurretStatComparison(statCanvas.transform, turretCatalog);

        if (turretCatalog != null && turretCatalog.Length > 0)
            SelectTurret(0);
        else
            RefreshResearchList();
    }

    public void LoadScene(string sceneName)
    {
        SceneLoader.Load(sceneName);
    }

    void EnsureDefaultCatalog()
    {
        if (turretCatalog != null && turretCatalog.Length > 0)
            return;

        turretCatalog = new TurretLabEntry[0];
    }

    void ConfigureCanvas()
    {
        Canvas canvas = FindCanvas();
        if (canvas == null)
            return;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        Transform panel = canvas.transform.Find("Panel");
        if (panel != null)
        {
            Image image = panel.GetComponent<Image>();
            if (image != null)
                image.raycastTarget = false;
        }
    }

    void BuildLabPanel()
    {
        Canvas canvas = FindCanvas();
        if (canvas == null)
            return;

        Transform host = canvas.transform.Find("Panel");
        if (host == null)
            host = canvas.transform;

        Transform existing = host.Find("Panel_TurretLab");
        if (existing != null)
            Destroy(existing.gameObject);

        Image sidebar = UiFont.AddImage(host, "Panel_TurretLab", panelColor);
        RectTransform sidebarRt = sidebar.rectTransform;
        sidebarRt.anchorMin = new Vector2(1f, 0f);
        sidebarRt.anchorMax = new Vector2(1f, 1f);
        sidebarRt.pivot = new Vector2(1f, 0.5f);
        sidebarRt.sizeDelta = new Vector2(520f, 0f);
        sidebarRt.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup layout = sidebar.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 24, 24);
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        AddSectionLabel(sidebarRt, "포탑");
        turretListContent = CreateScrollSection(sidebarRt, "TurretList", 240f);
        AddSectionLabel(sidebarRt, "연구");
        researchListContent = CreateScrollSection(sidebarRt, "ResearchList", 380f);

        Image pointsBar = UiFont.AddImage(sidebarRt, "ResearchPoints", new Color(.36f, .16f, .65f, 1f));
        LayoutElement pointsLayout = pointsBar.gameObject.AddComponent<LayoutElement>();
        pointsLayout.minHeight = 52f;
        pointsLayout.preferredHeight = 52f;

        pointsText = UiFont.AddText(
            pointsBar.rectTransform,
            "Value",
            ResearchPointsLabel(),
            22,
            TextAnchor.MiddleLeft,
            textColor,
            UiType.BodyMedium);
        UiFont.Stretch(pointsText.rectTransform);
        pointsText.rectTransform.offsetMin = new Vector2(16f, 0f);
        pointsText.rectTransform.offsetMax = new Vector2(-16f, 0f);

        BuildAppearanceSection(sidebarRt);
        BuildDetailHud(canvas.transform);
        RebuildTurretList();
        RefreshResearchPoints();
    }

    void BuildDetailHud(Transform canvas)
    {
        Transform existing = canvas.Find("DetailHud");
        if (existing != null)
            Destroy(existing.gameObject);

        Image panel = UiFont.AddImage(canvas, "DetailHud", panelColor);
        UiSkin.Surface(panel);
        RectTransform rt = panel.rectTransform;
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.offsetMin = new Vector2(690f, 16f);
        rt.offsetMax = new Vector2(-544f, 236f);
        panel.raycastTarget = true;

        detailArtwork = UiFont.AddImage(rt, "SkillArtwork", Color.white);
        detailArtwork.preserveAspect = true;
        detailArtwork.raycastTarget = false;
        UiSkin.Place(detailArtwork.rectTransform, new Vector2(0f, 0.5f), new Vector2(80f, 0f), new Vector2(128f, 128f));
        detailArtwork.gameObject.SetActive(false);

        detailTitle = UiFont.AddText(rt, "Title", "상세 정보", 26, TextAnchor.UpperLeft, textColor, UiType.Title);
        detailTitle.raycastTarget = false;
        RectTransform titleRt = detailTitle.rectTransform;
        titleRt.anchorMin = new Vector2(0f, 1f);
        titleRt.anchorMax = new Vector2(1f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0f, -16f);
        titleRt.sizeDelta = new Vector2(-32f, 36f);

        detailBody = UiFont.AddText(rt, "Body", "포탑이나 연구를 선택하면 설명이 표시됩니다.", 18, TextAnchor.UpperLeft, mutedColor, UiType.Body);
        detailBody.raycastTarget = false;
        detailBody.horizontalOverflow = HorizontalWrapMode.Wrap;
        detailBody.verticalOverflow = VerticalWrapMode.Overflow;
        RectTransform bodyRt = detailBody.rectTransform;
        bodyRt.anchorMin = new Vector2(0f, 0f);
        bodyRt.anchorMax = new Vector2(1f, 1f);
        bodyRt.offsetMin = new Vector2(16f, 58f);
        bodyRt.offsetMax = new Vector2(-16f, -56f);

        GameObject levels = new GameObject("PreviewLevels", typeof(RectTransform));
        RectTransform levelsRt = levels.GetComponent<RectTransform>();
        levelsRt.SetParent(rt, false);
        levelsRt.anchorMin = new Vector2(0f, 0f);
        levelsRt.anchorMax = new Vector2(0.58f, 0f);
        levelsRt.pivot = new Vector2(0f, 0f);
        levelsRt.offsetMin = new Vector2(16f, 12f);
        levelsRt.offsetMax = new Vector2(-8f, 48f);
        HorizontalLayoutGroup levelLayout = levels.AddComponent<HorizontalLayoutGroup>();
        levelLayout.spacing = 6f;
        levelLayout.childAlignment = TextAnchor.MiddleLeft;
        levelLayout.childControlWidth = true;
        levelLayout.childControlHeight = true;
        levelLayout.childForceExpandWidth = true;
        levelLayout.childForceExpandHeight = true;
        previewLevelRow = levelsRt;
        levels.SetActive(false);

        detailAction = UiSkin.Button(rt, "Action", "진행", UiSkin.Blue, new Vector2(1f, 0f), new Vector2(-118f, 30f), new Vector2(200f, 44f));
        detailActionLabel = detailAction.GetComponentInChildren<Text>();
        detailAction.gameObject.SetActive(false);
    }

    void AddSectionLabel(Transform parent, string label)
    {
        Text text = UiFont.AddText(parent, label + "Header", label, 26, TextAnchor.MiddleLeft, textColor, UiType.Title);
        LayoutElement layout = text.gameObject.AddComponent<LayoutElement>();
        layout.minHeight = 36f;
        layout.preferredHeight = 36f;
    }

    Transform CreateScrollSection(Transform parent, string name, float height)
    {
        Image frame = UiFont.AddImage(parent, name, UiSkin.Navy);
        LayoutElement frameLayout = frame.gameObject.AddComponent<LayoutElement>();
        frameLayout.minHeight = 120f;
        frameLayout.preferredHeight = height;
        frameLayout.flexibleHeight = name == "TurretList" ? 0f : 1f;

        Mask mask = frame.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = true;
        ScrollRect scroll = frame.gameObject.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 24f;

        GameObject content = new GameObject("Content");
        RectTransform contentRt = content.AddComponent<RectTransform>();
        contentRt.SetParent(frame.rectTransform, false);
        contentRt.anchorMin = new Vector2(0f, 1f);
        contentRt.anchorMax = new Vector2(1f, 1f);
        contentRt.pivot = new Vector2(0.5f, 1f);
        contentRt.anchoredPosition = Vector2.zero;
        contentRt.sizeDelta = new Vector2(0f, 0f);

        VerticalLayoutGroup list = content.AddComponent<VerticalLayoutGroup>();
        list.padding = new RectOffset(8, 8, 8, 8);
        list.spacing = 8f;
        list.childAlignment = TextAnchor.UpperCenter;
        list.childControlWidth = true;
        list.childControlHeight = true;
        list.childForceExpandWidth = true;
        list.childForceExpandHeight = false;

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.content = contentRt;
        scroll.viewport = frame.rectTransform;
        return contentRt;
    }

    void RebuildTurretList()
    {
        ClearChildren(turretListContent);
        int count = turretCatalog != null ? turretCatalog.Length : 0;
        turretRowImages = new Image[count];

        for (int i = 0; i < count; i++)
        {
            TurretLabEntry entry = turretCatalog[i];
            if (entry == null)
                continue;

            int index = i;
            Image row = CreateRow(turretListContent, "Turret_" + i, entry.Label, mutedColor);
            turretRowImages[i] = row;
            Button button = row.gameObject.AddComponent<Button>();
            button.targetGraphic = row;
            BindUiClick(button);
            button.onClick.AddListener(() => SelectTurret(index));
        }

        if (count == 0)
            CreateRow(turretListContent, "Empty", "등록된 포탑이 없습니다.", mutedColor);
    }

    void RefreshResearchList()
    {
        statComparison?.Show(selectedIndex, previewLevel);
        if (appearanceContent != null)
        {
            ClearChildren(appearanceContent);
            var appearanceEntry = GetSelectedEntry();
            appearanceContent.parent.gameObject.SetActive(appearanceEntry != null && !appearanceEntry.isCctv);
            CreatePaintOptions(appearanceEntry);
        }
        ClearChildren(researchListContent);

        TurretLabEntry entry = GetSelectedEntry();
        if (entry != null && entry.isCctv)
        {
            CreateCctvLoadoutList();
            return;
        }

        string group = entry != null ? TurretResearch.GroupOfPrefab(entry.prefab) : "";
        TurretResearchDef[] researches = TurretResearch.ForGroup(group);
        if (researches == null || researches.Length == 0)
        {
            CreateRow(researchListContent, "EmptyResearch", "준비 중", mutedColor);
            ShowTurretDetail();
            return;
        }

        for (int i = 0; i < researches.Length; i++)
        {
            TurretResearchDef research = researches[i];
            if (research == null)
                continue;

            bool complete = TurretResearch.IsComplete(research.id);
            bool unlocked = TurretResearch.PrerequisitesMet(research);
            bool selected = research.id == selectedResearchId;
            Color titleColor = complete ? new Color(0.55f, 0.92f, 0.68f, 1f) : unlocked ? textColor : mutedColor;
            Image row = CreateRow(researchListContent, "Research_" + i, research.displayName, titleColor);
            if (complete)
                row.color = new Color(0.16f, 0.28f, 0.22f, 1f);
            else if (!unlocked)
                row.color = new Color(0.16f, 0.17f, 0.19f, 1f);
            if (selected)
                row.color = selectedColor;

            string researchId = research.id;
            Button button = row.gameObject.AddComponent<Button>();
            button.targetGraphic = row;
            BindUiClick(button);
            button.onClick.AddListener(() =>
            {
                selectedResearchId = researchId;
                selectedPrimaryId = null;
                RefreshResearchList();
            });
        }

        if (string.IsNullOrEmpty(selectedResearchId))
            ShowTurretDetail();
        else
            ShowResearchDetail(selectedResearchId);
    }

    void TryBuyResearch(string id)
    {
        TurretResearchDef research = TurretResearch.Find(id);
        if (research == null)
            return;

        AskConfirm(
            "연구 진행",
            research.displayName + " 연구를 진행할까요?\n" + research.cost + " RP가 소모됩니다.",
            () =>
            {
                if (!TurretResearch.TryUnlock(id))
                    return;

                RefreshResearchPoints();
                RefreshResearchList();
                SpawnPreview(GetSelectedEntry());
            }
        );
    }

    void CreateCctvLoadoutList()
    {
        string equipped = GameProgress.EquippedPrimarySkill;
        CctvPrimaryDef[] loadout = CctvPrimaryLoadout.All;
        for (int i = 0; i < loadout.Length; i++)
        {
            CctvPrimaryDef def = loadout[i];
            bool equippedNow = def.id == equipped;
            bool selected = def.id == selectedPrimaryId;
            Color titleColor = equippedNow ? new Color(0.55f, 0.92f, 0.68f, 1f) : textColor;
            Image row = CreateRow(researchListContent, "Primary_" + def.id, def.displayName, titleColor);
            AddPrimaryArtwork(row, def);
            if (equippedNow)
                row.color = new Color(0.16f, 0.28f, 0.22f, 1f);
            if (selected)
                row.color = selectedColor;

            string skillId = def.id;
            Button button = row.gameObject.AddComponent<Button>();
            button.targetGraphic = row;
            BindUiClick(button);
            button.onClick.AddListener(() =>
            {
                selectedPrimaryId = skillId;
                selectedResearchId = null;
                RefreshResearchList();
            });
        }

        if (string.IsNullOrEmpty(selectedPrimaryId))
            ShowCctvDetail();
        else
            ShowPrimaryDetail(selectedPrimaryId);
    }

    void ShowTurretDetail()
    {
        statComparison?.Show(selectedIndex, previewLevel);
        TurretLabEntry entry = GetSelectedEntry();
        if (entry == null)
        {
            SetDetail("상세 정보", "포탑이나 연구를 선택하면 설명이 표시됩니다.", null, null, false);
            return;
        }

        if (entry.isCctv)
        {
            ShowCctvDetail();
            return;
        }

        TurretController source = entry.prefab != null ? entry.prefab.GetComponentInChildren<TurretController>(true) : null;
        if (source == null)
        {
            SetDetail(entry.Label, "이 포탑의 상세 정보가 아직 없습니다.", null, null, false);
            return;
        }

        previewLevel = Mathf.Clamp(previewLevel, 1, source.MaxLevel);
        TurretUpgradeTier tier = source.upgradeTiers != null && source.upgradeTiers.Length >= previewLevel
            ? source.upgradeTiers[previewLevel - 1] : null;
        WeaponSystem weapon = source.weapon != null ? source.weapon : source.GetComponent<WeaponSystem>();
        float baseRange = tier != null && tier.range > 0f ? tier.range : source.range;
        float baseAp = tier != null ? tier.armorPierce : source.armorPierce;
        float rangeBonus = TurretResearch.RangeBonus(source);
        float apBonus = TurretResearch.ArmorPierceBonus(source);
        TurretResearchDef[] researches = TurretResearch.ForGroup(TurretResearch.GroupOfPrefab(entry.prefab));
        int complete = 0;
        if (researches != null)
            foreach (TurretResearchDef research in researches)
                if (research != null && TurretResearch.IsComplete(research.id))
                    complete++;

        string body = entry.Label + "  ·  " + previewLevel + "단계 미리보기"
            + "\n연구 완료  " + complete + " / " + (researches != null ? researches.Length : 0)
            + "\n사거리  " + (baseRange + rangeBonus).ToString("0.#") + "  (연구 +" + rangeBonus.ToString("0.#") + ")"
            + "    AP  " + (baseAp + apBonus).ToString("0.#") + "  (+" + apBonus.ToString("0.#") + ")";
        if (weapon != null)
        {
            float speed = weapon.ammo != null ? weapon.ammo.bulletSpeed : 0f;
            float reload = tier != null ? tier.reloadTime : weapon.reloadTime;
            body += "\n탄속  " + (speed + TurretResearch.BulletSpeedBonus(source)).ToString("0.#")
                + "    탄퍼짐  " + Mathf.Max(0f, weapon.spreadAngle + TurretResearch.SpreadBonus(source)).ToString("0.#") + "°"
                + "\n재장전  " + Mathf.Max(0.01f, reload + TurretResearch.ReloadBonus(source)).ToString("0.##") + "초";
        }
        body += "\n영구 연구는 오른쪽 목록에서 고르세요. 단계 업그레이드는 전투 중 포탑을 선택해 진행합니다.";
        SetDetail(entry.Label, body, null, null, true);
        RebuildPreviewLevels(source.MaxLevel, entry);
    }

    void ShowResearchDetail(string id)
    {
        TurretResearchDef research = TurretResearch.Find(id);
        if (research == null)
        {
            ShowTurretDetail();
            return;
        }

        bool complete = TurretResearch.IsComplete(research.id);
        bool unlocked = TurretResearch.PrerequisitesMet(research);
        bool affordable = GameProgress.ResearchPoints >= research.cost;
        string body = research.description;
        if (complete)
            body += "\n상태  완료";
        else if (!unlocked)
            body += "\n상태  선행 필요  ·  " + TurretResearch.PrerequisiteLabel(research);
        else
            body += "\n비용  " + research.cost + " RP"
                + (affordable ? "" : "  ·  포인트 부족")
                + "\n보유  " + GameProgress.ResearchPoints + " RP";

        string action = null;
        Action onAction = null;
        if (!complete && unlocked && affordable)
        {
            action = "연구 진행";
            onAction = () => TryBuyResearch(research.id);
        }

        SetDetail(research.displayName, body, action, onAction, false);
    }

    void ShowCctvDetail()
    {
        CctvPrimaryDef current = CctvPrimaryLoadout.Find(GameProgress.EquippedPrimarySkill);
        SetDetail(
            "CCTV",
            "주력 스킬은 한 종류만 장착됩니다.\n현재 장착  " + current.displayName + "\n" + current.description + "\n오른쪽 목록에서 스킬을 고르면 자세한 설명이 나옵니다.",
            null,
            null,
            false
        );
        SetPrimaryArtwork(current.Icon);
    }

    void ShowPrimaryDetail(string id)
    {
        CctvPrimaryDef def = CctvPrimaryLoadout.Find(id);
        bool equipped = def.id == GameProgress.EquippedPrimarySkill;
        string body = def.description + (equipped ? "\n상태  장착 중" : "\n상태  미장착  ·  전투에서 이 스킬만 주력으로 사용합니다.");
        string action = equipped ? null : "장착";
        Action onAction = equipped
            ? null
            : (Action)(() =>
            {
                AskConfirm(
                    "주력 스킬 장착",
                    def.displayName + "을(를) 주력 스킬로 장착할까요?\n전투에서 한 종류만 사용할 수 있습니다.",
                    () =>
                    {
                        GameProgress.SetEquippedPrimarySkill(def.id);
                        selectedPrimaryId = def.id;
                        RefreshResearchList();
                    }
                );
            });
        SetDetail(def.displayName, body, action, onAction, false);
        SetPrimaryArtwork(def.Icon);
    }

    void AddPrimaryArtwork(Image row, CctvPrimaryDef def)
    {
        Sprite sprite = def.Icon;
        if (sprite == null)
            return;

        LayoutElement layout = row.GetComponent<LayoutElement>();
        layout.minHeight = 84f;
        layout.preferredHeight = 84f;
        Image artwork = UiFont.AddImage(row.transform, "SkillArtwork", Color.white);
        artwork.sprite = sprite;
        artwork.preserveAspect = true;
        artwork.raycastTarget = false;
        UiSkin.Place(artwork.rectTransform, new Vector2(0f, 0.5f), new Vector2(46f, 0f), new Vector2(72f, 72f));
        Text label = row.transform.Find("Label").GetComponent<Text>();
        label.raycastTarget = false;
        label.rectTransform.offsetMin = new Vector2(96f, 0f);
    }

    void SetPrimaryArtwork(Sprite sprite)
    {
        if (detailArtwork == null)
            return;

        detailArtwork.sprite = sprite;
        detailArtwork.gameObject.SetActive(sprite != null);
        float inset = sprite != null ? 160f : 16f;
        if (detailTitle != null)
            detailTitle.rectTransform.offsetMin = new Vector2(inset, detailTitle.rectTransform.offsetMin.y);
        if (detailBody != null)
            detailBody.rectTransform.offsetMin = new Vector2(inset, detailBody.rectTransform.offsetMin.y);
    }

    void SetDetail(string title, string body, string actionLabel, Action onAction, bool showLevels)
    {
        SetPrimaryArtwork(null);
        if (detailTitle != null)
            detailTitle.text = title;
        if (detailBody != null)
            detailBody.text = body;
        if (previewLevelRow != null)
            previewLevelRow.gameObject.SetActive(showLevels);
        if (detailAction == null)
            return;

        detailAction.onClick.RemoveAllListeners();
        bool showAction = !string.IsNullOrEmpty(actionLabel) && onAction != null;
        detailAction.gameObject.SetActive(showAction);
        if (!showAction)
            return;

        if (detailActionLabel != null)
            detailActionLabel.text = actionLabel;
        BindUiClick(detailAction);
        detailAction.onClick.AddListener(() => onAction());
    }

    void RebuildPreviewLevels(int maxLevel, TurretLabEntry entry)
    {
        if (previewLevelRow == null)
            return;

        ClearChildren(previewLevelRow);
        for (int level = 1; level <= maxLevel; level++)
        {
            int captured = level;
            Image image = UiFont.AddImage(previewLevelRow, "PreviewLevel_" + level, level == previewLevel ? selectedColor : rowColor);
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            BindUiClick(button);
            Text label = UiFont.AddText(image.rectTransform, "Label", level + "단계", 16, TextAnchor.MiddleCenter, textColor, UiType.BodyMedium);
            label.raycastTarget = false;
            UiFont.Stretch(label.rectTransform);
            button.onClick.AddListener(() =>
            {
                previewLevel = captured;
                SpawnPreview(entry);
                ShowTurretDetail();
            });
        }
    }

    Image CreateRow(Transform parent, string name, string label, Color labelColor)
    {
        Image row = UiFont.AddImage(parent, name, rowColor);
        LayoutElement layout = row.gameObject.AddComponent<LayoutElement>();
        layout.minHeight = 48f;
        layout.preferredHeight = 48f;

        Text text = UiFont.AddText(row.rectTransform, "Label", label, 20, TextAnchor.MiddleLeft, labelColor, UiType.BodyMedium);
        UiFont.Stretch(text.rectTransform);
        text.rectTransform.offsetMin = new Vector2(14f, 0f);
        text.rectTransform.offsetMax = new Vector2(-14f, 0f);
        return row;
    }

    void SelectTurret(int index)
    {
        if (turretCatalog == null || index < 0 || index >= turretCatalog.Length)
            return;

        selectedIndex = index;
        previewLevel = 1;
        selectedResearchId = null;
        selectedPrimaryId = null;
        for (int i = 0; i < turretRowImages.Length; i++)
        {
            if (turretRowImages[i] != null)
                turretRowImages[i].color = i == selectedIndex ? selectedColor : rowColor;
        }

        RefreshResearchList();
        SpawnPreview(turretCatalog[index]);
    }

    void SpawnPreview(TurretLabEntry entry)
    {
        ClearPreview();
        GameObject source = entry != null && entry.previewPrefab != null ? entry.previewPrefab : (entry != null ? entry.prefab : null);
        if (source == null || previewPlatform == null)
            return;

        Vector3 mount = previewPlatform.GetMountPosition();
        Quaternion facing = previewPlatform.transform.rotation;
        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 toCam = cam.transform.position - mount;
            toCam.y = 0f;
            if (toCam.sqrMagnitude > 0.01f)
                facing = Quaternion.LookRotation(toCam.normalized);
        }

        previewInstance = Instantiate(source, mount, facing);
        previewInstance.name = entry.Label;
        previewInstance.transform.SetParent(previewPlatform.transform, true);
        FreezePreview(previewInstance);

        TurretController turret = previewInstance.GetComponent<TurretController>();
        if (turret != null)
        {
            turret.enabled = false;
            turret.sourcePrefab = entry.prefab != null ? entry.prefab : source;
            turret.displayName = entry.Label;
            turret.upgradeLevel = Mathf.Clamp(previewLevel, 1, turret.MaxLevel);
            turret.BeginConstruction(0f, true, null);
            previewPlatform.Mount(turret);
        }
        else
        {
            previewPlatform.Mount(null);
            previewPlatform.occupied = true;
        }

        WeaponSystem weapon = previewInstance.GetComponent<WeaponSystem>();
        if (weapon != null)
            weapon.enabled = false;
    }

    static void FreezePreview(GameObject root)
    {
        if (root == null)
            return;

        Behaviour[] behaviours = root.GetComponentsInChildren<Behaviour>(true);
        for (int i = 0; i < behaviours.Length; i++)
        {
            Behaviour behaviour = behaviours[i];
            if (behaviour == null)
                continue;
            if (behaviour is Animator)
                continue;
            if (behaviour is Camera || behaviour is AudioListener || behaviour is AudioSource)
            {
                behaviour.enabled = false;
                continue;
            }
            if (behaviour is CCTVDroneController || behaviour is CCTVController || behaviour is CCTVSkillSystem)
                behaviour.enabled = false;
        }

        Camera[] cameras = root.GetComponentsInChildren<Camera>(true);
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] != null)
                cameras[i].enabled = false;
        }

        Rigidbody[] bodies = root.GetComponentsInChildren<Rigidbody>(true);
        for (int i = 0; i < bodies.Length; i++)
        {
            Rigidbody body = bodies[i];
            if (body == null)
                continue;
            body.isKinematic = true;
            body.useGravity = false;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
    }

    void ClearPreview()
    {
        if (previewInstance != null)
            Destroy(previewInstance);
        previewInstance = null;

        if (previewPlatform != null)
        {
            previewPlatform.mountedTurret = null;
            previewPlatform.occupied = false;
        }
    }

    TurretLabEntry GetSelectedEntry()
    {
        if (turretCatalog == null || selectedIndex < 0 || selectedIndex >= turretCatalog.Length)
            return null;
        return turretCatalog[selectedIndex];
    }

    void RefreshResearchPoints()
    {
        if (pointsText != null)
            pointsText.text = ResearchPointsLabel();
    }

    static string ResearchPointsLabel()
    {
        return "연구 포인트  " + GameProgress.ResearchPoints;
    }

    static void ClearChildren(Transform parent)
    {
        if (parent == null)
            return;

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject child = parent.GetChild(i).gameObject;
            child.SetActive(false);
            Destroy(child);
        }
    }

    void StripLegacyHud()
    {
        Canvas canvas = FindCanvas();
        if (canvas == null)
            return;

        Transform panel = canvas.transform.Find("Panel");
        if (panel != null)
            Destroy(panel.gameObject);

        Transform dataset = canvas.transform.Find("Button_Dataset");
        if (dataset != null)
            Destroy(dataset.gameObject);

        Transform main = canvas.transform.Find("Button_MainMenu");
        if (main != null)
            Destroy(main.gameObject);
    }

    void CreateNavButtons()
    {
        Canvas canvas = FindCanvas();
        if (canvas == null)
            return;

        Button dataset = UiSkin.Button(
            canvas.transform,
            "DatasetFactory",
            "데이터셋 팩토리",
            UiSkin.Blue,
            Vector2.zero,
            new Vector2(180f, 72f),
            new Vector2(300f, 76f));
        dataset.onClick.AddListener(() => LoadScene("DatasetGeneratorScene"));

        Button main = UiSkin.Button(
            canvas.transform,
            "MainMenu",
            "메인 메뉴",
            UiSkin.Blue,
            Vector2.zero,
            new Vector2(520f, 72f),
            new Vector2(300f, 76f));
        main.onClick.AddListener(() => LoadScene("MainMenu"));
    }

    void AskConfirm(string title, string body, Action onYes)
    {
        pendingConfirm = onYes;
        EnsureConfirmDialog();
        Transform card = confirmRoot.transform.Find("Card");
        Text titleText = card != null ? card.Find("Title") != null ? card.Find("Title").GetComponent<Text>() : null : null;
        Text bodyText = card != null ? card.Find("Body") != null ? card.Find("Body").GetComponent<Text>() : null : null;
        if (titleText != null)
            titleText.text = title;
        if (bodyText != null)
            bodyText.text = body;
        confirmRoot.SetActive(true);
        confirmRoot.transform.SetAsLastSibling();
    }

    void CloseConfirm(bool accepted)
    {
        Action action = pendingConfirm;
        pendingConfirm = null;
        if (confirmRoot != null)
            confirmRoot.SetActive(false);
        if (accepted && action != null)
            action();
    }

    void EnsureConfirmDialog()
    {
        if (confirmRoot != null)
            return;

        Canvas canvas = FindCanvas();
        if (canvas == null)
            return;

        Image dim = UiFont.AddImage(canvas.transform, "ConfirmDialog", new Color(0.02f, 0.03f, 0.04f, 0.72f));
        confirmRoot = dim.gameObject;
        UiFont.Stretch(dim.rectTransform);
        dim.raycastTarget = true;

        Image card = UiFont.AddImage(dim.rectTransform, "Card", UiSkin.Panel);
        UiSkin.Surface(card);
        UiSkin.Place(card.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(640f, 280f));

        Text title = UiFont.AddText(card.rectTransform, "Title", "확인", 28, TextAnchor.MiddleCenter, textColor, UiType.Title);
        title.raycastTarget = false;
        RectTransform titleRt = title.rectTransform;
        titleRt.anchorMin = new Vector2(0f, 1f);
        titleRt.anchorMax = new Vector2(1f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0f, -28f);
        titleRt.sizeDelta = new Vector2(-48f, 40f);

        Text body = UiFont.AddText(card.rectTransform, "Body", "", 20, TextAnchor.UpperCenter, mutedColor, UiType.Body);
        body.raycastTarget = false;
        RectTransform bodyRt = body.rectTransform;
        bodyRt.anchorMin = new Vector2(0f, 0f);
        bodyRt.anchorMax = new Vector2(1f, 1f);
        bodyRt.offsetMin = new Vector2(28f, 88f);
        bodyRt.offsetMax = new Vector2(-28f, -76f);

        Button yes = UiSkin.Button(card.transform, "Yes", "확인", UiSkin.Blue, new Vector2(0f, 0f), new Vector2(150f, 44f), new Vector2(200f, 56f));
        Button no = UiSkin.Button(card.transform, "No", "취소", UiSkin.Navy, new Vector2(1f, 0f), new Vector2(-150f, 44f), new Vector2(200f, 56f));
        yes.onClick.AddListener(() => CloseConfirm(true));
        no.onClick.AddListener(() => CloseConfirm(false));
        confirmRoot.SetActive(false);
    }

    static void BindUiClick(Button button)
    {
        if (button != null)
            button.onClick.AddListener(GameSfx.PlayUiClick);
    }

    Canvas FindCanvas()
    {
        Canvas canvas = UiFont.FindSceneCanvas(this);
        if (canvas == null)
            canvas = SceneLoader.FindSceneCanvas();
        return canvas;
    }

    void BuildAppearanceSection(Transform parent)
    {
        Image frame=UiFont.AddImage(parent,"AppearanceSection",UiSkin.Navy);
        var sizing=frame.gameObject.AddComponent<LayoutElement>();sizing.minHeight=sizing.preferredHeight=60;
        var layout=frame.gameObject.AddComponent<VerticalLayoutGroup>();layout.padding=new RectOffset(8,8,8,8);layout.spacing=8;
        layout.childControlHeight=layout.childControlWidth=true;layout.childForceExpandHeight=false;
        var header=UiSkin.Button(frame.transform,"AppearanceToggle","외형 · 색상 변경  ›",UiSkin.Panel,Vector2.zero,Vector2.zero,new Vector2(440,44));
        var headerSize=header.gameObject.AddComponent<LayoutElement>();headerSize.minHeight=headerSize.preferredHeight=44;
        header.GetComponentInChildren<Text>().fontSize=20;
        var body=new GameObject("AppearanceContent",typeof(RectTransform),typeof(VerticalLayoutGroup),typeof(LayoutElement));
        body.transform.SetParent(frame.transform,false);appearanceContent=body.transform;
        var bodySize=body.GetComponent<LayoutElement>();bodySize.minHeight=bodySize.preferredHeight=354;
        var bodyLayout=body.GetComponent<VerticalLayoutGroup>();bodyLayout.childControlWidth=bodyLayout.childControlHeight=true;bodyLayout.childForceExpandHeight=false;
        body.SetActive(false);
        header.onClick.AddListener(()=>
        {
            bool open=!body.activeSelf;body.SetActive(open);sizing.minHeight=sizing.preferredHeight=open?422:60;
            var scrollFrame = researchListContent != null ? researchListContent.parent.GetComponent<LayoutElement>() : null;
            if (scrollFrame != null) scrollFrame.minHeight = scrollFrame.preferredHeight = open ? 160 : 380;
            var listFrame = turretListContent != null ? turretListContent.parent.GetComponent<LayoutElement>() : null;
            if (listFrame != null) listFrame.minHeight = listFrame.preferredHeight = open ? 120 : 240;
            header.GetComponentInChildren<Text>().text=open?"외형 · 색상 변경  ▾":"외형 · 색상 변경  ›";
        });
    }


    void CreatePaintOptions(TurretLabEntry entry)
    {
        if (entry == null || entry.prefab == null) return;
        var source = entry.prefab.GetComponentInChildren<TurretController>(true);
        if (source == null) return;
        if(appearanceContent==null)return;
        Image row = CreateRow(appearanceContent, "TurretPaint", "포탑 색상", textColor);
        row.GetComponent<LayoutElement>().minHeight = row.GetComponent<LayoutElement>().preferredHeight = 354;
        var heading = row.GetComponentInChildren<Text>();
        UiSkin.Place(heading.rectTransform, new Vector2(.5f,1), new Vector2(0,-18), new Vector2(280,30));
        heading.text="외장 색상 · 64색";
        int selected=TurretPaint.Selected(source);
        for (int i=1; i<TurretPaint.Colors.Length; i++)
        {
            int choice=i;
            int column=(i-1)%8, line=(i-1)/8;
            var button=UiSkin.Button(row.transform,"Paint_"+i,selected==i?"✓":"",TurretPaint.Colors[i],
                new Vector2(.5f,1),new Vector2((column-3.5f)*32,-58-line*32),new Vector2(28,28));
            button.GetComponentInChildren<Text>().fontSize=18;
            button.GetComponentInChildren<Text>().color=TurretPaint.Colors[i].grayscale>.65f?UiSkin.Navy:Color.white;
            button.onClick.AddListener(()=>{TurretPaint.Select(source,choice);SpawnPreview(entry);RefreshResearchList();});
        }
        var reset=UiSkin.Button(row.transform,"ResetPaint","기본 색상",UiSkin.Blue,new Vector2(.5f,0),new Vector2(0,32),new Vector2(130,34));
        reset.GetComponentInChildren<Text>().fontSize=16;
        reset.onClick.AddListener(()=>{TurretPaint.Select(source,0);SpawnPreview(entry);RefreshResearchList();});
    }

}
