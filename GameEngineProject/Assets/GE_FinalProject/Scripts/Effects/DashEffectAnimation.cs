using UnityEngine;

/// <summary>
/// Simple dash effect that scales up and fades out
/// 스케일이 커지면서 페이드 아웃되는 간단한 대쉬 이펙트
/// </summary>
public class DashEffectAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private float startScale = 0.5f;
    [SerializeField] private float endScale = 2f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private float timer = 0f;
    private SpriteRenderer spriteRenderer;
    private Vector3 initialScale;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        initialScale = transform.localScale;

        // Auto-destroy after duration
        Destroy(gameObject, duration);
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / duration);

        // Scale animation
        float scaleMultiplier = Mathf.Lerp(startScale, endScale, scaleCurve.Evaluate(progress));
        transform.localScale = initialScale * scaleMultiplier;

        // Fade out animation
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = alphaCurve.Evaluate(progress);
            spriteRenderer.color = color;
        }
    }
}
