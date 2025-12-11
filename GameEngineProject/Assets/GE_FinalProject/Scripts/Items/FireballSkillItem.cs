using UnityEngine;

/// <summary>
/// Fireball skill item dropped by Wizard Boss
/// 마법사 보스가 드롭하는 파이어볼 스킬 아이템
/// </summary>
public class FireballSkillItem : SkillItem
{
    protected override void Start()
    {
        base.Start();
        skillName = "Fireball";
    }

    protected override void GrantSkill(PlayerController player)
    {
        // Unlock fireball skill
        player.UnlockFireballSkill();

        Debug.Log("Player unlocked Fireball skill! Press Right Mouse Button to cast.");

        // Note: WizardSpellbookItem is used instead, which shows both skills
        // Optional: Show UI notification here
        // Example: UIManager.Instance.ShowSkillUnlockNotification("Fireball Unlocked!");
    }
}