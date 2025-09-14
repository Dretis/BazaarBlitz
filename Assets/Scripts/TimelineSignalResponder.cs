using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimelineSignalResponder : MonoBehaviour
{
    // Can call specific events from Timeline
    // Should ONLY broadcast events

    [Header("Broadcast On Event Channels")]
    public VoidEventChannelSO m_FinishedUsedItem;

    public void FinishUsedItem()
    {
        m_FinishedUsedItem.OnEventRaised();
    }
}
