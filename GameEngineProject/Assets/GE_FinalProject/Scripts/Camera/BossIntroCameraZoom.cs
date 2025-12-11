using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Unity.Cinemachine;

/// <summary>
/// Boss intro camera pan - moves camera to boss, then starts dialogue (Cinemachine compatible)
/// 보스 인트로 카메라 이동 - 보스로 이동 후 대사 시작 (Cinemachine 호환)
/// </summary>
public class BossIntroCameraZoom : MonoBehaviour
{
    [Header("Camera Pan Settings")]
    [SerializeField] private float panDuration = 1.5f; // 보스로 이동 시간
    [SerializeField] private float holdDuration = 1f; // 보스에게 고정된 시간
    [SerializeField] private float panBackDuration = 1f; // 플레이어로 복귀 시간
    [SerializeField] private AnimationCurve panCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Dialogue")]
    [SerializeField] private bool triggerDialogueAfterPan = true;

    [Header("Events")]
    [SerializeField] private UnityEvent onZoomComplete;

    private bool hasPlayed = false;
    private Camera mainCamera;
    private CinemachineVirtualCamera playerVCam;
    private CinemachineBrain cinemachineBrain;
    private GameObject tempBossVCam;
    private Transform bossTransform; // Auto-detected boss transform

    private void Awake()
    {
        // Reset hasPlayed for each scene (allows intro to play in each boss scene)
        hasPlayed = false;

        Debug.Log("[BossIntroCameraZoom] Awake called");

        // Find main camera
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Debug.Log($"[BossIntroCameraZoom] Found main camera: {mainCamera.name}");

            // Find Cinemachine Brain
            cinemachineBrain = mainCamera.GetComponent<CinemachineBrain>();
            if (cinemachineBrain != null)
            {
                Debug.Log("[BossIntroCameraZoom] Found CinemachineBrain");
            }
        }

        // Find player's Cinemachine Virtual Camera
        playerVCam = FindObjectOfType<CinemachineVirtualCamera>();
        if (playerVCam != null)
        {
            Debug.Log($"[BossIntroCameraZoom] Found player VCam: {playerVCam.name}");
        }
    }

    private void Start()
    {
        Debug.Log("[BossIntroCameraZoom] Start called");

        // Auto-find boss
        WizardBoss wizardBoss = FindObjectOfType<WizardBoss>();
        FinalBoss finalBoss = FindObjectOfType<FinalBoss>();

        // Start dialogue immediately without camera movement
        if (wizardBoss != null || finalBoss != null)
        {
            Debug.Log("[BossIntroCameraZoom] Boss found! Starting dialogue...");
            Invoke(nameof(StartDialogueOnly), 0.5f);
        }
        else
        {
            Debug.Log("[BossIntroCameraZoom] No boss found in scene");
        }
    }

    private void StartDialogueOnly()
    {
        if (hasPlayed) return;
        hasPlayed = true;

        // Get BossDialogue component
        BossDialogue bossDialogue = FindObjectOfType<BossDialogue>();
        if (bossDialogue == null)
        {
            Debug.LogWarning("[BossIntroCameraZoom] BossDialogue component not found!");
            return;
        }

        Dialogue[] introDialogues = null;

        // Get appropriate dialogues
        WizardBoss wizardBoss = FindObjectOfType<WizardBoss>();
        FinalBoss finalBoss = FindObjectOfType<FinalBoss>();

        if (wizardBoss != null)
        {
            introDialogues = bossDialogue.GetBoss1IntroDialogues();
            Debug.Log($"[BossIntroCameraZoom] Boss 1 dialogue - {introDialogues.Length} lines");
        }
        else if (finalBoss != null)
        {
            introDialogues = bossDialogue.GetBoss2IntroDialogues();
            Debug.Log($"[BossIntroCameraZoom] Boss 2 dialogue - {introDialogues.Length} lines");
        }

        // Start dialogue
        if (triggerDialogueAfterPan && introDialogues != null && introDialogues.Length > 0)
        {
            Debug.Log("[BossIntroCameraZoom] Starting boss dialogue...");
            // pauseGame: false (게임 계속 진행), disablePlayer: true (플레이어 이동 불가)
            DialogueManager.StartDialogue(introDialogues, pauseGame: false, disablePlayer: true);
        }
        else
        {
            Debug.LogWarning("[BossIntroCameraZoom] No dialogue to play");
        }
    }

    private void StartBossIntro()
    {
        if (hasPlayed) return;
        hasPlayed = true;

        if (bossTransform == null)
        {
            Debug.LogWarning("[BossIntroCameraZoom] Boss transform not found! Skipping intro.");
            return;
        }

        StartCoroutine(BossIntroSequence());
    }

    private IEnumerator BossIntroSequence()
    {
        Debug.Log("[BossIntroCameraZoom] BossIntroSequence started!");

        // Get BossDialogue component to fetch dialogues
        BossDialogue bossDialogue = FindObjectOfType<BossDialogue>();
        if (bossDialogue == null)
        {
            Debug.LogWarning("[BossIntroCameraZoom] BossDialogue component not found in scene! Please add BossDialogue to the scene.");
        }

        Dialogue[] introDialogues = null;

        // Determine which boss and get appropriate dialogues
        WizardBoss wizardBoss = FindObjectOfType<WizardBoss>();
        FinalBoss finalBoss = FindObjectOfType<FinalBoss>();

        if (wizardBoss != null && bossDialogue != null)
        {
            introDialogues = bossDialogue.GetBoss1IntroDialogues();
            Debug.Log($"[BossIntroCameraZoom] Using Boss 1 (Wizard) intro dialogues - {introDialogues.Length} lines");
        }
        else if (finalBoss != null && bossDialogue != null)
        {
            introDialogues = bossDialogue.GetBoss2IntroDialogues();
            Debug.Log($"[BossIntroCameraZoom] Using Boss 2 (Final Boss) intro dialogues - {introDialogues.Length} lines");
        }
        else if (bossDialogue == null)
        {
            Debug.LogWarning("[BossIntroCameraZoom] Cannot get dialogues - BossDialogue component missing!");
        }

        // Disable player control
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        PlayerController playerController = null;
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = false;
                Debug.Log("[BossIntroCameraZoom] Disabled player control");
            }
        }

        // Create temporary Cinemachine VCam for boss
        tempBossVCam = new GameObject("TempBossVCam");
        CinemachineVirtualCamera bossVCam = tempBossVCam.AddComponent<CinemachineVirtualCamera>();

        // Set boss VCam to highest priority
        if (playerVCam != null)
        {
            bossVCam.Priority = playerVCam.Priority + 10;
            playerVCam.enabled = false; // Disable player VCam
            Debug.Log("[BossIntroCameraZoom] Disabled player VCam, created boss VCam");
        }
        else
        {
            bossVCam.Priority = 20;
        }

        // Make boss VCam follow and look at the boss
        bossVCam.Follow = bossTransform;
        bossVCam.LookAt = bossTransform;

        // Copy orthographic size from player camera if available
        if (playerVCam != null && mainCamera != null && mainCamera.orthographic)
        {
            bossVCam.m_Lens.OrthographicSize = mainCamera.orthographicSize;
        }
        else if (mainCamera != null && mainCamera.orthographic)
        {
            bossVCam.m_Lens.OrthographicSize = mainCamera.orthographicSize;
        }

        Debug.Log($"[BossIntroCameraZoom] Boss VCam created, targeting: {bossTransform.position}");

        // Wait for Cinemachine to blend to boss
        yield return new WaitForSeconds(panDuration);
        Debug.Log("[BossIntroCameraZoom] Reached boss position");

        // Phase 2: Hold on boss
        Debug.Log($"[BossIntroCameraZoom] Holding on boss for {holdDuration}s");
        yield return new WaitForSeconds(holdDuration);

        // Phase 3: Start dialogue (if enabled)
        if (triggerDialogueAfterPan && introDialogues != null && introDialogues.Length > 0)
        {
            Debug.Log("[BossIntroCameraZoom] Starting boss dialogue...");
            DialogueManager.StartDialogue(introDialogues, pauseGame: false, disablePlayer: false);

            // Wait for dialogue to complete
            while (DialogueManager.IsDialogueActive())
            {
                yield return null;
            }
            Debug.Log("[BossIntroCameraZoom] Dialogue completed");
        }
        else
        {
            Debug.LogWarning($"[BossIntroCameraZoom] No dialogue to play. triggerDialogueAfterPan={triggerDialogueAfterPan}, introDialogues={(introDialogues == null ? "null" : introDialogues.Length.ToString())}");
        }

        // Phase 4: Pan back to player
        Debug.Log($"[BossIntroCameraZoom] Panning back to player (duration: {panBackDuration}s)");

        // Destroy temp boss VCam
        if (tempBossVCam != null)
        {
            Destroy(tempBossVCam);
            Debug.Log("[BossIntroCameraZoom] Destroyed temp boss VCam");
        }

        // Re-enable player VCam
        if (playerVCam != null)
        {
            playerVCam.enabled = true;
            Debug.Log("[BossIntroCameraZoom] Re-enabled player VCam");
        }

        // Wait for blend back to player
        yield return new WaitForSeconds(panBackDuration);
        Debug.Log("[BossIntroCameraZoom] Returned to player");

        // Re-enable player control
        if (playerController != null)
        {
            playerController.enabled = true;
            Debug.Log("[BossIntroCameraZoom] Re-enabled player control");
        }

        // Invoke completion event
        onZoomComplete?.Invoke();

        Debug.Log("[BossIntroCameraZoom] Boss intro sequence completed!");
    }

    /// <summary>
    /// Manually trigger boss intro (for testing)
    /// </summary>
    public void TriggerBossIntro()
    {
        hasPlayed = false;
        StartBossIntro();
    }

    /// <summary>
    /// Skip the intro sequence
    /// </summary>
    public void SkipIntro()
    {
        StopAllCoroutines();
        hasPlayed = true;

        // Clean up temp boss VCam
        if (tempBossVCam != null)
        {
            Destroy(tempBossVCam);
        }

        // Re-enable player VCam
        if (playerVCam != null)
        {
            playerVCam.enabled = true;
        }

        // Re-enable player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.enabled = true;
            }
        }

        // End any active dialogue
        DialogueManager.ForceEndDialogue();
    }
}
