using UnityEngine;

/// <summary>
/// Generic sound effects component for enemies
/// Attach to any enemy to add hit/death sounds
/// 적에게 피격/사망 효과음을 추가하는 범용 컴포넌트
/// </summary>
public class EnemySoundEffects : MonoBehaviour
{
    [Header("Sound Effects")]
    [SerializeField] private AudioClip[] hitSounds; // 피격 소리 (여러 개 중 랜덤)
    [SerializeField] private AudioClip deathSound; // 사망 소리
    [SerializeField] private AudioClip attackSound; // 공격 소리
    [SerializeField] [Range(0f, 1f)] private float volume = 0.7f;

    private AudioSource audioSource;

    private void Awake()
    {
        // Add or get AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
        audioSource.volume = volume;
    }

    /// <summary>
    /// Play random hit sound
    /// 랜덤 피격 소리 재생
    /// </summary>
    public void PlayHitSound()
    {
        if (hitSounds != null && hitSounds.Length > 0 && audioSource != null)
        {
            // Pick random hit sound
            AudioClip clip = hitSounds[Random.Range(0, hitSounds.Length)];
            if (clip != null)
            {
                audioSource.PlayOneShot(clip, volume);
            }
        }
    }

    /// <summary>
    /// Play death sound
    /// 사망 소리 재생
    /// </summary>
    public void PlayDeathSound()
    {
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound, volume);
        }
    }

    /// <summary>
    /// Play attack sound
    /// 공격 소리 재생
    /// </summary>
    public void PlayAttackSound()
    {
        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound, volume);
        }
    }
}
