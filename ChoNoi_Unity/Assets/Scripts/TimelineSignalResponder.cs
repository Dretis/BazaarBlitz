using Febucci.UI.Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineSignalResponder : MonoBehaviour
{
    // Can call specific events from Timeline, also holds the playabledirectors
    // Should ONLY broadcast events
    private EntityPiece assignedPlayer;

    [Header("pd_UseItem Variables")]
    [SerializeField] private SpriteRenderer usedItemSprite; // for p_UseItem
    [SerializeField] private TextMeshProUGUI usedItemName; // for p_UseItem
    [Header("pd_Warp Variables")]
    [SerializeField] private MapNode destination;
    [SerializeField] private bool warpedByItem;

    [Header("Playable Directors")]
    public PlayableDirector pd_UseItem;
    public PlayableDirector pd_BuildStore;
    public PlayableDirector pd_Warp;
    public PlayableDirector pd_WinGame;

    [Header("Broadcast On Event Channels")]
    public VoidEventChannelSO m_FinishedUsedItem;
    public PlayerEventChannelSO m_BuildStore;
    [Space]
    public PlayerEventChannelSO m_WarpStarted;
    public NodeEventChannelSO m_WarpToNode;
    public PlayerEventChannelSO m_WarpOver;
    [Space]
    public VoidEventChannelSO m_GameFinished;

    [Header("Listen On Event Channels")]
    public IntItemEventChannelSO m_ItemUsed;
    public NodeEventChannelSO m_LandOnWarpNode;
    public NodeEventChannelSO m_WarpByItem;

    private void Start()
    {
        assignedPlayer = gameObject.GetComponent<EntityPiece>();
    }

    private void OnEnable()
    {
        m_ItemUsed.OnEventRaised += OnItemUsed;
        m_LandOnWarpNode.OnEventRaised += OnLandOnWarpNode;
        m_WarpByItem.OnEventRaised += OnWarpByItem;
    }

    private void OnDisable()
    {
        m_ItemUsed.OnEventRaised -= OnItemUsed;
        m_LandOnWarpNode.OnEventRaised -= OnLandOnWarpNode;
        m_WarpByItem.OnEventRaised -= OnWarpByItem;
    }
    private void OnItemUsed(int index, ItemStats item)
    {
        if (assignedPlayer != GameplayTest.instance.currentPlayer) return;
        //var usedItemSprite = currentDirector.gameObject.GetComponentInChildren<SpriteRenderer>();
        usedItemSprite.sprite = item.itemSprite;
        var itemName = $"{item.l_itemName.GetLocalizedString()}!";
        usedItemName.text = itemName;
        //usedItemName.ShowText(itemName);

        pd_UseItem.Play();
        //usedItemSprite = pd_UseItem.GetComponentInChildren<SpriteRenderer>();
    }

    private void OnLandOnWarpNode(MapNode m)
    {
        if (assignedPlayer != GameplayTest.instance.currentPlayer) return;

        if (m is WarpNode)
        {
            var w = m as WarpNode;
            destination = w.GetDestinationNode();
        }

        //Debug.Log($"{assignedPlayer} is warping to {destination.name}");
        pd_Warp.Play();
    }

    private void OnWarpByItem(MapNode m)
    {
        if (assignedPlayer != GameplayTest.instance.currentPlayer) return;

        destination = m;
        warpedByItem = true;

        //Debug.Log($"{assignedPlayer} is warping to {destination.name}");
        pd_Warp.Play();
    }

    #region Signals
    public void FinishUsedItem()
    {
        m_FinishedUsedItem.RaiseEvent();
    }

    public void FinishBuildingStore()
    {
        m_BuildStore.RaiseEvent(assignedPlayer);
    }

    public void SignalWarpStarted()
    {
        Debug.Log($"SignalStartedWarping");
        //m_StartedWarping.RaiseEvent();
        Debug.Log($"{assignedPlayer} is warping to {destination.name}");
        GameplayTest.instance.phase = GameplayTest.GamePhase.IncidentHappening;

        m_WarpStarted.RaiseEvent(assignedPlayer);
    }

    public void SignalWarpToNode()
    {
        m_WarpToNode.RaiseEvent(destination);
    }

    public void SignalWarpOver()
    {
        GameplayTest.instance.phase = GameplayTest.instance.expectedPhase;
        if (warpedByItem)
        {
            FinishUsedItem();
            warpedByItem = false;
        }
        m_WarpOver.RaiseEvent(assignedPlayer);
        //m_FinishedWarping.RaiseEvent();
    }

    public void GameFinished()
    {
        Debug.Log("Game done LOL");
        GameplayTest.instance.phase = GameplayTest.GamePhase.GameOver;
        //m_GameFinished.OnEventRaised();
    }
    #endregion Signals
}
