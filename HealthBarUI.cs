using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image armorBarFill;

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBarFill != null) {
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }
    }

    public void UpdateArmorBar(float currentArmor, float maxArmor)
    {
        if (armorBarFill != null) {
            armorBarFill.fillAmount = currentArmor / maxArmor;
        }
    }
}

