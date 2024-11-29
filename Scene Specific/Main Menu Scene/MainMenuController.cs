using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject settingsPanel;
    public GameObject confirmPanel;

    // Main menu buttons
    //public Button playButton;
    //public Button quitButton;
    //public Button websiteButton;
    //public Button optionsButton;
    //public Button confirmButton;
    //public Button cancelButton;
    //public Button quitConfirmButton;
    //public Button resetAccountButton;

    // Options menu elements (two toggles and a slider)
    public Toggle soundToggle;
    public Toggle fullscreenToggle;
    public Slider volumeSlider;

    private PlayerControls menuControls;
    private InputSystemUIInputModule inputModule;

    // Arrays to hold the UI elements for each menu
    //private Selectable[] mainMenuElements;
    //private Selectable[] optionsMenuElements;
    //private Selectable[] confirmMenuElements;
    //private Selectable[] activeElements;

    private bool isUsingMouse = false;

    private void Awake()
    {
        menuControls = new PlayerControls();
        inputModule = FindAnyObjectByType<InputSystemUIInputModule>();

        //// Initialize the elements arrays
        //mainMenuElements = new Selectable[] { playButton, quitButton, websiteButton, optionsButton };
        //optionsMenuElements = new Selectable[] { soundToggle, volumeSlider, fullscreenToggle, resetAccountButton };
        //confirmMenuElements = new Selectable[] { cancelButton, quitConfirmButton, confirmButton };

        // Set active elements to main menu by default
        //activeElements = mainMenuElements;
    }

    private void OnEnable()
    {
        menuControls.MainMenu.Navigate.performed += OnNavigate;
        menuControls.MainMenu.Submit.performed += OnSubmit;
        menuControls.MainMenu.Adjust.performed += OnAdjust;
        menuControls.MainMenu.Back.performed += ControllerBackOut;
        menuControls.Enable();
    }

    private void OnDisable()
    {
        menuControls.MainMenu.Navigate.performed -= OnNavigate;
        menuControls.MainMenu.Submit.performed -= OnSubmit;
        menuControls.MainMenu.Adjust.performed -= OnAdjust;
        menuControls.MainMenu.Back.performed -= ControllerBackOut;
        menuControls.Disable();
    }

    private void Start()
    {
        LoadSettingsToUI();

        AudioManager.Instance.PlayMusic("Menu Music");

        // Add listeners to save settings when they change
        soundToggle.onValueChanged.AddListener(isOn => SaveManager.Instance.SetSoundOn(isOn));
        fullscreenToggle.onValueChanged.AddListener(isFullscreen => SaveManager.Instance.SetFullscreen(isFullscreen));
        volumeSlider.onValueChanged.AddListener(volume => SaveManager.Instance.SetSoundVolume(volume));
    }

    private void Update()
    {
        // Check for mouse movement to determine if the user is using the mouse
        if (Mouse.current != null && Mouse.current.delta.ReadValue() != Vector2.zero) {
            if (!isUsingMouse) {
                isUsingMouse = true;
                EnableInputModule(true); // Enable mouse input
            }
        } else if ((Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame) ||
                   (Keyboard.current != null && Keyboard.current.wasUpdatedThisFrame)) {
            if (isUsingMouse) {
                isUsingMouse = false;
                EnableInputModule(false); // Disable mouse input
                UpdateElementSelection(); // Reset focus to the active menu selection
            }
        }
    }

    private void EnableInputModule(bool enable)
    {
        if (inputModule != null) {
            inputModule.enabled = enable;
        }
    }

    private void OnNavigate(InputAction.CallbackContext context)
    {
        if (isUsingMouse) return;

        //Vector2 navigationInput = context.ReadValue<Vector2>();

        //if (navigationInput.y > 0) {
        //    selectedIndex = (selectedIndex - 1 + activeElements.Length) % activeElements.Length;
        //} else if (navigationInput.y < 0) {
        //    selectedIndex = (selectedIndex + 1) % activeElements.Length;
        //}

        UpdateElementSelection();
    }

    private void UpdateElementSelection()
    {
        // Highlight the selected element in the currently active menu
        //for (int i = 0; i < activeElements.Length; i++) {
        //    if (activeElements[i] is Toggle toggle) {
        //        toggle.image.color = i == selectedIndex ? Color.yellow : Color.white;
        //    } else if (activeElements[i] is Slider slider) {
        //        slider.targetGraphic.color = i == selectedIndex ? Color.yellow : Color.white;
        //    } else if (activeElements[i] is Button button) {
        //        button.image.color = i == selectedIndex ? Color.yellow : Color.white;
        //    }
        //}

        //// Set selected GameObject in EventSystem for UI navigation
        //EventSystem.current.SetSelectedGameObject(activeElements[selectedIndex].gameObject);
    }

    public void ResetAccount()
    {
        if (confirmPanel != null && confirmPanel.activeSelf) {
            SaveManager.Instance.DeleteGame();
        }
    }

    public void OpenConfirmPanel()
    {
        confirmPanel.SetActive(true);
        //selectedIndex = 0;
        //activeElements = confirmMenuElements; // Switch to confirm menu elements
        UpdateElementSelection();
    }

    public void OnCloseConfirmPanelViaButton()
    {
        confirmPanel.SetActive(false);
        //activeElements = mainMenuElements; // Return to main menu elements
        //selectedIndex = 0;
        UpdateElementSelection();
    }

    private void OnSubmit(InputAction.CallbackContext context)
    {
        // Ensure the action only triggers on the performed phase to avoid double execution
        if (isUsingMouse || context.phase != InputActionPhase.Performed) return;

        // Determine which set of elements to use based on the active menu
        //if (confirmPanel.activeSelf) {
        //    activeElements = confirmMenuElements;
        //} else if (isOptionsMenuActive) {
        //    activeElements = optionsMenuElements;
        //} else {
        //    activeElements = mainMenuElements;
        //}

        //// Get the current selected element from the active menu
        //Selectable currentElement = activeElements[selectedIndex];

        //if (currentElement is Toggle toggle) {
        //    // Toggle the current toggle and save settings if in options menu
        //    toggle.isOn = !toggle.isOn;
        //    SaveSettingsFromUI(); // Only call this if you're in options menu
        //} else if (currentElement is Button button) {
        //    // Invoke the button's onClick action
        //    AudioManager.Instance.PlaySFX("Button Click");
        //    button.onClick.Invoke();
        //}
    }


    private void OnAdjust(InputAction.CallbackContext context)
    {
        //if (isUsingMouse) return; // Skip adjustment if using the mouse

        // Adjust the slider value if the selected element is a slider
        //Selectable[] activeElements = isOptionsMenuActive ? optionsMenuElements : mainMenuElements;
        //Selectable currentElement = activeElements[selectedIndex];

        //if (currentElement is Slider slider) {
        //    float adjustment = context.ReadValue<Vector2>().x; // Get horizontal input
        //    slider.value += adjustment * 0.1f; // Adjust slider value
        //    SaveSettingsFromUI(); // Save settings whenever the slider is adjusted
        //}
    }

    public void ControllerBackOut(InputAction.CallbackContext context)
    {
        if (context.performed) {
            CloseOptionsMenu();
            if (confirmPanel != null && confirmPanel.activeSelf) {
                confirmPanel.SetActive(false);
            }
        }
    }

    public void OnVolumeSliderChanged(float volume)
    {
        // Save volume in SaveManager
        SaveManager.Instance.SetSoundVolume(volume);

        // Update AudioManager's volumes
        AudioManager.Instance.SetSoundVolume(volume);
    }

    public void OpenOptionsMenu()
    {
        AudioManager.Instance.PlaySFX("Button Click");
        settingsPanel.SetActive(true);
        //isOptionsMenuActive = true;
        //selectedIndex = 0;
        //activeElements = optionsMenuElements; // Switch to options elements
        LoadSettingsToUI();
        UpdateElementSelection();
    }

    public void CloseOptionsMenu()
    {
        AudioManager.Instance.PlaySFX("Button Click");
        settingsPanel.SetActive(false);
        //isOptionsMenuActive = false;
        //selectedIndex = 0;
        //activeElements = mainMenuElements; // Switch back to main menu elements
        SaveSettingsFromUI(); // Save settings before closing
        UpdateElementSelection();
    }


    // MainMenuController.cs
    private void LoadSettingsToUI()
    {
        if (SaveManager.Instance != null) {
            // Load saved settings from SaveManager into the UI elements
            soundToggle.isOn = SaveManager.Instance.IsSoundOn();
            fullscreenToggle.isOn = SaveManager.Instance.IsFullscreen();
            volumeSlider.value = SaveManager.Instance.GetSoundVolume();

            AudioManager.Instance.SetSoundVolume(volumeSlider.value);
            // Debug log to verify
            //Debug.Log("Settings Loaded: SoundOn=" + soundToggle.isOn + ", Fullscreen=" + fullscreenToggle.isOn + ", Volume=" + volumeSlider.value);
        }
    }

    private void SaveSettingsFromUI()
    {
        if (SaveManager.Instance != null) {
            SaveManager.Instance.SetSoundOn(soundToggle.isOn);
            SaveManager.Instance.SetFullscreen(fullscreenToggle.isOn);
            SaveManager.Instance.SetSoundVolume(volumeSlider.value);
        }
    }

    // Called by PlayerData's TargetRpc after loading or creating data on the server
    public void LoadProfileScreenOnClient()
    {
        SceneManager.LoadScene("Profile");
    }

    public void OpenProfileScreen()
    {
        AudioManager.Instance.PlaySFX("Button Click");
        SceneManager.LoadScene("Profile");
    }

    public void OpenWebsite()
    {
        Application.OpenURL("https://www.splitrockgames.com");
    }

    public void QuitGame()
    {
        Debug.Log("Quit game!");
        Application.Quit();
    }

    public void BackToMainMenu()
    {
        AudioManager.Instance.PlaySFX("Button Click");
        CloseOptionsMenu();
    }
}