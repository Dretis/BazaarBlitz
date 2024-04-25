using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    // CHANGE THIS SCRIPTS NAME, THIS ONE IS ONLY HANDLING STOREFRONT UI
    [Header("UI Elements")]
    [SerializeField] private Canvas storefrontCanvas;
    [SerializeField] private List<Button> itemButtons;
    [SerializeField] private List<ItemSelectionHandler> itemSelectionHandlers;
    [SerializeField] private List<EventTrigger> itemSelectionTriggers;
    [SerializeField] private TextMeshProUGUI storeChatBubble;
    [SerializeField] private Image storekeeperImage;

    // May move this to another script
    [Header("Store Stock")]
    [SerializeField] private List<Image> itemInventoryImages;
    [SerializeField] private List<ItemStats> itemInventory;

    [Header("Broadcast on Event Channels")]
    public ItemEventChannelSO m_itemSold;
    public IntEventChannelSO m_UpdatePlayerScore;

    [Header("Listen on Event Channels")]
    public NodeEventChannelSO m_LandOnStorefront;
    public VoidEventChannelSO m_ExitStorefront;
    public ItemEventChannelSO m_HoveringItem;
    public ItemListEventChannelSO m_StockItems;
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
        m_RemoveItem.OnEventRaised += RemoveItemStockAt;

        // Can you even listen to your own event?
        m_itemSold.OnEventRaised += FinishShopping;
    }

    // Unsubscribe to event(s) to avoid errors
    private void OnDisable()
    {
        m_LandOnStorefront.OnEventRaised -= EnterStorefront;
        m_ExitStorefront.OnEventRaised -= ExitStorefront;
        m_HoveringItem.OnEventRaised -= HighlightItem;
        m_StockItems.OnEventRaised -= StockItems;
        m_RemoveItem.OnEventRaised -= RemoveItemStockAt;

        m_itemSold.OnEventRaised -= FinishShopping;
    }

    // Set dependencies here and in Inspector (if needed)
    private void Start()
    {
        
    }

    private void EnterStorefront(MapNode mapNode)
    {
        storefrontCanvas.gameObject.SetActive(true);
        storefrontCanvas.enabled = true;
        // storefrontCanvas.enabled = !storefrontCanvas.enabled;
        currentStore = mapNode.GetComponent<StoreManager>();
        currentPlayer = mapNode.playerOccupied;
        
        for (int i = 0; i < itemInventory.Count; i++)
        {
            itemInventory[i] = currentStore.storeInventory[i];
        }

        storeChatBubble.text = "\"Greetings, customer! Welcome to " + currentStore.playerOwner.entityName + "'s wonderful store! \nPlease purchase something.\"";
        storekeeperImage.color = currentStore.playerOwner.playerColor;
        StockItems(itemInventory);

        // If no item exists that is affordable to the player, enter Death's Row.
        if (!itemInventory.Where(item => item != null).ToList().Exists(item => currentPlayer.heldPoints >= item.basePrice))
        {
            // Note: Need to display Death's Row notice somehow. Maybe have an icon in the overworld?
            currentPlayer.isInDeathsRow = true;
            // Force player to buy cheapest item in the store.
            var cheapestItem = itemInventory.Where(item => item != null).OrderBy(i => i.basePrice).FirstOrDefault();
            // Note: I don't think the SPACE bar prompt is displaying. UI Issue.
            RemoveItemStockAt(itemInventory.FindIndex(i => i == cheapestItem));
        }
    }

    private void ExitStorefront()
    {
        // Enable selection of items upon finishing a shopping sesh.
        EnableItemSelection();

        storefrontCanvas.enabled = false;
        storefrontCanvas.gameObject.SetActive(false);
    }

    private void FinishShopping(ItemStats item)
    {
        // Disable buying of all other items.
        DisableItemSelection();

        if (currentPlayer.isInDeathsRow)
            storeChatBubble.text = "\"You have received " + item.itemName +". \n Unfortunately, you've just entered DEATH'S ROW.\"";
        else if (item != null)
            storeChatBubble.text = "\"Enjoy your brand new " + item.itemName + "! \nThank you for your patronage, and we hope to see you very soon!\"";
        else
            storeChatBubble.text = "\"I'm sorry but you cannot afford this item.\"";
    }

    public void HighlightItem(int i)
    {
        // Changes the chat bubble to show the information of the selected item in the store
        if (itemInventory[i] == null)
        {
            // There is no item in that spot
            storeChatBubble.text = "<color=red>SOLD OUT</color>  ";
            storeChatBubble.text += "<color=yellow>@ ----</color>\n";
            storeChatBubble.text += "<size=36>No more stock left.\n\n";
            storeChatBubble.text += "<color=grey>\"Come back another time when we refill it!\"</color></size>";
        }
        else
        {
            storeChatBubble.text = "<color=lightblue><size=54>" + itemInventory[i].itemName + "</color>  ";
            storeChatBubble.text += "<color=yellow>@" + itemInventory[i].basePrice + "</color></size>\n";
            storeChatBubble.text += "<size=36>" + itemInventory[i].effectDescription + "</size>\n\n";
            storeChatBubble.text += "<color=grey>" + itemInventory[i].flavorText + "</color>";
        }
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

    private void RemoveItemStockAt(int i)
    {
        if (currentPlayer.isInDeathsRow || currentPlayer.heldPoints >= itemInventory[i].basePrice)
        {
            // Visually remove the item from the list of items in the store
            var itemImage = itemInventoryImages[i];

            itemImage.sprite = null;
            itemImage.color = new Color(itemImage.color.r, itemImage.color.g, itemImage.color.b, 0);

            Debug.Log("Removed the " + itemImage + " sprite from the store.");

            // May need to move the rest of the following code to another script

            // Signal that this item was sold.
            // Likely for the PlayerManager to subtract currency based off item's price.

            m_itemSold.RaiseEvent(itemInventory[i]);

            currentPlayer.inventory.Add(itemInventory[i]);
            currentStore.playerOwner.heldPoints += itemInventory[i].basePrice;

            // Update store owner's score.
            m_UpdatePlayerScore.RaiseEvent(currentStore.playerOwner.id);

            itemInventory[i] = null;
            currentStore.storeInventory[i] = null;
        }
        else
        {
            Debug.Log("Not enough money to buy " + itemInventory[i] + " from the store.");
            m_itemSold.RaiseEvent(null);
        }
        
    }

    private void StockItems(List<ItemStats> items)
    {
        // Updates all the items in the store based off what the store manager contains
        for (int i = 0; i < items.Count; i++)
        {
            var itemImage = itemInventoryImages[i];
            if(itemInventory[i] != null)
            {
                itemImage.sprite = items[i].itemSprite;
                itemImage.color = new Color(itemImage.color.r, itemImage.color.g, itemImage.color.b, 255);
            }
            else
            {
                // make the sprite not visible to the player
                itemImage.color = new Color(itemImage.color.r, itemImage.color.g, itemImage.color.b, 0);
            }
            // temp code, prob remove this later
            itemInventory[i] = items[i];
        }
    }
    
    // Functions to enable/disable item selection.
    // The interactability of buttons determines the items' opacity.
    // The selection handlers determine the "popping out" effect of the items on hover.
    // The selection triggers determine the actual functionality of "selecting an item."
    private void EnableItemSelection()
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

    private void DisableItemSelection()
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
