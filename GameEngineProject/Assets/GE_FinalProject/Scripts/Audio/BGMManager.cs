using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages background music across scenes with fade in/out effects
/// 씬 전환 시 배경음악을 페이드 효과와 함께 관리
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BGMManager : MonoBehaviour
{
    [Header("BGM Settings")]
    [SerializeField] private AudioClip startBGM;
    [SerializeField] private AudioClip tutorialBGM;
    [SerializeField] private AudioClip stage1BGM;
    [SerializeField] private AudioClip stage2BGM;
    [SerializeField] private AudioClip stage3BGM;
    [SerializeField] private AudioClip boss1BGM;
    [SerializeField] private AudioClip boss2BGM;
    [SerializeField] private AudioClip endBGM;

    [Header("Volume Settings")]
    [SerializeField] private float masterVolume = 0.5f;
    [SerializeField] private float fadeSpeed = 1f; // Seconds to fade in/out

    private AudioSource audioSource;
    private static BGMManager instance;

    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = GetComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.volume = 0f; // Start silent

            // Subscribe to scene changes
            SceneManager.sceneLoaded += OnSceneLoaded;

            Debug.Log("[BGMManager] BGM Manager initialized and will persist across scenes");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from scene changes
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Get BGM for the loaded scene
        AudioClip newBGM = GetBGMForScene(scene.name);

        if (newBGM != null)
        {
            // If different BGM, change with fade
            if (audioSource.clip != newBGM)
            {
                StartCoroutine(ChangeBGMWithFade(newBGM));
            }
            else if (!audioSource.isPlaying)
            {
                // Same BGM but not playing, start it
                audioSource.Play();
                StartCoroutine(FadeIn());
            }
        }
        else
        {
            // No BGM for this scene, fade out current music
            StartCoroutine(FadeOut());
        }
    }

    private AudioClip GetBGMForScene(string sceneName)
    {
        switch (sceneName)
        {
            case "Start":
                return startBGM;
            case "Tutorial":
                return tutorialBGM;
            case "Stage1":
                return stage1BGM;
            case "Stage2":
                return stage2BGM;
            case "Stage3":
                return stage3BGM;
            case "boss1":
                return boss1BGM;
            case "boss2":
                return boss2BGM;
            case "End":
                return endBGM;
            default:
                Debug.LogWarning($"[BGMManager] No BGM defined for scene: {sceneName}");
                return null;
        }
    }

    private IEnumerator ChangeBGMWithFade(AudioClip newClip)
    {
        // Fade out current music
        yield return StartCoroutine(FadeOut());

        // Change clip
        audioSource.clip = newClip;
        audioSource.Play();

        // Fade in new music
        yield return StartCoroutine(FadeIn());

        Debug.Log($"[BGMManager] Changed BGM to: {newClip.name}");
    }

    private IEnumerator FadeOut()
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeSpeed;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = 0f;
    }

    private IEnumerator FadeIn()
    {
        audioSource.volume = 0f;

        while (audioSource.volume < masterVolume)
        {
            audioSource.volume += masterVolume * Time.deltaTime / fadeSpeed;
            yield return null;
        }

        audioSource.volume = masterVolume;
    }

    // Public methods for manual control
    public void SetVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        audioSource.volume = masterVolume;
    }

    public void Stop()
    {
        StartCoroutine(FadeOut());
    }

    public void Play()
    {
        if (!audioSource.isPlaying && audioSource.clip != null)
        {
            audioSource.Play();
            StartCoroutine(FadeIn());
        }
    }
}
