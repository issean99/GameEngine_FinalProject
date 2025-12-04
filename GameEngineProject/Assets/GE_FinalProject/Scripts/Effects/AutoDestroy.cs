using UnityEngine;

/// <summary>
/// Automatically destroys GameObject after a set lifetime
/// 설정된 시간 후 자동으로 GameObject를 삭제
/// </summary>
public class AutoDestroy : MonoBehaviour
{
    [Header("Destroy Settings")]
    [SerializeField] private float lifetime = 1f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
