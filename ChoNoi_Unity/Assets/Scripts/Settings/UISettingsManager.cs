using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; // For List
using System.Linq; // For LINQ operations, e.g. populating dropdown
using TMPro;

public class UISettingsManager : MonoBehaviour
{
    // --- Settings Manager Reference ---
    private SettingsManager settingsManager; // Manages loading/saving game settings
    private GameSettings currentSettings;    // The actual settings data

    // --- UI Element References ---
    [Header("UI Elements")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;  // UI dropdown for resolution selection
    [SerializeField] private Toggle fullscreenToggle;      // UI toggle for fullscreen mode
    [Space]
    [SerializeField] private Slider masterVolumeSlider;    // UI slider for master volume
    [SerializeField] private Slider musicVolumeSlider;    // UI slider for music volume
    [SerializeField] private Slider effectsVolumeSlider;       // UI slider for sound effects volume

    //[SerializeField] private InputField playerNameInputField; // UI input field for player name
    //[SerializeField] private Button saveButton;            // UI button to save settings
    [Header("Listen on Event Channels")]
    public VoidEventChannelSO m_SettingsLoaded;
    public VoidEventChannelSO m_SettingsSaved;

    private void OnEnable()
    {
        m_SettingsLoaded.OnEventRaised += OnSettingsLoaded;
        m_SettingsSaved.OnEventRaised += OnSettingsSaved;
    }

    private void OnDisable()
    {
        m_SettingsLoaded.OnEventRaised -= OnSettingsLoaded;
        m_SettingsSaved.OnEventRaised -= OnSettingsSaved;
    }

    private void OnSettingsLoaded()
    {
        UpdateUIFromSettings();
    }

    private void OnSettingsSaved()
    {
        OnSettingsSavedFeedback();
    }

    // Called when the script instance is being loaded
    void Awake()
    {
        // Find and get the SettingsManager component in the scene
        settingsManager = FindObjectOfType<SettingsManager>();
        if (settingsManager == null)
        {
            Debug.LogError("UIManager: SettingsManager not found in scene!");
            return;
        }

        // Get the current GameSettings data from the manager
        currentSettings = settingsManager.GetCurrentSettings();
        if (currentSettings == null)
        {
            Debug.LogError("UIManager: GameSettings reference from SettingsManager is null!");
            return;
        }

        // --- Populate Resolution Dropdown ---
        // Convert available resolutions to display strings
        List<string> resolutionOptions = GameSettings.AvailableResolutions.Select(res => res.ToString()).ToList();
        resolutionDropdown.ClearOptions();       // Clear existing dropdown items
        resolutionDropdown.AddOptions(resolutionOptions); // Add new resolution options

        // --- Attach UI Event Listeners ---
        // Assign methods to be called when UI element values change
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        effectsVolumeSlider.onValueChanged.AddListener(OnEffectsVolumeChanged);
        //playerNameInputField.onEndEdit.AddListener(OnPlayerNameChanged); // Listens for input field completion
        //saveButton.onClick.AddListener(OnSaveButtonClicked);           // Listens for save button click

        // --- Subscribe to SettingsManager Events ---
        // Update UI when settings are loaded/defaulted
        //SettingsManager.OnSettingsLoaded += UpdateUIFromSettings;
        // Provide feedback when settings are saved
        //SettingsManager.OnSettingsSaved += OnSettingsSavedFeedback;
    }

    // Called once when the script starts
    void Start()
    {
        // Initialize UI elements with current settings
        UpdateUIFromSettings();
    }

    void OnDestroy()
    {
        // --- Unsubscribe from Events to prevent memory leaks ---
        if (resolutionDropdown != null) resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        if (effectsVolumeSlider != null) effectsVolumeSlider.onValueChanged.RemoveListener(OnEffectsVolumeChanged);
        //if (playerNameInputField != null) playerNameInputField.onEndEdit.RemoveListener(OnPlayerNameChanged);
        //if (saveButton != null) saveButton.onClick.RemoveListener(OnSaveButtonClicked); // Remove listener for save button

        //SettingsManager.OnSettingsLoaded -= UpdateUIFromSettings;
        //SettingsManager.OnSettingsSaved -= OnSettingsSavedFeedback;
    }

    // --- Update UI Elements from GameSettings data ---
    private void UpdateUIFromSettings()
    {
        // Temporarily remove listeners to prevent recursive calls during UI update
        RemoveAllListeners();

        // Set UI element values based on current settings
        resolutionDropdown.value = currentSettings.resolutionIndex;
        fullscreenToggle.isOn = currentSettings.isFullscreen;
        masterVolumeSlider.value = currentSettings.masterVolume * 100;
        musicVolumeSlider.value = currentSettings.musicVolume * 100;
        effectsVolumeSlider.value = currentSettings.effectsVolume * 100;
        //playerNameInputField.text = currentSettings.playerName;

        // Re-add listeners after updating UI
        AddAllListeners();

        Debug.Log("UI updated from current settings.");
    }

    // --- UI Event Handlers ---
    // Called when resolution dropdown value changes
    private void OnResolutionChanged(int newIndex)
    {
        settingsManager.SetResolutionIndex(newIndex);
        currentSettings.ApplySettings();
    }

    // Called when fullscreen toggle value changes
    private void OnFullscreenChanged(bool newValue)
    {
        settingsManager.SetFullscreen(newValue);
        currentSettings.ApplySettings();
    }

    // Called when master volume slider value changes
    private void OnMasterVolumeChanged(float newValue)
    {
        settingsManager.SetMasterVolume(newValue / 100);
    }

    private void OnMusicVolumeChanged(float newValue)
    {
        settingsManager.SetMusicVolume(newValue / 100);
    }

    // Called when SFX volume slider value changes
    private void OnEffectsVolumeChanged(float newValue)
    {
        settingsManager.SetEffectsVolume(newValue / 100);
    }

    // Called when the Save button is clicked
    private void OnSaveButtonClicked()
    {
        settingsManager.SaveSettings();          // Save current settings to file
        currentSettings.ApplySettings();         // Apply the saved settings to the game
    }

    // --- Optional Feedback after Saving ---
    // Provides visual/console feedback after settings are saved
    private void OnSettingsSavedFeedback()
    {
        Debug.Log("Settings saved successfully! (UI Feedback)");
    }

    // --- Helper Methods for Listener Management ---
    // Removes all listeners from UI elements
    private void RemoveAllListeners()
    {
        resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
        fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
        masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        effectsVolumeSlider.onValueChanged.RemoveListener(OnEffectsVolumeChanged);
        //playerNameInputField.onEndEdit.RemoveListener(OnPlayerNameChanged);
        // Note: Save button listener is not removed here as it's not programmatically updated
    }

    // Adds all listeners back to UI elements
    private void AddAllListeners()
    {
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        effectsVolumeSlider.onValueChanged.AddListener(OnEffectsVolumeChanged);
        //playerNameInputField.onEndEdit.AddListener(OnPlayerNameChanged);
        // Note: Save button listener is not re-added here
    }
}