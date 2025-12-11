using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Displays skill description when player acquires a new skill
/// 플레이어가 새 스킬을 획득할 때 스킬 설명 패널을 표시
/// </summary>
public class SkillDescriptionPanel : MonoBehaviour
{
    [System.Serializable]
    public class SkillInfo
    {
        [Header("스킬 정보")]
        public string skillName = "스킬 이름";

        [TextArea(3, 10)]
        public string skillDescription = "스킬 설명을 여기에 작성하세요.";

        public Sprite skillIcon;
    }

    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI skillDescriptionText;
    [SerializeField] private Image skillIconImage;
    [SerializeField] private TextMeshProUGUI continueText; // "Click to continue" text

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.2f;

    [Header("Continue Text Settings")]
    [SerializeField] private float blinkSpeed = 1f;

    [Header("스킬 설명 목록 (Inspector에서 작성)")]
    [SerializeField] private SkillInfo[] skillInfos = new SkillInfo[0];

    private static SkillDescriptionPanel instance;
    private CanvasGroup canvasGroup;
    private bool isShowing = false;
    private Coroutine blinkCoroutine;

    private void Awake()
    {
        // Singleton pattern with DontDestroyOnLoad
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
            Debug.Log("[SkillDescriptionPanel] Panel will persist across scenes");
        }
        else
        {
            Debug.Log("[SkillDescriptionPanel] Duplicate panel found, destroying this one");
            Destroy(gameObject);
            return;
        }

        // Get or add CanvasGroup
        if (panel != null)
        {
            canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = panel.AddComponent<CanvasGroup>();
            }
        }

        // Hide panel initially
        HideImmediate();
    }

    private void Update()
    {
        // Check for click to close panel (using new Input System)
        if (isShowing)
        {
            // Check for mouse click
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                HidePanel();
                return;
            }

            // Check for keyboard input
            if (Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame ||
                    Keyboard.current.enterKey.wasPressedThisFrame ||
                    Keyboard.current.numpadEnterKey.wasPressedThisFrame)
                {
                    HidePanel();
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Show skill description panel by skill name (finds from inspector list)
    /// Inspector에 등록된 스킬 이름으로 패널 표시
    /// </summary>
    public static void ShowSkillByName(string skillName)
    {
        if (instance != null)
        {
            instance.ShowByName(skillName);
        }
        else
        {
            Debug.LogWarning("[SkillDescriptionPanel] Instance not found!");
        }
    }

    /// <summary>
    /// Show skill description panel by index (from inspector list)
    /// Inspector 목록의 인덱스로 패널 표시
    /// </summary>
    public static void ShowSkillByIndex(int index)
    {
        if (instance != null)
        {
            instance.ShowByIndex(index);
        }
        else
        {
            Debug.LogWarning("[SkillDescriptionPanel] Instance not found!");
        }
    }

    /// <summary>
    /// Show skill description panel (static method - for direct call)
    /// 직접 정보를 전달하여 패널 표시 (코드에서 직접 호출용)
    /// </summary>
    public static void ShowSkillDescription(string skillName, string description, Sprite icon = null)
    {
        if (instance != null)
        {
            instance.Show(skillName, description, icon);
        }
        else
        {
            Debug.LogWarning("[SkillDescriptionPanel] Instance not found!");
        }
    }

    /// <summary>
    /// Show skill by name from inspector list
    /// </summary>
    private void ShowByName(string skillName)
    {
        foreach (SkillInfo info in skillInfos)
        {
            if (info.skillName == skillName)
            {
                Show(info.skillName, info.skillDescription, info.skillIcon);
                return;
            }
        }

        Debug.LogWarning($"[SkillDescriptionPanel] Skill '{skillName}' not found in inspector list!");
    }

    /// <summary>
    /// Show skill by index from inspector list
    /// </summary>
    private void ShowByIndex(int index)
    {
        if (index >= 0 && index < skillInfos.Length)
        {
            SkillInfo info = skillInfos[index];
            Show(info.skillName, info.skillDescription, info.skillIcon);
        }
        else
        {
            Debug.LogWarning($"[SkillDescriptionPanel] Invalid skill index: {index}");
        }
    }

    /// <summary>
    /// Show the panel with skill information
    /// </summary>
    private void Show(string skillName, string description, Sprite icon)
    {
        if (isShowing) return;

        // Set skill information
        if (skillNameText != null)
        {
            skillNameText.text = skillName;
        }

        if (skillDescriptionText != null)
        {
            skillDescriptionText.text = description;
        }

        if (skillIconImage != null && icon != null)
        {
            skillIconImage.sprite = icon;
            skillIconImage.gameObject.SetActive(true);
        }
        else if (skillIconImage != null)
        {
            skillIconImage.gameObject.SetActive(false);
        }

        // Show panel
        StartCoroutine(ShowPanelCoroutine());

        // Pause game
        Time.timeScale = 0f;

        Debug.Log($"[SkillDescriptionPanel] Showing skill: {skillName}");
    }

    private IEnumerator ShowPanelCoroutine()
    {
        isShowing = true;

        if (panel != null)
        {
            panel.SetActive(true);
        }

        // Fade in
        if (canvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        // Start blinking "continue" text
        if (continueText != null && blinkCoroutine == null)
        {
            blinkCoroutine = StartCoroutine(BlinkContinueText());
        }
    }

    /// <summary>
    /// Hide the panel
    /// </summary>
    private void HidePanel()
    {
        if (!isShowing) return;

        StartCoroutine(HidePanelCoroutine());
    }

    private IEnumerator HidePanelCoroutine()
    {
        // Stop blinking
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        // Fade out
        if (canvasGroup != null)
        {
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutDuration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
        }

        if (panel != null)
        {
            panel.SetActive(false);
        }

        isShowing = false;

        // Resume game
        Time.timeScale = 1f;

        Debug.Log("[SkillDescriptionPanel] Panel hidden, game resumed");
    }

    private void HideImmediate()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        isShowing = false;
    }

    private IEnumerator BlinkContinueText()
    {
        if (continueText == null) yield break;

        CanvasGroup textCanvasGroup = continueText.GetComponent<CanvasGroup>();
        if (textCanvasGroup == null)
        {
            textCanvasGroup = continueText.gameObject.AddComponent<CanvasGroup>();
        }

        while (true)
        {
            // Fade out
            float elapsed = 0f;
            float duration = 1f / blinkSpeed;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                textCanvasGroup.alpha = Mathf.Lerp(1f, 0.3f, elapsed / duration);
                yield return null;
            }

            // Fade in
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                textCanvasGroup.alpha = Mathf.Lerp(0.3f, 1f, elapsed / duration);
                yield return null;
            }
        }
    }

    /// <summary>
    /// Check if panel is currently showing
    /// </summary>
    public static bool IsShowing()
    {
        return instance != null && instance.isShowing;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
