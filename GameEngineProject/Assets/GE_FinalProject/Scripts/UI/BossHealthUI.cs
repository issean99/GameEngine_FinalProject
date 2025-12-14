using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 화면 상단에 보스 체력을 표시하는 UI 스크립트
/// boss_bar (배경 틀) 위에 boss_bar_filler (내부 채움)을 오버레이하여 표시
/// </summary>
public class BossHealthUI : MonoBehaviour
{
    [Header("Health Bar References")]
    [SerializeField] private Image healthBarFiller; // boss_bar_filler 이미지 (Filled 타입)

    [Header("Boss Reference")]
    [SerializeField] private bool autoFindBoss = true; // 자동으로 보스 찾기
    [SerializeField] private string bossTag = "Enemy"; // 보스의 태그 (일반 적과 동일하게 "Enemy" 사용)
    private Component bossController; // WizardBoss 또는 FinalBoss

    [Header("Optional Text")]
    [SerializeField] private TextMeshProUGUI healthText; // 체력 텍스트 (80/100 같은 형식) - 옵션

    [Header("Boss Name Object")]
    [SerializeField] private GameObject bossNameObject; // 보스 이름 오브젝트 (별도로 생성한 게임 오브젝트)

    [Header("Visibility Settings")]
    [SerializeField] private bool hideWhenBossDead = true; // 보스가 죽으면 UI 숨김
    [SerializeField] private float hideDelay = 2f; // 죽은 후 UI 숨기는 지연 시간
    [SerializeField] private bool waitForDialogue = true; // 다이얼로그가 끝날 때까지 대기

    private bool isBossDead = false;
    private bool dialogueCompleted = false; // 다이얼로그 완료 여부

    private void Start()
    {
        // 다이얼로그 대기가 활성화되어 있으면 UI를 숨김
        if (waitForDialogue)
        {
            gameObject.SetActive(false);

            // 다이얼로그 완료 시 콜백 등록
            DialogueManager.OnDialogueComplete(OnDialogueFinished);
            Debug.Log("[BossHealthUI] Waiting for dialogue to complete before showing boss health bar.");
        }
        else
        {
            // 다이얼로그 대기 없이 바로 표시
            InitializeBossHealthUI();
        }
    }

    /// <summary>
    /// 다이얼로그가 끝났을 때 호출되는 콜백
    /// </summary>
    private void OnDialogueFinished()
    {
        dialogueCompleted = true;
        Debug.Log("[BossHealthUI] Dialogue completed! Showing boss health bar.");

        // UI 활성화 및 초기화
        gameObject.SetActive(true);

        // 보스 이름 오브젝트도 활성화
        if (bossNameObject != null)
        {
            bossNameObject.SetActive(true);
        }

        InitializeBossHealthUI();
    }

    /// <summary>
    /// 보스 체력 UI 초기화
    /// </summary>
    private void InitializeBossHealthUI()
    {
        if (autoFindBoss)
        {
            FindBoss();
        }

        // 초기 체력 표시
        UpdateHealthUI();
    }

    private void Update()
    {
        // 보스가 없으면 찾기 시도
        if (bossController == null && autoFindBoss && !isBossDead)
        {
            FindBoss();
            return;
        }

        // 체력 UI 업데이트
        if (bossController != null && !isBossDead)
        {
            UpdateHealthUI();
        }
    }

    /// <summary>
    /// 보스 찾기
    /// </summary>
    private void FindBoss()
    {
        // Find all objects with boss tag
        GameObject[] potentialBosses = GameObject.FindGameObjectsWithTag(bossTag);

        foreach (GameObject obj in potentialBosses)
        {
            // Try to find WizardBoss component
            WizardBoss wizardBoss = obj.GetComponent<WizardBoss>();
            if (wizardBoss != null)
            {
                bossController = wizardBoss;
                Debug.Log("[BossHealthUI] Wizard Boss found and linked!");
                return;
            }

            // Try to find FinalBoss component
            FinalBoss finalBoss = obj.GetComponent<FinalBoss>();
            if (finalBoss != null)
            {
                bossController = finalBoss;
                Debug.Log("[BossHealthUI] Final Boss found and linked!");
                return;
            }
        }

        // If no boss found, hide the UI
        if (bossController == null)
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 체력 UI 업데이트 - fillAmount로 연속적 표시
    /// </summary>
    private void UpdateHealthUI()
    {
        if (healthBarFiller == null)
        {
            Debug.LogWarning("[BossHealthUI] Health Bar Filler is not assigned!");
            return;
        }

        int currentHealth = 0;
        int maxHealth = 1;

        // Get health from boss controller
        if (bossController is WizardBoss wizardBoss)
        {
            currentHealth = wizardBoss.CurrentHealth;
            maxHealth = wizardBoss.MaxHealth;

            // Check if boss is dead
            if (currentHealth <= 0 && !isBossDead)
            {
                OnBossDeath();
            }
        }
        else if (bossController is FinalBoss finalBoss)
        {
            currentHealth = finalBoss.CurrentHealth;
            maxHealth = finalBoss.MaxHealth;

            // Check if boss is dead
            if (currentHealth <= 0 && !isBossDead)
            {
                OnBossDeath();
            }
        }
        else
        {
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
    /// 보스가 죽었을 때 호출
    /// </summary>
    private void OnBossDeath()
    {
        isBossDead = true;
        Debug.Log("[BossHealthUI] Boss defeated!");

        if (hideWhenBossDead)
        {
            Invoke(nameof(HideUI), hideDelay);
        }
    }

    /// <summary>
    /// UI 숨기기
    /// </summary>
    private void HideUI()
    {
        gameObject.SetActive(false);

        // 보스 이름 오브젝트도 함께 숨김
        if (bossNameObject != null)
        {
            bossNameObject.SetActive(false);
            Debug.Log("[BossHealthUI] Boss name object hidden with health bar.");
        }
    }

    /// <summary>
    /// 외부에서 보스 설정
    /// </summary>
    public void SetBoss(WizardBoss boss)
    {
        bossController = boss;
        isBossDead = false;
        gameObject.SetActive(true);
        UpdateHealthUI();
    }

    /// <summary>
    /// 외부에서 보스 설정 (FinalBoss)
    /// </summary>
    public void SetBoss(FinalBoss boss)
    {
        bossController = boss;
        isBossDead = false;
        gameObject.SetActive(true);
        UpdateHealthUI();
    }
}
