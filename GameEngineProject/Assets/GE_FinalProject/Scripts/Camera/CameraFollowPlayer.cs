using UnityEngine;

/// <summary>
/// Main Camera가 플레이어를 부드럽게 따라가도록 하는 스크립트
/// Cinemachine 없이 사용 가능
/// </summary>
public class CameraFollowPlayer : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target; // 따라갈 타겟 (플레이어)
    [SerializeField] private bool findPlayerOnStart = true; // 시작 시 자동으로 플레이어 찾기
    [SerializeField] private string playerTag = "Player";

    [Header("Follow Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f); // 카메라 오프셋
    [SerializeField] private float smoothSpeed = 0.125f; // 부드러운 따라가기 속도 (낮을수록 부드럽지만 느림)
    [SerializeField] private bool smoothFollow = true; // 부드럽게 따라갈지 여부

    [Header("Boundaries (Optional)")]
    [SerializeField] private bool useBoundaries = false;
    [SerializeField] private float minX = -100f;
    [SerializeField] private float maxX = 100f;
    [SerializeField] private float minY = -100f;
    [SerializeField] private float maxY = 100f;

    private void Start()
    {
        // 자동으로 플레이어 찾기
        if (findPlayerOnStart && target == null)
        {
            FindPlayer();
        }
    }

    private void LateUpdate()
    {
        // 타겟이 없으면 찾기 시도
        if (target == null)
        {
            FindPlayer();
            return;
        }

        // 카메라가 따라갈 위치 계산
        Vector3 desiredPosition = target.position + offset;

        // 경계 제한 적용
        if (useBoundaries)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }

        // 부드럽게 따라가기 또는 즉시 따라가기
        if (smoothFollow)
        {
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
        else
        {
            transform.position = desiredPosition;
        }
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            target = player.transform;
            Debug.Log($"[CameraFollowPlayer] Found player: {player.name}");
        }
        else
        {
            Debug.LogWarning($"[CameraFollowPlayer] Player with tag '{playerTag}' not found!");
        }
    }

    /// <summary>
    /// 외부에서 타겟을 설정할 수 있는 메서드
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        Debug.Log($"[CameraFollowPlayer] Target set to: {newTarget.name}");
    }

    /// <summary>
    /// 현재 타겟 가져오기
    /// </summary>
    public Transform GetTarget()
    {
        return target;
    }
}
