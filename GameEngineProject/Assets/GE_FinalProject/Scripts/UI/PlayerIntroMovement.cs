using UnityEngine;

/// <summary>
/// 인트로 씬에서 플레이어 이동을 위한 간단한 스크립트
/// StartSceneManager가 있다면 이 스크립트는 선택사항입니다.
/// 이 스크립트는 플레이어 오브젝트에 직접 붙여서 사용할 수 있습니다.
/// </summary>
public class PlayerIntroMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private bool shouldMove = false; // 외부에서 제어

    private Animator animator;
    private Rigidbody2D rb;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (shouldMove)
        {
            MoveUp();
        }
        else
        {
            StopMovement();
        }
    }

    private void MoveUp()
    {
        // 위로 이동
        Vector2 movement = Vector2.up * moveSpeed;

        if (rb != null)
        {
            rb.linearVelocity = movement;
        }
        else
        {
            transform.Translate(movement * Time.deltaTime, Space.World);
        }

        // 애니메이션 설정
        if (animator != null)
        {
            animator.SetFloat("MoveSpeed", 1f);
        }
    }

    private void StopMovement()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (animator != null)
        {
            animator.SetFloat("MoveSpeed", 0f);
        }
    }

    // 외부에서 이동 시작/정지를 제어하는 메서드
    public void StartMoving()
    {
        shouldMove = true;
    }

    public void StopMoving()
    {
        shouldMove = false;
    }
}
