using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance { get; private set; }

    [Header("Pause Menu Settings")]
    [SerializeField] private GameObject pauseMenuCanvas;

    private bool isPaused = false;
    private Keyboard keyboard;

    private void Awake()
    {
        // Singleton 패턴: 이미 인스턴스가 있으면 파괴
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 씬 전환 시에도 유지
        DontDestroyOnLoad(gameObject);

        keyboard = Keyboard.current;

        // 시작 시 메뉴 숨기기
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(false);

            // Canvas도 씬 전환 시 유지
            DontDestroyOnLoad(pauseMenuCanvas);
        }
    }

    private void Update()
    {
        if (keyboard == null)
        {
            Debug.LogWarning("Keyboard not detected!");
            return;
        }

        // ESC 키 입력 확인
        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(isPaused);
        }

        // 게임 시간 정지/재개
        Time.timeScale = isPaused ? 0f : 1f;

        Debug.Log(isPaused ? "Game Paused" : "Game Resumed");
    }

    // Resume 버튼에서 호출
    public void Resume()
    {
        if (isPaused)
        {
            TogglePause();
        }
    }

    // Back to Menu 버튼에서 호출 - 게임 완전 초기화 후 시작 화면으로
    public void BackToMenu()
    {
        Debug.Log("[PauseMenuManager] Back to Menu clicked - completely resetting game");

        Time.timeScale = 1f; // 시간 복원

        // 일시정지 상태 해제
        isPaused = false;
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(false);
        }

        // Destroy ALL persistent objects to completely reset game state
        DestroyAllPersistentObjects();

        // Load Start scene
        SceneManager.LoadScene("Start");
    }

    /// <summary>
    /// Destroy all DontDestroyOnLoad objects to reset game completely
    /// </summary>
    private void DestroyAllPersistentObjects()
    {
        Debug.Log("[PauseMenuManager] Starting complete game reset - destroying ALL persistent objects...");

        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        // 1. Destroy ALL players (persistent and scene-based)
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            Debug.Log($"[PauseMenuManager] Destroying player: {player.name}");
            Destroy(player);
        }

        // 2. Destroy ALL PlayerHealthUI (HP bars)
        PlayerHealthUI[] healthUIs = FindObjectsOfType<PlayerHealthUI>();
        foreach (PlayerHealthUI healthUI in healthUIs)
        {
            Debug.Log($"[PauseMenuManager] Destroying PlayerHealthUI: {healthUI.gameObject.name}");
            Destroy(healthUI.gameObject);
        }

        // 3. Destroy StartScenePlayerManager
        StartScenePlayerManager[] playerManagers = FindObjectsOfType<StartScenePlayerManager>();
        foreach (StartScenePlayerManager manager in playerManagers)
        {
            Debug.Log($"[PauseMenuManager] Destroying StartScenePlayerManager: {manager.gameObject.name}");
            Destroy(manager.gameObject);
        }

        // 4. Destroy ALL Camera Systems (persistent and scene-based)
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Camera System") || obj.name.Contains("CameraSystem"))
            {
                Debug.Log($"[PauseMenuManager] Destroying camera system: {obj.name}");
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
                Debug.Log($"[PauseMenuManager] Destroying canvas from other scene: {canvas.gameObject.name}");
                Destroy(canvas.gameObject);
            }
        }

        // 6. Find and destroy all DontDestroyOnLoad objects
        foreach (GameObject obj in allObjects)
        {
            // DontDestroyOnLoad objects have no scene (scene.name is null)
            if (obj.scene.name == null && obj.transform.parent == null)
            {
                // Keep only essential UI/audio managers
                bool isEssential = obj.GetComponent<BGMManager>() != null ||
                                  obj.GetComponent<SceneTransitionManager>() != null ||
                                  obj == gameObject; // Don't destroy PauseMenuManager itself yet

                if (!isEssential)
                {
                    Debug.Log($"[PauseMenuManager] Destroying DontDestroyOnLoad object: {obj.name}");
                    Destroy(obj);
                }
                else
                {
                    Debug.Log($"[PauseMenuManager] Keeping essential manager: {obj.name}");
                }
            }
        }

        // 7. Reset static references
        Debug.Log("[PauseMenuManager] Clearing all static references");
        StartScenePlayerManager.ResetAllStaticReferences();
        StartSceneManager.ResetPlayerPersistedFlag();
        PlayerController.ResetRespawnStatics(); // Reset respawn count statics

        Debug.Log("[PauseMenuManager] ✓ Complete game reset finished");
    }

    // Quit Game 버튼에서 호출
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Time.timeScale = 1f; // 시간 복원

        #if UNITY_EDITOR
        // 에디터에서 실행 중지
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // 빌드된 게임 종료
        Application.Quit();
        #endif
    }

    // 게임이 종료될 때 Time.timeScale 복원
    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
