using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIInventoryManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private List<TextMeshProUGUI> itemNames;
    [SerializeField] private Canvas inventoryCanvas;

    [Header("Broadcast on Event Channels")]
    public IntEventChannelSO m_ItemUsed;

    [Header("Listen on Event Channels")]
    public PlayerEventChannelSO m_OpenInventory;
    public VoidEventChannelSO m_ExitInventory;

    private void OnEnable()
    {
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
        inventoryCanvas.enabled = true;

        var playerInv = entity.inventory;
        ItemStats playerItem = null;

        for (int i = 0; i < itemNames.Count; i++)
        {
            if(i < playerInv.Count)
            {
                playerItem = playerInv[i];
            }

            if (playerItem == null || i >= playerInv.Count)
            {
                itemNames[i].text = "";
                itemNames[i].GetComponentInParent<Button>().interactable = false;
            }
            else
            {
                itemNames[i].text = $"{playerItem.itemName} <size=16>{playerItem.effectDescription}</size>";
                itemNames[i].GetComponentInParent<Button>().interactable = true;
            }
        }
    }

    private void HideInventory()
    {
        inventoryCanvas.enabled = false;
    }

    public void UseItem(int index)
    {
        // Tell listeners that the item at this index in player's inventory is being used
        m_ItemUsed.RaiseEvent(index);

        // Re-used code, change later
        itemNames[index].text = "";
        itemNames[index].GetComponentInParent<Button>().interactable = false;
    }
}
