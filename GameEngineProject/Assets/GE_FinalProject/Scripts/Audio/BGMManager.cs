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
    [SerializeField] private AudioClip boss1Phase2BGM; // Boss 1 Phase 2 BGM
    [SerializeField] private AudioClip boss2BGM;
    [SerializeField] private AudioClip boss2Phase2BGM; // Boss 2 Phase 2 BGM
    [SerializeField] private AudioClip endBGM;

    [Header("Volume Settings")]
    [SerializeField] private float masterVolume = 0.5f;
    [SerializeField] private float bossVolumeMultiplier = 1.5f; // 보스 BGM 볼륨 배율 (보스 BGM을 더 크게)
    [SerializeField] private float fadeSpeed = 1f; // Seconds to fade in/out

    private AudioSource audioSource;
    private static BGMManager instance;
    private float currentTargetVolume; // 현재 목표 볼륨
    private bool isManualControl = false; // 수동 제어 모드 (보스 페이즈 전환 시)

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
        bool isBossScene = scene.name == "boss1" || scene.name == "boss2";

        // ALWAYS reset manual control when entering a NEW scene (regardless of which scene)
        // This ensures boss phase BGM doesn't persist when leaving boss scenes
        Debug.Log($"[BGMManager] OnSceneLoaded: {scene.name}, isManualControl before reset: {isManualControl}");

        // Reset manual control for ANY scene transition
        isManualControl = false;

        // Get BGM for the loaded scene
        AudioClip newBGM = GetBGMForScene(scene.name);

        if (newBGM != null)
        {
            // Determine target volume based on scene type
            currentTargetVolume = isBossScene ? masterVolume * bossVolumeMultiplier : masterVolume;

            // For boss scenes, ALWAYS reload the Phase 1 BGM (even if reloading the same scene)
            // This ensures respawn always starts from Phase 1 music
            bool forceReload = isBossScene && (audioSource.clip == boss1Phase2BGM || audioSource.clip == boss2Phase2BGM);

            // If different BGM or force reload
            if (audioSource.clip != newBGM || forceReload)
            {
                Debug.Log($"[BGMManager] Changing BGM to: {newBGM.name} (forceReload: {forceReload})");

                // For boss scene respawn (force reload), play immediately without fade
                if (forceReload)
                {
                    // Stop current music
                    audioSource.Stop();

                    // Change clip and play immediately at full volume
                    audioSource.clip = newBGM;
                    audioSource.volume = currentTargetVolume;
                    audioSource.Play();

                    Debug.Log($"[BGMManager] Boss respawn - BGM changed instantly to: {newBGM.name}");
                }
                else
                {
                    // Normal scene transition - use fade
                    StartCoroutine(ChangeBGMWithFade(newBGM));
                }
            }
            else if (!audioSource.isPlaying)
            {
                // Same BGM but not playing, start it
                Debug.Log($"[BGMManager] Starting BGM: {newBGM.name}");
                audioSource.Play();
                StartCoroutine(FadeIn());
            }
            else
            {
                Debug.Log($"[BGMManager] BGM already playing: {audioSource.clip.name}");
            }
        }
        else
        {
            // No BGM for this scene, fade out current music
            Debug.Log($"[BGMManager] No BGM defined for scene {scene.name}, fading out");
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
        Debug.Log($"[BGMManager] ChangeBGMWithFade START for: {newClip.name}");

        // Fade out current music
        yield return StartCoroutine(FadeOut());
        Debug.Log("[BGMManager] FadeOut complete");

        // Change clip
        audioSource.clip = newClip;
        Debug.Log($"[BGMManager] AudioSource clip set to: {audioSource.clip.name}");

        audioSource.Play();
        Debug.Log($"[BGMManager] audioSource.Play() called. IsPlaying: {audioSource.isPlaying}");

        // Fade in new music
        yield return StartCoroutine(FadeIn());
        Debug.Log($"[BGMManager] FadeIn complete. Final volume: {audioSource.volume}");

        Debug.Log($"[BGMManager] ChangeBGMWithFade COMPLETE for: {newClip.name}");
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

        while (audioSource.volume < currentTargetVolume)
        {
            audioSource.volume += currentTargetVolume * Time.deltaTime / fadeSpeed;
            yield return null;
        }

        audioSource.volume = currentTargetVolume;
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

    /// <summary>
    /// Changes BGM to Boss 1 Phase 2 music
    /// </summary>
    public void PlayBoss1Phase2BGM()
    {
        Debug.Log($"[BGMManager] PlayBoss1Phase2BGM called. boss1Phase2BGM assigned: {boss1Phase2BGM != null}");

        if (boss1Phase2BGM == null)
        {
            Debug.LogError("[BGMManager] Boss 1 Phase 2 BGM is NOT assigned in Inspector! Please assign in Unity Editor.");
            Debug.LogError("[BGMManager] Go to: BGMManager GameObject → Inspector → Boss 1 Phase 2 BGM field");
            return;
        }

        if (audioSource.clip == boss1Phase2BGM && audioSource.isPlaying)
        {
            Debug.Log("[BGMManager] Boss 1 Phase 2 BGM is already playing");
            return;
        }

        Debug.Log($"[BGMManager] Switching to Boss 1 Phase 2 BGM: {boss1Phase2BGM.name}");

        // Enable manual control mode to prevent immediate scene events from interfering
        // This will be reset when a new scene is loaded
        isManualControl = true;

        // Stop any ongoing fade coroutines to prevent conflicts
        StopAllCoroutines();

        // Set boss volume for Phase 2
        currentTargetVolume = masterVolume * bossVolumeMultiplier;

        StartCoroutine(ChangeBGMWithFade(boss1Phase2BGM));
    }

    /// <summary>
    /// Changes BGM to Boss 2 Phase 2 music
    /// </summary>
    public void PlayBoss2Phase2BGM()
    {
        Debug.Log($"[BGMManager] PlayBoss2Phase2BGM called. boss2Phase2BGM assigned: {boss2Phase2BGM != null}");

        if (boss2Phase2BGM == null)
        {
            Debug.LogError("[BGMManager] Boss 2 Phase 2 BGM is NOT assigned in Inspector! Please assign in Unity Editor.");
            Debug.LogError("[BGMManager] Go to: BGMManager GameObject → Inspector → Boss 2 Phase 2 BGM field");
            return;
        }

        if (audioSource.clip == boss2Phase2BGM && audioSource.isPlaying)
        {
            Debug.Log("[BGMManager] Boss 2 Phase 2 BGM is already playing");
            return;
        }

        Debug.Log($"[BGMManager] Switching to Boss 2 Phase 2 BGM: {boss2Phase2BGM.name}");

        // Enable manual control mode to prevent immediate scene events from interfering
        // This will be reset when a new scene is loaded
        isManualControl = true;

        // Stop any ongoing fade coroutines to prevent conflicts
        StopAllCoroutines();

        // Set boss volume for Phase 2
        currentTargetVolume = masterVolume * bossVolumeMultiplier;

        StartCoroutine(ChangeBGMWithFade(boss2Phase2BGM));
    }

    /// <summary>
    /// Changes BGM to Boss 1 Phase 1 music (for boss death)
    /// </summary>
    public void PlayBoss1BGM()
    {
        Debug.Log("[BGMManager] PlayBoss1BGM called for boss death");

        if (boss1BGM == null)
        {
            Debug.LogError("[BGMManager] Boss 1 BGM is NOT assigned in Inspector!");
            return;
        }

        if (audioSource.clip == boss1BGM && audioSource.isPlaying)
        {
            Debug.Log("[BGMManager] Boss 1 BGM is already playing");
            return;
        }

        Debug.Log($"[BGMManager] Switching to Boss 1 Phase 1 BGM: {boss1BGM.name}");

        // Enable manual control mode
        isManualControl = true;

        // Stop any ongoing fade coroutines
        StopAllCoroutines();

        // Set boss volume
        currentTargetVolume = masterVolume * bossVolumeMultiplier;

        StartCoroutine(ChangeBGMWithFade(boss1BGM));
    }

    /// <summary>
    /// Changes BGM to Boss 2 Phase 1 music (for boss death)
    /// </summary>
    public void PlayBoss2BGM()
    {
        Debug.Log("[BGMManager] PlayBoss2BGM called for boss death");

        if (boss2BGM == null)
        {
            Debug.LogError("[BGMManager] Boss 2 BGM is NOT assigned in Inspector!");
            return;
        }

        if (audioSource.clip == boss2BGM && audioSource.isPlaying)
        {
            Debug.Log("[BGMManager] Boss 2 BGM is already playing");
            return;
        }

        Debug.Log($"[BGMManager] Switching to Boss 2 Phase 1 BGM: {boss2BGM.name}");

        // Enable manual control mode
        isManualControl = true;

        // Stop any ongoing fade coroutines
        StopAllCoroutines();

        // Set boss volume
        currentTargetVolume = masterVolume * bossVolumeMultiplier;

        StartCoroutine(ChangeBGMWithFade(boss2BGM));
    }
}
