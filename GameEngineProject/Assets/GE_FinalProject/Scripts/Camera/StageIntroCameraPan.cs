using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

/// <summary>
/// Stage intro camera pan - shows path from player spawn to portal (Cinemachine compatible)
/// 스테이지 시작 시 플레이어 시작 위치에서 포탈까지 카메라 이동 연출 (Cinemachine 호환)
/// </summary>
public class StageIntroCameraPan : MonoBehaviour
{
    [Header("Pan Settings")]
    [SerializeField] private float panDuration = 3f; // 카메라 이동 시간
    [SerializeField] private float pauseAtPortal = 1f; // 포탈에서 멈춰있는 시간
    [SerializeField] private float pauseBeforeStart = 0.5f; // 시작 전 대기 시간
    [SerializeField] private AnimationCurve panCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Zoom Settings")]
    [SerializeField] private bool enableZoomOut = true; // 이동 중 줌 아웃 효과
    [SerializeField] private float zoomOutAmount = 2f; // 줌 아웃 양

    [Header("Scene Settings")]
    [SerializeField] private string[] stageScenesWithIntro = new string[]
    {
        "Tutorial",
        "Stage1",
        "Stage2",
        "Stage3"
    }; // 인트로를 재생할 씬 목록

    private bool isPanning = false;
    private bool hasPlayedIntro = false;
    private Camera mainCamera;
    private CinemachineVirtualCamera playerVCam;
    private GameObject tempPanVCam;
    private float originalOrthographicSize;

    private static StageIntroCameraPan instance;

    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }

        // Auto-find references
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            originalOrthographicSize = mainCamera.orthographicSize;
            Debug.Log($"[StageIntroCameraPan] Found main camera: {mainCamera.name}");
        }

        // Find player's Cinemachine Virtual Camera
        playerVCam = FindObjectOfType<CinemachineVirtualCamera>();
        if (playerVCam != null)
        {
            Debug.Log($"[StageIntroCameraPan] Found player VCam: {playerVCam.name}");
        }

        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (instance == this)
        {
            instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check if this scene should have intro
        bool shouldPlayIntro = false;
        foreach (string stageName in stageScenesWithIntro)
        {
            if (scene.name == stageName)
            {
                shouldPlayIntro = true;
                break;
            }
        }

        if (shouldPlayIntro)
        {
            hasPlayedIntro = false;
            StartCoroutine(PlayStageIntro());
        }
    }

    private IEnumerator PlayStageIntro()
    {
        // Wait for scene to fully load
        yield return new WaitForSeconds(0.2f);

        // Find player and portal
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject portal = GameObject.FindGameObjectWithTag("Portal");

        // If portal not found, try to find ScenePortal script
        if (portal == null)
        {
            ScenePortal portalScript = FindObjectOfType<ScenePortal>();
            if (portalScript != null)
            {
                portal = portalScript.gameObject;
            }
        }

        if (player == null)
        {
            Debug.LogWarning("[StageIntroCameraPan] Player not found! Skipping intro.");
            hasPlayedIntro = true;
            yield break;
        }

        if (portal == null)
        {
            Debug.LogWarning("[StageIntroCameraPan] Portal not found! Skipping intro.");
            hasPlayedIntro = true;
            yield break;
        }

        // Disable player movement during intro
        PlayerController playerController = player.GetComponent<PlayerController>();
        bool playerWasEnabled = false;
        if (playerController != null)
        {
            playerWasEnabled = playerController.enabled;
            playerController.enabled = false;
        }

        // Disable player VCam during intro
        if (playerVCam != null)
        {
            playerVCam.enabled = false;
        }

        // Start camera pan
        yield return StartCoroutine(PanCamera(player.transform.position, portal.transform.position));

        // Re-enable player VCam
        if (playerVCam != null)
        {
            playerVCam.enabled = true;
        }

        // Clean up temporary VCam
        if (tempPanVCam != null)
        {
            Destroy(tempPanVCam);
            tempPanVCam = null;
        }

        // Re-enable player movement
        if (playerController != null && playerWasEnabled)
        {
            playerController.enabled = true;
        }

        hasPlayedIntro = true;
        Debug.Log("[StageIntroCameraPan] Stage intro completed!");
    }

    private IEnumerator PanCamera(Vector3 startPos, Vector3 endPos)
    {
        isPanning = true;

        // Create temporary GameObject for pan target
        GameObject panTarget = new GameObject("StageIntroPanTarget");
        panTarget.transform.position = startPos;

        // Create temporary Cinemachine Virtual Camera for panning
        tempPanVCam = new GameObject("TempStagePanVCam");
        CinemachineVirtualCamera panVCam = tempPanVCam.AddComponent<CinemachineVirtualCamera>();

        // Set pan VCam to highest priority
        if (playerVCam != null)
        {
            panVCam.Priority = playerVCam.Priority + 10;
        }
        else
        {
            panVCam.Priority = 20;
        }

        panVCam.Follow = panTarget.transform;
        panVCam.LookAt = panTarget.transform;

        // Apply zoom out if enabled
        if (enableZoomOut)
        {
            panVCam.m_Lens.OrthographicSize = originalOrthographicSize + zoomOutAmount;
        }
        else
        {
            panVCam.m_Lens.OrthographicSize = originalOrthographicSize;
        }

        // Initial pause at player position
        yield return new WaitForSeconds(pauseBeforeStart);

        // Pan phase - move target from start to end
        float elapsedTime = 0f;

        while (elapsedTime < panDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / panDuration;

            // Apply animation curve
            float curveT = panCurve.Evaluate(t);

            // Move pan target smoothly
            panTarget.transform.position = Vector3.Lerp(startPos, endPos, curveT);

            yield return null;
        }

        // Ensure final position
        panTarget.transform.position = endPos;

        // Pause at portal
        yield return new WaitForSeconds(pauseAtPortal);

        // Return to player
        elapsedTime = 0f;
        float returnDuration = 1f;

        while (elapsedTime < returnDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / returnDuration;

            // Move back to player
            panTarget.transform.position = Vector3.Lerp(endPos, startPos, t);

            // Zoom back in
            if (enableZoomOut)
            {
                panVCam.m_Lens.OrthographicSize = Mathf.Lerp(
                    originalOrthographicSize + zoomOutAmount,
                    originalOrthographicSize,
                    t
                );
            }

            yield return null;
        }

        // Clean up
        Destroy(panTarget);

        isPanning = false;
    }

    /// <summary>
    /// Skip the intro camera pan (useful for debugging or replaying stages)
    /// </summary>
    public static void SkipIntro()
    {
        if (instance != null && instance.isPanning)
        {
            instance.StopAllCoroutines();
            instance.isPanning = false;
            instance.hasPlayedIntro = true;

            // Re-enable player VCam
            if (instance.playerVCam != null)
            {
                instance.playerVCam.enabled = true;
            }

            // Clean up temporary VCam
            if (instance.tempPanVCam != null)
            {
                Destroy(instance.tempPanVCam);
                instance.tempPanVCam = null;
            }

            // Clean up any remaining pan targets
            GameObject panTarget = GameObject.Find("StageIntroPanTarget");
            if (panTarget != null)
            {
                Destroy(panTarget);
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

            Debug.Log("[StageIntroCameraPan] Intro skipped!");
        }
    }

    /// <summary>
    /// Check if intro is currently playing
    /// </summary>
    public static bool IsPlayingIntro()
    {
        return instance != null && instance.isPanning;
    }

    /// <summary>
    /// Manually trigger intro (useful for testing)
    /// </summary>
    public static void PlayIntro()
    {
        if (instance != null)
        {
            instance.hasPlayedIntro = false;
            instance.StartCoroutine(instance.PlayStageIntro());
        }
    }

    private void Update()
    {
        // Allow player to skip intro by pressing any key (using new Input System)
        if (isPanning)
        {
            // Check for any keyboard input
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            {
                SkipIntro();
                return;
            }

            // Check for mouse input
            if (Mouse.current != null &&
                (Mouse.current.leftButton.wasPressedThisFrame ||
                 Mouse.current.rightButton.wasPressedThisFrame))
            {
                SkipIntro();
                return;
            }
        }
    }
}
