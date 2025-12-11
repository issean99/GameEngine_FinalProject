using UnityEngine;

/// <summary>
/// Werewolf dash skill item - dropped by WereWolf enemies
/// 웨어울프가 드롭하는 대쉬 스킬 아이템
/// </summary>
public class WerewolfDashSkillItem : SkillItem
{
    protected override void GrantSkill(PlayerController player)
    {
        player.UnlockDashSkill();
        Debug.Log("Werewolf Soul absorbed! Learned Dash skill (Shift Key)");

        // Show skill description panel
        SkillDescriptionPanel.ShowSkillByName("Dash");
    }
}
