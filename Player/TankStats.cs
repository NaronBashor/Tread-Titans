using UnityEngine;

public class TankStats : MonoBehaviour
{
    [Header("Base Tank Stats")]
    private const float MAX_ALLOWED_SPEED = 5f; // Maximum allowed speed

    private float finalSpeed;
    private float finalArmor;
    private float finalWeight;

    private float finalGunDamage;
    private float finalGunFireRate;
    private float finalGunAccuracy;

    private PlayerData playerData;

    private void Start()
    {
        playerData = FindAnyObjectByType<PlayerData>();
        if (playerData == null) {
            Debug.LogError("PlayerData not found in the scene.");
            return;
        }

        CalculateFinalStats();
    }

    private void CalculateFinalStats()
    {
        if (playerData.selectedChassis != null) {
            finalArmor = playerData.selectedChassis.armor;
            finalSpeed = playerData.selectedChassis.speed;
            finalWeight = playerData.selectedChassis.weight;
        }

        if (playerData.selectedGun != null) {
            finalGunDamage = playerData.selectedGun.damage;
            finalGunFireRate = playerData.selectedGun.fireRate;
            finalGunAccuracy = playerData.selectedGun.accuracy;
        }

        if (playerData.selectedTrack != null) {
            SetTrackAnimation(playerData.selectedTrack.trackTypeId);
        }

        ApplyStatsToTank();
    }

    private void SetTrackAnimation(int trackTypeId)
    {
        var leftTrackAnimator = transform.Find("Left Track").GetComponent<Animator>();
        var rightTrackAnimator = transform.Find("Right Track").GetComponent<Animator>();

        if (leftTrackAnimator != null && rightTrackAnimator != null) {
            leftTrackAnimator.SetInteger("TrackType", trackTypeId);
            rightTrackAnimator.SetInteger("TrackType", trackTypeId);
        }
    }

    private void ApplyStatsToTank()
    {
        // Example: set these values on the tank's movement and health components
        var tankMovement = GetComponent<TankController>();
        var tankHealth = GetComponent<TankHealth>();
        var playerGun = GetComponent<PlayerGun>();

        if (tankMovement != null) tankMovement.SetSpeed(Mathf.Min(finalSpeed, MAX_ALLOWED_SPEED));
        if (tankHealth != null) tankHealth.SetArmor(finalArmor);

        if (playerGun != null) playerGun.SetStats(finalGunDamage, finalGunFireRate, finalGunAccuracy);
    }
}
