using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Coffee.UIEffects;
using Core;

public class InventorySelectionHandler : MonoBehaviour, ISubmitHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    private UIInventoryManager _uiInventoryManager;
    private EntityData assignedEntityData;
    [Header("Item Info")]
    public int itemIndex;
    [SerializeField] private ItemStats heldItem;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemPrice;
    [SerializeField] private bool _itemIsUnusable = false;

    [Header("Broadcast On Event")]
    //public IntItemEventChannelSO m_TryUseItemAt; // aka m_ItemUsed or "I want to Use Inventory Item At"
    //public IntItemEventChannelSO m_ItemStocked;
    public IntItemEventChannelSO m_ItemDiscarded;
    //public ItemEventChannelSO m_ItemSelected; // basically hovering on item in inv

    public ItemStats HeldItem { 
        get { return heldItem; } 
        set { heldItem = value; }
    }

    
    [Header("UI Elements")]
    [SerializeField] private Image heldItemContainer;
    [SerializeField] private float verticalMoveAmount = 30f;
    [SerializeField] private float moveTime = 0.1f;
    [Range(0f, 2f), SerializeField] private float scaleAmount = 1.1f;

    [SerializeField] private UIEffect containerPattern;

    private Vector3 startPos;
    private Vector3 startScale;

    public void Initialize(EntityData ed, UIInventoryManager uiInvManager)
    {
        _uiInventoryManager = uiInvManager;
        assignedEntityData = ed;
    }

    private void Start()
    {
        startPos = transform.position;
        startScale = transform.localScale;
    }

    public void UpdateItemInfo(ItemStats item)
    {
        heldItem = item;

        bool playerInCombat = (assignedEntityData.Statuses.Contains(Status.Fighting) || assignedEntityData.Statuses.Contains(Status.FightingParty));

        //Debug.Log($"{item} | playerInCombat = {playerInCombat}");
        //Debug.Log($"{item} | Inventory Phase = {GameplayTest.instance.phase == GameplayTest.GamePhase.Inventory}");
        //Debug.Log($"{item} | fighting state = {entity.currentStates.Contains(EntityPiece.State.Fighting)}");

        if (item == null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
            itemName.text = "";
            itemPrice.text = "";

            GetComponent<Button>().interactable = false;
        }
        else if (playerInCombat && !item.usableInCombat)
        {
            // Cannot use this item atm, grey it out
            itemIcon.sprite = item.itemSprite;
            itemName.text = $"{item.l_itemName.GetLocalizedString()}";

            GreyOutItemSelectionVisual();
            GetComponent<Button>().interactable = true;
        }
        else
        {
            itemIcon.sprite = item.itemSprite;
            itemIcon.enabled = true;

            //itemName.text = $"{item.itemName}";
            itemName.text = $"{item.l_itemName.GetLocalizedString()}";
            itemPrice.text = $"<sprite=\"Coin Icon\" index=0>{item.basePrice}";
            _itemIsUnusable = false;

            ResetItemSelectionVisual();
            GetComponent<Button>().interactable = true;
        }
    }

    public void OnSubmit(BaseEventData eventData)
    {
        Debug.Log($"OnSubmit | InventorySelectionHandler |{eventData}");
        if (heldItem == null) return;

        if(_uiInventoryManager.CurrentState is UIInventoryManager.InventoryState.Restock) 
        {
            if (_itemIsUnusable) return;

            // Stock Item into Store
            Debug.Log($"{heldItem.name} stocked");
            //itemIcon.sprite = null;
            //itemIcon.enabled = false;
            //itemName.text = "";
            //itemPrice.text = "";

            //GetComponent<Button>().interactable = false;
            //m_ItemStocked.RaiseEvent(itemIndex, heldItem);
            GreyOutItemSelectionVisual();
            _uiInventoryManager.AddToExpectedItemsToStock(itemIndex, heldItem);
        }
        else if(_uiInventoryManager.CurrentState is UIInventoryManager.InventoryState.Discard)
        {
            Debug.Log($"{heldItem.name} discarded");

            itemIcon.sprite = null;
            itemIcon.enabled = false;
            itemName.text = "";
            itemPrice.text = "";

            GetComponent<Button>().interactable = false;
            m_ItemDiscarded.RaiseEvent(itemIndex, heldItem);
        }
        else if (_itemIsUnusable)
        {
            Debug.Log($"{heldItem.name} cannot be used right now!");
        }
        else if (_uiInventoryManager.CurrentState is UIInventoryManager.InventoryState.UseItem)
        {
            // Use Item
            Debug.Log($"Trying to use {heldItem.name}");
            /*
            itemIcon.sprite = null;
            itemIcon.enabled = false;
            itemName.text = "";
            itemPrice.text = "";

            GetComponent<Button>().interactable = false;
            */
            //m_TryUseItemAt.RaiseEvent(itemIndex, heldItem);
            _uiInventoryManager.OnTryUseItemAt(itemIndex, heldItem);
        }
        else
        {
            Debug.Log($"{name} - InventorySelectionHandler | Something went wrong!");
        }
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
        //StartCoroutine(MoveItem(true));
        // Display Detailed Item Information
        //m_ItemSelected.RaiseEvent(heldItem);
        _uiInventoryManager.ShowSelectedItemDetails(HeldItem);

        containerPattern.transitionAutoPlaySpeed = 1;
        containerPattern.transitionColorFilter = ColorFilter.MultiplyAdditive;
        //Debug.Log("selected ???");
    }

    public void OnDeselect(BaseEventData eventData)
    {
        //StartCoroutine(MoveItem(false));
        containerPattern.transitionAutoPlaySpeed = 0f;
        containerPattern.transitionColorFilter = ColorFilter.Multiply;
    }

    private void GreyOutItemSelectionVisual()
    {
        // Cannot use this item atm, grey it out
        itemIcon.color = Color.grey;
        itemIcon.enabled = true;

        itemName.color = Color.grey;

        itemPrice.text = $"<sprite=\"Coin Icon\" index=0 tint=1>{heldItem.basePrice}";
        itemPrice.color = Color.grey;

        heldItemContainer.color = Color.grey;
        _itemIsUnusable = true;
        //GetComponent<Button>().interactable = true;
    }
    
    private void ResetItemSelectionVisual()
    {
        // Cannot use this item atm, grey it out
        itemIcon.color = Color.white;
        itemIcon.enabled = true;

        itemName.color = Color.white;

        itemPrice.text = $"<sprite=\"Coin Icon\" index=0 tint=1>{heldItem.basePrice}";
        itemPrice.color = Color.white;

        heldItemContainer.color = Color.white;
    }
}
