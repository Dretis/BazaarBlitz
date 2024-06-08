using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.EventSystems;
using static UnityEditor.Progress;

public class UIManager : MonoBehaviour
{
    // CHANGE THIS SCRIPTS NAME, THIS ONE IS ONLY HANDLING STOREFRONT UI
    [Header("UI Visual Elements")]
    [SerializeField] private Canvas storefrontCanvas;
    [SerializeField] private List<Button> itemButtons;
    [SerializeField] private List<ItemSelectionHandler> itemSelectionHandlers;
    [SerializeField] private List<EventTrigger> itemSelectionTriggers;
    [SerializeField] private TextMeshProUGUI storeChatBubble;
    [SerializeField] private Image storekeeperImage;

    [SerializeField] private CanvasGroup confirmButtonGroup;
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
    public IntEventChannelSO m_RemoveItem; 
    
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
        m_RemoveItem.OnEventRaised += RemoveItemStockAt;

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
        m_RemoveItem.OnEventRaised -= RemoveItemStockAt;

        m_ItemBought.OnEventRaised -= FinishShopping;
    }

    // Set dependencies here and in Inspector (if needed)
    private void Start()
    {
        
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
            currentPlayer.isInDeathsRow = true;
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
        confirmButtonGroup.gameObject.SetActive(false);
        storefrontCanvas.gameObject.SetActive(true);
        storefrontCanvas.enabled = true;
        // storefrontCanvas.enabled = !storefrontCanvas.enabled;
        currentStore = mapNode.GetComponent<StoreManager>();
        currentPlayer = mapNode.playerOccupied;

        SpawnItemsInStore(mapNode);

        /*
        for (int i = 0; i < itemInventory.Count; i++)
        {
            itemInventory[i] = currentStore.storeInventory[i];
        }
        */

        storeChatBubble.text = "\"Greetings, customer! Welcome to " + currentStore.playerOwner.entityName + "'s wonderful store! \nPlease purchase something.\"";
        storekeeperImage.color = currentStore.playerOwner.playerColor;

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

    private void ExitStorefront()
    {
        // Enable selection of items upon finishing a shopping sesh.
        EnableItemSelections();
        DestroyAllStoreItems();
        storefrontCanvas.enabled = false;
        confirmButtonGroup.gameObject.SetActive(false);
        storefrontCanvas.gameObject.SetActive(false);
    }

    private void FinishShopping(ItemStats item)
    {
        // Disable buying of all other items.
        DisableItemSelections();

        if (currentPlayer.isInDeathsRow)
            storeChatBubble.text = "\"You have received " + item.itemName +". \n Unfortunately, you've just entered DEATH'S ROW.\"";
        else if (item != null)
            storeChatBubble.text = "\"Enjoy your brand new " + item.itemName + "! \nThank you for your patronage, and we hope to see you very soon!\"";
        else
            storeChatBubble.text = "\"I'm sorry but you cannot afford this item.\"";
    }

    private void HighlightItem(ItemStats item)
    {
        // Changes the chat bubble to show the information of the selected item in the store
        if (item == null)
        {
            // There is no item in that spot
            storeChatBubble.text = "<color=red>SOLD OUT</color>  ";
            storeChatBubble.text += "<color=yellow>@ ----</color>\n";
            storeChatBubble.text += "<size=36>No more stock left.\n\n";
            storeChatBubble.text += "<color=grey>\"Come back another time when we refill it!\"</color></size>";
        }
        else
        {
            storeChatBubble.text = "<color=lightblue>" + item.itemName + "</color>  ";
            storeChatBubble.text += "<color=yellow>@" + item.basePrice + "</color>\n";
            storeChatBubble.text += "<size=36>" + item.effectDescription + "\n\n";
            storeChatBubble.text += "<color=grey>" + item.flavorText + "</color></size>";
        }
    }
    private void OnTryBuyItemAt(int i)
    {
        var selectedStoreItem = storeItemHolders[i].GetComponent<StoreSelectionHandler>().HeldItem;

        storeChatBubble.text = $"Buy {selectedStoreItem.itemName}?";
        storeChatBubble.text += $"\nIt costs <color=yellow>@{selectedStoreItem.basePrice}</color>.";
        if(currentPlayer.heldPoints <= selectedStoreItem.basePrice)
        {
            storeChatBubble.text += $"\n\nWARNING: You will be in <color=red>Debt's Row</color> upon buying!!!";
        }

        confirmButtonGroup.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(confirmYesButton);
    }

    private void RemoveItemStockAt(int i)
    {
        // Try to buy an item
        var selectedStoreItem = storeItemHolders[i].GetComponent<StoreSelectionHandler>().HeldItem;
        if (selectedStoreItem != null || currentPlayer.isInDeathsRow ||
            currentPlayer.heldPoints >= selectedStoreItem.basePrice)
        {
            // Item is buyable, buy it and remove the item from the store
            //var itemImage = itemInventoryImages[i];

            //itemImage.sprite = null;
            //itemImage.color = new Color(itemImage.color.r, itemImage.color.g, itemImage.color.b, 0);

            // May need to move the rest of the following code to another script

            // Signal that this item was sold.
            // Likely for the PlayerManager to subtract currency based off item's price.

            m_ItemBought.RaiseEvent(selectedStoreItem);
            currentPlayer.inventory.Add(selectedStoreItem);
            currentStore.playerOwner.heldPoints += selectedStoreItem.basePrice;

            // Update store owner's score.
            m_UpdatePlayerScore.RaiseEvent(currentStore.playerOwner.id);

            //Broadcast the player's score difference for the sound effect
            m_PlayerScoreDecreased.RaiseEvent(currentPlayer.heldPoints - selectedStoreItem.basePrice);

            selectedStoreItem = null;
            currentStore.storeInventory[i] = null;
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
