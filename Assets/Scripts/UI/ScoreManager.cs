using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Febucci.UI.Core;
using LitMotion;
using LitMotion.Extensions;
using System.Linq;
using Unity.VisualScripting;

public class ScoreManager : MonoBehaviour
{
    private MotionHandle currentMotion;

    [SerializeField] private EntityPiece currentPlayer;
    [SerializeField] private List<EntityPiece> players;

    [Header("Number Holders")]
    [SerializeField] private List<int> playerScoreNumbers;
    [SerializeField] private List<int> playerHPNumbers;

    [Header("Colors")]
    [SerializeField] private Color negativeMoneyColor;

    [Space]
    [SerializeField] private Color healthyColor;
    [SerializeField] private Color injuredColor;
    [SerializeField] private Color dangerColor;

    [Space]
    [SerializeField] private Color greenStampColor;
    [SerializeField] private Color redStampColor;
    [SerializeField] private Color blueStampColor;
    [SerializeField] private Color orangeStampColor;

    [Header("Color Gradients")]
    [SerializeField] private List<TMP_ColorGradient> levelColorGradients;
    [SerializeField] private List<TMP_ColorGradient> actionTypeGradients;

    [Header("UI Elements")]
    [SerializeField] private Canvas scoreCanvas;

    [SerializeField] private RectTransform scoreContainerTransform;

    [Space]
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

    [Header("\"More Info\" UI Elements")]
    [SerializeField] private List<GameObject> playerInfoDiceHolder;
    [SerializeField] private List<List<TextMeshProUGUI>> playerInfoDiceNumbers = new List<List<TextMeshProUGUI>>();

    [SerializeField] private List<GameObject> playerInfoInvHolder;
    [SerializeField] private List<List<Image>> playerInfoInvItems = new List<List<Image>>();

    [Header("Broadcast On Event Channels")]
    public PlayerEventChannelSO m_CheerForPlayer;

    [Header("Listen On Event Channels")]
    public IntEventChannelSO m_ChangeInScore;
    public IntEventChannelSO m_PlayerScoreDecreased;
    public IntEventChannelSO m_PlayerScoreIncreased;

    public PlayerEventChannelSO m_NextPlayerTurn;

    public VoidEventChannelSO m_HoldPlayerInfo;
    public VoidEventChannelSO m_ReleasePlayerInfo;

    public PlayerEventChannelSO m_FinishStockingStore;

    public PlayerEventChannelSO m_PassedByStamp;
    public StampEventChannelSO m_UndoPassByStamp;

    public VoidEventChannelSO m_PassByPawnShop;
    public PlayerEventChannelSO m_UndoPassByPawnShop;

    public VoidEventChannelSO m_ExitLevelUp;

    public PlayerEventChannelSO m_RefreshedActiveEffects;

    public PlayerEventChannelSO m_PlayerWon;

    private void OnEnable()
    {
        m_ChangeInScore.OnEventRaised += UpdateScoreForPlayer;
        m_PlayerScoreDecreased.OnEventRaised += OnPlayerScoreDecreased;
        m_PlayerScoreIncreased.OnEventRaised += OnPlayerScoreIncreased;

        m_NextPlayerTurn.OnEventRaised += ChangeCurrentPlayer;
        m_NextPlayerTurn.OnEventRaised += UpdateHeldStamps;

        m_HoldPlayerInfo.OnEventRaised += OnHoldPlayerInfo;
        m_ReleasePlayerInfo.OnEventRaised += OnReleasePlayerInfo;

        m_FinishStockingStore.OnEventRaised += OnFinishStockingStore;

        m_PassedByStamp.OnEventRaised += UpdateHeldStamps;
        m_UndoPassByStamp.OnEventRaised += HideObtainedStamps;

        m_PassByPawnShop.OnEventRaised += ClearHeldStamps;
        m_UndoPassByPawnShop.OnEventRaised += OnUndoPassByPawnShop;

        m_ExitLevelUp.OnEventRaised += OnExitLevelUp;

        m_RefreshedActiveEffects.OnEventRaised += OnRefreshedActiveEffects;

        m_PlayerWon.OnEventRaised += OnPlayerWon;
    }

    private void OnDisable()
    {
        m_ChangeInScore.OnEventRaised -= UpdateScoreForPlayer;
        m_PlayerScoreDecreased.OnEventRaised -= OnPlayerScoreDecreased;
        m_PlayerScoreIncreased.OnEventRaised -= OnPlayerScoreIncreased;

        m_NextPlayerTurn.OnEventRaised -= ChangeCurrentPlayer;
        m_NextPlayerTurn.OnEventRaised -= UpdateHeldStamps;

        m_HoldPlayerInfo.OnEventRaised -= OnHoldPlayerInfo;
        m_ReleasePlayerInfo.OnEventRaised -= OnReleasePlayerInfo;

        m_FinishStockingStore.OnEventRaised -= OnFinishStockingStore;

        m_PassedByStamp.OnEventRaised -= UpdateHeldStamps;
        m_UndoPassByStamp.OnEventRaised -= HideObtainedStamps;

        m_PassByPawnShop.OnEventRaised -= ClearHeldStamps;
        m_UndoPassByPawnShop.OnEventRaised -= OnUndoPassByPawnShop;

        m_ExitLevelUp.OnEventRaised -= OnExitLevelUp;

        m_RefreshedActiveEffects.OnEventRaised -= OnRefreshedActiveEffects;

        m_PlayerWon.OnEventRaised -= OnPlayerWon;
    }

    void Start()
    {
        //playerInfoDiceNumbers.Add("1");

        // Initial Setup
        for (int i = 0; i < players.Count; i++)
        {
            // Set name
            playerNames[i].text = "" + players[i].entityName; 

            // Set Color of Baggie in score
            playerImages[i].color = players[i].playerColor - new Color32(0, 0, 0, 0); // minus transparency

            playerScoreNumbers[i] = players[i].heldPoints;
            playerHPNumbers[i] = players[i].health;

            //Debug.Log($"playerInfoDiceNumbers[{i}] = {playerInfoDiceHolder[i]}");
            
            playerInfoDiceNumbers.Add(null);
            var tempDiceList = new List<TextMeshProUGUI>();

            foreach (Transform child in playerInfoDiceHolder[i].transform)
            {
                tempDiceList.Add(child.gameObject.GetComponentInChildren<TextMeshProUGUI>());
            }
            playerInfoDiceNumbers[i] = tempDiceList;

            playerInfoInvItems.Add(null);
            var tempInvList = new List<Image>();
            foreach (Transform child in playerInfoInvHolder[i].transform)
            {
                tempInvList.Add(child.GetComponentInChildren<Image>());
            }
            playerInfoInvItems[i] = tempInvList;

            UpdateScoreForPlayer(i);

            SetMoneyForPlayer(i);
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
        // Update specific player score on the scoreboard based on their ID.
        UpdateLevelForPlayer(id);

        UpdateMoneyForPlayer(id);

        UpdateHealthForPlayer(id);

        UpdateInfoDiceNumbersForPlayer(id);

        UpdateInfoInvItemsForPlayer(id);
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
        if (playerScoreNumbers[id] == players[id].heldPoints)
        {
            //Debug.Log($"!! Money is the same.");
            return;
        }

        if (players[id].heldPoints < 0)
            playerScores[id].color = negativeMoneyColor;
        else
            playerScores[id].color = Color.white;

        if (playerScoreNumbers[id] < players[id].heldPoints)
        {
            m_CheerForPlayer.RaiseEvent(players[id]);
        }

        var handle = LMotion.Create(playerScoreNumbers[id], players[id].heldPoints, 1f)
                        .BindToText(playerScores[id]);

        playerScoreNumbers[id] = players[id].heldPoints;

        /*
        var currentVisualPoints = int.Parse(playerScores[id].text);
        Debug.Log($"HELP!! Player[{id}] CurrentVisualPoints: {currentVisualPoints} | HeldPoints: {players[id].heldPoints}");

        if(currentVisualPoints == players[id].heldPoints)
        {
            Debug.Log($"!! Money is the same.");
            return;
        }

        if(currentVisualPoints < players[id].heldPoints)
        {
            m_CheerForPlayer.RaiseEvent(players[id]);
        }

        var handle = LMotion.Create(currentVisualPoints, players[id].heldPoints, 1f)
                        .BindToText(playerScores[id]);
        */
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

        if (playerHPNumbers[id] == players[id].health)
        {
            //Debug.Log($"!! Health is the same.");
            return;
        }

        var handle = LMotion.Create(playerHPNumbers[id], players[id].health, .5f)
                        .BindToText(playerCurrentHPs[id]);

        playerHPNumbers[id] = players[id].health;

        /*
        var currentVisualHealth = int.Parse(playerCurrentHPs[id].text);
        Debug.Log($"HELP!! Player[{id}] currentVisualHealth: {currentVisualHealth} | Health: {players[id].health}");

        if (currentVisualHealth == players[id].health)
        {
            Debug.Log($"!! Health is the same.");
            return;
        }

        var handle = LMotion.Create(currentVisualHealth, players[id].health, .5f)
                        .BindToText(playerCurrentHPs[id]);
        */

        /*
        playerCurrentHPs[id].text = $"<color=#4DCF56>HP</color> {players[id].health}<size=18>/" +
            $"{players[id].maxHealth * players[id].currentStatsModifier.maxHealthMultModifier + players[id].currentStatsModifier.maxHealthFlatModifier}</size>";
        */
    }

    // "More Info" stuffs
    private void UpdateInfoDiceNumbersForPlayer(int id)
    {
        UpdateDiceStat(id, Action.WeaponTypes.Melee);
        UpdateDiceStat(id, Action.WeaponTypes.Gun);
        UpdateDiceStat(id, Action.WeaponTypes.Magic);
    }

    private void UpdateDiceStat(int id, Action.WeaponTypes type)
    {
        var player = players[id];
        var ti = (int)type; // type index

        var statDieFlatMod = player.currentStatsModifier.dieModifiers[ti].finalResultFlatModifier;
        var statDieMultMod = player.currentStatsModifier.dieModifiers[ti].finalResultMultModifier;
        var diceGradient = actionTypeGradients[ti];

        // Buffed gradient
        if (!(statDieFlatMod == 0 && statDieMultMod == 1)) diceGradient = actionTypeGradients[3];

        DieConfig die = player.entityStats.dieConfigs[(int)type];

        var start = 0;
        var end = 0;

        switch (type)
        {
            case Action.WeaponTypes.Melee:
                die = player.strDie;
                start = 0;
                end = 6;
                break;
            case Action.WeaponTypes.Gun:
                die = player.dexDie;
                start = 6;
                end = 12;
                break;
            case Action.WeaponTypes.Magic:
                die = player.intDie;
                start = 12;
                end = 18;
                break;
        }

        var faceIndex = 0;
        int finalFaceValue;

        for (int i = start; i < end; i++)
        {
            finalFaceValue = (int) ((die[faceIndex] * statDieMultMod) + statDieFlatMod);

            playerInfoDiceNumbers[id][i].colorGradientPreset = diceGradient;
            playerInfoDiceNumbers[id][i].text = $"{finalFaceValue}";

            faceIndex++;
        }

    }

    private void UpdateInfoInvItemsForPlayer(int id)
    {
        var player = players[id];
        Sprite playerItemSprite;

        for (int i = 0; i < player.inventoryLimit; i++)
        {
            //Debug.Log($"updating glance inventory {i}");
            if(i < player.inventory.Count && player.inventory[i] != null)
            {
                playerItemSprite = player.inventory[i].itemSprite;

                playerInfoInvItems[id][i].sprite = playerItemSprite;
                playerInfoInvItems[id][i].enabled = true;
            }
            else
                playerInfoInvItems[id][i].enabled = false;
        }
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

    private void OnHoldPlayerInfo()
    {
        // Lift up scoreboard to show additional info
        currentMotion = LMotion.Create(scoreContainerTransform.anchoredPosition, new Vector2(0, 150), 0.25f)
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
        UpdateInfoInvItemsForPlayer(ps.id);
    }

    private void OnExitLevelUp()
    {
        Debug.Log("updating");
        UpdateScoreForPlayer(currentPlayer.id); //spagetti ass code
    }

    private void OnPlayerWon(EntityPiece winner)
    {
        scoreCanvas.enabled = false;
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
