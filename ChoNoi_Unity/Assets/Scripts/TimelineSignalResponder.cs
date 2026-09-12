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
    [Header("pd_UseItem Variables")]
    [SerializeField] private TextMeshProUGUI levelUpIndicator;
    [SerializeField] private List<TMP_ColorGradient> levelColorGradients;

    [Header("Playable Directors")]
    public PlayableDirector pd_UseItem;
    public PlayableDirector pd_BuildStore;
    public PlayableDirector pd_Warp;
    public PlayableDirector pd_LevelUp;
    public PlayableDirector pd_WinGame;

    [Header("Broadcast On Event Channels")]
    public IntEventChannelSO m_UpdatePlayerScore;
    [Space]
    public VoidEventChannelSO m_FinishedUsedItem;
    public PlayerEventChannelSO m_BuildStore;
    [Space]
    public PlayerEventChannelSO m_WarpStarted;
    public NodeEventChannelSO m_WarpToNode;
    public PlayerEventChannelSO m_WarpOver;
    [Space]
    public PlayerEventChannelSO m_EnterStatAllocation;
    public PlayerEventChannelSO m_EnterBlessingsTree;
    //public VoidEventChannelSO m_ExitStatAllocation;
    [Space]
    public VoidEventChannelSO m_GameFinished;

    [Header("Listen On Event Channels")]
    public IntItemEventChannelSO m_ItemUsed;
    public NodeEventChannelSO m_LandOnWarpNode;
    public NodeEventChannelSO m_WarpByItem;
    public PlayerEventChannelSO m_EnterLevelUp;

    private void Start()
    {
        assignedPlayer = gameObject.GetComponent<EntityPiece>();
    }

    private void OnEnable()
    {
        m_ItemUsed.OnEventRaised += OnItemUsed;
        m_LandOnWarpNode.OnEventRaised += OnLandOnWarpNode;
        m_WarpByItem.OnEventRaised += OnWarpByItem;
        m_EnterLevelUp.OnEventRaised += OnEnterLevelUp;
    }

    private void OnDisable()
    {
        m_ItemUsed.OnEventRaised -= OnItemUsed;
        m_LandOnWarpNode.OnEventRaised -= OnLandOnWarpNode;
        m_WarpByItem.OnEventRaised -= OnWarpByItem;
        m_EnterLevelUp.OnEventRaised -= OnEnterLevelUp;
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
    
    private void OnEnterLevelUp(EntityPiece p)
    {
        if (assignedPlayer != p) return;

        p.unspentLevelUpPoints += 6;
        p.maxHealth += 5;
        p.health += 5;
        p.RenownLevel += 1;
        p.levelThreshold = (p.RenownLevel * 100) * (Mathf.Pow(1.15f, p.RenownLevel - 1));

        var lvl = p.RenownLevel;
        if (lvl >= 10)
        {
            levelUpIndicator.colorGradientPreset = levelColorGradients[levelColorGradients.Count - 1];
        }
        else
        {
            levelUpIndicator.colorGradientPreset = levelColorGradients[lvl - 1];
        }

        levelUpIndicator.text = $"*\n{lvl}";

        m_UpdatePlayerScore.RaiseEvent(p.id);
        //Debug.Log($"{assignedPlayer} is warping to {destination.name}");
        pd_LevelUp.Play();
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
    public void SignalLevelUpStarted()
    {
        //Debug.Log($"SignalLevelUpStarted");
        //m_StartedWarping.RaiseEvent();
        //Debug.Log($"{assignedPlayer} has leveled up");
        GameplayTest.instance.phase = GameplayTest.GamePhase.IncidentHappening;

        //m_WarpStarted.RaiseEvent(assignedPlayer);
    }

    public void SignalLevelUpOver()
    {
        //GameplayTest.instance.phase = GameplayTest.instance.expectedPhase;
        switch (assignedPlayer.RenownLevel)
        {
            case 3:
                // Blessing
                GameplayTest.instance.phase = GameplayTest.GamePhase.BlessingTree;
                GameplayTest.instance.expectedPhase = GameplayTest.GamePhase.LevelUp;
                m_EnterBlessingsTree.RaiseEvent(assignedPlayer);
                break;
            default:
                //m_EnterLevelUp.RaiseEvent(assignedPlayer);
                GameplayTest.instance.phase = GameplayTest.GamePhase.LevelUp;
                GameplayTest.instance.expectedPhase = GameplayTest.GamePhase.RollDice;
                m_EnterStatAllocation.RaiseEvent(assignedPlayer);
                break;
        }
        //m_EnterLevelUp.RaiseEvent(assignedPlayer);
    }

    public void GameFinished()
    {
        Debug.Log("Game done LOL");
        GameplayTest.instance.phase = GameplayTest.GamePhase.GameOver;
        //m_GameFinished.OnEventRaised();
    }
    #endregion Signals
}
