using System.Collections.Generic;
using UnityEngine;

public class TankInitializer : MonoBehaviour
{
    [Header("Tank Parts")]
    [SerializeField] private List<Sprite> guns;
    [SerializeField] private List<Sprite> tracks;
    [SerializeField] private List<Sprite> chassis;

    [Header("Tank Renderers")]
    [SerializeField] private SpriteRenderer tankGunRenderer;
    [SerializeField] private SpriteRenderer leftTrackRenderer;
    [SerializeField] private SpriteRenderer rightTrackRenderer;
    [SerializeField] private SpriteRenderer tankChassisRenderer;

    [Header("Chassis Configurations")]
    [SerializeField] private List<ChassisConfig> chassisConfigs;

    private int gunIndex;
    private int tracksIndex;
    private int chassisIndex;

    private ChassisConfig currentChassisConfig;
    private PlayerData playerData;

    private void Start()
    {
        playerData = FindAnyObjectByType<PlayerData>();
        if (playerData == null) {
            Debug.LogError("PlayerData not found in the scene.");
            return;
        }

        // Load customization data from PlayerData
        LoadCustomization();

        // Apply customization to this player instance's renderers
        ApplyCustomization();
    }

    private void LoadCustomization()
    {
        gunIndex = playerData.currentGunIndex;
        tracksIndex = playerData.currentTracksIndex;
        chassisIndex = playerData.currentChassisIndex;

        // Get the chassis config based on the saved chassis index
        if (chassisIndex >= 0 && chassisIndex < chassisConfigs.Count) {
            currentChassisConfig = chassisConfigs[chassisIndex];
        } else {
            Debug.LogWarning("Invalid chassis index; using default chassis config.");
            currentChassisConfig = null;
        }
    }

    private void ApplyCustomization()
    {
        // Apply selected sprites to the player prefab's renderers
        if (guns.Count > gunIndex) tankGunRenderer.sprite = guns[gunIndex];
        if (tracks.Count > tracksIndex) {
            leftTrackRenderer.sprite = tracks[tracksIndex];
            rightTrackRenderer.sprite = tracks[tracksIndex];
        }
        if (chassis.Count > chassisIndex) tankChassisRenderer.sprite = chassis[chassisIndex];

        // Apply offsets and scale if a valid chassis config is set
        if (currentChassisConfig != null) {
            ApplyOffsetsAndScale();
        }
    }

    private void ApplyOffsetsAndScale()
    {
        // Apply the track offsets and scale based on the chassis configuration
        leftTrackRenderer.transform.localPosition = currentChassisConfig.leftTrackOffset;
        leftTrackRenderer.transform.localScale = currentChassisConfig.trackScale;

        rightTrackRenderer.transform.localPosition = currentChassisConfig.rightTrackOffset;
        rightTrackRenderer.transform.localScale = currentChassisConfig.trackScale;

        // Apply the gun offset
        tankGunRenderer.transform.localPosition = currentChassisConfig.gunOffset;
    }
}
