using UnityEngine;

/// <summary>
/// Base class for skill items that can be picked up by the player
/// 플레이어가 획득할 수 있는 스킬 아이템 베이스 클래스
/// </summary>
public class SkillItem : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] protected string skillName = "Unknown Skill";
    [SerializeField] protected float floatHeight = 0.5f;
    [SerializeField] protected float floatSpeed = 2f;
    [SerializeField] protected bool enableRotation = false; // 회전 여부
    [SerializeField] protected float rotationSpeed = 90f;

    [Header("Visual Effects")]
    [SerializeField] protected GameObject pickupEffectPrefab;
    [SerializeField] protected bool pulseEffect = false; // 펄스 효과 (크기 변화)
    [SerializeField] protected float pulseSpeed = 3f;
    [SerializeField] protected float pulseMin = 0.8f;
    [SerializeField] protected float pulseMax = 1.2f;

    protected SpriteRenderer spriteRenderer;
    protected Vector3 startPosition;
    protected bool isPickedUp = false;

    protected virtual void Start()
    {
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    protected virtual void Update()
    {
        if (isPickedUp) return;

        // Floating animation
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Rotation (옵션이 켜져 있을 때만)
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

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (isPickedUp) return;

        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                OnPickup(player);
            }
        }
    }

    protected virtual void OnPickup(PlayerController player)
    {
        isPickedUp = true;

        Debug.Log($"Player picked up: {skillName}");

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

        // Grant skill to player (override in derived classes)
        GrantSkill(player);

        // Destroy item
        Destroy(gameObject);
    }

    /// <summary>
    /// Override this in derived classes to grant specific skills
    /// </summary>
    protected virtual void GrantSkill(PlayerController player)
    {
        // Base implementation - to be overridden
        Debug.LogWarning($"GrantSkill not implemented for {skillName}");
    }

    private void OnDrawGizmosSelected()
    {
        // Draw pickup range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}