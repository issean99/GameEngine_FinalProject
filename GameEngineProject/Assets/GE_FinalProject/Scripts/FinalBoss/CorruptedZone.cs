using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorruptedZone : MonoBehaviour
{
    [Header("Zone Settings")]
    [SerializeField] private float damageInterval = 1f; // Damage every second
    private float duration;
    private int damage;

    [Header("Visual Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private Color zoneColor = new Color(0.5f, 0f, 0.8f, 0.5f); // Purple transparent

    private SpriteRenderer spriteRenderer;
    private CircleCollider2D zoneCollider;
    private HashSet<PlayerController> playersInZone = new HashSet<PlayerController>();
    private float damageTimer = 0f;
    private bool isActive = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        zoneCollider = GetComponent<CircleCollider2D>();

        // Ensure collider is a trigger
        if (zoneCollider != null)
        {
            zoneCollider.isTrigger = true;
        }

        // Start with transparent sprite
        if (spriteRenderer != null)
        {
            Color initialColor = zoneColor;
            initialColor.a = 0f;
            spriteRenderer.color = initialColor;
        }
    }

    public void Initialize(float zoneDuration, int zoneDamage)
    {
        duration = zoneDuration;
        damage = zoneDamage;

        StartCoroutine(ZoneLifecycle());
    }

    private IEnumerator ZoneLifecycle()
    {
        // Fade in
        yield return StartCoroutine(FadeIn());

        // Activate zone
        isActive = true;
        damageTimer = damageInterval; // Start damage timer

        // Wait for duration
        yield return new WaitForSeconds(duration);

        // Deactivate and fade out
        isActive = false;
        yield return StartCoroutine(FadeOut());

        // Destroy zone
        Destroy(gameObject);
    }

    private IEnumerator FadeIn()
    {
        if (spriteRenderer == null) yield break;

        float elapsed = 0f;
        Color startColor = spriteRenderer.color;
        Color targetColor = zoneColor;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, zoneColor.a, elapsed / fadeInDuration);

            Color newColor = targetColor;
            newColor.a = alpha;
            spriteRenderer.color = newColor;

            yield return null;
        }

        spriteRenderer.color = targetColor;
    }

    private IEnumerator FadeOut()
    {
        if (spriteRenderer == null) yield break;

        float elapsed = 0f;
        Color startColor = spriteRenderer.color;
        Color targetColor = zoneColor;
        targetColor.a = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(zoneColor.a, 0f, elapsed / fadeOutDuration);

            Color newColor = zoneColor;
            newColor.a = alpha;
            spriteRenderer.color = newColor;

            yield return null;
        }

        spriteRenderer.color = targetColor;
    }

    private void Update()
    {
        if (!isActive) return;

        // Damage timer
        damageTimer -= Time.deltaTime;

        if (damageTimer <= 0f)
        {
            // Deal damage to all players in zone
            foreach (PlayerController player in playersInZone)
            {
                if (player != null)
                {
                    player.TakeDamage(damage);
                }
            }

            // Reset timer
            damageTimer = damageInterval;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                playersInZone.Add(player);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                playersInZone.Remove(player);
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Draw zone radius in editor
        Gizmos.color = new Color(0.5f, 0f, 0.8f, 0.3f);

        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col != null)
        {
            Gizmos.DrawWireSphere(transform.position, col.radius);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, 1.5f); // Default radius
        }
    }
}
