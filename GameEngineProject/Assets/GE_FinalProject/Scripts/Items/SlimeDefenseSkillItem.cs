using UnityEngine;

public class SlimeDefenseSkillItem : SkillItem
{
    protected override void GrantSkill(PlayerController player)
    {
        player.UnlockDefenseSkill();
        Debug.Log("Slime Soul absorbed! Learned Slime Defense skill (Down Arrow Key)");

        // Show skill description panel
        SkillDescriptionPanel.ShowSkillByName("Slime Defense");
    }
}
