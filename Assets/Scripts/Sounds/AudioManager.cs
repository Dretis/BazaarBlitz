using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Volume")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;

    [Range(0f, 1f)]
    public float musicVolume = 1f;

    [Range(0f, 1f)]
    public float effectsVolume = 1f;

    private Bus masterBus;
    private Bus musicBus;
    private Bus effectsBus;

    [Header("Listen on Event Channels")]
    public VoidEventChannelSO m_SettingsLoaded;
    public VoidEventChannelSO m_SettingsSaved;

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    // Start is called before the first frame update
    void Awake()
    {
        if(instance != null)
        {
            Debug.Log("ERROR! More than 1 AudioManager found.");
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this);

        masterBus = RuntimeManager.GetBus("bus:/");
        musicBus = RuntimeManager.GetBus("bus:/Music");
        effectsBus = RuntimeManager.GetBus("bus:/Effects");
    }

    // Update is called once per frame
    void Update()
    {
        // temp
        masterBus.setVolume(masterVolume);
        musicBus.setVolume(musicVolume);
        effectsBus.setVolume(effectsVolume);
    }
}
