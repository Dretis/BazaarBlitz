using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Febucci.UI.Core;
using LitMotion;
using LitMotion.Extensions;
using System;
using UnityEngine.EventSystems;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private EntityPiece currentPlayer;
    [SerializeField] private List<EntityPiece> players;

    [Header("Colors")]
    [SerializeField] private Color healthyColor;
    [SerializeField] private Color injuredColor;
    [SerializeField] private Color dangerColor;

    [SerializeField] private Color greenStampColor;
    [SerializeField] private Color redStampColor;
    [SerializeField] private Color blueStampColor;
    [SerializeField] private Color orangeStampColor;

    [SerializeField] private List<TMP_ColorGradient> levelColorGradients;

    [Header("UI Elements")]
    [SerializeField] private Canvas scoreCanvas;
    [SerializeField] private List<TextMeshProUGUI> playerNames;
    [SerializeField] private List<TextMeshProUGUI> playerLevels;
    [SerializeField] private List<TextMeshProUGUI> playerExps;

    //[SerializeField] private List<TextMeshProUGUI> playerScores;
    //[SerializeField] private List<TypewriterCore> playerScores;
    [SerializeField] private List<TMP_Text> playerScores;

    [SerializeField] private List<TextMeshProUGUI> playerCurrentHPs;
    [SerializeField] private List<TextMeshProUGUI> playerMaxHPs;
    [SerializeField] private List<Image> playerImages;

    [Header("Active Effects / Buffs Indicator")]
    [SerializeField] private GameObject effectIndicatorPrefab;
    [SerializeField] private List<RectTransform> activeEffectsGridContainers;

    [Header("Stamp Elements")]
    [SerializeField] private List<Image> greenStamps;
    [SerializeField] private List<Image> redStamps;
    [SerializeField] private List<Image> blueStamps;
    [SerializeField] private List<Image> orangeStamps;

    [Header("Listen On Event Channels")]
    public IntEventChannelSO m_ChangeInScore;
    public IntEventChannelSO m_PlayerScoreDecreased;
    public IntEventChannelSO m_PlayerScoreIncreased;

    public PlayerEventChannelSO m_NextPlayerTurn;

    public PlayerEventChannelSO m_PassedByStamp;
    public StampEventChannelSO m_UndoPassByStamp;

    public VoidEventChannelSO m_PassByPawnShop;
    public PlayerEventChannelSO m_UndoPassByPawnShop;

    public VoidEventChannelSO m_ExitLevelUp;

    public PlayerEventChannelSO m_RefreshedActiveEffects;

    private void OnEnable()
    {
        m_ChangeInScore.OnEventRaised += UpdateScoreForPlayer;
        m_PlayerScoreDecreased.OnEventRaised += OnPlayerScoreDecreased;
        m_PlayerScoreIncreased.OnEventRaised += OnPlayerScoreIncreased;

        m_NextPlayerTurn.OnEventRaised += ChangeCurrentPlayer;
        m_NextPlayerTurn.OnEventRaised += UpdateHeldStamps;
        m_PassedByStamp.OnEventRaised += UpdateHeldStamps;
        m_UndoPassByStamp.OnEventRaised += HideObtainedStamps;

        m_PassByPawnShop.OnEventRaised += ClearHeldStamps;
        m_UndoPassByPawnShop.OnEventRaised += OnUndoPassByPawnShop;

        m_ExitLevelUp.OnEventRaised += OnExitLevelUp;

        m_RefreshedActiveEffects.OnEventRaised += OnRefreshedActiveEffects;
    }

    private void OnDisable()
    {
        m_ChangeInScore.OnEventRaised -= UpdateScoreForPlayer;
        m_PlayerScoreDecreased.OnEventRaised -= OnPlayerScoreDecreased;
        m_PlayerScoreIncreased.OnEventRaised -= OnPlayerScoreIncreased;

        m_NextPlayerTurn.OnEventRaised -= ChangeCurrentPlayer;
        m_NextPlayerTurn.OnEventRaised -= UpdateHeldStamps;
        m_PassedByStamp.OnEventRaised -= UpdateHeldStamps;
        m_UndoPassByStamp.OnEventRaised -= HideObtainedStamps;

        m_PassByPawnShop.OnEventRaised -= ClearHeldStamps;
        m_UndoPassByPawnShop.OnEventRaised -= OnUndoPassByPawnShop;

        m_ExitLevelUp.OnEventRaised -= OnExitLevelUp;

        m_RefreshedActiveEffects.OnEventRaised -= OnRefreshedActiveEffects;
    }

    void Start()
    {
        // Initial Setup
        for (int i = 0; i < players.Count; i++)
        {
            // Set name
            playerNames[i].text = "" + players[i].entityName; 

            // Set Color of Baggie in score
            playerImages[i].color = players[i].playerColor - new Color32(0, 0, 0, 0); // minus transparency

            UpdateScoreForPlayer(i);
        }

        for (int i = 0; i < players.Count; i++)
        {

            greenStamps[i].color -= new Color(0, 0, 0, 0.75f);
            redStamps[i].color -= new Color(0, 0, 0, 0.75f);
            blueStamps[i].color -= new Color(0, 0, 0, 0.75f);
            orangeStamps[i].color -= new Color(0, 0, 0, 0.75f);
            //greenStamps[i].enabled = false;
            //redStamps[i].enabled = false;
            //blueStamps[i].enabled = false;
            //orangeStamps[i].enabled = false;
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
    }

    private void UpdateScoreForPlayer(int id)
    {
        // Debug.Log("updating player {id} score");
        // Update specific player score on the scoreboard based on their ID.
        UpdateLevelForPlayer(id);

        UpdateMoneyForPlayer(id);

        UpdateHealthForPlayer(id);
    }

    private void SetMoneyForPlayer(int id)
    {
        playerScores[id].text = players[id].heldPoints.ToString();
    }

    private void UpdateLevelForPlayer(int id)
    {
        //levelColorGradients
        var lvl = players[id].RenownLevel;

        if (lvl >= 10)
        {
            playerLevels[id].colorGradientPreset = levelColorGradients[levelColorGradients.Count - 1];
        }
        else
        {
            playerLevels[id].colorGradientPreset = levelColorGradients[lvl - 1];
        }

        playerLevels[id].text = "*\n" + players[id].RenownLevel;
        playerExps[id].text = "[" + (int)players[id].ReputationPoints + "/" + (int)players[id].levelThreshold + "]";
    }

    private void UpdateMoneyForPlayer(int id)
    {
        var currentVisualPoints = int.Parse(playerScores[id].text);

        if(currentVisualPoints == players[id].heldPoints)
        {
            return;
        }

        var handle = LMotion.Create(currentVisualPoints, players[id].heldPoints, 1f)
                        .BindToText(playerScores[id]);
        /*
        if (players[id].heldPoints < 0)
        {

            // Red numbers when negative balance
            playerScores[id].ShowText($"<sprite=\"Coin Icon\" index=0> <color=red> {players[id].heldPoints}</color>");
        }
        else
        {
           playerScores[id].ShowText($"<sprite=\"Coin Icon\" index=0> {players[id].heldPoints}");
        }
        */
    }

    private void UpdateHealthForPlayer(int id)
    {
        playerMaxHPs[id].text = $"/{players[id].maxHealth * players[id].currentStatsModifier.maxHealthMultModifier + players[id].currentStatsModifier.maxHealthFlatModifier}";

        var currentVisualHealth = int.Parse(playerCurrentHPs[id].text);

        if (currentVisualHealth == players[id].health)
        {
            return;
        }

        var handle = LMotion.Create(currentVisualHealth, players[id].health, .5f)
                        .BindToText(playerCurrentHPs[id]);

        /*
        playerCurrentHPs[id].text = $"<color=#4DCF56>HP</color> {players[id].health}<size=18>/" +
            $"{players[id].maxHealth * players[id].currentStatsModifier.maxHealthMultModifier + players[id].currentStatsModifier.maxHealthFlatModifier}</size>";
        */
    }

    private void ChangeCurrentPlayer(EntityPiece ps)
    {
        currentPlayer = ps;
    }

    private void UpdateHeldStamps(EntityPiece ps)
    {
        foreach(Stamp.StampType s in ps.stamps)
        {
            Debug.Log(s);
            switch (s)
            {
                case Stamp.StampType.Green:
                    //greenStamps[ps.id].enabled = true;
                    greenStamps[ps.id].color = greenStampColor;
                    break;
                case Stamp.StampType.Red:
                    //redStamps[ps.id].enabled = true;
                    redStamps[ps.id].color = redStampColor;
                    break;
                case Stamp.StampType.Blue:
                    //blueStamps[ps.id].enabled = true;
                    blueStamps[ps.id].color = blueStampColor;
                    break;
                case Stamp.StampType.Orange:
                    //orangeStamps[ps.id].enabled = true;
                    orangeStamps[ps.id].color = orangeStampColor;
                    break;
            }
        }
    }

    private void HideObtainedStamps(Stamp.StampType type)
    {
        switch (type)
        {
            case Stamp.StampType.Green:
                //greenStamps[currentPlayer.id].enabled = false;
                greenStamps[currentPlayer.id].color -= new Color(0, 0, 0, 0.75f);

                break;
            case Stamp.StampType.Red:
                //redStamps[currentPlayer.id].enabled = false;
                redStamps[currentPlayer.id].color -= new Color(0, 0, 0, 0.75f);
                break;
            case Stamp.StampType.Blue:
                //blueStamps[currentPlayer.id].enabled = false;
                blueStamps[currentPlayer.id].color -= new Color(0, 0, 0, 0.75f);
                break;
            case Stamp.StampType.Orange:
                //orangeStamps[currentPlayer.id].enabled = false;
                orangeStamps[currentPlayer.id].color -= new Color(0, 0, 0, 0.75f);
                break;
        }
    }

    private void ClearHeldStamps()
    {
        foreach(Stamp.StampType s in currentPlayer.stamps)
        {
            HideObtainedStamps(s);
        }
        /*
        greenStamps[currentPlayer.id].color -= new Color(0, 0, 0, 0.75f);
        redStamps[currentPlayer.id].color -= new Color(0, 0, 0, 0.75f);
        blueStamps[currentPlayer.id].color -= new Color(0, 0, 0, 0.75f);
        orangeStamps[currentPlayer.id].color -= new Color(0, 0, 0, 0.75f);
        */
    }

    private void OnUndoPassByPawnShop(EntityPiece ps)
    {
        ClearHeldStamps();
        UpdateHeldStamps(ps);
    }

    private void OnExitLevelUp()
    {
        Debug.Log("updating");
        UpdateScoreForPlayer(currentPlayer.id); //spagetti ass code
    }

    private void OnRefreshedActiveEffects(EntityPiece player)
    {
        Debug.Log("Refreshing Active Effect [Buff] Indicators");
        var id = player.id;
        var activeEffects = player.activeEffects;

        DestroyAllActiveEffects(id);

        Debug.Log("spawning");

        //heldItemHolders.Clear();
        var activeEffectsParentTransform = activeEffectsGridContainers[id].transform;

        foreach (var effect in activeEffects)
        {
            if (effect.originalItem.showAsEffect)
            {
                var indicator = Instantiate(effectIndicatorPrefab, activeEffectsGridContainers[id]);
                indicator.GetComponent<ActiveEffectIndicatorHandler>().UpdateEffectInfo(effect);
            }
        }
        /*
        var itemsToSpawn = playerInventory.Count;

        if (itemsToSpawn <= INVENTORY_LIMIT)
        {
            itemsToSpawn = INVENTORY_LIMIT;
        }

        ItemStats itemToSpawn;
        for (int i = 0; i < itemsToSpawn; i++)
        {
            if (i < playerInventory.Count)
            {
                itemToSpawn = playerInventory[i];
            }
            else
            {
                itemToSpawn = null;
            }

            var item = Instantiate(heldItemPrefab, activeEffectsGridContainer);
            item.GetComponent<InventorySelectionHandler>().UpdateItemInfo(itemToSpawn);
            item.GetComponent<InventorySelectionHandler>().itemIndex = i;

            heldItemHolders.Add(item);

            if (i == 0)
            {
                EventSystem.current.SetSelectedGameObject(item);
            }
        }
        */
        //throw new NotImplementedException();
    }

    private void DestroyAllActiveEffects(int id)
    {
        // Gets rid of all the held item containers in the inventory UI

        //var inventoryParentTransform = inventoryGridContainer.transform;
        var activeEffectsParentTransform = activeEffectsGridContainers[id].transform;

        for (int i = activeEffectsParentTransform.childCount - 1; i >= 0; i--)
        {
            Destroy(activeEffectsParentTransform.GetChild(i).gameObject);
        }
    }
}
