using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcaneBurstEffect : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float delayTime = 1f;
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private int damage = 20;

    private bool hasExploded = false;
    private bool isInitialized = false;

    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer warningMarker;
    [SerializeField] private SpriteRenderer explosionEffect;
    [SerializeField] private float warningPulseSpeed = 3f;

    private HashSet<Collider2D> damagedTargets = new HashSet<Collider2D>();

    private void Awake()
    {
        // Get sprite renderers if attached
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        if (renderers.Length >= 2)
        {
            warningMarker = renderers[0];
            explosionEffect = renderers[1];
        }
        else if (renderers.Length == 1)
        {
            warningMarker = renderers[0];
        }

        // Hide explosion effect initially
        if (explosionEffect != null)
        {
            explosionEffect.enabled = false;
        }

        Debug.Log($"ArcaneBurst Awake: warningMarker={warningMarker != null}, explosionEffect={explosionEffect != null}");
    }

    private void Start()
    {
        // Auto-start if not initialized manually
        if (!isInitialized)
        {
            Debug.Log("ArcaneBurst auto-starting with default values");
            Initialize(delayTime, explosionRadius, damage);
        }
    }

    public void Initialize(float delay, float radius, int damageAmount)
    {
        isInitialized = true;
        delayTime = delay;
        explosionRadius = radius;
        damage = damageAmount;

        Debug.Log($"ArcaneBurst initialized: delay={delay}s, radius={radius}, damage={damageAmount}");

        // Start warning phase
        StartCoroutine(WarningPhase());
    }

    private IEnumerator WarningPhase()
    {
        float elapsedTime = 0f;

        // Show pulsing warning marker
        while (elapsedTime < delayTime)
        {
            if (warningMarker != null)
            {
                // Pulse effect
                float alpha = Mathf.Lerp(0.3f, 1f, Mathf.PingPong(Time.time * warningPulseSpeed, 1f));
                Color color = warningMarker.color;
                color.a = alpha;
                warningMarker.color = color;

                // Scale pulse
                float scale = Mathf.Lerp(0.8f, 1.2f, Mathf.PingPong(Time.time * warningPulseSpeed, 1f));
                warningMarker.transform.localScale = Vector3.one * scale * explosionRadius;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Trigger explosion
        Explode();
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        Debug.Log($"Arcane Burst exploded at {transform.position} with radius {explosionRadius}");

        // Hide warning marker
        if (warningMarker != null)
        {
            warningMarker.enabled = false;
        }

        // Show explosion effect
        if (explosionEffect != null)
        {
            explosionEffect.enabled = true;
            explosionEffect.transform.localScale = Vector3.one * explosionRadius;
        }

        // Detect and damage all players in radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player") && !damagedTargets.Contains(hit))
            {
                PlayerController player = hit.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                    damagedTargets.Add(hit);
                    Debug.Log($"Arcane Burst hit player for {damage} damage!");
                }
            }
        }

        // Destroy after explosion animation
        Destroy(gameObject, 0.5f);
    }

    // Visualize explosion radius in editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, explosionRadius);
    }
}
