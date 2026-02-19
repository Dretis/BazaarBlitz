using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.EventSystems;
using static UnityEditor.Progress;
using Febucci.UI.Core;
using LitMotion;
using LitMotion.Extensions;

public class UIStoreManager : MonoBehaviour
{
    // CHANGE THIS SCRIPTS NAME, THIS ONE IS ONLY HANDLING STOREFRONT UI
    private StoreSelectionHandler selectedStoreHandler;

    [Header("UI Visual Elements")]
    //[SerializeField] private Canvas storefrontCanvas;
    [SerializeField] private CanvasGroup storefrontFullGroup;
    [SerializeField] private List<Button> itemButtons;
    [SerializeField] private List<ItemSelectionHandler> itemSelectionHandlers;
    [SerializeField] private List<EventTrigger> itemSelectionTriggers;
    [SerializeField] private TextMeshProUGUI storeChatBubble;
    private TypewriterCore storeChatTypewriter;
    [Space]
    [SerializeField] private Image storeboat;
    [SerializeField] private Image storekeeperImage;
    [SerializeField] private RectTransform customerRect;
    [SerializeField] private Image customerVisual;
    [Space]
    [SerializeField] private CanvasGroup confirmBuyGroup;
    [SerializeField] private GameObject confirmYesButton;

    // May move this to another script
    [Header("Store Stock")]
    [SerializeField] private List<Image> itemInventoryImages;
    [SerializeField] private List<ItemStats> stockedItems;

    [SerializeField] private RectTransform storeItemGridContainer;
    [SerializeField] private GameObject storeItemPrefab;
    [SerializeField] private List<GameObject> storeItemHolders;


    [Header("Broadcast on Event Channels")]
    public ItemEventChannelSO m_ItemBought;
    public IntEventChannelSO m_UpdatePlayerScore;
    public IntEventChannelSO m_PlayerScoreDecreased;

    [Header("Listen on Event Channels")]
    public NodeEventChannelSO m_LandOnStorefront;
    public VoidEventChannelSO m_ExitStorefront;
    public ItemEventChannelSO m_HoveringItem;
    public ItemListEventChannelSO m_StockItems;
    public IntEventChannelSO m_TryBuyItemAt; 
    public IntEventChannelSO m_RemoveItemAt; 
    
    // this probably needs to be in a seperate script too
    private StoreManager currentStore;
    private EntityPiece currentPlayer;

    // Subscribe to event(s)
    private void OnEnable()
    {
        m_LandOnStorefront.OnEventRaised += EnterStorefront;
        m_ExitStorefront.OnEventRaised += ExitStorefront;
        m_HoveringItem.OnEventRaised += HighlightItem;
        m_StockItems.OnEventRaised += StockItems;

        m_TryBuyItemAt.OnEventRaised += OnTryBuyItemAt;
        m_RemoveItemAt.OnEventRaised += OnRemoveItemStockAt;

        // Can you even listen to your own event?
        m_ItemBought.OnEventRaised += FinishShopping;
    }

    // Unsubscribe to event(s) to avoid errors
    private void OnDisable()
    {
        m_LandOnStorefront.OnEventRaised -= EnterStorefront;
        m_ExitStorefront.OnEventRaised -= ExitStorefront;
        m_HoveringItem.OnEventRaised -= HighlightItem;
        m_StockItems.OnEventRaised -= StockItems;

        m_TryBuyItemAt.OnEventRaised -= OnTryBuyItemAt;
        m_RemoveItemAt.OnEventRaised -= OnRemoveItemStockAt;

        m_ItemBought.OnEventRaised -= FinishShopping;
    }

    // Set dependencies here and in Inspector (if needed)
    private void Start()
    {
        storeChatTypewriter = storeChatBubble.GetComponent<TypewriterCore>();

        storefrontFullGroup.alpha = 0f;
        storefrontFullGroup.interactable = false;

        confirmBuyGroup.alpha = 0f;
        confirmBuyGroup.interactable = false;
    }

    private void UpdateItemsInStore(MapNode node)
    {
        var store = node.GetComponent<StoreManager>();
        //var storeItemParentTransform = storeItemGridContainer.transform;
        stockedItems.Clear();

        foreach (ItemStats item in store.storeInventory)
        {
            stockedItems.Add(item);
        }

        for (int i = 0; i < stockedItems.Count; i++)
        {

            var item = storeItemHolders[i];
            //item.transform.localScale = Vector3.one;
            var storeHandler = item.GetComponent<StoreSelectionHandler>();
            storeHandler.UpdateItemInfo(stockedItems[i]);
            storeHandler.itemIndex = i;
            //storeHandler.button.interactable = true;

            //storeItemHolders.Add(item);

            if (storeHandler.HeldItem != null &&
                currentPlayer.heldPoints < storeHandler.HeldItem.basePrice) // Disable player from buying if too expensive
            {
                //storeHandler.GetComponent<Button>().enabled = false;
                //DisableItemSelection(i);
            }

            if (i == 0)
            {
                Debug.Log($"ID: {storeHandler.itemIndex} | {storeHandler.HeldItem}");
                EventSystem.current.SetSelectedGameObject(item);
            }
        }
    }

    private void SpawnItemsInStore(MapNode node)
    {
        var store = node.GetComponent<StoreManager>();

        Debug.Log("spawning");
        storeItemHolders.Clear();
        var storeItemParentTransform = storeItemGridContainer.transform;
        stockedItems.Clear();

        foreach(ItemStats item in store.storeInventory)
        {
            stockedItems.Add(item);
        }
        //if (itemsToSpawn <= INVENTORY_LIMIT)
        //{
        //    itemsToSpawn = INVENTORY_LIMIT;
        //}

        for (int i = 0; i < stockedItems.Count; i++)
        {
            var item = Instantiate(storeItemPrefab, storeItemParentTransform);
            //item.transform.localScale = Vector3.one;
            var storeHandler = item.GetComponent<StoreSelectionHandler>();
            storeHandler.UpdateItemInfo(stockedItems[i]);
            storeHandler.itemIndex = i;

            storeItemHolders.Add(item);

            if (storeHandler.HeldItem != null &&
                currentPlayer.heldPoints < storeHandler.HeldItem.basePrice) // Disable player from buying if too expensive
            {
                //storeHandler.GetComponent<Button>().enabled = false;
                //DisableItemSelection(i);
            }

            if (i == 0)
            {
                Debug.Log($"ID: {storeHandler.itemIndex} | {storeHandler.HeldItem}");
                EventSystem.current.SetSelectedGameObject(item);
            }
        }

        if (!stockedItems.Where(item => item != null).ToList().
            Exists(item => currentPlayer.heldPoints >= item.basePrice))
        {
            // Note: Need to display Death's Row notice somehow. Maybe have an icon in the overworld?
            //currentPlayer.isInDeathsRow = true;
            currentPlayer.currentStates.Add(EntityPiece.State.DeathsRow);
            // Force player to buy cheapest item in the store.
            var cheapestItem = stockedItems.Where(item => item != null).
                OrderBy(i => i.basePrice).FirstOrDefault();
            // Note: I don't think the SPACE bar prompt is displaying. UI Issue.
            EnableItemSelection(stockedItems.FindIndex(item => item == cheapestItem));
        }
    }

    private void DestroyAllStoreItems()
    {
        // Clear all the items upon leaving the store
        var storeItemParentTransform = storeItemGridContainer.transform;

        for (int i = storeItemParentTransform.childCount - 1; i >= 0; i--)
        {
            Destroy(storeItemParentTransform.GetChild(i).gameObject);
        }
    }

    private void EnterStorefront(MapNode mapNode)
    {
        confirmBuyGroup.alpha = 0f;
        confirmBuyGroup.interactable = false;

        storefrontFullGroup.alpha = 1f;
        storefrontFullGroup.interactable = true;

        currentStore = mapNode.GetComponent<StoreManager>();
        currentPlayer = GameplayTest.instance.currentPlayer; //mapNode.playerOccupied;

        UpdateItemsInStore(mapNode);

        //storeChatBubble.text = "\"Greetings, customer! Welcome to " + currentStore.playerOwner.entityName + "'s wonderful store! \nPlease purchase something.\"";
        var greeting = "\"Greetings, customer! Welcome to " + currentStore.playerOwner.entityName + "'s wonderful store! \nPlease purchase something.\"";
        storeChatTypewriter.ShowText(greeting);

        // Set colors
        //storekeeperImage.color = currentStore.playerOwner.playerColor;
        storeboat.color = currentStore.playerOwner.playerColor;
        customerVisual.color = currentPlayer.playerColor;

        MoveStoreboat();
        MoveCustomerBoat();
        /*
        StockItems(stockedItems);

        // If no item exists that is affordable to the player, enter Death's Row.
        if (!stockedItems.Where(item => item != null).ToList().
            Exists(item => currentPlayer.heldPoints >= item.basePrice))
        {
            // Note: Need to display Death's Row notice somehow. Maybe have an icon in the overworld?
            currentPlayer.isInDeathsRow = true;
            // Force player to buy cheapest item in the store.
            var cheapestItem = stockedItems.Where(item => item != null).
                OrderBy(i => i.basePrice).FirstOrDefault();
            // Note: I don't think the SPACE bar prompt is displaying. UI Issue.
            EnableItemSelection(stockedItems.FindIndex(item => item == cheapestItem));
        }
        */
    }

    private void MoveCustomerBoat()
    {
        //var startingPos = new Vector2(160, -55);
        var startingPos = new Vector2(320, -75);

        var motion = LMotion.Create(startingPos, customerRect.anchoredPosition, 0.4f)
            //.WithEase(Ease.InQuad)
            .WithEase(Ease.OutBack)
            .BindToAnchoredPosition(customerRect);
    }

    private void MoveStoreboat()
    {
        var storeboatRect = storeboat.GetComponent<RectTransform>();
        //var startingPos = new Vector2(-60, 20);
        var startingPos = new Vector2(240, 20);

        var motion = LMotion.Create(startingPos, storeboatRect.anchoredPosition, 0.35f)
            //.WithEase(Ease.InQuad)
            .WithEase(Ease.OutBack)
            .BindToAnchoredPosition(storeboatRect);
    }

    private void ExitStorefront()
    {
        Debug.Log("ExitStorefront guh?");
        // Enable selection of items upon finishing a shopping sesh.
        EnableItemSelections();
        //DestroyAllStoreItems();
        //storefrontCanvas.enabled = false;
        //confirmBuyGroup.gameObject.SetActive(false);
        confirmBuyGroup.alpha = 0f;
        confirmBuyGroup.interactable = false;
        //storefrontCanvas.gameObject.SetActive(false);

        storefrontFullGroup.alpha = 0f;
        storefrontFullGroup.interactable = false;
    }

    private void FinishShopping(ItemStats item)
    {
        // Disable buying of all other items.
        DisableItemSelections();

        //if (currentPlayer.currentStates.Contains(EntityPiece.State.DeathsRow))
        if (currentPlayer.currentStates.Contains(EntityPiece.State.DeathsRow))
            storeChatBubble.text = "\"You have received " + item.itemName + ". \n Unfortunately, you've just entered <color=red>DEBT'S ROW</color>.\"";
        else if (item != null)
            storeChatBubble.text = "\"Enjoy your brand new " + item.itemName + "! \nThank you for your patronage, and we hope to see you very soon!\"";
        else
            storeChatBubble.text = "\"I'm sorry but you cannot afford the "+ item.itemName + ".\"";
    }

    private void HighlightItem(ItemStats item)
    {
        // Changes the chat bubble to show the information of the selected item in the store
        if (item == null)
        {
            // There is no item in that spot
            var emptyStock = "<size=36><color=red>SOLD OUT</color></size>";
            emptyStock += "<color=yellow><sprite=\"Coin Icon\" index=0> ----</color>\n";
            emptyStock += "<size=36>No more stock left.\n\n";
            emptyStock += "<color=grey>\"Come back another time when we refill it!\"</color></size>";

            storeChatTypewriter.ShowText(emptyStock);
        }
        else
        {

            var flavor = item.l_flavor.GetLocalizedString();
            //var typewriter = storeChatBubble.GetComponent<TypewriterCore>();

            //storeChatBubble.text = flavor;
            storeChatTypewriter.ShowText(flavor);
            /*
            storeChatBubble.text = "<size=36><color=lightblue>" + item.itemName + "</color></size>";
            storeChatBubble.text += "<color=yellow><sprite=\"Coin Icon\" index=0>" + item.basePrice + "</color>\n";
            storeChatBubble.text += "" + item.effectDescription + "\n\n";
            storeChatBubble.text += "<color=grey>" + item.flavorText + "</color>";
            */
        }
    }
    private void OnTryBuyItemAt(int i)
    {
        selectedStoreHandler = storeItemHolders[i].GetComponent<StoreSelectionHandler>();
        var item = selectedStoreHandler.HeldItem;

        var confirmBuyText = $"\"Buy the {item.itemName}? It costs <color=yellow>{item.basePrice}</color><sprite=\"Coin Icon\" index=0>.\"";
        if(currentPlayer.heldPoints <= item.basePrice)
        {
            confirmBuyText += $"\n\n<color=red>[WARNING]</color> You don't have enough <sprite=\"Coin Icon\" index=0>, so buying this will put you in <color=red>Debt's Row</color>!";
        }

        storeChatTypewriter.ShowText(confirmBuyText);

        //confirmBuyGroup.gameObject.SetActive(true);
        ShowConfirmBuyGroup();
        EventSystem.current.SetSelectedGameObject(confirmYesButton);
    }

    // Button Functions
    public void ConfirmBuyItem()
    {
        // YES
        m_RemoveItemAt.RaiseEvent(selectedStoreHandler.itemIndex);
    }

    public void CancelBuyItem()
    {
        // NO
        selectedStoreHandler.isCurrentlySelected = false;

        HideConfirmBuyGroup();
        EventSystem.current.SetSelectedGameObject(selectedStoreHandler.gameObject);
    }

    public void ShowConfirmBuyGroup()
    {
        confirmBuyGroup.alpha = 1f;
        confirmBuyGroup.interactable = true;


    }

    public void HideConfirmBuyGroup()
    {
        confirmBuyGroup.alpha = 0f;
        confirmBuyGroup.interactable = false;
    }

    private void OnRemoveItemStockAt(int i)
    {
        // Try to buy an item
        var selectedStoreItem = selectedStoreHandler.HeldItem;

        if (selectedStoreItem != null || currentPlayer.currentStates.Contains(EntityPiece.State.DeathsRow) ||
            currentPlayer.heldPoints >= selectedStoreItem.basePrice)
        {
            // Item is buyable, buy it and remove the item from the store
            //var itemImage = itemInventoryImages[i];

            //itemImage.sprite = null;
            //itemImage.color = new Color(itemImage.color.r, itemImage.color.g, itemImage.color.b, 0);

            // May need to move the rest of the following code to another script

            // Signal that this item was sold.
            // Likely for the PlayerManager to subtract currency based off item's price.

            currentPlayer.inventory.Add(selectedStoreItem);
            currentStore.playerOwner.heldPoints += selectedStoreItem.basePrice;

            // Update store owner's score.
            m_UpdatePlayerScore.RaiseEvent(currentStore.playerOwner.id);

            //Broadcast the player's score difference for the sound effect
            m_PlayerScoreDecreased.RaiseEvent(currentPlayer.heldPoints - selectedStoreItem.basePrice);

            currentStore.storeInventory[i] = null;
            m_ItemBought.RaiseEvent(selectedStoreItem);

            HideConfirmBuyGroup();
        }
        else
        {
            // Not enough money, or doesn't meet the requirements
            Debug.Log("Not enough money to buy " + selectedStoreItem + " from the store.");
            storeChatBubble.text = "\"I'm sorry but you cannot buy this item.\"";
            //m_ItemBought.RaiseEvent(null);
        }
        
    }

    private void StockItems(List<ItemStats> items)
    {
        // Updates all the items in the store based off what the store manager contains
        for (int i = 0; i < items.Count; i++)
        {
            var itemImage = itemInventoryImages[i];
            if (stockedItems[i] != null)
            {
                itemImage.sprite = items[i].itemSprite;
                itemImage.color = new Color(itemImage.color.r, itemImage.color.g, itemImage.color.b, 255);

                if (currentPlayer.heldPoints < stockedItems[i].basePrice)
                {
                    DisableItemSelection(i);
                }
            }
            else
            {
                // make the sprite not visible to the player
                itemImage.color = new Color(itemImage.color.r, itemImage.color.g, itemImage.color.b, 0);
                DisableItemSelection(i);
            }
            // temp code, prob remove this later
            stockedItems[i] = items[i];         
        }
    }

    // Functions to enable/disable item selection.
    // The interactability of buttons determines the items' opacity.
    // The selection handlers determine the "popping out" effect of the items on hover.
    // The selection triggers determine the actual functionality of "selecting an item."

    private void EnableItemSelection(int index)
    {
        storeItemHolders[index].GetComponent<Button>().interactable = true;
        EventSystem.current.SetSelectedGameObject(storeItemHolders[index]);
        /*
        itemButtons[index].interactable = true;
        itemSelectionHandlers[index].enabled = true;
        itemSelectionTriggers[index].enabled = true;
        */
    }

    private void EnableItemSelections()
    {
        foreach (var button in itemButtons)
        {
            button.interactable = true;
        }

        foreach (var itemSelectionHandler in itemSelectionHandlers)
        {
            itemSelectionHandler.enabled = true;
        }

        foreach (var itemSelectionTrigger in itemSelectionTriggers)
        {
            itemSelectionTrigger.enabled = true;
        }
    }

    private void DisableItemSelection(int index)
    {
        itemButtons[index].interactable = false;
        itemSelectionHandlers[index].enabled = false;
        itemSelectionTriggers[index].enabled = false;
    }

    private void DisableItemSelections()
    {
        foreach (var button in itemButtons)
        {
            button.interactable = false;
        }

        foreach (var itemSelectionHandler in itemSelectionHandlers)
        {
            itemSelectionHandler.enabled = false;
        }

        foreach (var itemSelectionTrigger in itemSelectionTriggers)
        {
            itemSelectionTrigger.enabled = false;
        }
    }

    private void ChangeInCurrency(EntityPiece ps)
    {
        // Update the text visually according to player's remaining currency and the item's price
        // PROBABLY PUT THIS IN A DIFFERENT LISTENER SCRIPT (like a Scorekeeper Listener script for UI)

    }
}
