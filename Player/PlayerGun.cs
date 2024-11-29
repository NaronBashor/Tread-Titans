using UnityEngine;

public class PlayerGun : MonoBehaviour
{
    [Header("Gun Stats")]
    public float damage = 10f;          // Damage per shot
    public float fireRate = 1f;         // Shots per second
    [Range(0f, 100f)]
    public float accuracy = 100f;       // Accuracy percentage (100% is perfect accuracy)

    [Header("Projectile Settings")]
    public GameObject projectilePrefab; // Prefab of the projectile to be fired
    public Transform firePoint;         // Point from where the projectile is fired

    private float fireCooldown;         // Tracks the time between shots
    public Animator gunAnimator;

    private void Update()
    {
        if (fireCooldown > 0) {
            fireCooldown -= Time.deltaTime;
        }
    }

    public void SetStats(float damage, float fireRate, float accuracy)
    {
        this.damage = damage;
        this.fireRate = fireRate;
        this.accuracy = Mathf.Clamp(accuracy, 0f, 100f); // Clamp to ensure within 0-100%
    }

    public void TryFire()
    {
        if (fireCooldown <= 0) {
            Fire();
            fireCooldown = 1f / fireRate; // Reset cooldown based on fire rate
        }
        //Debug.DrawLine(firePoint.position, firePoint.position + (Vector3)firePoint.right * 2f, Color.red, 1f);
    }

    private void Fire()
    {
        if (projectilePrefab == null || firePoint == null) {
            Debug.LogWarning("Projectile prefab or fire point not set for TankGun.");
            return;
        }

        gunAnimator.SetTrigger("Shoot");

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projScript = projectile.GetComponent<Projectile>();

        if (projScript != null) {
            projScript.SetDamage(damage);
            Vector2 direction = ApplyInaccuracy(firePoint.right); // Apply inaccuracy if needed
            projScript.SetDirection(direction); // Use adjusted direction
        }
    }






    private Vector2 ApplyInaccuracy(Vector2 direction)
    {
        // Calculate inaccuracy based on accuracy stat
        float inaccuracyFactor = 1f - (accuracy / 100f);
        float angleOffset = Random.Range(-inaccuracyFactor, inaccuracyFactor) * 10f; // Adjust as needed
        Quaternion rotation = Quaternion.Euler(0, 0, angleOffset);
        return rotation * direction;
    }
}
