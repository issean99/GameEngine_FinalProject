using UnityEngine;

public class SkeletonGroup : MonoBehaviour
{
    [Header("Group Detection")]
    [SerializeField] private float groupDetectionRange = 8f; // 그룹 전체의 감지 범위

    private bool isGroupAlerted = false; // 그룹이 플레이어를 감지했는지
    private Transform player;

    private void Awake()
    {
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void Update()
    {
        // 그룹이 아직 경보 상태가 아니고 플레이어가 범위 안에 들어오면 경보 발동
        if (!isGroupAlerted && player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= groupDetectionRange)
            {
                AlertGroup();
            }
        }
    }

    // 그룹 전체를 경보 상태로 만듦
    public void AlertGroup()
    {
        if (isGroupAlerted) return;

        isGroupAlerted = true;
        Debug.Log($"Skeleton Group {gameObject.name} has been alerted!");
    }

    // 그룹이 경보 상태인지 확인
    public bool IsGroupAlerted()
    {
        return isGroupAlerted;
    }

    // 그룹 감지 범위 시각화
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, groupDetectionRange);
    }
}
