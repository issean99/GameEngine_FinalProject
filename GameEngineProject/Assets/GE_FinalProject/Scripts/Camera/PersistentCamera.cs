using UnityEngine;

/// <summary>
/// Keeps camera persistent across scene transitions
/// 씬 전환 시에도 카메라를 유지
/// </summary>
public class PersistentCamera : MonoBehaviour
{
    private static PersistentCamera instance;

    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[PersistentCamera] Camera will persist across scenes");
        }
        else
        {
            // Destroy duplicate camera from new scene
            Destroy(gameObject);
            Debug.Log("[PersistentCamera] Duplicate camera destroyed");
        }
    }
}
