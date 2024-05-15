using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class InventorySelectionHandler : MonoBehaviour, ISubmitHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("Item Info")]
    public int itemIndex;
    [SerializeField] private ItemStats heldItem;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemPrice;

    [Header("Broadcast On Event")]
    public IntEventChannelSO m_ItemUsed;
    public ItemEventChannelSO m_ItemSelected; // basically hovering on item in inv

    public ItemStats HeldItem { 
        get { return heldItem; } 
        set { heldItem = value; }
    }

    [SerializeField] private float verticalMoveAmount = 30f;
    [SerializeField] private float moveTime = 0.1f;
    [Range(0f, 2f), SerializeField] private float scaleAmount = 1.1f;

    private Vector3 startPos;
    private Vector3 startScale;

    private void Start()
    {
        startPos = transform.position;
        startScale = transform.localScale;
    }

    public void UpdateItemInfo(ItemStats item)
    {
        heldItem = item;
        if (item == null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
            itemName.text = "";
            itemPrice.text = "";

            GetComponent<Button>().interactable = false;
        }
        else
        {
            itemIcon.sprite = item.itemSprite;
            itemIcon.enabled = true;
            itemName.text = $"{item.itemName}";
            itemPrice.text = $"<color=#C3B789>@</color>{item.basePrice}";

            GetComponent<Button>().interactable = true;
        }
    }

    public void OnSubmit(BaseEventData eventData)
    {
        Debug.Log("submit");

        m_ItemUsed.RaiseEvent(itemIndex);

        itemIcon.sprite = null;
        itemIcon.enabled = false;
        itemName.text = "";
        itemPrice.text = "";

        GetComponent<Button>().interactable = false;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("click");

        m_ItemUsed.RaiseEvent(itemIndex);

        itemIcon.sprite = null;
        itemIcon.enabled = false;
        itemName.text = "";
        itemPrice.text = "";

        GetComponent<Button>().interactable = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Select item
        eventData.selectedObject = gameObject;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        eventData.selectedObject = null;
    }

    public void OnSelect(BaseEventData eventData)
    {
        //StartCoroutine(MoveItem(true));
        // Display Detailed Item Information
        m_ItemSelected.RaiseEvent(heldItem);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        //StartCoroutine(MoveItem(false));
    }
}
