using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartSceneManager : MonoBehaviour
{
    private static bool playerPersisted = false; // 플레이어가 이미 유지되었는지 확인
    [Header("UI Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;

    [Header("Player Settings")]
    [SerializeField] private GameObject player; // 플레이어 오브젝트
    [SerializeField] private Transform playerStartPosition; // 시작 위치 (아래)
    [SerializeField] private Transform castleGatePosition; // 목표 위치 (성문)
    [SerializeField] private float playerMoveSpeed = 3f; // 이동 속도
    [SerializeField] private bool keepPlayerAcrossScenes = true; // 씬 전환 시 플레이어 유지 여부

    [Header("Scene Settings")]
    [SerializeField] private string nextSceneName = "Tutorial"; // 다음 씬 이름
    [SerializeField] private float delayBeforeSceneLoad = 0.5f; // 성문 도착 후 씬 전환까지 대기 시간

    private bool isMoving = false;

    private void Start()
    {
        // StartScenePlayerManager가 있는지 확인, 없으면 생성
        if (FindObjectOfType<StartScenePlayerManager>() == null)
        {
            GameObject manager = new GameObject("StartScenePlayerManager");
            manager.AddComponent<StartScenePlayerManager>();
            Debug.Log("[StartSceneManager] StartScenePlayerManager created");
        }

        // 플레이어 초기 설정
        if (player != null)
        {
            // 플레이어를 시작 위치로 이동
            if (playerStartPosition != null)
            {
                player.transform.position = playerStartPosition.position;
            }

            // 플레이어 비활성화 (버튼 클릭 전까지 숨김)
            player.SetActive(false);
        }

        // Add listeners to buttons
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
        else
        {
            Debug.LogWarning("Start Button is not assigned in StartSceneManager!");
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitButtonClicked);
        }
        else
        {
            Debug.LogWarning("Exit Button is not assigned in StartSceneManager!");
        }
    }

    private void OnStartButtonClicked()
    {
        Debug.Log("Start button clicked!");

        // Hide both buttons
        HideButtons();

        // Start player movement
        StartCoroutine(MovePlayerToCastleGate());
    }

    private void OnExitButtonClicked()
    {
        Debug.Log("Exit button clicked!");

        // Quit the application
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void HideButtons()
    {
        if (startButton != null)
        {
            startButton.gameObject.SetActive(false);
        }

        if (exitButton != null)
        {
            exitButton.gameObject.SetActive(false);
        }

        Debug.Log("Start and Exit buttons hidden!");
    }

    private IEnumerator MovePlayerToCastleGate()
    {
        if (player == null)
        {
            Debug.LogError("[StartSceneManager] Player is not assigned!");
            SceneManager.LoadScene(nextSceneName);
            yield break;
        }

        if (castleGatePosition == null)
        {
            Debug.LogError("[StartSceneManager] Castle Gate Position is not assigned!");
            SceneManager.LoadScene(nextSceneName);
            yield break;
        }

        Debug.Log($"[StartSceneManager] Starting player movement. Player name: {player.name}");

        // 씬 전환 시 플레이어를 유지하도록 설정
        if (keepPlayerAcrossScenes)
        {
            if (!playerPersisted)
            {
                // StartScenePlayerManager에 플레이어 등록
                StartScenePlayerManager.RegisterPlayer(player);
                playerPersisted = true;
            }
            else
            {
                Debug.LogWarning("[StartSceneManager] Player already persisted!");
            }
        }
        else
        {
            Debug.LogWarning("[StartSceneManager] keepPlayerAcrossScenes is FALSE! Player will be destroyed!");
        }

        // 플레이어 활성화
        player.SetActive(true);
        Debug.Log($"[StartSceneManager] Player activated: {player.activeSelf}");

        // PlayerController 비활성화 (플레이어가 조종할 수 없도록)
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
            Debug.Log("[StartSceneManager] PlayerController disabled - player cannot control movement");
        }

        isMoving = true;

        // 플레이어를 성문까지 이동
        while (isMoving)
        {
            // 현재 위치에서 성문까지의 거리 계산
            float distance = Vector3.Distance(player.transform.position, castleGatePosition.position);

            // 성문에 도착했는지 확인 (거리가 0.1 이하면 도착)
            if (distance < 0.1f)
            {
                isMoving = false;
                player.transform.position = castleGatePosition.position;
                Debug.Log("Player reached the castle gate!");
                break;
            }

            // 플레이어를 성문 방향으로 이동
            player.transform.position = Vector3.MoveTowards(
                player.transform.position,
                castleGatePosition.position,
                playerMoveSpeed * Time.deltaTime
            );

            // 플레이어 애니메이션 업데이트 (있다면)
            Animator animator = player.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetFloat("MoveSpeed", 1f); // 걷는 애니메이션
            }

            yield return null;
        }

        // 애니메이션 정지
        Animator finalAnimator = player.GetComponent<Animator>();
        if (finalAnimator != null)
        {
            finalAnimator.SetFloat("MoveSpeed", 0f);
        }

        // 잠시 대기 후 씬 전환
        yield return new WaitForSeconds(delayBeforeSceneLoad);

        // 다음 씬 로드 전 최종 확인
        Debug.Log($"[StartSceneManager] About to load scene: {nextSceneName}");
        Debug.Log($"[StartSceneManager] Player still exists: {(player != null)}");
        Debug.Log($"[StartSceneManager] Player active: {(player != null && player.activeSelf)}");
        string playerPos = player != null ? player.transform.position.ToString() : "NULL";
        Debug.Log($"[StartSceneManager] Player position: {playerPos}");

        if (keepPlayerAcrossScenes)
        {
            // Additive 모드로 새 씬 로드 (기존 오브젝트 유지)
            Debug.Log($"[StartSceneManager] Loading scene '{nextSceneName}' in ADDITIVE mode...");
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);

            // 씬 로드 완료 대기
            yield return asyncLoad;

            Debug.Log($"[StartSceneManager] Scene '{nextSceneName}' loaded!");

            // 새 씬을 활성 씬으로 설정
            Scene newScene = SceneManager.GetSceneByName(nextSceneName);
            if (newScene.IsValid())
            {
                SceneManager.SetActiveScene(newScene);
                Debug.Log($"[StartSceneManager] Active scene set to: {newScene.name}");
            }

            // Start Scene 언로드
            Scene currentScene = SceneManager.GetSceneByName("Start");
            if (currentScene.IsValid())
            {
                Debug.Log($"[StartSceneManager] Unloading Start scene...");
                SceneManager.UnloadSceneAsync(currentScene);
            }

            // 플레이어 확인
            if (player != null)
            {
                Debug.Log($"[StartSceneManager] SUCCESS! Player still alive! Position: {player.transform.position}");
            }
            else
            {
                Debug.LogError("[StartSceneManager] FAILED! Player is NULL after scene load!");
            }
        }
        else
        {
            // 기존 방식 (Single 모드) - 플레이어 파괴됨
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
