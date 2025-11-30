using UnityEngine;

public class TelegraphIndicator : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private Color telegraphColor = new Color(1f, 0f, 0f, 0.3f); // Red transparent
    [SerializeField] private float pulseSpeed = 3f; // Speed of pulsing effect
    [SerializeField] private float minAlpha = 0.2f;
    [SerializeField] private float maxAlpha = 0.6f;

    private SpriteRenderer spriteRenderer;
    private LineRenderer lineRenderer;
    private float pulseTimer = 0f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        lineRenderer = GetComponent<LineRenderer>();

        // Set initial color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = telegraphColor;
        }

        if (lineRenderer != null)
        {
            lineRenderer.startColor = telegraphColor;
            lineRenderer.endColor = telegraphColor;
        }
    }

    private void Update()
    {
        // Pulse effect
        pulseTimer += Time.deltaTime * pulseSpeed;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(pulseTimer) + 1f) / 2f);

        if (spriteRenderer != null)
        {
            Color newColor = telegraphColor;
            newColor.a = alpha;
            spriteRenderer.color = newColor;
        }

        if (lineRenderer != null)
        {
            Color newColor = telegraphColor;
            newColor.a = alpha;
            lineRenderer.startColor = newColor;
            lineRenderer.endColor = newColor;
        }
    }

    // Optional: Set up as a line from start to end position
    public void SetupLine(Vector3 startPos, Vector3 endPos, float width = 0.5f)
    {
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.startColor = telegraphColor;
        lineRenderer.endColor = telegraphColor;

        // Disable sprite renderer if using line
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }

    // Optional: Set as a directional arrow/line
    public void SetupDirectionalIndicator(Vector3 position, Vector3 direction, float length = 5f, float width = 0.5f)
    {
        transform.position = position;

        Vector3 endPos = position + direction.normalized * length;
        SetupLine(position, endPos, width);

        // Rotate to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
