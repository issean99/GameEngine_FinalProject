using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private GameObject slashEffectPrefab; // WhiteSlash 프리팹

    [Header("Fireball Skill Settings")]
    [SerializeField] private GameObject fireballPrefab; // Fireball 프리팹
    [SerializeField] private float fireballCooldown = 1.5f;
    [SerializeField] private bool hasFireballSkill = false; // 파이어볼 스킬 보유 여부
    [SerializeField] private int fireballCount = 3; // 한 번에 발사할 파이어볼 개수
    [SerializeField] private float fireballBurstDelay = 0.1f; // 연속 발사 간격

    [Header("Explosion Skill Settings")]
    [SerializeField] private GameObject explosionPrefab; // ArcaneBurst 프리팹
    [SerializeField] private float explosionCooldown = 3f;
    [SerializeField] private bool hasExplosionSkill = false; // 폭발 스킬 보유 여부
    [SerializeField] private float explosionDelay = 1f; // 폭발까지 지연 시간 (경고)
    [SerializeField] private float explosionRadius = 3f; // 폭발 반경
    [SerializeField] private int explosionDamage = 40; // 폭발 데미지

    [Header("Defense Skill Settings")]
    [SerializeField] private bool hasDefenseSkill = false; // 방어 스킬 보유 여부
    [SerializeField] private float defenseDuration = 2f; // 최대 방어 지속 시간
    [SerializeField] private float defenseCooldown = 5f; // 쿨타임
    [SerializeField] private Color defenseColor = new Color(0.5f, 1f, 0.5f, 0.7f); // 녹색 틴트
    [SerializeField] private GameObject defenseEffect; // 방어 이펙트 (슬라임 실드)

    [Header("Dash Skill Settings")]
    [SerializeField] private bool hasDashSkill = false; // 대쉬 스킬 보유 여부
    [SerializeField] private float dashDistance = 5f; // 대쉬 거리
    [SerializeField] private float dashDuration = 0.2f; // 대쉬 지속 시간
    [SerializeField] private float dashCooldown = 2f; // 쿨타임
    [SerializeField] private GameObject dashEffect; // 대쉬 이펙트 (발밑 오오라)

    [Header("Player Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private float invincibilityDuration = 0.5f;
    [SerializeField] private float blinkSpeed = 10f;

    [Header("Stun Settings")]
    [SerializeField] private Color stunColor = new Color(0.5f, 0.5f, 1f, 1f); // Bluish tint when stunned

    [Header("Visual Settings")]
    [SerializeField] private GameObject auraEffect; // 스킬 사용 시 발밑 오오라
    [SerializeField] private SpriteRenderer playerSpriteRenderer; // 플레이어 캐릭터 스프라이트 (색상 변경용)

    private Vector2 moveInput;
    private float invincibilityTimer = 0f;
    private bool isSprinting;
    private float lastAttackTime;
    private float lastFireballTime;
    private float lastExplosionTime;
    private float lastDefenseTime;
    private float lastDashTime;
    private bool isStunned = false;
    private float stunTimer = 0f;
    private bool isDefending = false;
    private float defenseTimer = 0f;
    private bool isDashing = false;
    private float dashTimer = 0f;
    private Vector2 dashDirection;
    private string lastVisualState = "normal"; // Track visual state changes

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Keyboard keyboard;
    private Mouse mouse;

    // Track facing direction
    private bool facingRight = true;

    // Track enemies in contact
    private HashSet<Collider2D> enemiesInContact = new HashSet<Collider2D>();

    private void Awake()
    {
        // Get required components
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Use manually assigned SpriteRenderer if available, otherwise auto-detect
        if (playerSpriteRenderer != null)
        {
            spriteRenderer = playerSpriteRenderer;
        }
        else
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            // If no SpriteRenderer on this object, try to find in children
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (spriteRenderer == null)
            {
                Debug.LogError("[Player] SpriteRenderer not found!");
            }
        }

        // Get input devices
        keyboard = Keyboard.current;
        mouse = Mouse.current;

        // Initialize health
        currentHealth = maxHealth;

        // Find aura effect if not assigned (look for child with "Aura" in name)
        if (auraEffect == null)
        {
            foreach (Transform child in transform)
            {
                if (child.name.Contains("Aura") || child.name.Contains("aura"))
                {
                    auraEffect = child.gameObject;
                    break;
                }
            }
        }

        // Hide aura initially
        if (auraEffect != null)
        {
            auraEffect.SetActive(false);
        }

        // Hide defense effect initially
        if (defenseEffect != null)
        {
            defenseEffect.SetActive(false);
        }

        // Hide dash effect initially
        if (dashEffect != null)
        {
            dashEffect.SetActive(false);
        }
    }

    private void Update()
    {
        if (keyboard == null || mouse == null)
        {
            Debug.LogWarning("Keyboard or Mouse not detected!");
            return;
        }

        // Update stun timer
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;

            if (stunTimer <= 0f)
            {
                // End stun
                isStunned = false;
            }
        }

        // Update defense timer
        if (isDefending)
        {
            defenseTimer -= Time.deltaTime;

            if (defenseTimer <= 0f)
            {
                // End defense
                EndDefense();
            }
        }

        // Update dash timer
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0f)
            {
                // End dash
                EndDash();
            }
        }

        // Update invincibility timer
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
        }

        // Update visual effects (priority: stun > defense > invincibility > normal)
        // MUST be called every frame to update colors
        UpdateVisualEffects();

        // Don't process input while stunned or dashing
        if (isStunned || isDashing)
        {
            return;
        }

        // Check for damage from enemies in contact (경직 상태가 아닌 적만)
        foreach (Collider2D enemyCollider in enemiesInContact)
        {
            if (enemyCollider != null)
            {
                // 슬라임 체크
                SlimeController slime = enemyCollider.GetComponent<SlimeController>();
                if (slime != null && !slime.IsStaggeredOrDead())
                {
                    TakeDamage(10);
                    break; // 한 번만 대미지 받음
                }

                // 웨어울프 체크
                WereWolfController werewolf = enemyCollider.GetComponent<WereWolfController>();
                if (werewolf != null && !werewolf.IsStaggeredOrDead())
                {
                    TakeDamage(15);
                    break; // 한 번만 대미지 받음
                }

                // Wizard 보스 체크
                WizardBoss wizardBoss = enemyCollider.GetComponent<WizardBoss>();
                if (wizardBoss != null && !wizardBoss.IsStaggeredOrDead())
                {
                    TakeDamage(10);
                    break; // 한 번만 대미지 받음
                }

                // Final 보스 체크
                FinalBoss finalBoss = enemyCollider.GetComponent<FinalBoss>();
                if (finalBoss != null && !finalBoss.IsStaggeredOrDead())
                {
                    TakeDamage(10);
                    break; // 한 번만 대미지 받음
                }
            }
        }

        HandleInput();
    }

    private void FixedUpdate()
    {
        // Don't move while stunned
        if (isStunned)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetFloat("MoveSpeed", 0f);
            return;
        }

        // Handle dash movement
        if (isDashing)
        {
            // Calculate dash speed based on distance and duration
            float dashSpeed = dashDistance / dashDuration;
            rb.linearVelocity = dashDirection * dashSpeed;
            animator.SetFloat("MoveSpeed", dashSpeed);
            return;
        }

        HandleMovement();
    }

    private void HandleInput()
    {
        // Read WASD movement input
        float horizontal = 0f;
        float vertical = 0f;

        if (keyboard.wKey.isPressed) vertical += 1f;
        if (keyboard.sKey.isPressed) vertical -= 1f;
        if (keyboard.aKey.isPressed) horizontal -= 1f;
        if (keyboard.dKey.isPressed) horizontal += 1f;

        moveInput = new Vector2(horizontal, vertical).normalized;

        // Check sprint input (Left Shift) - only when dash skill not unlocked
        if (!hasDashSkill)
        {
            isSprinting = keyboard.leftShiftKey.isPressed;
        }
        else
        {
            isSprinting = false; // Disable sprint when dash is available
        }

        // Check attack input (Left Mouse Button)
        if (mouse.leftButton.wasPressedThisFrame)
        {
            TryAttack();
        }

        // Check fireball skill input (E Key)
        if (hasFireballSkill && keyboard.eKey.wasPressedThisFrame)
        {
            TryFireball();
        }

        // Check explosion skill input (Q Key)
        if (hasExplosionSkill && keyboard.qKey.wasPressedThisFrame)
        {
            TryExplosion();
        }

        // Check defense skill input (Space Bar)
        if (hasDefenseSkill && keyboard.spaceKey.wasPressedThisFrame)
        {
            TryDefense();
        }

        // Release defense when key is released
        if (isDefending && keyboard.spaceKey.wasReleasedThisFrame)
        {
            EndDefense();
        }

        // Check dash skill input (Right Mouse Button)
        if (hasDashSkill && mouse.rightButton.wasPressedThisFrame && moveInput.magnitude > 0.1f)
        {
            TryDash();
        }
    }

    private void HandleMovement()
    {
        // Calculate current speed
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        // Calculate velocity
        Vector2 velocity = moveInput * currentSpeed;

        // Apply movement using Rigidbody2D
        if (rb != null)
        {
            rb.linearVelocity = velocity;
        }
        else
        {
            // Fallback: use Transform if no Rigidbody2D
            transform.Translate(velocity * Time.fixedDeltaTime, Space.World);
        }

        // Update animator based on movement
        UpdateAnimator();

        // Handle sprite flipping based on horizontal movement
        HandleSpriteFlip();
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        // Calculate movement speed for animator
        float speed = moveInput.magnitude;

        // Update MoveSpeed parameter (this matches your animator parameter)
        animator.SetFloat("MoveSpeed", speed);

        // Optional: Update IsSprinting if you add this parameter later
        // animator.SetBool("IsSprinting", isSprinting);
    }

    private void HandleSpriteFlip()
    {
        // Flip based on mouse position relative to player
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());

        if (mouseWorldPos.x > transform.position.x && !facingRight)
        {
            Flip();
        }
        else if (mouseWorldPos.x < transform.position.x && facingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;

        // Flip using SpriteRenderer instead of Transform.localScale
        // This prevents the "squishing" effect you were seeing
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !facingRight;
        }
    }

    private void TryAttack()
    {
        // Check cooldown
        if (Time.time - lastAttackTime < attackCooldown)
            return;

        PerformAttack();
    }

    private void PerformAttack()
    {
        Debug.Log("Attack performed!");
        lastAttackTime = Time.time;

        // Trigger attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Spawn slash effect at mouse direction
        SpawnSlashEffect();
    }

    private void SpawnSlashEffect()
    {
        if (slashEffectPrefab == null)
        {
            Debug.LogWarning("Slash Effect Prefab is not assigned!");
            return;
        }

        // Get mouse world position
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
        mouseWorldPos.z = 0; // 2D 게임이므로 z를 0으로 설정

        // Instantiate slash effect
        GameObject slashObj = Instantiate(slashEffectPrefab);

        // Initialize slash with player position and mouse position
        SlashEffect slashEffect = slashObj.GetComponent<SlashEffect>();
        if (slashEffect != null)
        {
            slashEffect.Initialize(transform.position, mouseWorldPos);
        }
    }

    // Animation Event functions - 더 이상 사용하지 않음 (SlashEffect로 대체)
    // 애니메이션에 이벤트가 남아있으면 에러 방지를 위해 빈 함수 유지
    public void ActivateHitbox()
    {
        // SlashEffect로 대체됨
    }

    public void DeactivateHitbox()
    {
        // SlashEffect로 대체됨
    }

    // Track when enemies enter/exit collision with player's hurtbox only
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only add enemies that touch the player's main body collider
        if (other.CompareTag("Enemy"))
        {
            enemiesInContact.Add(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemiesInContact.Remove(other);
        }
    }

    public void TakeDamage(int damage)
    {
        // Check if player is defending
        if (isDefending)
        {
            return;
        }

        // Check if player is invincible
        if (invincibilityTimer > 0)
        {
            return;
        }

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Start invincibility period with blink effect
            invincibilityTimer = invincibilityDuration;
        }
    }

    private void Die()
    {
        currentHealth = 0;

        // Stop all movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Reset movement input
        moveInput = Vector2.zero;

        // Trigger death animation
        if (animator != null)
        {
            animator.SetTrigger("Die");
            animator.SetFloat("MoveSpeed", 0);
        }

        // Disable player controls
        enabled = false;

        // Optional: Restart level after delay
        // Invoke(nameof(RestartLevel), 2f);
    }

    // Optional: Add this if you want to restart the level
    // private void RestartLevel()
    // {
    //     UnityEngine.SceneManagement.SceneManager.LoadScene(
    //         UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
    //     );
    // }

    // Public method to apply stun effect
    public void ApplyStun(float duration)
    {
        if (duration <= 0f) return;

        isStunned = true;
        stunTimer = duration;

        // Stop movement immediately
        rb.linearVelocity = Vector2.zero;

        // Force color update immediately
        if (spriteRenderer != null)
        {
            Debug.Log($"[ApplyStun] Player stunned for {duration}s! Setting color to: {stunColor}");
            spriteRenderer.color = stunColor;
        }
        else
        {
            Debug.LogError("[ApplyStun] SpriteRenderer is NULL!");
        }
    }

    // Fireball skill methods
    private void TryFireball()
    {
        // Check cooldown
        if (Time.time - lastFireballTime < fireballCooldown)
        {
            Debug.Log("Fireball on cooldown!");
            return;
        }

        // Check if skill is unlocked
        if (!hasFireballSkill)
        {
            Debug.Log("Fireball skill not unlocked!");
            return;
        }

        // TODO: Check mana cost if mana system is implemented
        // if (currentMana < fireballManaCost) return;

        CastFireball();
    }

    private void CastFireball()
    {
        Debug.Log("Fireball cast!");
        lastFireballTime = Time.time;

        // TODO: Consume mana if mana system is implemented
        // currentMana -= fireballManaCost;

        // Trigger cooldown in UI
        if (SkillUIManager.Instance != null)
        {
            SkillUIManager.Instance.TriggerCooldown(SkillUIManager.SkillType.Fireball, fireballCooldown);
        }

        // Show aura effect for 1 second
        if (auraEffect != null)
        {
            auraEffect.SetActive(true);
            StartCoroutine(HideAuraAfterDelay(1f));
        }

        // Trigger FireAttack1 animation
        if (animator != null)
        {
            animator.SetTrigger("FireAttack1");
        }

        // Start coroutine for burst fire
        StartCoroutine(FireballBurst());
    }

    private System.Collections.IEnumerator FireballBurst()
    {
        // Get mouse world position for direction (capture once at start)
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
        mouseWorldPos.z = 0;
        Vector2 direction = (mouseWorldPos - transform.position).normalized;

        // Fire multiple fireballs with delay
        for (int i = 0; i < fireballCount; i++)
        {
            // Spawn fireball
            if (fireballPrefab != null)
            {
                // Spawn slightly away from player to avoid self-collision
                Vector3 spawnPos = transform.position + (Vector3)direction * 0.5f;
                GameObject fireballObj = Instantiate(fireballPrefab, spawnPos, Quaternion.identity);

                // Initialize fireball
                FireballProjectile fireball = fireballObj.GetComponent<FireballProjectile>();
                if (fireball != null)
                {
                    fireball.Initialize(direction);
                }

                Debug.Log($"Fireball {i + 1}/{fireballCount} fired!");
            }
            else
            {
                Debug.LogWarning("Fireball prefab is not assigned!");
                yield break;
            }

            // Wait before next fireball (skip delay on last shot)
            if (i < fireballCount - 1)
            {
                yield return new WaitForSeconds(fireballBurstDelay);
            }
        }
    }

    // Explosion skill methods
    private void TryExplosion()
    {
        // Check cooldown
        if (Time.time - lastExplosionTime < explosionCooldown)
        {
            Debug.Log("Explosion on cooldown!");
            return;
        }

        // Check if skill is unlocked
        if (!hasExplosionSkill)
        {
            Debug.Log("Explosion skill not unlocked!");
            return;
        }

        // TODO: Check mana cost if mana system is implemented
        // if (currentMana < explosionManaCost) return;

        CastExplosion();
    }

    private void CastExplosion()
    {
        Debug.Log("Arcane Explosion cast!");
        lastExplosionTime = Time.time;

        // TODO: Consume mana if mana system is implemented
        // currentMana -= explosionManaCost;

        // Trigger cooldown in UI
        if (SkillUIManager.Instance != null)
        {
            SkillUIManager.Instance.TriggerCooldown(SkillUIManager.SkillType.Explosion, explosionCooldown);
        }

        // Show aura effect and hide after explosion delay
        if (auraEffect != null)
        {
            auraEffect.SetActive(true);
            StartCoroutine(HideAuraAfterDelay(explosionDelay));
        }

        // Trigger FireAttack2 animation
        if (animator != null)
        {
            animator.SetTrigger("FireAttack2");
        }

        // Get mouse world position for explosion location
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
        mouseWorldPos.z = 0;

        // Spawn explosion at mouse position
        if (explosionPrefab != null)
        {
            GameObject explosionObj = Instantiate(explosionPrefab, mouseWorldPos, Quaternion.identity);

            // Initialize explosion (ArcaneBurstEffect)
            ArcaneBurstEffect explosion = explosionObj.GetComponent<ArcaneBurstEffect>();
            if (explosion != null)
            {
                explosion.Initialize(explosionDelay, explosionRadius, explosionDamage, playerCast: true);
                Debug.Log($"Explosion created at {mouseWorldPos} - Delay: {explosionDelay}s, Radius: {explosionRadius}, Damage: {explosionDamage}");
            }
        }
        else
        {
            Debug.LogWarning("Explosion prefab is not assigned!");
        }
    }

    private System.Collections.IEnumerator HideAuraAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (auraEffect != null)
        {
            auraEffect.SetActive(false);
        }
    }

    // Public method to unlock fireball skill (called by FireballSkillItem)
    public void UnlockFireballSkill()
    {
        hasFireballSkill = true;
        Debug.Log("Fireball skill unlocked! Press E Key to cast.");

        // Add to skill UI
        if (SkillUIManager.Instance != null)
        {
            SkillUIManager.Instance.AddSkill(SkillUIManager.SkillType.Fireball, fireballCooldown);
        }
    }

    // Public method to unlock explosion skill (called by ExplosionSkillItem)
    public void UnlockExplosionSkill()
    {
        hasExplosionSkill = true;
        Debug.Log("Arcane Explosion skill unlocked! Press Q to cast at mouse position.");

        // Add to skill UI
        if (SkillUIManager.Instance != null)
        {
            SkillUIManager.Instance.AddSkill(SkillUIManager.SkillType.Explosion, explosionCooldown);
        }
    }

    // Defense skill methods
    private void TryDefense()
    {
        // Check cooldown
        if (Time.time - lastDefenseTime < defenseCooldown)
        {
            Debug.Log($"Defense on cooldown! {(defenseCooldown - (Time.time - lastDefenseTime)):F1}s remaining");
            return;
        }

        // Check if skill is unlocked
        if (!hasDefenseSkill)
        {
            Debug.Log("Defense skill not unlocked!");
            return;
        }

        StartDefense();
    }

    private void StartDefense()
    {
        Debug.Log("Slime Defense activated!");
        lastDefenseTime = Time.time;
        isDefending = true;
        defenseTimer = defenseDuration;

        // Trigger cooldown in UI
        if (SkillUIManager.Instance != null)
        {
            SkillUIManager.Instance.TriggerCooldown(SkillUIManager.SkillType.Defense, defenseCooldown);
        }

        // Show defense effect
        if (defenseEffect != null)
        {
            defenseEffect.SetActive(true);
        }

        Debug.Log($"Defense active for {defenseDuration}s - blocking all damage");
    }

    private void EndDefense()
    {
        if (!isDefending) return;

        Debug.Log("Defense ended!");
        isDefending = false;
        defenseTimer = 0f;

        // Hide defense effect
        if (defenseEffect != null)
        {
            defenseEffect.SetActive(false);
        }

        // Color will be updated by UpdateVisualEffects()
    }

    // Public method to unlock defense skill (called by SlimeDefenseSkillItem)
    public void UnlockDefenseSkill()
    {
        hasDefenseSkill = true;
        Debug.Log("Slime Defense skill unlocked! Press Space Bar to defend (max 2s, cooldown 5s).");

        // Add to skill UI
        if (SkillUIManager.Instance != null)
        {
            SkillUIManager.Instance.AddSkill(SkillUIManager.SkillType.Defense, defenseCooldown);
        }
    }

    // Dash skill methods
    private void TryDash()
    {
        // Check cooldown
        if (Time.time - lastDashTime < dashCooldown)
        {
            Debug.Log($"Dash on cooldown! {(dashCooldown - (Time.time - lastDashTime)):F1}s remaining");
            return;
        }

        // Check if skill is unlocked
        if (!hasDashSkill)
        {
            Debug.Log("Dash skill not unlocked!");
            return;
        }

        // Cannot dash while defending
        if (isDefending)
        {
            Debug.Log("Cannot dash while defending!");
            return;
        }

        // Start dash
        lastDashTime = Time.time;
        isDashing = true;
        dashTimer = dashDuration;
        dashDirection = moveInput.normalized;

        // Trigger cooldown in UI
        if (SkillUIManager.Instance != null)
        {
            SkillUIManager.Instance.TriggerCooldown(SkillUIManager.SkillType.Dash, dashCooldown);
        }

        // Show dash effect (like aura) and rotate it to match dash direction
        if (dashEffect != null)
        {
            dashEffect.SetActive(true);

            // Calculate rotation angle from direction vector
            // Add -90 if sprite faces up by default, 0 if sprite faces right by default
            float angle = Mathf.Atan2(dashDirection.y, dashDirection.x) * Mathf.Rad2Deg - 90f;
            dashEffect.transform.rotation = Quaternion.Euler(0, 0, angle);

            Debug.Log($"Dash effect rotated to {angle} degrees");
        }

        // Grant temporary invincibility during dash
        invincibilityTimer = dashDuration;

        Debug.Log($"Dash started! Direction: {dashDirection}, Duration: {dashDuration}s");
    }

    private void EndDash()
    {
        if (!isDashing) return;

        Debug.Log("Dash ended!");
        isDashing = false;
        dashTimer = 0f;

        // Hide dash effect
        if (dashEffect != null)
        {
            dashEffect.SetActive(false);
        }
    }

    // Public method to unlock dash skill (called by WerewolfDashSkillItem)
    public void UnlockDashSkill()
    {
        hasDashSkill = true;
        Debug.Log("Werewolf Dash skill unlocked! Press Right Mouse Button while moving to dash (cooldown 2s).");

        // Add to skill UI
        if (SkillUIManager.Instance != null)
        {
            SkillUIManager.Instance.AddSkill(SkillUIManager.SkillType.Dash, dashCooldown);
        }
    }

    // Update visual effects based on current state (priority order)
    private void UpdateVisualEffects()
    {
        if (spriteRenderer == null)
        {
            Debug.LogWarning("SpriteRenderer is null!");
            return;
        }

        string currentState = "normal";
        Color targetColor = Color.white;

        // Priority 1: Stun effect (blue tint)
        if (isStunned)
        {
            spriteRenderer.color = stunColor;
            currentState = "stun";
            if (lastVisualState != currentState)
            {
                Debug.Log($"[Visual] Changed to STUN - Color: {stunColor}, isStunned: {isStunned}, stunTimer: {stunTimer}");
                lastVisualState = currentState;
            }
            return;
        }

        // Priority 2: Defense effect (green tint, full alpha)
        if (isDefending)
        {
            spriteRenderer.color = defenseColor;
            currentState = "defense";
            if (lastVisualState != currentState)
            {
                Debug.Log($"[Visual] Changed to DEFENSE - Color: {defenseColor}, isDefending: {isDefending}, defenseTimer: {defenseTimer}");
                lastVisualState = currentState;
            }
            return;
        }

        // Priority 3: Invincibility blink effect
        if (invincibilityTimer > 0)
        {
            float alpha = Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed));
            Color color = Color.white;
            color.a = alpha;
            spriteRenderer.color = color;
            currentState = "invincible";
            if (lastVisualState != currentState)
            {
                Debug.Log($"[Visual] Changed to INVINCIBLE - invincibilityTimer: {invincibilityTimer}");
                lastVisualState = currentState;
            }
            return;
        }

        // Default: Normal color
        if (lastVisualState != "normal")
        {
            Debug.Log("[Visual] Changed to NORMAL");
            lastVisualState = "normal";
        }
        spriteRenderer.color = Color.white;
    }

    // LateUpdate to apply colors after Animator updates
    private void LateUpdate()
    {
        // Re-apply visual effects after Animator has updated
        // This ensures colors are not overwritten by animation
        if (spriteRenderer == null) return;

        if (isStunned)
        {
            spriteRenderer.color = stunColor;
        }
        else if (isDefending)
        {
            spriteRenderer.color = defenseColor;
        }
        else if (invincibilityTimer > 0)
        {
            float alpha = Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed));
            Color color = Color.white;
            color.a = alpha;
            spriteRenderer.color = color;
        }
        else
        {
            spriteRenderer.color = Color.white;
        }
    }

    // Optional: Visualize movement in editor
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && moveInput.magnitude > 0.1f)
        {
            Gizmos.color = Color.blue;
            Vector2 direction = moveInput;
            Gizmos.DrawRay(transform.position, direction * 2f);
        }
    }
}
