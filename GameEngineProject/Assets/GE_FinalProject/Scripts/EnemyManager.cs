using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 씬의 모든 적을 추적하고 관리하는 매니저
/// Tracks and manages all enemies in the scene
/// </summary>
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [Header("Enemy Tracking")]
    [SerializeField] private bool trackEnemiesAutomatically = true; // 씬 로드 시 자동으로 적 추적

    private HashSet<GameObject> enemies = new HashSet<GameObject>();
    private int totalEnemyCount = 0;
    private int defeatedEnemyCount = 0;

    // Public properties
    public int TotalEnemyCount => totalEnemyCount;
    public int DefeatedEnemyCount => defeatedEnemyCount;
    public int RemainingEnemyCount => totalEnemyCount - defeatedEnemyCount;
    public bool AllEnemiesDefeated => RemainingEnemyCount <= 0 && totalEnemyCount > 0;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (trackEnemiesAutomatically)
        {
            FindAndRegisterAllEnemies();
        }
    }

    /// <summary>
    /// 씬의 모든 적을 찾아서 등록
    /// Find and register all enemies in the scene
    /// </summary>
    public void FindAndRegisterAllEnemies()
    {
        enemies.Clear();
        defeatedEnemyCount = 0;

        // Find all GameObjects with "Enemy" tag
        GameObject[] foundEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in foundEnemies)
        {
            RegisterEnemy(enemy);
        }

        totalEnemyCount = enemies.Count;

        Debug.Log($"[EnemyManager] Found and registered {totalEnemyCount} enemies in the scene");
    }

    /// <summary>
    /// 적을 매니저에 등록
    /// Register an enemy with the manager
    /// </summary>
    public void RegisterEnemy(GameObject enemy)
    {
        if (enemy == null) return;

        if (enemies.Add(enemy))
        {
            Debug.Log($"[EnemyManager] Registered enemy: {enemy.name}");
        }
    }

    /// <summary>
    /// 적이 죽었을 때 호출 (적 스크립트에서 호출)
    /// Called when an enemy is defeated (called from enemy scripts)
    /// </summary>
    public void OnEnemyDefeated(GameObject enemy)
    {
        if (enemies.Contains(enemy))
        {
            defeatedEnemyCount++;
            Debug.Log($"[EnemyManager] Enemy defeated: {enemy.name} ({defeatedEnemyCount}/{totalEnemyCount})");

            if (AllEnemiesDefeated)
            {
                Debug.Log("[EnemyManager] ✅ All enemies defeated! Portal is now accessible.");
            }
        }
    }

    /// <summary>
    /// 수동으로 적 제거 (씬에서 적이 삭제될 때)
    /// Manually remove an enemy (when enemy is destroyed from scene)
    /// </summary>
    public void UnregisterEnemy(GameObject enemy)
    {
        if (enemies.Remove(enemy))
        {
            Debug.Log($"[EnemyManager] Unregistered enemy: {enemy.name}");
        }
    }

    /// <summary>
    /// 매니저 리셋 (씬 전환 시)
    /// Reset the manager (when changing scenes)
    /// </summary>
    public void Reset()
    {
        enemies.Clear();
        totalEnemyCount = 0;
        defeatedEnemyCount = 0;
        Debug.Log("[EnemyManager] Reset");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
