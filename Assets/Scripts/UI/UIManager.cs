using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Canvas storefrontCanvas;
    [SerializeField] private TextMeshProUGUI storeChatBubble;

    // May move this to another script
    [Header("Store Stock")]
    [SerializeField] private List<Image> itemInventoryImages;
    [SerializeField] private List<ItemStats> itemInventory;

    [Header("Broadcast on Event Channels")]
    public VoidEventChannelSO broadcastEvent;
    public ItemEventChannelSO m_itemSold;

    [Header("Listen on Event Channels")]
    public VoidEventChannelSO m_LandOnStorefront;
    public VoidEventChannelSO m_ExitStorefront;
    public ItemEventChannelSO m_HoveringItem;
    public ItemListEventChannelSO m_StockItems;
    public IntEventChannelSO m_RemoveItem; // change it to RemoveItem, may need this to be an IntEventChannel


    // Subscribe to event(s)
    private void OnEnable()
    {
        m_LandOnStorefront.OnEventRaised += EnterStorefront;
        m_ExitStorefront.OnEventRaised += ExitStorefront;
        m_HoveringItem.OnEventRaised += HighlightItem;
        m_StockItems.OnEventRaised += StockItems;
        m_RemoveItem.OnEventRaised += RemoveItemStockAt;
    }

    // Unsubscribe to event(s) to avoid errors
    private void OnDisable()
    {
        m_LandOnStorefront.OnEventRaised -= EnterStorefront;
        m_ExitStorefront.OnEventRaised -= ExitStorefront;
        m_HoveringItem.OnEventRaised -= HighlightItem;
        m_StockItems.OnEventRaised -= StockItems;
        m_RemoveItem.OnEventRaised -= RemoveItemStockAt;
    }

    // Set dependencies here and in Inspector (if needed)
    private void Start()
    {
        
    }

    private void EnterStorefront()
    {
        storefrontCanvas.gameObject.SetActive(true);
        storefrontCanvas.enabled = true;
        // storefrontCanvas.enabled = !storefrontCanvas.enabled;


    }

    private void ExitStorefront()
    {
        storefrontCanvas.enabled = false;
        storefrontCanvas.gameObject.SetActive(false);


    }
    private void HighlightItem(int i)
    {
        // Changes the chat bubble to show the information of the selected item in the store
        if (itemInventory[i] == null)
        {
            // There is no item in that spot
            storeChatBubble.text = "<color=red>SOLD OUT</color>  ";
            storeChatBubble.text += "<color=yellow>@ NULL</color>\n";
            storeChatBubble.text += "<size=36>No more stock left.\n\n";
            storeChatBubble.text += "<color=grey>\"Come back another time when we refill it!\"</color></size>";
        }
        else
        {
            storeChatBubble.text = "<color=lightblue>" + itemInventory[i].itemName + "</color>  ";
            storeChatBubble.text += "<color=yellow>@" + itemInventory[i].basePrice + "</color>\n";
            storeChatBubble.text += "<size=36>" + itemInventory[i].effectDescription + "\n\n";
            storeChatBubble.text += "<color=grey>" + itemInventory[i].flavorText + "</color></size>";
        }
    }

    private void HighlightItem(ItemStats item)
    {
        // Changes the chat bubble to show the information of the selected item in the store
        if(item == null)
        {
            // There is no item in that spot
            storeChatBubble.text = "<color=red>SOLD OUT</color>  ";
            storeChatBubble.text += "<color=yellow>@ NULL</color>\n";
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
        // Visually remove the item from the list of items in the store
        var itemImage = itemInventoryImages[i];

        itemImage.sprite = null;
        itemImage.color = new Color(itemImage.color.r, itemImage.color.g, itemImage.color.b, 0);

        Debug.Log("Removed " + itemImage + " sprite from the store.");

        // May need to move the rest of the following code to another script

        // Signal that this item was sold.
        // Likely for the PlayerManager to subtract currency based of item's price.
        m_itemSold.RaiseEvent(itemInventory[i]);

        itemInventory[i] = null;
    }

    private void StockItems(List<ItemStats> items)
    {
        // Updates all the items in the store based off what the store manager contains
        for (int i = 0; i < items.Count; i++)
        {
            itemInventoryImages[i].sprite = items[i].itemSprite;

            // temp code, prob remove this later
            itemInventory[i] = items[i];
        }
    }

    private void ChangeInCurrency(PlayerStats ps)
    {
        // Update the text visually according to player's remaining currency and the item's price
        // PROBABLY PUT THIS IN A DIFFERENT LISTENER SCRIPT (like a Scorekeeper Listener script for UI)

    }
}
