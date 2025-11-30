using System.Collections.Generic;
using UnityEngine;

public class FinalBossAttackHitbox : MonoBehaviour
{
    [Header("Hitbox Settings")]
    [SerializeField] private int damage = 20;
    [SerializeField] private float knockbackForce = 10f;

    private FinalBoss boss;
    private HashSet<Collider2D> hitTargets = new HashSet<Collider2D>();

    private void Awake()
    {
        boss = GetComponentInParent<FinalBoss>();
    }

    private void OnEnable()
    {
        // Clear hit targets when hitbox is activated
        hitTargets.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Only damage if boss is not staggered or dead
        if (boss != null && boss.IsStaggeredOrDead())
        {
            return;
        }

        // Hit player
        if (collision.CompareTag("Player") && !hitTargets.Contains(collision))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);

                // Apply knockback
                Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    Vector2 knockbackDirection = (collision.transform.position - transform.position).normalized;
                    playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
                }

                // Add to hit targets to prevent multiple hits from same attack
                hitTargets.Add(collision);

                Debug.Log($"Final Boss hit player for {damage} damage!");
            }
        }
    }

    // Optional: Set damage dynamically
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    // Optional: Set knockback force dynamically
    public void SetKnockbackForce(float newForce)
    {
        knockbackForce = newForce;
    }

    private void OnDrawGizmos()
    {
        // Visualize hitbox in editor
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            if (col is BoxCollider2D)
            {
                BoxCollider2D boxCol = col as BoxCollider2D;
                Gizmos.DrawWireCube(transform.position + (Vector3)boxCol.offset, boxCol.size);
            }
            else if (col is CircleCollider2D)
            {
                CircleCollider2D circleCol = col as CircleCollider2D;
                Gizmos.DrawWireSphere(transform.position + (Vector3)circleCol.offset, circleCol.radius);
            }
        }
    }
}
