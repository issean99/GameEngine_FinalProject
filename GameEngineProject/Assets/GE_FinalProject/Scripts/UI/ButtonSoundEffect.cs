using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 버튼 클릭 시 사운드 재생
/// 모든 버튼에 자동으로 추가 가능
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonSoundEffect : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Sound Settings")]
    [SerializeField] private AudioClip clickSound; // 클릭 사운드
    [SerializeField] private AudioClip hoverSound; // 마우스 오버 사운드 (선택사항)
    [SerializeField] [Range(0f, 1f)] private float volume = 0.5f;
    [SerializeField] private bool playOnClick = true;
    [SerializeField] private bool playOnHover = false;

    private AudioSource audioSource;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        // AudioSource 추가
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D 사운드
        audioSource.volume = volume;
    }

    /// <summary>
    /// 마우스가 버튼 위로 올라갔을 때
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (playOnHover && hoverSound != null && button.interactable)
        {
            audioSource.PlayOneShot(hoverSound, volume * 0.7f); // Hover는 살짝 작게
        }
    }

    /// <summary>
    /// 버튼 클릭 시
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (playOnClick && clickSound != null && button.interactable)
        {
            audioSource.PlayOneShot(clickSound, volume);
        }
    }

    /// <summary>
    /// 코드에서 직접 사운드 재생
    /// </summary>
    public void PlayClickSound()
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound, volume);
        }
    }
}
