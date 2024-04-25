using System.Collections;
using UnityEngine;
using TMPro;

public class UIPromptManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI inputPrompt;
    [SerializeField] private TextMeshProUGUI rolledNumber;
    [SerializeField] private TextMeshProUGUI movementRoll;

    [SerializeField] private GameObject menuPrompt;
    [SerializeField] private TextMeshProUGUI inventoryPromptText;

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
        m_NextPlayerTurn.OnEventRaised += DisplayInitialMenu;
        m_NextPlayerTurn.OnEventRaised += NormalizeInventoryPrompt; //temp
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
        m_ExitRaycastedTile.OnEventRaised += DisplayInitialMenu;
    }

    private void OnDisable()
    {
        m_RollForMovement.OnEventRaised -= RolledDice;
        m_DiceRollPrep.OnEventRaised -= DisplayRollPrompt;
        m_DiceRollUndo.OnEventRaised -= DisplayInitialMenu;
        m_NextPlayerTurn.OnEventRaised -= DisplayInitialMenu;
        m_NextPlayerTurn.OnEventRaised -= NormalizeInventoryPrompt; //temp
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
        m_ExitRaycastedTile.OnEventRaised -= DisplayInitialMenu;
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
        yield return new WaitForSeconds(0.25f);

        rolledNumber.text = "" + diceRoll;

        yield return new WaitForSeconds(0.75f);

        /*
        Color lowerAlpha;
        lowerAlpha = Color.Lerp(Color.white, new Color(rolledNumber.color.r, rolledNumber.color.g, rolledNumber.color.b, 0), Mathf.PingPong(Time.time, 2));
        rolledNumber.color = lowerAlpha;
        */
        rolledNumber.text = "";
        rolledNumber.enabled = false;

        yield return null;
        //movementRoll.text = "" + diceRoll;
    }

    private void DisplayRollPrompt(EntityPiece ps)
    {
        // This will get swapped out with a menu selection
        inputPrompt.text = "<color=white>[SPACE]</color> to roll!";
        inputPrompt.text += "\n<color=white>[LSHIFT]</color> to go back.";

        HideInitialMenu();
    }

    private void DisplayFreeviewPrompt()
    {
        inputPrompt.text = "<color=white>[Left Click]</color> View Tile Details";
        inputPrompt.text += "\n<color=white>[Right Click]</color> Back";
    }

    private void DisplayInitialMenu(EntityPiece ps)
    {
        ClearInputText();
        menuPrompt.SetActive(true);
    }

    private void DisplayInitialMenu()
    {
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
        inputPrompt.text = "<color=white>[SPACE/ENTER]</color> to encounter an enemy.";
        inputPrompt.text += "\n<color=white>[SHIFT]</color> to build a store. <color=white>Costs</color> <color=yellow>@</color>200";
        inputPrompt.text += $"\nYou can build {4 - ps.storeCount} more stores.";
    }

    private void ClearInputText()
    {
        inputPrompt.text = "";
        movementRoll.text = "";
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

    private void NormalizeInventoryPrompt(EntityPiece ps)
    {
        inventoryPromptText.text = "<color=white>[S]</color> Item";
    }

    private void StrikethroughInventoryPrompt(int index)
    {
        inventoryPromptText.text = "<color=grey>[S] Item</color>";
    }

    private void DisplayOverturnChoices(EntityPiece storeOwner)
    {
        inputPrompt.text = $"There's no items in {storeOwner.entityName}'s store...  Overturn ownership?";
        inputPrompt.text += "\n<color=white>[SPACE]</color> No, leave it alone.";
        inputPrompt.text += "\n<color=white>[SHIFT]</color> Yes, take it over! <color=white>Costs</color> <color=yellow>@</color>600";
    }
}
