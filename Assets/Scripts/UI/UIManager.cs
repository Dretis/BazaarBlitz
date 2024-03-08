using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class UIManager : Subject
{
    [Header("UI Elements")]
    [SerializeField] private Canvas storefrontCanvas;
    [SerializeField] private TextMeshProUGUI storeChatBubble;

    // May move this to another script
    [Header("Store Stock")]
    [SerializeField] private List<Image> itemInventoryImages;

    [Header("Broadcast on Event Channels")]
    public VoidEventChannelSO broadcastEvent;

    [Header("Listen on Event Channels")]
    public VoidEventChannelSO m_LandOnStorefront;
    public ItemEventChannelSO m_HoveringItem;
    public IntEventChannelSO m_RemoveItem; // change it to RemoveItem, may need this to be an IntEventChannel

    // Subscribe to event(s)
    private void OnEnable()
    {
        m_LandOnStorefront.OnEventRaised += EnterStorefront;
        m_HoveringItem.OnEventRaised += HighlightItem;
        m_RemoveItem.OnEventRaised += RemoveItemStockAt;
    }

    // Unsubscribe to event(s) to avoid errors
    private void OnDisable()
    {
        m_LandOnStorefront.OnEventRaised -= EnterStorefront;
        m_HoveringItem.OnEventRaised -= HighlightItem;
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
        var item = itemInventoryImages[i];

        item.sprite = null;
        item.color = new Color(item.color.r, item.color.g, item.color.b, 0);

        Debug.Log("Removed " + item + " sprite from the store.");
    }

    private void ChangeInCurrency(PlayerStats ps)
    {
        // Update the text visually according to player's remaining currency and the item's price
        // PROBABLY PUT THIS IN A DIFFERENT LISTENER SCRIPT (like a Scorekeeper Listener script for UI)

    }
}
