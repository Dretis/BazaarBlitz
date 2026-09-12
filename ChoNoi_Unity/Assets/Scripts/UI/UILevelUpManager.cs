using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Febucci.UI.Core;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEngine.UI;
using LitMotion;
using LitMotion.Extensions;

public class UILevelUpManager : MonoBehaviour
{
    private EntityPiece currentPlayer;
    [SerializeField] private CanvasGroup statAllocationGroup;
    [SerializeField] private RectTransform backgroundStripParent;
    [SerializeField] private RectTransform backgroundStrip;
    [SerializeField] private TypewriterCore levelIndicator;
    [SerializeField] private Image levelIndicatorDiamond;
    [Space]
    [SerializeField] private Image tooltipTextbox;
    [SerializeField] private Image diceStatsBox;
    [SerializeField] private TextMeshProUGUI remainingSP;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [Space]
    [SerializeField] private CanvasGroup diceStatsRegularGroup;
    [SerializeField] private CanvasGroup diceStatsWithSpeedGroup;
    [SerializeField] private GameObject diceStatsRegular;
    [SerializeField] private GameObject diceStatsWithSpeed;

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
    public PlayerEventChannelSO m_EnterStatAllocation;
    public VoidEventChannelSO m_ExitStatAllocation;

    public WeaponTypeIntEventChannel m_TryAugmentDieFaceValue; // lvl up
    public VoidEventChannelSO m_HoldPlayerInfo;
    public VoidEventChannelSO m_ReleasePlayerInfo;

    private void OnEnable()
    {
        //m_EnterLevelUp.OnEventRaised += OnEnterLevelUp;
        m_ExitLevelUp.OnEventRaised += OnExitLevelUp;
        m_EnterStatAllocation.OnEventRaised += OnEnterStatAllocation;
        m_ExitStatAllocation.OnEventRaised += OnExitStatAllocation;
        m_TryAugmentDieFaceValue.OnEventRaised += OnTryAugmentDieFaceValue;

        m_HoldPlayerInfo.OnEventRaised += OnHoldPlayerInfo;
        m_ReleasePlayerInfo.OnEventRaised += OnReleasePlayerInfo;
    }

    private void OnDisable()
    {
        //m_EnterLevelUp.OnEventRaised -= OnEnterLevelUp;
        m_ExitLevelUp.OnEventRaised -= OnExitLevelUp;
        m_EnterStatAllocation.OnEventRaised -= OnEnterStatAllocation;
        m_ExitStatAllocation.OnEventRaised -= OnExitStatAllocation;
        m_TryAugmentDieFaceValue.OnEventRaised -= OnTryAugmentDieFaceValue;

        m_HoldPlayerInfo.OnEventRaised -= OnHoldPlayerInfo;
        m_ReleasePlayerInfo.OnEventRaised -= OnReleasePlayerInfo;
    }

    private void Start()
    {
        statAllocationGroup.alpha = 0f;
        statAllocationGroup.interactable = false;
        statAllocationGroup.blocksRaycasts = false;
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

        if (currentPlayer.currentStatsModifier.canUseSpeedDie)
        {
            faceIndex = 0;
            for (int i = 18; i < 24; i++)
            {
                playerDiceNumbers[i].SetDieFaceValue(entity.spdDie[faceIndex]);
                faceIndex++;
            }
        }
    }

    private void OnEnterStatAllocation(EntityPiece p)
    {
        currentPlayer = p;
        //p.unspentLevelUpPoints += 6;
        //p.maxHealth += 5;
        //p.health += 5;
        //p.RenownLevel += 1;
        //p.levelThreshold = (p.RenownLevel * 100) * (Mathf.Pow(1.15f, p.RenownLevel - 1));
        if (p.currentStatsModifier.canUseSpeedDie)
        {
            diceStatsRegularGroup.alpha = 0;
            diceStatsRegularGroup.interactable = false;
            diceStatsRegularGroup.blocksRaycasts = false;

            diceStatsWithSpeedGroup.alpha = 1;
            diceStatsWithSpeedGroup.interactable = true;
            diceStatsWithSpeedGroup.blocksRaycasts = true;

            //diceStatsWithSpeed.SetActive(true);
            //diceStatsRegular.SetActive(false);
            UpdatePlayerDiceStats(p, diceStatsWithSpeed);
        }
        else
        {
            diceStatsRegularGroup.alpha = 1;
            diceStatsRegularGroup.interactable = true;
            diceStatsRegularGroup.blocksRaycasts = true;

            diceStatsWithSpeedGroup.alpha = 0;
            diceStatsWithSpeedGroup.interactable = false;
            diceStatsWithSpeedGroup.blocksRaycasts = false;

            //diceStatsWithSpeed.SetActive(false);
            //diceStatsRegular.SetActive(true);
            UpdatePlayerDiceStats(p, diceStatsRegular);
        }

        var lvl = p.RenownLevel;

        if (lvl >= 10)
        {
            levelIndicator.GetComponent<TextMeshProUGUI>().colorGradientPreset = levelColorGradients[levelColorGradients.Count - 1];
        }
        else
        {
            levelIndicator.GetComponent<TextMeshProUGUI>().colorGradientPreset = levelColorGradients[lvl - 1];
        }

        levelIndicator.ShowText($"*\n{p.RenownLevel}");
        //remainingSP.text = $"Remaining SP: {p.unspentLevelUpPoints}";
        remainingSP.text = $"<size=72>{p.unspentLevelUpPoints}</size>\nSP";

        levelIndicatorDiamond.color = p.playerColor - new Color32 (0,0,0,25);
        //diceStatsBox.color = p.playerColor - new Color32 (0,0,0, 200);
        tooltipTextbox.color = p.playerColor - new Color32 (0,0,0, 150);

        //m_UpdatePlayerScore.RaiseEvent(p.id);

        //statAllocationGroup.alpha = 1f;
        statAllocationGroup.interactable = true;
        statAllocationGroup.blocksRaycasts = true;

        var statMotion = LMotion.Create(statAllocationGroup.alpha, 1, 0.35f)
            .WithEase(Ease.OutQuad)
            .Bind(x => statAllocationGroup.alpha = x);

        var stripParentMotion = LMotion.Create(Vector3.zero, Vector3.one, .5f)
            .WithEase(Ease.OutBack)
            .BindToLocalScale(backgroundStripParent);
    }
    private void OnExitStatAllocation()
    {
        EventSystem.current.SetSelectedGameObject(null);
        //statAllocationGroup.alpha = 0f;
        statAllocationGroup.interactable = false;
        statAllocationGroup.blocksRaycasts = false;

        var statMotion = LMotion.Create(statAllocationGroup.alpha, 0, 0.25f)
            .WithEase(Ease.OutQuad)
            .Bind(x => statAllocationGroup.alpha = x);

        var stripParentMotion = LMotion.Create(Vector3.one, Vector3.zero, .4f)
            .WithEase(Ease.OutBack)
            .BindToLocalScale(backgroundStripParent);
    }

    private void OnExitLevelUp()
    {
        OnExitStatAllocation();
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
            case Action.WeaponTypes.Speed:
                selectedDie = currentPlayer.spdDie[diceIndex];
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
                case Action.WeaponTypes.Speed:
                    currentPlayer.spdDie[diceIndex]++;
                    break;
            }

            // broadcast that it did in fact upgrade
            m_AugmentedDieFaceValue.RaiseEvent();
            UpdatePlayerDiceStatsInLevelUp(currentPlayer);
            //remainingSP.text = $"Remaining SP: {currentPlayer.unspentLevelUpPoints}";
            remainingSP.text = $"<size=72>{currentPlayer.unspentLevelUpPoints}</size>\nSP";

            Debug.Log("selctedDie = " + selectedDie);
            if (selectedDie + 2 >= GameplayTest.instance.costArray.Length)
            {
                tooltipText.text = $"This <sprite={(int)diceType}> die face cannot be augmented further!";
            }
            else
            {
                tooltipText.text = $"Increase <sprite={(int)diceType}> <color=white>[{selectedDie+1}]</color> to <color=white>[{selectedDie + 2}]</color> for ";

                if (GameplayTest.instance.currentPlayer.unspentLevelUpPoints < GameplayTest.instance.costArray[selectedDie + 1])
                    tooltipText.text += $"<color=red>{GameplayTest.instance.costArray[selectedDie + 1]} SP</color>.";
                else
                    tooltipText.text += $"<color=#8AEFFF>{GameplayTest.instance.costArray[selectedDie + 1]} SP</color>.";
            }
            
            if (currentPlayer.unspentLevelUpPoints <= 0)
            {
                m_ExitLevelUp.RaiseEvent();
            }
        }
        else
        {
            // Can't augment, fail L bozo
            Debug.Log("failed to augment wtf how");
            LMotion.Shake.Create(0f, 10f, .5f)
                .WithFrequency(5)
                .BindToAnchoredPositionX(diceStatsBox.rectTransform);
            m_FailAugmentDieFaceValue.RaiseEvent();
        }
    }

    private void OnHoldPlayerInfo()
    {
        //backgroundStrip.anchoredPosition = new Vector2(0, 475);
        var stripMotion = LMotion.Create(backgroundStrip.localScale, new Vector3(1, 0.85f, 1), .2f)
            .WithEase(Ease.OutBack)
            .BindToLocalScale(backgroundStrip);
    }

    private void OnReleasePlayerInfo()
    {
        //backgroundStrip.anchoredPosition = new Vector2(0, 360);
        //backgroundStrip.localScale = Vector3.one;
        var stripMotion = LMotion.Create(backgroundStrip.localScale, Vector3.one, .2f)
            .WithEase(Ease.OutBack)
            .BindToLocalScale(backgroundStrip);
    }
}
