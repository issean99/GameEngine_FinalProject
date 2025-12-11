using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class SkeletonController : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private int maxHealth = 60;
    [SerializeField] private int currentHealth;
    [SerializeField] private int attackDamage = 15;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float attackRange = 1.5f;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private GameObject attackHitbox;

    [Header("Stagger Settings")]
    [SerializeField] private float staggerDuration = 0.8f;
    [SerializeField] private float blinkSpeed = 10f;

    [Header("Wall Detection")]
    [SerializeField] private float wallCheckDistance = 0.5f; // Distance to check for walls
    [SerializeField] private LayerMask wallLayer; // Layer for walls (set in Inspector)

    [Header("Group Settings")]
    [SerializeField] private SkeletonGroup group; // 그룹 스크립트 참조

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Transform player;
    private float lastAttackTime;
    private bool isDead = false;
    private bool isStaggered = false;
    private float staggerTimer = 0f;
    [SerializeField] private bool facingRight = true;

    private bool isPlayerDetected = false; // 개별 감지 상태

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

        // Set initial sprite flip
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !facingRight;
        }
    }

    private void Update()
    {
        if (isDead || player == null) return;

        // Handle stagger state
        if (isStaggered)
        {
            staggerTimer -= Time.deltaTime;

            // Blink effect
            float alpha = Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed));
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;

            if (staggerTimer <= 0)
            {
                // End stagger
                isStaggered = false;
                Color resetColor = spriteRenderer.color;
                resetColor.a = 1f;
                spriteRenderer.color = resetColor;
            }

            // Don't act while staggered
            StopMoving();
            return;
        }

        // Check if group has detected player
        if (group != null && group.IsGroupAlerted() && !isPlayerDetected)
        {
            isPlayerDetected = true; // Activate this skeleton
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Individual detection (if not part of group)
        if (group == null && distanceToPlayer <= detectionRange && !isPlayerDetected)
        {
            isPlayerDetected = true;
        }

        // Check if player is in detection range or already detected
        if (isPlayerDetected)
        {
            // Notify group that player is detected (only once)
            if (group != null && !group.IsGroupAlerted() && distanceToPlayer <= detectionRange)
            {
                group.AlertGroup();
            }

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
            // Player not detected yet - idle
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

        // Flip the attack hitbox position
        if (attackHitbox != null)
        {
            Vector3 hitboxPos = attackHitbox.transform.localPosition;
            hitboxPos.x = -hitboxPos.x;
            attackHitbox.transform.localPosition = hitboxPos;
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;

        // Check if there's a wall in the direction of movement
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, wallCheckDistance, wallLayer);

        if (hit.collider != null)
        {
            // Wall detected - stop moving
            StopMoving();
            Debug.Log($"[Skeleton] Wall detected ahead, stopping movement");
            return;
        }

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
        return stateInfo.IsName("SkeletonAttack");
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

    // Animation Event functions - called from SkeletonAttack animation
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
            // Start stagger
            isStaggered = true;
            staggerTimer = staggerDuration;

            // Deactivate attack hitbox (if attacking)
            if (attackHitbox != null)
            {
                attackHitbox.SetActive(false);
            }

            // Reset to idle (interrupt attack)
            if (animator != null)
            {
                animator.SetFloat("MoveSpeed", 0f);
                animator.ResetTrigger("Attack");
                animator.SetTrigger("Hurt"); // Play hurt animation
            }

            // Alert group when damaged
            if (group != null && !group.IsGroupAlerted())
            {
                group.AlertGroup();
                isPlayerDetected = true;
            }
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died!");
        isDead = true;
        currentHealth = 0;

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

        // Reset color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
        }

        // Trigger death animation
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Destroy after animation
        Destroy(gameObject, 1f);
    }

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
