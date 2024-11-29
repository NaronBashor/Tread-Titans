using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private static SaveManager instance;
    public static SaveManager Instance
    {
        get {
            if (instance == null) {
                //Debug.LogError("SaveManager instance is null! Ensure SaveManager exists in the scene.");
            }
            return instance;
        }
    }

    private string saveFilePath;
    private GameData gameData;

    private void Awake()
    {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes
        saveFilePath = Path.Combine(Application.persistentDataPath, "game_save.json");

        LoadGame();
    }

    public void NewGame()
    {
        gameData = new GameData {
            soundOn = true,
            soundVolume = 1.0f,  // Default volume level
            fullscreen = true
        };
        SaveGame(); // Create a new save file with default values
    }

    public void DeleteGame()
    {
        if (File.Exists(saveFilePath)) {
            //Debug.Log("Account reset.");
            File.Delete(saveFilePath);
        }
    }

    public void LoadGame()
    {
        if (File.Exists(saveFilePath)) {
            string json = File.ReadAllText(saveFilePath);
            gameData = JsonUtility.FromJson<GameData>(json);
            //Debug.Log("Game data loaded successfully.");
        } else {
            //Debug.LogWarning("No save file found. Starting new game.");
            NewGame();
        }
    }

    public void SaveGame()
    {
        if (gameData == null) {
            //Debug.LogWarning("No game data to save.");
            return;
        }

        string json = JsonUtility.ToJson(gameData, true);
        File.WriteAllText(saveFilePath, json);
        //Debug.Log("Game data saved successfully.");
    }

    // Immediate save after each change for reliability
    public void OnSoundToggleValueChanged(bool isOn)
    {
        if (gameData != null) {
            gameData.soundOn = isOn;
            SaveGame();
        }
    }

    // Update the sound setting and save immediately
    public void SetSoundOn(bool value)
    {
        if (gameData != null) {
            gameData.soundOn = value;
            SaveGame();
            //Debug.Log("SoundOn saved as: " + value); // Log to confirm saving
        }
    }

    public float GetSoundVolume()
    {
        return gameData != null ? gameData.soundVolume : 1.0f; // Default to 1.0 if no data
    }

    public void SetSoundVolume(float value)
    {
        if (gameData != null) {
            gameData.soundVolume = Mathf.Clamp01(value);
            //Debug.Log("Slider passed volume: " + value); // Log the incoming value
            SaveGame(); // Save immediately
        }
    }

    // Update the fullscreen setting and save immediately
    public void SetFullscreen(bool value)
    {
        if (gameData != null) {
            gameData.fullscreen = value;
            Screen.fullScreen = value; // Apply fullscreen setting immediately
            SaveGame();
            //Debug.Log("Fullscreen saved as: " + value); // Log to confirm saving
        }
    }

    // Accessor methods for retrieving saved values
    public bool IsSoundOn() => gameData != null && gameData.soundOn;
    public bool IsFullscreen() => gameData != null && gameData.fullscreen;

    // Accessor methods for game data (uncomment and extend as needed)

    // public int GetPlayerLevel() => gameData != null ? gameData.playerLevel : 1;
    // public void SetPlayerLevel(int level) { if (gameData != null) { gameData.playerLevel = level; SaveGame(); } }

    // public float GetPlayerHealth() => gameData != null ? gameData.playerHealth : 100f;
    // public void SetPlayerHealth(float health) { if (gameData != null) { gameData.playerHealth = health; SaveGame(); } }

    // public int GetCurrency() => gameData != null ? gameData.currency : 0;
    // public void SetCurrency(int amount) { if (gameData != null) { gameData.currency = amount; SaveGame(); } }

    // public List<string> GetInventory() => gameData != null ? gameData.inventoryItems : new List<string>();
    // public void AddToInventory(string item) { if (gameData != null) { gameData.inventoryItems.Add(item); SaveGame(); } }
}