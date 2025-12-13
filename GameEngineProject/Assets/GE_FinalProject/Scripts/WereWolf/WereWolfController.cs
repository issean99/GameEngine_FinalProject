using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class WereWolfController : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float attackRange = 2f;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private GameObject attackHitbox;

    [Header("Stagger Settings")]
    [SerializeField] private float staggerDuration = 0.5f;
    [SerializeField] private float blinkSpeed = 10f;

    [Header("Item Drop")]
    [SerializeField] private GameObject werewolfDashItemPrefab; // Werewolf Dash skill item
    [SerializeField] private float dropChance = 0.3f; // 30% chance to drop skill item
    [SerializeField] private GameObject healthItemPrefab; // Health item
    [SerializeField] private float healthDropChance = 0.5f; // 50% chance to drop health

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Transform player;
    private float lastAttackTime;
    private bool isDead = false;
    private bool isStaggered = false;
    private float staggerTimer = 0f;
    [SerializeField] private bool facingRight = true; // Inspector에서 초기 방향 설정 가능

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;

        // Configure Rigidbody2D for proper collision
        if (rb != null)
        {
            rb.gravityScale = 0; // No gravity for 2D top-down
            rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Prevent rotation
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Better collision
        }

        // Ensure we have a collider for wall collision
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.4f;
            Debug.Log("[WereWolf] Added CircleCollider2D for wall collision");
        }
        collider.isTrigger = false; // NOT a trigger - we want physical collision

        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // Set initial sprite flip based on facingRight
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !facingRight;
        }

        // Initialize hitbox position based on initial facing direction
        if (attackHitbox != null && !facingRight)
        {
            // If initially facing left, flip the hitbox position
            Vector3 hitboxPos = attackHitbox.transform.localPosition;
            hitboxPos.x = -Mathf.Abs(hitboxPos.x); // Ensure it's negative (left side)
            attackHitbox.transform.localPosition = hitboxPos;
        }

    }

    private void Update()
    {
        if (isDead || player == null) return;

        // 경직 상태 처리 (Hurt 애니메이션 + 깜빡임)
        if (isStaggered)
        {
            staggerTimer -= Time.deltaTime;

            // 깜빡임 효과
            float alpha = Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed));
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;

            if (staggerTimer <= 0)
            {
                // 경직 해제
                isStaggered = false;
                Color resetColor = spriteRenderer.color;
                resetColor.a = 1f;
                spriteRenderer.color = resetColor;
            }

            // 경직 중에는 행동하지 않음
            StopMoving();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Check if player is in detection range
        if (distanceToPlayer <= detectionRange)
        {
            // Face the player
            FacePlayer();

            // Check if currently attacking
            bool isCurrentlyAttacking = IsAttacking();

            if (distanceToPlayer <= attackRange)
            {
                // In attack range - stop and attack
                if (!isCurrentlyAttacking)
                {
                    StopMoving();
                    TryAttack();
                }
                else
                {
                    // Keep stopped while attacking
                    StopMoving();
                }
            }
            else
            {
                // Move towards player (only if not attacking)
                if (!isCurrentlyAttacking)
                {
                    MoveTowardsPlayer();
                }
                else
                {
                    // Keep stopped while attacking (even if player moved away)
                    StopMoving();
                }
            }
        }
        else
        {
            // Player out of range - idle
            StopMoving();
        }
    }

    private void FacePlayer()
    {
        if (player.position.x > transform.position.x && !facingRight)
        {
            Flip();
        }
        else if (player.position.x < transform.position.x && facingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !facingRight;
        }

        // Flip the attack hitbox position (same method as player)
        if (attackHitbox != null)
        {
            Vector3 hitboxPos = attackHitbox.transform.localPosition;
            hitboxPos.x = -hitboxPos.x; // Simply negate X position
            attackHitbox.transform.localPosition = hitboxPos;
            Debug.Log($"WereWolf Hitbox flipped: X={hitboxPos.x}");
        }
        else
        {
            Debug.LogWarning("WereWolf attackHitbox is not assigned!");
        }

        Debug.Log($"WereWolf Flip: facingRight={facingRight}, flipX={spriteRenderer.flipX}");
    }

    private void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;

        // Update animator
        if (animator != null)
        {
            animator.SetFloat("MoveSpeed", moveSpeed);
        }
    }

    private void StopMoving()
    {
        rb.linearVelocity = Vector2.zero;

        if (animator != null)
        {
            animator.SetFloat("MoveSpeed", 0f);
        }
    }

    private bool IsAttacking()
    {
        if (animator == null) return false;

        // Check if currently in Attack state
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName("WereWolfAttack");
    }

    private void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;
        StopMoving(); // 공격 시작 시 즉시 멈춤

        // Play attack sound
        EnemySoundEffects soundFX = GetComponent<EnemySoundEffects>();
        if (soundFX != null)
        {
            soundFX.PlayAttackSound();
        }

        // Trigger attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    // Animation Event functions - called from WereWolfAttack animation
    public void ActivateHitbox()
    {
        if (attackHitbox != null)
            attackHitbox.SetActive(true);
    }

    public void DeactivateHitbox()
    {
        if (attackHitbox != null)
            attackHitbox.SetActive(false);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage! Current health: {currentHealth}");

        // Play hit sound
        EnemySoundEffects soundFX = GetComponent<EnemySoundEffects>();
        if (soundFX != null)
        {
            soundFX.PlayHitSound();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // 경직 상태 시작
            isStaggered = true;
            staggerTimer = staggerDuration;

            // 공격 히트박스 비활성화 (공격 중이었다면)
            if (attackHitbox != null)
            {
                attackHitbox.SetActive(false);
            }

            // Hurt 애니메이션 재생
            if (animator != null)
            {
                animator.SetTrigger("Hurt");
            }
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died!");
        isDead = true;
        currentHealth = 0;

        // Notify EnemyManager
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.OnEnemyDefeated(gameObject);
        }

        // Play death sound
        EnemySoundEffects soundFX = GetComponent<EnemySoundEffects>();
        if (soundFX != null)
        {
            soundFX.PlayDeathSound();
        }

        // Stop movement
        rb.linearVelocity = Vector2.zero;

        // Disable colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        // Deactivate attack hitbox
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }

        // Drop item with chance
        DropItem();

        // Trigger death animation
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Destroy after animation
        Destroy(gameObject, 1f);
    }

    private void DropItem()
    {
        // Drop skill item with chance
        float skillRandomValue = Random.Range(0f, 1f);
        if (skillRandomValue <= dropChance && werewolfDashItemPrefab != null)
        {
            Vector3 dropPosition = transform.position + Vector3.up * 0.5f;
            GameObject droppedItem = Instantiate(werewolfDashItemPrefab, dropPosition, Quaternion.identity);
            Debug.Log($"WereWolf dropped Dash Skill item at {dropPosition}!");
        }

        // Drop health item with chance
        float healthRandomValue = Random.Range(0f, 1f);
        if (healthRandomValue <= healthDropChance && healthItemPrefab != null)
        {
            Vector3 dropPosition = transform.position + Vector3.up * 0.5f + Vector3.right * 0.3f;
            GameObject droppedHealth = Instantiate(healthItemPrefab, dropPosition, Quaternion.identity);
            Debug.Log($"WereWolf dropped Health item at {dropPosition}!");
        }
    }

    // 경직 또는 죽은 상태인지 확인 (접촉 대미지 무효화용)
    public bool IsStaggeredOrDead()
    {
        return isStaggered || isDead;
    }

    // Visualize ranges in editor
    private void OnDrawGizmosSelected()
    {
        // Detection range (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Attack range (red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
