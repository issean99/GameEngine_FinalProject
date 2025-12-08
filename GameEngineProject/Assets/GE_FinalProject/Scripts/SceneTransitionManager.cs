using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manages scene transitions with various visual effects
/// 다양한 시각 효과와 함께 씬 전환을 관리
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    public enum TransitionType
    {
        Fade,           // 일반 페이드 (일반 스테이지)
        Flash,          // 플래시 (보스 입장)
        SlowFade        // 느린 페이드 (사망 시)
    }

    [Header("Transition Settings")]
    [SerializeField] private Image transitionImage;
    [SerializeField] private float fadeSpeed = 1f;          // 일반 페이드 속도
    [SerializeField] private float flashSpeed = 0.3f;       // 플래시 속도 (빠름)
    [SerializeField] private float slowFadeSpeed = 2f;      // 느린 페이드 속도
    [SerializeField] private Color fadeColor = Color.black; // 페이드 색상 (검은색)
    [SerializeField] private Color flashColor = Color.white; // 플래시 색상 (흰색)

    private static SceneTransitionManager instance;
    private bool isTransitioning = false;

    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Ensure transition image exists
            if (transitionImage == null)
            {
                CreateTransitionCanvas();
            }

            // Start with transparent
            SetImageAlpha(0f);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void CreateTransitionCanvas()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("TransitionCanvas");
        canvasObj.transform.SetParent(transform);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // Always on top
        canvasObj.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Create Image
        GameObject imageObj = new GameObject("TransitionImage");
        imageObj.transform.SetParent(canvasObj.transform);
        transitionImage = imageObj.AddComponent<Image>();
        transitionImage.color = fadeColor;
        transitionImage.raycastTarget = false; // Allow clicks to pass through when transparent

        // Make it fullscreen
        RectTransform rect = imageObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
    }

    private void SetImageAlpha(float alpha)
    {
        if (transitionImage != null)
        {
            Color color = transitionImage.color;
            color.a = alpha;
            transitionImage.color = color;
        }
    }

    /// <summary>
    /// Load scene with Fade transition (for normal stages)
    /// </summary>
    public static void LoadSceneWithFade(string sceneName)
    {
        if (instance != null && !instance.isTransitioning)
        {
            instance.StartCoroutine(instance.TransitionToScene(sceneName, TransitionType.Fade));
        }
    }

    /// <summary>
    /// Load scene with Flash transition (for boss stages)
    /// </summary>
    public static void LoadSceneWithFlash(string sceneName)
    {
        if (instance != null && !instance.isTransitioning)
        {
            instance.StartCoroutine(instance.TransitionToScene(sceneName, TransitionType.Flash));
        }
    }

    /// <summary>
    /// Load scene with Slow Fade transition (for death)
    /// </summary>
    public static void LoadSceneWithSlowFade(string sceneName)
    {
        if (instance != null && !instance.isTransitioning)
        {
            instance.StartCoroutine(instance.TransitionToScene(sceneName, TransitionType.SlowFade));
        }
    }

    /// <summary>
    /// Load scene with Fade after zoom-in effect (for Start -> Tutorial)
    /// </summary>
    public static void LoadSceneAfterZoom(string sceneName, Transform targetTransform, float zoomDuration = 1.5f)
    {
        if (instance != null && !instance.isTransitioning)
        {
            instance.StartCoroutine(instance.ZoomAndTransition(sceneName, targetTransform, zoomDuration));
        }
    }

    private IEnumerator TransitionToScene(string sceneName, TransitionType transitionType)
    {
        isTransitioning = true;

        // Determine transition parameters
        float speed;
        Color targetColor;

        switch (transitionType)
        {
            case TransitionType.Flash:
                speed = flashSpeed;
                targetColor = flashColor;
                break;
            case TransitionType.SlowFade:
                speed = slowFadeSpeed;
                targetColor = fadeColor;
                break;
            case TransitionType.Fade:
            default:
                speed = fadeSpeed;
                targetColor = fadeColor;
                break;
        }

        // Set the color
        transitionImage.color = new Color(targetColor.r, targetColor.g, targetColor.b, 0f);

        // Fade out (cover screen)
        yield return StartCoroutine(FadeOut(speed));

        // Load scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // Wait for scene to load
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Small delay for scene initialization
        yield return new WaitForSeconds(0.1f);

        // Fade in (reveal new scene)
        yield return StartCoroutine(FadeIn(speed));

        isTransitioning = false;
    }

    private IEnumerator FadeOut(float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / duration);
            SetImageAlpha(alpha);
            yield return null;
        }

        SetImageAlpha(1f);
    }

    private IEnumerator FadeIn(float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsedTime / duration);
            SetImageAlpha(alpha);
            yield return null;
        }

        SetImageAlpha(0f);
    }

    private IEnumerator ZoomAndTransition(string sceneName, Transform target, float zoomDuration)
    {
        isTransitioning = true;

        // Get main camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("[SceneTransitionManager] Main camera not found! Skipping zoom effect.");
            yield return StartCoroutine(TransitionToScene(sceneName, TransitionType.Fade));
            yield break;
        }

        if (target == null)
        {
            Debug.LogWarning("[SceneTransitionManager] Target transform is null! Skipping zoom effect.");
            yield return StartCoroutine(TransitionToScene(sceneName, TransitionType.Fade));
            yield break;
        }

        // Store original camera settings
        float originalSize = mainCamera.orthographicSize;
        Vector3 originalPosition = mainCamera.transform.position;

        // Target zoom settings (zoom in closer)
        float targetSize = originalSize * 0.3f; // Zoom in to 30% of original size
        Vector3 targetPosition = new Vector3(target.position.x, target.position.y, originalPosition.z);

        // Zoom in to target
        float elapsedTime = 0f;
        while (elapsedTime < zoomDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / zoomDuration;

            // Smooth interpolation
            float smoothT = t * t * (3f - 2f * t); // Smoothstep

            mainCamera.orthographicSize = Mathf.Lerp(originalSize, targetSize, smoothT);
            mainCamera.transform.position = Vector3.Lerp(originalPosition, targetPosition, smoothT);

            yield return null;
        }

        // Ensure final values
        mainCamera.orthographicSize = targetSize;
        mainCamera.transform.position = targetPosition;

        // Small pause at zoomed state
        yield return new WaitForSeconds(0.3f);

        // Now fade out
        transitionImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        yield return StartCoroutine(FadeOut(fadeSpeed));

        // Load scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Small delay for scene initialization
        yield return new WaitForSeconds(0.1f);

        // Fade in
        yield return StartCoroutine(FadeIn(fadeSpeed));

        isTransitioning = false;
    }

    /// <summary>
    /// Public method to check if transition is in progress
    /// </summary>
    public static bool IsTransitioning()
    {
        return instance != null && instance.isTransitioning;
    }
}
