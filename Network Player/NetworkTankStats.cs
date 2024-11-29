using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkTankStats : NetworkBehaviour
{
    [Header("Base Tank Stats")]
    private const float MAX_ALLOWED_SPEED = 5f;

    private float finalSpeed;
    private float finalArmor;
    private float finalWeight;

    private float finalGunDamage;
    private float finalGunFireRate;
    private float finalGunAccuracy;

    private PlayerData playerData;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        playerData = FindAnyObjectByType<PlayerData>();
        if (playerData == null) {
            Debug.LogError("PlayerData not found in the scene.");
            return;
        }

        CalculateFinalStats();
        CalculateFinalStatsServer();
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
    }

    private void CalculateFinalStatsServer()
    {
        ApplyStatsToTankClient(finalSpeed, finalArmor, finalGunDamage, finalGunFireRate, finalGunAccuracy);
    }

    private void ApplyStatsToTankClient(float speed, float armor, float damage, float fireRate, float accuracy)
    {
        var tankMovement = GetComponent<NetworkTankController>();
        var tankHealth = GetComponent<NetworkTankHealth>();
        var playerGun = GetComponent<NetworkTankController>();

        if (tankMovement != null) {
            //tankMovement.SetSpeed(Mathf.Min(speed, MAX_ALLOWED_SPEED));
        }
        if (tankHealth != null) {
            //tankHealth.SetArmor(armor);
        }
        if (playerGun != null) {
            playerGun.SetStats(damage, fireRate, accuracy);
        }
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
}
