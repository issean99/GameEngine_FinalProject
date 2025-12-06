using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

/// <summary>
/// Start Scene 전용 플레이어 관리 스크립트
/// Tutorial Scene의 기존 Player를 찾아서 제거하고 Start Scene의 Player를 유지합니다.
/// </summary>
public class StartScenePlayerManager : MonoBehaviour
{
    private static StartScenePlayerManager instance;
    private static GameObject persistentPlayer;
    private static GameObject persistentCameraSystem; // Camera System GameObject 유지 (Main Camera + Cinemachine Camera 포함)

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    [Header("Camera System Settings")]
    [SerializeField] private string cameraSystemName = "Camera System"; // Camera System GameObject 이름

    private void Awake()
    {
        // Singleton 패턴
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // 씬 로드 이벤트 구독
            SceneManager.sceneLoaded += OnSceneLoaded;

            if (showDebugLogs)
                Debug.Log("[StartScenePlayerManager] Manager initialized and will persist across scenes");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Start Scene의 Player를 등록하여 씬 전환 후에도 유지
    /// </summary>
    public static void RegisterPlayer(GameObject player)
    {
        if (player == null)
        {
            Debug.LogError("[StartScenePlayerManager] Cannot register NULL player!");
            return;
        }

        persistentPlayer = player;

        // 부모에서 분리
        if (player.transform.parent != null)
        {
            Debug.Log($"[StartScenePlayerManager] Detaching player from parent '{player.transform.parent.name}'");
            player.transform.SetParent(null);
        }

        // DontDestroyOnLoad 설정
        DontDestroyOnLoad(player);

        Debug.Log($"[StartScenePlayerManager] Player '{player.name}' registered and set to DontDestroyOnLoad");
        Debug.Log($"[StartScenePlayerManager] Player position: {player.transform.position}");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (showDebugLogs)
            Debug.Log($"[StartScenePlayerManager] Scene '{scene.name}' loaded. Checking for duplicates...");

        // 새 씬에서 Player 태그를 가진 모든 오브젝트 찾기
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");

        if (showDebugLogs)
            Debug.Log($"[StartScenePlayerManager] Found {allPlayers.Length} Player(s) in scene");

        foreach (GameObject player in allPlayers)
        {
            // persistentPlayer가 아닌 다른 Player는 모두 제거
            if (player != persistentPlayer)
            {
                Debug.Log($"[StartScenePlayerManager] Destroying duplicate player: '{player.name}' at position {player.transform.position}");
                Destroy(player);
            }
            else
            {
                if (showDebugLogs)
                    Debug.Log($"[StartScenePlayerManager] Keeping persistent player: '{player.name}'");
            }
        }

        // Tutorial 씬으로 전환 시 플레이어 위치 및 스케일 초기화
        if (scene.name == "Tutorial" && persistentPlayer != null)
        {
            // 위치 설정
            persistentPlayer.transform.position = new Vector3(30.25f, -41.17f, 0f);

            // 스케일 초기화
            persistentPlayer.transform.localScale = new Vector3(1f, 1f, 1f);

            Debug.Log($"[StartScenePlayerManager] Player position reset to: {persistentPlayer.transform.position}");
            Debug.Log($"[StartScenePlayerManager] Player scale reset to: {persistentPlayer.transform.localScale}");

            // PlayerController 다시 활성화 (플레이어가 조종할 수 있도록)
            PlayerController playerController = persistentPlayer.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = true;
                Debug.Log("[StartScenePlayerManager] PlayerController enabled - player can now control movement");
            }

            // Tutorial 씬에서 Camera System을 찾아서 유지
            RegisterCameraSystem();
        }

        // Tutorial 이후 씬에서는 중복 Camera System 제거
        if (scene.name != "Tutorial" && scene.name != "Start")
        {
            RemoveDuplicateCameraSystems();
        }

        // 최종 확인
        if (persistentPlayer != null)
        {
            Debug.Log($"[StartScenePlayerManager] ✓ Player survived scene transition! Position: {persistentPlayer.transform.position}");
        }
        else
        {
            Debug.LogError("[StartScenePlayerManager] ✗ Persistent player is NULL!");
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// 현재 등록된 플레이어 가져오기
    /// </summary>
    public static GameObject GetPersistentPlayer()
    {
        return persistentPlayer;
    }

    /// <summary>
    /// Tutorial 씬에서 Camera System GameObject를 찾아서 DontDestroyOnLoad로 등록
    /// </summary>
    private void RegisterCameraSystem()
    {
        if (persistentCameraSystem != null)
        {
            if (showDebugLogs)
                Debug.Log("[StartScenePlayerManager] Camera System already registered");
            return;
        }

        // Camera System GameObject 찾기
        GameObject cameraSystem = GameObject.Find(cameraSystemName);

        if (cameraSystem != null)
        {
            // Camera System을 DontDestroyOnLoad로 설정
            persistentCameraSystem = cameraSystem;
            DontDestroyOnLoad(persistentCameraSystem);

            Debug.Log($"[StartScenePlayerManager] ✓ Camera System '{cameraSystem.name}' registered and will persist!");

            // Camera System 내부의 Cinemachine Camera 찾아서 플레이어 추적 설정
            CinemachineCamera[] cinemachineCameras = cameraSystem.GetComponentsInChildren<CinemachineCamera>();

            if (cinemachineCameras.Length > 0)
            {
                var cam = cinemachineCameras[0];
                Debug.Log($"[StartScenePlayerManager] Found Cinemachine Camera in Camera System: '{cam.name}'");
                Debug.Log($"[StartScenePlayerManager] Persistent player: {(persistentPlayer != null ? persistentPlayer.name : "NULL")}");

                // 플레이어 추적 설정
                cam.Target.TrackingTarget = persistentPlayer.transform;
                cam.Follow = persistentPlayer.transform;

                Debug.Log($"[StartScenePlayerManager] ✓ Set TrackingTarget to: {cam.Target.TrackingTarget?.name}");
                Debug.Log($"[StartScenePlayerManager] ✓ Set Follow to: {cam.Follow?.name}");
            }
            else
            {
                Debug.LogWarning("[StartScenePlayerManager] No Cinemachine Camera found in Camera System!");
            }

            // Main Camera 확인
            Camera mainCam = cameraSystem.GetComponentInChildren<Camera>();
            if (mainCam != null)
            {
                Debug.Log($"[StartScenePlayerManager] ✓ Main Camera '{mainCam.name}' found in Camera System");
            }
            else
            {
                Debug.LogWarning("[StartScenePlayerManager] No Main Camera found in Camera System!");
            }
        }
        else
        {
            Debug.LogError($"[StartScenePlayerManager] Camera System GameObject '{cameraSystemName}' not found in Tutorial scene!");
        }
    }

    /// <summary>
    /// 새 씬의 중복 Camera System 제거
    /// </summary>
    private void RemoveDuplicateCameraSystems()
    {
        if (persistentCameraSystem == null)
            return;

        // 씬에 있는 모든 "Camera System" GameObject 찾기
        GameObject[] allCameraSystems = GameObject.FindGameObjectsWithTag("Untagged");

        foreach (GameObject obj in allCameraSystems)
        {
            // Camera System 이름을 가진 오브젝트 중 persistent가 아닌 것 제거
            if (obj.name == cameraSystemName && obj != persistentCameraSystem)
            {
                Debug.Log($"[StartScenePlayerManager] Destroying duplicate Camera System: '{obj.name}'");
                Destroy(obj);
            }
        }
    }
}
