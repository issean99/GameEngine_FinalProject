using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Triggers dialogue when Tutorial scene starts
/// 튜토리얼 씬 시작 시 대사 재생
/// </summary>
public class TutorialDialogue : MonoBehaviour
{
    [Header("Tutorial Dialogues")]
    [TextArea(2, 5)]
    [SerializeField] private string[] dialogueLines = new string[]
    {
        "여기는... 던전의 입구인가.",
        "조심해서 들어가야겠어.",
        "WASD로 이동하고, 마우스 왼쪽 클릭으로 공격할 수 있어.",
        "준비됐어. 가보자!"
    };

    [Header("Settings")]
    [SerializeField] private string speakerName = "플레이어";
    [SerializeField] private float delayBeforeDialogue = 1.5f; // 씬 시작 후 대기 시간
    [SerializeField] private bool playOnlyOnce = true;

    private static bool hasPlayed = false;

    private void Start()
    {
        // Only play in Tutorial scene
        if (SceneManager.GetActiveScene().name != "Tutorial")
        {
            return;
        }

        // Check if already played
        if (playOnlyOnce && hasPlayed)
        {
            return;
        }

        // Start dialogue after delay
        Invoke(nameof(StartTutorialDialogue), delayBeforeDialogue);
    }

    private void StartTutorialDialogue()
    {
        // Convert string array to Dialogue array
        Dialogue[] dialogues = new Dialogue[dialogueLines.Length];

        for (int i = 0; i < dialogueLines.Length; i++)
        {
            dialogues[i] = new Dialogue(speakerName, dialogueLines[i]);
        }

        // Start dialogue
        DialogueManager.StartDialogue(dialogues, pauseGame: true, disablePlayer: true);

        // Mark as played
        hasPlayed = true;

        Debug.Log("[TutorialDialogue] Started tutorial dialogue");
    }

    /// <summary>
    /// Reset so dialogue can play again (useful for testing)
    /// </summary>
    public static void ResetDialogue()
    {
        hasPlayed = false;
    }

    private void OnDestroy()
    {
        // Reset when leaving tutorial scene
        if (SceneManager.GetActiveScene().name == "Tutorial")
        {
            hasPlayed = false;
        }
    }
}
