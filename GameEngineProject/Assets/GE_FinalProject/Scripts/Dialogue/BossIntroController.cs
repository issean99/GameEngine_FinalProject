using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Combines boss camera zoom and dialogue into one controller
/// 보스 카메라 줌과 대사를 하나로 통합
/// </summary>
public class BossIntroController : MonoBehaviour
{
    [Header("Scene Detection")]
    [SerializeField] private bool isBoss1Scene = false;
    [SerializeField] private bool isBoss2Scene = false;
    [SerializeField] private bool autoDetectScene = true;

    [Header("References")]
    [SerializeField] private BossIntroCameraZoom cameraZoom;
    [SerializeField] private BossDialogue bossDialogue;

    private void Awake()
    {
        // Auto-detect which boss scene we're in
        if (autoDetectScene)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            isBoss1Scene = sceneName == "boss1";
            isBoss2Scene = sceneName == "boss2";
        }

        // Auto-find components
        if (cameraZoom == null)
        {
            cameraZoom = GetComponent<BossIntroCameraZoom>();
        }

        if (bossDialogue == null)
        {
            bossDialogue = GetComponent<BossDialogue>();
        }
    }

    private void Start()
    {
        // Setup dialogues based on which boss scene
        if (cameraZoom != null && bossDialogue != null)
        {
            Dialogue[] dialogues = null;

            if (isBoss1Scene)
            {
                dialogues = bossDialogue.GetBoss1IntroDialogues();
                Debug.Log("[BossIntroController] Loaded Boss 1 intro dialogues");
            }
            else if (isBoss2Scene)
            {
                dialogues = bossDialogue.GetBoss2IntroDialogues();
                Debug.Log("[BossIntroController] Loaded Boss 2 intro dialogues");
            }

            // Assign dialogues to camera zoom system
            if (dialogues != null)
            {
                // Use reflection to set the private field (not ideal but works)
                var field = typeof(BossIntroCameraZoom).GetField("bossDialogues",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(cameraZoom, dialogues);
                }
            }
        }
    }

    /// <summary>
    /// Manually trigger boss intro (useful for testing or special events)
    /// </summary>
    public void TriggerBossIntro()
    {
        if (cameraZoom != null)
        {
            cameraZoom.TriggerBossIntro();
        }
    }

    /// <summary>
    /// Skip the intro
    /// </summary>
    public void SkipIntro()
    {
        if (cameraZoom != null)
        {
            cameraZoom.SkipIntro();
        }
    }
}
