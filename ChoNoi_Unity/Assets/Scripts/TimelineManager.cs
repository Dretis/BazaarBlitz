using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using static GameplayTest;

public class TimelineManager : MonoBehaviour
{
    // script includes handling Timelines, should really be only for non-player based cutscenes

    [SerializeField] private TimelineSignalResponder currentResponder; // Current player's director to play timelines

    [SerializeField] private SpriteRenderer usedItemSprite; // for p_UseItem
    [SerializeField] private MapNode destination; // for p_Warp

    //private GameplayTest.GamePhase expectedPhase; // After the cutscene, what is expected to be next

    [Header("Listen On Event Channels")]
    public PlayerEventChannelSO m_NextPlayerTurn;
    //public IntEventChannelSO m_UseItemAt; // aka m_ItemUsed or "I want to Use Inventory Item At"
    public IntItemEventChannelSO m_ItemUsed; // aka m_ItemUsed or "I want to Use Inventory Item At"
    public PlayerEventChannelSO m_ConfirmBuildStore;
    //public NodeEventChannelSO m_LandOnWarpNode;
    //public PlayerEventChannelSO m_ResetToIdle;
    public PlayerEventChannelSO m_PlayerWon;

    //public NodeEventChannelSO m_UpgradeStore;

    private void OnEnable()
    {
        m_NextPlayerTurn.OnEventRaised += OnNextPlayerTurn;
        //m_UseItemAt.OnEventRaised += OnUseItemAt;
        m_ItemUsed.OnEventRaised += OnItemUsed;
        m_ConfirmBuildStore.OnEventRaised += OnConfirmBuildStore;
        //m_LandOnWarpNode.OnEventRaised += OnLandOnWarpNode;
        m_PlayerWon.OnEventRaised += OnPlayerWon;
        //m_ResetToIdle.OnEventRaised += ResetToIdleAnim;
        //m_UpgradeStore.OnEventRaised += OnUpgradeStore;

    }

    private void OnDisable()
    {
        m_NextPlayerTurn.OnEventRaised -= OnNextPlayerTurn;
        //m_UseItemAt.OnEventRaised -= OnUseItemAt;
        m_ItemUsed.OnEventRaised -= OnItemUsed;
        m_ConfirmBuildStore.OnEventRaised -= OnConfirmBuildStore;
        //m_LandOnWarpNode.OnEventRaised -= OnLandOnWarpNode;
        m_PlayerWon.OnEventRaised -= OnPlayerWon;
        //m_ResetToIdle.OnEventRaised -= ResetToIdleAnim;
        //m_UpgradeStore.OnEventRaised -= OnUpgradeStore;
    }
    private void OnNextPlayerTurn(EntityPiece entity)
    {
        if (entity.TryGetComponent<TimelineSignalResponder>(out TimelineSignalResponder responder))
        {
            currentResponder = responder;
            usedItemSprite = responder.pd_UseItem.GetComponentInChildren<SpriteRenderer>();
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
        //usedItemSprite.sprite = item.itemSprite;

        //currentResponder.pd_UseItem.Play();
        //usedItemSprite = currentResponder.pd_UseItem.GetComponentInChildren<SpriteRenderer>();

    }

    private void OnConfirmBuildStore(EntityPiece currentPlayer)
    {
        if (currentPlayer.TryGetComponent<TimelineSignalResponder>(out TimelineSignalResponder responder))
        {
            Debug.Log($"{currentPlayer} is building a store");

            if (currentPlayer.playerSprite.flipX) currentPlayer.playerSprite.flipX = false;

            responder.pd_BuildStore.Play();
        }
    }

    private void OnLandOnWarpNode(MapNode m)
    {
        /*
        //GameplayTest.instance.expectedPhase = GamePhase.EndTurn;
        if (m is WarpNode)
        {
            var w = m as WarpNode;
            destination = w.GetDestinationNode();
        }

        PlayWarp(GameplayTest.instance.currentPlayer);
        */
    }

    private void PlayWarp(EntityPiece currentPlayer)
    {
        if (currentPlayer.TryGetComponent<TimelineSignalResponder>(out TimelineSignalResponder responder))
        {
            Debug.Log($"{currentPlayer} is warping to...");

            //if (currentPlayer.playerSprite.flipX) currentPlayer.playerSprite.flipX = false;

            responder.pd_Warp.Play();
        }
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

    private void OnUpgradeStore(MapNode node)
    {

    }
}
