using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using TMPro;
using DG.Tweening;
using Febucci.UI;
using Febucci.UI.Core;

public class UICombatOverlayManager : MonoBehaviour
{
    [SerializeField] private Volume volume;
    [SerializeField] private ParticleSystem hitParticle;

    [Header("Colors")]
    [SerializeField] private Color32 attackColor;
    [SerializeField] private Color32 defendColor;
    [SerializeField] private Color32 grayoutColor;

    [SerializeField] private TMP_ColorGradient[] actionTypeGradients;
    [SerializeField] private TMP_ColorGradient[] advantageTypeGradients;


    [Header("UI Elements")]
    [SerializeField] private RectTransform vsHeader;

    [SerializeField] private CanvasGroup floatingDamage;
    [SerializeField] private TypewriterCore floatingDamageNumber;
    [SerializeField] private TypewriterCore leftDiceRoll;
    [SerializeField] private TypewriterCore rightDiceRoll;

    [Header("Results Elements")]
    [SerializeField] private CanvasGroup stalemateGroup;
    [SerializeField] private CanvasGroup resultsGroup;
    [SerializeField] private TypewriterCore victoryText;
    [SerializeField] private TypewriterCore rewardsText;

    private RectTransform victoryTextPos;
    private RectTransform rewardsTextPos;

    [SerializeField] private Vector3 p1VictoryPosition = new Vector3(300, -32, 0); //Default
    [SerializeField] private Vector3 p2VictoryPosition = new Vector3(-250, -32, 0); 

    [SerializeField] private Vector3 p1RewardsPosition = new Vector3(300, -330, 0); //Default
    [SerializeField] private Vector3 p2RewardsPosition = new Vector3(-32, -330, 0);



    [Header("Dice Information")]
    [SerializeField] private RectTransform diceInfo;

    [SerializeField] private GameObject leftDiceStat;
    [SerializeField] private GameObject rightDiceStat;

    [SerializeField] private List<TextMeshProUGUI> diceNumbers = new List<TextMeshProUGUI>();

    [Header("UI Inputs")]
    [SerializeField] private CanvasGroup leftAttackPrompt;
    [SerializeField] private CanvasGroup leftDefendPrompt;
    [SerializeField] private CanvasGroup leftSelectedAction;

    [SerializeField] private CanvasGroup rightAttackPrompt;
    [SerializeField] private CanvasGroup rightDefendPrompt;
    [SerializeField] private CanvasGroup rightSelectedAction;


    [Header("Text")]
    [SerializeField] private TextMeshProUGUI leftHealthPoints;
    [SerializeField] private TextMeshProUGUI rightHealthPoints;

    [SerializeField] private TextMeshProUGUI phaseTestPrompt; //placeholder for this build, remove later
    [SerializeField] private TextMeshProUGUI resultTestText; //placeholder for this build, remove later

    [Header("UI Positions")]
    [SerializeField] private Vector2 vsInitialPosition;
    [SerializeField] private Vector2 vsHidePosition;

    [SerializeField] private Vector2 floatingDmgLeftInitPosition;
    [SerializeField] private Vector2 floatingDmgLeftPosition;

    [SerializeField] private Vector2 floatingDmgRightInitPosition;
    [SerializeField] private Vector2 floatingDmgRightPosition;

    [SerializeField] private Vector2 diceInitialPosition;
    [SerializeField] private Vector2 diceHidePosition;
    // private RectTransform vsHeaderRectTransform;

    [Header("Listen on Event Channels")]
    //public PlayerEventChannelSO m_DecidedTurnOrder; // pass in the attacker
    public PlayerEventChannelSO m_SwapPhase; // void event

    public EntityActionPhaseEventChannelSO m_ActionSelected; // Entity, check side and phase | Either the attacker or defender picked an action
    public EntityActionEventChannelSO m_BothActionsSelected; // prep time to show what they picked, follow with the dice roll too
    public PlayerFloatActionTypeEventChannelSO m_StoreDiceRolled; // 2 floats

    public PlayerEventChannelSO m_PlayOutCombat; // play attack anim and defend anim

    public DamageEventChannelSO m_DamageTaken; //upon attack anim finishing, show floating dmg ontop of defender, play hurt anim

    public EntityItemEventChannelSO m_EntityDied; // someone's HP dropped to 0, Victory, show rewards
    
    public VoidEventChannelSO m_Stalemate; // Combat is suspended, no one died this stime
    public IntListItemListEventChannelSO m_VictoryAgainstEnemy;
    public IntListItemListEventChannelSO m_DefeatAgainstEnemy;


    private void OnEnable()
    {
        volume.weight = 0;

        resultTestText.text = "";

        leftDiceRoll.GetComponent<TextMeshProUGUI>().text = "";
        rightDiceRoll.GetComponent<TextMeshProUGUI>().text = "";

        rewardsText.GetComponent<TextMeshProUGUI>().text = "";
        resultsGroup.alpha = 0;
        stalemateGroup.alpha = 0;
        ShowDiceInfo();
        ShowVSHeader();


        //m_DecidedTurnOrder.OnEventRaised += UpdateInputPrompts;
        m_SwapPhase.OnEventRaised += SwapPhaseTransitions;

        m_ActionSelected.OnEventRaised += UpdatePhaseTextPrompt;
        m_BothActionsSelected.OnEventRaised += ShowSelectedAction;
        m_StoreDiceRolled.OnEventRaised += SetDiceActionRoll;

        //m_PlayOutCombat.OnEventRaised += HideHeaderInfo;

        m_DamageTaken.OnEventRaised += ShowFloatingDamageNumber;

        m_VictoryAgainstEnemy.OnEventRaised += VictoryResults;
        m_DefeatAgainstEnemy.OnEventRaised += DefeatResults;
        m_Stalemate.OnEventRaised += StalemateResults;

    }

    private void OnDisable()
    {
        //m_DecidedTurnOrder.OnEventRaised -= UpdateInputPrompts;
        m_SwapPhase.OnEventRaised -= SwapPhaseTransitions;

        m_ActionSelected.OnEventRaised -= UpdatePhaseTextPrompt;
        m_BothActionsSelected.OnEventRaised -= ShowSelectedAction;
        m_StoreDiceRolled.OnEventRaised -= SetDiceActionRoll;

        //m_PlayOutCombat.OnEventRaised += HideHeaderInfo;

        m_DamageTaken.OnEventRaised -= ShowFloatingDamageNumber;

        m_VictoryAgainstEnemy.OnEventRaised -= VictoryResults;
        m_DefeatAgainstEnemy.OnEventRaised -= DefeatResults;
        m_Stalemate.OnEventRaised -= StalemateResults;

    }

    // Start is called before the first frame update
    void Start()
    {
        phaseTestPrompt.color = Color.white;
        //phaseTestPrompt.text = "";
        //vsHeaderRectTransform = vsHeader.GetComponent<RectTransform>();
        vsInitialPosition = vsHeader.anchoredPosition;
        diceInitialPosition = diceInfo.anchoredPosition;


        vsHeader.anchoredPosition = vsHidePosition;
        diceInfo.anchoredPosition = diceHidePosition;

        victoryTextPos = victoryText.GetComponent<RectTransform>();
        rewardsTextPos = rewardsText.GetComponent<RectTransform>();

        UpdateDiceStats(CombatManager.Instance.player1, leftDiceStat);
        UpdateDiceStats(CombatManager.Instance.player2, rightDiceStat);

        ShowVSHeader();
        ShowDiceInfo();


    }
    /*
    private void Update()
    {
        // Testing functions
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ShowVSHeader();
            ShowDiceInfo();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            HideVSHeader();
            HideDiceInfo();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            // Left attacking | right defending
            ShowInputPrompt(rightDefendPrompt, 0.25f);
            ShowInputPrompt(leftAttackPrompt, 0.25f);

            HideInputPrompt(leftDefendPrompt, 0.25f);
            HideInputPrompt(rightAttackPrompt, 0.25f);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            // Left defend | right attack
            ShowInputPrompt(leftDefendPrompt, 0.25f);
            ShowInputPrompt(rightAttackPrompt, 0.25f);

            HideInputPrompt(rightDefendPrompt, 0.25f);
            HideInputPrompt(leftAttackPrompt, 0.25f);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            // fill left side's dice stats
            //UpdateDiceStats(entity1, leftDiceStat);
            //UpdateDiceStats(entity2, rightDiceStat);
            //StalemateResults();
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            UpdateInputPrompts(CombatManager.Instance.player1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            UpdateInputPrompts(CombatManager.Instance.player2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            //VictoryResults(CombatManager.Instance.player2, null);
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            ShowFloatingDamageNumber(CombatManager.Instance.player2, 34);
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            ShowFloatingDamageNumber(CombatManager.Instance.player1, 34);
        }
    }
    */
    public void ShowVSHeader()
    {
        vsHeader.DOAnchorPos(vsInitialPosition, 0.5f, false).SetEase(Ease.OutSine);
    }

    public void HideVSHeader()
    {
        vsHeader.DOAnchorPos(vsHidePosition, 0.5f, false).SetEase(Ease.InFlash);
    }

    public void ShowDiceInfo()
    {
        diceInfo.DOAnchorPos(diceInitialPosition, 0.5f, false).SetEase(Ease.OutSine);
    }

    public void HideDiceInfo()
    {
        diceInfo.DOAnchorPos(diceHidePosition, 0.5f, false).SetEase(Ease.InFlash);
    }

    public void ShowInputPrompt(CanvasGroup inputPrompt, float duration)
    {
        DOTween.To(()=> inputPrompt.alpha, x=> inputPrompt.alpha =  x, 1, duration); 
    }

    public void HideInputPrompt(CanvasGroup inputPrompt, float duration)
    {
        DOTween.To(() => inputPrompt.alpha, x => inputPrompt.alpha = x, 0, duration);
    }

    public void SetInputPromptAlpha(CanvasGroup inputPrompt, float alphaValue, float duration)
    {
        DOTween.To(() => inputPrompt.alpha, x => inputPrompt.alpha = x, alphaValue, duration);
    }

    public void AttackerActionSelected(EntityPiece entity)
    {
        // Entity, check side and phase | Either the attacker or defender picked an action
        if(entity.fightingPosition == CombatUIManager.FightingPosition.Left)
        {
            
        }
    }

    public void DefenderActionSelected(EntityPiece entity)
    {
        // Entity, check side and phase | Either the attacker or defender picked an action
        if (entity.fightingPosition == CombatUIManager.FightingPosition.Right)
        {

        }
    }
    public void UpdatePhaseTextPrompt(EntityPiece entity, Action.PhaseTypes phase)
    {
        if (entity.isEnemy)
            return;
        if(phase == Action.PhaseTypes.Attack)
            phaseTestPrompt.text = $"{entity.entityName}, choose an action.";
        else
            phaseTestPrompt.text = $"{entity.entityName}, choose an action.";
    }

    public void ShowSelectedAction(EntityPiece entity, Action action)
    {
        HideHeaderInfo();
        //HideBothInutPrompts();

        CanvasGroup sideSelectedAction;
        if (entity.fightingPosition == CombatUIManager.FightingPosition.Left)
        {
            sideSelectedAction = leftSelectedAction;
        }
        else
        {
            sideSelectedAction = rightSelectedAction;
        }
            int typeIcon = 0;
        switch (action.type)
        {
            case Action.WeaponTypes.Melee: typeIcon = 0; break;
            case Action.WeaponTypes.Gun: typeIcon = 1; break;
            case Action.WeaponTypes.Magic: typeIcon = 2; break;
        }

        if(action.phase == Action.PhaseTypes.Attack)
        {
            Debug.Log($"{entity.entityName} is attacking");
            sideSelectedAction.GetComponent<Image>().color = attackColor;
        }
        else
        {
            Debug.Log($"{entity.entityName} is defending");
            sideSelectedAction.GetComponent<Image>().color = defendColor;
        }

        if (entity.fightingPosition == CombatUIManager.FightingPosition.Left)
        {
            ShowInputPrompt(sideSelectedAction, 0.15f);
            sideSelectedAction.GetComponentInChildren<TextMeshProUGUI>().text = $"<sprite={typeIcon}>  {action.actionName}";
        }
        else
        {
            ShowInputPrompt(sideSelectedAction, 0.15f);
            sideSelectedAction.GetComponentInChildren<TextMeshProUGUI>().text = $"<sprite={typeIcon}>  {action.actionName}";
        }
    }

    public void DisplayResultsGroup()
    {
        DOTween.To(() => resultsGroup.alpha, x => resultsGroup.alpha = x, 1, 0.25f).SetEase(Ease.InFlash);
    }

    public void DisplayStalemateGroup()
    {
        DOTween.To(() => stalemateGroup.alpha, x => stalemateGroup.alpha = x, 1, 0.25f).SetEase(Ease.InFlash);
    }

    public void UpdateDiceStats(EntityPiece entity, GameObject diceStats)
    {
        // Visually updates the dice stats ui based on the entity and side
        diceNumbers.Clear();

        // Goes through the diceStats UI List and finds the text components
        foreach (Transform child in diceStats.transform)
        {
            diceNumbers.Add(child.GetComponentInChildren<TextMeshProUGUI>());
        }

        // Updates each individual dice from the text list based on the type
        // the following code is ABSOLUTELY DISGUSTING
        var faceIndex = 0;

        for (int i = 0; i < 6; i++)
        {
            diceNumbers[i].text = $"{entity.strDie[faceIndex]}";
            faceIndex++;
        }

        faceIndex = 0;

        for (int i = 6; i < 12; i++)
        {
            diceNumbers[i].text = $"{entity.dexDie[faceIndex]}";
            faceIndex++;
        }

        faceIndex = 0;

        for (int i = 12; i < 18; i++)
        {
            diceNumbers[i].text = $"{entity.intDie[faceIndex]}";
            faceIndex++;
        }
    }

    public void UpdateInputPrompts(EntityPiece attacker)
    {
        // should always pass player that is attacking
        if (attacker.fightingPosition == CombatUIManager.FightingPosition.Left)
        {
            // Left attacking | right defending
            ShowInputPrompt(rightDefendPrompt, 0.25f);
            ShowInputPrompt(leftAttackPrompt, 0.25f);

            HideInputPrompt(leftDefendPrompt, 0.25f);
            HideInputPrompt(rightAttackPrompt, 0.25f);
        }
        else
        {
            // Left defend | right attack
            ShowInputPrompt(leftDefendPrompt, 0.25f);
            ShowInputPrompt(rightAttackPrompt, 0.25f);

            HideInputPrompt(rightDefendPrompt, 0.25f);
            HideInputPrompt(leftAttackPrompt, 0.25f);
        }
    }

    public void HideBothInputPrompts()
    {
        leftDefendPrompt.alpha = 0;
        rightAttackPrompt.alpha = 0;
        leftAttackPrompt.alpha = 0;
        rightDefendPrompt.alpha = 0;
        /*
        HideInputPrompt(leftDefendPrompt, 0.15f);
        HideInputPrompt(rightAttackPrompt, 0.15f);

        HideInputPrompt(rightDefendPrompt, 0.15f);
        HideInputPrompt(leftAttackPrompt, 0.15f);
        */
    }

    public void StalemateResults()
    {
        DisplayStalemateGroup();
        //HideHeaderInfo();
        Debug.Log("stalemate.");
        //volume.weight = 1;
        DOTween.To(() => volume.weight, x => volume.weight = x, 1, 0.15f).SetEase(Ease.InOutExpo);

        leftDiceRoll.StartDisappearingText();
        rightDiceRoll.StartDisappearingText();

        HideInputPrompt(leftSelectedAction, 0.25f);
        HideInputPrompt(rightSelectedAction, 0.25f);
        HideDiceInfo();
        HideVSHeader();

        //resultsGroup.alpha = 1;
        var result = stalemateGroup.GetComponentInChildren<TextMeshProUGUI>();
        //result.color = new Color32(118, 118, 118, 255);
        result.text = "To be continued...";
        /*
        var resultsText = resultsGroup.GetComponentInChildren<TextMeshProUGUI>();
        resultsText.color = new Color32(118, 118, 118, 255);
        resultsText.text = "To Be Continued...";

        DisplayresultsGroup();
        */
    }

    public void VictoryResults(List<int> rewards, List<ItemStats> items)
    {
        //rewards[0] = rep/exp gained, rewards[1] = money gained
        if(rewards.Count >= 3) // stupid ass check for a PvP win
        {
            if (rewards[2] == 2) // P2 won
            {
                victoryTextPos.anchoredPosition = p2VictoryPosition;
                rewardsTextPos.anchoredPosition = p2RewardsPosition;
            }
        }
        else // P1 won, also against wildlife
        {
            victoryTextPos.anchoredPosition = p1VictoryPosition;
            rewardsTextPos.anchoredPosition = p1RewardsPosition;
        }

        Debug.Log("victory!");

        DisplayResultsGroup();

        leftDiceRoll.StartDisappearingText();
        rightDiceRoll.StartDisappearingText();

        HideInputPrompt(leftSelectedAction, 0.25f);
        HideInputPrompt(rightSelectedAction, 0.25f);

        victoryText.ShowText("Victory!");
        //rewardsText.ShowText($"<sprite=\"item_icons\" index={items[0].id}> {items[0].itemName}\n<sprite=\"Coin Icon\" index=0> {items[1].itemName}");
        string rewardInfo = "";
        if (rewards[1] > 0) // Money Gained
        {
            rewardInfo += $"<color=#FFDC5B>+{rewards[1]}</color><sprite=\"Coin Icon\" index=0>\t";
        }
        if (rewards[0] >= 0) // Repuation Gained
        {
            rewardInfo += $"<color=#7AFFF7>+{rewards[0]} EXP</color>\n";
        }
        if (items != null) // Items Gained
        {
            rewardInfo += $"+1 {items[0].itemName}\n+1 {items[1].itemName}";
        }

        rewardsText.ShowText(rewardInfo);

        //resultsGroup.alpha = 1;
        /*
        var result = resultsGroup.GetComponentInChildren<TextMeshProUGUI>();
        result.color = new Color32(240, 250, 0, 255);
        result.text = $"Victory!\n";
        result.text += "Obtained:\n";
        foreach(var item in items)
        {
            result.text += $"<color=white>{item.itemName}</color>\n";
        }
        */
    }
    public void DefeatResults(List<int> rewards, List<ItemStats> items)
    {
        // Lost to a wildlife, make fun of the player
        victoryTextPos.anchoredPosition = p2VictoryPosition;
        rewardsTextPos.anchoredPosition = p2RewardsPosition;

        DisplayResultsGroup();

        leftDiceRoll.StartDisappearingText();
        rightDiceRoll.StartDisappearingText();

        HideInputPrompt(leftSelectedAction, 0.25f);
        HideInputPrompt(rightSelectedAction, 0.25f);

        victoryText.ShowText("Defeat...");

        string rewardInfo = "";
        rewardInfo += $"<color=#FFDC5B>{rewards[1]}<sprite=\"Coin Icon\" index=0></color> was stolen!\n";
        rewardInfo += $"The enemy becomes stronger.";

        rewardsText.ShowText(rewardInfo);
    }


        public void ShowFloatingDamageNumber(EntityPiece defender, float damage, CombatManager.TypeAdvantage typeAdvantage)
    {
        Vector2 initPos;
        Vector2 goToPos;
        RectTransform dmgPos = floatingDamage.GetComponent<RectTransform>();

        if (defender.fightingPosition == CombatUIManager.FightingPosition.Right)
        {
            initPos = floatingDmgRightInitPosition;
            goToPos = floatingDmgRightPosition;
            hitParticle.transform.position = new Vector3(4.5f, .5f, 0);
        }
        else
        {
            // Defender is on the left
            initPos = floatingDmgLeftInitPosition;
            goToPos = floatingDmgLeftPosition;
            hitParticle.transform.position = new Vector3(-4.5f, .5f,0);
        }
        dmgPos.anchoredPosition = initPos;

        ShowInputPrompt(floatingDamage, 0.05f);

        DecideFloatingNumberVisual((int)damage, typeAdvantage);

        hitParticle.Play();

        dmgPos.DOAnchorPos(goToPos, 0.25f, false).SetEase(Ease.OutBounce);

        StartCoroutine(HideFloatingDamageNumber(floatingDamage));
    }

    public void DecideFloatingNumberVisual(int damage, CombatManager.TypeAdvantage typeAdvantage)
    {
        if(damage < 0)
        {
            // Rainbow heal bullshit, only works if the defense was an 11
            floatingDamage.GetComponent<TextMeshProUGUI>().colorGradientPreset = advantageTypeGradients[3];
            floatingDamageNumber.ShowText($"+{(damage * -1)}");
        }
        else
        {
            floatingDamage.GetComponent<TextMeshProUGUI>().colorGradientPreset = advantageTypeGradients[(int)typeAdvantage];
            switch (typeAdvantage)
            {
                case CombatManager.TypeAdvantage.Resist:
                    //floatingDamage.GetComponent<TextMeshProUGUI>().text = $"<size=120>{damage}</size>";
                    floatingDamageNumber.ShowText($"<size=156>{damage}</size>");
                    break;
                case CombatManager.TypeAdvantage.Neutral:
                    floatingDamageNumber.ShowText($"{damage}");
                    break;
                case CombatManager.TypeAdvantage.Strong:
                    floatingDamageNumber.ShowText($"<size=218>{damage}!!");
                    break;
                default:
                    floatingDamageNumber.ShowText($"{damage}???");
                    break;

            }
        }
    }

    public IEnumerator HideFloatingDamageNumber(CanvasGroup floatingDamage)
    {
        yield return new WaitForSeconds(.75f);
        HideInputPrompt(floatingDamage, 0.15f);
        yield return null;
    }

    public void SwapPhaseTransitions(EntityPiece attacker)
    {
        UpdateInputPrompts(attacker);
        ShowVSHeader();
        ShowDiceInfo();
        HideInputPrompt(leftSelectedAction, 0.25f);
        HideInputPrompt(rightSelectedAction, 0.25f);

        leftDiceRoll.StartDisappearingText();
        rightDiceRoll.StartDisappearingText();
        //leftDiceRoll.GetComponent<TextMeshProUGUI>().text = "";
        //rightDiceRoll.GetComponent<TextMeshProUGUI>().text = "";
    }

    public void HideHeaderInfo()
    {
        phaseTestPrompt.text = "";

        HideVSHeader();
        HideDiceInfo();

        HideInputPrompt(leftDefendPrompt, 0.15f);
        HideInputPrompt(rightAttackPrompt, 0.15f);
        HideInputPrompt(rightDefendPrompt, 0.15f);
        HideInputPrompt(leftAttackPrompt, 0.15f);
    }

    public void SetDiceActionRoll(EntityPiece entity, float roll, Action.WeaponTypes weaponType)
    {
        Debug.Log($"{entity.name} picked {weaponType} with roll[{roll}]");
        if(entity.fightingPosition == CombatUIManager.FightingPosition.Left)
        {
            leftDiceRoll.GetComponent<TextMeshProUGUI>().colorGradientPreset = actionTypeGradients[(int)weaponType];

            if (roll <= 1)
                leftDiceRoll.ShowText($"<size=120><shake a=.2 d=.5>{roll}</shake></size>");
            else if (roll >= 10)
                leftDiceRoll.ShowText($"<size=180>{roll}</size>");
            else
                leftDiceRoll.ShowText($"{roll}");
        }
        else
        {
            rightDiceRoll.GetComponent<TextMeshProUGUI>().colorGradientPreset = actionTypeGradients[(int)weaponType];

            if (roll <= 1)
                rightDiceRoll.ShowText($"<size=126><shake a=.2 d=.5>{roll}</shake></size>");
            else if (roll >= 10)
                rightDiceRoll.ShowText($"<size=180>{roll}</size>");
            else
                rightDiceRoll.ShowText($"{roll}");
        }
    }
}
