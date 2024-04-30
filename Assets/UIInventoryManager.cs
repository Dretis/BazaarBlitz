using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIInventoryManager : MonoBehaviour
{
    [SerializeField] private List<ItemStats> playerInventory; //effectively copy of player's inv

    [Header("Main Inventory")]
    [SerializeField] private Canvas inventoryCanvas;
    [SerializeField] private CanvasGroup inventoryGroup;
    [SerializeField] private List<Image> itemIcons;
    [SerializeField] private List<TextMeshProUGUI> itemNames;
    //[SerializeField] private List<TextMeshProUGUI> itemPrices;

    [Header("Selected Item Details")]
    [SerializeField] private Image selectedItemIcon;
    [SerializeField] private List<TextMeshProUGUI> selectedItemInfo;

    [Header("Storestock Inventory")]
    [SerializeField] private CanvasGroup storestockGroup;
    [SerializeField] private List<Image> storestockIcons;
    [SerializeField] private List<TextMeshProUGUI> storestockNames;
    [SerializeField] private List<TextMeshProUGUI> storestockPrices;
    [SerializeField] private List<ItemStats> storeInv;

    [Header("Broadcast on Event Channels")]
    public IntEventChannelSO m_ItemUsed;

    [Header("Listen on Event Channels")]
    public PlayerEventChannelSO m_OpenInventory;
    public VoidEventChannelSO m_ExitInventory;
    public NodeEventChannelSO m_RestockStore;

    private void OnEnable()
    {
        inventoryGroup.alpha = 0;
        inventoryGroup.interactable = false;

        storestockGroup.alpha = 0;

        m_OpenInventory.OnEventRaised += DisplayInventory;
        m_ExitInventory.OnEventRaised += HideInventory;
        m_ExitInventory.OnEventRaised += HideStoreStock;
        m_RestockStore.OnEventRaised += ShowStoreStock;
    }

    private void OnDisable()
    {
        m_OpenInventory.OnEventRaised -= DisplayInventory;
        m_ExitInventory.OnEventRaised -= HideInventory;
        m_ExitInventory.OnEventRaised -= HideStoreStock;
        m_RestockStore.OnEventRaised -= ShowStoreStock;
    }

    private void DisplayInventory(EntityPiece entity)
    {
        FadeTo(inventoryGroup, 1, 0.25f);
        inventoryGroup.interactable = true;

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
                //itemPrices[i].text = "";

                itemNames[i].GetComponentInParent<Button>().interactable = false;
            }
            else
            {
                itemIcons[i].sprite = playerItem.itemSprite;
                itemIcons[i].enabled = true;
                itemNames[i].text = $"{playerItem.itemName}";
                //itemPrices[i].text = $"<color=#C3B789>@</color>{playerItem.basePrice}";

                itemNames[i].GetComponentInParent<Button>().interactable = true;
            }
        }
    }

    private void HideInventory()
    {
        FadeTo(inventoryGroup, 0, 0.25f);
        inventoryGroup.interactable = false;
        //inventoryCanvas.enabled = false;
    }

    public void UseItem(int index)
    {
        if (!GameplayTest.instance.isStockingStore)
        {
            // Regular item use, visually removes the item from the player's inventory

            itemIcons[index].sprite = null;
            itemIcons[index].enabled = false;
            itemNames[index].text = "";
            //itemPrices[index].text = "";
            for (int i = 0; i < itemNames.Count; i++)
            {
                // Make the rest of the inventory interactable, prevents multiple item uses in one turn
                itemNames[i].GetComponentInParent<Button>().interactable = false;
            }
        }
        else if(GameplayTest.instance.isStockingStore)
        {
            // Restocking Items in an owned store.

            int storeTotalIndex = 0;

            // Find the first empty spot/item in the store's inventory
            for (int i = 0; i < storeInv.Count; i++)
            {
                if (storeInv[i] == null)
                {
                    storeInv[i] = playerInventory[index];
                    storeTotalIndex = i;
                    Debug.Log($"Found a null slot at {storeTotalIndex}");
                    break;
                }
            }

            // Visually update that spot in the Restock UI
            storestockIcons[storeTotalIndex].sprite = playerInventory[index].itemSprite;
            storestockIcons[storeTotalIndex].enabled = true;
            storestockNames[storeTotalIndex].text = $"{playerInventory[index].itemName}";
            storestockPrices[storeTotalIndex].text = $"<color=yellow>@</color>{playerInventory[index].basePrice}";
        }

        // Tell listeners that the item at this index in player's inventory is being used
        m_ItemUsed.RaiseEvent(index);
    }

    public void ShowSelectedItemDetails(int index)
    {
        if (index >= playerInventory.Count || playerInventory[index] == null)
            return;

        selectedItemIcon.sprite = playerInventory[index].itemSprite;
        selectedItemIcon.enabled = true;

        selectedItemInfo[0].text = $"{playerInventory[index].itemName}";
        selectedItemInfo[1].text = $"<color=yellow>@</color>{playerInventory[index].basePrice}";
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

    public void ShowStoreStock(MapNode node)
    {
        Debug.Log("Show store stock");
        FadeTo(storestockGroup, 1, 0.25f);

        var storeInventory = node.GetComponent<StoreManager>().storeInventory;

        storeInv.Clear();
        foreach (ItemStats item in storeInventory)
        {
            storeInv.Add(item);
        }

        DisplayInventory(node.GetComponent<StoreManager>().playerOwner);

        ItemStats storeItem = null;

        for (int i = 0; i < storestockNames.Count; i++)
        {
            if (i < storeInv.Count)
            {
                storeItem = storeInv[i];
            }

            if (storeItem == null || i >= storeInv.Count)
            {
                storestockIcons[i].sprite = null;
                storestockIcons[i].enabled = false;
                storestockNames[i].text = "";
                storestockPrices[i].text = "";

                storestockNames[i].GetComponentInParent<Button>().interactable = false;
            }
            else
            {
                storestockIcons[i].sprite = storeItem.itemSprite;
                storestockIcons[i].enabled = true;
                storestockNames[i].text = $"{storeItem.itemName}";
                storestockPrices[i].text = $"<color=yellow>@</color>{storeItem.basePrice}";

                storestockNames[i].GetComponentInParent<Button>().interactable = false;
            }
        }
    }

    public void HideStoreStock()
    {
        FadeTo(storestockGroup, 0, 0.25f);
    }

    public void FadeTo(CanvasGroup group, float alphaValue, float duration)
    {
        DOTween.To(() => group.alpha, x => group.alpha = x, alphaValue, duration);
    }
}
