using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class StoreSelectionHandler : MonoBehaviour, ISubmitHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("Item Info")]
    public int itemIndex;
    [SerializeField] private ItemStats heldItem;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemPrice;

    [Header("Broadcast On Event")]
    public ItemEventChannelSO m_HightlightItem; // basically hovering on item in inv

    public IntEventChannelSO m_BuyItemAt;
    public ItemEventChannelSO m_ItemBought;

    public ItemStats HeldItem
    {
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
        transform.localScale = Vector3.one;
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
            itemPrice.text = "<color=red>SOLD!</color>";

            GetComponent<Button>().interactable = false;
        }
        else
        {
            itemIcon.sprite = item.itemSprite;
            itemIcon.enabled = true;
            itemPrice.text = $"<color=#FFC900>@</color>{item.basePrice}";

            GetComponent<Button>().interactable = true;
        }
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (heldItem == null) return;

        // Attempt to buy item event
        Debug.Log($"{heldItem.name} bought");
        itemIcon.sprite = null;
        itemIcon.enabled = false;
        itemPrice.text = "<color=red>SOLD!</color>";

        GetComponent<Button>().interactable = false;
        m_BuyItemAt.RaiseEvent(itemIndex);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        /*
        Debug.Log("click");

        m_ItemUsed.RaiseEvent(itemIndex);

        itemIcon.sprite = null;
        itemIcon.enabled = false;
        itemName.text = "";
        itemPrice.text = "";

        GetComponent<Button>().interactable = false;
        */
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
        m_HightlightItem.RaiseEvent(heldItem);
        //StartCoroutine(MoveItem(true));
    }

    public void OnDeselect(BaseEventData eventData)
    {
        //StartCoroutine(MoveItem(false));
    }

    private IEnumerator MoveItem(bool startingAnimation)
    {
        Vector3 endPosition = startPos;
        Vector3 endScale = startScale;

        float elapsedTime = 0f;
        while (elapsedTime < moveTime)
        {
            elapsedTime += Time.deltaTime;

            if (startingAnimation)
            {
                endPosition = startPos + new Vector3(0f, verticalMoveAmount, 0f);
                endScale = startScale * scaleAmount;
            }
            else
            {
                endPosition = startPos;
                endScale = startScale;
            }
        }

        // Calc lerp amounts
        Vector3 lerpedPos = Vector3.Lerp(transform.position, endPosition, (elapsedTime / moveTime));
        Vector3 lerpedScale = Vector3.Lerp(transform.position, endScale, (elapsedTime / moveTime));

        // Apply changes to the position and scale
        transform.position = lerpedPos;
        transform.localScale = lerpedScale;

        yield return null;
    }
}
