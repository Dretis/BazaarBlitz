using UnityEngine;
using System.Collections.Generic; // For List

// Allows creating this object via the Unity Editor menu
[CreateAssetMenu(fileName = "GameSettings", menuName = "Scriptable Objects/Game Settings")]

public class GameSettings : ScriptableObject
{
    // --- Public Settings Variables ---
    [Header("Video Settings")]
    public int resolutionIndex;  // Index for the chosen screen resolution
    public bool isFullscreen;    // Flag for fullscreen mode

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float masterVolume;   // Global volume level (0.0 to 1.0)
    [Range(0f, 1f)]
    public float musicVolume;      // Sound effects volume level (0.0 to 1.0)
    [Range(0f, 1f)]
    public float effectsVolume;      // Sound effects volume level (0.0 to 1.0)
    //public string playerName;    // Player's name

    // --- Predefined Resolution Options (for dropdown) ---
    // A static list of available screen resolutions for display
    public static readonly List<ResolutionSetting> AvailableResolutions = new List<ResolutionSetting>
    {
        new ResolutionSetting(800, 600),
        new ResolutionSetting(1280, 720),   //720p
        new ResolutionSetting(1920, 1080),  //1080p
        new ResolutionSetting(2560, 1440),  //1440p
        new ResolutionSetting(3440, 1440),  //1440p Widescreen
        new ResolutionSetting(3840, 2160)   //4k
        // Add more common resolutions as needed
    };

    // --- Default Values ---
    // Sets all game settings to their default values
    public void SetDefaultValues()
    {
        resolutionIndex = 2; // Default to 1920x1080 (index 2 in AvailableResolutions)
        isFullscreen = true;
        masterVolume = 0.75f; // 75% volume
        musicVolume = 0.75f;
        effectsVolume = 0.75f;    
        //playerName = "Player1";
    }

    // --- Validation Method ---
    // Validates the current values of the game settings variables
    public bool ValidateSettings()
    {
        // 1. Validate resolutionIndex
        if (resolutionIndex < 0 || resolutionIndex >= AvailableResolutions.Count)
        {
            Debug.LogWarning($"Validation Error: Resolution index {resolutionIndex} is out of bounds. Max index is {AvailableResolutions.Count - 1}.");
            return false;
        }

        // 2. isFullscreen is a bool, so it's inherently valid, no need to check

        // 3. Validate masterVolume and sfxVolume (should be between 0.0 and 1.0)
        if (masterVolume < 0f || masterVolume > 1f)
        {
            Debug.LogWarning($"Validation Error: Master volume {masterVolume} is out of range (0.0-1.0).");
            return false;
        }
        if (musicVolume < 0f || musicVolume > 1f)
        {
            Debug.LogWarning($"Validation Error: Music volume {musicVolume} is out of range (0.0-1.0).");
            return false;
        }
        if (effectsVolume < 0f || effectsVolume > 1f)
        {
            Debug.LogWarning($"Validation Error: SFX volume {effectsVolume} is out of range (0.0-1.0).");
            return false;
        }

        // 4. Validate playerName (e.g., not null or empty)
        /*
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("Validation Error: Player name cannot be empty or null.");
            return false;
        }
        */
        // Optional: Add length checks, character restrictions, etc.
        // if (playerName.Length > 20) { Debug.LogWarning(...); return false; }

        Debug.Log("Game settings validated successfully.");
        return true;
    }

    // --- Apply Settings to Actual Game (Example) ---
    // Applies the current settings to the game's environment
    public void ApplySettings()
    {
        // Apply chosen resolution and fullscreen mode
        if (resolutionIndex >= 0 && resolutionIndex < AvailableResolutions.Count)
        {
            ResolutionSetting res = AvailableResolutions[resolutionIndex];
            // https://docs.unity3d.com/6000.1/Documentation/ScriptReference/FullScreenMode.html
            // Determine fullscreen mode (exclusive or windowed)
            FullScreenMode fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            // https://docs.unity3d.com/6000.1/Documentation/ScriptReference/RefreshRate.html
            // Create Unity's RefreshRate object
            //RefreshRate refreshRate = new RefreshRate { numerator = (uint)res.refreshRate, denominator = 1 };
            // Apply the resolution
            Screen.SetResolution(res.width, res.height, fullScreenMode);
            Debug.Log($"Applied Resolution: {res.width}x{res.height}, Fullscreen: {isFullscreen}");
        }
        else
        {
            Debug.LogError($"Cannot apply resolution: Invalid resolution index {resolutionIndex}.");
        }

        // Apply Volume
        //AudioListener.volume = masterVolume; // Simplified for example, might use audio mixers
        //Debug.Log($"Applied Master Volume: {masterVolume}");
        // For SFX, you'd likely control a specific AudioMixer group.
        // E.g., mixer.SetFloat("SFXVolume", Mathf.Log10(sfxVolume) * 20); // Conversion for mixer dB
        AudioManager.instance.masterVolume = masterVolume;
        AudioManager.instance.musicVolume = musicVolume;
        AudioManager.instance.effectsVolume = effectsVolume;
    }
}