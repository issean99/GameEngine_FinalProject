using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttackHitbox : MonoBehaviour
{
    [SerializeField] private int damage = 20;
    private HashSet<Collider2D> hitTargets = new HashSet<Collider2D>();

    private void OnEnable()
    {
        // Clear hit targets when hitbox is activated
        hitTargets.Clear();

        // Check for overlapping enemies immediately when enabled
        StartCoroutine(CheckOverlapOnEnable());
    }

    private IEnumerator CheckOverlapOnEnable()
    {
        // Wait one frame for physics to update
        yield return new WaitForFixedUpdate();

        // Get the BoxCollider2D or CircleCollider2D size
        Collider2D myCollider = GetComponent<Collider2D>();
        if (myCollider == null) yield break;

        // Check for overlaps
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.SetLayerMask(Physics2D.AllLayers);

        List<Collider2D> results = new List<Collider2D>();
        myCollider.Overlap(filter, results);

        foreach (Collider2D other in results)
        {
            if (other.CompareTag("Player"))
            {
                TryDamagePlayer(other);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TryDamagePlayer(other);
        }
    }

    private void TryDamagePlayer(Collider2D playerCollider)
    {
        // Prevent hitting the same player multiple times
        if (hitTargets.Contains(playerCollider))
            return;

        PlayerController player = playerCollider.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(damage);
            hitTargets.Add(playerCollider);
            Debug.Log($"Boss hitbox dealt {damage} damage to player!");
        }
    }

    private void OnDisable()
    {
        // Clear hit targets when disabled
        hitTargets.Clear();
    }
}
