using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 화면 왼쪽 위에 플레이어 체력을 표시하는 UI 스크립트
/// player_bar (배경 틀) 위에 player_bar_filler_health (내부 채움)을 오버레이하여 표시
/// </summary>
public class PlayerHealthUI : MonoBehaviour
{
    [Header("Health Bar References")]
    [SerializeField] private Image healthBarFiller; // player_bar_filler_health 이미지 (Filled 타입)

    [Header("Player Reference")]
    [SerializeField] private bool autoFindPlayer = true; // 자동으로 플레이어 찾기
    private PlayerController playerController;

    [Header("Optional Text")]
    [SerializeField] private TextMeshProUGUI healthText; // 체력 텍스트 (80/100 같은 형식) - 옵션

    private void Start()
    {
        if (autoFindPlayer)
        {
            FindPlayer();
        }

        // 초기 체력 표시
        if (playerController != null)
        {
            UpdateHealthUI(playerController.CurrentHealth, playerController.MaxHealth);
        }
    }

    private void Update()
    {
        // 플레이어가 없으면 찾기 시도
        if (playerController == null && autoFindPlayer)
        {
            FindPlayer();
            return;
        }

        // 체력 UI 업데이트
        if (playerController != null)
        {
            UpdateHealthUI(playerController.CurrentHealth, playerController.MaxHealth);
        }
    }

    /// <summary>
    /// 플레이어 찾기
    /// </summary>
    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                Debug.Log("[PlayerHealthUI] Player found and linked!");
            }
            else
            {
                Debug.LogWarning("[PlayerHealthUI] Player found but PlayerController component is missing!");
            }
        }
    }

    /// <summary>
    /// 체력 UI 업데이트 - fillAmount로 연속적 표시
    /// </summary>
    private void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (healthBarFiller == null)
        {
            Debug.LogWarning("[PlayerHealthUI] Health Bar Filler is not assigned!");
            return;
        }

        // fillAmount 계산 (0.0 ~ 1.0)
        float fillAmount = (float)currentHealth / maxHealth;
        fillAmount = Mathf.Clamp01(fillAmount); // 0~1 범위로 제한

        healthBarFiller.fillAmount = fillAmount;

        // 옵션: 체력 텍스트 업데이트
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }

    /// <summary>
    /// 외부에서 플레이어 설정
    /// </summary>
    public void SetPlayer(PlayerController player)
    {
        playerController = player;
        if (playerController != null)
        {
            UpdateHealthUI(playerController.CurrentHealth, playerController.MaxHealth);
        }
    }
}
