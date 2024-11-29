using UnityEngine;

public class TankHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float regenerationRate = 5f;
    public bool isRegenerating = false;

    [Header("Armor Settings")]
    private const float MAX_ALLOWED_ARMOR = 150f; // Maximum allowed armor
    public float currentArmor;
    [Range(0, 1)] public float armorEffectiveness = 0.5f;

    public delegate void HealthChanged(float health);
    public event HealthChanged OnHealthChanged;

    private void Start()
    {
        // Cap maxArmor to ensure it doesn’t exceed the allowed maximum
        currentArmor = Mathf.Clamp(currentArmor, 0, MAX_ALLOWED_ARMOR);
        currentHealth = maxHealth;

        if (OnHealthChanged != null) OnHealthChanged.Invoke(currentHealth);
    }

    private void Update()
    {
        if (isRegenerating && currentHealth < maxHealth) {
            RegenerateHealth();
        }
    }

    public void SetArmor(float armor)
    {
        currentArmor = Mathf.Min(armor, MAX_ALLOWED_ARMOR);
    }

    public void TakeDamage(float damage)
    {
        float damageToHealth = ApplyArmor(damage);
        currentHealth -= damageToHealth;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (OnHealthChanged != null) OnHealthChanged.Invoke(currentHealth);

        if (currentHealth <= 0) {
            DestroyTank();
        }
    }

    private float ApplyArmor(float damage)
    {
        if (currentArmor > 0) {
            float armorAbsorption = damage * armorEffectiveness;
            float damageToHealth = damage - armorAbsorption;
            currentArmor -= armorAbsorption;
            currentArmor = Mathf.Clamp(currentArmor, 0, MAX_ALLOWED_ARMOR); // Ensure armor stays within limits
            return damageToHealth;
        }

        return damage;
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Clamp to max health
        if (OnHealthChanged != null) OnHealthChanged.Invoke(currentHealth);
    }

    private void RegenerateHealth()
    {
        Heal(regenerationRate * Time.deltaTime);
    }

    private void DestroyTank()
    {
        // Implement destruction effects, sounds, animations, or UI notifications here
        Debug.Log("Tank has been destroyed!");

        // Optionally, disable tank controls, play effects, or destroy the GameObject
        gameObject.SetActive(false); // Temporary destruction by disabling the object
        // Alternatively, use Destroy(gameObject);
    }

    public void RepairArmor(float amount)
    {
        currentArmor += amount;
        currentArmor = Mathf.Clamp(currentArmor, 0, MAX_ALLOWED_ARMOR); // Ensure armor stays within limits
    }
}
