using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Individual skill slot UI - displays icon, cooldown, and key binding
/// 개별 스킬 슬롯 UI - 아이콘, 쿨타임, 키 바인딩 표시
/// </summary>
public class SkillSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image skillIcon;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private TextMeshProUGUI keyBindingText;

    [Header("Visual Settings")]
    [SerializeField] private Color readyColor = Color.white;
    [SerializeField] private Color cooldownColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);

    private string skillName;
    private Sprite iconSprite;
    private string keyBinding;
    private float currentCooldown = 0f;
    private float maxCooldown = 1f;
    private bool isActive = false;

    private void Awake()
    {
        // Initialize UI
        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillAmount = 0f;
        }

        if (cooldownText != null)
        {
            cooldownText.text = "";
        }

        // Hide by default
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isActive) return;

        // Update cooldown display
        if (currentCooldown > 0f)
        {
            currentCooldown -= Time.deltaTime;

            if (currentCooldown < 0f)
            {
                currentCooldown = 0f;
            }

            UpdateCooldownDisplay();
        }
    }

    /// <summary>
    /// Initialize skill slot with icon and key binding
    /// </summary>
    public void Initialize(string name, Sprite icon, string key, float cooldown)
    {
        skillName = name;
        iconSprite = icon;
        keyBinding = key;
        maxCooldown = cooldown;

        // Set icon
        if (skillIcon != null && icon != null)
        {
            skillIcon.sprite = icon;
            skillIcon.color = readyColor;
        }

        // Set key binding text
        if (keyBindingText != null)
        {
            keyBindingText.text = key;
        }

        // Reset cooldown
        currentCooldown = 0f;
        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillAmount = 0f;
        }

        if (cooldownText != null)
        {
            cooldownText.text = "";
        }

        // Show slot
        isActive = true;
        gameObject.SetActive(true);

        Debug.Log($"SkillSlotUI initialized: {name} ({key})");
    }

    /// <summary>
    /// Start cooldown for this skill
    /// </summary>
    public void StartCooldown(float cooldownDuration)
    {
        currentCooldown = cooldownDuration;
        maxCooldown = cooldownDuration;

        // Darken icon
        if (skillIcon != null)
        {
            skillIcon.color = cooldownColor;
        }

        UpdateCooldownDisplay();
    }

    /// <summary>
    /// Update cooldown visual display
    /// </summary>
    private void UpdateCooldownDisplay()
    {
        // Update fill amount (1 = full cooldown, 0 = ready)
        float fillAmount = currentCooldown / maxCooldown;

        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillAmount = fillAmount;
        }

        // Update cooldown text
        if (cooldownText != null)
        {
            if (currentCooldown > 0f)
            {
                cooldownText.text = currentCooldown.ToString("F1");
            }
            else
            {
                cooldownText.text = "";
                // Restore icon color when ready
                if (skillIcon != null)
                {
                    skillIcon.color = readyColor;
                }
            }
        }
    }

    /// <summary>
    /// Check if skill is ready (not on cooldown)
    /// </summary>
    public bool IsReady()
    {
        return currentCooldown <= 0f;
    }

    /// <summary>
    /// Get remaining cooldown time
    /// </summary>
    public float GetRemainingCooldown()
    {
        return currentCooldown;
    }

    /// <summary>
    /// Hide this skill slot
    /// </summary>
    public void Hide()
    {
        isActive = false;
        gameObject.SetActive(false);
    }
}
