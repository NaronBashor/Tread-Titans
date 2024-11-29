using System.Collections.Generic;
using UnityEngine;

public class TrackManager : MonoBehaviour
{
    public List<TankTrack> tracks = new List<TankTrack>();
    private int selectedIndex = 0;

    // Initialize guns to lock all but the first item
    public void InitializeTracks()
    {
        for (int i = 0; i < tracks.Count; i++) {
            tracks[i].isLocked = i != 0; // Lock all except the first item
        }
    }

    public TankTrack GetSelectedTrack()
    {
        return tracks[selectedIndex];
    }

    public void SetSelectedTrack(int index)
    {
        if (index >= 0 && index < tracks.Count && !tracks[index].isLocked) {
            selectedIndex = index;
        }
    }
}

[System.Serializable]
public class TankTrack
{
    public string trackName;
    public int trackTypeId;
    public Sprite trackSprite;          // Sprite to represent the track
    public string grip;                  // Grip level of the track
    public string durability;            // Durability level of the track
    public float speedAdjustment;       // Speed adjustment multiplier
    public string text;                 // Description text
    public bool isLocked = true;        // Lock status for shop

    // Updated constructor to include all fields
    public TankTrack(string name, Sprite sprite, string grip, string durability, float speedAdjustment, string text, bool isLocked = true)
    {
        trackName = name;
        trackSprite = sprite;
        this.grip = grip;
        this.durability = durability;
        this.speedAdjustment = speedAdjustment;
        this.text = text;
        this.isLocked = isLocked;
    }

    // Method to return a formatted string of the chassis stats
    public string GetStats()
    {
        return $"Grip: {grip}\nDurability: {durability}\nSpeed: {speedAdjustment}";
    }
}
