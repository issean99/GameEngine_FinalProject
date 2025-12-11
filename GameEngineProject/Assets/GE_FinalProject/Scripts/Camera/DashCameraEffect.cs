using System.Collections;
using UnityEngine;

/// <summary>
/// Dash motion blur camera effect - camera leads ahead during dash for speed feel
/// 대쉬 시 카메라가 진행 방향으로 앞서 나가면서 모션 블러 효과 생성
/// </summary>
public class DashCameraEffect : MonoBehaviour
{
    [Header("Motion Blur Settings")]
    [SerializeField] private float leadDistance = 2f; // 대쉬 방향으로 카메라가 앞서 나가는 거리
    [SerializeField] private float leadSpeed = 15f; // 카메라가 앞서 나가는 속도
    [SerializeField] private float returnSpeed = 8f; // 원래 위치로 돌아오는 속도
    [SerializeField] private AnimationCurve leadCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 앞서 나가는 곡선

    [Header("Camera Shake Settings")]
    [SerializeField] private bool enableShake = true;
    [SerializeField] private float shakeIntensity = 0.15f; // 흔들림 강도
    [SerializeField] private float shakeDuration = 0.15f; // 흔들림 지속 시간
    [SerializeField] private float shakeFrequency = 25f; // 흔들림 빈도 (높을수록 빠르게 흔들림)

    [Header("FOV Zoom Settings (2D = Orthographic Size)")]
    [SerializeField] private bool enableZoom = true;
    [SerializeField] private float zoomAmount = 0.8f; // 줌 아웃 양 (Orthographic Size에 더해짐)
    [SerializeField] private float zoomSpeed = 10f; // 줌 속도

    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CameraFollowPlayer cameraFollow;

    private Vector3 originalOffset; // 카메라의 원래 오프셋
    private Vector3 currentLeadOffset; // 현재 리드 오프셋
    private bool isDashing = false;
    private Coroutine shakeCoroutine;
    private float originalOrthographicSize;
    private float targetOrthographicSize;

    private static DashCameraEffect instance;

    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }

        // Auto-find references if not assigned
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (cameraFollow == null)
        {
            cameraFollow = GetComponent<CameraFollowPlayer>();
        }

        if (mainCamera != null)
        {
            originalOrthographicSize = mainCamera.orthographicSize;
            targetOrthographicSize = originalOrthographicSize;
        }
    }

    private void Update()
    {
        // Smoothly update orthographic size for zoom effect
        if (enableZoom && mainCamera != null)
        {
            mainCamera.orthographicSize = Mathf.Lerp(
                mainCamera.orthographicSize,
                targetOrthographicSize,
                Time.deltaTime * zoomSpeed
            );
        }
    }

    private void LateUpdate()
    {
        // Apply lead offset to camera position if dashing
        if (isDashing && currentLeadOffset != Vector3.zero)
        {
            transform.position += currentLeadOffset;
        }
    }

    /// <summary>
    /// Trigger dash camera effect
    /// </summary>
    /// <param name="dashDirection">Direction of the dash</param>
    /// <param name="dashDuration">Duration of the dash</param>
    public static void TriggerDashEffect(Vector2 dashDirection, float dashDuration)
    {
        if (instance != null)
        {
            instance.StartDashEffect(dashDirection, dashDuration);
        }
        else
        {
            Debug.LogWarning("[DashCameraEffect] Instance not found! Add DashCameraEffect to Main Camera.");
        }
    }

    private void StartDashEffect(Vector2 dashDirection, float dashDuration)
    {
        if (isDashing)
        {
            // Stop previous dash effect
            StopAllCoroutines();
        }

        // Start all effects
        StartCoroutine(MotionBlurEffect(dashDirection, dashDuration));

        if (enableShake)
        {
            StartCoroutine(ShakeEffect());
        }

        if (enableZoom)
        {
            StartCoroutine(ZoomEffect(dashDuration));
        }
    }

    private IEnumerator MotionBlurEffect(Vector2 dashDirection, float dashDuration)
    {
        isDashing = true;
        currentLeadOffset = Vector3.zero;

        // Normalize direction
        Vector2 direction = dashDirection.normalized;

        // Lead phase - camera moves ahead in dash direction
        float leadTime = dashDuration * 0.5f; // Lead for first half of dash
        float elapsedTime = 0f;

        while (elapsedTime < leadTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / leadTime;

            // Apply curve to make movement smoother
            float curveValue = leadCurve.Evaluate(t);

            // Calculate lead offset (2D direction, maintain Z = 0 for 2D game)
            Vector3 targetLead = new Vector3(direction.x, direction.y, 0f) * leadDistance;
            currentLeadOffset = Vector3.Lerp(Vector3.zero, targetLead, curveValue);

            yield return null;
        }

        // Return phase - camera returns to normal position
        float returnTime = dashDuration * 0.7f; // Take a bit longer to return
        elapsedTime = 0f;
        Vector3 startLeadOffset = currentLeadOffset;

        while (elapsedTime < returnTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / returnTime;

            currentLeadOffset = Vector3.Lerp(startLeadOffset, Vector3.zero, t);

            yield return null;
        }

        // Ensure we end at exactly zero
        currentLeadOffset = Vector3.zero;
        isDashing = false;
    }

    private IEnumerator ShakeEffect()
    {
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            elapsedTime += Time.deltaTime;

            // Decay intensity over time
            float intensity = shakeIntensity * (1f - (elapsedTime / shakeDuration));

            // Random offset with high frequency
            float x = Mathf.PerlinNoise(Time.time * shakeFrequency, 0f) * 2f - 1f;
            float y = Mathf.PerlinNoise(0f, Time.time * shakeFrequency) * 2f - 1f;

            Vector3 shakeOffset = new Vector3(x, y, 0f) * intensity;

            // Apply shake offset directly to camera position
            // This is temporary and will be overridden by CameraFollowPlayer in the next frame
            transform.position += shakeOffset;

            yield return null;
        }
    }

    private IEnumerator ZoomEffect(float dashDuration)
    {
        // Quick zoom out
        targetOrthographicSize = originalOrthographicSize + zoomAmount;

        // Wait for dash to almost finish
        yield return new WaitForSeconds(dashDuration * 0.7f);

        // Zoom back in
        targetOrthographicSize = originalOrthographicSize;
    }

    /// <summary>
    /// Reset camera to normal state (useful when player dies or scene changes)
    /// </summary>
    public static void ResetCamera()
    {
        if (instance != null)
        {
            instance.StopAllCoroutines();
            instance.isDashing = false;
            instance.currentLeadOffset = Vector3.zero;

            if (instance.mainCamera != null)
            {
                instance.targetOrthographicSize = instance.originalOrthographicSize;
                instance.mainCamera.orthographicSize = instance.originalOrthographicSize;
            }
        }
    }

    /// <summary>
    /// Adjust effect intensity at runtime
    /// </summary>
    public static void SetEffectIntensity(float leadMultiplier, float shakeMultiplier, float zoomMultiplier)
    {
        if (instance != null)
        {
            instance.leadDistance *= leadMultiplier;
            instance.shakeIntensity *= shakeMultiplier;
            instance.zoomAmount *= zoomMultiplier;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize lead distance
        if (Application.isPlaying && isDashing)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position + currentLeadOffset, 0.5f);
            Gizmos.DrawLine(transform.position, transform.position + currentLeadOffset);
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
