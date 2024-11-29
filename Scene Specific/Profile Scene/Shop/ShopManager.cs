using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public TrackManager trackManager;
    public GunManager gunManager;
    public ChassisManager chassisManager;
    private PlayerData playerData;

    [Header("UI Setup")]
    public GameObject buttonPrefab; // Assign the ShopItemButton prefab
    public Transform buttonContainer; // Container for the shop buttons
    public ItemDetailsPopup itemDetailsPopup; // Reference to the pop-up

    private void Start()
    {
        playerData = FindAnyObjectByType<PlayerData>();
        LoadUnlockData();

        PopulateShop();
    }

    private void LoadUnlockData()
    {
        // Load lock status for each item based on player data
        foreach (var track in trackManager.tracks) {
            track.isLocked = !playerData.IsTrackUnlocked(track.trackName);
        }

        foreach (var gun in gunManager.guns) {
            gun.isLocked = !playerData.IsGunUnlocked(gun.gunName);
        }

        foreach (var chassis in chassisManager.chassisOptions) {
            chassis.isLocked = !playerData.IsChassisUnlocked(chassis.chassisName);
        }
    }

    private void PopulateShop()
    {
        // Populate tracks
        foreach (var track in trackManager.tracks) {
            CreateButton(track);
        }

        // Populate guns
        foreach (var gun in gunManager.guns) {
            CreateButton(gun);
        }

        // Populate chassis
        foreach (var chassis in chassisManager.chassisOptions) {
            CreateButton(chassis);
        }
    }

    // Overloaded CreateButton for TankChassis
    private void CreateButton(TankChassis chassis)
    {
        var buttonObj = Instantiate(buttonPrefab, buttonContainer);
        var button = buttonObj.GetComponent<ShopItemButton>();

        button.Initialize(
            this,
            chassis.chassisName,
            "Chassis",
            200, // Example cost for chassis
            chassis.chassisSprite,
            chassis.isLocked,
            chassis.GetStats(),
            itemDetailsPopup
        );
    }

    // Overloaded CreateButton for TankGun
    private void CreateButton(TankGun gun)
    {
        var buttonObj = Instantiate(buttonPrefab, buttonContainer);
        var button = buttonObj.GetComponent<ShopItemButton>();

        button.Initialize(
            this,
            gun.gunName,
            "Gun",
            150, // Example cost for guns
            gun.gunSprite,
            gun.isLocked,
            gun.GetStats(),
            itemDetailsPopup
        );
    }

    // Overloaded CreateButton for TankTrack
    private void CreateButton(TankTrack track)
    {
        var buttonObj = Instantiate(buttonPrefab, buttonContainer);
        var button = buttonObj.GetComponent<ShopItemButton>();

        button.Initialize(
            this,
            track.trackName,
            "Track",
            100, // Example cost for tracks
            track.trackSprite,
            track.isLocked,
            track.GetStats(),
            itemDetailsPopup
        );
    }

    public bool UnlockItem(string itemName, string itemType, int unlockCost)
    {
        if (playerData.CanAfford(unlockCost)) {
            playerData.DeductCurrency(unlockCost);

            bool itemUnlocked = false;

            switch (itemType) {
                case "Track":
                    var track = trackManager.tracks.Find(t => t.trackName == itemName);
                    if (track != null && track.isLocked) {
                        track.isLocked = false;
                        playerData.UnlockItem(itemName, "Track");
                        itemUnlocked = true;
                    }
                    break;

                case "Gun":
                    var gun = gunManager.guns.Find(g => g.gunName == itemName);
                    if (gun != null && gun.isLocked) {
                        gun.isLocked = false;
                        playerData.UnlockItem(itemName, "Gun");
                        itemUnlocked = true;
                    }
                    break;

                case "Chassis":
                    var chassis = chassisManager.chassisOptions.Find(c => c.chassisName == itemName);
                    if (chassis != null && chassis.isLocked) {
                        chassis.isLocked = false;
                        playerData.UnlockItem(itemName, "Chassis");
                        itemUnlocked = true;
                    }
                    break;

                default:
                    Debug.LogWarning($"Unknown item type: {itemType}");
                    return false;
            }

            if (itemUnlocked) {
                playerData.SaveProfile();
                Debug.Log($"{itemType} '{itemName}' unlocked successfully.");
                return true;
            } else {
                Debug.LogWarning($"{itemType} '{itemName}' not found or already unlocked.");
            }
        } else {
            Debug.Log("Not enough currency to unlock this item.");
        }

        return false;
    }
}
