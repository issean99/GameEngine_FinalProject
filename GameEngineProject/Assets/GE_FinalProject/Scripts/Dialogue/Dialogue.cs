using UnityEngine;

/// <summary>
/// Single dialogue line with speaker name and text
/// 화자 이름과 대사를 포함하는 단일 대사
/// </summary>
[System.Serializable]
public class Dialogue
{
    [Header("Speaker")]
    [Tooltip("화자 이름 (예: 플레이어, 위저드, 등)")]
    public string speakerName;

    [Header("Dialogue Text")]
    [TextArea(2, 5)]
    [Tooltip("표시할 대사 텍스트")]
    public string text;

    [Header("Optional Settings")]
    [Tooltip("이 대사를 표시하는 시간 (0이면 플레이어가 넘김)")]
    public float displayDuration = 0f;

    [Tooltip("대사 표시 속도 (글자/초, 0이면 기본값 사용)")]
    public float typingSpeed = 0f;

    // Constructor for easy creation in code
    public Dialogue(string speaker, string dialogueText)
    {
        speakerName = speaker;
        text = dialogueText;
        displayDuration = 0f;
        typingSpeed = 0f;
    }

    public Dialogue(string speaker, string dialogueText, float duration)
    {
        speakerName = speaker;
        text = dialogueText;
        displayDuration = duration;
        typingSpeed = 0f;
    }
}
