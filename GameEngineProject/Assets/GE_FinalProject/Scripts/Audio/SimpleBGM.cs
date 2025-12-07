using UnityEngine;

/// <summary>
/// Simple BGM player that stops previous BGM when scene loads
/// 씬 로드 시 이전 BGM을 멈추는 간단한 BGM 플레이어
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SimpleBGM : MonoBehaviour
{
    private void Start()
    {
        // Find all other BGM objects and stop them
        SimpleBGM[] allBGMs = FindObjectsOfType<SimpleBGM>();
        foreach (SimpleBGM bgm in allBGMs)
        {
            if (bgm != this)
            {
                Destroy(bgm.gameObject);
            }
        }

        // Start playing this BGM
        AudioSource audioSource = GetComponent<AudioSource>();
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}
