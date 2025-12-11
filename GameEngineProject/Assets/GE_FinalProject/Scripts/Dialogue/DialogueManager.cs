using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// Manages dialogue display and progression
/// 대사 표시 및 진행 관리
/// </summary>
public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private DialogueUI dialogueUI;

    [Header("Typing Settings")]
    [SerializeField] private float defaultTypingSpeed = 30f; // 글자/초
    [SerializeField] private bool allowSkipTyping = true; // 타이핑 중 스킵 가능

    [Header("Sound Effects")]
    [SerializeField] private AudioClip typingSound;
    [SerializeField] private AudioClip advanceSound; // 다음 대사로 넘어갈 때
    [SerializeField] private AudioClip completeSound; // 대화 종료 시
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.5f;

    [Header("Input Settings")]
    [SerializeField] private bool useSpaceToAdvance = true;
    [SerializeField] private bool useMouseToAdvance = true;

    private AudioSource audioSource;
    private Keyboard keyboard;
    private Mouse mouse;

    private Dialogue[] currentDialogues;
    private int currentDialogueIndex = 0;
    private bool isTyping = false;
    private bool isDialogueActive = false;
    private Coroutine typingCoroutine;

    private bool pausedGame = false;
    private bool disabledPlayer = false;
    private PlayerController playerController;

    private UnityEvent onDialogueComplete = new UnityEvent();

    private static DialogueManager instance;

    public static DialogueManager Instance => instance;

    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Get or add AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.volume = sfxVolume;

        // Get input devices
        keyboard = Keyboard.current;
        mouse = Mouse.current;

        // Auto-find DialogueUI if not assigned
        if (dialogueUI == null)
        {
            dialogueUI = FindObjectOfType<DialogueUI>();
        }

        if (dialogueUI == null)
        {
            Debug.LogError("[DialogueManager] DialogueUI not found! Please assign or create DialogueUI in scene.");
        }
    }

    private void Update()
    {
        if (!isDialogueActive) return;

        // Get input devices if not available
        if (keyboard == null) keyboard = Keyboard.current;
        if (mouse == null) mouse = Mouse.current;

        // Check for advance input
        bool advancePressed = false;

        if (useSpaceToAdvance && keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
        {
            advancePressed = true;
        }

        if (useMouseToAdvance && mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            advancePressed = true;
        }

        if (advancePressed)
        {
            if (isTyping && allowSkipTyping)
            {
                // Skip typing animation
                SkipTyping();
            }
            else if (!isTyping)
            {
                // Advance to next dialogue
                AdvanceDialogue();
            }
        }
    }

    /// <summary>
    /// Start a dialogue sequence
    /// </summary>
    public static void StartDialogue(Dialogue[] dialogues, bool pauseGame = true, bool disablePlayer = true)
    {
        if (instance != null)
        {
            instance.BeginDialogue(dialogues, pauseGame, disablePlayer);
        }
        else
        {
            Debug.LogError("[DialogueManager] Instance not found!");
        }
    }

    /// <summary>
    /// Start a dialogue sequence from ScriptableObject
    /// </summary>
    public static void StartDialogue(DialogueSequence sequence)
    {
        if (instance != null && sequence != null)
        {
            instance.BeginDialogue(sequence.dialogues, sequence.pauseGame, sequence.disablePlayerControl);
        }
        else
        {
            Debug.LogError("[DialogueManager] Instance or sequence is null!");
        }
    }

    private void BeginDialogue(Dialogue[] dialogues, bool pauseGame, bool disablePlayer)
    {
        if (isDialogueActive)
        {
            Debug.LogWarning("[DialogueManager] Dialogue already active!");
            return;
        }

        if (dialogues == null || dialogues.Length == 0)
        {
            Debug.LogError("[DialogueManager] No dialogues to display!");
            return;
        }

        currentDialogues = dialogues;
        currentDialogueIndex = 0;
        isDialogueActive = true;

        // Pause game if needed
        if (pauseGame)
        {
            Time.timeScale = 0f;
            pausedGame = true;
        }

        // Disable player control if needed
        if (disablePlayer)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerController = player.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.enabled = false;
                    disabledPlayer = true;
                }
            }
        }

        // Show dialogue UI
        if (dialogueUI != null)
        {
            dialogueUI.Show();
        }

        // Display first dialogue
        DisplayCurrentDialogue();

        Debug.Log($"[DialogueManager] Started dialogue sequence with {dialogues.Length} lines");
    }

    private void DisplayCurrentDialogue()
    {
        if (currentDialogueIndex >= currentDialogues.Length)
        {
            EndDialogue();
            return;
        }

        Dialogue dialogue = currentDialogues[currentDialogueIndex];

        // Stop previous typing coroutine
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // Start typing animation
        float speed = dialogue.typingSpeed > 0 ? dialogue.typingSpeed : defaultTypingSpeed;
        typingCoroutine = StartCoroutine(TypeDialogue(dialogue, speed));
    }

    private IEnumerator TypeDialogue(Dialogue dialogue, float speed)
    {
        isTyping = true;

        // Update speaker name
        if (dialogueUI != null)
        {
            dialogueUI.SetSpeakerName(dialogue.speakerName);
            dialogueUI.SetDialogueText("");
        }

        // Type out text character by character
        string fullText = dialogue.text;
        string currentText = "";

        float timePerCharacter = 1f / speed;
        float timer = 0f;
        int currentCharIndex = 0;

        while (currentCharIndex < fullText.Length)
        {
            // Use unscaled time if game is paused
            timer += pausedGame ? Time.unscaledDeltaTime : Time.deltaTime;

            if (timer >= timePerCharacter)
            {
                timer -= timePerCharacter;
                currentText += fullText[currentCharIndex];
                currentCharIndex++;

                // Update UI
                if (dialogueUI != null)
                {
                    dialogueUI.SetDialogueText(currentText);
                }

                // Play typing sound
                if (typingSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(typingSound, sfxVolume * 0.3f);
                }
            }

            yield return null;
        }

        // Ensure full text is displayed
        if (dialogueUI != null)
        {
            dialogueUI.SetDialogueText(fullText);
        }

        isTyping = false;

        // Auto-advance if display duration is set
        if (dialogue.displayDuration > 0)
        {
            float waitTime = dialogue.displayDuration;
            float elapsed = 0f;

            while (elapsed < waitTime)
            {
                elapsed += pausedGame ? Time.unscaledDeltaTime : Time.deltaTime;
                yield return null;
            }

            AdvanceDialogue();
        }
    }

    private void SkipTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;

        // Display full text immediately
        if (currentDialogueIndex < currentDialogues.Length)
        {
            Dialogue dialogue = currentDialogues[currentDialogueIndex];
            if (dialogueUI != null)
            {
                dialogueUI.SetDialogueText(dialogue.text);
            }
        }
    }

    private void AdvanceDialogue()
    {
        // Play advance sound
        if (advanceSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(advanceSound, sfxVolume);
        }

        currentDialogueIndex++;

        if (currentDialogueIndex < currentDialogues.Length)
        {
            DisplayCurrentDialogue();
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        isTyping = false;

        // Hide dialogue UI
        if (dialogueUI != null)
        {
            dialogueUI.Hide();
        }

        // Resume game if paused
        if (pausedGame)
        {
            Time.timeScale = 1f;
            pausedGame = false;
        }

        // Re-enable player control
        if (disabledPlayer && playerController != null)
        {
            playerController.enabled = true;
            disabledPlayer = false;
            playerController = null;
        }

        // Play complete sound
        if (completeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(completeSound, sfxVolume);
        }

        // Invoke completion event
        onDialogueComplete?.Invoke();
        onDialogueComplete.RemoveAllListeners();

        Debug.Log("[DialogueManager] Dialogue sequence completed");
    }

    /// <summary>
    /// Check if dialogue is currently active
    /// </summary>
    public static bool IsDialogueActive()
    {
        return instance != null && instance.isDialogueActive;
    }

    /// <summary>
    /// Add a callback to execute when dialogue completes
    /// </summary>
    public static void OnDialogueComplete(UnityAction callback)
    {
        if (instance != null)
        {
            instance.onDialogueComplete.AddListener(callback);
        }
    }

    /// <summary>
    /// Force end current dialogue
    /// </summary>
    public static void ForceEndDialogue()
    {
        if (instance != null && instance.isDialogueActive)
        {
            instance.EndDialogue();
        }
    }
}
