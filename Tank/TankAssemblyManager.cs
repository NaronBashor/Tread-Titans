using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TankAssemblyManager : MonoBehaviour
{
    [Header("Tank Parts")]
    public Transform chassisAnchor;
    public Transform leftTrack;
    public Transform rightTrack;
    public Transform gun;

    [Header("Gun Display (UI or SpriteRenderer)")]
    public Image gunImage;  // UI Image for the gun display on the tank
    public Image chasisImage;  // UI Image for the gun display on the tank
    public Image leftTrackImage;  // UI Image for the gun display on the tank
    public Image rightTrackImage;  // UI Image for the gun display on the tank

    [Header("Chassis Configurations")]
    public List<ChassisConfig> chassisConfigs;
    public List<Sprite> defaultTracks;

    public ChassisManager chassisManager;
    public GunManager gunManager;
    public TrackManager trackManager;

    private PlayerData playerData;

    private ChassisConfig currentChassisConfig;

    private void Start()
    {
        currentChassisConfig = chassisConfigs[0]; ;
    }

    public void SetChassis(int chassisIndex, Sprite image)
    {
        SaveSelectedParts();
        //Debug.Log("Chassis Configurations Count: " + chassisConfigs.Count); // Debug line to confirm list count
        if (chassisIndex < 0 || chassisIndex >= chassisConfigs.Count) {
            Debug.LogError("Invalid chassis index.");
            return;
        }

        chasisImage.sprite = image;

        currentChassisConfig = chassisConfigs[chassisIndex];
        ApplyOffsets(); // Apply offsets based on the selected chassis
    }

    public void SetTrackSprites(Sprite leftSprite, Sprite rightSprite)
    {
        SaveSelectedParts();
        if (leftTrackImage != null) leftTrackImage.sprite = leftSprite;
        if (rightTrackImage != null) rightTrackImage.sprite = rightSprite;

        ApplyOffsets(); // Apply offsets whenever track sprites change
    }

    public void SetGunPosition()
    {
        ApplyOffsets(); // Apply offsets whenever the gun position is updated
    }

    // Method to update the gun sprite on the main tank
    public void SetGunSprite(Sprite gunSprite)
    {
        SaveSelectedParts();
        if (gunImage != null) {
            gunImage.sprite = gunSprite;
        } else {
            Debug.LogWarning("Gun image reference is missing in TankAssemblyManager.");
        }
    }

    // This method updates positions for tracks and gun based on the selected chassis configuration
    private void ApplyOffsets()
    {
        if (currentChassisConfig != null) {
            leftTrack.localPosition = chassisAnchor.localPosition + currentChassisConfig.leftTrackOffset;
            leftTrack.localScale = currentChassisConfig.trackScale;

            rightTrack.localPosition = chassisAnchor.localPosition + currentChassisConfig.rightTrackOffset;
            rightTrack.localScale = currentChassisConfig.trackScale;

            gun.localPosition = chassisAnchor.localPosition + currentChassisConfig.gunOffset;
        } else {
            Debug.LogWarning("Chassis configuration not set.");
        }
    }

    public void SaveSelectedParts()
    {
        playerData = FindAnyObjectByType<PlayerData>();

        var chassis = chassisManager.GetSelectedChassis();
        var gun = gunManager.GetSelectedGun();
        var track = trackManager.GetSelectedTrack();

        playerData.SetSelectedParts(chassis, gun, track);
    }
}
