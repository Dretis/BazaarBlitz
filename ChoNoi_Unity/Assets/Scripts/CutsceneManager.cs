using Core;
using Core.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

/// <summary>
/// UNUSED!
/// Handles what cutscene to play given the game state. 
/// </summary>
public class CutsceneManager : MonoBehaviour
{
    private GameplayTest _gameplayTest;

    [SerializeField] private GameStateEventChannelSO _gameStateEvent;
    [SerializeField] private List<TimelineAsset> _timelineAssets;

    // Supposed to replace TimelineSignalResponder and TimelineManger
    private void OnEnable()
    {
        _gameStateEvent.OnEventRaised += DetermineCutsceneFromGameState;

    }

    private void OnDisable()
    {
        _gameStateEvent.OnEventRaised -= DetermineCutsceneFromGameState;

    }
    private void DetermineCutsceneFromGameState(FrameGameState gameState)
    {
        if (!gameState.DidChangePhases()) return;

        if (gameState.NewState.GamePhase is GameplayTest.GamePhase.IncidentHappening)
        {

        }
    }
}
