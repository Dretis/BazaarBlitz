using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineSignalResponder : MonoBehaviour
{
    // Can call specific events from Timeline, also holds the playabledirectors
    // Should ONLY broadcast events

    [Header("Playable Directors")]
    public PlayableDirector pd_UseItem;
    public PlayableDirector pd_WinGame;

    [Header("Broadcast On Event Channels")]
    public VoidEventChannelSO m_FinishedUsedItem;
    public VoidEventChannelSO m_GameFinished; 

    public void FinishUsedItem()
    {
        m_FinishedUsedItem.OnEventRaised();
    }

    public void GameFinished()
    {
        Debug.Log("Game done LOL");
        //m_GameFinished.OnEventRaised();
    }
}
