using UnityEngine;
using UnityEngine.UI;

public class TrackStatsUIController : MonoBehaviour
{
    [Header("UI Bars")]
    public Image gripBar;          // Bar for displaying grip level
    public Image durabilityBar;     // Bar for displaying durability level
    public Image speedAdjustmentBar; // Bar for displaying speed adjustment

    [Header("Track Stat Max Values")]
    public float maxGrip = 100f;       // Maximum possible value for grip
    public float maxDurability = 100f; // Maximum possible value for durability
    public float maxSpeedAdjustment = 2f; // Maximum possible multiplier for speed adjustment (e.g., 2x)

    public void UpdateTrackStatsUI(TankTrack selectedTrack)
    {
        gripBar.fillAmount = Mathf.Clamp01(50 / 100);
        //gripBar.fillAmount = Mathf.Clamp01(selectedTrack.grip / maxGrip);
        durabilityBar.fillAmount = Mathf.Clamp01(50 / 100);
        //durabilityBar.fillAmount = Mathf.Clamp01(selectedTrack.durability / maxDurability);
        speedAdjustmentBar.fillAmount = Mathf.Clamp01(selectedTrack.speedAdjustment / maxSpeedAdjustment);
    }
}
