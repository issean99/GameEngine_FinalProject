using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 플레이어 사망 시 카메라 줌인 및 검정 배경 오버레이 효과
/// Unity 6.x (Cinemachine 3.x) 버전용
/// URP Post-Processing 불필요 - UI Canvas Overlay 방식
/// </summary>
public class DeathCameraEffect : MonoBehaviour
{
    public static DeathCameraEffect Instance { get; private set; }

    [Header("Camera Zoom Settings")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private float deathZoomSize = 3f; // 죽을 때 카메라 크기 (작을수록 확대)
    [SerializeField] private float zoomDuration = 1.5f; // 줌인 시간

    [Header("Black Overlay Settings")]
    [SerializeField] private Image blackOverlayImage; // UI Canvas에 배치된 검정 이미지
    [SerializeField] private float overlayFadeDuration = 1.5f; // 페이드 인 시간
    [SerializeField] private RectTransform playerSpotlight; // 플레이어 위치의 구멍 (투명한 원형 영역)

    [Header("UI to Hide on Death")]
    [SerializeField] private GameObject healthBarUI; // 체력바 UI
    [SerializeField] private GameObject skillIconUI; // 스킬 아이콘 UI (있으면 추가)

    [Header("Player Reference")]
    [SerializeField] private GameObject player; // 플레이어 오브젝트
    private SpriteRenderer[] playerSprites; // 플레이어의 모든 스프라이트
    private string[] originalSortingLayers; // 원본 Sorting Layer 저장

    private float originalZoomSize;
    private bool isEffectActive = false;
    private string originalCanvasSortingLayer; // Canvas 원본 Sorting Layer
    private int originalCanvasOrder; // Canvas 원본 Order

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Cinemachine Camera 찾기
        if (cinemachineCamera == null)
        {
            cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        }

        // 원본 줌 크기 저장
        if (cinemachineCamera != null)
        {
            originalZoomSize = cinemachineCamera.Lens.OrthographicSize;
        }

        // Black Overlay 초기화 (완전 투명)
        if (blackOverlayImage != null)
        {
            Color color = blackOverlayImage.color;
            color.a = 0f;
            blackOverlayImage.color = color;
            blackOverlayImage.gameObject.SetActive(false);
        }

        // Canvas 찾기 및 원본 설정 저장
        if (blackOverlayCanvas == null && blackOverlayImage != null)
        {
            blackOverlayCanvas = blackOverlayImage.GetComponentInParent<Canvas>();
        }

        if (blackOverlayCanvas != null)
        {
            originalCanvasSortingLayer = blackOverlayCanvas.sortingLayerName;
            originalCanvasOrder = blackOverlayCanvas.sortingOrder;
        }

        // 플레이어 찾기 및 스프라이트 저장
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (player != null)
        {
            playerSprites = player.GetComponentsInChildren<SpriteRenderer>();
            if (playerSprites != null && playerSprites.Length > 0)
            {
                // 원본 Sorting Layer 저장
                originalSortingLayers = new string[playerSprites.Length];
                for (int i = 0; i < playerSprites.Length; i++)
                {
                    originalSortingLayers[i] = playerSprites[i].sortingLayerName;
                }
            }
        }
    }

    /// <summary>
    /// 플레이어 사망 시 호출할 메서드
    /// </summary>
    public void TriggerDeathEffect()
    {
        if (isEffectActive) return;

        isEffectActive = true;
        StartCoroutine(PlayDeathEffectCoroutine());
    }

    private IEnumerator PlayDeathEffectCoroutine()
    {
        // UI 숨기기
        HideUI();

        // Black Overlay 활성화
        if (blackOverlayImage != null)
        {
            blackOverlayImage.gameObject.SetActive(true);
        }

        // Canvas를 DeathEffect 레이어로 변경
        if (blackOverlayCanvas != null)
        {
            blackOverlayCanvas.sortingLayerName = deathEffectSortingLayer;
            blackOverlayCanvas.sortingOrder = 0;
        }

        // 플레이어를 Player 레이어로 변경 (DeathEffect보다 위)
        SetPlayerSortingLayer(playerSortingLayer);

        float elapsed = 0f;
        float maxDuration = Mathf.Max(zoomDuration, overlayFadeDuration);

        while (elapsed < maxDuration)
        {
            elapsed += Time.deltaTime;

            // 카메라 줌인 (Orthographic Size 감소)
            if (cinemachineCamera != null && elapsed < zoomDuration)
            {
                float t = elapsed / zoomDuration;
                float currentSize = Mathf.Lerp(originalZoomSize, deathZoomSize, t);
                cinemachineCamera.Lens.OrthographicSize = currentSize;
            }

            // 검정 오버레이 페이드 인 (Alpha 0 -> 1)
            if (blackOverlayImage != null && elapsed < overlayFadeDuration)
            {
                float t = elapsed / overlayFadeDuration;
                Color color = blackOverlayImage.color;
                color.a = Mathf.Lerp(0f, 1f, t); // 완전 불투명하게
                blackOverlayImage.color = color;
            }

            yield return null;
        }

        Debug.Log("[DeathCameraEffect] Death effect complete!");
    }

    /// <summary>
    /// UI 숨기기 (체력바, 스킬 아이콘 등)
    /// </summary>
    private void HideUI()
    {
        if (healthBarUI != null)
        {
            healthBarUI.SetActive(false);
        }

        if (skillIconUI != null)
        {
            skillIconUI.SetActive(false);
        }
    }

    /// <summary>
    /// UI 다시 보이기
    /// </summary>
    private void ShowUI()
    {
        if (healthBarUI != null)
        {
            healthBarUI.SetActive(true);
        }

        if (skillIconUI != null)
        {
            skillIconUI.SetActive(true);
        }
    }

    /// <summary>
    /// 플레이어 Sorting Layer 설정 (검정 배경보다 앞에 표시)
    /// </summary>
    private void SetPlayerSortingLayer(string layerName)
    {
        if (playerSprites == null || playerSprites.Length == 0) return;

        foreach (SpriteRenderer sprite in playerSprites)
        {
            if (sprite != null)
            {
                sprite.sortingLayerName = layerName;
            }
        }
    }

    /// <summary>
    /// 플레이어 Sorting Layer 원복
    /// </summary>
    private void ResetPlayerSortingLayer()
    {
        if (playerSprites == null || originalSortingLayers == null) return;
        if (playerSprites.Length != originalSortingLayers.Length) return;

        for (int i = 0; i < playerSprites.Length; i++)
        {
            if (playerSprites[i] != null)
            {
                playerSprites[i].sortingLayerName = originalSortingLayers[i];
            }
        }
    }

    /// <summary>
    /// 효과 리셋 (게임 재시작 시 호출)
    /// </summary>
    public void ResetEffect()
    {
        StopAllCoroutines();
        isEffectActive = false;

        // 카메라 줌 원복
        if (cinemachineCamera != null)
        {
            cinemachineCamera.Lens.OrthographicSize = originalZoomSize;
        }

        // Black Overlay 숨기기
        if (blackOverlayImage != null)
        {
            Color color = blackOverlayImage.color;
            color.a = 0f;
            blackOverlayImage.color = color;
            blackOverlayImage.gameObject.SetActive(false);
        }

        // Canvas Sorting Layer 원복
        if (blackOverlayCanvas != null)
        {
            blackOverlayCanvas.sortingLayerName = originalCanvasSortingLayer;
            blackOverlayCanvas.sortingOrder = originalCanvasOrder;
        }

        // 플레이어 Sorting Layer 원복
        ResetPlayerSortingLayer();

        // UI 다시 보이기
        ShowUI();

        Debug.Log("[DeathCameraEffect] Effect reset!");
    }
}
