using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Game Over UI - shown when player dies
/// 게임오버 UI - 플레이어 사망 시 표시
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel; // 게임오버 패널
    [SerializeField] private Button respawnButton; // 리스폰 버튼
    [SerializeField] private Button backButton; // 시작 화면으로 돌아가기 버튼
    [SerializeField] private TextMeshProUGUI gameOverText; // "Game Over" 텍스트
    [SerializeField] private TextMeshProUGUI respawnCountText; // 남은 리스폰 횟수 텍스트

    [Header("Settings")]
    [SerializeField] private string startSceneName = "Start"; // 시작 씬 이름
    [SerializeField] private bool pauseGameOnDeath = true; // 사망 시 게임 일시정지

    private static GameOverUI instance;

    public static GameOverUI Instance => instance;

    private void Awake()
    {
        // Singleton
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Hide panel on start
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Setup button listeners
        if (respawnButton != null)
        {
            respawnButton.onClick.AddListener(OnRespawnClicked);
            Debug.Log("[GameOverUI] Respawn button listener added");
        }
        else
        {
            Debug.LogError("[GameOverUI] Respawn button is not assigned!");
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
            Debug.Log("[GameOverUI] Back button listener added");
        }
        else
        {
            Debug.LogError("[GameOverUI] Back button is not assigned!");
        }
    }

    /// <summary>
    /// Show game over screen
    /// </summary>
    public void ShowGameOver()
    {
        Debug.Log("[GameOverUI] ShowGameOver called!");

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log("[GameOverUI] Game Over panel activated");
        }
        else
        {
            Debug.LogError("[GameOverUI] Game Over panel is null!");
        }

        // Update respawn count and button state
        UpdateRespawnUI();

        // Pause game
        if (pauseGameOnDeath)
        {
            Time.timeScale = 0f;
            Debug.Log("[GameOverUI] Game paused (Time.timeScale = 0)");
        }

        // Disable player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = false;
                Debug.Log("[GameOverUI] Player controller disabled");
            }
        }

        Debug.Log("[GameOverUI] Game Over screen shown");
    }

    /// <summary>
    /// Update respawn count text and button state
    /// </summary>
    private void UpdateRespawnUI()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                int remainingRespawns = playerController.RemainingRespawns;

                // Update respawn count text
                if (respawnCountText != null)
                {
                    respawnCountText.text = $"RemainRespawn : {remainingRespawns}";
                    Debug.Log($"[GameOverUI] Respawn count updated: {remainingRespawns}");
                }

                // Show/hide respawn button based on remaining count
                if (respawnButton != null)
                {
                    if (remainingRespawns > 0)
                    {
                        respawnButton.gameObject.SetActive(true);
                        respawnButton.interactable = true;
                        Debug.Log("[GameOverUI] Respawn button shown and enabled");
                    }
                    else
                    {
                        respawnButton.gameObject.SetActive(false);
                        Debug.Log("[GameOverUI] Respawn button hidden - no respawns remaining");

                        // Update text to show no respawns left
                        if (respawnCountText != null)
                        {
                            respawnCountText.text = "RemainRespawn : 0";
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Hide game over screen
    /// </summary>
    public void HideGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Resume game
        Time.timeScale = 1f;

        Debug.Log("[GameOverUI] Game Over screen hidden");
    }

    /// <summary>
    /// Respawn - reload current scene with full health
    /// </summary>
    private void OnRespawnClicked()
    {
        Debug.Log("[GameOverUI] Respawn clicked - reloading current scene");

        // Resume time before loading scene
        Time.timeScale = 1f;

        // IMMEDIATELY grant invincibility before fade starts
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // Make player invincible immediately (prevent death during fade)
                playerController.SetInvincible(99f); // Very long duration, will be reset properly after respawn
                playerController.enabled = true; // Re-enable to allow invincibility to work
                Debug.Log("[GameOverUI] Player made invincible before fade");
            }
        }

        // Get current scene name
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Check if this is a boss scene or stage scene (both need full reload)
        bool isBossScene = currentSceneName.ToLower().Contains("boss");
        bool isStageScene = currentSceneName.ToLower().Contains("stage");
        bool needsFullReload = isBossScene || isStageScene;

        if (needsFullReload)
        {
            // Boss/Stage scene: Always reload to reset everything (boss, enemies, dialogue)
            Debug.Log($"[GameOverUI] {(isBossScene ? "Boss" : "Stage")} scene detected - fully reloading scene with fade");

            // Find and restore player health before reload
            if (player != null)
            {
                PlayerController playerController = player.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.ResetPlayerState();
                }
            }

            // Reload scene with fade effect
            SceneTransitionManager.ReloadCurrentSceneWithFade();
        }
        else
        {
            // Normal scene: Just reset player position and health with fade
            Debug.Log("[GameOverUI] Normal scene - resetting player only with fade");

            // Use fade transition for respawn
            SceneTransitionManager.RespawnWithFade(() =>
            {
                // This callback is executed during the black screen

                // Hide game over panel
                HideGameOver();

                // Find player and restore health
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    PlayerController playerController = player.GetComponent<PlayerController>();
                    if (playerController != null)
                    {
                        // Reset player state (health, status effects, etc.)
                        playerController.ResetPlayerState();
                        Debug.Log("[GameOverUI] Player health restored before respawn");
                    }

                    // Remove duplicate Audio Listener from player (Main Camera will have one)
                    AudioListener playerListener = player.GetComponent<AudioListener>();
                    if (playerListener != null)
                    {
                        Destroy(playerListener);
                        Debug.Log("[GameOverUI] Removed duplicate Audio Listener from player");
                    }

                    // Move player to spawn point if it exists
                    PlayerSpawnPoint spawnPoint = PlayerSpawnPoint.Instance;
                    if (spawnPoint != null)
                    {
                        player.transform.position = spawnPoint.GetSpawnPosition();
                        Debug.Log($"[GameOverUI] Player moved to spawn point: {spawnPoint.GetSpawnPosition()}");
                    }
                    else
                    {
                        // No spawn point found, reload scene with fade
                        Debug.Log("[GameOverUI] No spawn point found, reloading scene");
                        SceneTransitionManager.ReloadCurrentSceneWithFade();
                    }
                }
                else
                {
                    // If no player found, reload scene with fade
                    Debug.Log("[GameOverUI] No player found, reloading scene");
                    SceneTransitionManager.ReloadCurrentSceneWithFade();
                }
            });
        }
    }

    /// <summary>
    /// Back to start menu - completely reset game state
    /// </summary>
    private void OnBackClicked()
    {
        Debug.Log("[GameOverUI] Back clicked - completely resetting game and returning to start menu");

        // Resume time before loading scene
        Time.timeScale = 1f;

        // Destroy ALL persistent objects to completely reset game state
        DestroyAllPersistentObjects();

        // Load start scene
        SceneManager.LoadScene(startSceneName);
    }

    /// <summary>
    /// Destroy all DontDestroyOnLoad objects to reset game completely
    /// </summary>
    private void DestroyAllPersistentObjects()
    {
        Debug.Log("[GameOverUI] Starting complete game reset - destroying ALL persistent objects...");

        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        // 1. Destroy ALL players (persistent and scene-based)
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            Debug.Log($"[GameOverUI] Destroying player: {player.name}");
            Destroy(player);
        }

        // 2. Destroy ALL PlayerHealthUI (HP bars)
        PlayerHealthUI[] healthUIs = FindObjectsOfType<PlayerHealthUI>();
        foreach (PlayerHealthUI healthUI in healthUIs)
        {
            Debug.Log($"[GameOverUI] Destroying PlayerHealthUI: {healthUI.gameObject.name}");
            Destroy(healthUI.gameObject);
        }

        // 3. Destroy StartScenePlayerManager
        StartScenePlayerManager[] playerManagers = FindObjectsOfType<StartScenePlayerManager>();
        foreach (StartScenePlayerManager manager in playerManagers)
        {
            Debug.Log($"[GameOverUI] Destroying StartScenePlayerManager: {manager.gameObject.name}");
            Destroy(manager.gameObject);
        }

        // 4. Destroy ALL Camera Systems (persistent and scene-based)
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Camera System") || obj.name.Contains("CameraSystem"))
            {
                Debug.Log($"[GameOverUI] Destroying camera system: {obj.name}");
                Destroy(obj);
            }
        }

        // 5. Destroy ALL UI Canvases that came from other scenes
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in allCanvases)
        {
            // Don't destroy canvases from Start scene
            if (canvas.gameObject.scene.name != "Start" && canvas.gameObject.scene.name != null)
            {
                Debug.Log($"[GameOverUI] Destroying canvas from other scene: {canvas.gameObject.name}");
                Destroy(canvas.gameObject);
            }
        }

        // 6. Find and destroy all DontDestroyOnLoad objects
        // This catches everything that was set to persist across scenes
        foreach (GameObject obj in allObjects)
        {
            // DontDestroyOnLoad objects have no scene (scene.name is null)
            if (obj.scene.name == null && obj.transform.parent == null)
            {
                // Keep only essential UI/audio managers
                bool isEssential = obj.GetComponent<BGMManager>() != null ||
                                  obj.GetComponent<SceneTransitionManager>() != null ||
                                  obj == gameObject; // Don't destroy GameOverUI itself

                if (!isEssential)
                {
                    Debug.Log($"[GameOverUI] Destroying DontDestroyOnLoad object: {obj.name}");
                    Destroy(obj);
                }
                else
                {
                    Debug.Log($"[GameOverUI] Keeping essential manager: {obj.name}");
                }
            }
        }

        // 7. Destroy SceneTransitionManager's TransitionCanvas (child objects)
        // This will be recreated automatically when needed
        GameObject[] allDontDestroyObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allDontDestroyObjects)
        {
            if (obj.scene.name == null && obj.name == "TransitionCanvas")
            {
                Debug.Log($"[GameOverUI] Destroying TransitionCanvas (will be recreated): {obj.name}");
                Destroy(obj);
            }
        }

        // 8. Reset static references in StartScenePlayerManager
        // This ensures the next game start is completely fresh
        Debug.Log("[GameOverUI] Clearing all static references");
        StartScenePlayerManager.ResetAllStaticReferences();
        StartSceneManager.ResetPlayerPersistedFlag();
        PlayerController.ResetRespawnStatics(); // Reset respawn count statics

        Debug.Log("[GameOverUI] ✓ Complete game reset finished - all game data destroyed");
        Debug.Log("[GameOverUI] Game will start from scratch in Start scene with original objects only");
        Debug.Log("[GameOverUI] Next Start → Tutorial transition will work normally with fresh player");
    }

    /// <summary>
    /// Static method to show game over from anywhere
    /// </summary>
    public static void ShowGameOverScreen()
    {
        if (instance != null)
        {
            instance.ShowGameOver();
        }
        else
        {
            Debug.LogWarning("[GameOverUI] No GameOverUI instance found in scene!");
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }

        // Cleanup button listeners
        if (respawnButton != null)
        {
            respawnButton.onClick.RemoveListener(OnRespawnClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackClicked);
        }
    }
}
