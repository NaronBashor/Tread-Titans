using UnityEngine;
using UnityEngine.UI;

public class GunSelectionController : MonoBehaviour
{
    [Header("Gun Options")]
    [SerializeField] private GunManager gunManager; // Reference to the GunManager for gun options
    [SerializeField] private Image displayImage; // Image component for gun display
    [SerializeField] private Button cycleAndAssignButton; // Button to cycle and assign gun
    public GunStatsUIController gunStatsUIController; // Reference to update gun stats UI

    [Header("Gun Display (UI or SpriteRenderer)")]
    public Image mainGunImage;  // UI Image for the gun display on the tank

    public TankAssemblyManager tankAssemblyManager;

    private int currentIndex = 0; // Tracks the current gun selection index
    private PlayerData playerData;

    private void Awake()
    {
        playerData = FindAnyObjectByType<PlayerData>();

        // Check if PlayerData is found
        if (playerData == null) {
            Debug.LogError("PlayerData not found in the scene.");
            return;
        }
    }

    private void Start()
    {
        if (gunManager == null) {
            Debug.LogError("GunManager not found.");
            return;
        }

        // Load saved gun index or default to 0
        currentIndex = playerData.currentGunIndex;
        UpdateDisplay();

        // Assign button listener to cycle through unlocked guns
        cycleAndAssignButton.onClick.AddListener(CycleToNextUnlockedGun);
    }

    private void CycleToNextUnlockedGun()
    {
        int originalIndex = currentIndex;

        do {
            currentIndex = (currentIndex + 1) % gunManager.guns.Count;
        }
        while (gunManager.guns[currentIndex].isLocked && currentIndex != originalIndex);

        // Check if we ended up back where we started with all items locked
        if (gunManager.guns[currentIndex].isLocked) {
            Debug.Log("No unlocked guns available.");
            return;
        }

        // Update the gun selection and save it
        playerData.currentGunIndex = currentIndex;
        playerData.SaveProfile();
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        // Retrieve the currently selected gun
        TankGun selectedGun = gunManager.guns[currentIndex];

        // Ensure the selected gun has a sprite
        if (selectedGun.gunSprite != null) {
            displayImage.sprite = selectedGun.gunSprite;
        } else {
            Debug.LogWarning("Selected gun does not have an assigned sprite.");
        }

        // Update the tank and UI with the selected gun
        tankAssemblyManager.SetGunSprite(selectedGun.gunSprite);
        gunStatsUIController.UpdateGunStatsUI(selectedGun);
    }
}
