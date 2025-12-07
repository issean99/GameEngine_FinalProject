using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SkeletonArcherArrow : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private int damage;
    private Rigidbody2D rb;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip launchSound; // 발사 소리
    [SerializeField] private AudioClip hitSound; // 충돌 소리
    [SerializeField] [Range(0f, 1f)] private float volume = 0.7f;

    private AudioSource audioSource;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        // Add AudioSource for sound effects
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
        audioSource.volume = volume;
    }

    public void Initialize(Vector2 dir, float projectileSpeed, int projectileDamage)
    {
        direction = dir.normalized;
        speed = projectileSpeed;
        damage = projectileDamage;

        // Play launch sound
        if (launchSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(launchSound, volume);
        }

        // Set velocity
        rb.linearVelocity = direction * speed;

        // Rotate arrow to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Destroy after some time
        Destroy(gameObject, 10f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Hit player
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // Deal damage only (no stun)
                player.TakeDamage(damage);

                Debug.Log($"Skeleton Arrow hit player! Damage: {damage}");
            }

            // Play hit sound
            if (hitSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hitSound, volume);
            }

            // Destroy arrow (delayed if sound is playing)
            Destroy(gameObject, hitSound != null ? 0.5f : 0f);
        }
        // Hit walls or obstacles
        else if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            // Play hit sound
            if (hitSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hitSound, volume);
            }

            // Destroy arrow (delayed if sound is playing)
            Destroy(gameObject, hitSound != null ? 0.5f : 0f);
        }
    }
}
