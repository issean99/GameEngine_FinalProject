using UnityEngine;

/// <summary>
/// Health item that restores player health when picked up
/// 플레이어가 획득하면 체력을 회복시켜주는 체력 아이템
/// </summary>
public class HealthItem : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int healAmount = 30; // 회복량
    [SerializeField] private string itemName = "Health Potion";

    [Header("Animation Settings")]
    [SerializeField] private float floatHeight = 0.5f;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private bool enableRotation = true;
    [SerializeField] private float rotationSpeed = 90f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject pickupEffectPrefab;
    [SerializeField] private bool pulseEffect = true;
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private float pulseMin = 0.9f;
    [SerializeField] private float pulseMax = 1.1f;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] [Range(0f, 1f)] private float volume = 0.8f;

    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private bool isPickedUp = false;
    private AudioSource audioSource;

    private void Start()
    {
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        // Ensure we have a trigger collider for pickup detection
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f; // Pickup radius
            Debug.Log("[HealthItem] Added CircleCollider2D automatically");
        }
        collider.isTrigger = true; // Ensure it's a trigger

        // Add AudioSource for sound effects
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
        audioSource.volume = volume;
    }

    private void Update()
    {
        if (isPickedUp) return;

        // Floating animation
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Rotation
        if (enableRotation)
        {
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }

        // Pulse effect
        if (pulseEffect && spriteRenderer != null)
        {
            float scale = Mathf.Lerp(pulseMin, pulseMax, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
            transform.localScale = Vector3.one * scale;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isPickedUp) return;

        Debug.Log($"[HealthItem] OnTriggerEnter2D with: {collision.gameObject.name}, Tag: {collision.tag}");

        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                Debug.Log("[HealthItem] Player detected, picking up!");
                OnPickup(player);
            }
            else
            {
                Debug.LogWarning("[HealthItem] Object has Player tag but no PlayerController component!");
            }
        }
    }

    private void OnPickup(PlayerController player)
    {
        isPickedUp = true;

        // Heal the player
        int currentHealth = player.CurrentHealth;
        int maxHealth = player.MaxHealth;
        int actualHealAmount = Mathf.Min(healAmount, maxHealth - currentHealth);

        if (actualHealAmount > 0)
        {
            player.Heal(healAmount);
            Debug.Log($"[HealthItem] Player healed for {actualHealAmount} HP. Current HP: {player.CurrentHealth}/{player.MaxHealth}");
        }
        else
        {
            Debug.Log($"[HealthItem] Player already at full health!");
        }

        // Play pickup sound
        if (pickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound, volume);
        }

        // Spawn pickup effect
        if (pickupEffectPrefab != null)
        {
            GameObject effect = Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);

            // Auto-destroy effect after 2 seconds if it doesn't have AutoDestroy script
            if (effect.GetComponent<AutoDestroy>() == null)
            {
                Destroy(effect, 2f);
            }
        }

        // Hide sprite immediately
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        // Destroy item (delayed to allow sound to play)
        Destroy(gameObject, pickupSound != null ? 0.5f : 0f);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw pickup range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}
