using UnityEngine;
using UnityEngine.UI;

public class TrackSelectionController : MonoBehaviour
{
    [Header("Track Options")]
    [SerializeField] private TrackManager trackManager;
    [SerializeField] private Image leftTrackDisplay; // UI image for left track
    [SerializeField] private Image rightTrackDisplay; // UI image for right track
    [SerializeField] private Button cycleAndAssignButton; // Button to cycle and assign tracks
    public TrackStatsUIController controller;

    public TankAssemblyManager tankAssemblyManager;

    private int currentIndex = 0; // Tracks the current track selection index
    private PlayerData playerData;

    private void Awake()
    {
        playerData = FindAnyObjectByType<PlayerData>();
    }

    private void Start()
    {
        if (trackManager == null || trackManager.tracks.Count == 0) {
            Debug.LogError("Track options not set.");
            return;
        }

        // Load saved track index and initialize the display
        currentIndex = playerData.currentTracksIndex;
        UpdateDisplay();

        // Set the current track in the tank assembly manager
        TankTrack selectedTrack = trackManager.tracks[currentIndex];
        tankAssemblyManager.SetTrackSprites(selectedTrack.trackSprite, selectedTrack.trackSprite);

        // Assign button listener to cycle through unlocked tracks
        cycleAndAssignButton.onClick.AddListener(CycleToNextUnlockedTrack);
    }

    private void CycleToNextUnlockedTrack()
    {
        int originalIndex = currentIndex;

        // Cycle through tracks, skipping locked ones
        do {
            currentIndex = (currentIndex + 1) % trackManager.tracks.Count;
        }
        while (trackManager.tracks[currentIndex].isLocked && currentIndex != originalIndex);

        // If all tracks are locked, revert to the original index and exit
        if (trackManager.tracks[currentIndex].isLocked) {
            Debug.Log("No unlocked tracks available.");
            return;
        }

        // Save the new index and update the display
        playerData.currentTracksIndex = currentIndex;
        playerData.SaveProfile();
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        // Update UI display for the selected track
        TankTrack selectedTrack = trackManager.tracks[currentIndex];
        leftTrackDisplay.sprite = selectedTrack.trackSprite;
        rightTrackDisplay.sprite = selectedTrack.trackSprite;
        tankAssemblyManager.SetTrackSprites(selectedTrack.trackSprite, selectedTrack.trackSprite);
        controller.UpdateTrackStatsUI(selectedTrack);
    }
}
