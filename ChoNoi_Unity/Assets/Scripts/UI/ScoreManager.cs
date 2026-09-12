using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Febucci.UI.Core;
using LitMotion;
using LitMotion.Extensions;
using System.Collections;
using Coffee.UIEffects;


public class ScoreManager : MonoBehaviour
{
    private MotionHandle currentMotion;

    [SerializeField] private EntityPiece currentPlayer;
    [SerializeField] private List<EntityPiece> players;

    [Header("Player Scores")]
    [SerializeField] private List<PlayerScoreHandler> playerScoreHandlers;

    [Header("Additional UI Elements")]
    [SerializeField] private Canvas scoreCanvas;
    [SerializeField] private RectTransform scoreContainerTransform;

    //[Header("Broadcast On Event Channels")]
    //public PlayerEventChannelSO m_CheerForPlayer;

    [Header("Listen On Event Channels")]
    public VoidEventChannelSO m_GameStart; // should only be called once

    public IntEventChannelSO m_ChangeInScore;
    public IntEventChannelSO m_PlayerScoreDecreased;
    public IntEventChannelSO m_PlayerScoreIncreased;

    public PlayerEventChannelSO m_NextPlayerTurn;

    public VoidEventChannelSO m_HoldPlayerInfo;
    public VoidEventChannelSO m_ReleasePlayerInfo;

    public PlayerEventChannelSO m_FinishStockingStore;
    public VoidEventChannelSO m_FinishedUsedItem;

    public PlayerEventChannelSO m_PassedByStamp;
    public StampEventChannelSO m_UndoPassByStamp;

    public VoidEventChannelSO m_PassByPawnShop;
    public PlayerEventChannelSO m_UndoPassByPawnShop;

    public VoidEventChannelSO m_ExitLevelUp;
    public ItemEventChannelSO m_PlayerPickedFirstBlessing;

    public PlayerEventChannelSO m_RefreshedActiveEffects;

    public PlayerEventChannelSO m_PlayerWon;

    private void OnEnable()
    {
        //m_GameStart.OnEventRaised += OnGameStart;

        m_ChangeInScore.OnEventRaised += UpdateScoreForPlayer;
        m_PlayerScoreDecreased.OnEventRaised += OnPlayerScoreDecreased;
        m_PlayerScoreIncreased.OnEventRaised += OnPlayerScoreIncreased;

        m_NextPlayerTurn.OnEventRaised += ChangeCurrentPlayer;
        //m_NextPlayerTurn.OnEventRaised += UpdateHeldStamps;

        m_HoldPlayerInfo.OnEventRaised += OnHoldPlayerInfo;
        m_ReleasePlayerInfo.OnEventRaised += OnReleasePlayerInfo;

        m_FinishStockingStore.OnEventRaised += OnFinishStockingStore;
        m_FinishedUsedItem.OnEventRaised += OnFinishedUsedItem;

        m_PassedByStamp.OnEventRaised += OnPassedByStamp;
        m_UndoPassByStamp.OnEventRaised += OnUndoPassByStamp;

        m_PassByPawnShop.OnEventRaised += OnPassByPawnShop;
        m_UndoPassByPawnShop.OnEventRaised += OnUndoPassByPawnShop;

        m_ExitLevelUp.OnEventRaised += OnExitLevelUp;
        m_PlayerPickedFirstBlessing.OnEventRaised += OnPlayerPickedFirstBlessing;

        m_RefreshedActiveEffects.OnEventRaised += OnRefreshedActiveEffects;

        m_PlayerWon.OnEventRaised += OnPlayerWon;
        m_GameStart.OnEventRaised += OnGameStart;
    }

    private void OnDisable()
    {
        //m_GameStart.OnEventRaised -= OnGameStart;

        m_ChangeInScore.OnEventRaised -= UpdateScoreForPlayer;
        m_PlayerScoreDecreased.OnEventRaised -= OnPlayerScoreDecreased;
        m_PlayerScoreIncreased.OnEventRaised -= OnPlayerScoreIncreased;

        m_NextPlayerTurn.OnEventRaised -= ChangeCurrentPlayer;
        //m_NextPlayerTurn.OnEventRaised -= UpdateHeldStamps;

        m_HoldPlayerInfo.OnEventRaised -= OnHoldPlayerInfo;
        m_ReleasePlayerInfo.OnEventRaised -= OnReleasePlayerInfo;

        m_FinishStockingStore.OnEventRaised -= OnFinishStockingStore;
        m_FinishedUsedItem.OnEventRaised -= OnFinishedUsedItem;

        m_PassedByStamp.OnEventRaised -= OnPassedByStamp;
        m_UndoPassByStamp.OnEventRaised -= OnUndoPassByStamp;

        m_PassByPawnShop.OnEventRaised -= OnPassByPawnShop;
        m_UndoPassByPawnShop.OnEventRaised -= OnUndoPassByPawnShop;

        m_ExitLevelUp.OnEventRaised -= OnExitLevelUp;
        m_PlayerPickedFirstBlessing.OnEventRaised -= OnPlayerPickedFirstBlessing;

        m_RefreshedActiveEffects.OnEventRaised -= OnRefreshedActiveEffects;

        m_PlayerWon.OnEventRaised -= OnPlayerWon;
        m_GameStart.OnEventRaised -= OnGameStart;
    }

    void Awake()
    {
        Debug.Log("Awake scoremanager");
        InitializePlayerScores();
        Debug.Log("Awake scoremanager2");
    }

    private void InitializePlayerScores()
    {
        //playerInfoDiceNumbers.Add("1");
        players = GameplayTest.instance.playerUnits;
        Debug.Log("InitializePlayerScores scoremanager");
        // Initial Setup
        for (int i = 0; i < players.Count; i++)
        {
            //Debug.Log($"i={i} | {players[i].name}");
            playerScoreHandlers[i].AssignPlayerToScore(players[i]);
            playerScoreHandlers[i].InitialScoreSetup();
            playerScoreHandlers[i].ToggleCurrentPlayerEffect(false);
        }
    }

    private void OnGameStart()
    {
        Debug.Log($"ScoreManager | OnGameStart");
        //InitializePlayerScores();
        var playerCount = GameplayTest.instance.currentRuleset.numberOfPlayers;
        players = GameplayTest.instance.playerUnits;

        for (int i = 4; i > playerCount; i--)
        {
            Debug.Log($"playerScoreHandlers[^1] = {playerScoreHandlers[^1]}");
            //playerUnits.Remove(playerUnits[^1]);
            //playerScoreHandlers[^1].gameObject.SetActive(false);
            //playerScoreHandlers[^1].enabled = false;

            playerScoreHandlers[^1].DisablePlayerScore();
            playerScoreHandlers.Remove(playerScoreHandlers[^1]);
        }
    }

    private void OnPlayerScoreDecreased(int scoreChange)
    {
        // Prob some text vfx that shows it going down
        currentPlayer.coinDrop.Play();
    }

    private void OnPlayerScoreIncreased(int scoreChange)
    {
        // Prob some text vfx that shows it going up lol
        //Debug.Log("yo i got more money");
        currentPlayer.coinSucking.Play();
        StartCoroutine(ShowCoinGainNumber(currentPlayer, scoreChange));
    }

    private IEnumerator ShowCoinGainNumber(EntityPiece entity, int coinGain)
    {
        var coinGainLocalScale = entity.coinGainNumber.GetComponent<RectTransform>().localScale;
        coinGainLocalScale = Vector3.zero;

        LMotion.Create(coinGainLocalScale, Vector3.one, 0.2f)
            .WithEase(Ease.OutBack)
            .Bind(x => coinGainLocalScale = x);

        if (!(entity.heldPoints >= GameplayTest.instance.currentRuleset.pointGoal))
        {
            entity.coinGainNumber.ShowText($"+{coinGain}<sprite=\"Coin Icon\" index=0> ");
        }
        //entity.coinGainNumber.ShowText($"+{coinGain}<sprite=\"Coin Icon\" index=0> ");

        yield return new WaitForSeconds(1f);

        LMotion.Create(coinGainLocalScale, Vector3.zero, 0.5f)
            .WithEase(Ease.OutQuad)
            .Bind(x => coinGainLocalScale = x);

        entity.coinGainNumber.StartDisappearingText();
        yield return null;
    }

    private void UpdateScoreForPlayer(int id)
    {
        // Update specific player score on the scoreboard based on their ID.
        playerScoreHandlers[id].UpdateScore();
    }

    private void ChangeCurrentPlayer(EntityPiece ps)
    {
        if (currentPlayer != null)
            playerScoreHandlers[currentPlayer.id].ToggleCurrentPlayerEffect(false);

        currentPlayer = ps;

        playerScoreHandlers[currentPlayer.id].ToggleCurrentPlayerEffect(true);
        playerScoreHandlers[currentPlayer.id].UpdateHeldStamps();

        //test!!!
        //playerScoreHandlers[currentPlayer.id].ShakePlayerSprite(0.2f);
    }

    private void OnPassedByStamp(EntityPiece ps)
    {
        var id = ps.id;
        playerScoreHandlers[id].UpdateHeldStamps();
    }

    private void OnUndoPassByStamp(Stamp.StampType type)
    {
        var id = currentPlayer.id;
        playerScoreHandlers[id].ClearHeldStamps();
        playerScoreHandlers[id].UpdateHeldStamps();
    }

    private void OnPassByPawnShop()
    {
        playerScoreHandlers[currentPlayer.id].ClearHeldStamps();
    }

    private void OnUndoPassByPawnShop(EntityPiece ps)
    {
        var id = ps.id;

        playerScoreHandlers[id].ClearHeldStamps();
        playerScoreHandlers[id].UpdateHeldStamps();
    }

    private void OnHoldPlayerInfo()
    {
        // Lift up scoreboard to show additional info
        currentMotion = LMotion.Create(scoreContainerTransform.anchoredPosition, new Vector2(0, 150+58), 0.25f)
            //.WithEase(Ease.InQuad)
            .WithEase(Ease.OutBack)
            .BindToAnchoredPosition(scoreContainerTransform);
    }

    private void OnReleasePlayerInfo()
    {
        // Bring scoreboard back down to normal
        if (currentMotion.IsActive()) currentMotion.Cancel();

        currentMotion = LMotion.Create(scoreContainerTransform.anchoredPosition, new Vector2(0, 4), 0.25f)
            .WithEase(Ease.OutQuad)
            .WithEase(Ease.OutBack)
            .BindToAnchoredPosition(scoreContainerTransform);
    }

    private void OnFinishStockingStore(EntityPiece ps)
    {
        //UpdateInfoInvItemsForPlayer(ps.id);
        var id = ps.id;
        playerScoreHandlers[id].UpdateInfoInvItems();
        playerScoreHandlers[id].UpdateMoneyInterest();
    }
    private void OnFinishedUsedItem()
    {
        //UpdateInfoInvItemsForPlayer(ps.id);
        var id = currentPlayer.id;
        playerScoreHandlers[id].UpdateInfoInvItems();
    }

    private void OnExitLevelUp()
    {
        Debug.Log("updating");
        var id = currentPlayer.id;
        playerScoreHandlers[id].UpdateScore();
    }

    private void OnPlayerPickedFirstBlessing(ItemStats blessing)
    {
        var id = currentPlayer.id;
        playerScoreHandlers[id].UnlockTalentGroup.alpha = 0;
        Debug.Log($"{blessing.itemName} |{blessing.category}");
        switch (blessing.category)
        {
            case ItemStats.ItemCategory.Wealth:
                // Show interest rate
                playerScoreHandlers[id].MoneyInterestGroup.alpha = 1;
                break;
            case ItemStats.ItemCategory.Power:
                // show advantage mults
                //Debug.Log($"How");
                playerScoreHandlers[id].PowerGroup.alpha = 1;
                break;
            case ItemStats.ItemCategory.Speed:
                // show Speed dice
                //Debug.Log($"CORRECT!");
                playerScoreHandlers[id].SpeedDiceGroup.alpha = 1;
                break;
        }
    }

    private void OnPlayerWon(EntityPiece winner)
    {
        scoreCanvas.enabled = false;
        scoreCanvas.gameObject.SetActive(false);
    }

    private void OnRefreshedActiveEffects(EntityPiece player)
    {
        playerScoreHandlers[player.id].RefreshActiveEffects();
    }
}
