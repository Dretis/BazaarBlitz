using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineSignalResponder : MonoBehaviour
{
    // Can call specific events from Timeline, also holds the playabledirectors
    // Should ONLY broadcast events
    private EntityPiece assignedPlayer;

    [Header("Playable Directors")]
    public PlayableDirector pd_UseItem;
    public PlayableDirector pd_BuildStore;
    public PlayableDirector pd_WinGame;

    [Header("Broadcast On Event Channels")]
    public VoidEventChannelSO m_FinishedUsedItem;
    public PlayerEventChannelSO m_BuildStore;
    public VoidEventChannelSO m_GameFinished;

    private void Start()
    {
        assignedPlayer = gameObject.GetComponent<EntityPiece>();
    }

    public void FinishUsedItem()
    {
        m_FinishedUsedItem.OnEventRaised();
    }

    public void FinishBuildingStore()
    {
        m_BuildStore.OnEventRaised(assignedPlayer);
    }

    public void GameFinished()
    {
        Debug.Log("Game done LOL");
        //m_GameFinished.OnEventRaised();
    }
}
