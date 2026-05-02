using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    private enum VolumeType
    {
        MASTER,
        MUSIC,
        EFFECTS
    }

    [SerializeField] private VolumeType volumeType;

    private Slider volumeSlider;
    private Color volumeDefaultValueColor;

    [SerializeField] private TextMeshProUGUI volumeValueText;

    // Start is called before the first frame update
    void Awake()
    {
        volumeSlider = GetComponent<Slider>();
        volumeDefaultValueColor = volumeValueText.color;
    }

    private void Start()
    {
        float volume = 1;
        switch (volumeType)
        {
            case VolumeType.MASTER:
                volume = AudioManager.instance.masterVolume;
                break;
            case VolumeType.MUSIC:
                volume = AudioManager.instance.musicVolume;
                break;
            case VolumeType.EFFECTS:
                volume = AudioManager.instance.effectsVolume;
                break;
            default:
                Debug.Log($"{volumeType} | Volume type not supported. Error!");
                break;
        }

        volumeSlider.value = volume * 100;
        volumeValueText.text = volumeSlider.value.ToString();
    }

    public void UpdateVolumeValue()
    {
        var volume = volumeSlider.value / 100;
        switch (volumeType)
        {
            case VolumeType.MASTER:
                AudioManager.instance.masterVolume = volume;
                break;
            case VolumeType.MUSIC:
                AudioManager.instance.musicVolume = volume;
                break;
            case VolumeType.EFFECTS:
                AudioManager.instance.effectsVolume = volume;
                break;
            default:
                Debug.Log($"{volumeType} | Volume type not supported. Error!");
                break;
        }

        if(volumeSlider.value == 0) volumeValueText.color = Color.grey;
        else volumeValueText.color = volumeDefaultValueColor;

        volumeValueText.text = volumeSlider.value.ToString();
    }
}
