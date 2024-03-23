using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraObserver : MonoBehaviour
{
    private CinemachineVirtualCamera vcam;

    [Header("Listen on Event Channels")]
    public PlayerEventChannelSO m_NextPlayerTurn;
    // public PlayerEventChannelSO m_FreeLook; // For when the player wants to look around the map via the menu option

    private void OnEnable()
    {
        m_NextPlayerTurn.OnEventRaised += SwitchTargetFocus;
    }

    private void OnDisable()
    {
        m_NextPlayerTurn.OnEventRaised += SwitchTargetFocus;
    }

    // Start is called before the first frame update
    void Start()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
    }

    void SwitchTargetFocus(EntityPiece entity)
    {
        vcam.Follow = entity.transform;
    }
}
