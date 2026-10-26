using Core;
using Core.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering.Universal;

public class StampComponent : MonoBehaviour
{
    private BoardNodeManager _boardNodeManager;
    private BoardNode _boardNode;

    [Header("Stamp Visual Variables")]
    [SerializeField] private SpriteRenderer stampVisual;
    [SerializeField] private Light2D stampLight;

    [SerializeField] private PlayableDirector pd_nCollectStamp;

    [SerializeField] private GameStateEventChannelSO _gameStateEvent;

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
        if (gameState.NewState.CurrentPlayer().EntityData.OccupiedNodeId != _boardNode.Id) return;
        if (!gameState.DidChangePhases()) return;

        if (gameState.DidChangeStamps(gameState.NewState.CurrPlayerId))
        {
            PlayCollectStamp();
        }
    }

    public void Initialize(BoardNodeManager bnm, BoardNode bn)
    {
        _boardNodeManager = bnm;
        _boardNode = bn;
        //_boardNode.BoardNodeData.StampData = new StampData();
    }

    public void UpdateVisual(StampData stampData)
    {
        _boardNode.nodeVisual.sprite = _boardNodeManager.Database.GetStampNodeSprite(stampData.CurrentStampType);

        stampVisual.sprite = _boardNodeManager.Database.GetStampVisualSprite(stampData.CurrentStampType);
        stampLight.color = _boardNodeManager.Database.GetStampColor(stampData.CurrentStampType);

        stampVisual.gameObject.SetActive(true);
    }

    public void PlayCollectStamp()
    {
        pd_nCollectStamp.Play();
    }
    
    public void HideVisual()
    {
        stampVisual.gameObject.SetActive(false);
    }
}
