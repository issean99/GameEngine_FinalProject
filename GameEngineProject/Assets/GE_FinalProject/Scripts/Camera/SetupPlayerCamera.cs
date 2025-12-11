using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Automatically sets up Cinemachine Virtual Camera for player
/// Player용 Cinemachine Virtual Camera 자동 설정
/// </summary>
public class SetupPlayerCamera : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private int virtualCameraPriority = 10;
    [SerializeField] private float orthographicSize = 10f; // 렌즈 크기 10으로 고정
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 0f, -10f);

    [Header("Smoothing Settings")]
    [SerializeField] private float positionDampingX = 1.5f; // X축 부드러운 이동 (값이 클수록 느림)
    [SerializeField] private float positionDampingY = 1.5f; // Y축 부드러운 이동
    [SerializeField] private float positionDampingZ = 1.5f; // Z축 부드러운 이동

    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupVirtualCamera();
        }
    }

    /// <summary>
    /// Create and setup virtual camera for this player
    /// </summary>
    public void SetupVirtualCamera()
    {
        // Check if virtual camera already exists
        CinemachineVirtualCamera existingVCam = GetComponentInChildren<CinemachineVirtualCamera>();
        if (existingVCam != null)
        {
            Debug.Log("[SetupPlayerCamera] Virtual Camera already exists, skipping setup");
            return;
        }

        // Create new GameObject for Virtual Camera
        GameObject vcamObj = new GameObject("PlayerVirtualCamera");
        vcamObj.transform.SetParent(transform);
        vcamObj.transform.localPosition = cameraOffset;
        vcamObj.transform.localRotation = Quaternion.identity;

        // Add CinemachineVirtualCamera component
        CinemachineVirtualCamera vcam = vcamObj.AddComponent<CinemachineVirtualCamera>();

        // Configure Virtual Camera
        vcam.Priority = virtualCameraPriority;
        vcam.Follow = transform; // Follow player
        vcam.LookAt = transform; // Look at player

        // Set lens settings
        vcam.m_Lens.OrthographicSize = orthographicSize;
        vcam.m_Lens.NearClipPlane = 0.3f;
        vcam.m_Lens.FarClipPlane = 1000f;

        // Add and configure Position Composer for smooth following
        var positionComposer = vcam.AddCinemachineComponent<CinemachinePositionComposer>();
        if (positionComposer != null)
        {
            // Set damping for smooth camera movement
            positionComposer.Damping = new Vector3(positionDampingX, positionDampingY, positionDampingZ);

            // Center the player on screen (Cinemachine 3.x uses TargetOffset)
            positionComposer.TargetOffset = new Vector3(0f, 0f, 0f);

            // Composition settings (centering and framing)
            positionComposer.CenterOnActivate = true;
        }

        Debug.Log("[SetupPlayerCamera] Virtual Camera created and configured successfully with smooth following!");
    }

    /// <summary>
    /// Remove virtual camera (for testing)
    /// </summary>
    public void RemoveVirtualCamera()
    {
        CinemachineVirtualCamera vcam = GetComponentInChildren<CinemachineVirtualCamera>();
        if (vcam != null)
        {
            DestroyImmediate(vcam.gameObject);
            Debug.Log("[SetupPlayerCamera] Virtual Camera removed");
        }
    }
}
