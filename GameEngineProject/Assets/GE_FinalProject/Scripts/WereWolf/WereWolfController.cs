using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class WereWolfController : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private int attackDamage = 15;

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

            if (distanceToPlayer <= attackRange)
            {
                // In attack range - stop and attack
                StopMoving();
                TryAttack();
            }
            else
            {
                // Move towards player
                MoveTowardsPlayer();
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

    private void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;

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

        // Trigger death animation
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Destroy after animation
        Destroy(gameObject, 1f);
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
