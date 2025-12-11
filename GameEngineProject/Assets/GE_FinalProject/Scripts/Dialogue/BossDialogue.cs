using UnityEngine;

/// <summary>
/// Predefined boss dialogues for easy setup
/// 보스 대사 미리 정의
/// </summary>
public class BossDialogue : MonoBehaviour
{
    [Header("Boss 1 (Wizard) Dialogues")]
    [TextArea(2, 5)]
    [SerializeField] private string[] boss1IntroLines = new string[]
    {
        "누가 감히 내 영역에 발을 들였는가...",
        "어리석은 인간이여, 후회하게 될 것이다!",
        "내 마법의 힘을 보여주지!"
    };

    [TextArea(2, 5)]
    [SerializeField] private string[] boss1Phase2Lines = new string[]
    {
        "흥! 이제부터가 진짜다!",
        "내 진정한 힘을 보여주지!"
    };

    [TextArea(2, 5)]
    [SerializeField] private string[] boss1DeathLines = new string[]
    {
        "이럴 수가... 인간 따위에게...",
        "내가... 패배하다니..."
    };

    [Header("Boss 2 (Final Boss) Dialogues")]
    [TextArea(2, 5)]
    [SerializeField] private string[] boss2IntroLines = new string[]
    {
        "...",
        "이곳이 네 무덤이 될 것이다.",
        "준비는 되었는가?"
    };

    [TextArea(2, 5)]
    [SerializeField] private string[] boss2Phase2Lines = new string[]
    {
        "허약한 것... 이게 전부인가?",
        "내 진정한 힘을 보여주마!"
    };

    [TextArea(2, 5)]
    [SerializeField] private string[] boss2DeathLines = new string[]
    {
        "...불가능하다...",
        "내가... 이곳에서...",
        "........."
    };

    [Header("Speaker Names")]
    [SerializeField] private string boss1Name = "위저드";
    [SerializeField] private string boss2Name = "???";
    [SerializeField] private string playerName = "플레이어";

    /// <summary>
    /// Get Boss 1 intro dialogues
    /// </summary>
    public Dialogue[] GetBoss1IntroDialogues()
    {
        return CreateDialogues(boss1Name, boss1IntroLines);
    }

    /// <summary>
    /// Get Boss 1 Phase 2 dialogues
    /// </summary>
    public Dialogue[] GetBoss1Phase2Dialogues()
    {
        return CreateDialogues(boss1Name, boss1Phase2Lines);
    }

    /// <summary>
    /// Get Boss 2 intro dialogues
    /// </summary>
    public Dialogue[] GetBoss2IntroDialogues()
    {
        return CreateDialogues(boss2Name, boss2IntroLines);
    }

    /// <summary>
    /// Get Boss 2 Phase 2 dialogues
    /// </summary>
    public Dialogue[] GetBoss2Phase2Dialogues()
    {
        return CreateDialogues(boss2Name, boss2Phase2Lines);
    }

    /// <summary>
    /// Get Boss 1 death dialogues
    /// </summary>
    public Dialogue[] GetBoss1DeathDialogues()
    {
        return CreateDialogues(boss1Name, boss1DeathLines);
    }

    /// <summary>
    /// Get Boss 2 death dialogues
    /// </summary>
    public Dialogue[] GetBoss2DeathDialogues()
    {
        return CreateDialogues(boss2Name, boss2DeathLines);
    }

    private Dialogue[] CreateDialogues(string speaker, string[] lines)
    {
        Dialogue[] dialogues = new Dialogue[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            dialogues[i] = new Dialogue(speaker, lines[i]);
        }
        return dialogues;
    }

    /// <summary>
    /// Helper method to add player response dialogue
    /// </summary>
    public Dialogue CreatePlayerDialogue(string text)
    {
        return new Dialogue(playerName, text);
    }

    /// <summary>
    /// Create a dialogue with custom speaker
    /// </summary>
    public static Dialogue CreateDialogue(string speaker, string text)
    {
        return new Dialogue(speaker, text);
    }
}
