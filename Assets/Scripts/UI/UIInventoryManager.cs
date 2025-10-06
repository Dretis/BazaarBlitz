using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;

public class UIInventoryManager : MonoBehaviour
{
    public enum InventoryState
    {
        UseItem,
        RestockStore,
        Discard,
        DropItem
    }
    //[SerializeField] private List<ItemStats> playerInventory; //effectively copy of player's inv
    //[SerializeField] private EntityPiece currentPlayer; //reference of player

    private int currentPlayerIndex;

    [Header("Main Inventory")]
    [SerializeField] private Canvas inventoryCanvas;
    [SerializeField] private CanvasGroup inventoryGroup;
    [SerializeField] private RectTransform inventoryGridContainer;
    [SerializeField] private List<Image> itemIcons;
    [SerializeField] private List<TextMeshProUGUI> itemNames;
    [SerializeField] private List<TextMeshProUGUI> itemPrices;

    [Header("Selected Item Details")]
    [SerializeField] private Image selectedItemIcon;
    [SerializeField] private List<TextMeshProUGUI> selectedItemInfo;

    [Header("Inventory Instruction Details")]
    [SerializeField] private CanvasGroup instructionGroup;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private TextMeshProUGUI inventoryInputPromptText;

    [Header("Storestock Inventory")]
    [SerializeField] private CanvasGroup storestockGroup;
    [SerializeField] private List<Image> storestockIcons;
    [SerializeField] private List<TextMeshProUGUI> storestockNames;
    [SerializeField] private List<TextMeshProUGUI> storestockPrices;
    [SerializeField] private List<ItemStats> storeInv;
    [SerializeField] private TextMeshProUGUI storestockTooltip;

    [Header("Extra Space")]
    [SerializeField] private CanvasGroup extraGroup;
    [SerializeField] private List<Image> extraIcons;
    [SerializeField] private List<TextMeshProUGUI> extraNames;
    [SerializeField] private List<TextMeshProUGUI> extraPrices;
    [SerializeField] private List<ItemStats> extraInv;

    [Header("Confirmation Space")]
    [SerializeField] private CanvasGroup confirmGroup;
    [SerializeField] private List<GameObject> confirmButtonHolders;
    [SerializeField] private TextMeshProUGUI confirmText;

    [Header("Inventory Stats")]
    [SerializeField] private InventoryState currentState;
    //[SerializeField] private int currentItemCount;
    [SerializeField] private int discardedItemCount;
    [SerializeField] private const int INVENTORY_LIMIT = 8;
    [SerializeField] private GameObject heldItemPrefab;
    [SerializeField] private List<GameObject> heldItemHolders;

    private int selectedItemIndex;
    private ItemStats selectedItem;

    [Header("Broadcast on Event Channels")]
    public IntItemEventChannelSO m_ItemUsed;

    [Header("Listen on Event Channels")]
    public PlayerEventChannelSO m_OpenInventory;
    public VoidEventChannelSO m_ExitInventory;
    public PlayerEventChannelSO m_RefreshInventory;
    public NodeEventChannelSO m_RestockStore;

    public IntItemEventChannelSO m_ItemStocked;
    public IntItemEventChannelSO m_ItemDiscarded;
    public ItemEventChannelSO m_ItemSelected;

    public IntItemEventChannelSO m_TryUseItemAt;

    public PlayerEventChannelSO m_FullInventory;


    private void OnEnable()
    {
        inventoryGroup.alpha = 0;
        inventoryGroup.interactable = false;

        storestockGroup.alpha = 0;

        confirmGroup.interactable = false;
        confirmGroup.alpha = 0f;

        instructionGroup.alpha = 0;

        m_OpenInventory.OnEventRaised += DisplayInventory;
        m_ExitInventory.OnEventRaised += HideInventory;
        m_ExitInventory.OnEventRaised += HideStoreStock;
        m_ExitInventory.OnEventRaised += HideConfirmGroup;
        m_RefreshInventory.OnEventRaised += RefreshInventory;
        m_RestockStore.OnEventRaised += OnRestockStore;

        m_ItemStocked.OnEventRaised += AddItemToStoreStock;
        m_ItemDiscarded.OnEventRaised += OnItemDiscarded;
        m_ItemSelected.OnEventRaised += ShowSelectedItemDetails;

        m_TryUseItemAt.OnEventRaised += OnTryUseItemAt;

        m_FullInventory.OnEventRaised += OnFullInventory;
    }

    private void OnDisable()
    {
        m_OpenInventory.OnEventRaised -= DisplayInventory;
        m_ExitInventory.OnEventRaised -= HideInventory;
        m_ExitInventory.OnEventRaised -= HideStoreStock;
        m_ExitInventory.OnEventRaised -= HideConfirmGroup;
        m_RefreshInventory.OnEventRaised -= RefreshInventory;
        m_RestockStore.OnEventRaised -= OnRestockStore;
        
        m_ItemStocked.OnEventRaised -= AddItemToStoreStock;
        m_ItemDiscarded.OnEventRaised -= OnItemDiscarded;
        m_ItemSelected.OnEventRaised -= ShowSelectedItemDetails;

        m_TryUseItemAt.OnEventRaised -= OnTryUseItemAt;

        m_FullInventory.OnEventRaised -= OnFullInventory;
    }

    private void SpawnItemsInInventory(List<ItemStats> playerInventory)
    {
        Debug.Log("spawning");
        heldItemHolders.Clear();
        var inventoryParentTransform = inventoryGridContainer.transform;
        var itemsToSpawn = playerInventory.Count;

        if (itemsToSpawn <= INVENTORY_LIMIT)
        {
            itemsToSpawn = INVENTORY_LIMIT;
        }
        else
        {
            itemsToSpawn = itemsToSpawn = playerInventory.Count + (playerInventory.Count % 2);
        }

        ItemStats itemToSpawn;
        for (int i = 0; i < itemsToSpawn; i++)
        {
            if (i < playerInventory.Count)
            {
                itemToSpawn = playerInventory[i];
            }
            else
            {
                itemToSpawn = null;
            }

            var item = Instantiate(heldItemPrefab, inventoryParentTransform);
            item.GetComponent<InventorySelectionHandler>().UpdateItemInfo(itemToSpawn);
            item.GetComponent<InventorySelectionHandler>().itemIndex = i;

            heldItemHolders.Add(item);

            if (i == 0)
            {
                EventSystem.current.SetSelectedGameObject(item);
            }
        }
    }

    private void DisplayInventory(EntityPiece entity)
    {
        Debug.Log("wtf how");

        FadeTo(inventoryGroup, 1, 0.25f);

        inventoryGroup.interactable = true;
        inventoryInputPromptText.text = "<sprite=0> Use Item\r\n<sprite=1> Exit";

        //inventoryCanvas.enabled = true;
        HideSelectedItemDetails();

        SpawnItemsInInventory(entity.inventory);
        currentPlayerIndex = entity.id;
        /*
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
        */
    }

    private void HideInventory()
    {
        FadeTo(inventoryGroup, 0, 0.25f);
        inventoryGroup.interactable = false;
        instructionGroup.alpha = 0;
        //inventoryCanvas.enabled = false;

        DestroyAllHeldItems();
    }

    private void RefreshInventory(EntityPiece entity)
    {
        //DestroyAllHeldItems();
        //SpawnItemsInInventory(entity.inventory);

        if(GameplayTest.instance.phase == GameplayTest.GamePhase.StockStore)
        {
            instructionText.text = $"<color=#FFEC75>Store resupply!\r\nStock up to {GameplayTest.instance.emptyStockCount} more items.";
        }

        var playerInventory = entity.inventory;
        ItemStats itemToSpawn;

        var itemsToSpawn = playerInventory.Count;

        if (itemsToSpawn <= INVENTORY_LIMIT)
        {
            itemsToSpawn = INVENTORY_LIMIT;
        }
        else
        {
            itemsToSpawn = playerInventory.Count + (playerInventory.Count % 2);
        }
        //i < INVENTORY_LIMIT

        for (int i = 0; i < itemsToSpawn; i++)
        {
            if (i < playerInventory.Count)
            {
                itemToSpawn = playerInventory[i];
            }
            else
            {
                itemToSpawn = null;
            }

            heldItemHolders[i].GetComponent<InventorySelectionHandler>().UpdateItemInfo(itemToSpawn);
        }
        EventSystem.current.SetSelectedGameObject(heldItemHolders[0]);
    }

    private void DestroyAllHeldItems()
    {
        // Gets rid of all the held item containers in the inventory UI

        var inventoryParentTransform = inventoryGridContainer.transform;

        for (int i = inventoryParentTransform.childCount - 1; i >= 0; i--)
        {
            Destroy(inventoryParentTransform.GetChild(i).gameObject);
        }
    }

    public void DeactivateItemAt(int index)
    {

         for (int i = 0; i < itemNames.Count; i++)
         {
             // Make the rest of the inventory interactable, prevents multiple item uses in one turn
             itemNames[i].GetComponentInParent<Button>().interactable = false;
         }
    }

    public void AddItemToStoreStock(int index, ItemStats item)
    {
        //var item = currentPlayer.inventory[index];
        // Restocking Items in an owned store.
        int storeTotalIndex = 0;

        // Find the first empty spot/item in the store's inventory
        for (int i = 0; i < storeInv.Count; i++)
        {
            if (storeInv[i] == null)
            {
                storeInv[i] = item;
                storeTotalIndex = i;
                Debug.Log($"Found a null slot at {storeTotalIndex}");
                break;
            }
        }

        // Visually update that spot in the Restock UI
        storestockIcons[storeTotalIndex].sprite = item.itemSprite;
        storestockIcons[storeTotalIndex].enabled = true;
        storestockNames[storeTotalIndex].text = $"{item.itemName}";
        storestockPrices[storeTotalIndex].text = $"<sprite=\"Coin Icon\" index=0>{item.basePrice}";
    }

    public void ShowSelectedItemDetails(ItemStats item)
    {
        if (item == null)
            return;

        selectedItemIcon.sprite = item.itemSprite;
        selectedItemIcon.enabled = true;

        selectedItemInfo[0].text = $"{item.itemName}";
        selectedItemInfo[1].text = $"<sprite=\"Coin Icon\" index=0>{item.basePrice}";
        selectedItemInfo[2].text = $"{item.effectDescription}";
        selectedItemInfo[3].text = $"{item.flavorText}";
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

    public void OnRestockStore(MapNode node)
    {
        ShowStoreStock(node);

        instructionText.text = $"<color=#FFEC75>Store resupply!\r\nStock up to {GameplayTest.instance.emptyStockCount} more items.";
        inventoryInputPromptText.text = "<sprite=0> Stock Item\r\n<sprite=1> Finish Stocking";
        instructionGroup.alpha = 1;
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
                storestockPrices[i].text = $"<sprite=\"Coin Icon\" index=0>{storeItem.basePrice}";

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
        DOTween.Kill(group.gameObject);
        DOTween.To(() => group.alpha, x => group.alpha = x, alphaValue, duration);
    }

    public void OnTryUseItemAt(int index, ItemStats item)
    {
        ShowConfirmGroup();

        confirmText.text = $"Use {item.itemName}?";

        selectedItemIndex = index;
        selectedItem = item;

        EventSystem.current.SetSelectedGameObject(confirmButtonHolders[0]);
    }

    public void ConfirmUseItem()
    {
        HideConfirmGroup();
        OnUseItemAt(selectedItemIndex, selectedItem);
    }

    public void CancelUseItem()
    {
        HideConfirmGroup();

        EventSystem.current.SetSelectedGameObject(heldItemHolders[selectedItemIndex]);
    }

    public void OnUseItemAt(int index, ItemStats item)
    {
        HideConfirmGroup();

        //UseItem(selectedItemIndex);
        HideInventory();

        //Tell everything that this item is being used
        m_ItemUsed.RaiseEvent(index, item);

    }

    public void ShowConfirmGroup()
    {
        confirmGroup.interactable = true;
        FadeTo(confirmGroup, 1, 0.25f);
    }
    public void HideConfirmGroup()
    {
        confirmGroup.interactable = false;
        FadeTo(confirmGroup, 0, 0.25f);
    }
    public void OnItemDiscarded(int index, ItemStats item)
    {
        discardedItemCount--;

        instructionText.text = $"<color=#FF8A8A>Inventory full!\r\nChoose {discardedItemCount} items to discard.</color>";

        if ( discardedItemCount <= 0)
        {
            instructionText.text = "";
            instructionGroup.alpha = 0;

            m_ExitInventory.RaiseEvent();
            //GameplayTest.instance.phase = GameplayTest.GamePhase.EndTurn; // CHANGE THIS LATER PLZ
        }
    }

    public void OnFullInventory(EntityPiece player)
    {
        discardedItemCount = player.inventory.Count - player.inventoryLimit;

        DisplayInventory(player);

        inventoryInputPromptText.text = "<sprite=0> <color=#FF8A8A>Discard Item</color>";
        instructionGroup.alpha = 1;
        m_RefreshInventory.RaiseEvent(player);


        if (discardedItemCount > 0) 
        {
            instructionText.text = $"<color=#FF8A8A>Inventory full!\r\nChoose {discardedItemCount} items to discard.</color>";
        }
    }
}
