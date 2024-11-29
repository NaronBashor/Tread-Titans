using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkTankInitializer : NetworkBehaviour
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

    private NetworkVariable<int> gunIndex = new NetworkVariable<int>();
    private NetworkVariable<int> tracksIndex = new NetworkVariable<int>();
    private NetworkVariable<int> chassisIndex = new NetworkVariable<int>();

    private ChassisConfig currentChassisConfig;
    private PlayerData playerData;

    public override void OnNetworkSpawn()
    {
        if (IsServer) {
            InitializeCustomizationData();  // Server sets initial customization values
            UpdateCustomizationClientRpc(gunIndex.Value, tracksIndex.Value, chassisIndex.Value, OwnerClientId); // Sync to clients
        }

        // Listen for customization changes and apply them only to this specific instance
        gunIndex.OnValueChanged += OnCustomizationChanged;
        tracksIndex.OnValueChanged += OnCustomizationChanged;
        chassisIndex.OnValueChanged += OnCustomizationChanged;

        if (IsClient) {
            ApplyCustomizationClient(gunIndex.Value, tracksIndex.Value, chassisIndex.Value);
        }
    }

    private void InitializeCustomizationData()
    {
        playerData = FindAnyObjectByType<PlayerData>();
        if (playerData != null) {
            gunIndex.Value = playerData.currentGunIndex;
            tracksIndex.Value = playerData.currentTracksIndex;
            chassisIndex.Value = playerData.currentChassisIndex;
        } else {
            Debug.LogError("PlayerData not found in the scene.");
        }
    }

    private void OnCustomizationChanged(int oldValue, int newValue)
    {
        if (IsClient) {
            ApplyCustomizationClient(gunIndex.Value, tracksIndex.Value, chassisIndex.Value);
        }
    }

    [ClientRpc]
    private void UpdateCustomizationClientRpc(int gunIdx, int tracksIdx, int chassisIdx, ulong ownerClientId)
    {
        // Only apply the customization for the tank owned by this client
        if (NetworkManager.Singleton.LocalClientId == ownerClientId) {
            ApplyCustomizationClient(gunIdx, tracksIdx, chassisIdx);
        }
    }

    private void ApplyCustomizationClient(int gunIdx, int tracksIdx, int chassisIdx)
    {
        if (guns.Count > gunIdx) {
            tankGunRenderer.sprite = guns[gunIdx];
        }

        if (tracks.Count > tracksIdx) {
            leftTrackRenderer.sprite = tracks[tracksIdx];
            rightTrackRenderer.sprite = tracks[tracksIdx];
        }

        if (chassis.Count > chassisIdx) {
            tankChassisRenderer.sprite = chassis[chassisIdx];
            currentChassisConfig = chassisConfigs[chassisIdx];
        }

        if (currentChassisConfig != null) {
            ApplyOffsetsAndScale();
        }
    }

    private void ApplyOffsetsAndScale()
    {
        if (currentChassisConfig == null) return;

        leftTrackRenderer.transform.localPosition = currentChassisConfig.leftTrackOffset;
        leftTrackRenderer.transform.localScale = currentChassisConfig.trackScale;

        rightTrackRenderer.transform.localPosition = currentChassisConfig.rightTrackOffset;
        rightTrackRenderer.transform.localScale = currentChassisConfig.trackScale;

        tankGunRenderer.transform.localPosition = currentChassisConfig.gunOffset;
    }
}
