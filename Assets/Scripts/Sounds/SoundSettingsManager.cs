using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundSettingsManager : MonoBehaviour
{
    public bool isPaused = false;
    public SoundManager soundManager;
    public FMOD.Studio.EventInstance FMODinstance;
    public FMOD.Studio.EventInstance FMODinstance2;
    public FMOD.Studio.EventInstance FMODinstance3;
    public GameObject pauseMenu;
    private Canvas pauseCanvas;
    public GameObject musicSlider;
    public GameObject sfxSlider;



    // Start is called before the first frame update
    void Start()
    {
        FMODinstance = soundManager.overworldThemeInstance;
        FMODinstance2 = soundManager.battleThemeInstance;
        FMODinstance3 = soundManager.diceRollInstance;
        pauseCanvas = pauseMenu.GetComponent<Canvas>();
        musicSlider.GetComponent<Slider>().value = soundManager.musicVolume;
        sfxSlider.GetComponent<Slider>().value = soundManager.SFXVolume;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) & !isPaused)
        {
            pauseCanvas.enabled = true;
            isPaused = true;
            Time.timeScale = 0.0f;
        }
        else if (Input.GetKeyDown(KeyCode.Escape) & isPaused)
        {
            pauseCanvas.enabled = false;
            isPaused = false;
            Time.timeScale = 1.0f;
        }
    }

    public void MusicSlider(float volume)
    {
        FMODinstance.setParameterByName("MusicVolume", volume);
        FMODinstance2.setParameterByName("MusicVolume", volume);
        soundManager.musicVolume = volume;
    }

    public void SFXSlider(float volume)
    {
        FMODinstance3.setParameterByName("SoundVolume", volume);
        soundManager.SFXVolume = volume;
    }

}
