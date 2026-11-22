using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class CCTVSkillSystem : MonoBehaviour
{
    public const int SlotCount = 4;

    [Tooltip("이 스테이지에서 쓸 액티브 스킬. 최대 4칸, 스테이지 중에는 바뀌지 않는다.")]
    public CCTVActiveSkill[] equippedSkills = new CCTVActiveSkill[SlotCount];

    [Tooltip("주력 스킬. 한 종류만 착용한다.")]
    public CCTVActiveSkill equippedPrimary;

    public float crosshairSize = 14f;
    public float crosshairThickness = 2f;
    public Color crosshairColor = new Color(0.95f, 0.95f, 0.9f, 0.9f);

    public float slotSize = 72f;
    public float slotGap = 8f;
    public float slotBottomMargin = 28f;

    static readonly KeyCode[] SlotKeys =
    {
        KeyCode.Q,
        KeyCode.E,
        KeyCode.R,
        KeyCode.F
    };

    public static readonly string[] SlotKeyLabels = { "Q", "E", "R", "F" };

    WaveDirector waveDirector;
    CCTVController cctv;
    TurretPlatformPlacer buildPlacer;
    Camera viewCamera;
    readonly List<RaycastResult> uiRaycasts = new List<RaycastResult>();
    static int primaryMouseConsumedFrame = -1;

    public static void ConsumePrimaryMouse()
    {
        primaryMouseConsumedFrame = Time.frameCount;
    }

    public static bool PrimaryMouseConsumed
    {
        get { return primaryMouseConsumedFrame == Time.frameCount; }
    }

    void Start()
    {
        waveDirector = FindFirstObjectByType<WaveDirector>();
        cctv = GetComponent<CCTVController>();
        if (cctv == null)
            cctv = FindFirstObjectByType<CCTVController>();
        buildPlacer = FindFirstObjectByType<TurretPlatformPlacer>();
        ApplyPrimaryLoadout();
        EnsureUltimateSlot();
    }

    public void EnsureUltimateSlot()
    {
        if (equippedSkills == null) equippedSkills = new CCTVActiveSkill[SlotCount];
        if (equippedSkills.Length < SlotCount) System.Array.Resize(ref equippedSkills, SlotCount);
        int last = SlotCount - 1;
        if (equippedSkills[last] != null) return;
        CCTVSkill_Overdrive ultimate = GetComponent<CCTVSkill_Overdrive>();
        if (ultimate == null) ultimate = gameObject.AddComponent<CCTVSkill_Overdrive>();
        ultimate.displayName = "오버드라이브";
        ultimate.cooldown = 60f;
        equippedSkills[last] = ultimate;
    }

    void ApplyPrimaryLoadout()
    {
        string id = GameProgress.EquippedPrimarySkill;
        CCTVActiveSkill[] skills = GetComponents<CCTVActiveSkill>();
        CCTVActiveSkill chosen = null;
        for (int i = 0; i < skills.Length; i++)
        {
            CCTVActiveSkill skill = skills[i];
            if (skill == null || string.IsNullOrEmpty(skill.LoadoutId))
                continue;

            bool match = skill.LoadoutId == id;
            skill.enabled = match;
            if (match)
                chosen = skill;
        }

        if (chosen == null)
        {
            for (int i = 0; i < skills.Length; i++)
            {
                if (skills[i] != null && skills[i].LoadoutId == CctvPrimaryLoadout.DefaultId)
                {
                    skills[i].enabled = true;
                    chosen = skills[i];
                    break;
                }
            }
        }

        if (chosen != null)
            equippedPrimary = chosen;
    }

    void Update()
    {
        if (BattlePauseMenu.IsPaused)
            return;
        if (!IsHudActive())
            return;

        if (buildPlacer != null && buildPlacer.BlocksPlayerLook)
            return;

        bool specialPrimed = HasPrimedSkill();

        for (int i = 0; i < SlotCount; i++)
        {
            if (WasSlotPressed(i))
                TryUseSlot(i);
        }

        TickPrimary(specialPrimed);
    }

    public bool ShouldDrawHud
    {
        get
        {
            if (!IsHudActive())
                return false;
            if (buildPlacer == null)
                buildPlacer = FindFirstObjectByType<TurretPlatformPlacer>();
            return buildPlacer == null || !buildPlacer.BlocksPlayerLook;
        }
    }

    public bool HasPrimedSkill()
    {
        for (int i = 0; i < SlotCount; i++)
        {
            CCTVActiveSkill skill = GetEquipped(i);
            if (skill != null && skill.IsPrimed)
                return true;
        }

        return false;
    }

    public void TryUseSlot(int index)
    {
        if (!IsHudActive())
            return;
        if (buildPlacer != null && buildPlacer.BlocksPlayerLook)
            return;

        CCTVActiveSkill skill = GetEquipped(index);
        if (skill == null)
            return;

        skill.TryUse(BuildContext());
    }

    public bool IsHudActive()
    {
        if (waveDirector == null)
            waveDirector = FindFirstObjectByType<WaveDirector>();

        if (waveDirector == null)
            return false;

        return waveDirector.Phase == WavePhase.Prep || waveDirector.Phase == WavePhase.Combat;
    }

    public CCTVActiveSkill GetEquipped(int slot)
    {
        if (equippedSkills == null || slot < 0 || slot >= equippedSkills.Length)
            return null;
        return equippedSkills[slot];
    }

    public CCTVActiveSkill GetPrimary()
    {
        return equippedPrimary;
    }

    public void TryUsePrimary(int button)
    {
        if (!CanUsePrimaryMouse())
            return;

        CCTVActiveSkill skill = equippedPrimary;
        if (skill == null)
            return;

        skill.TryPrimaryFire(BuildContext(), button);
    }

    void TickPrimary(bool specialPrimed)
    {
        CCTVActiveSkill skill = equippedPrimary;
        if (skill == null || !skill.isActiveAndEnabled)
            return;

        CCTVSkillContext context = BuildContext();
        if (skill.PrimaryUseMode == CCTVPrimaryUseMode.Passive)
        {
            skill.TickPrimary(context);
            return;
        }

        if (specialPrimed || HasPrimedSkill() || PrimaryMouseConsumed || IsPointerOverHud())
            return;

        if (WasFirePressed(0))
            skill.TryPrimaryFire(context, 0);
        else if (WasFirePressed(1))
            skill.TryPrimaryFire(context, 1);
    }

    bool CanUsePrimaryMouse()
    {
        if (!IsHudActive())
            return false;
        if (buildPlacer != null && buildPlacer.BlocksPlayerLook)
            return false;
        if (HasPrimedSkill() || PrimaryMouseConsumed || IsPointerOverHud())
            return false;
        return true;
    }

    bool IsPointerOverHud()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
            return false;

        PointerEventData pointer = new PointerEventData(eventSystem);
        pointer.position = GetMouseScreenPosition();
        uiRaycasts.Clear();
        eventSystem.RaycastAll(pointer, uiRaycasts);
        return uiRaycasts.Count > 0;
    }

    static Vector2 GetMouseScreenPosition()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
            return Mouse.current.position.ReadValue();
#endif
        return Input.mousePosition;
    }

    static bool WasFirePressed(int button)
    {
#if ENABLE_INPUT_SYSTEM
        Mouse mouse = Mouse.current;
        if (mouse == null)
            return false;
        if (button == 0)
            return mouse.leftButton.wasPressedThisFrame;
        if (button == 1)
            return mouse.rightButton.wasPressedThisFrame;
        return false;
#else
        return Input.GetMouseButtonDown(button);
#endif
    }

    CCTVSkillContext BuildContext()
    {
        Camera camera = GetViewCamera();
        var context = new CCTVSkillContext();
        context.Cctv = cctv;
        context.ViewCamera = camera;
        context.Origin = transform;
        if (camera != null)
            context.AimRay = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        return context;
    }

    Camera GetViewCamera()
    {
        if (viewCamera != null && viewCamera.enabled && viewCamera.targetTexture == null)
            return viewCamera;

        Camera best = null;
        float bestDepth = float.MinValue;
        Camera[] cameras = GetComponentsInChildren<Camera>(true);
        for (int i = 0; i < cameras.Length; i++)
        {
            Camera camera = cameras[i];
            if (camera == null || !camera.enabled || !camera.gameObject.activeInHierarchy)
                continue;
            if (camera.targetTexture != null)
                continue;
            if (best != null && camera.depth < bestDepth)
                continue;
            best = camera;
            bestDepth = camera.depth;
        }

        viewCamera = best;
        return viewCamera;
    }

    static bool WasSlotPressed(int index)
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            switch (index)
            {
                case 0: return keyboard.qKey.wasPressedThisFrame;
                case 1: return keyboard.eKey.wasPressedThisFrame;
                case 2: return keyboard.rKey.wasPressedThisFrame;
                case 3: return keyboard.fKey.wasPressedThisFrame;
            }
        }
#endif
        if (index < 0 || index >= SlotKeys.Length)
            return false;
#if ENABLE_INPUT_SYSTEM
        return false;
#else
        return Input.GetKeyDown(SlotKeys[index]);
#endif
    }
}
