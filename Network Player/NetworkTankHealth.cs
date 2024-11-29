using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkTankHealth : NetworkBehaviour
{
    public enum SpawnSide
    {
        left,
        right
    }

    public NetworkVariable<SpawnSide> spawnSide = new NetworkVariable<SpawnSide>(SpawnSide.left,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public NetworkVariable<float> currentHealth = new NetworkVariable<float>(
        100f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public float regenerationRate = 5f;

    [Header("Armor Settings")]
    private const float MAX_ALLOWED_ARMOR = 150f;
    public NetworkVariable<float> currentArmor = new NetworkVariable<float>(
        100f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    [Range(0, 1)] public float armorEffectiveness = 0.5f;

    [Header("UI Settings")]
    [SerializeField] private GameObject healthBarPrefab;

    public delegate void HealthChanged(float health);

    private Vector3 spawnPoint;
    private GameManager gameManager;

    public override void OnNetworkSpawn()
    {
        if (IsServer) InitializeTankHealth();

        gameManager = FindAnyObjectByType<GameManager>();

        // Setup health and armor value change listeners
        currentHealth.OnValueChanged += (oldValue, newValue) => UpdateHealthUI();
        currentArmor.OnValueChanged += (oldValue, newValue) => UpdateHealthUI();

        // Initialize health bar for the owning player only
        if (IsOwner) {
            InstantiateHealthBar();
            UpdateHealthUI();
            spawnPoint = this.transform.position;
        }
    }

    private void InitializeTankHealth()
    {
        currentHealth.Value = maxHealth;
        currentArmor.Value = MAX_ALLOWED_ARMOR;
    }

    private void InstantiateHealthBar()
    {
        Canvas mainCanvas = FindAnyObjectByType<Canvas>();
        if (mainCanvas == null) {
            //Debug.LogError("Main Canvas not found in the scene.");
            return;
        }

        //healthBarInstance = Instantiate(healthBarPrefab, mainCanvas.transform);
        healthBarPrefab.SetActive(true);
    }

    private void UpdateHealthUI()
    {
        if (healthBarPrefab != null) {
            healthBarPrefab.GetComponent<HealthBarUI>().UpdateHealthBar(currentHealth.Value, maxHealth);
            healthBarPrefab.GetComponent<HealthBarUI>().UpdateArmorBar(currentArmor.Value, MAX_ALLOWED_ARMOR);
        }
    }

    public void RepairArmor(float amount)
    {
        currentArmor.Value = Mathf.Clamp(currentArmor.Value + amount, 0, MAX_ALLOWED_ARMOR);
        //Debug.Log($"[RepairArmor] Armor repaired by {amount}. Current armor: {currentArmor.Value}");
        UpdateHealthUIClientRpc(currentHealth.Value, currentArmor.Value);
    }

    private float ApplyArmor(float damage)
    {
        if (currentArmor.Value > 0) {
            float armorAbsorption = damage * armorEffectiveness;
            float damageToHealth = damage - armorAbsorption;
            currentArmor.Value = Mathf.Clamp(currentArmor.Value - armorAbsorption, 0, MAX_ALLOWED_ARMOR);
            //Debug.Log($"[ApplyArmor] Armor absorbed: {armorAbsorption}, Damage to Health: {damageToHealth}, Remaining Armor: {currentArmor.Value}");
            return damageToHealth;
        }

        return damage;
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage, NetworkTankHealth.SpawnSide attackerSide)
    {
        if (attackerSide == spawnSide.Value) // Use the synchronized spawnSide value
        {
            //Debug.Log("Friendly fire prevented: Attacker and target are on the same side.");
            return; // Prevent damage if they are on the same team
        }

        // Apply armor reduction and calculate damage to health
        float damageToHealth = ApplyArmor(damage);

        currentHealth.Value = Mathf.Clamp(currentHealth.Value - damageToHealth, 0, maxHealth);
        //Debug.Log($"[Server] Player took damage: {damageToHealth}. Current health: {currentHealth.Value}");

        // Update all clients with new health and armor values
        UpdateHealthUIClientRpc(currentHealth.Value, currentArmor.Value);

        if (currentHealth.Value <= 0) {
            HandleDeath();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageFromAnythingServerRpc(float damage)
    {
        // Apply armor reduction and calculate damage to health
        float damageToHealth = ApplyArmor(damage);

        currentHealth.Value = Mathf.Clamp(currentHealth.Value - damageToHealth, 0, maxHealth);
        //Debug.Log($"[Server] Player took damage: {damageToHealth}. Current health: {currentHealth.Value}");

        // Update all clients with new health and armor values
        UpdateHealthUIClientRpc(currentHealth.Value, currentArmor.Value);

        if (currentHealth.Value <= 0) {
            HandleDeath();
        }
    }

    public void RespawnTank()
    {
        //Debug.Log("Respawning...");
        Vector3 spawnPoint = ChooseSpawnPoint();
        transform.position = spawnPoint;
        currentHealth.Value = maxHealth;
        currentArmor.Value = MAX_ALLOWED_ARMOR;
        UpdateHealthUIClientRpc(currentHealth.Value, currentArmor.Value);
        ShowTankClientRpc(); // Make tank visible again on all clients
    }

    private Vector3 ChooseSpawnPoint()
    {
        return spawnPoint;
    }

    [ClientRpc]
    private void UpdateHealthUIClientRpc(float newHealth, float newArmor)
    {
        if (healthBarPrefab != null) {
            healthBarPrefab.GetComponent<HealthBarUI>().UpdateHealthBar(newHealth, maxHealth);
            healthBarPrefab.GetComponent<HealthBarUI>().UpdateArmorBar(newArmor, MAX_ALLOWED_ARMOR);
        }
    }

    private void HandleDeath()
    {
        gameManager.StartRespawnCoroutine(this); // Initiate respawn
        DestroyTankClientRpc();
    }

    [ClientRpc]
    private void ShowTankClientRpc()
    {
        //Debug.Log("Respawn Complete.");
        gameObject.SetActive(true); // Make tank visible upon respawn
    }

    [ClientRpc]
    public void DestroyTankClientRpc()
    {
        //Debug.Log("Tank has been destroyed!");
        gameObject.SetActive(false); // Consider using Destroy(gameObject) if complete removal is needed
    }
}
