<<<<<<< Updated upstream
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

    [Header("Player Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private float invincibilityDuration = 1f;
    [SerializeField] private float blinkSpeed = 10f;

    [Header("Stun Settings")]
    [SerializeField] private Color stunColor = new Color(0.5f, 0.5f, 1f, 1f); // Bluish tint when stunned

    private Vector2 moveInput;
    private float invincibilityTimer = 0f;
    private bool isSprinting;
    private float lastAttackTime;
    private bool isStunned = false;
    private float stunTimer = 0f;

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
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Get input devices
        keyboard = Keyboard.current;
        mouse = Mouse.current;

        // Initialize health
        currentHealth = maxHealth;
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
                spriteRenderer.color = Color.white; // Reset color
                Debug.Log("Player stun ended");
            }
            else
            {
                // Visual effect during stun (apply stun color)
                spriteRenderer.color = stunColor;
                return; // Don't process input while stunned
            }
        }

        // Update invincibility timer and blink effect
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;

            // Blink effect during invincibility
            float alpha = Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed));
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
        else
        {
            // Reset to full visibility when invincibility ends
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }

        // Check for damage from enemies in contact (경직 상태가 아닌 적만)
        foreach (Collider2D enemyCollider in enemiesInContact)
        {
            if (enemyCollider != null)
            {
                // 일반 적 체크
                Enemy enemy = enemyCollider.GetComponent<Enemy>();
                if (enemy != null && !enemy.IsStaggeredOrDead())
                {
                    TakeDamage(10);
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

        // Check sprint input (Left Shift)
        isSprinting = keyboard.leftShiftKey.isPressed;

        // Check attack input (Left Mouse Button)
        if (mouse.leftButton.wasPressedThisFrame)
        {
            TryAttack();
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
        // Check if player is invincible
        if (invincibilityTimer > 0)
        {
            Debug.Log("Player is invincible!");
            return;
        }

        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage! Current health: {currentHealth}");

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
        Debug.Log("Player died!");
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

        Debug.Log($"Player stunned for {duration} seconds!");
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
=======
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

    [Header("Player Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private float invincibilityDuration = 1f;
    [SerializeField] private float blinkSpeed = 10f;

    [Header("Stun Settings")]
    [SerializeField] private Color stunColor = new Color(0.5f, 0.5f, 1f, 1f); // Bluish tint when stunned

    private Vector2 moveInput;
    private float invincibilityTimer = 0f;
    private bool isSprinting;
    private float lastAttackTime;
    private bool isStunned = false;
    private float stunTimer = 0f;

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
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Get input devices
        keyboard = Keyboard.current;
        mouse = Mouse.current;

        // Initialize health
        currentHealth = maxHealth;
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
                spriteRenderer.color = Color.white; // Reset color
                Debug.Log("Player stun ended");
            }
            else
            {
                // Visual effect during stun (apply stun color)
                spriteRenderer.color = stunColor;
                return; // Don't process input while stunned
            }
        }

        // Update invincibility timer and blink effect
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;

            // Blink effect during invincibility
            float alpha = Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed));
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
        else
        {
            // Reset to full visibility when invincibility ends
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
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

        // Check sprint input (Left Shift)
        isSprinting = keyboard.leftShiftKey.isPressed;

        // Check attack input (Left Mouse Button)
        if (mouse.leftButton.wasPressedThisFrame)
        {
            TryAttack();
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
        // Check if player is invincible
        if (invincibilityTimer > 0)
        {
            Debug.Log("Player is invincible!");
            return;
        }

        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage! Current health: {currentHealth}");

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
        Debug.Log("Player died!");
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

        Debug.Log($"Player stunned for {duration} seconds!");
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
>>>>>>> Stashed changes
