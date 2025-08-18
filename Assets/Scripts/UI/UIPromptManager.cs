using System.Collections;
using UnityEngine;
using TMPro;
using Febucci.UI.Core;
using LitMotion;
using static UnityEngine.Rendering.DebugUI;
using LitMotion.Extensions;

public class UIPromptManager : MonoBehaviour
{
    private bool NextPlayerGoIsRunning = false;
    private Coroutine oldNextPlayerGo;
    private EntityPiece currentPlayer;

    [SerializeField] private TAnimCore rollTextAnimator;
    [SerializeField] private TypewriterCore rollTypewriter;
    [SerializeField] private TypewriterCore turnTypewriter;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI inputPrompt;
    [SerializeField] private TextMeshProUGUI rolledNumber;
    [SerializeField] private TextMeshProUGUI movementRoll;

    [SerializeField] private CanvasGroup menuPrompt;
    [SerializeField] private TextMeshProUGUI inventoryPromptText;
    [SerializeField] private TextMeshProUGUI buildPromptText;
    [SerializeField] private TextMeshProUGUI buildLimitText;

    [Header("Listen on Event Channels")]
    public IntEventChannelSO m_RollForMovement;
    public PlayerEventChannelSO m_DiceRollPrep;
    public PlayerEventChannelSO m_DiceRollUndo;

    public PlayerEventChannelSO m_NextPlayerTurn;
    public PlayerEventChannelSO m_EncounterDecisions;
    public NodeEventChannelSO m_LandOnStorefront;
    public ItemEventChannelSO m_ItemSold;

    public PlayerEventChannelSO m_OpenInventory;
    public VoidEventChannelSO m_ExitInventory;
    public NodeEventChannelSO m_RestockStore;

    public PlayerEventChannelSO m_BuildStore;

    public PlayerEventChannelSO m_OverturnOpportunity;
    public IntItemEventChannelSO m_ItemUsed;

    public VoidEventChannelSO m_EnableFreeview;
    public VoidEventChannelSO m_DisableFreeview;
    public VoidEventChannelSO m_ExitRaycastedTile;

    public PlayerEventChannelSO m_EnterLevelUp;
    public VoidEventChannelSO m_ExitLevelUp;

    private void Start()
    {
        rolledNumber.text = "";
        movementRoll.text = "-";
    }

    private void OnEnable()
    {
        m_RollForMovement.OnEventRaised += RolledDice;

        m_DiceRollPrep.OnEventRaised += DisplayRollPrompt;
        m_DiceRollUndo.OnEventRaised += DisplayInitialMenu;

        m_NextPlayerTurn.OnEventRaised += OnNextPlayerTurn;
        //m_NextPlayerTurn.OnEventRaised += NormalizeInventoryPrompt; //temp
        m_EncounterDecisions.OnEventRaised += DisplayEncounterChoices;
        m_LandOnStorefront.OnEventRaised += DisplayStorefrontPrompt;
        m_ItemSold.OnEventRaised += DisplayLeavePrompt;

        m_OpenInventory.OnEventRaised += HideInitialMenu;
        m_ExitInventory.OnEventRaised += DisplayInitialMenu;

        m_RestockStore.OnEventRaised += ClearInputText;

        m_BuildStore.OnEventRaised += OnBuildStore;

        m_OverturnOpportunity.OnEventRaised += DisplayOverturnChoices;

        m_ItemUsed.OnEventRaised += StrikethroughInventoryPrompt;

        m_EnableFreeview.OnEventRaised += DisplayFreeviewPrompt;
        m_EnableFreeview.OnEventRaised += HideInitialMenu;
        m_DisableFreeview.OnEventRaised += DisplayInitialMenu;

        m_EnterLevelUp.OnEventRaised += OnEnterLevelUp;
        m_ExitLevelUp.OnEventRaised += OnExitLevelUp;
    }

    private void OnDisable()
    {
        m_RollForMovement.OnEventRaised -= RolledDice;

        m_DiceRollPrep.OnEventRaised -= DisplayRollPrompt;
        m_DiceRollUndo.OnEventRaised -= DisplayInitialMenu;
        m_NextPlayerTurn.OnEventRaised -= OnNextPlayerTurn;
        //m_NextPlayerTurn.OnEventRaised -= NormalizeInventoryPrompt; //temp
        m_EncounterDecisions.OnEventRaised -= DisplayEncounterChoices;
        m_LandOnStorefront.OnEventRaised -= DisplayStorefrontPrompt;
        m_ItemSold.OnEventRaised -= DisplayLeavePrompt;

        m_EnableFreeview.OnEventRaised -= DisplayFreeviewPrompt;
        m_OpenInventory.OnEventRaised -= HideInitialMenu;
        m_ExitInventory.OnEventRaised -= DisplayInitialMenu;

        m_RestockStore.OnEventRaised -= ClearInputText;

        m_BuildStore.OnEventRaised -= OnBuildStore;

        m_OverturnOpportunity.OnEventRaised -= DisplayOverturnChoices;

        m_ItemUsed.OnEventRaised -= StrikethroughInventoryPrompt;

        m_EnableFreeview.OnEventRaised -= HideInitialMenu;
        m_DisableFreeview.OnEventRaised -= DisplayInitialMenu;

        m_EnterLevelUp.OnEventRaised -= OnEnterLevelUp;
        m_ExitLevelUp.OnEventRaised -= OnExitLevelUp;
    }

    private void OnNextPlayerTurn(EntityPiece ps)
    {
        currentPlayer = ps;

        DisplayInitialMenu(ps);
        NormalizeInventoryPrompt();

        if (NextPlayerGoIsRunning)
            StopCoroutine(oldNextPlayerGo);
        oldNextPlayerGo = StartCoroutine(NotifyNextPlayerGo(ps));
    }

    private IEnumerator NotifyNextPlayerGo(EntityPiece ps)
    {
        NextPlayerGoIsRunning = true;
        string goLine = "{offset}{size}Go, " + ps.entityName + "!";

        turnTypewriter.GetComponent<TextMeshProUGUI>().color = ps.playerColor;
        turnTypewriter.ShowText(goLine);

        yield return new WaitForSeconds(2f);

        turnTypewriter.StartDisappearingText();

        NextPlayerGoIsRunning = false;
        yield return null;
    }

    private void RolledDice(int diceRoll)
    {
        inputPrompt.text = "";

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

    private void DisplayRollPrompt(EntityPiece ps)
    {
        // This will get swapped out with a menu selection
        inputPrompt.text = "<sprite=0><color=white></color> Roll Dice";
        inputPrompt.text += "\n<sprite=1><color=white></color> Back";

        rollTypewriter.ShowText("<size=84><bounce a=.3>Rolling...</>");

        HideInitialMenu();
    }

    private void DisplayFreeviewPrompt()
    {
        inputPrompt.text = "<color=white>Freeview Mode</color>";
        inputPrompt.text += "\n<sprite=0><color=white></color> Select Tile";
        inputPrompt.text += "\n<sprite=1><color=white></color> Back";
        //inputPrompt.text += "\n<color=white>[Scroll Wheel]</color> Zoom In/Out";
    }

    private void DisplayInitialMenu(EntityPiece ps)
    {
        ClearInputText();
        NormalizeBuildPrompt(ps);
        if (GameplayTest.instance.phase == GameplayTest.GamePhase.PickDirection)
            return;

        //menuPrompt.alpha = 1;
        ShowMenuPrompt();
    }

    private void DisplayInitialMenu()
    {
        //ClearInputText();

        if (GameplayTest.instance.phase == GameplayTest.GamePhase.PickDirection) //fuck ass way for ITM to not show while exiting freeview during pickdirection
            return;

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
        LMotion.Create(menuPrompt.alpha, 1, 0.1f)
            .Bind(x => menuPrompt.alpha = x);

        LMotion.Create(menuPrompt.GetComponent<RectTransform>().localScale, Vector3.one, 0.1f)
            .WithEase(Ease.InQuad)
            .Bind(x => menuPrompt.GetComponent<RectTransform>().localScale = x);
    }

    private void HideMenuPrompt()
    {
        LMotion.Create(menuPrompt.alpha, 0, 0.1f)
            .Bind(x => menuPrompt.alpha = x);

        LMotion.Create(menuPrompt.GetComponent<RectTransform>().localScale, Vector3.zero, 0.1f)
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
        inputPrompt.text = "<color=white>Hover over</color> items to see details.\n";
        inputPrompt.text += "<color=white>Left click</color> to buy an item.\n";
        inputPrompt.text += "You <color=red>must</color> buy one item to leave.";
    }

    private void DisplayLeavePrompt(ItemStats item)
    {
        inputPrompt.text = "<color=white>[SPACE]</color> to leave the store.\n";
    }

    private void NormalizeInventoryPrompt()
    {
        inventoryPromptText.text = "Item";
    }

    private void StrikethroughInventoryPrompt(int index, ItemStats item)
    {
        inventoryPromptText.text = "<color=grey>Item</color>";
    }

    private void NormalizeBuildPrompt(EntityPiece ps)
    {
        var buildCount = 4 - ps.storeCount;

        if(buildCount <= 0 || !ps.occupiedNode.CompareTag("Encounter"))
        {
            buildPromptText.text = "<color=grey>Build</color>";
            buildLimitText.text = "<color=grey>No</color>";
        }
        else
        {
            buildPromptText.text = "Build";
            buildLimitText.text = $"{4 - ps.storeCount} Left";
        }
    }

    private void StrikethroughBuildPrompt()
    {
        buildPromptText.text = "<color=grey>Build</color>";
        buildLimitText.text = "<color=grey>No</color>";
    }

    private void DisplayOverturnChoices(EntityPiece storeOwner)
    {
        inputPrompt.text = $"There's no items in {storeOwner.entityName}'s store...  Overturn ownership?";
        inputPrompt.text += "\n<color=white>[LMB]/[SPACE]</color> No, leave it alone.";
        inputPrompt.text += "\n<color=white>[RMB]/[SHIFT]</color> Yes, take it over! <color=white>Costs</color> <color=yellow>@</color>600";
    }

    private void OnBuildStore(EntityPiece ep)
    {
        StrikethroughBuildPrompt();
    }

    private void OnEnterLevelUp(EntityPiece ep)
    {
        ClearInputText();
    }

    private void OnExitLevelUp()
    {
        DisplayRollPrompt(null);
        m_DiceRollPrep.RaiseEvent(currentPlayer);
    }
}
