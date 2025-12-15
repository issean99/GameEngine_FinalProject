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
    public int RemainingEnemyCount => GetActualRemainingEnemyCount();
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

    private void Update()
    {
        // Check for debug key press (K key) to show enemy status
        if (UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.kKey.wasPressedThisFrame)
        {
            ShowEnemyStatus();
        }
    }

    /// <summary>
    /// 실제로 살아있는 적의 수를 계산 (보스의 경우 HP가 0이면 죽은 것으로 간주)
    /// Calculate actual remaining enemy count (bosses with HP <= 0 are considered dead)
    /// </summary>
    private int GetActualRemainingEnemyCount()
    {
        int aliveCount = 0;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null)
            {
                // GameObject가 파괴됨 - 죽은 것으로 간주
                continue;
            }

            bool isAlive = true;

            // Check boss types - they don't immediately destroy, so check health
            WizardBoss wizardBoss = enemy.GetComponent<WizardBoss>();
            if (wizardBoss != null)
            {
                isAlive = wizardBoss.CurrentHealth > 0;
                if (isAlive) aliveCount++;
                continue;
            }

            FinalBoss finalBoss = enemy.GetComponent<FinalBoss>();
            if (finalBoss != null)
            {
                isAlive = finalBoss.CurrentHealth > 0;
                if (isAlive) aliveCount++;
                continue;
            }

            // For regular enemies, check if they're dead (not staggered)
            SkeletonController skeleton = enemy.GetComponent<SkeletonController>();
            if (skeleton != null)
            {
                isAlive = !skeleton.IsDead();
                if (isAlive) aliveCount++;
                continue;
            }

            SlimeController slime = enemy.GetComponent<SlimeController>();
            if (slime != null)
            {
                isAlive = !slime.IsDead();
                if (isAlive) aliveCount++;
                continue;
            }

            WereWolfController werewolf = enemy.GetComponent<WereWolfController>();
            if (werewolf != null)
            {
                isAlive = !werewolf.IsDead();
                if (isAlive) aliveCount++;
                continue;
            }

            SkeletonArcherController skeletonArcher = enemy.GetComponent<SkeletonArcherController>();
            if (skeletonArcher != null)
            {
                isAlive = !skeletonArcher.IsDead();
                if (isAlive) aliveCount++;
                continue;
            }
        }

        return aliveCount;
    }

    /// <summary>
    /// 현재 적의 상태를 콘솔에 표시
    /// Display current enemy status in console
    /// </summary>
    public void ShowEnemyStatus()
    {
        Debug.Log("═══════════════════════════════════════");
        Debug.Log($"[EnemyManager] Enemy Status Report:");
        Debug.Log($"  Total Enemies: {totalEnemyCount}");
        Debug.Log($"  Defeated: {defeatedEnemyCount}");
        Debug.Log($"  Remaining: {RemainingEnemyCount}");
        Debug.Log($"  All Defeated: {AllEnemiesDefeated}");

        if (RemainingEnemyCount > 0)
        {
            Debug.Log($"  Registered Enemies:");
            int index = 1;
            foreach (GameObject enemy in enemies)
            {
                if (enemy != null)
                {
                    // Check if enemy is alive
                    bool isAlive = true;

                    // Check various enemy types
                    SkeletonController skeleton = enemy.GetComponent<SkeletonController>();
                    if (skeleton != null)
                    {
                        isAlive = !skeleton.IsDead();
                        Debug.Log($"    {index}. {enemy.name} (Skeleton) - {(isAlive ? "ALIVE" : "DEAD")}");
                    }

                    SlimeController slime = enemy.GetComponent<SlimeController>();
                    if (slime != null)
                    {
                        isAlive = !slime.IsDead();
                        Debug.Log($"    {index}. {enemy.name} (Slime) - {(isAlive ? "ALIVE" : "DEAD")}");
                    }

                    WereWolfController werewolf = enemy.GetComponent<WereWolfController>();
                    if (werewolf != null)
                    {
                        isAlive = !werewolf.IsDead();
                        Debug.Log($"    {index}. {enemy.name} (WereWolf) - {(isAlive ? "ALIVE" : "DEAD")}");
                    }

                    SkeletonArcherController skeletonArcher = enemy.GetComponent<SkeletonArcherController>();
                    if (skeletonArcher != null)
                    {
                        isAlive = !skeletonArcher.IsDead();
                        Debug.Log($"    {index}. {enemy.name} (Skeleton Archer) - {(isAlive ? "ALIVE" : "DEAD")}");
                    }

                    WizardBoss wizardBoss = enemy.GetComponent<WizardBoss>();
                    if (wizardBoss != null)
                    {
                        isAlive = wizardBoss.CurrentHealth > 0;
                        Debug.Log($"    {index}. {enemy.name} (Wizard Boss) - HP: {wizardBoss.CurrentHealth}/{wizardBoss.MaxHealth} - {(isAlive ? "ALIVE" : "DEAD")}");
                    }

                    FinalBoss finalBoss = enemy.GetComponent<FinalBoss>();
                    if (finalBoss != null)
                    {
                        isAlive = finalBoss.CurrentHealth > 0;
                        Debug.Log($"    {index}. {enemy.name} (Final Boss) - HP: {finalBoss.CurrentHealth}/{finalBoss.MaxHealth} - {(isAlive ? "ALIVE" : "DEAD")}");
                    }

                    index++;
                }
                else
                {
                    Debug.Log($"    {index}. [NULL - Enemy was destroyed but not unregistered]");
                    index++;
                }
            }
        }
        Debug.Log("═══════════════════════════════════════");
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
