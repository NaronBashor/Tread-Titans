using System.Collections.Generic;
using UnityEngine;

public class ChassisManager : MonoBehaviour
{
    public List<TankChassis> chassisOptions = new List<TankChassis>();
    private int selectedIndex = 0;

    // Initialize chassis to lock all but the first item
    public void InitializeChassis()
    {
        for (int i = 0; i < chassisOptions.Count; i++) {
            chassisOptions[i].isLocked = i != 0; // Lock all except the first item
        }
    }

    public TankChassis GetSelectedChassis()
    {
        return chassisOptions[selectedIndex];
    }

    public void SetSelectedChassis(int index)
    {
        if (index >= 0 && index < chassisOptions.Count && !chassisOptions[index].isLocked) {
            selectedIndex = index;
        }
    }
}

[System.Serializable]
public class TankChassis
{
    public string chassisName;          // Name of the chassis
    public Sprite chassisSprite;        // Sprite to represent the chassis
    public float armor;                 // Durability of the chassis
    public float speed;                 // Maximum speed of the tank
    public float weight;                // Weight affecting handling and acceleration
    public string text;                 // Description text
    public bool isLocked = true;        // Lock status for shop

    // Updated constructor to include all fields
    public TankChassis(string name, Sprite sprite, float armor, float speed, float weight, string text, bool isLocked = true)
    {
        chassisName = name;
        chassisSprite = sprite;
        this.armor = armor;
        this.speed = speed;
        this.weight = weight;
        this.text = text;
        this.isLocked = isLocked;
    }

    // Method to return a formatted string of the chassis stats
    public string GetStats()
    {
        return $"Armor: {armor}\nSpeed: {speed}\nWeight: {weight}";
    }
}

