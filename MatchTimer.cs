using TMPro;
using UnityEngine;

public class MatchTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    public float startTimeInMinutes = 6f; // Start time in minutes
    private float timeRemaining; // Time remaining in seconds
    private bool isTimerRunning = false;

    [Header("UI Elements")]
    public TextMeshProUGUI timerText; // Reference to the UI Text to display the timer

    [ContextMenu("Start Timer")]
    public void StartTimer()
    {
        // Convert start time to seconds and start the timer
        timeRemaining = startTimeInMinutes * 60;
        isTimerRunning = true;
        UpdateTimerDisplay(); // Update the initial display
    }

    private void Update()
    {
        if (isTimerRunning) {
            if (timeRemaining > 0) {
                timeRemaining -= Time.deltaTime;
                UpdateTimerDisplay();
            } else {
                timeRemaining = 0;
                isTimerRunning = false;
                TimerEnded();
            }
        }
    }

    private void UpdateTimerDisplay()
    {
        // Calculate minutes and seconds
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);

        // Format and display the timer
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    [ContextMenu("Reset Timer")]
    public void ResetTimer()
    {
        StartTimer();
    }

    private void TimerEnded()
    {
        // Perform actions when the timer ends
        Debug.Log("Timer has ended!");
        // You can trigger additional logic here (e.g., end the game, play a sound, etc.)
    }
}
