using UnityEngine;
using UnityEngine.UI;

public class ChassisStatsUIController : MonoBehaviour
{
    [Header("UI Bars")]
    public Image armorBar;
    public Image speedBar;
    public Image weightBar;

    [Header("Chassis Stat Max Values")]
    public float maxArmor = 150f;
    public float maxSpeed = 5f;
    public float maxWeight = 100f;

    public void UpdateChassisStatsUI(TankChassis selectedChassis)
    {
        armorBar.fillAmount = Mathf.Clamp01(selectedChassis.armor / maxArmor);
        speedBar.fillAmount = Mathf.Clamp01(selectedChassis.speed / maxSpeed);
        weightBar.fillAmount = Mathf.Clamp01(selectedChassis.weight / maxWeight);
    }
}
