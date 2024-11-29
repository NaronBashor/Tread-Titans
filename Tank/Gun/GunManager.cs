using System.Collections.Generic;
using UnityEngine;

public class GunManager : MonoBehaviour
{
    public List<TankGun> guns = new List<TankGun>();
    private int selectedIndex = 0;

    // Initialize guns to lock all but the first item
    public void InitializeGuns()
    {
        for (int i = 0; i < guns.Count; i++) {
            guns[i].isLocked = i != 0; // Lock all except the first item
        }
    }

    public TankGun GetSelectedGun()
    {
        return guns[selectedIndex];
    }

    public void SetSelectedgun(int index)
    {
        if (index >= 0 && index < guns.Count && !guns[index].isLocked) {
            selectedIndex = index;
        }
    }
}

[System.Serializable]
public class TankGun
{
    public string gunName;              // Name of the gun
    public Sprite gunSprite;            // Sprite to represent the gun
    public float damage;                // Damage per shot
    public float fireRate;              // Shots per second
    public float accuracy;              // Accuracy percentage (0-100%)
    public string text;                 // Description text
    public bool isLocked = true;        // Lock status for shop

    // Updated constructor to include all fields
    public TankGun(string name, Sprite sprite, float dmg, float rate, float acc, string desc, bool isLocked = true)
    {
        gunName = name;
        gunSprite = sprite;
        this.damage = dmg;
        this.fireRate = rate;
        this.accuracy = acc;
        this.text = desc;
        this.isLocked = isLocked;
    }

    // Method to return a formatted string of the chassis stats
    public string GetStats()
    {
        return $"Damage: {damage}\nFire Rate: {fireRate}\nAccuracy: {accuracy}";
    }
}
