using UnityEngine;

/// <summary>
/// Marks the spawn point for the player in a scene
/// 씬에서 플레이어 스폰 위치 표시
/// </summary>
public class PlayerSpawnPoint : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private bool movePlayerOnSceneLoad = true; // 씬 로드 시 플레이어 이동
    [SerializeField] private Vector3 spawnOffset = Vector3.zero; // 스폰 위치 오프셋

    private static PlayerSpawnPoint instance;

    public static PlayerSpawnPoint Instance => instance;

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

        Debug.Log($"[PlayerSpawnPoint] Spawn point set at {transform.position}");
    }

    private void Start()
    {
        if (movePlayerOnSceneLoad)
        {
            MovePlayerToSpawnPoint();
        }
    }

    /// <summary>
    /// Move player to this spawn point
    /// </summary>
    public void MovePlayerToSpawnPoint()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 spawnPosition = transform.position + spawnOffset;
            player.transform.position = spawnPosition;
            Debug.Log($"[PlayerSpawnPoint] Player moved to spawn point: {spawnPosition}");

            // Reset player velocity
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            Debug.LogWarning("[PlayerSpawnPoint] Player not found in scene!");
        }
    }

    /// <summary>
    /// Get spawn position
    /// </summary>
    public Vector3 GetSpawnPosition()
    {
        return transform.position + spawnOffset;
    }

    /// <summary>
    /// Static method to move player to spawn point from anywhere
    /// </summary>
    public static void MovePlayerToSpawn()
    {
        if (instance != null)
        {
            instance.MovePlayerToSpawnPoint();
        }
        else
        {
            Debug.LogWarning("[PlayerSpawnPoint] No spawn point found in scene!");
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    // Draw gizmo in editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + spawnOffset, 0.5f);
        Gizmos.DrawLine(transform.position + spawnOffset + Vector3.up * 0.5f,
                        transform.position + spawnOffset + Vector3.up * 1.5f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position + spawnOffset, 0.5f);
    }
}
