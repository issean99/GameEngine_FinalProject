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

        // Show skill description panels for both skills
        // First show Fireball, then when closed, show Arcane Explosion
        StartCoroutine(ShowBothSkillPanels());
    }

    private System.Collections.IEnumerator ShowBothSkillPanels()
    {
        // Show Fireball panel first
        SkillDescriptionPanel.ShowSkillByName("Fireball");

        // Wait until the panel is closed
        while (SkillDescriptionPanel.IsShowing())
        {
            yield return null;
        }

        // Small delay between panels
        yield return new WaitForSeconds(0.2f);

        // Show Arcane Explosion panel
        SkillDescriptionPanel.ShowSkillByName("Arcane Explosion");
    }
}
