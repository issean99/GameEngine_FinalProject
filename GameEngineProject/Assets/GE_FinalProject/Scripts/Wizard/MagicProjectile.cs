using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MagicProjectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private int damage;
    private Rigidbody2D rb;

    [Header("Projectile Settings")]
    [SerializeField] private float lifetime = 5f; // Destroy after 5 seconds
    [SerializeField] private GameObject hitEffectPrefab; // Optional hit effect

    [Header("Sound Effects")]
    [SerializeField] private AudioClip launchSound; // 발사 소리
    [SerializeField] private AudioClip hitSound; // 충돌 소리
    [SerializeField] [Range(0f, 1f)] private float volume = 0.7f;

    private AudioSource audioSource;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Add AudioSource for sound effects
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
        audioSource.volume = volume;
    }

    public void Initialize(Vector2 moveDirection, float moveSpeed, int projectileDamage)
    {
        direction = moveDirection.normalized;
        speed = moveSpeed;
        damage = projectileDamage;

        // Play launch sound
        if (launchSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(launchSound, volume);
        }

        // Set velocity
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }

        // Rotate projectile to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Hit player
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }

            DestroyProjectile();
        }
        // Hit wall or obstacle
        else if (collision.CompareTag("Wall") || collision.CompareTag("Obstacle"))
        {
            DestroyProjectile();
        }
    }

    private void DestroyProjectile()
    {
        // Play hit sound
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound, volume);
        }

        // Spawn hit effect if available
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        // Destroy projectile (delayed if sound is playing)
        Destroy(gameObject, hitSound != null ? 0.5f : 0f);
    }

    // Optional: Draw gizmo to visualize direction in editor
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(transform.position, direction * 2f);
        }
    }
}
