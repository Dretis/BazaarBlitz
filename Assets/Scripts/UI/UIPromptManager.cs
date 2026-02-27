using System.Collections;
using UnityEngine;
using TMPro;
using Febucci.UI.Core;
using LitMotion;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using LitMotion.Extensions;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.Localization.Components;

public class UIPromptManager : MonoBehaviour
{
    private bool NextPlayerGoIsRunning = false;
    private Coroutine oldNextPlayerGo;
    private EntityPiece currentPlayer;

    private MotionHandle goTurnMotion;

    [SerializeField] private Color normalPromptColor;

    [SerializeField] private TAnimCore rollTextAnimator;
    [SerializeField] private TypewriterCore rollTypewriter;
    [SerializeField] private TypewriterCore turnTypewriter;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI turnRoundIndicator;

    [SerializeField] private TextMeshProUGUI inputPrompt;
    private RectTransform inputPromptTransform;
    private Vector3 initialInputPromptPos;
    [SerializeField] private Vector3 hiddenInputPromptPos;


    [SerializeField] private TextMeshProUGUI rolledNumber;
    [SerializeField] private TextMeshProUGUI movementRoll;

    [Space]
    [SerializeField] private CanvasGroup promptInstructionGroup;
    [SerializeField] private TextMeshProUGUI promptInstructionText;
    [SerializeField] private List<GameObject> promptConfirmButtonHolders = new List<GameObject>();

    [Header("ITM UI Elements")]
    [SerializeField] private CanvasGroup menuPrompt;
    [SerializeField] private TextMeshProUGUI movePromptText;

    [Space]
    [SerializeField] private TextMeshProUGUI inventoryPromptText;
    [SerializeField] private TextMeshProUGUI inventoryLimitText;

    [Space]
    [SerializeField] private TextMeshProUGUI buildPromptText;
    [SerializeField] private TextMeshProUGUI buildLimitText;

    [Header("Backseating Elements")]
    [SerializeField] private CanvasGroup backseatGroup;

    [Header("Broadcast on Event Channels")]
    public PlayerEventChannelSO m_ConfirmBuildStore;
    public VoidEventChannelSO m_CancelBuildStore; // also listening
    public NodeEventChannelSO m_RestockStore; // also listening

    [Header("Listen on Event Channels")]
    public IntEventChannelSO m_RollForMovement;
    public PlayerEventChannelSO m_TryDiceRollPrep;
    public PlayerEventChannelSO m_DiceRollPrep;
    public PlayerEventChannelSO m_DiceRollUndo;

    public PlayerEventChannelSO m_NextPlayerTurn;
    public IntEventChannelSO m_NextTurnRound;

    public PlayerEventChannelSO m_EncounterDecisions;
    public NodeEventChannelSO m_LandOnStorefront;
    public ItemEventChannelSO m_ItemSold;

    public NodeEventChannelSO m_LandOnVendor;

    public PlayerEventChannelSO m_OpenInventory;
    public VoidEventChannelSO m_ExitInventory;

    public PlayerEventChannelSO m_TryBuildStore;
    public PlayerEventChannelSO m_BuildStore; // also listening
    public PlayerEventChannelSO m_FinishStockingStore;

    public PlayerEventChannelSO m_OverturnOpportunity;
    public IntItemEventChannelSO m_ItemUsed;

    public VoidEventChannelSO m_EnableFreeview;
    public VoidEventChannelSO m_DisableFreeview;
    public VoidEventChannelSO m_ExitRaycastedTile;

    public PlayerEventChannelSO m_EnterLevelUp;
    public VoidEventChannelSO m_ExitLevelUp;

    public VoidEventChannelSO m_IncidentStarted;

    public PlayerEventChannelSO m_PlayerWon;

    public VoidEventChannelSO m_HoldPlayerInfo;
    public VoidEventChannelSO m_ReleasePlayerInfo;

    private void Start()
    {
        inputPromptTransform = inputPrompt.GetComponent<RectTransform>();

        backseatGroup.alpha = 0;
        backseatGroup.interactable = false;

        rolledNumber.text = "";
        movementRoll.text = "-";

        promptInstructionGroup.alpha = 0;
        promptInstructionGroup.interactable = false;
        promptInstructionGroup.GetComponent<RectTransform>().localScale = Vector3.zero;
    }

    private void OnEnable()
    {
        m_RollForMovement.OnEventRaised += RolledDice;

        m_TryDiceRollPrep.OnEventRaised += OnTryDiceRollPrep;
        m_DiceRollPrep.OnEventRaised += DisplayRollPrompt;
        m_DiceRollUndo.OnEventRaised += DisplayInitialMenu;

        m_NextPlayerTurn.OnEventRaised += OnNextPlayerTurn;
        m_NextTurnRound.OnEventRaised += OnNextTurnRound;
        //m_NextPlayerTurn.OnEventRaised += NormalizeInventoryPrompt; //temp
        m_EncounterDecisions.OnEventRaised += DisplayEncounterChoices;
        m_LandOnStorefront.OnEventRaised += DisplayStorefrontPrompt;
        m_ItemSold.OnEventRaised += DisplayLeavePrompt;

        m_LandOnVendor.OnEventRaised += OnLandOnVendor;

        m_OpenInventory.OnEventRaised += HideInitialMenu;
        m_ExitInventory.OnEventRaised += DisplayInitialMenu;

        m_RestockStore.OnEventRaised += ClearInputText;

        m_TryBuildStore.OnEventRaised += OnTryBuildStore;
        m_BuildStore.OnEventRaised += OnBuildStore;
        m_CancelBuildStore.OnEventRaised += OnCancelBuildStore;
        m_FinishStockingStore.OnEventRaised += OnFinishStockingStore;

        m_OverturnOpportunity.OnEventRaised += DisplayOverturnChoices;

        m_ItemUsed.OnEventRaised += StrikethroughInventoryPrompt;

        m_EnableFreeview.OnEventRaised += DisplayFreeviewPrompt;
        m_EnableFreeview.OnEventRaised += HideInitialMenu;
        m_DisableFreeview.OnEventRaised += DisplayInitialMenu;

        m_EnterLevelUp.OnEventRaised += OnEnterLevelUp;
        m_ExitLevelUp.OnEventRaised += OnExitLevelUp;

        m_IncidentStarted.OnEventRaised += OnIncidentStarted;

        m_PlayerWon.OnEventRaised += OnPlayerWon;

        m_HoldPlayerInfo.OnEventRaised += OnHoldPlayerInfo;
        m_ReleasePlayerInfo.OnEventRaised += OnReleasePlayerInfo;
    }

    private void OnDisable()
    {
        m_RollForMovement.OnEventRaised -= RolledDice;

        m_TryDiceRollPrep.OnEventRaised -= OnTryDiceRollPrep;
        m_DiceRollPrep.OnEventRaised -= DisplayRollPrompt;
        m_DiceRollUndo.OnEventRaised -= DisplayInitialMenu;

        m_NextPlayerTurn.OnEventRaised -= OnNextPlayerTurn;
        m_NextTurnRound.OnEventRaised -= OnNextTurnRound;
        //m_NextPlayerTurn.OnEventRaised -= NormalizeInventoryPrompt; //temp
        m_EncounterDecisions.OnEventRaised -= DisplayEncounterChoices;
        m_LandOnStorefront.OnEventRaised -= DisplayStorefrontPrompt;
        m_ItemSold.OnEventRaised -= DisplayLeavePrompt;

        m_LandOnVendor.OnEventRaised -= OnLandOnVendor;

        m_EnableFreeview.OnEventRaised -= DisplayFreeviewPrompt;
        m_OpenInventory.OnEventRaised -= HideInitialMenu;
        m_ExitInventory.OnEventRaised -= DisplayInitialMenu;

        m_RestockStore.OnEventRaised -= ClearInputText;

        m_TryBuildStore.OnEventRaised -= OnTryBuildStore;
        m_BuildStore.OnEventRaised -= OnBuildStore;
        m_CancelBuildStore.OnEventRaised -= OnCancelBuildStore;
        m_FinishStockingStore.OnEventRaised -= OnFinishStockingStore;

        m_OverturnOpportunity.OnEventRaised -= DisplayOverturnChoices;

        m_ItemUsed.OnEventRaised -= StrikethroughInventoryPrompt;

        m_EnableFreeview.OnEventRaised -= HideInitialMenu;
        m_DisableFreeview.OnEventRaised -= DisplayInitialMenu;

        m_EnterLevelUp.OnEventRaised -= OnEnterLevelUp;
        m_ExitLevelUp.OnEventRaised -= OnExitLevelUp;

        m_IncidentStarted.OnEventRaised -= OnIncidentStarted;

        m_PlayerWon.OnEventRaised -= OnPlayerWon;

        m_HoldPlayerInfo.OnEventRaised -= OnHoldPlayerInfo;
        m_ReleasePlayerInfo.OnEventRaised -= OnReleasePlayerInfo;
    }

    private void OnNextPlayerTurn(EntityPiece ps)
    {
        currentPlayer = ps;

        DisplayInitialMenu(ps);
        NormalizeInventoryPrompt(ps);
        ContextualizeMovePrompt(ps);

        
        //if (NextPlayerGoIsRunning)
        if(goTurnMotion.IsPlaying())
        {
            //NextPlayerGoIsRunning = false;
            StopCoroutine(oldNextPlayerGo);
            goTurnMotion.TryCancel();
        }
        
        oldNextPlayerGo = StartCoroutine(NotifyNextPlayerGo(ps));
    }

    private void OnNextTurnRound(int round)
    {
        turnRoundIndicator.text = round.ToString();
    }

    private IEnumerator NotifyNextPlayerGo(EntityPiece ps)
    {
        NextPlayerGoIsRunning = true;
        string goLine = "{offset}{size}Go, " + ps.entityName + "!";
        var localizedString = turnTypewriter.GetComponent<LocalizeStringEvent>().StringReference;

        var variable = localizedString["PLAYER_NAME"] as StringVariable;
        variable.Value = ps.entityName;
        //Debug.Log("The value is NOW = " + variable.Value);

        turnTypewriter.GetComponent<TextMeshProUGUI>().color = ps.playerColor;
        turnTypewriter.ShowText(localizedString.GetLocalizedString());

        var goTurnRect = turnTypewriter.GetComponent<RectTransform>();
        Vector2 startingPos = new Vector2(0, 150);
        Vector2 showingPos = new Vector2(0, -25);

        goTurnMotion = LMotion.Create(startingPos, showingPos, 1f)
            .WithEase(Ease.OutBack)
            .BindToAnchoredPosition(goTurnRect);
        //BindToAnchoredPosition

        yield return new WaitForSeconds(2f);

        //if(goTurnMotion.)
        //turnTypewriter.ShowText("");
        goTurnMotion = LMotion.Create(showingPos, startingPos, .75f)
            .WithEase(Ease.OutQuad)
            .BindToAnchoredPosition(goTurnRect);

        //turnTypewriter.StartDisappearingText();

        NextPlayerGoIsRunning = false;
        yield return null;
    }

    private void RolledDice(int diceRoll)
    {
        //inputPrompt.text = "";
        DisplayMoveAroundPrompt();

        StartCoroutine(DisplayDiceRoll(diceRoll));

        //Debug.Log("Rolled");
    }

    private IEnumerator DisplayDiceRoll(int diceRoll)
    {
        rolledNumber.enabled = true;
        yield return new WaitForSeconds(0.1f);

        string roll = "" + diceRoll;
        rollTypewriter.ShowText(roll);

        yield return null;
        //movementRoll.text = "" + diceRoll;
    }

    private void UpdateDiceRoll(int diceRoll)
    {
        string roll = "" + diceRoll;
        rollTypewriter.ShowText("{size a=1.1}"+roll);
    }

    private void OnTryDiceRollPrep(EntityPiece ps)
    {
        HideMenuPrompt();
    }

    private void DisplayMoveAroundPrompt()
    {
        //inputPrompt.text = "<sprite name=stick_l><color=white></color> Move";
        inputPrompt.text = "\n<sprite name=up><color=white></color> View Board";
        inputPrompt.text += "\n<sprite name=lb><color=white></color> Player Info";
    }

    private void DisplayRollPrompt(EntityPiece ps)
    {
        // This will get swapped out with a menu selection
        inputPrompt.text = "<sprite name=down><color=white></color> Roll Dice";
        inputPrompt.text += "\n<sprite name=right><color=white></color> Back";

        //rollTypewriter.ShowText("<size=84><bounce a=.3>Rolling...</>");
        //LMotion.Punch.Create(0, 25, 0.5f)
        //    .WithDampingRatio(0f)
        //   .BindToAnchoredPositionX(movePromptText.GetComponent<RectTransform>());

        HideInitialMenu();
    }

    private void DisplayFreeviewPrompt()
    {
        inputPrompt.text = "<color=white>Freeview Mode</color>";
        //inputPrompt.text += "\n<sprite name=stick_l><color=white></color> Move";
        inputPrompt.text += "\n<sprite name=down><color=white></color> Select Tile";
        inputPrompt.text += "\n<sprite name=right><color=white></color> Back";
        //inputPrompt.text += "\n<color=white>[Scroll Wheel]</color> Zoom In/Out";
    }

    private void DisplayInitialMenu(EntityPiece ps)
    {
        ClearInputText();
        NormalizeBuildPrompt(ps);
        if (GameplayTest.instance.phase == GameplayTest.GamePhase.PickDirection 
            || GameplayTest.instance.phase == GameplayTest.GamePhase.CombatSelector)
            return;

        //menuPrompt.alpha = 1;
        ShowMenuPrompt();
    }

    private void DisplayInitialMenu()
    {
        //ClearInputText();

        if (GameplayTest.instance.phase == GameplayTest.GamePhase.PickDirection)
        {
            DisplayMoveAroundPrompt();
            //fuck ass way for ITM to not show while exiting freeview during pickdirection
            return;
        }

        ClearInputText();
        //menuPrompt.alpha = 1;
        ShowMenuPrompt();
    }

    private void HideInitialMenu(EntityPiece ps)
    {
        //menuPrompt.alpha = 0;
        HideMenuPrompt();
    }

    private void HideInitialMenu()
    {
        //menuPrompt.alpha = 0;
        HideMenuPrompt();
    }

    private void ShowMenuPrompt()
    {
        LMotion.Create(menuPrompt.alpha, 1, 0.15f)
            .Bind(x => menuPrompt.alpha = x);

        LMotion.Create(menuPrompt.GetComponent<RectTransform>().localScale, Vector3.one, 0.15f)
            //.WithEase(Ease.InQuad)
            .WithEase(Ease.InOutBack)
            //.WithEase(Ease.InQuint)
            .Bind(x => menuPrompt.GetComponent<RectTransform>().localScale = x);
    }

    private void HideMenuPrompt()
    {
        LMotion.Create(menuPrompt.alpha, 0, 0.15f)
            .Bind(x => menuPrompt.alpha = x);

        LMotion.Create(menuPrompt.GetComponent<RectTransform>().localScale, Vector3.zero, 0.15f)
            .WithEase(Ease.OutQuad)
            .Bind(x => menuPrompt.GetComponent<RectTransform>().localScale = x);
    }

    private void DisplayEncounterChoices(EntityPiece ps)
    {
        inputPrompt.text = "<color=white>[LMB]/[SPACE]</color> to encounter an enemy.";
        inputPrompt.text += "\n<color=white>[RMB]/[SHIFT]</color> to build a store.";
        inputPrompt.text += $"\nYou can build {4 - ps.storeCount} more stores.";
    }

    private void ClearInputText()
    {
        inputPrompt.text = "";
        movementRoll.text = "";
        rollTypewriter.StartDisappearingText();
    }

    private void ClearInputText(MapNode node)
    {
        inputPrompt.text = "";
        movementRoll.text = "";
    }

    private void DisplayStorefrontPrompt(MapNode mapNode)
    {
        inputPrompt.text = "";
        //inputPrompt.text = "<sprite=0><color=white></color> Purchase";
        /*
        inputPrompt.text = "<color=white>Hover over</color> items to see details.\n";
        inputPrompt.text += "<color=white>Left click</color> to buy an item.\n";
        inputPrompt.text += "You <color=red>must</color> buy one item to leave.";
        */
    }

    private void DisplayLeavePrompt(ItemStats item)
    {
        inputPrompt.text = "";
        //inputPrompt.text = "<sprite=0><color=white></color> Leave";
    }

    private void OnLandOnVendor(MapNode mapNode)
    {
        inputPrompt.text = "<sprite name=down><color=white></color> Buy and Enter";
        inputPrompt.text += "\n<sprite name=right><color=white></color> Leave Vendor";
    }

    private void ContextualizeMovePrompt(EntityPiece ps)
    {
        if(ps == null) return;

        if (ps.currentStates.Contains(EntityPiece.State.Fighting) 
            || ps.currentStates.Contains(EntityPiece.State.FightingParty)) 
        {
            movePromptText.text = "Fight";
        }
        else movePromptText.text = $"Move";

    }

    private void NormalizeInventoryPrompt(EntityPiece ps)
    {
        //inventoryPromptText.text = "Item";
        inventoryPromptText.color = normalPromptColor;

        inventoryLimitText.color = Color.white;
        inventoryLimitText.text = $"{ps.inventory.Count}/{ps.inventoryLimit}";
    }

    private void StrikethroughInventoryPrompt(int index, ItemStats item)
    {
        //inventoryPromptText.text = "<color=grey>Item</color>";
        inventoryPromptText.color = Color.gray;
        inventoryLimitText.color = Color.grey;
        inventoryLimitText.text = $"{currentPlayer.inventory.Count-1}/{currentPlayer.inventoryLimit}";
    }

    private void NormalizeBuildPrompt(EntityPiece ps)
    {
        var buildCount = GameplayTest.instance.currentRuleset.storeLimit - ps.storeCount;

        if(buildCount <= 0 || !ps.occupiedNode.CompareTag("Encounter"))
        {
            //buildPromptText.text = "<color=grey>Build</color>";
            buildPromptText.color = Color.gray;
            buildLimitText.text = "<color=grey>No</color>";
        }
        else
        {
            //buildPromptText.text = "Build";
            buildPromptText.color = normalPromptColor;
            buildLimitText.text = $"{buildCount} Left";
        }
    }

    private void StrikethroughBuildPrompt()
    {
        buildPromptText.color = Color.gray;
        //buildPromptText.text = "<color=grey>Build</color>";
        buildLimitText.text = "<color=grey>No</color>";
    }

    private void DisplayOverturnChoices(EntityPiece storeOwner)
    {
        inputPrompt.text = $"There's no items in {storeOwner.entityName}'s store...  Overturn ownership?";
        inputPrompt.text += "\n<color=white>[LMB]/[SPACE]</color> No, leave it alone.";
        inputPrompt.text += "\n<color=white>[RMB]/[SHIFT]</color> Yes, take it over! <color=white>Costs</color> <color=yellow>@</color>600";
    }
    private void ShowPromptInstruction()
    {
        //promptInstructionGroup.alpha = 1;
        promptInstructionGroup.interactable = true;

        LMotion.Create(promptInstructionGroup.alpha, 1, 0.2f)
            .Bind(x => promptInstructionGroup.alpha = x);

        LMotion.Create(promptInstructionGroup.GetComponent<RectTransform>().localScale, Vector3.one * .75f, 0.2f)
            //.WithEase(Ease.InQuad)
            .WithEase(Ease.InOutBack)
            .Bind(x => promptInstructionGroup.GetComponent<RectTransform>().localScale = x);
    }

    private void HidePromptInstruction()
    {
        promptInstructionGroup.interactable = false;

        LMotion.Create(promptInstructionGroup.alpha, 0, 0.2f)
            .Bind(x => promptInstructionGroup.alpha = x);

        LMotion.Create(promptInstructionGroup.GetComponent<RectTransform>().localScale, Vector3.zero, 0.2f)
            .WithEase(Ease.OutQuad)
            .Bind(x => promptInstructionGroup.GetComponent<RectTransform>().localScale = x);
    }

    // Button functions
    public void ConfirmBuildStoreButton()
    {
        m_ConfirmBuildStore.RaiseEvent(currentPlayer);

        HidePromptInstruction();
        StrikethroughBuildPrompt();
        //m_RestockStore.RaiseEvent(currentPlayer.occupiedNode);
    }

    public void CancelBuildStoreButton()
    {
        m_CancelBuildStore.RaiseEvent();
    }
    // end of button functions

    private void OnTryBuildStore(EntityPiece ep)
    {
        HideMenuPrompt();

        ShowPromptInstruction();

        promptInstructionText.text = "Build a store on this space?";

        if(ep.inventory.Count == 0)
        {
            promptInstructionText.text += "\n<color=#D94A45>[!] You have no items to stock.";
        }

        EventSystem.current.SetSelectedGameObject(promptConfirmButtonHolders[0]);
    }

    private void OnBuildStore(EntityPiece ep)
    {
        HidePromptInstruction();

        StrikethroughBuildPrompt();
    }

    private void OnCancelBuildStore()
    {
        ShowMenuPrompt();

        HidePromptInstruction();
    }

    private void OnFinishStockingStore(EntityPiece ep)
    {
        inventoryLimitText.text = $"{ep.inventory.Count}/{ep.inventoryLimit}";
    }

    private void OnEnterLevelUp(EntityPiece ep)
    {
        ClearInputText();

        HideMenuPrompt();
    }

    private void OnExitLevelUp()
    {
        DisplayRollPrompt(null);
        m_DiceRollPrep.RaiseEvent(currentPlayer);
    }

    private void OnIncidentStarted()
    {
        HideMenuPrompt();
        turnTypewriter.ShowText("");
        inputPrompt.text = "";
    }

    private void OnPlayerWon(EntityPiece winner)
    {
        rolledNumber.enabled = false;
        turnTypewriter.ShowText("");
    }

    private void OnHoldPlayerInfo()
    {
        LMotion.Create(backseatGroup.alpha, 1, 0.2f)
            .Bind(x => backseatGroup.alpha = x);

        inputPrompt.enabled = false;
        //LMotion.Create(initialInputPromptPos, hiddenInputPromptPos, 0.2f)
        //    .BindToPosition(inputPromptTransform.transform);

        //backseatGroup.alpha = 1;
        //backseatGroup.interactable = true;
    }

    private void OnReleasePlayerInfo()
    {
        LMotion.Create(backseatGroup.alpha, 0, 0.2f)
            .Bind(x => backseatGroup.alpha = x);

        inputPrompt.enabled = true;
        //LMotion.Create(hiddenInputPromptPos, initialInputPromptPos, 0.2f)
        //    .BindToPosition(inputPromptTransform.transform);

        //backseatGroup.alpha = 0;
        //backseatGroup.interactable = false;
    }
}
