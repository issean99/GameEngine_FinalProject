using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI component for displaying dialogue text
/// 대사 텍스트를 표시하는 UI 컴포넌트
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject continueIndicator; // "Press SPACE to continue" 표시

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.2f;
    [SerializeField] private bool animateOnShow = true;

    [Header("Continue Indicator")]
    [SerializeField] private float indicatorBlinkSpeed = 2f; // 깜빡임 속도

    private CanvasGroup canvasGroup;
    private Coroutine blinkCoroutine;
    private bool isVisible = false;

    private void Awake()
    {
        // Get or add CanvasGroup for fade animations
        canvasGroup = dialoguePanel != null ? dialoguePanel.GetComponent<CanvasGroup>() : null;
        if (canvasGroup == null && dialoguePanel != null)
        {
            canvasGroup = dialoguePanel.AddComponent<CanvasGroup>();
        }

        // Hide dialogue panel on start
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (continueIndicator != null)
        {
            continueIndicator.SetActive(false);
        }
    }

    /// <summary>
    /// Show dialogue panel
    /// </summary>
    public void Show()
    {
        if (isVisible) return;

        isVisible = true;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        if (animateOnShow && canvasGroup != null)
        {
            StartCoroutine(FadeIn());
        }
        else if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        // Start continue indicator animation
        if (continueIndicator != null && blinkCoroutine == null)
        {
            blinkCoroutine = StartCoroutine(BlinkContinueIndicator());
        }
    }

    /// <summary>
    /// Hide dialogue panel
    /// </summary>
    public void Hide()
    {
        if (!isVisible) return;

        isVisible = false;

        if (animateOnShow && canvasGroup != null)
        {
            StartCoroutine(FadeOut());
        }
        else
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }
        }

        // Stop continue indicator animation
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        if (continueIndicator != null)
        {
            continueIndicator.SetActive(false);
        }
    }

    /// <summary>
    /// Set speaker name
    /// </summary>
    public void SetSpeakerName(string name)
    {
        if (speakerNameText != null)
        {
            speakerNameText.text = name;
        }
    }

    /// <summary>
    /// Set dialogue text
    /// </summary>
    public void SetDialogueText(string text)
    {
        if (dialogueText != null)
        {
            dialogueText.text = text;
        }
    }

    private IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;

        float elapsed = 0f;
        canvasGroup.alpha = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled time in case game is paused
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        if (canvasGroup == null) yield break;

        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    private IEnumerator BlinkContinueIndicator()
    {
        if (continueIndicator == null) yield break;

        continueIndicator.SetActive(true);

        while (true)
        {
            // Fade in
            float elapsed = 0f;
            float duration = 1f / indicatorBlinkSpeed;

            CanvasGroup indicatorGroup = continueIndicator.GetComponent<CanvasGroup>();
            if (indicatorGroup == null)
            {
                indicatorGroup = continueIndicator.AddComponent<CanvasGroup>();
            }

            // Fade out
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                indicatorGroup.alpha = Mathf.Lerp(1f, 0.3f, elapsed / duration);
                yield return null;
            }

            // Fade in
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                indicatorGroup.alpha = Mathf.Lerp(0.3f, 1f, elapsed / duration);
                yield return null;
            }
        }
    }

    /// <summary>
    /// Check if dialogue UI is currently visible
    /// </summary>
    public bool IsVisible()
    {
        return isVisible;
    }
}
