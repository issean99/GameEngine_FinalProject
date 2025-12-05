using UnityEngine;

/// <summary>
/// Keeps player persistent across scene transitions
/// 씬 전환 시에도 플레이어를 유지
/// </summary>
public class PersistentPlayer : MonoBehaviour
{
    private static PersistentPlayer instance;

    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[PersistentPlayer] Player will persist across scenes");
        }
        else
        {
            // Destroy duplicate player from new scene
            Destroy(gameObject);
            Debug.Log("[PersistentPlayer] Duplicate player destroyed");
        }
    }
}
