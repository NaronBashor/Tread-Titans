using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

public class PlayerData : MonoBehaviour
{
    public string playerName = ".";
    public int currentGunIndex = 0;
    public int currentTracksIndex = 0;
    public int currentChassisIndex = 0;
    public int currency = 10000;

    public List<string> unlockedChassis = new List<string>();
    public List<string> unlockedGuns = new List<string>();
    public List<string> unlockedTracks = new List<string>();

    public TankChassis selectedChassis;
    public TankGun selectedGun;
    public TankTrack selectedTrack;

    public bool needPlayerName = true;

    private string saveFilePath;

    public event Action<int> OnCurrencyChanged;

    private void Awake()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "playerData.json");

        if (!File.Exists(saveFilePath)) {
            InitializeNewGame();
        } else {
            LoadData();
        }

        CheckPlayerName();
    }

    // This is added currency for testing, remove for game build
    private void Start()
    {
        currency += 100000;
    }

    private void CheckPlayerName()
    {
        needPlayerName = string.IsNullOrWhiteSpace(playerName) || playerName.Length <= 2;
    }

    public void SaveProfile()
    {
        if (string.IsNullOrEmpty(saveFilePath)) return;

        try {
            string json = JsonUtility.ToJson(this, true);
            File.WriteAllText(saveFilePath, json);
        } catch (Exception e) {
            Debug.LogError("Failed to save player data: " + e.Message);
        }
    }

    public void LoadData()
    {
        try {
            if (File.Exists(saveFilePath)) {
                string json = File.ReadAllText(saveFilePath);
                JsonUtility.FromJsonOverwrite(json, this);
            }
        } catch (Exception e) {
            Debug.LogError("Failed to load player data: " + e.Message);
            InitializeNewGame();
        }
    }

    private void InitializeNewGame()
    {
        var gunManager = FindAnyObjectByType<GunManager>();
        var trackManager = FindAnyObjectByType<TrackManager>();
        var chassisManager = FindAnyObjectByType<ChassisManager>();

        gunManager?.InitializeGuns();
        trackManager?.InitializeTracks();
        chassisManager?.InitializeChassis();

        if (gunManager != null && gunManager.guns.Count > 0)
            unlockedGuns.Add(gunManager.guns[0].gunName);

        if (trackManager != null && trackManager.tracks.Count > 0)
            unlockedTracks.Add(trackManager.tracks[0].trackName);

        if (chassisManager != null && chassisManager.chassisOptions.Count > 0)
            unlockedChassis.Add(chassisManager.chassisOptions[0].chassisName);

        SaveProfile();
    }

    public bool IsChassisUnlocked(string chassisName) => unlockedChassis.Contains(chassisName);
    public bool IsGunUnlocked(string gunName) => unlockedGuns.Contains(gunName);
    public bool IsTrackUnlocked(string trackName) => unlockedTracks.Contains(trackName);

    public void UnlockItem(string itemName, string itemType)
    {
        bool itemAlreadyUnlocked = false;

        switch (itemType) {
            case "Chassis":
                itemAlreadyUnlocked = unlockedChassis.Contains(itemName);
                if (!itemAlreadyUnlocked) unlockedChassis.Add(itemName);
                break;
            case "Gun":
                itemAlreadyUnlocked = unlockedGuns.Contains(itemName);
                if (!itemAlreadyUnlocked) unlockedGuns.Add(itemName);
                break;
            case "Track":
                itemAlreadyUnlocked = unlockedTracks.Contains(itemName);
                if (!itemAlreadyUnlocked) unlockedTracks.Add(itemName);
                break;
            default:
                Debug.LogWarning("Unknown item type: " + itemType);
                return;
        }

        if (!itemAlreadyUnlocked) {
            SaveProfile();
            Debug.Log($"{itemType} '{itemName}' unlocked and saved.");
        } else {
            Debug.LogWarning($"{itemType} '{itemName}' is already unlocked.");
        }
    }

    public void SetSelectedParts(TankChassis chassis, TankGun gun, TankTrack track)
    {
        if (IsChassisUnlocked(chassis.chassisName))
            selectedChassis = chassis;
        else
            Debug.LogWarning("Selected chassis is not unlocked.");

        if (IsGunUnlocked(gun.gunName))
            selectedGun = gun;
        else
            Debug.LogWarning("Selected gun is not unlocked.");

        if (IsTrackUnlocked(track.trackName))
            selectedTrack = track;
        else
            Debug.LogWarning("Selected track is not unlocked.");
    }

    public bool CanAfford(int amount) => currency >= amount;

    public void DeductCurrency(int amount)
    {
        if (CanAfford(amount)) {
            currency -= amount;
            SaveProfile();
            OnCurrencyChanged?.Invoke(currency);
        } else {
            Debug.LogWarning("Not enough currency to complete the transaction.");
        }
    }

    public void AddCurrency(int amount)
    {
        currency += amount;
        SaveProfile();
        OnCurrencyChanged?.Invoke(currency);
    }

    public int GetCurrency()
    {
        return currency;
    }
}
