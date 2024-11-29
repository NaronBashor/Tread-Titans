using UnityEngine;
using UnityEngine.UI;

public class ChassisSelectionController : MonoBehaviour
{
    [Header("Chassis Options")]
    [SerializeField] private ChassisManager chassisManager; // Reference to the ChassisManager
    [SerializeField] private Image displayImage; // Image component for chassis display
    [SerializeField] private Button cycleAndAssignButton; // Button to cycle and assign chassis
    public ChassisStatsUIController chassisStatsUIController; // UI controller for chassis stats

    public TankAssemblyManager tankAssemblyManager;

    private int currentIndex = 0; // Tracks the current chassis selection index
    private PlayerData playerData;

    private void Awake()
    {
        playerData = FindAnyObjectByType<PlayerData>();
    }

    private void Start()
    {
        if (chassisManager == null) {
            Debug.LogError("ChassisManager not found.");
            return;
        }

        // Load saved chassis index and initialize the display
        currentIndex = playerData.currentChassisIndex;
        UpdateDisplay();

        // Set the current chassis in the tank assembly manager
        TankChassis selectedChassis = chassisManager.chassisOptions[currentIndex];
        tankAssemblyManager.SetChassis(currentIndex, selectedChassis.chassisSprite);

        // Assign button listener to cycle through unlocked chassis
        cycleAndAssignButton.onClick.AddListener(CycleToNextUnlockedChassis);
    }

    private void CycleToNextUnlockedChassis()
    {
        int originalIndex = currentIndex;

        // Cycle through chassis options, skipping locked ones
        do {
            currentIndex = (currentIndex + 1) % chassisManager.chassisOptions.Count;
        }
        while (chassisManager.chassisOptions[currentIndex].isLocked && currentIndex != originalIndex);

        // If all chassis options are locked, revert to the original index and exit
        if (chassisManager.chassisOptions[currentIndex].isLocked) {
            Debug.Log("No unlocked chassis available.");
            return;
        }

        // Save the new index and update the display
        playerData.currentChassisIndex = currentIndex;
        playerData.SaveProfile();
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        // Update UI display for the selected chassis
        TankChassis selectedChassis = chassisManager.chassisOptions[currentIndex];
        displayImage.sprite = selectedChassis.chassisSprite;
        chassisStatsUIController.UpdateChassisStatsUI(selectedChassis);
        tankAssemblyManager.SetChassis(currentIndex, selectedChassis.chassisSprite);
    }
}
