using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class WizardBoss : MonoBehaviour
{
    [Header("Boss Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private float phaseThreshold = 50; // Health threshold for Phase 2

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float keepDistance = 5f; // Maintain distance from player

    [Header("Attack Settings - Phase 1")]
    [SerializeField] private float magicMissileInterval = 3f;
    [SerializeField] private float arcaneBurstInterval = 5f;
    [SerializeField] private int phase1MissileCount = 3; // Number of missiles per volley in Phase 1
    [SerializeField] private float phase1MissileDelay = 0.3f; // Delay between missiles

    [Header("Attack Settings - Phase 2")]
    [SerializeField] private float arcaneFanInterval = 4f;
    [SerializeField] private float arcaneBurstUpgradedInterval = 6f;
    [SerializeField] private int phase2VolleyCount = 3; // Number of volleys in Phase 2
    [SerializeField] private int phase2MissilesPerVolley = 5; // Missiles per volley
    [SerializeField] private float phase2SpreadAngle = 40f; // Spread angle for fan pattern (degrees)
    [SerializeField] private float phase2VolleyDelay = 0.4f; // Delay between volleys

    [Header("Projectile Prefabs")]
    [SerializeField] private GameObject magicMissilePrefab;
    [SerializeField] private GameObject arcaneBurstPrefab;

    [Header("Visual Settings")]
    [SerializeField] private float blinkSpeed = 10f;
    [SerializeField] private GameObject auraEffect; // Boss aura sprite object

    [Header("Stagger Settings")]
    [SerializeField] private float staggerDuration = 0.5f;

    [Header("Item Drop")]
    [SerializeField] private GameObject wizardSpellbookItemPrefab; // Wizard's Spellbook item (grants both skills)

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Transform player;

    private bool isDead = false;
    private bool isInPhase2 = false;
    private bool facingRight = true;
    private bool isPerformingAttack = false;
    private bool isStaggered = false;
    private float staggerTimer = 0f;

    // Attack timers
    private float nextMagicMissileTime;
    private float nextArcaneBurstTime;
    private float nextArcaneFanTime;
    private float nextArcaneBurstUpgradedTime;

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

        // Find aura effect if not assigned (look for child with "Aura" in name)
        if (auraEffect == null)
        {
            try
            {
                foreach (Transform child in transform)
                {
                    if (child != null && child.name != null &&
                        (child.name.Contains("Aura") || child.name.Contains("aura")))
                    {
                        auraEffect = child.gameObject;
                        break;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not find Aura effect: {e.Message}");
            }
        }

        // Initialize attack timers
        nextMagicMissileTime = Time.time + magicMissileInterval;
        nextArcaneBurstTime = Time.time + arcaneBurstInterval;
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

            // Keep Phase 2 red tint during blink
            if (isInPhase2)
            {
                spriteRenderer.color = new Color(1f, 0.6f, 0.6f, alpha);
            }
            else
            {
                spriteRenderer.color = new Color(1f, 1f, 1f, alpha);
            }

            if (staggerTimer <= 0)
            {
                // End stagger - restore appropriate color
                isStaggered = false;

                if (isInPhase2)
                {
                    spriteRenderer.color = new Color(1f, 0.6f, 0.6f, 1f); // Phase 2 red tint
                }
                else
                {
                    spriteRenderer.color = new Color(1f, 1f, 1f, 1f); // Normal white
                }
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
            // Maintain distance from player (kiting behavior)
            if (distanceToPlayer < keepDistance)
            {
                // Move away from player
                Vector2 direction = (transform.position - player.position).normalized;
                rb.linearVelocity = direction * moveSpeed;

                if (animator != null)
                {
                    animator.SetFloat("MoveSpeed", moveSpeed);
                }
            }
            else if (distanceToPlayer > keepDistance + 1f)
            {
                // Move towards player
                Vector2 direction = (player.position - transform.position).normalized;
                rb.linearVelocity = direction * moveSpeed;

                if (animator != null)
                {
                    animator.SetFloat("MoveSpeed", moveSpeed);
                }
            }
            else
            {
                // Stay in position
                StopMoving();
            }
        }
        else
        {
            StopMoving();
        }
    }

    private void HandleAttackPattern()
    {
        if (isInPhase2)
        {
            // Phase 2 Attacks
            if (Time.time >= nextMagicMissileTime)
            {
                StartCoroutine(PerformMagicMissile());
                nextMagicMissileTime = Time.time + magicMissileInterval;
            }
            else if (Time.time >= nextArcaneBurstUpgradedTime)
            {
                StartCoroutine(PerformArcaneBurstUpgraded());
                nextArcaneBurstUpgradedTime = Time.time + arcaneBurstUpgradedInterval;
            }
        }
        else
        {
            // Phase 1 Attacks
            if (Time.time >= nextMagicMissileTime)
            {
                StartCoroutine(PerformMagicMissile());
                nextMagicMissileTime = Time.time + magicMissileInterval;
            }
            else if (Time.time >= nextArcaneBurstTime)
            {
                StartCoroutine(PerformArcaneBurst());
                nextArcaneBurstTime = Time.time + arcaneBurstInterval;
            }
        }
    }

    private IEnumerator PerformMagicMissile()
    {
        isPerformingAttack = true;
        StopMoving();

        // Trigger attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Wait for animation to start
        yield return new WaitForSeconds(0.3f);

        if (isInPhase2)
        {
            // Phase 2: Shoot multiple volleys of missiles in fan pattern
            for (int volley = 0; volley < phase2VolleyCount; volley++)
            {
                // Calculate angle spread
                float halfSpread = phase2SpreadAngle / 2f;
                float angleStep = phase2MissilesPerVolley > 1 ?
                    phase2SpreadAngle / (phase2MissilesPerVolley - 1) : 0f;
                float startAngle = -halfSpread;

                // Shoot missiles in fan pattern
                for (int i = 0; i < phase2MissilesPerVolley; i++)
                {
                    float angle = startAngle + (angleStep * i);
                    ShootProjectileAtAngle(magicMissilePrefab, 10f, angle);
                }

                yield return new WaitForSeconds(phase2VolleyDelay);
            }
        }
        else
        {
            // Phase 1: Shoot missiles straight at player
            for (int i = 0; i < phase1MissileCount; i++)
            {
                ShootProjectile(magicMissilePrefab, 10f);
                yield return new WaitForSeconds(phase1MissileDelay);
            }
        }

        yield return new WaitForSeconds(0.3f);
        isPerformingAttack = false;
    }

    private IEnumerator PerformArcaneBurst()
    {
        isPerformingAttack = true;
        StopMoving();

        // Trigger cast animation
        if (animator != null)
        {
            animator.SetTrigger("Cast");
        }

        // Wait for casting animation
        yield return new WaitForSeconds(0.5f);

        // Spawn burst at player's position (not boss position)
        if (player != null)
        {
            GameObject marker = Instantiate(arcaneBurstPrefab, player.position, Quaternion.identity);
            ArcaneBurstEffect burstEffect = marker.GetComponent<ArcaneBurstEffect>();
            if (burstEffect != null)
            {
                burstEffect.Initialize(1f, 3f, 20); // 1 second delay, 3 units radius, 20 damage
            }
        }

        yield return new WaitForSeconds(1.0f);
        isPerformingAttack = false;
    }

    private IEnumerator PerformArcaneFan()
    {
        isPerformingAttack = true;
        StopMoving();

        // Trigger attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Wait for animation to start
        yield return new WaitForSeconds(0.3f);

        // Shoot 2-3 volleys
        int volleyCount = Random.Range(2, 4);
        for (int v = 0; v < volleyCount; v++)
        {
            // Shoot 5-7 projectiles in a fan pattern
            int projectileCount = Random.Range(5, 8);
            float angleStep = 60f / (projectileCount - 1); // 60 degree spread
            float startAngle = -30f;

            for (int i = 0; i < projectileCount; i++)
            {
                float angle = startAngle + (angleStep * i);
                ShootProjectileAtAngle(magicMissilePrefab, 10f, angle);
            }

            yield return new WaitForSeconds(0.4f);
        }

        yield return new WaitForSeconds(0.3f);
        isPerformingAttack = false;
    }

    private IEnumerator PerformArcaneBurstUpgraded()
    {
        isPerformingAttack = true;
        StopMoving();

        // Trigger cast animation
        if (animator != null)
        {
            animator.SetTrigger("Cast");
        }

        // Wait for casting animation
        yield return new WaitForSeconds(0.5f);

        // Create 4 consecutive bursts near player
        for (int i = 0; i < 4; i++)
        {
            // Spawn burst at player's current position (with slight randomization)
            Vector2 burstPosition = player.position + (Vector3)Random.insideUnitCircle * 1f;
            GameObject marker = Instantiate(arcaneBurstPrefab, burstPosition, Quaternion.identity);

            ArcaneBurstEffect burstEffect = marker.GetComponent<ArcaneBurstEffect>();
            if (burstEffect != null)
            {
                burstEffect.Initialize(1f, 2.5f, 25); // 1 second delay, 2.5 units radius, 25 damage
            }

            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(0.3f);
        isPerformingAttack = false;
    }

    private void ShootProjectile(GameObject projectilePrefab, float speed)
    {
        if (projectilePrefab == null || player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;

        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        MagicProjectile projScript = projectile.GetComponent<MagicProjectile>();

        if (projScript != null)
        {
            projScript.Initialize(direction, speed, 15); // 15 damage
        }
    }

    private void ShootProjectileAtAngle(GameObject projectilePrefab, float speed, float angleOffset)
    {
        if (projectilePrefab == null || player == null) return;

        // Get base direction to player
        Vector2 baseDirection = (player.position - transform.position).normalized;

        // Rotate by angle offset
        float angle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg + angleOffset;
        Vector2 direction = new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        );

        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        MagicProjectile projScript = projectile.GetComponent<MagicProjectile>();

        if (projScript != null)
        {
            projScript.Initialize(direction, speed, 15); // 15 damage
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

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"Wizard Boss took {damage} damage! Current health: {currentHealth}");

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

            // Don't stop attacks or movement - boss continues fighting

            // Check for phase transition
            if (!isInPhase2 && currentHealth <= phaseThreshold)
            {
                EnterPhase2();
            }
        }
    }

    // Public method to check if boss is staggered or dead (for contact damage prevention)
    public bool IsStaggeredOrDead()
    {
        return isStaggered || isDead;
    }

    public bool IsDead()
    {
        return isDead;
    }

    private void EnterPhase2()
    {
        isInPhase2 = true;
        Debug.Log("Wizard Boss entered Phase 2!");

        // Initialize Phase 2 timers
        nextArcaneFanTime = Time.time + arcaneFanInterval;
        nextArcaneBurstUpgradedTime = Time.time + arcaneBurstUpgradedInterval;

        // Increase movement speed in Phase 2
        moveSpeed *= 1.3f;

        // Change sprite color to red tint (Phase 2 indicator)
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 0.6f, 0.6f, 1f); // Slightly reddish tint
        }

        // Optional: Play phase transition animation/effect
        if (animator != null)
        {
            animator.SetTrigger("PhaseTransition");
        }
    }

    private void Die()
    {
        Debug.Log("Wizard Boss defeated!");
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

        // Disable aura effect
        if (auraEffect != null)
        {
            auraEffect.SetActive(false);
        }

        // Reset color to original (remove red tint)
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f); // Normal white
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
        // Drop fireball skill item immediately on death
        DropFireballItem();

        // Wait for death animation to complete (adjust time if needed)
        yield return new WaitForSeconds(2f);

        // Freeze animator at the last frame of death animation
        if (animator != null)
        {
            animator.speed = 0f;
        }

        // Disable Rigidbody2D to stop physics
        if (rb != null)
        {
            rb.simulated = false;
        }

        // Disable this script but keep the GameObject and Animator (corpse remains with final frame)
        this.enabled = false;
    }

    private void DropFireballItem()
    {
        // Drop Wizard's Spellbook (grants both Fireball and Explosion skills)
        if (wizardSpellbookItemPrefab != null)
        {
            Vector3 dropPosition = transform.position + Vector3.up * 0.5f;
            GameObject droppedItem = Instantiate(wizardSpellbookItemPrefab, dropPosition, Quaternion.identity);
            Debug.Log("Wizard Boss dropped Wizard's Spellbook! (Fireball + Arcane Explosion)");
        }
        else
        {
            Debug.LogWarning("Wizard's Spellbook item prefab not assigned to WizardBoss!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Detection range (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Keep distance (cyan)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, keepDistance);
    }
}
