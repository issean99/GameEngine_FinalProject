using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class SkeletonArcherController : MonoBehaviour
{
    [Header("Archer Stats")]
    [SerializeField] private int maxHealth = 50;
    [SerializeField] private int currentHealth;
    [SerializeField] private int contactDamage = 5; // Damage when player touches archer

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float detectionRange = 12f; // Start detecting player
    [SerializeField] private float optimalRangeMin = 5f; // Minimum optimal distance
    [SerializeField] private float optimalRangeMax = 8f; // Maximum optimal distance
    [SerializeField] private float retreatDistance = 4f; // If player gets closer, retreat

    [Header("Attack Settings")]
    [SerializeField] private float shootInterval = 2.5f;
    [SerializeField] private int arrowDamage = 10;
    [SerializeField] private float arrowSpeed = 8f;
    [SerializeField] private float stunDuration = 0.3f;

    [Header("Projectile Prefabs")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform shootPoint; // Position where arrow spawns

    [Header("Visual Settings")]
    [SerializeField] private float blinkSpeed = 10f;

    [Header("Stagger Settings")]
    [SerializeField] private float staggerDuration = 0.5f;

    [Header("Contact Damage Settings")]
    [SerializeField] private float contactDamageInterval = 1f; // Damage player every X seconds when touching

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Transform player;

    private bool isDead = false;
    private bool facingRight = true;
    private bool isPerformingAttack = false;
    private bool isStaggered = false;
    private float staggerTimer = 0f;

    // Attack timer
    private float nextShootTime;
    private float nextContactDamageTime;

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

        // Find shoot point if not assigned (look for child with "ShootPoint" in name)
        if (shootPoint == null)
        {
            foreach (Transform child in transform)
            {
                if (child.name.Contains("ShootPoint") || child.name.Contains("shootPoint"))
                {
                    shootPoint = child;
                    break;
                }
            }
            // If still not found, use this transform
            if (shootPoint == null)
            {
                shootPoint = transform;
            }
        }

        // Initialize attack timer
        nextShootTime = Time.time + shootInterval;
    }

    private void Update()
    {
        if (isDead || player == null) return;

        // Handle stagger state (visual effect only, doesn't stop actions)
        if (isStaggered)
        {
            staggerTimer -= Time.deltaTime;

            // Blink effect during stagger
            float alpha = Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed));
            spriteRenderer.color = new Color(1f, 1f, 1f, alpha);

            if (staggerTimer <= 0)
            {
                // End stagger - restore color
                isStaggered = false;
                spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
            }
            // Continue with actions even during stagger
        }

        if (isPerformingAttack) return;

        FacePlayer();
        HandleMovement();
        HandleAttackPattern();
    }

    private void HandleMovement()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            // Priority 1: If player is too close, retreat immediately
            if (distanceToPlayer < retreatDistance)
            {
                // Move away from player quickly
                Vector2 direction = (transform.position - player.position).normalized;
                rb.linearVelocity = direction * moveSpeed;

                if (animator != null)
                {
                    animator.SetFloat("MoveSpeed", moveSpeed);
                }
            }
            // Priority 2: If too far from optimal range, move closer
            else if (distanceToPlayer > optimalRangeMax)
            {
                // Move towards player to get into optimal range
                Vector2 direction = (player.position - transform.position).normalized;
                rb.linearVelocity = direction * (moveSpeed * 0.8f); // Move slower when approaching

                if (animator != null)
                {
                    animator.SetFloat("MoveSpeed", moveSpeed * 0.8f);
                }
            }
            // Priority 3: If too close to optimal range, back up slowly
            else if (distanceToPlayer < optimalRangeMin)
            {
                // Move away from player slowly
                Vector2 direction = (transform.position - player.position).normalized;
                rb.linearVelocity = direction * (moveSpeed * 0.6f); // Move slower when adjusting position

                if (animator != null)
                {
                    animator.SetFloat("MoveSpeed", moveSpeed * 0.6f);
                }
            }
            // Priority 4: In optimal range - stay still and shoot
            else
            {
                StopMoving();
            }
        }
        else
        {
            // Player out of detection range
            StopMoving();
        }
    }

    private void HandleAttackPattern()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Only attack if player is in detection range
        if (distanceToPlayer <= detectionRange && Time.time >= nextShootTime)
        {
            StartCoroutine(PerformShootAttack());
            nextShootTime = Time.time + shootInterval;
        }
    }

    private IEnumerator PerformShootAttack()
    {
        isPerformingAttack = true;
        StopMoving();

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

        // Wait for animation to reach shoot frame
        yield return new WaitForSeconds(0.4f);

        // Shoot arrow
        ShootArrow();

        yield return new WaitForSeconds(0.3f);
        isPerformingAttack = false;
    }

    private void ShootArrow()
    {
        if (arrowPrefab == null || player == null) return;

        Vector2 direction = (player.position - shootPoint.position).normalized;

        GameObject arrow = Instantiate(arrowPrefab, shootPoint.position, Quaternion.identity);
        SkeletonArcherArrow arrowScript = arrow.GetComponent<SkeletonArcherArrow>();

        if (arrowScript != null)
        {
            arrowScript.Initialize(direction, arrowSpeed, arrowDamage);
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

    private void FacePlayer()
    {
        if (player == null) return;

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
    }

    // Handle contact damage to player
    private void OnTriggerStay2D(Collider2D other)
    {
        if (isDead || isStaggered) return;

        if (other.CompareTag("Player") && Time.time >= nextContactDamageTime)
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(contactDamage);
                nextContactDamageTime = Time.time + contactDamageInterval;
                Debug.Log($"Skeleton Archer dealt {contactDamage} contact damage to player!");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"Skeleton Archer took {damage} damage! Current health: {currentHealth}");

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
            // Enter stagger state (visual effect only)
            isStaggered = true;
            staggerTimer = staggerDuration;

            // Trigger hurt animation
            if (animator != null)
            {
                animator.SetTrigger("Hurt");
            }
        }
    }

    // Public method to check if archer is staggered or dead (for contact damage prevention)
    public bool IsStaggeredOrDead()
    {
        return isStaggered || isDead;
    }

    public bool IsDead()
    {
        return isDead;
    }

    private void Die()
    {
        Debug.Log("Skeleton Archer defeated!");
        isDead = true;
        currentHealth = 0;

        // Play death sound
        EnemySoundEffects soundFX = GetComponent<EnemySoundEffects>();
        if (soundFX != null)
        {
            soundFX.PlayDeathSound();
        }

        // Stop all ongoing attacks and coroutines
        StopAllCoroutines();
        isPerformingAttack = false;

        StopMoving();

        // Disable colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        // Reset color to original
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
        }

        // Trigger death animation
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Start coroutine to disable script after death animation
        StartCoroutine(DisableAfterDeath());
    }

    private IEnumerator DisableAfterDeath()
    {
        // Wait for death animation to complete
        yield return new WaitForSeconds(1f);

        // Destroy the GameObject completely
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Detection range (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Optimal range max (green)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, optimalRangeMax);

        // Optimal range min (cyan)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, optimalRangeMin);

        // Retreat distance (red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, retreatDistance);
    }
}
