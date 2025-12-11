using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Triggers dialogue when player enters trigger zone or on event
/// 플레이어가 트리거 영역에 들어가거나 이벤트 발생 시 대사 재생
/// </summary>
public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [Tooltip("대화 시퀀스 (ScriptableObject)")]
    [SerializeField] private DialogueSequence dialogueSequence;

    [Header("Or Define Dialogues Directly")]
    [Tooltip("직접 정의한 대사 목록 (ScriptableObject가 없을 때)")]
    [SerializeField] private Dialogue[] dialogues;

    [Header("Trigger Settings")]
    [Tooltip("자동으로 대화를 시작할지 (Collider 진입 시)")]
    [SerializeField] private bool triggerOnEnter = true;

    [Tooltip("한 번만 재생할지 여부")]
    [SerializeField] private bool playOnce = true;

    [Tooltip("트리거 후 이 GameObject를 비활성화할지")]
    [SerializeField] private bool disableAfterTrigger = false;

    [Header("Game Control")]
    [Tooltip("대화 중 게임 일시정지")]
    [SerializeField] private bool pauseGame = true;

    [Tooltip("대화 중 플레이어 조작 비활성화")]
    [SerializeField] private bool disablePlayerControl = true;

    [Header("Events")]
    [Tooltip("대화 완료 시 실행할 이벤트")]
    [SerializeField] private UnityEvent onDialogueComplete;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggerOnEnter) return;
        if (playOnce && hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            TriggerDialogue();
        }
    }

    /// <summary>
    /// Manually trigger dialogue (can be called from other scripts or buttons)
    /// </summary>
    public void TriggerDialogue()
    {
        if (playOnce && hasTriggered) return;

        hasTriggered = true;

        // Use ScriptableObject sequence if available
        if (dialogueSequence != null)
        {
            DialogueManager.StartDialogue(dialogueSequence);
        }
        // Otherwise use direct dialogues array
        else if (dialogues != null && dialogues.Length > 0)
        {
            DialogueManager.StartDialogue(dialogues, pauseGame, disablePlayerControl);
        }
        else
        {
            Debug.LogWarning($"[DialogueTrigger] No dialogues assigned on {gameObject.name}!");
            return;
        }

        // Add completion callback
        if (onDialogueComplete != null)
        {
            DialogueManager.OnDialogueComplete(() => onDialogueComplete.Invoke());
        }

        // Disable after trigger if set
        if (disableAfterTrigger)
        {
            DialogueManager.OnDialogueComplete(() => gameObject.SetActive(false));
        }

        Debug.Log($"[DialogueTrigger] Triggered dialogue on {gameObject.name}");
    }

    /// <summary>
    /// Reset trigger so it can be played again
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
    }

    private void OnDrawGizmos()
    {
        // Visualize trigger area
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && col.isTrigger)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f);

            if (col is BoxCollider2D boxCol)
            {
                Vector3 center = transform.position + (Vector3)boxCol.offset;
                Vector3 size = new Vector3(boxCol.size.x, boxCol.size.y, 0.1f);
                Gizmos.DrawCube(center, size);
            }
            else if (col is CircleCollider2D circleCol)
            {
                Vector3 center = transform.position + (Vector3)circleCol.offset;
                Gizmos.DrawSphere(center, circleCol.radius);
            }
        }
    }
}
