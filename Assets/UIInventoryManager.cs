using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using static UnityEditor.Progress;

public class UIInventoryManager : MonoBehaviour
{
    [SerializeField] private List<ItemStats> playerInventory;

    [Header("Main Inventory")]
    [SerializeField] private Canvas inventoryCanvas;
    [SerializeField] private CanvasGroup inventoryGroup;
    [SerializeField] private List<Image> itemIcons;
    [SerializeField] private List<TextMeshProUGUI> itemNames;
    [SerializeField] private List<TextMeshProUGUI> itemPrices;

    [Header("Selected Item Details")]
    [SerializeField] private Image selectedItemIcon;
    [SerializeField] private List<TextMeshProUGUI> selectedItemInfo;

    [Header("Broadcast on Event Channels")]
    public IntEventChannelSO m_ItemUsed;

    [Header("Listen on Event Channels")]
    public PlayerEventChannelSO m_OpenInventory;
    public VoidEventChannelSO m_ExitInventory;

    private void OnEnable()
    {
        inventoryGroup.alpha = 0;
        m_OpenInventory.OnEventRaised += DisplayInventory;
        m_ExitInventory.OnEventRaised += HideInventory;
    }

    private void OnDisable()
    {
        m_OpenInventory.OnEventRaised -= DisplayInventory;
        m_ExitInventory.OnEventRaised -= HideInventory;
    }

    private void DisplayInventory(EntityPiece entity)
    {
        DOTween.To(() => inventoryGroup.alpha, x => inventoryGroup.alpha = x, 1, 0.25f);

        //inventoryCanvas.enabled = true;
        HideSelectedItemDetails();

        playerInventory.Clear();
        foreach (ItemStats item in entity.inventory)
        {
            playerInventory.Add(item);
        }

        ItemStats playerItem = null;

        for (int i = 0; i < itemNames.Count; i++)
        {
            if(i < playerInventory.Count)
            {
                playerItem = playerInventory[i];
            }

            if (playerItem == null || i >= playerInventory.Count)
            {
                itemIcons[i].sprite = null;
                itemIcons[i].enabled = false;
                itemNames[i].text = "";
                itemPrices[i].text = "";

                itemNames[i].GetComponentInParent<Button>().interactable = false;
            }
            else
            {
                itemIcons[i].sprite = playerItem.itemSprite;
                itemIcons[i].enabled = true;
                itemNames[i].text = $"{playerItem.itemName}";
                itemPrices[i].text = $"<color=#C3B789>@</color>{playerItem.basePrice}";

                itemNames[i].GetComponentInParent<Button>().interactable = true;
            }
        }
    }

    private void HideInventory()
    {
        DOTween.To(() => inventoryGroup.alpha, x => inventoryGroup.alpha = x, 0, 0.25f);
        //inventoryCanvas.enabled = false;
    }

    public void UseItem(int index)
    {
        // Tell listeners that the item at this index in player's inventory is being used
        m_ItemUsed.RaiseEvent(index);

        playerInventory[index] = null;

        if (!GameplayTest.instance.isStockingStore)
        {
            itemIcons[index].sprite = null;
            itemIcons[index].enabled = false;
            itemNames[index].text = "";
            itemPrices[index].text = "";
            for (int i = 0; i < itemNames.Count; i++)
            {
                // Make the rest of the inventory interactable, prevents multiple item uses in one turn
                itemNames[i].GetComponentInParent<Button>().interactable = false;
            }
        }
    }

    public void ShowSelectedItemDetails(int index)
    {
        if (index >= playerInventory.Count || playerInventory[index] == null)
            return;

        selectedItemIcon.sprite = playerInventory[index].itemSprite;
        selectedItemIcon.enabled = true;

        selectedItemInfo[0].text = $"{playerInventory[index].itemName}";
        selectedItemInfo[1].text = $"<color=#C3B789>@</color>{playerInventory[index].basePrice}";
        selectedItemInfo[2].text = $"{playerInventory[index].effectDescription}";
        selectedItemInfo[3].text = $"{playerInventory[index].flavorText}";
    }

    public void HideSelectedItemDetails()
    {
        selectedItemIcon.sprite = null;
        selectedItemIcon.enabled = false;

        selectedItemInfo[0].text = "";
        selectedItemInfo[1].text = "";
        selectedItemInfo[2].text = "";
        selectedItemInfo[3].text = "";
    }
}
