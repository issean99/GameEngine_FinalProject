using UnityEngine;

/// <summary>
/// Player's fireball projectile - obtained from Wizard Boss
/// 마법사 보스에게서 얻은 플레이어의 파이어볼 투사체
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class FireballProjectile : MonoBehaviour
{
    [Header("Projectile Properties")]
    [SerializeField] private int damage = 30;
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 5f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject trailEffectPrefab;
    [SerializeField] private bool rotateWhileFlying = false; // 날아가는 동안 회전 여부
    [SerializeField] private float rotationSpeed = 360f;

    private Rigidbody2D rb;
    private Vector2 direction;
    private bool hasHit = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 shootDirection, float projectileSpeed = -1, int projectileDamage = -1)
    {
        direction = shootDirection.normalized;

        // Use custom values if provided, otherwise use defaults
        if (projectileSpeed > 0)
            speed = projectileSpeed;
        if (projectileDamage > 0)
            damage = projectileDamage;

        // Set velocity
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }

        // Rotate to face direction (한 번만 설정, 이후 회전 안 함)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Spawn trail effect
        if (trailEffectPrefab != null)
        {
            GameObject trail = Instantiate(trailEffectPrefab, transform.position, Quaternion.identity);
            trail.transform.SetParent(transform);
        }

        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // 회전 옵션이 켜져 있을 때만 회전 (기본: 꺼짐)
        if (rotateWhileFlying)
        {
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;

        // Hit enemies
        if (collision.CompareTag("Enemy"))
        {
            // Try different enemy types
            var slime = collision.GetComponent<SlimeController>();
            if (slime != null)
            {
                slime.TakeDamage(damage);
                OnHit();
                return;
            }

            var skeleton = collision.GetComponent<SkeletonController>();
            if (skeleton != null)
            {
                skeleton.TakeDamage(damage);
                OnHit();
                return;
            }

            var archer = collision.GetComponent<SkeletonArcherController>();
            if (archer != null)
            {
                archer.TakeDamage(damage);
                OnHit();
                return;
            }

            var werewolf = collision.GetComponent<WereWolfController>();
            if (werewolf != null)
            {
                werewolf.TakeDamage(damage);
                OnHit();
                return;
            }

            var wizardBoss = collision.GetComponent<WizardBoss>();
            if (wizardBoss != null)
            {
                wizardBoss.TakeDamage(damage);
                OnHit();
                return;
            }

            var finalBoss = collision.GetComponent<FinalBoss>();
            if (finalBoss != null)
            {
                finalBoss.TakeDamage(damage);
                OnHit();
                return;
            }
        }

        // Hit walls/obstacles (not player)
        if (!collision.CompareTag("Player") && !collision.isTrigger)
        {
            OnHit();
        }
    }

    private void OnHit()
    {
        if (hasHit) return;
        hasHit = true;

        // Stop movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Spawn hit effect
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        // Destroy projectile
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        // Draw direction ray
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, direction * 2f);
        }
    }
}
