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

    // Restart 버튼에서 호출
    public void Restart()
    {
        Time.timeScale = 1f; // 시간 복원

        // 일시정지 상태 해제
        isPaused = false;
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(false);
        }

        // 씬 재시작
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("Restarting scene...");
    }

    // Main Menu 버튼에서 호출
    public void QuitToMainMenu()
    {
        Time.timeScale = 1f; // 시간 복원

        // 메인 메뉴 씬 이름을 자동으로 찾거나 "StartScene"을 로드
        string mainMenuScene = "StartScene"; // 필요시 변경

        if (Application.CanStreamedLevelBeLoaded(mainMenuScene))
        {
            SceneManager.LoadScene(mainMenuScene);
            Debug.Log($"Loading {mainMenuScene}...");
        }
        else
        {
            Debug.LogWarning($"Scene '{mainMenuScene}' not found! Add it to Build Settings.");
        }
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
