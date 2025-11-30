using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SpearProjectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private int damage;
    private Rigidbody2D rb;

    [Header("Projectile Settings")]
    [SerializeField] private float lifetime = 5f; // Destroy after 5 seconds
    [SerializeField] private GameObject hitEffectPrefab; // Optional hit effect
    [SerializeField] private float knockbackForce = 15f; // Knockback strength

    // Track hit targets to prevent multiple damage to same player
    private HashSet<Collider2D> hitTargets = new HashSet<Collider2D>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 moveDirection, float moveSpeed, int projectileDamage)
    {
        direction = moveDirection.normalized;
        speed = moveSpeed;
        damage = projectileDamage;

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
            // Check if we already hit this target
            if (hitTargets.Contains(collision))
            {
                return; // Already damaged this player, don't destroy
            }

            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);

                // Apply knockback
                Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    Vector2 knockbackDirection = direction.normalized;
                    playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
                }

                // Mark this target as hit
                hitTargets.Add(collision);
            }

            // Don't destroy projectile, it continues moving
        }
        // Hit wall or obstacle
        else if (collision.CompareTag("Wall") || collision.CompareTag("Obstacle"))
        {
            DestroyProjectile();
        }
    }

    private void DestroyProjectile()
    {
        // Spawn hit effect if available
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    // Visualize direction in editor
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, direction * 2f);
        }
    }
}
