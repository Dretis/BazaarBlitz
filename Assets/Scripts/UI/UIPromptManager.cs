using System.Collections;
using UnityEngine;
using TMPro;
using Febucci.UI.Core;

public class UIPromptManager : MonoBehaviour
{
    private bool NextPlayerGoIsRunning = false;
    private Coroutine oldNextPlayerGo;

    [SerializeField] private TAnimCore rollTextAnimator;
    [SerializeField] private TypewriterCore rollTypewriter;
    [SerializeField] private TypewriterCore turnTypewriter;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI inputPrompt;
    [SerializeField] private TextMeshProUGUI rolledNumber;
    [SerializeField] private TextMeshProUGUI movementRoll;

    [SerializeField] private GameObject menuPrompt;
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

    public PlayerEventChannelSO m_OverturnOpportunity;
    public IntEventChannelSO m_ItemUsed;

    public VoidEventChannelSO m_EnableFreeview;
    public VoidEventChannelSO m_DisableFreeview;
    public VoidEventChannelSO m_ExitRaycastedTile;

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

        m_OverturnOpportunity.OnEventRaised += DisplayOverturnChoices;

        m_ItemUsed.OnEventRaised += StrikethroughInventoryPrompt;

        m_EnableFreeview.OnEventRaised += DisplayFreeviewPrompt;
        m_EnableFreeview.OnEventRaised += HideInitialMenu;
        m_DisableFreeview.OnEventRaised += DisplayInitialMenu;
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

        m_OverturnOpportunity.OnEventRaised -= DisplayOverturnChoices;

        m_ItemUsed.OnEventRaised -= StrikethroughInventoryPrompt;

        m_EnableFreeview.OnEventRaised -= HideInitialMenu;
        m_DisableFreeview.OnEventRaised -= DisplayInitialMenu;
    }

    private void OnNextPlayerTurn(EntityPiece ps)
    {
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
        inputPrompt.text = "<sprite=0><color=white>[SPACE]</color> Roll Dice";
        inputPrompt.text += "\n<sprite=1><color=white>[SHIFT]</color> Back";

        rollTypewriter.ShowText("<size=84><bounce a=.3>Rolling...</>");

        HideInitialMenu();
    }

    private void DisplayFreeviewPrompt()
    {
        inputPrompt.text = "<color=white>Freeview Mode</color>";
        inputPrompt.text += "\n<sprite=0><color=white>[SPACE]</color> Select Tile";
        inputPrompt.text += "\n<sprite=1><color=white>[SHIFT]</color> Back";
        //inputPrompt.text += "\n<color=white>[Scroll Wheel]</color> Zoom In/Out";
    }

    private void DisplayInitialMenu(EntityPiece ps)
    {
        ClearInputText();
        NormalizeBuildPrompt(ps);
        if (GameplayTest.instance.phase == GameplayTest.GamePhase.PickDirection)
            return; 
        menuPrompt.SetActive(true);
    }

    private void DisplayInitialMenu()
    {
        //ClearInputText();

        if (GameplayTest.instance.phase == GameplayTest.GamePhase.PickDirection) //fuck ass way for ITM to not show while exiting freeview during pickdirection
            return;

        ClearInputText();
        menuPrompt.SetActive(true);
    }

    private void HideInitialMenu(EntityPiece ps)
    {
        menuPrompt.SetActive(false);
    }

    private void HideInitialMenu()
    {
        menuPrompt.SetActive(false);
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

    private void StrikethroughInventoryPrompt(int index)
    {
        inventoryPromptText.text = "<color=grey>Item</color>";
    }

    private void NormalizeBuildPrompt(EntityPiece ps)
    {
        var buildCount = 4 - ps.storeCount;

        if(buildCount <= 0 || !ps.occupiedNode.CompareTag("Encounter"))
        {
            buildPromptText.text = "<color=grey>Build</color>";
            buildLimitText.text = "<color=grey>Cant</color>";
        }
        else
        {
            buildPromptText.text = "Build";
            buildLimitText.text = $"{4 - ps.storeCount} Left";
        }
    }

    private void StrikethroughBuildPrompt(int index)
    {
        inventoryPromptText.text = "<color=grey>Build</color>";
    }

    private void DisplayOverturnChoices(EntityPiece storeOwner)
    {
        inputPrompt.text = $"There's no items in {storeOwner.entityName}'s store...  Overturn ownership?";
        inputPrompt.text += "\n<color=white>[LMB]/[SPACE]</color> No, leave it alone.";
        inputPrompt.text += "\n<color=white>[RMB]/[SHIFT]</color> Yes, take it over! <color=white>Costs</color> <color=yellow>@</color>600";
    }
}
