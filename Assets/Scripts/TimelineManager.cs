using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using static UnityEngine.EventSystems.EventTrigger;

public class TimelineManager : MonoBehaviour
{
    // script includes handling Timelines

    [SerializeField] private PlayableDirector currentDirector; // Current player's director to play timelines
    [SerializeField] private SpriteRenderer usedItemSprite; // Current player's director to play timelines

    [Header("Listen On Event Channels")]
    public PlayerEventChannelSO m_NextPlayerTurn;
    //public IntEventChannelSO m_UseItemAt; // aka m_ItemUsed or "I want to Use Inventory Item At"
    public IntItemEventChannelSO m_ItemUsed; // aka m_ItemUsed or "I want to Use Inventory Item At"

    public PlayerEventChannelSO m_ResetToIdle;
    public PlayerEventChannelSO m_PlayerWon;

    private void OnEnable()
    {
        m_NextPlayerTurn.OnEventRaised += OnNextPlayerTurn;
        //m_UseItemAt.OnEventRaised += OnUseItemAt;
        m_ItemUsed.OnEventRaised += OnItemUsed;
        m_PlayerWon.OnEventRaised += OnPlayerWon;
        //m_ResetToIdle.OnEventRaised += ResetToIdleAnim;
    }

    private void OnDisable()
    {
        m_NextPlayerTurn.OnEventRaised -= OnNextPlayerTurn;
        //m_UseItemAt.OnEventRaised -= OnUseItemAt;
        m_ItemUsed.OnEventRaised -= OnItemUsed;
        m_PlayerWon.OnEventRaised -= OnPlayerWon;
        //m_ResetToIdle.OnEventRaised -= ResetToIdleAnim;
    }
    private void OnNextPlayerTurn(EntityPiece entity)
    {
        if (entity.TryGetComponent<TimelineSignalResponder>(out TimelineSignalResponder responder))
        {
            currentDirector = responder.pd_UseItem;
            usedItemSprite = currentDirector.GetComponentInChildren<SpriteRenderer>();
        }

        //currentDirector = entity.GetComponentInChildren<PlayableDirector>();
        //usedItemSprite = currentDirector.GetComponentInChildren<SpriteRenderer>();
    }

    private void OnUseItemAt(int index)
    {

    }

    private void OnItemUsed(int index, ItemStats item)
    {
        //var usedItemSprite = currentDirector.gameObject.GetComponentInChildren<SpriteRenderer>();
        usedItemSprite.sprite = item.itemSprite;

        // Make sure this plays PD_UseItem!
        currentDirector.Play();
    }

    private void OnPlayerWon(EntityPiece winner)
    {
        if (winner.TryGetComponent<TimelineSignalResponder>(out TimelineSignalResponder responder))
        {
            Debug.Log($"{winner} won game play timeline");
            responder.pd_WinGame.Play();
        }
        //currentDirector = winner.GetComponentInChildren<PlayableDirector>();
        //currentDirector.Play();

        //winner.pd_winGame.Play();
    }
}
