using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    // Settings
    public bool soundOn;
    public float soundVolume;
    public bool fullscreen;

    // Game-related data
    //public int playerLevel;
    //public float playerHealth;
    //public int currency;
    //public List<string> inventoryItems;
    //public Dictionary<string, bool> achievements;

    public GameData()
    {
        // Default settings
        soundOn = true;
        soundVolume = 0.5f;
        fullscreen = true;

        // Default game data
        //playerLevel = 1;
        //playerHealth = 100f;
        //currency = 0;
        //inventoryItems = new List<string>();
        //achievements = new Dictionary<string, bool>();
    }
}