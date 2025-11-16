using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Febucci.UI.Core;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEngine.UI;

public class UILevelUpManager : MonoBehaviour
{
    private EntityPiece currentPlayer;
    [SerializeField] private CanvasGroup levelUpGroup;
    [SerializeField] private TypewriterCore levelIndicator;
    [SerializeField] private Image levelIndicatorDiamond;
    [SerializeField] private Image tooltipTextbox;
    [SerializeField] private TextMeshProUGUI remainingSP;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private GameObject diceStats;
    [SerializeField] private List<DiceStatSelectionHandler> playerDiceNumbers = new List<DiceStatSelectionHandler>();
    [SerializeField] private List<TMP_ColorGradient> levelColorGradients;

    private int[] costArray = { 0, 1, 1, 2, 2, 2, 3, 3, 3, 4, 5, 999 };

    [Header("Broadcast on Event Channels")]
    public IntEventChannelSO m_UpdatePlayerScore;
    public PlayerEventChannelSO m_EnterLevelUp; // also listening to this
    public VoidEventChannelSO m_ExitLevelUp; // also listening to this
    public VoidEventChannelSO m_AugmentedDieFaceValue;
    public VoidEventChannelSO m_FailAugmentDieFaceValue;

    [Header("Listen on Event Channels")]
    public WeaponTypeIntEventChannel m_TryAugmentDieFaceValue; // lvl up

    private void OnEnable()
    {
        m_EnterLevelUp.OnEventRaised += OnEnterLevelUp;
        m_ExitLevelUp.OnEventRaised += OnExitLevelUp;
        m_TryAugmentDieFaceValue.OnEventRaised += OnTryAugmentDieFaceValue;
    }

    private void OnDisable()
    {
        m_EnterLevelUp.OnEventRaised -= OnEnterLevelUp;
        m_ExitLevelUp.OnEventRaised -= OnExitLevelUp;
        m_TryAugmentDieFaceValue.OnEventRaised -= OnTryAugmentDieFaceValue;
    }

    private void Start()
    {
        levelUpGroup.alpha = 0f;
        levelUpGroup.interactable = false;
    }

    public void UpdatePlayerDiceStats(EntityPiece entity, GameObject diceStats)
    {
        // Visually updates the dice stats ui based on the entity and side
        playerDiceNumbers.Clear();

        // Goes through the diceStats UI List and finds the text components
        foreach (Transform child in diceStats.transform)
        {
            playerDiceNumbers.Add(child.GetComponentInChildren<DiceStatSelectionHandler>());

            //EventSystem.current.SetSelectedGameObject(child.gameObject);
        }

        UpdatePlayerDiceStatsInLevelUp(entity);

        EventSystem.current.SetSelectedGameObject(playerDiceNumbers[0].gameObject);
    }

    public void UpdatePlayerDiceStatsInLevelUp(EntityPiece entity)
    {
        // like the other function but it doesn't reset the button position
        var faceIndex = 0;

        for (int i = 0; i < 6; i++)
        {
            playerDiceNumbers[i].SetDieFaceValue(entity.strDie[faceIndex]);
            faceIndex++;

        }

        faceIndex = 0;

        for (int i = 6; i < 12; i++)
        {
            playerDiceNumbers[i].SetDieFaceValue(entity.dexDie[faceIndex]);
            faceIndex++;
        }

        faceIndex = 0;

        for (int i = 12; i < 18; i++)
        {
            playerDiceNumbers[i].SetDieFaceValue(entity.intDie[faceIndex]);
            faceIndex++;
        }
    }

    private void OnEnterLevelUp(EntityPiece p)
    {
        currentPlayer = p;
        p.unspentLevelUpPoints += 5;
        p.maxHealth += 10;
        p.health += 10;
        p.RenownLevel += 1;
        p.levelThreshold = (p.RenownLevel * 100) * (Mathf.Pow(1.15f, p.RenownLevel - 1));

        UpdatePlayerDiceStats(p, diceStats);

        var lvl = p.RenownLevel;

        if (lvl >= 10)
        {
            levelIndicator.GetComponent<TextMeshProUGUI>().colorGradientPreset = levelColorGradients[levelColorGradients.Count - 1];
        }
        else
        {
            levelIndicator.GetComponent<TextMeshProUGUI>().colorGradientPreset = levelColorGradients[lvl - 1];
        }

        levelUpGroup.alpha = 1f;
        levelUpGroup.interactable = true;

        levelIndicator.ShowText($"*\n{p.RenownLevel}");
        remainingSP.text = $"Remaining SP: {p.unspentLevelUpPoints}";

        levelIndicatorDiamond.color = p.playerColor - new Color32 (0,0,0,25);
        tooltipTextbox.color = p.playerColor - new Color32 (0,0,0, 100);

        m_UpdatePlayerScore.RaiseEvent(p.id);
    }
    private void OnExitLevelUp()
    {
        EventSystem.current.SetSelectedGameObject(null);
        levelUpGroup.alpha = 0f;
        levelUpGroup.interactable = false;
    }

    private void OnTryAugmentDieFaceValue(Action.WeaponTypes diceType, int diceIndex)
    {
        Debug.Log($"checking if can upgradfe | {diceType} Dice at id{diceIndex} is []");
        // Debug.Log($"cpsts {costArray[diceIndex]} SP, player has {currentPlayer.unspentLevelUpPoints}");
        // Check if current player has enough SP to augment this die face

        var selectedDie = currentPlayer.strDie[diceIndex];

        switch (diceType)
        {
            case Action.WeaponTypes.Melee:
                selectedDie = currentPlayer.strDie[diceIndex];
                break;

            case Action.WeaponTypes.Gun:
                selectedDie = currentPlayer.dexDie[diceIndex];
                break;

            case Action.WeaponTypes.Magic:
                selectedDie = currentPlayer.intDie[diceIndex];
                break;
        }

        Debug.Log($"Selected Die [{selectedDie}]");

        if (costArray[selectedDie] <= currentPlayer.unspentLevelUpPoints)
        {
            Debug.Log("it can!!!");
            currentPlayer.unspentLevelUpPoints -= costArray[selectedDie];

            switch (diceType)
            {
                case Action.WeaponTypes.Melee:
                    currentPlayer.strDie[diceIndex]++;
                    break;

                case Action.WeaponTypes.Gun:
                    currentPlayer.dexDie[diceIndex]++;
                    break;

                case Action.WeaponTypes.Magic:
                    currentPlayer.intDie[diceIndex]++;
                    break;
            }

            // broadcast that it did in fact upgrade
            m_AugmentedDieFaceValue.RaiseEvent();
            UpdatePlayerDiceStatsInLevelUp(currentPlayer);
            remainingSP.text = $"Remaining SP: {currentPlayer.unspentLevelUpPoints}";
            tooltipText.text = $"Increase  <sprite={(int)diceType}> <color=white>[{selectedDie+1}]</color> to <color=white>[{selectedDie + 2}]</color> " +
                $"for <color=#8AEFFF>{GameplayTest.instance.costArray[selectedDie+1]} SP</color>.";  

            if (currentPlayer.unspentLevelUpPoints <= 0)
            {
                m_ExitLevelUp.RaiseEvent();
            }
        }
        else
        {
            // Can't augment, fail L bozo
            Debug.Log("failed to augment wtf how");
            m_FailAugmentDieFaceValue.RaiseEvent();
        }
    }
}
