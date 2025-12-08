using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// Portal that allows player to transition to next scene by pressing T key
/// T키를 눌러 다음 씬으로 전환할 수 있는 포탈
/// </summary>
public class ScenePortal : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string nextSceneName; // Name of the scene to load
    [Tooltip("Or use scene build index instead of name")]
    [SerializeField] private int nextSceneBuildIndex = -1; // -1 means use scene name instead
    [SerializeField] private bool isBossScene = false; // Boss 씬으로 가는 포탈인지 여부

    [Header("Player Spawn Settings")]
    [SerializeField] private Vector2 spawnPosition = Vector2.zero; // Where player spawns in next scene
    [Tooltip("Leave as (0,0) to keep player at current position")]
    [SerializeField] private bool useSpawnPosition = true; // Whether to move player to spawn position

    [Header("Portal Settings")]
    [SerializeField] private bool requirePlayerInRange = true; // Require player to be near portal
    [SerializeField] private float interactionRange = 2f; // Distance player needs to be from portal

    [Header("UI Feedback (Optional)")]
    [SerializeField] private GameObject promptUI; // UI prompt to show when player is in range (e.g., "Press T to Enter")
    [SerializeField] private SpriteRenderer portalSprite; // Portal sprite renderer for visual feedback
    [SerializeField] private Color highlightColor = Color.cyan; // Color when player is in range

    private bool playerInRange = false;
    private GameObject player;
    private Color originalColor;
    private Keyboard keyboard;

    private void Awake()
    {
        keyboard = Keyboard.current;

        // Store original color
        if (portalSprite != null)
        {
            originalColor = portalSprite.color;
        }

        // Hide prompt initially
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
    }

    private void Start()
    {
        // Find player
        player = GameObject.FindGameObjectWithTag("Player");

        // Validate scene settings
        if (nextSceneBuildIndex == -1 && string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError($"[ScenePortal] No scene specified! Set either nextSceneName or nextSceneBuildIndex on {gameObject.name}");
        }
    }

    private void Update()
    {
        if (keyboard == null)
        {
            keyboard = Keyboard.current;
            if (keyboard == null) return;
        }

        // Check if player is in range
        if (requirePlayerInRange && player != null)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            bool wasInRange = playerInRange;
            playerInRange = distance <= interactionRange;

            // Toggle visual feedback when player enters/exits range
            if (playerInRange != wasInRange)
            {
                UpdateVisualFeedback(playerInRange);
            }
        }
        else
        {
            // If no range requirement, always allow interaction
            playerInRange = true;
        }

        // Check for T key press
        if (keyboard.tKey.wasPressedThisFrame)
        {
            if (!requirePlayerInRange || playerInRange)
            {
                TransitionToNextScene();
            }
            else
            {
                Debug.Log("[ScenePortal] Player is too far from portal!");
            }
        }
    }

    private void UpdateVisualFeedback(bool inRange)
    {
        // Show/hide prompt UI
        if (promptUI != null)
        {
            promptUI.SetActive(inRange);
        }

        // Change portal color
        if (portalSprite != null)
        {
            portalSprite.color = inRange ? highlightColor : originalColor;
        }

        if (inRange)
        {
            Debug.Log($"[ScenePortal] Player entered portal range. Press T to enter.");
        }
    }

    private void TransitionToNextScene()
    {
        // Move player to spawn position after scene loads
        if (useSpawnPosition)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        // Determine which scene to load
        string sceneToLoad = "";

        if (nextSceneBuildIndex >= 0)
        {
            // Use build index - convert to scene name
            if (nextSceneBuildIndex < SceneManager.sceneCountInBuildSettings)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(nextSceneBuildIndex);
                sceneToLoad = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            }
            else
            {
                Debug.LogError($"[ScenePortal] Scene build index {nextSceneBuildIndex} is out of range! Total scenes: {SceneManager.sceneCountInBuildSettings}");
                return;
            }
        }
        else if (!string.IsNullOrEmpty(nextSceneName))
        {
            // Use scene name
            sceneToLoad = nextSceneName;
        }
        else
        {
            Debug.LogError("[ScenePortal] No valid scene specified!");
            return;
        }

        // Load scene with appropriate transition effect
        Debug.Log($"[ScenePortal] Loading scene: {sceneToLoad}, isBossScene: {isBossScene}");

        if (isBossScene)
        {
            // Boss scene - use Flash transition
            SceneTransitionManager.LoadSceneWithFlash(sceneToLoad);
        }
        else
        {
            // Normal stage - use Fade transition
            SceneTransitionManager.LoadSceneWithFade(sceneToLoad);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Unsubscribe from event
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Find player and move to spawn position
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = spawnPosition;
            Debug.Log($"[ScenePortal] Player moved to spawn position: {spawnPosition}");
        }
        else
        {
            Debug.LogWarning("[ScenePortal] Player not found after scene load!");
        }
    }

    // Draw interaction range and spawn position in Scene view
    private void OnDrawGizmosSelected()
    {
        if (requirePlayerInRange)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }

        // Draw spawn position marker
        if (useSpawnPosition)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnPosition, 0.5f);
            Gizmos.DrawLine(spawnPosition + Vector2.up * 0.5f, spawnPosition + Vector2.up * 1.5f);
        }
    }

    // Alternative: Use trigger collider instead of distance check
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            UpdateVisualFeedback(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            UpdateVisualFeedback(false);
        }
    }
}
