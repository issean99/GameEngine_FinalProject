using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ArrowProjectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private int damage;
    private float stunDuration;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    public void Initialize(Vector2 dir, float projectileSpeed, int projectileDamage, float stun)
    {
        direction = dir.normalized;
        speed = projectileSpeed;
        damage = projectileDamage;
        stunDuration = stun;

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
                // Deal damage
                player.TakeDamage(damage);

                // Apply stun effect
                if (stunDuration > 0f)
                {
                    player.ApplyStun(stunDuration);
                }

                Debug.Log($"Arrow hit player! Damage: {damage}, Stun: {stunDuration}s");
            }

            Destroy(gameObject);
        }
        // Hit walls or obstacles
        else if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}
