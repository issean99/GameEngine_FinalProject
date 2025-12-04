using UnityEngine;

/// <summary>
/// Explosion skill item dropped by Wizard Boss
/// 마법사 보스가 드롭하는 폭발 스킬 아이템
/// </summary>
public class ExplosionSkillItem : SkillItem
{
    protected override void Start()
    {
        base.Start();
        skillName = "Arcane Explosion";
    }

    protected override void GrantSkill(PlayerController player)
    {
        // Unlock explosion skill
        player.UnlockExplosionSkill();

        Debug.Log("Player unlocked Arcane Explosion skill! Press Q to cast at mouse position.");

        // Optional: Show UI notification here
        // Example: UIManager.Instance.ShowSkillUnlockNotification("Arcane Explosion Unlocked!");
    }
}
