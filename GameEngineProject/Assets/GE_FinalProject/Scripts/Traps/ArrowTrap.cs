using System.Collections;
using UnityEngine;

public class ArrowTrap : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 10f; // Range to detect player
    [SerializeField] private bool startActive = true; // Start shooting immediately

    [Header("Arrow Settings")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform firePoint; // Where arrows spawn from
    [SerializeField] private float arrowSpeed = 8f;
    [SerializeField] private int arrowDamage = 10;
    [SerializeField] private float stunDuration = 1.5f; // Stun duration in seconds

    [Header("Shooting Pattern")]
    [SerializeField] private ArrowPattern shootingPattern = ArrowPattern.Single;
    [SerializeField] private float fireInterval = 2f; // Time between volleys
    [SerializeField] private float burstDelay = 0.1f; // Delay between arrows in a volley

    [Header("Multi-shot Settings")]
    [SerializeField] private float spreadAngle = 30f; // Spread angle for multi-shot (degrees)

    [Header("Boss Connection (Optional)")]
    [SerializeField] private GameObject linkedBoss; // Boss that controls this trap

    [Header("Sound Effects")]
    [SerializeField] private AudioClip shootSound; // 화살 발사 소리
    [SerializeField] [Range(0f, 1f)] private float volume = 0.7f;

    private Transform player;
    private float nextFireTime;
    private bool isActive = true;
    private AudioSource audioSource;

    public enum ArrowPattern
    {
        Single,      // 1 arrow
        Triple,      // 3 arrows in a spread
        Quintuple    // 5 arrows in a spread
    }

    private void Awake()
    {
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // Use this object as fire point if none assigned
        if (firePoint == null)
        {
            firePoint = transform;
        }

        // Add AudioSource for sound effects
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
        audioSource.volume = volume;

        isActive = startActive;
        nextFireTime = Time.time + fireInterval;
    }

    private void Update()
    {
        if (!isActive || player == null) return;

        // Check if linked boss is dead
        if (linkedBoss != null && !IsBossAlive())
        {
            isActive = false;
            return;
        }

        // Check if player is in range
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange && Time.time >= nextFireTime)
        {
            StartCoroutine(ShootArrows());
            nextFireTime = Time.time + fireInterval;
        }
    }

    private IEnumerator ShootArrows()
    {
        Vector2 directionToPlayer = (player.position - firePoint.position).normalized;

        switch (shootingPattern)
        {
            case ArrowPattern.Single:
                ShootArrow(directionToPlayer, 0f);
                break;

            case ArrowPattern.Triple:
                // Shoot 3 arrows in a spread pattern
                ShootArrow(directionToPlayer, -spreadAngle / 2f);
                yield return new WaitForSeconds(burstDelay);
                ShootArrow(directionToPlayer, 0f);
                yield return new WaitForSeconds(burstDelay);
                ShootArrow(directionToPlayer, spreadAngle / 2f);
                break;

            case ArrowPattern.Quintuple:
                // Shoot 5 arrows in a spread pattern
                ShootArrow(directionToPlayer, -spreadAngle);
                yield return new WaitForSeconds(burstDelay);
                ShootArrow(directionToPlayer, -spreadAngle / 2f);
                yield return new WaitForSeconds(burstDelay);
                ShootArrow(directionToPlayer, 0f);
                yield return new WaitForSeconds(burstDelay);
                ShootArrow(directionToPlayer, spreadAngle / 2f);
                yield return new WaitForSeconds(burstDelay);
                ShootArrow(directionToPlayer, spreadAngle);
                break;
        }
    }

    private void ShootArrow(Vector2 baseDirection, float angleOffset)
    {
        if (arrowPrefab == null) return;

        // Play shoot sound
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound, volume);
        }

        // Apply angle offset
        float angle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg + angleOffset;
        Vector2 direction = new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        );

        // Spawn arrow
        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);
        ArrowProjectile arrowScript = arrow.GetComponent<ArrowProjectile>();

        if (arrowScript != null)
        {
            arrowScript.Initialize(direction, arrowSpeed, arrowDamage, stunDuration);
        }
    }

    // Public methods to control the trap
    public void Activate()
    {
        isActive = true;
    }

    public void Deactivate()
    {
        isActive = false;
    }

    public void SetPattern(ArrowPattern pattern)
    {
        shootingPattern = pattern;
    }

    private bool IsBossAlive()
    {
        if (linkedBoss == null) return true;

        // Check for WizardBoss
        WizardBoss wizardBoss = linkedBoss.GetComponent<WizardBoss>();
        if (wizardBoss != null)
        {
            return !wizardBoss.IsDead();
        }

        // Check for FinalBoss
        FinalBoss finalBoss = linkedBoss.GetComponent<FinalBoss>();
        if (finalBoss != null)
        {
            return !finalBoss.IsDead();
        }

        // Check if boss GameObject is active
        return linkedBoss.activeInHierarchy;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw fire point
        if (firePoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(firePoint.position, 0.2f);
        }
    }
}
