using UnityEngine;

/// <summary>
/// Wall component that blocks and destroys projectiles
/// 투사체를 막고 파괴하는 벽 컴포넌트
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ProjectileBlockerWall : MonoBehaviour
{
    [Header("Wall Settings")]
    [Tooltip("모든 투사체를 막을지 여부 (false면 특정 태그만 막음)")]
    [SerializeField] private bool blockAllProjectiles = true;

    [Header("Projectile Tags to Block")]
    [Tooltip("막을 투사체의 태그 목록 (blockAllProjectiles가 false일 때만 사용)")]
    [SerializeField] private string[] projectileTagsToBlock = new string[]
    {
        "Projectile",
        "EnemyProjectile",
        "PlayerProjectile"
    };

    [Header("Visual Effects")]
    [Tooltip("투사체가 벽에 부딪힐 때 생성할 이펙트 (선택사항)")]
    [SerializeField] private GameObject hitEffectPrefab;

    [Header("Sound Effects")]
    [Tooltip("투사체가 벽에 부딪힐 때 재생할 소리 (선택사항)")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] [Range(0f, 1f)] private float volume = 0.5f;

    private AudioSource audioSource;

    private void Awake()
    {
        // Collider가 Trigger가 아닌지 확인
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && col.isTrigger)
        {
            Debug.LogWarning($"[ProjectileBlockerWall] '{gameObject.name}'의 Collider가 Trigger로 설정되어 있습니다. " +
                           "투사체를 막으려면 Trigger를 해제해야 합니다!");
        }

        // AudioSource 추가 (사운드 이펙트용)
        if (hitSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound
            audioSource.volume = volume;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 충돌한 객체가 투사체인지 확인
        if (IsProjectile(collision.gameObject))
        {
            HandleProjectileHit(collision.gameObject, collision.GetContact(0).point);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Trigger 모드인 경우에도 투사체 감지
        if (IsProjectile(collision.gameObject))
        {
            HandleProjectileHit(collision.gameObject, collision.transform.position);
        }
    }

    private bool IsProjectile(GameObject obj)
    {
        // 모든 투사체를 막는 경우
        if (blockAllProjectiles)
        {
            // 일반적인 투사체 컴포넌트 확인
            return obj.GetComponent<FireballProjectile>() != null ||
                   obj.GetComponent<MagicProjectile>() != null ||
                   obj.GetComponent<SpearProjectile>() != null ||
                   obj.GetComponent<ArrowProjectile>() != null ||
                   obj.GetComponent<SkeletonArcherArrow>() != null;
        }
        else
        {
            // 특정 태그만 막는 경우
            foreach (string tag in projectileTagsToBlock)
            {
                if (obj.CompareTag(tag))
                {
                    return true;
                }
            }
            return false;
        }
    }

    private void HandleProjectileHit(GameObject projectile, Vector2 hitPosition)
    {
        Debug.Log($"[ProjectileBlockerWall] '{gameObject.name}'이(가) 투사체 '{projectile.name}'을(를) 막았습니다!");

        // 히트 이펙트 생성
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, hitPosition, Quaternion.identity);
        }

        // 히트 사운드 재생
        if (hitSound != null && audioSource != null)
        {
            // 벽에서 소리를 재생 (투사체가 파괴되기 전에)
            AudioSource.PlayClipAtPoint(hitSound, hitPosition, volume);
        }

        // 투사체 파괴
        // 투사체 스크립트들은 이미 벽과의 충돌을 처리하므로,
        // 여기서는 혹시 모를 경우를 대비해 강제로 파괴
        Destroy(projectile, 0.1f);
    }

    // Unity Editor에서 기즈모 표시
    private void OnDrawGizmos()
    {
        // 벽의 콜라이더 영역을 시각화
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f); // 반투명 초록색

            if (col is BoxCollider2D boxCol)
            {
                Vector2 size = boxCol.size;
                Vector2 offset = boxCol.offset;
                Vector3 center = transform.position + (Vector3)offset;
                Gizmos.DrawCube(center, new Vector3(size.x, size.y, 0.1f));
            }
            else if (col is CircleCollider2D circleCol)
            {
                Vector3 center = transform.position + (Vector3)circleCol.offset;
                Gizmos.DrawSphere(center, circleCol.radius);
            }
        }
    }
}
