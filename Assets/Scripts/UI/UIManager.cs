using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class UIManager : Subject
{
    [SerializeField] private Canvas storefrontCanvas;
    [SerializeField] private TextMeshProUGUI storeChatBubble;

    [Header("Broadcast on Event Channels")]
    public VoidEventChannelSO broadcastEvent;

    [Header("Listen on Event Channels")]
    public VoidEventChannelSO m_LandOnStorefront;
    public ItemEventChannelSO m_HoveringItem;

    // Subscribe to event(s)
    private void OnEnable()
    {
        m_LandOnStorefront.OnEventRaised += ShowStorefront;
        m_HoveringItem.OnEventRaised += HighlightItem;
    }

    // Unsubscribe to event(s) to avoid errors
    private void OnDisable()
    {
        m_LandOnStorefront.OnEventRaised -= ShowStorefront;
        m_HoveringItem.OnEventRaised -= HighlightItem;
    }

    // Set dependencies here and in Inspector (if needed)
    private void Start()
    {
        
    }

    private void ShowStorefront()
    {
        storefrontCanvas.enabled = !storefrontCanvas.enabled;
    }

    private void HighlightItem(ItemStats item)
    {
        storeChatBubble.text = "<color=lightblue>" + item.itemName + "</color>  ";
        storeChatBubble.text += "<color=yellow>@" + item.basePrice + "</color>\n";
        storeChatBubble.text += "<size=36>" + item.effectDescription +"\n\n";
        storeChatBubble.text += "<color=grey>" + item.flavorText + "</color></size>";
        /*
        storeChatBubble.SetText("<color=lightblue>Big Gear </color> <color=yellow>@ 2200</color> \n"
            + "<size=36> Increases  <sprite=1> damage by 4 on your next  <sprite=1> attack.\n"
            + "<color=grey>Something, adawdadw something something something something... something!!?!</color></size>");
        */
    }
}
