using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<AudioClip> soundList;

    [Header("Listen on Event Channels")]
    public PlayerEventChannelSO m_PassByStamp;


    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        m_PassByStamp.OnEventRaised += PlayStampSound;
        Debug.Log("heard stamp pass event and subbed");
    }

    private void OnDisable()
    {
        m_PassByStamp.OnEventRaised -= PlayStampSound;
    }

    private void PlayStampSound(EntityPiece entity)
    {
        audioSource.PlayOneShot(soundList[0], 1f);
        Debug.Log("played stamp sound");
    }

    


    
}
