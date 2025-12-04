using UnityEngine;

/// <summary>
/// Wizard's Spellbook item that grants both Fireball and Explosion skills
/// 마법사의 마법서 아이템 - 파이어볼과 폭발 스킬을 동시에 부여
/// </summary>
public class WizardSpellbookItem : SkillItem
{
    protected override void Start()
    {
        base.Start();
        skillName = "Wizard's Spellbook";
    }

    protected override void GrantSkill(PlayerController player)
    {
        // Unlock both fireball and explosion skills
        player.UnlockFireballSkill();
        player.UnlockExplosionSkill();

        Debug.Log("========================================");
        Debug.Log("Player obtained Wizard's Spellbook!");
        Debug.Log("Learned 2 Skills:");
        Debug.Log("  1. Fireball (Right Mouse Button) - 3 consecutive shots");
        Debug.Log("  2. Arcane Explosion (Q Key) - AoE explosion at mouse position");
        Debug.Log("========================================");

        // Optional: Show UI notification here
        // Example: UIManager.Instance.ShowSkillUnlockNotification("Wizard's Spellbook Obtained!\nFireball + Arcane Explosion");
    }
}
