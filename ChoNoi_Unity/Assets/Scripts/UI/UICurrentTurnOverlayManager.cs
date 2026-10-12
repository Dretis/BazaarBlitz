using Core;
using Core.Events;
using Febucci.UI.Core;
using LitMotion;
using LitMotion.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.SmartFormat.PersistentVariables;


public class UICurrentTurnOverlayManager : MonoBehaviour
{
    public GameStateEventChannelSO _gameStateEvent;

    private Coroutine _oldNextPlayerGo;
    private MotionHandle _goTurnMotion;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI goPlayerText;
    [SerializeField] private TypewriterCore turnTypewriter; // move to UICurrentTurnOverlayManager

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI turnRoundIndicator; // move to UICurrentTurnOverlayManager

    private void OnEnable()
    {
        _gameStateEvent.OnEventRaised += UpdateUIFromGameState;
    }

    private void OnDisable()
    {
        _gameStateEvent.OnEventRaised -= UpdateUIFromGameState;
    }

    private void Start()
    {
        //_oldNextPlayerGo = StartCoroutine(NotifyNextPlayerGo(GameplayTest.instance.currentPlayer));
    }

    private void UpdateUIFromGameState(FrameGameState gameState)
    {
        if (gameState.DidChangePhases())
        {
            //var index = gameState.NewState.CurrPlayerId;
            //var currPlayer = gameState.NewState.Players[index];

            if (gameState.NewState.GamePhase is GameplayTest.GamePhase.StartRound)
            {
                turnRoundIndicator.text = $"{gameState.NewState.Round}";
            }
            if (gameState.PrevState.GamePhase is GameplayTest.GamePhase.StartTurn)
            {
                Debug.Log($"Go, {gameState.NewState.CurrentPlayer().EntityData.EntityName}!");
                if (_goTurnMotion.IsPlaying())
                {
                    //NextPlayerGoIsRunning = false;
                    StopCoroutine(_oldNextPlayerGo);
                    _goTurnMotion.TryCancel();
                }

                _oldNextPlayerGo = StartCoroutine(NotifyNextPlayerGo(gameState.NewState.CurrentPlayer()));
            }
        }
    }

    private IEnumerator NotifyNextPlayerGo(PlayerState ps)
    {
        var ed = ps.EntityData;

        string goLine = "{offset}{size}Go, " + ed.EntityName + "!";
        var localizedString = turnTypewriter.GetComponent<LocalizeStringEvent>().StringReference;

        var variable = localizedString["PLAYER_NAME"] as StringVariable;
        variable.Value = ed.EntityName;

        turnTypewriter.GetComponent<TextMeshProUGUI>().color = ed.MainColor;
        turnTypewriter.ShowText(localizedString.GetLocalizedString());

        var goTurnRect = turnTypewriter.GetComponent<RectTransform>();
        Vector2 startingPos = new Vector2(0, 150);
        Vector2 showingPos = new Vector2(0, -25);

        _goTurnMotion = LMotion.Create(startingPos, showingPos, 1f)
            .WithEase(Ease.OutBack)
            .BindToAnchoredPosition(goTurnRect);

        yield return new WaitForSeconds(2f);

        _goTurnMotion = LMotion.Create(showingPos, startingPos, .75f)
            .WithEase(Ease.OutQuad)
            .BindToAnchoredPosition(goTurnRect);

        yield return null;
    }
}
