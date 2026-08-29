using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using LitMotion;
using LitMotion.Extensions;

public class StoreSelectionHandler : MonoBehaviour, ISubmitHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public bool isCurrentlySelected;

    [Header("Item Info")]
    public int itemIndex;
    [SerializeField] private ItemStats heldItem;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemPrice;

    [SerializeField] private StoreManager associatedStore;
    [SerializeField] private int associatedItemPrice;

    private RectTransform itemRect;
    public Button button;

    [Header("Hover Item Elements")]
    [SerializeField] private CanvasGroup hoverGroup;
    [SerializeField] private TextMeshProUGUI hoverItemName;
    [SerializeField] private TextMeshProUGUI hoverEffect;
    private RectTransform hoverRect;

    [Header("Broadcast On Event")]
    public ItemEventChannelSO m_HoverItemInStorefront; // basically hovering on item in inv

    public IntEventChannelSO m_TryBuyItemAt;
    public IntEventChannelSO m_RemoveItemAt;
    public ItemEventChannelSO m_ItemBought;

    public ItemStats HeldItem
    {
        get { return heldItem; }
        set { heldItem = value; }
    }

    public StoreManager AssociatedStore
    {
        get { return associatedStore; }
        set { associatedStore = value; }
    }

    public int AssociatedItemPrice
    {
        get { return associatedItemPrice; }
        set { associatedItemPrice = value; }
    }

    [SerializeField] private float verticalMoveAmount = 30f;
    [SerializeField] private float moveTime = 0.1f;
    [Range(0f, 2f), SerializeField] private float scaleAmount = 1.1f;

    private Vector3 startPos;
    private Vector3 startScale;
    private void OnEnable()
    {
        m_RemoveItemAt.OnEventRaised += OnRemoveItemStockAt;
        //m_ItemBought.OnEventRaised += OnItemBought;
    }

    private void OnDisable()
    {
        m_RemoveItemAt.OnEventRaised -= OnRemoveItemStockAt;
        //m_ItemBought.OnEventRaised -= OnItemBought;
    }

    private void Awake()
    {
        transform.localScale = Vector3.one;
        startPos = transform.position;
        startScale = transform.localScale;

        itemRect = itemIcon.GetComponent<RectTransform>();

        //hoverGroup.alpha = 0;

        hoverRect = hoverGroup.GetComponent<RectTransform>();
        hoverRect.localScale = Vector3.zero;

        button = GetComponent<Button>();
        button.interactable = false;
    }

    public void UpdateItemInfo(ItemStats item, StoreManager store)
    {

        heldItem = item;
        associatedStore = store;
        //associatedItemPrice = (int)(item.basePrice * associatedStore.storePriceMultiplier);
        if (item == null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
            itemPrice.text = "<color=red>SOLD!</color>";

            hoverItemName.text = "";
            hoverEffect.text = "There is nothing there.";

            button.interactable = true;
        }
        else
        {
            associatedItemPrice = (int)(item.basePrice * associatedStore.storePriceMultiplier);
            itemIcon.sprite = item.itemSprite;
            itemIcon.enabled = true;
            itemPrice.text = $"<sprite=\"Coin Icon\" index=0>{associatedItemPrice}";

            hoverItemName.text = heldItem.l_itemName.GetLocalizedString();
            hoverEffect.text = heldItem.l_effect.GetLocalizedString();

            button.interactable = true;
        }
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (heldItem == null) return;
        Debug.Log($"Trying to buy {heldItem.name}");

        // Attempt to buy item event
        isCurrentlySelected = true;
        m_TryBuyItemAt.RaiseEvent(itemIndex);

        //m_RemoveItemAt.RaiseEvent(itemIndex);
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
        //eventData.selectedObject = null;
    }

    public void OnSelect(BaseEventData eventData)
    {
        m_HoverItemInStorefront.RaiseEvent(heldItem);
        HoverItem();
        //StartCoroutine(MoveItem(true));
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (isCurrentlySelected) return;

        UnhoverItem();
        //StartCoroutine(MoveItem(false));
    }
    private void OnRemoveItemStockAt(int index)
    {
        button.interactable = false;

        if (itemIndex != index) return;

        button.interactable = false;

        itemIcon.sprite = null;
        itemIcon.enabled = false;
        itemPrice.text = "<color=red>SOLD!</color>";
        button.interactable = false;

        isCurrentlySelected = false;

        UnhoverItem();
    }

    private void HoverItem()
    {
        if(heldItem == null) return;

        var motion = LMotion.Create(itemRect.anchoredPosition, new Vector2(0, 75), 0.25f)
            //.WithEase(Ease.InQuad)
            .WithEase(Ease.OutBack)
            .BindToAnchoredPosition(itemRect);

        LMotion.Create(itemRect.localScale, Vector3.one * 1.2f, 0.25f)
                .WithEase(Ease.OutBack)
                .Bind(x => itemRect.localScale = x);

        LMotion.Create(hoverRect.localScale, Vector3.one * 0.8f, 0.25f)
                .WithEase(Ease.OutBack)
                .Bind(x => hoverRect.localScale = x);

        //itemSpriteMat.SetFloat("_ShakeUvSpeed", 2);
    }

    private void UnhoverItem()
    {
        if (heldItem == null) return;

        var motion = LMotion.Create(itemRect.anchoredPosition, Vector2.zero, 0.25f)
            //.WithEase(Ease.InQuad)
            .WithEase(Ease.OutBack)
            .BindToAnchoredPosition(itemRect);

        LMotion.Create(itemRect.localScale, Vector3.one, 0.25f)
                .WithEase(Ease.OutBack)
                .Bind(x => itemRect.localScale = x);

        //hoverEffect.text = heldItem.l_effect.ToString();

        LMotion.Create(hoverRect.localScale, Vector3.zero, 0.25f)
                .WithEase(Ease.OutQuad)
                .Bind(x => hoverRect.localScale = x);

        //itemSpriteMat.SetFloat("_ShakeUvSpeed", 0);
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
