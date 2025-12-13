using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 씬의 모든 버튼에 자동으로 사운드 효과 추가
/// Canvas에 부착하면 자동으로 모든 자식 버튼에 적용됨
/// </summary>
public class ButtonSoundManager : MonoBehaviour
{
    [Header("Default Button Sounds")]
    [SerializeField] private AudioClip defaultClickSound;
    [SerializeField] private AudioClip defaultHoverSound;
    [SerializeField] [Range(0f, 1f)] private float defaultVolume = 0.5f;
    [SerializeField] private bool addHoverSound = false;

    [Header("Auto-Setup")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool includeInactiveButtons = true;

    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupAllButtons();
        }
    }

    /// <summary>
    /// 모든 버튼에 사운드 효과 추가
    /// </summary>
    [ContextMenu("Setup All Buttons")]
    public void SetupAllButtons()
    {
        // 현재 GameObject와 모든 자식에서 Button 컴포넌트 찾기
        Button[] buttons = GetComponentsInChildren<Button>(includeInactiveButtons);

        int setupCount = 0;
        foreach (Button button in buttons)
        {
            // 이미 ButtonSoundEffect가 있는지 확인
            ButtonSoundEffect soundEffect = button.GetComponent<ButtonSoundEffect>();

            if (soundEffect == null)
            {
                // ButtonSoundEffect 추가
                soundEffect = button.gameObject.AddComponent<ButtonSoundEffect>();
                setupCount++;
            }

            // 기본 사운드 설정 (Inspector에서 설정되지 않은 경우에만)
            if (defaultClickSound != null)
            {
                // Reflection을 사용하여 private field 설정
                var clickSoundField = typeof(ButtonSoundEffect).GetField("clickSound",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (clickSoundField != null && clickSoundField.GetValue(soundEffect) == null)
                {
                    clickSoundField.SetValue(soundEffect, defaultClickSound);
                }

                var volumeField = typeof(ButtonSoundEffect).GetField("volume",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (volumeField != null)
                {
                    volumeField.SetValue(soundEffect, defaultVolume);
                }

                if (addHoverSound && defaultHoverSound != null)
                {
                    var hoverSoundField = typeof(ButtonSoundEffect).GetField("hoverSound",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (hoverSoundField != null)
                    {
                        hoverSoundField.SetValue(soundEffect, defaultHoverSound);
                    }

                    var playOnHoverField = typeof(ButtonSoundEffect).GetField("playOnHover",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (playOnHoverField != null)
                    {
                        playOnHoverField.SetValue(soundEffect, true);
                    }
                }
            }
        }

        Debug.Log($"[ButtonSoundManager] Setup complete! Added sound effects to {setupCount} buttons (Total buttons: {buttons.Length})");
    }

    /// <summary>
    /// 모든 버튼에서 사운드 효과 제거
    /// </summary>
    [ContextMenu("Remove All Button Sounds")]
    public void RemoveAllButtonSounds()
    {
        ButtonSoundEffect[] soundEffects = GetComponentsInChildren<ButtonSoundEffect>(includeInactiveButtons);

        foreach (ButtonSoundEffect effect in soundEffects)
        {
            DestroyImmediate(effect);
        }

        Debug.Log($"[ButtonSoundManager] Removed {soundEffects.Length} button sound effects");
    }
}
