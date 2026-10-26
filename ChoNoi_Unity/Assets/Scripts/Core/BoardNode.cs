using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Core;
using Core.Events;
using System;

// Visual aspect?
public class BoardNode : MonoBehaviour // Network object?
{
    // Replaces MapNode
    private BoardNodeManager _boardNodeManager;
    public int Id { get => BoardNodeData.Id; }
    public BoardNodeData BoardNodeData;
    [SerializeField] private GameStateEventChannelSO _gameStateEvent;

    [Header("Visual Variables")]
    public SpriteRenderer nodeVisual;
    [SerializeField] private SpriteRenderer deployableVisual;
    [SerializeField] private Sprite[] deployableSprites; // put this in a lookuptable?

    [Header("Type Components")]
    public StorefrontComponent StorefrontComponent;
    public StampComponent StampComponent;

    [Header("Debug")]
    [SerializeField] private bool isDebugOn;
    [SerializeField] public TextMeshProUGUI debugIdText;
    [SerializeField] public TextMeshProUGUI debugTypeText;

    public void Initialize(BoardNodeManager bnm)
    {
        _boardNodeManager = bnm;
        StorefrontComponent.Initialize(this);
        StampComponent.Initialize(_boardNodeManager, this);
        OnTypeChanged();
    }

    private void OnEnable()
    {
        _gameStateEvent.OnEventRaised += UpdateFromGameState;
    }

    private void OnDisable()
    {
        _gameStateEvent.OnEventRaised -= UpdateFromGameState;
    }

    private void UpdateFromGameState(FrameGameState gameState)
    {
        var prevData = gameState.PrevState.BoardNodes[Id];
        var newData = gameState.NewState.BoardNodes[Id];
        BoardNodeData = newData;

        if (prevData.Type != newData.Type)
        {
            OnTypeChanged();
        }


        if (gameState.NewState.CurrentPlayer().EntityData.OccupiedNodeId != Id) return;
        
        if(gameState.PrevState.GamePhase is GameplayTest.GamePhase.PassBy)
        {
            //Debug.Log($"{name} - I just got passed by!");
        }
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        // DEBUG EDITOR ONLY

        name = $"{BoardNodeData.Id}_Node-{BoardNodeData.Type}";
        if (BoardNodeData.Type is NodeType.Stamp)
        {
            name += $"_{BoardNodeData.StampData.CurrentStampType}";
        }

        if (_boardNodeManager) OnTypeChanged();

        if (isDebugOn)
        {
            debugIdText.text = $"{BoardNodeData.Id}";
            debugTypeText.text = $"{BoardNodeData.Type}";
            //debugIdText.color = _database.GetNodeInfo(BoardNodeData.Type).nodeHoverColor;
        }
        else
        {
            debugIdText.text = "";
            debugTypeText.text = "";
        }
    }

    private void Update()
    {
        if (isDebugOn)
        {
            debugIdText.text = $"{BoardNodeData.Id}";
            debugTypeText.text = $"{BoardNodeData.Type}";
        }
        else
        {
            debugIdText.text = "";
            debugTypeText.text = "";
        }
    }
    #endif
    private void OnTypeChanged()
    {
        nodeVisual.sprite = _boardNodeManager.Database.GetNodeInfo(BoardNodeData.Type).nodeSprite;
        //StorefrontComponent.HideStoreVisual();

        switch (BoardNodeData.Type)
        {
            case NodeType.OpenSpace:
                // Nothing
                break;
            case NodeType.Storefront:
                //StorefrontComponent.enabled = true;
                StorefrontComponent.Initialize(this);
                break;
            case NodeType.Exchange:
                //nodeVisual.sprite = _boardNodeManager.Database.GetNodeInfo(BoardNodeData.Type).nodeSprite;
                break;
            case NodeType.Stamp:
                //StampComponent.enabled = false;
                StampComponent.Initialize(_boardNodeManager, this);
                StampComponent.UpdateVisual(BoardNodeData.StampData);
                break;
            case NodeType.Wildcard:

                break;
            case NodeType.MoveAgain:

                break;
            case NodeType.Warp:

                break;
            case NodeType.Vendor:

                break;
            case NodeType.LuckyDice:

                break;
            case NodeType.CoconutTree:

                break;
            default:
                Debug.Log("Invalid Node type!");
                break;
        }
    }
}
