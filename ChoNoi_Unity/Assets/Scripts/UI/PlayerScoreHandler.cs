using Coffee.UIEffects;
using LitMotion;
using LitMotion.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScoreHandler : MonoBehaviour
{
    private EntityPiece assignedPlayer;
    [SerializeField] private int playerMoneyNumber;
    [SerializeField] private int playerHPNumber;
    [SerializeField] private int playerExpNumber;

    [Header("Colors")]
    [SerializeField] private Color goalMoneyColor;
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

    [Header("General UI Elements")]
    [SerializeField] private TextMeshProUGUI playerName;
    //[SerializeField] private TextMeshProUGUI playerLevel;
    //[SerializeField] private TextMeshProUGUI playerExp;

    //[SerializeField] private List<TextMeshProUGUI> playerMoneys;
    //[SerializeField] private List<TypewriterCore> playerMoneys;
    [Space]
    [SerializeField] private TMP_Text playerMoney;
    
    [SerializeField] private TextMeshProUGUI playerCurrentHP;
    [SerializeField] private TextMeshProUGUI playerMaxHP;
    [Space]
    [SerializeField] private TextMeshProUGUI playerLevel;
    [SerializeField] private TextMeshProUGUI playerExp;
    [SerializeField] private TextMeshProUGUI playerNextExp;
    [Space]
    //[SerializeField] private List<Image> playerImages;
    [SerializeField] private PlayerPaletteLoader playerPaletteLoader;

    [Header("Stamp Elements")]
    [SerializeField] private Image greenStamp;
    [SerializeField] private Image redStamp;
    [SerializeField] private Image blueStamp;
    [SerializeField] private Image orangeStamp;

    [Header("\"More Info\" UI Elements")]
    [SerializeField] private GameObject playerInfoDiceHolder;
    [SerializeField] private List<TextMeshProUGUI> playerInfoDiceNumber = new List<TextMeshProUGUI>();

    [SerializeField] private GameObject playerInfoInvHolder;
    [SerializeField] private List<Image> playerInfoInvItems = new List<Image>();

    [Header("\"More Info\" UI Talent Elements")]
    [SerializeField] private CanvasGroup unlockTalentGroup;

    [SerializeField] private CanvasGroup moneyInterestGroup;
    [SerializeField] private TextMeshProUGUI moneyInterestText;

    [SerializeField] private CanvasGroup powerGroup;
    [SerializeField] private CanvasGroup speedDiceGroup;
    [SerializeField] private List<TextMeshProUGUI> speedDiceNumber = new List<TextMeshProUGUI>();

    public CanvasGroup UnlockTalentGroup => unlockTalentGroup;
    public CanvasGroup MoneyInterestGroup => moneyInterestGroup;
    public CanvasGroup PowerGroup => powerGroup;
    public CanvasGroup SpeedDiceGroup => speedDiceGroup;

    [Header("Active Effects / Buffs Indicator")]
    [SerializeField] private GameObject effectIndicatorPrefab;
    [SerializeField] private RectTransform activeEffectsGridContainer;

    [Header("UI Effects")]
    [SerializeField] private UIEffect currentPlayerEffect;
    [SerializeField] private UIEffect moneyShineEffect;

    [Header("Broadcast On Event Channels")]
    public PlayerEventChannelSO m_CheerForPlayer;

    public void AssignPlayerToScore(EntityPiece p)
    {
        assignedPlayer = p;
    }

    public void InitialScoreSetup()
    {
        // Set name
        //playerName.text = "" + assignedPlayer.entityName;
        SetPlayerName(assignedPlayer.entityName);

        // Set Color of Baggie in score
        //playerImage.color = assignedPlayer.playerColor - new Color32(0, 0, 0, 0); // minus transparency

        var palette = assignedPlayer.GetComponent<PlayerPaletteLoader>().GetInspectorPalette();
        playerPaletteLoader.SetInspectorPalette(palette);

        playerMoneyNumber = assignedPlayer.heldPoints;
        playerHPNumber = assignedPlayer.health;

        //Debug.Log($"playerInfoDiceNumbers[{i}] = {playerInfoDiceHolder[i]}");
        /*
        playerInfoDiceNumber.Add(null);
        var tempDiceList = new List<TextMeshProUGUI>();

        foreach (Transform child in playerInfoDiceHolder.transform)
        {
            tempDiceList.Add(child.gameObject.GetComponentInChildren<TextMeshProUGUI>());
        }
        playerInfoDiceNumber = tempDiceList;
        */
        playerInfoInvItems.Add(null);
        var tempInvList = new List<Image>();
        foreach (Transform child in playerInfoInvHolder.transform)
        {
            tempInvList.Add(child.GetComponentInChildren<Image>());
        }
        playerInfoInvItems = tempInvList;

        UpdateScore();

        SetMoney(assignedPlayer.heldPoints);
        playerMoney.GetComponent<UIEffect>().enabled = false;

        greenStamp.color -= new Color(0, 0, 0, 0.75f);
        redStamp.color -= new Color(0, 0, 0, 0.75f);
        blueStamp.color -= new Color(0, 0, 0, 0.75f);
        orangeStamp.color -= new Color(0, 0, 0, 0.75f);
        //greenStamp.enabled = false;
        //redStamp.enabled = false;
        //blueStamp.enabled = false;
        //orangeStamp.enabled = false;
        unlockTalentGroup.alpha = 1;
        moneyInterestGroup.alpha = 0;
        powerGroup.alpha = 0;
        speedDiceGroup.alpha = 0;
    }

    public void DisablePlayerScore()
    {
        Debug.Log($"DisablePlayerScore() | Disabling/hiding this {name}");
        gameObject.SetActive(false);
        currentPlayerEffect.enabled = false;
        this.enabled = false;
    }

    public void UpdateScore()
    {
        // Update specific player score on the scoreboard based on their ID.
        UpdateLevel();

        UpdateMoney();

        UpdateHealth();

        UpdateInfoDiceNumbers();

        UpdateInfoInvItems();

        UpdateMoneyInterest(); // maybe only if you have interest
    }

    public void SetPlayerName(string newName)
    {
        playerName.text = newName;
    }

    public void SetMoney(int heldMoney)
    {
        playerMoney.text = heldMoney.ToString();
        if(heldMoney < 0)
        {
            playerMoney.color = negativeMoneyColor;
        }
        //playerMoney.GetComponent<UIEffect>().enabled = false;
    }

    public void UpdateMoney()
    {
        if (playerMoneyNumber == assignedPlayer.heldPoints)
        {
            //Debug.Log($"!! Money is the same.");
            return;
        }

        var moneyShine = playerMoney.GetComponent<UIEffect>();

        if (moneyShine.enabled && assignedPlayer.heldPoints < GameplayTest.instance.currentRuleset.pointGoal)
            playerMoney.GetComponent<UIEffect>().enabled = false;

        if (assignedPlayer.heldPoints >= GameplayTest.instance.currentRuleset.pointGoal)
        {
            playerMoney.color = goalMoneyColor;
            playerMoney.GetComponent<UIEffect>().enabled = true;
        }
        else if (assignedPlayer.heldPoints < 0)
            playerMoney.color = negativeMoneyColor;
        else
            playerMoney.color = Color.white;

        if (playerMoneyNumber < assignedPlayer.heldPoints)
        {
            m_CheerForPlayer.RaiseEvent(assignedPlayer);
        }

        var handle = LMotion.Create(playerMoneyNumber, assignedPlayer.heldPoints, 1f)
                        .BindToText(playerMoney);

        playerMoneyNumber = assignedPlayer.heldPoints;
    }

    public void UpdateHealth()
    {
        playerMaxHP.text = $"/{assignedPlayer.maxHealth * assignedPlayer.currentStatsModifier.maxHealthMultModifier + assignedPlayer.currentStatsModifier.maxHealthFlatModifier}";

        if (playerHPNumber == assignedPlayer.health)
        {
            //Debug.Log($"!! Health is the same.");
            return;
        }
        if (playerHPNumber > assignedPlayer.health)
        {
            float damageTaken = playerHPNumber - assignedPlayer.health;
            float shakeDuration = damageTaken / (float)assignedPlayer.maxHealth;

            ShakePlayerSprite(shakeDuration);
        }

        var handle = LMotion.Create(playerHPNumber, assignedPlayer.health, .5f)
                        //.WithEase(Ease.OutSine)
                        .BindToText(playerCurrentHP);

        playerHPNumber = assignedPlayer.health;
    }

    public void UpdateExp()
    {
        //playerExp.text = "[" + (int)assignedPlayer.ReputationPoints + "/" + (int)assignedPlayer.levelThreshold + "]";
        playerNextExp.text = $"/{(int)assignedPlayer.levelThreshold}";

        if (playerExpNumber == assignedPlayer.ReputationPoints)
        {
            //Debug.Log($"!! Health is the same.");
            return;
        }

        var handle = LMotion.Create(playerExpNumber, (int)assignedPlayer.ReputationPoints, 1f)
                        .WithEase(Ease.OutCubic)
                        .BindToText(playerExp);

        playerExpNumber = (int)assignedPlayer.ReputationPoints;
    }

    public void UpdateLevel()
    {
        var lvl = assignedPlayer.RenownLevel;

        if (lvl >= 10)
        {
            playerLevel.colorGradientPreset = levelColorGradients[levelColorGradients.Count - 1];
        }
        else
        {
            playerLevel.colorGradientPreset = levelColorGradients[lvl - 1];
        }

        playerLevel.text = "*\n" + assignedPlayer.RenownLevel;
        UpdateExp();
        //playerExp.text = "[" + (int)assignedPlayer.ReputationPoints + "/" + (int)assignedPlayer.levelThreshold + "]";
    }


    public void UpdateHeldStamps()
    {
        var notObtainColor = Color.white - new Color(0, 0, 0, 0.75f);
        //Debug.Log($"assignedPlayer = {assignedPlayer}");
        if (assignedPlayer.stamps.Contains(Stamp.StampType.Green)) greenStamp.color = Color.white;
        else greenStamp.color = notObtainColor;

        if (assignedPlayer.stamps.Contains(Stamp.StampType.Red)) redStamp.color = Color.white;
        else redStamp.color = notObtainColor;

        if (assignedPlayer.stamps.Contains(Stamp.StampType.Blue)) blueStamp.color = Color.white;
        else blueStamp.color = notObtainColor;

        if (assignedPlayer.stamps.Contains(Stamp.StampType.Orange)) orangeStamp.color = Color.white;
        else orangeStamp.color = notObtainColor;
        /*
        foreach (Stamp.StampType s in assignedPlayer.stamps)
        {
            Debug.Log(s);
            switch (s)
            {
                case Stamp.StampType.Green:
                    //greenStamp.enabled = true;
                    //greenStamp.color = greenStampColor;
                    greenStamp.color = Color.white;
                    break;
                case Stamp.StampType.Red:
                    //redStamp.enabled = true;
                    //redStamp.color = redStampColor;
                    redStamp.color = Color.white;
                    break;
                case Stamp.StampType.Blue:
                    //blueStamp.enabled = true;
                    //blueStamp.color = blueStampColor;
                    blueStamp.color = Color.white;
                    break;
                case Stamp.StampType.Orange:
                    //orangeStamp.enabled = true;
                    //orangeStamp.color = orangeStampColor;
                    orangeStamp.color = Color.white;
                    break;
            }
        }
        */
    }

    private void HideObtainedStamps(Stamp.StampType type)
    {
        switch (type)
        {
            case Stamp.StampType.Green:
                //greenStamp.enabled = false;
                greenStamp.color -= new Color(0, 0, 0, 0.75f);

                break;
            case Stamp.StampType.Red:
                //redStamp.enabled = false;
                redStamp.color -= new Color(0, 0, 0, 0.75f);
                break;
            case Stamp.StampType.Blue:
                //blueStamp.enabled = false;
                blueStamp.color -= new Color(0, 0, 0, 0.75f);
                break;
            case Stamp.StampType.Orange:
                //orangeStamp.enabled = false;
                orangeStamp.color -= new Color(0, 0, 0, 0.75f);
                break;
        }
    }

    public void ClearHeldStamps()
    {
        foreach (Stamp.StampType s in assignedPlayer.stamps)
        {
            HideObtainedStamps(s);
        }
    }

    // "More Info" stuffs
    public void UpdateInfoDiceNumbers()
    {
        UpdateDiceStat(Action.WeaponTypes.Melee);
        UpdateDiceStat(Action.WeaponTypes.Gun);
        UpdateDiceStat(Action.WeaponTypes.Magic);
        UpdateDiceStat(Action.WeaponTypes.Speed);
    }

    public void UpdateDiceStat(Action.WeaponTypes type)
    {
        var player = assignedPlayer;
        var ti = (int)type; // type index
        float statDieFlatMod = 0;
        float statDieMultMod = 1;

        if (type != Action.WeaponTypes.Speed)
        {
            statDieFlatMod = player.currentStatsModifier.dieModifiers[ti].finalResultFlatModifier;
            statDieMultMod = player.currentStatsModifier.dieModifiers[ti].finalResultMultModifier;
        }

        var diceGradient = actionTypeGradients[ti];

        // Buffed gradient
        if (!(statDieFlatMod == 0 && statDieMultMod == 1)) diceGradient = actionTypeGradients[4];

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
            case Action.WeaponTypes.Speed:
                die = player.spdDie;
                start = 18;
                end = 24;
                break;
        }

        var faceIndex = 0;
        int finalFaceValue;

        for (int i = start; i < end; i++)
        {
            finalFaceValue = (int)((die[faceIndex] * statDieMultMod) + statDieFlatMod);

            playerInfoDiceNumber[i].colorGradientPreset = diceGradient;
            playerInfoDiceNumber[i].text = $"{finalFaceValue}";

            faceIndex++;
        }

    }
    public void UpdateInfoInvItems()
    {
        var player = assignedPlayer;
        Sprite playerItemSprite;

        for (int i = 0; i < player.inventoryLimit; i++)
        {
            //Debug.Log($"updating glance inventory {i}");
            if (i < player.inventory.Count && player.inventory[i] != null)
            {
                playerItemSprite = player.inventory[i].itemSprite;

                playerInfoInvItems[i].sprite = playerItemSprite;
                playerInfoInvItems[i].enabled = true;
            }
            else
                playerInfoInvItems[i].enabled = false;
        }
    }
    
    public void UpdateMoneyInterest()
    {
        var ratePercentage = assignedPlayer.currentStatsModifier.interestRate * 100;
        int interest = (int)(assignedPlayer.currentStatsModifier.interestRate * assignedPlayer.storestockTotal);

        // change this to work with localized strings
        moneyInterestText.text = $"<color=#FFF5C6>Storestock Total: </color=><b>{assignedPlayer.storestockTotal}<sprite=\"Coin Icon\" index=0></b>\r" +
                                 $"\n<color=#FFF5C6>{ratePercentage}% Interest: </color=> <b>+{interest}<sprite=\"Coin Icon\" index=0></b>";
    }

    public void RefreshActiveEffects()
    {
        var activeEffects = assignedPlayer.activeEffects;

        DestroyAllActiveEffects();

        //Debug.Log("spawning");

        //heldItemHolders.Clear();
        var activeEffectsParentTransform = activeEffectsGridContainer.transform;

        foreach (var effect in activeEffects)
        {
            if (effect.originalItem.showAsEffect)
            {
                var indicator = Instantiate(effectIndicatorPrefab, activeEffectsGridContainer);
                indicator.GetComponent<ActiveEffectIndicatorHandler>().UpdateEffectInfo(effect);
            }
        }
    }

    private void DestroyAllActiveEffects()
    {
        // Gets rid of all the held item containers in the inventory UI

        //var inventoryParentTransform = inventoryGridContainer.transform;
        var activeEffectsParentTransform = activeEffectsGridContainer.transform;

        for (int i = activeEffectsParentTransform.childCount - 1; i >= 0; i--)
        {
            Destroy(activeEffectsParentTransform.GetChild(i).gameObject);
        }
    }

    public void ToggleCurrentPlayerEffect(bool isOn)
    {
        currentPlayerEffect.enabled = isOn;
    }

    public void ShakePlayerSprite(float duration)
    {
        var spriteTransform = playerPaletteLoader.gameObject.GetComponent<Transform>();
        var a = LMotion.Punch.Create(spriteTransform.localPosition.y, .1f, duration)
            //.WithFrequency(2)
            //.WithDampingRatio(0f)
            .BindToLocalPositionY(spriteTransform);

        var b = LMotion.Shake.Create(spriteTransform.localPosition.x, .05f, duration)
            //.WithFrequency(2)
            //.WithDampingRatio(0f)
            .BindToLocalPositionX(spriteTransform);
    }
}
