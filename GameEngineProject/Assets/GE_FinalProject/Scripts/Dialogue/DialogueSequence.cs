using UnityEngine;

/// <summary>
/// Container for multiple dialogue lines (for Inspector setup)
/// 여러 대사를 포함하는 컨테이너 (Inspector에서 설정 가능)
/// </summary>
[CreateAssetMenu(fileName = "NewDialogueSequence", menuName = "Dialogue/Dialogue Sequence")]
public class DialogueSequence : ScriptableObject
{
    [Header("Dialogue Settings")]
    [Tooltip("이 대화 시퀀스의 이름/설명")]
    public string sequenceName;

    [Tooltip("대화 시퀀스의 대사 목록")]
    public Dialogue[] dialogues;

    [Header("Sequence Settings")]
    [Tooltip("대화 중 게임을 일시정지할지 여부")]
    public bool pauseGame = true;

    [Tooltip("대화 중 플레이어 조작을 비활성화할지 여부")]
    public bool disablePlayerControl = true;

    [Tooltip("대화 종료 후 실행할 이벤트 이름 (선택사항)")]
    public string onCompleteEventName;
}
