using System.Collections.Generic;
using UnityEngine;

public class SlashEffect : MonoBehaviour
{
    [Header("Slash Settings")]
    [SerializeField] private float slashDuration = 0.3f; // 애니메이션 지속 시간
    [SerializeField] private float offsetDistance = 1f; // 플레이어로부터의 거리
    [SerializeField] private int damage = 20; // 공격 데미지
    [SerializeField] private float attackRadius = 1.5f; // 공격 범위 반경
    [SerializeField] private LayerMask enemyLayer; // Enemy 레이어

    private float spawnTime;
    private SpriteRenderer spriteRenderer;
    private HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>(); // 이미 맞은 적 추적
    private bool hasDealtDamage = false; // 데미지를 한 번만 주기 위한 플래그

    private void Start()
    {
        spawnTime = Time.time;

        // 자식 오브젝트에서 SpriteRenderer 찾기
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // 즉시 범위 내 적 감지 및 공격
        DetectAndAttackEnemies();
    }

    private void Update()
    {
        // 애니메이션이 끝나면 자동으로 파괴
        if (Time.time - spawnTime >= slashDuration)
        {
            Destroy(gameObject);
        }

        // 지속적으로 범위 내 적 감지 (이미 맞은 적 제외)
        if (!hasDealtDamage)
        {
            DetectAndAttackEnemies();
        }
    }

    // 외부에서 위치와 방향을 설정하는 함수
    public void Initialize(Vector3 playerPosition, Vector3 mousePosition)
    {
        // 플레이어에서 마우스로의 방향 벡터 계산
        Vector2 direction = (mousePosition - playerPosition).normalized;

        // 플레이어 위치에서 약간 떨어진 곳에 배치
        transform.position = playerPosition + (Vector3)(direction * offsetDistance);

        // 방향에 맞게 회전
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 왼쪽을 향하는지 확인 (각도가 90도보다 크고 270도보다 작으면 왼쪽)
        bool isFacingLeft = angle > 90f || angle < -90f;

        if (isFacingLeft)
        {
            // 왼쪽을 향할 때: X축으로 180도 회전하여 위아래 반전
            transform.rotation = Quaternion.Euler(180, 0, -angle);

            // SpriteRenderer가 자식에 있는 경우를 위해 추가 처리
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }
        else
        {
            // 오른쪽을 향할 때: 정상 회전
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    // 범위 내 적 감지 및 공격
    private void DetectAndAttackEnemies()
    {
        // 원형 범위로 적 탐지
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRadius, enemyLayer);

        Debug.Log($"SlashEffect detected {hits.Length} enemies in range");

        foreach (Collider2D hit in hits)
        {
            // 이미 맞은 적은 제외
            if (hitEnemies.Contains(hit))
                continue;

            // Enemy 태그 확인
            if (hit.CompareTag("Enemy"))
            {
                hitEnemies.Add(hit);

                // 일반 적에게 데미지
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    Debug.Log($"Slash hit {hit.name} for {damage} damage!");
                    hasDealtDamage = true;
                    continue;
                }

                // Wizard 보스에게 데미지
                WizardBoss wizardBoss = hit.GetComponent<WizardBoss>();
                if (wizardBoss != null)
                {
                    wizardBoss.TakeDamage(damage);
                    Debug.Log($"Slash hit Wizard boss for {damage} damage!");
                    hasDealtDamage = true;
                    continue;
                }

                // Final 보스에게 데미지
                FinalBoss finalBoss = hit.GetComponent<FinalBoss>();
                if (finalBoss != null)
                {
                    finalBoss.TakeDamage(damage);
                    Debug.Log($"Slash hit Final boss for {damage} damage!");
                    hasDealtDamage = true;
                }
            }
        }
    }

    // 공격 범위 시각화 (디버깅용)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
