using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class FinalBoss : MonoBehaviour
{
    [Header("Boss Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private float phaseThreshold = 50; // Health threshold for Phase 2

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f; // Normal movement speed
    [SerializeField] private float detectionRange = 8f; // Range to detect player
    [SerializeField] private float minDistance = 3f; // Minimum distance from player
    [SerializeField] private float maxDistance = 6f; // Maximum distance from player
    [SerializeField] private float wanderInterval = 2f; // Time between random movements
    [SerializeField] private float chargeSpeed = 10f; // Speed during charge attack
    [SerializeField] private float finalChargeSpeedMultiplier = 1.3f; // Last charge is faster

    [Header("Phase 1 Attack Settings")]
    [SerializeField] private float dashAttackInterval = 6f; // Time between dash attack sequences
    [SerializeField] private float lungeAttackInterval = 8f; // Time between lunge attacks
    [SerializeField] private int phase1DashCount = 2; // Number of dashes in Phase 1

    [Header("Phase 2 Attack Settings")]
    [SerializeField] private int phase2DashCount = 3; // Number of dashes in Phase 2
    [SerializeField] private int phase1SpearCount = 6; // Number of spears in Phase 1 (circle pattern)
    [SerializeField] private int phase2SpearCount = 12; // Number of spears in Phase 2 (circle pattern)

    [Header("Dash Attack Settings")]
    [SerializeField] private float dashChargeTime = 0.8f; // Wind-up time before dash
    [SerializeField] private float dashDuration = 0.4f; // How long each dash lasts
    [SerializeField] private float dashCooldown = 0.5f; // Pause between dashes
    [SerializeField] private int dashDamage = 20;
    [SerializeField] private float dashHitboxRadius = 0.8f; // Dash collision detection radius

    [Header("Lunge Attack Settings")]
    [SerializeField] private float lungeChargeTime = 1.2f; // Wind-up time for lunge
    [SerializeField] private float lungeRetreatDistance = 3f; // Distance to retreat before lunge
    [SerializeField] private float lungeRetreatSpeed = 6f;
    [SerializeField] private int spearDamage = 15;
    [SerializeField] private float spearSpeed = 12f;
    [SerializeField] private float spearSpread = 15f; // Angle spread for multiple spears

    [Header("Prefabs")]
    [SerializeField] private GameObject spearPrefab;
    [SerializeField] private GameObject corruptedZonePrefab;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip chargeSound; // 돌진 소리
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.7f;

    [Header("Visual Settings")]
    [SerializeField] private float blinkSpeed = 10f;
    [SerializeField] private GameObject auraEffect; // Boss aura sprite object

    [Header("Stagger Settings")]
    [SerializeField] private float staggerDuration = 0.5f;

    [Header("Corrupted Zone Settings")]
    [SerializeField] private float corruptedZoneInterval = 2f; // How often to spawn zones
    [SerializeField] private float corruptedZoneDuration = 5f;
    [SerializeField] private int corruptedZoneDamage = 5;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Transform player;
    private AudioSource audioSource;

    private bool isDead = false;
    private bool isInPhase2 = false;
    private bool facingRight = true;
    private bool isPerformingAttack = false;
    private bool isStaggered = false;
    private float staggerTimer = 0f;
    private bool hasDetectedPlayer = false; // Track if player has been detected

    // Attack timers
    private float nextDashAttackTime;
    private float nextLungeAttackTime;
    private float lastCorruptedZoneTime;

    // Wandering state
    private Vector2 wanderTarget;
    private float nextWanderTime;

    // Dash hit tracking
    private HashSet<Collider2D> dashHitTargets = new HashSet<Collider2D>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;

        // Add AudioSource for sound effects
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
        audioSource.volume = sfxVolume;

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

        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // Initialize attack timers
        nextDashAttackTime = Time.time + 2f; // First attack after 2 seconds
        nextLungeAttackTime = Time.time + 4f;
        lastCorruptedZoneTime = Time.time; // Start spawning zones immediately

        // Initialize wandering
        nextWanderTime = Time.time;
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

            // Keep Phase 2 tint during blink
            if (isInPhase2)
            {
                spriteRenderer.color = new Color(1f, 0.6f, 0.6f, alpha); // Light red tint
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
                    spriteRenderer.color = new Color(1f, 0.6f, 0.6f, 1f); // Light red tint
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

    private void FixedUpdate()
    {
        // No continuous spawning - zones only spawn during dash
    }

    private void HandleMovement()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Check if player is within detection range for the first time
        if (!hasDetectedPlayer && distanceToPlayer <= detectionRange)
        {
            hasDetectedPlayer = true;
            Debug.Log("Final Boss detected player!");
        }

        // Only move if player has been detected
        if (hasDetectedPlayer)
        {
            // If too close to player, move away (safety distance)
            if (distanceToPlayer < minDistance)
            {
                // Move away from player
                Vector2 direction = (transform.position - player.position).normalized;
                rb.linearVelocity = direction * moveSpeed;

                if (animator != null)
                {
                    animator.SetFloat("MoveSpeed", moveSpeed);
                }
            }
            // If too far from player, move towards them
            else if (distanceToPlayer > maxDistance)
            {
                // Move towards player
                Vector2 direction = (player.position - transform.position).normalized;
                rb.linearVelocity = direction * moveSpeed;

                if (animator != null)
                {
                    animator.SetFloat("MoveSpeed", moveSpeed);
                }
            }
            // In optimal range: wander freely
            else
            {
                // Generate new wander target periodically
                if (Time.time >= nextWanderTime)
                {
                    GenerateWanderTarget();
                    nextWanderTime = Time.time + wanderInterval;
                }

                // Move towards wander target
                float distanceToTarget = Vector2.Distance(transform.position, wanderTarget);

                if (distanceToTarget > 0.5f) // If not at target yet
                {
                    Vector2 direction = (wanderTarget - (Vector2)transform.position).normalized;
                    rb.linearVelocity = direction * moveSpeed;

                    if (animator != null)
                    {
                        animator.SetFloat("MoveSpeed", moveSpeed);
                    }
                }
                else
                {
                    // Reached target, slow down a bit
                    rb.linearVelocity = rb.linearVelocity * 0.5f;
                }
            }
        }
        else
        {
            StopMoving();
        }
    }

    private void GenerateWanderTarget()
    {
        // Generate a random position within the min-max distance range from player
        float randomDistance = Random.Range(minDistance + 0.5f, maxDistance - 0.5f);
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

        Vector2 offset = new Vector2(
            Mathf.Cos(randomAngle) * randomDistance,
            Mathf.Sin(randomAngle) * randomDistance
        );

        wanderTarget = (Vector2)player.position + offset;
    }

    private void HandleAttackPattern()
    {
        // Only attack if player has been detected
        if (!hasDetectedPlayer) return;

        // Prioritize dash attack
        if (Time.time >= nextDashAttackTime)
        {
            Debug.Log($"Starting Dash Attack! Time: {Time.time}, Next: {nextDashAttackTime}");
            StartCoroutine(PerformDashAttackSequence());
            nextDashAttackTime = Time.time + dashAttackInterval;
        }
        // Then lunge attack
        else if (Time.time >= nextLungeAttackTime)
        {
            Debug.Log($"Starting Lunge Attack! Time: {Time.time}, Next: {nextLungeAttackTime}");
            StartCoroutine(PerformLungeAttack());
            nextLungeAttackTime = Time.time + lungeAttackInterval;
        }
    }

    private IEnumerator PerformDashAttackSequence()
    {
        isPerformingAttack = true;
        StopMoving();

        int dashCount = isInPhase2 ? phase2DashCount : phase1DashCount;

        for (int i = 0; i < dashCount; i++)
        {
            bool isFinalDash = (i == dashCount - 1);
            yield return StartCoroutine(PerformSingleDash(isFinalDash));

            // Pause between dashes (except after last dash)
            if (!isFinalDash)
            {
                yield return new WaitForSeconds(dashCooldown);
            }
        }

        yield return new WaitForSeconds(0.5f);
        isPerformingAttack = false;
    }

    private IEnumerator PerformSingleDash(bool isFinalDash)
    {
        // Get current player position for targeting
        Vector2 targetPosition = player.position;
        Vector2 startPosition = transform.position;
        Vector2 dashDirection = (targetPosition - startPosition).normalized;

        // Face the player before charging
        UpdateFacingDirection(dashDirection);

        // Charge up animation
        if (animator != null)
        {
            animator.SetTrigger("Charge");
        }

        yield return new WaitForSeconds(dashChargeTime);

        // Update direction again right before dash (player might have moved)
        targetPosition = player.position;
        dashDirection = (targetPosition - (Vector2)transform.position).normalized;

        // Face the player again
        UpdateFacingDirection(dashDirection);

        // Calculate dash speed (final dash is faster)
        float currentDashSpeed = isFinalDash ? chargeSpeed * finalChargeSpeedMultiplier : chargeSpeed;

        // Play charge sound
        if (chargeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(chargeSound, sfxVolume);
        }

        // Trigger attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Perform dash
        float dashTimer = 0f;
        HashSet<Collider2D> hitTargets = new HashSet<Collider2D>();

        while (dashTimer < dashDuration)
        {
            rb.linearVelocity = dashDirection * currentDashSpeed;

            // Spawn corrupted zones during dash
            if (Time.time - lastCorruptedZoneTime >= corruptedZoneInterval)
            {
                SpawnCorruptedZone();
                lastCorruptedZoneTime = Time.time;
            }

            // Check for player collision during dash
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, dashHitboxRadius);
            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Player") && !hitTargets.Contains(hit))
                {
                    PlayerController playerController = hit.GetComponent<PlayerController>();
                    if (playerController != null)
                    {
                        playerController.TakeDamage(dashDamage);
                        Debug.Log($"Final Boss Dash hit player! Damage: {dashDamage}");

                        // Apply knockback
                        Rigidbody2D playerRb = hit.GetComponent<Rigidbody2D>();
                        if (playerRb != null)
                        {
                            Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;
                            playerRb.AddForce(knockbackDir * 10f, ForceMode2D.Impulse);
                        }

                        hitTargets.Add(hit);
                    }
                }
            }

            dashTimer += Time.deltaTime;
            yield return null;
        }

        StopMoving();
    }

    private IEnumerator PerformLungeAttack()
    {
        isPerformingAttack = true;
        StopMoving();

        // Retreat from player
        Vector2 retreatDirection = (transform.position - player.position).normalized;
        Vector2 retreatTarget = (Vector2)transform.position + retreatDirection * lungeRetreatDistance;

        // Move to retreat position
        float retreatTimer = 0f;
        float retreatDuration = lungeRetreatDistance / lungeRetreatSpeed;

        while (retreatTimer < retreatDuration)
        {
            rb.linearVelocity = retreatDirection * lungeRetreatSpeed;
            retreatTimer += Time.deltaTime;
            yield return null;
        }

        StopMoving();

        // Charge animation
        if (animator != null)
        {
            animator.SetTrigger("Cast");
        }

        // Wait for a shorter time to sync with animation (reduced delay)
        yield return new WaitForSeconds(lungeChargeTime * 0.6f);

        // Fire spear projectiles in a circle around boss
        int spearCount = isInPhase2 ? phase2SpearCount : phase1SpearCount;

        // Fire spears in 360-degree circle pattern
        float angleStep = 360f / spearCount;

        for (int i = 0; i < spearCount; i++)
        {
            float angle = i * angleStep;
            Vector2 direction = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            FireSpear(direction);
        }

        yield return new WaitForSeconds(0.5f);
        isPerformingAttack = false;
    }

    private void FireSpear(Vector2 direction)
    {
        if (spearPrefab == null) return;

        GameObject spear = Instantiate(spearPrefab, transform.position, Quaternion.identity);
        SpearProjectile spearScript = spear.GetComponent<SpearProjectile>();

        if (spearScript != null)
        {
            spearScript.Initialize(direction, spearSpeed, spearDamage);
        }
    }

    private void SpawnCorruptedZone()
    {
        if (corruptedZonePrefab == null) return;

        GameObject zone = Instantiate(corruptedZonePrefab, transform.position, Quaternion.identity);
        CorruptedZone zoneScript = zone.GetComponent<CorruptedZone>();

        if (zoneScript != null)
        {
            zoneScript.Initialize(corruptedZoneDuration, corruptedZoneDamage);
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

    private void UpdateFacingDirection(Vector2 direction)
    {
        // Face right if direction is to the right, face left if direction is to the left
        if (direction.x > 0 && !facingRight)
        {
            Flip();
        }
        else if (direction.x < 0 && facingRight)
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
        Debug.Log($"Final Boss took {damage} damage! Current health: {currentHealth}");

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
        Debug.Log("Final Boss entered Phase 2!");

        // Change to Phase 2 BGM
        BGMManager bgmManager = FindObjectOfType<BGMManager>();
        if (bgmManager != null)
        {
            Debug.Log("[FinalBoss] BGMManager found, calling PlayBoss2Phase2BGM");
            bgmManager.PlayBoss2Phase2BGM();
        }
        else
        {
            Debug.LogWarning("[FinalBoss] BGMManager not found!");
        }

        // Immediately trigger an attack when entering Phase 2
        // Set timers to trigger immediately (past time)
        nextDashAttackTime = Time.time - 1f; // Set to past time to trigger immediately
        nextLungeAttackTime = Time.time + lungeAttackInterval; // Keep lunge on normal schedule
        lastCorruptedZoneTime = Time.time;

        // Increase movement speed in Phase 2
        moveSpeed *= 1.3f;
        chargeSpeed *= 1.2f;

        // Change sprite color to light red tint (Phase 2 indicator)
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 0.6f, 0.6f, 1f); // Light red tint
        }

        // Optional: Play phase transition animation/effect (disabled - animator parameter doesn't exist)
        // if (animator != null)
        // {
        //     animator.SetTrigger("PhaseTransition");
        // }
    }

    private void Die()
    {
        Debug.Log("Final Boss defeated!");
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
        // Play death dialogue
        BossDialogue bossDialogue = FindObjectOfType<BossDialogue>();
        if (bossDialogue != null)
        {
            Dialogue[] deathDialogues = bossDialogue.GetBoss2DeathDialogues();
            DialogueManager.StartDialogue(deathDialogues, pauseGame: false, disablePlayer: false);

            // Wait for dialogue to complete
            while (DialogueManager.IsDialogueActive())
            {
                yield return null;
            }
        }

        // Change BGM back to Boss 2 Phase 1 BGM after boss death
        BGMManager bgmManager = FindObjectOfType<BGMManager>();
        if (bgmManager != null)
        {
            Debug.Log("[FinalBoss] Changing BGM back to Boss 2 Phase 1 BGM after death");
            bgmManager.PlayBoss2BGM();
        }

        // Wait for death animation to complete (adjust time if needed)
        yield return new WaitForSeconds(2f);

        // Freeze animator at the last frame of death animation
        if (animator != null)
        {
            // Set animator to play Die state at the last frame
            animator.Play("Lancer_Die", 0, 1f); // Play at 100% (last frame)
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

    private void OnDrawGizmosSelected()
    {
        // Detection range (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Min distance (cyan)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, minDistance);

        // Max distance (blue)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, maxDistance);

        // Dash detection range (red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.8f);

        // Show current wander target (green)
        if (Application.isPlaying && wanderTarget != Vector2.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(wanderTarget, 0.3f);
            Gizmos.DrawLine(transform.position, wanderTarget);
        }
    }
}
