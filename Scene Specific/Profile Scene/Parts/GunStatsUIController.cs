using UnityEngine;
using UnityEngine.UI;

public class GunStatsUIController : MonoBehaviour
{
    [Header("UI Bars")]
    public Image damageBar;       // Reference to the damage UI bar
    public Image fireRateBar;     // Reference to the fire rate UI bar
    public Image accuracyBar;     // Reference to the accuracy UI bar

    [Header("Gun Stat Max Values")]
    public float maxDamage = 150f;    // Set this based on the highest possible damage
    public float maxFireRate = 4f;    // Set this based on the highest possible fire rate
    public float maxAccuracy = 100f;  // Accuracy is typically out of 100%

    // Method to update the bars based on the selected gun's stats
    public void UpdateGunStatsUI(TankGun selectedGun)
    {
        // Update damage bar fill based on the gun's damage relative to max damage
        damageBar.fillAmount = Mathf.Clamp01(selectedGun.damage / maxDamage);

        // Update fire rate bar fill based on the gun's fire rate relative to max fire rate
        fireRateBar.fillAmount = Mathf.Clamp01(selectedGun.fireRate / maxFireRate);

        // Update accuracy bar fill based on the gun's accuracy relative to 100%
        accuracyBar.fillAmount = Mathf.Clamp01(selectedGun.accuracy / maxAccuracy);
    }
}
