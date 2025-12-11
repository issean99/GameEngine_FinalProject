using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Ending scene dialogue with player color change and "The End" display
/// 엔딩 씬 대사와 플레이어 색상 변화, "The End" 표시
/// </summary>
public class EndingDialogue : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [TextArea(2, 5)]
    [SerializeField] private string[] beforeColorChangeDialogues = new string[]
    {
        "드디어... 끝났어.",
        "던전을 모두 정복했다.",
        "하지만... 이 힘은..."
    };

    [TextArea(2, 5)]
    [SerializeField] private string[] afterColorChangeDialogues = new string[]
    {
        "...뭐지? 내 몸이...",
        "던전의 힘이... 내 안으로...",
        "이제... 나는..."
    };

    [Header("Player Settings")]
    [SerializeField] private Color corruptedColor = new Color(0.5f, 0f, 0.8f, 1f); // Purple color
    [SerializeField] private string playerName = "플레이어";
    [SerializeField] private string corruptedPlayerName = "???";

    [Header("Fade Settings")]
    [SerializeField] private float fadeToBlackDuration = 2f;
    [SerializeField] private float delayAfterTheEnd = 3f; // Time to show "The End" before quitting game

    [Header("The End Settings")]
    [SerializeField] private TextMeshProUGUI theEndText; // TextMeshPro text component for "The End"
    [SerializeField] private Font theEndFont; // Custom font for "The End" text (optional)
    [SerializeField] private int theEndFontSize = 80;
    [SerializeField] private float theEndFadeInDuration = 2f;
    [SerializeField] private float delayBeforeTheEnd = 1f;

    [Header("Timing")]
    [SerializeField] private float delayBeforeStart = 1.5f;

    private GameObject player;
    private SpriteRenderer playerSpriteRenderer;
    private Camera mainCamera;
    private Texture2D fadeTexture;
    private float currentFadeAlpha = 0f;

    private void Start()
    {
        // Find player
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerSpriteRenderer = player.GetComponent<SpriteRenderer>();
        }

        // Find main camera
        mainCamera = Camera.main;

        // Create fade texture
        fadeTexture = new Texture2D(1, 1);
        fadeTexture.SetPixel(0, 0, Color.black);
        fadeTexture.Apply();

        // Hide "The End" text initially
        if (theEndText != null)
        {
            CanvasGroup canvasGroup = theEndText.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = theEndText.gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0f;
        }

        // Hide player UI (health, skills, etc.)
        HidePlayerUI();

        // Start ending sequence
        StartCoroutine(EndingSequence());
    }

    private void HidePlayerUI()
    {
        Debug.Log("[EndingDialogue] Starting to hide all player UI...");

        // Method 1: Find and disable PlayerHealthUI component
        PlayerHealthUI[] healthUIs = FindObjectsOfType<PlayerHealthUI>(true);
        foreach (PlayerHealthUI healthUI in healthUIs)
        {
            healthUI.gameObject.SetActive(false);
            Debug.Log($"[EndingDialogue] Disabled PlayerHealthUI on {healthUI.gameObject.name}");
        }

        // Method 2: Find and disable SkillUIManager component
        SkillUIManager[] skillManagers = FindObjectsOfType<SkillUIManager>(true);
        foreach (SkillUIManager skillManager in skillManagers)
        {
            skillManager.gameObject.SetActive(false);
            Debug.Log($"[EndingDialogue] Disabled SkillUIManager on {skillManager.gameObject.name}");
        }

        // Method 3: Find all SkillSlotUI components
        SkillSlotUI[] skillSlots = FindObjectsOfType<SkillSlotUI>(true);
        foreach (SkillSlotUI skillSlot in skillSlots)
        {
            skillSlot.gameObject.SetActive(false);
            Debug.Log($"[EndingDialogue] Disabled SkillSlotUI on {skillSlot.gameObject.name}");
        }

        // Method 4: Search by common GameObject names
        string[] uiNames = new string[]
        {
            "PlayerHealthUI", "HealthUI", "Health UI", "HP UI",
            "SkillUI", "Skill UI", "PlayerSkillUI", "Skills",
            "PlayerUI", "HUD", "GameUI"
        };

        foreach (string uiName in uiNames)
        {
            GameObject foundUI = GameObject.Find(uiName);
            if (foundUI != null)
            {
                foundUI.SetActive(false);
                Debug.Log($"[EndingDialogue] Disabled UI GameObject: {uiName}");
            }
        }

        // Method 5: Find all Canvas and check their children
        Canvas[] allCanvases = FindObjectsOfType<Canvas>(true);
        Debug.Log($"[EndingDialogue] Found {allCanvases.Length} canvases in scene");

        foreach (Canvas canvas in allCanvases)
        {
            // Skip dialogue canvas
            if (canvas.name.Contains("Dialogue") || canvas.name.Contains("TheEnd"))
            {
                continue;
            }

            // Check for health/skill related children
            foreach (Transform child in canvas.transform)
            {
                string childName = child.name.ToLower();
                if (childName.Contains("health") || childName.Contains("hp") ||
                    childName.Contains("skill") || childName.Contains("player"))
                {
                    child.gameObject.SetActive(false);
                    Debug.Log($"[EndingDialogue] Disabled canvas child: {child.name} in {canvas.name}");
                }
            }
        }

        Debug.Log("[EndingDialogue] Player UI hiding complete");
    }

    private IEnumerator EndingSequence()
    {
        // Wait before starting
        yield return new WaitForSeconds(delayBeforeStart);

        // Phase 1: Before color change dialogues
        Dialogue[] phase1Dialogues = CreateDialogues(playerName, beforeColorChangeDialogues);
        DialogueManager.StartDialogue(phase1Dialogues, pauseGame: false, disablePlayer: true);

        // Wait for phase 1 dialogues to complete
        while (DialogueManager.IsDialogueActive())
        {
            yield return null;
        }

        // Change player color to corrupted purple
        if (player != null)
        {
            if (playerSpriteRenderer != null)
            {
                Debug.Log($"[EndingDialogue] Changing player color to corrupted purple: {corruptedColor}");
                playerSpriteRenderer.color = corruptedColor;
                Debug.Log($"[EndingDialogue] Player color changed! Current color: {playerSpriteRenderer.color}");
            }
            else
            {
                Debug.LogWarning("[EndingDialogue] PlayerSpriteRenderer is null! Trying to find it again...");
                playerSpriteRenderer = player.GetComponent<SpriteRenderer>();
                if (playerSpriteRenderer != null)
                {
                    playerSpriteRenderer.color = corruptedColor;
                    Debug.Log($"[EndingDialogue] Found SpriteRenderer and changed color: {playerSpriteRenderer.color}");
                }
                else
                {
                    Debug.LogError("[EndingDialogue] Failed to find SpriteRenderer on player!");
                }
            }

            // Also check for child sprite renderers (in case player has child objects)
            SpriteRenderer[] allRenderers = player.GetComponentsInChildren<SpriteRenderer>();
            Debug.Log($"[EndingDialogue] Found {allRenderers.Length} sprite renderers on player");
            foreach (SpriteRenderer renderer in allRenderers)
            {
                renderer.color = corruptedColor;
                Debug.Log($"[EndingDialogue] Changed color of {renderer.gameObject.name} to purple");
            }
        }
        else
        {
            Debug.LogError("[EndingDialogue] Player is null! Cannot change color.");
        }

        // Small delay for visual impact
        yield return new WaitForSeconds(0.5f);

        // Phase 2: After color change dialogues (with corrupted name)
        Dialogue[] phase2Dialogues = CreateDialogues(corruptedPlayerName, afterColorChangeDialogues);
        DialogueManager.StartDialogue(phase2Dialogues, pauseGame: false, disablePlayer: true);

        // Wait for phase 2 dialogues to complete
        while (DialogueManager.IsDialogueActive())
        {
            yield return null;
        }

        // Wait before showing "The End"
        yield return new WaitForSeconds(delayBeforeTheEnd);

        // Show "The End"
        yield return StartCoroutine(ShowTheEnd());
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

    private IEnumerator ShowTheEnd()
    {
        Debug.Log("[EndingDialogue] Starting fade to black...");

        // Fade to black
        float elapsed = 0f;
        while (elapsed < fadeToBlackDuration)
        {
            elapsed += Time.deltaTime;
            currentFadeAlpha = Mathf.Clamp01(elapsed / fadeToBlackDuration);
            yield return null;
        }
        currentFadeAlpha = 1f;

        Debug.Log("[EndingDialogue] Screen faded to black");

        // Small delay after fade to black
        yield return new WaitForSeconds(0.5f);

        Debug.Log("[EndingDialogue] Showing 'The End'");

        // Fade in "The End" text
        if (theEndText != null)
        {
            CanvasGroup canvasGroup = theEndText.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = theEndText.gameObject.AddComponent<CanvasGroup>();
            }

            elapsed = 0f;
            while (elapsed < theEndFadeInDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / theEndFadeInDuration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        Debug.Log("[EndingDialogue] 'The End' displayed");

        // Wait before quitting game
        yield return new WaitForSeconds(delayAfterTheEnd);

        Debug.Log("[EndingDialogue] Quitting game...");

        // Quit the game
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private void OnGUI()
    {
        // Draw black fade overlay
        if (currentFadeAlpha > 0f && fadeTexture != null)
        {
            Color fadeColor = new Color(0, 0, 0, currentFadeAlpha);
            GUI.color = fadeColor;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeTexture);
            GUI.color = Color.white;
        }

        // Draw "The End" text on black screen
        if (currentFadeAlpha >= 1f && theEndText != null)
        {
            CanvasGroup canvasGroup = theEndText.GetComponent<CanvasGroup>();
            if (canvasGroup != null && canvasGroup.alpha > 0f)
            {
                // Make sure the text is on top of the black overlay
                GUIStyle style = new GUIStyle(GUI.skin.label);

                // Use custom font if provided
                if (theEndFont != null)
                {
                    style.font = theEndFont;
                }

                style.fontSize = theEndFontSize;
                style.alignment = TextAnchor.MiddleCenter;
                style.normal.textColor = new Color(1, 1, 1, canvasGroup.alpha);
                style.fontStyle = FontStyle.Bold;

                GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "The End", style);
            }
        }
    }
}
