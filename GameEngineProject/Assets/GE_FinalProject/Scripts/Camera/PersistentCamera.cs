using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Makes the main camera persist across scene transitions
/// 메인 카메라를 씬 전환 시에도 유지되게 만들기
/// </summary>
public class PersistentCamera : MonoBehaviour
{
    [Header("Camera Reset Settings")]
    [SerializeField] private Vector3 defaultPosition = new Vector3(0f, 0f, -10f);
    [SerializeField] private float defaultOrthographicSize = 5f;
    [SerializeField] private bool resetOnSceneChange = true;

    private static PersistentCamera instance;
    private Camera cam;
    private Vector3 originalPosition;
    private float originalSize;

    private void Awake()
    {
        // Singleton pattern - only one camera should persist
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[PersistentCamera] Camera will persist across scenes");

            cam = GetComponent<Camera>();
            if (cam != null)
            {
                originalPosition = cam.transform.position;
                originalSize = cam.orthographicSize;
            }

            // Subscribe to scene loaded event
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            // If another camera already exists, destroy this one
            Debug.Log("[PersistentCamera] Duplicate camera found, destroying this one");
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!resetOnSceneChange || cam == null) return;

        // Reset camera position and size when entering new scene
        Debug.Log($"[PersistentCamera] Resetting camera for scene: {scene.name}");

        cam.transform.position = defaultPosition;
        cam.orthographicSize = defaultOrthographicSize;

        // Also reset CameraFollowPlayer if it exists
        CameraFollowPlayer followScript = cam.GetComponent<CameraFollowPlayer>();
        if (followScript != null)
        {
            // Force it to find the player again
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                followScript.SetTarget(player.transform);
            }
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
