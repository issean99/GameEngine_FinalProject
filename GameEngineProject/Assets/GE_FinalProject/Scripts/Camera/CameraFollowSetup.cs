using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// 씬 시작 시 Cinemachine Camera가 플레이어를 자동으로 찾아서 Follow하도록 설정
/// Unity 6.x (Cinemachine 3.x) 버전용
/// </summary>
public class CameraFollowSetup : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private string playerTag = "Player";

    private CinemachineCamera cinemachineCamera;

    private void Awake()
    {
        cinemachineCamera = GetComponent<CinemachineCamera>();

        if (cinemachineCamera == null)
        {
            Debug.LogError("[CameraFollowSetup] CinemachineCamera component not found!");
            return;
        }

        FindAndFollowPlayer();
    }

    private void FindAndFollowPlayer()
    {
        // 플레이어 찾기
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (player == null)
        {
            Debug.LogWarning($"[CameraFollowSetup] Player with tag '{playerTag}' not found!");
            return;
        }

        // Follow 타겟 설정 (Cinemachine 3.x)
        cinemachineCamera.Follow = player.transform;

        // Tracking 타겟도 설정 (LookAt 대신 Tracking 사용)
        cinemachineCamera.LookAt = player.transform;

        Debug.Log($"[CameraFollowSetup] Camera now following: {player.name}");
    }
}
