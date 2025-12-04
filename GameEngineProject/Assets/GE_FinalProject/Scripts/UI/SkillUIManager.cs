using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all skill UI slots - adds skills dynamically as player acquires them
/// 모든 스킬 UI 슬롯 관리 - 플레이어가 획득하는 순서대로 동적으로 추가
/// </summary>
public class SkillUIManager : MonoBehaviour
{
    [Header("Skill Slot Prefab")]
    [SerializeField] private GameObject skillSlotPrefab; // SkillSlotUI prefab

    [Header("Container")]
    [SerializeField] private RectTransform skillContainer; // Horizontal layout container

    [Header("Spacing Settings")]
    [SerializeField] private float slotSpacing = 70f; // Spacing between slots

    [Header("Skill Icons")]
    [SerializeField] private Sprite fireballIcon;
    [SerializeField] private Sprite explosionIcon;
    [SerializeField] private Sprite defenseIcon;
    [SerializeField] private Sprite dashIcon;

    // Skill slots by skill type
    private Dictionary<SkillType, SkillSlotUI> activeSkills = new Dictionary<SkillType, SkillSlotUI>();
    private List<SkillSlotUI> skillSlots = new List<SkillSlotUI>();

    // Singleton instance
    public static SkillUIManager Instance { get; private set; }

    public enum SkillType
    {
        Fireball,
        Explosion,
        Defense,
        Dash
    }

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Validate references
        if (skillSlotPrefab == null)
        {
            Debug.LogError("SkillUIManager: Skill Slot Prefab not assigned!");
        }

        if (skillContainer == null)
        {
            Debug.LogError("SkillUIManager: Skill Container not assigned!");
        }
    }

    /// <summary>
    /// Add a skill slot when player acquires a skill
    /// </summary>
    public void AddSkill(SkillType skillType, float cooldownDuration)
    {
        // Check if skill already exists
        if (activeSkills.ContainsKey(skillType))
        {
            Debug.LogWarning($"Skill {skillType} already exists in UI!");
            return;
        }

        // Get skill data
        string skillName = GetSkillName(skillType);
        Sprite icon = GetSkillIcon(skillType);
        string keyBinding = GetKeyBinding(skillType);

        if (icon == null)
        {
            Debug.LogWarning($"No icon assigned for skill: {skillType}");
            return;
        }

        // Create skill slot
        GameObject slotObj = Instantiate(skillSlotPrefab, skillContainer);
        SkillSlotUI slotUI = slotObj.GetComponent<SkillSlotUI>();

        if (slotUI == null)
        {
            Debug.LogError("Skill Slot Prefab does not have SkillSlotUI component!");
            Destroy(slotObj);
            return;
        }

        // Initialize slot
        slotUI.Initialize(skillName, icon, keyBinding, cooldownDuration);

        // Add to tracking
        activeSkills[skillType] = slotUI;
        skillSlots.Add(slotUI);

        // Reposition slots (right to left)
        RepositionSlots();

        Debug.Log($"Skill added to UI: {skillName} ({keyBinding}) - Total skills: {skillSlots.Count}");
    }

    /// <summary>
    /// Trigger cooldown for a specific skill
    /// </summary>
    public void TriggerCooldown(SkillType skillType, float cooldownDuration)
    {
        if (activeSkills.ContainsKey(skillType))
        {
            activeSkills[skillType].StartCooldown(cooldownDuration);
        }
    }

    /// <summary>
    /// Reposition all skill slots (right to left order)
    /// </summary>
    private void RepositionSlots()
    {
        // Position slots from right to left
        // First skill = rightmost, newer skills go to the left

        for (int i = 0; i < skillSlots.Count; i++)
        {
            RectTransform slotRect = skillSlots[i].GetComponent<RectTransform>();
            if (slotRect != null)
            {
                // Calculate position from right
                // Index 0 = rightmost, higher index = more to the left
                float xPos = -(i * slotSpacing);
                slotRect.anchoredPosition = new Vector2(xPos, 0f);
            }
        }
    }

    /// <summary>
    /// Get skill name by type
    /// </summary>
    private string GetSkillName(SkillType type)
    {
        switch (type)
        {
            case SkillType.Fireball: return "Fireball";
            case SkillType.Explosion: return "Arcane Burst";
            case SkillType.Defense: return "Slime Shield";
            case SkillType.Dash: return "Wolf Dash";
            default: return "Unknown";
        }
    }

    /// <summary>
    /// Get skill icon by type
    /// </summary>
    private Sprite GetSkillIcon(SkillType type)
    {
        switch (type)
        {
            case SkillType.Fireball: return fireballIcon;
            case SkillType.Explosion: return explosionIcon;
            case SkillType.Defense: return defenseIcon;
            case SkillType.Dash: return dashIcon;
            default: return null;
        }
    }

    /// <summary>
    /// Get key binding text by type
    /// </summary>
    private string GetKeyBinding(SkillType type)
    {
        switch (type)
        {
            case SkillType.Fireball: return "E";
            case SkillType.Explosion: return "Q";
            case SkillType.Defense: return "Space";
            case SkillType.Dash: return "RMB";
            default: return "?";
        }
    }

    /// <summary>
    /// Clear all skill slots
    /// </summary>
    public void ClearAllSkills()
    {
        foreach (var slot in skillSlots)
        {
            if (slot != null)
            {
                Destroy(slot.gameObject);
            }
        }

        skillSlots.Clear();
        activeSkills.Clear();

        Debug.Log("All skills cleared from UI");
    }
}
