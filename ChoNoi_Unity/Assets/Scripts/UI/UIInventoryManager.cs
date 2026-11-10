using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core.Events;
using System;
using UnityEngine.InputSystem.UI;
using Core;
using LitMotion;
using LitMotion.Extensions;

public class UIInventoryManager : MonoBehaviour
{
    private PlayerState _playerState;
    private StorefrontData _selectedStoreData;
    private GameplayTest _gameplayTest;
    private List<ItemStats> _storestockHolder = new List<ItemStats>();
    private Dictionary<int, ItemStats> _expectedItemsToStock = new Dictionary<int, ItemStats>(); // int = index in Player Inventory
    [SerializeField] public InventoryState CurrentState { get; private set; }
    public enum InventoryState
    {
        None, // Inactive
        UseItem,
        Restock,
        Discard,
        DropItem
    }
    //[SerializeField] private List<ItemStats> playerInventory; //effectively copy of player's inv
    //[SerializeField] private EntityPiece currentPlayer; //reference of player

    //private int currentPlayerIndex;
    //private StoreManager selectedStore;
    [SerializeField] private MultiplayerEventSystem _eventSystem;

    [Header("Main Inventory")]
    [SerializeField] private Canvas inventoryCanvas;
    [SerializeField] private CanvasGroup inventoryGroup;
    [SerializeField] private RectTransform inventoryGridContainer;
    [SerializeField] private List<Image> itemIcons;
    [SerializeField] private List<TextMeshProUGUI> itemNames;
    [SerializeField] private List<TextMeshProUGUI> itemPrices;

    [Header("Selected Item Details")]
    [SerializeField] private RectTransform selectedItemRect;
    [SerializeField] private Image selectedItemIcon;
    [SerializeField] private List<TextMeshProUGUI> selectedItemInfo;

    [Header("Inventory Instruction Details")]
    [SerializeField] private CanvasGroup instructionGroup;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private TextMeshProUGUI inventoryInputPromptText;

    [Header("Storestock Inventory")]
    [SerializeField] private CanvasGroup storestockGroup;
    [SerializeField] private TextMeshProUGUI storestockLevelIndicator;
    [SerializeField] private List<Image> storestockIcons;
    [SerializeField] private List<TextMeshProUGUI> storestockNames;
    [SerializeField] private List<TextMeshProUGUI> storestockPrices;
    //[SerializeField] private List<ItemStats> storeInv;
    //[SerializeField] private TextMeshProUGUI storestockTooltip;

    //[Header("Extra Space")]
    //[SerializeField] private CanvasGroup extraGroup;
    //[SerializeField] private List<Image> extraIcons;
    //[SerializeField] private List<TextMeshProUGUI> extraNames;
    //[SerializeField] private List<TextMeshProUGUI> extraPrices;
    //[SerializeField] private List<ItemStats> extraInv;

    [Header("Confirmation Space")]
    [SerializeField] private CanvasGroup _useItemConfirmGroup;
    [SerializeField] private List<GameObject> _useItemConfirmButtonHolders;
    [SerializeField] private TextMeshProUGUI _useItemconfirmText;
    [Space]
    [SerializeField] private CanvasGroup _restockConfirmGroup;
    [SerializeField] private List<GameObject> _restockConfirmButtonHolders;
    //[SerializeField] private TextMeshProUGUI _restockconfirmText;

    [Header("Inventory Stats")]
    //[SerializeField] private int currentItemCount;
    [SerializeField] private int discardedItemCount;
    [SerializeField] private int _emptyStorestockCount = 0;
    [SerializeField] private const int INVENTORY_LIMIT = 8;
    [SerializeField] private GameObject heldItemPrefab;
    [SerializeField] private List<InventorySelectionHandler> heldItemHolders;

    private int _selectedItemIndex;
    private ItemStats _selectedItem;

    [Header("Broadcast on Event Channels")]
    //public IntItemEventChannelSO m_ItemUsed;
    public IntItemEventChannelSO m_RaycastItemUsed;

    [Header("Listen on Event Channels")]
    //public PlayerEventChannelSO m_OpenInventory;
    //public VoidEventChannelSO m_ExitInventory;
    //public PlayerEventChannelSO m_RefreshInventory;
    //public NodeEventChannelSO m_RestockStore;

    //public IntItemEventChannelSO m_ItemStocked;
    public IntItemEventChannelSO m_ItemDiscarded;
    //public ItemEventChannelSO m_ItemSelected;

    //public IntItemEventChannelSO m_TryUseItemAt;

    //public PlayerEventChannelSO m_FullInventory;


    public GameStateEventChannelSO _gameStateEvent;
    public void Initialize(PlayerState ps, GameplayTest gt)
    {
        _playerState = ps;
        _gameplayTest = gt;
        //storestockHolder.
    }

    private void UpdateUIFromGameState(FrameGameState gameState)
    {
        if (gameState.DidChangePhases())
        {
            var ped = gameState.NewState.CurrentPlayer().EntityData;

            if (gameState.PrevState.GamePhase is GameplayTest.GamePhase.Inventory)
            {
                CurrentState = InventoryState.None;
                Debug.Log("Hiding Inventory");
                HideInventory();
                HideStoreStock();
                HideUseItemConfirmGroup();
            }

            if (gameState.NewState.GamePhase is GameplayTest.GamePhase.Inventory)
            {
                CurrentState = InventoryState.UseItem;
                DisplayInventory(ped);
            }
            else
            {
                //HideInventory();
                //HideStoreStock();
                //HideConfirmGroup();
            }

            if (gameState.NewState.GamePhase is GameplayTest.GamePhase.StockingStore)
            {
                if(gameState.PrevState.GamePhase is GameplayTest.GamePhase.IncidentHappening)
                {
                    // After Build Store cutscene
                    CurrentState = InventoryState.Restock;
                    _selectedStoreData = gameState.NewState.BoardNodes[ped.OccupiedNodeId].StorefrontData;
                    DisplayInventory(ped);
                    SetupRestockStore(_selectedStoreData);
                }
                if (gameState.PrevState.GamePhase is GameplayTest.GamePhase.PreStockStore)
                {
                    // After picking Restock during Renovate
                    CurrentState = InventoryState.Restock;
                    _selectedStoreData = gameState.NewState.BoardNodes[gameState.NewState.SelectedBoardNodeId].StorefrontData;
                    DisplayInventory(ped);
                    SetupRestockStore(_selectedStoreData);
                }
            }
            if (gameState.NewState.GamePhase is GameplayTest.GamePhase.PrepStockStore)
            {
                PromptRestockItems();

            }
            else if (gameState.PrevState.GamePhase is GameplayTest.GamePhase.PrepStockStore && 
                     gameState.NewState.GamePhase is GameplayTest.GamePhase.InitialTurnMenu)
            {
                CurrentState = InventoryState.None;
                HideInventory();
                HideStoreStock();
                HideUseItemConfirmGroup();
            }

            if (gameState.PrevState.GamePhase is GameplayTest.GamePhase.DiscardItem)
            {
                CurrentState = InventoryState.None;
                HideInventory();
                HideStoreStock();
                HideUseItemConfirmGroup();
            }
            if (gameState.NewState.GamePhase is GameplayTest.GamePhase.DiscardItem)
            {
                CurrentState = InventoryState.Discard;
                DisplayInventory(ped);
            }
        }
    }

    private void OnEnable()
    {
        inventoryGroup.alpha = 0;
        inventoryGroup.interactable = false;

        storestockGroup.alpha = 0;

        _useItemConfirmGroup.interactable = false;
        _useItemConfirmGroup.alpha = 0f;

        _restockConfirmGroup.interactable = false;
        _restockConfirmGroup.alpha = 0f;

        instructionGroup.alpha = 0;

        //m_OpenInventory.OnEventRaised += DisplayInventory;
        //m_ExitInventory.OnEventRaised += HideInventory;
        //m_ExitInventory.OnEventRaised += HideStoreStock;
        //m_ExitInventory.OnEventRaised += HideConfirmGroup;
        //m_RefreshInventory.OnEventRaised += RefreshInventory;
        //m_RestockStore.OnEventRaised += OnRestockStore;

        //m_ItemStocked.OnEventRaised += AddItemToStoreStock;
        m_ItemDiscarded.OnEventRaised += OnItemDiscarded;
        //m_ItemSelected.OnEventRaised += ShowSelectedItemDetails;

        //m_TryUseItemAt.OnEventRaised += OnTryUseItemAt;

        //m_FullInventory.OnEventRaised += OnFullInventory;

        _gameStateEvent.OnEventRaised += UpdateUIFromGameState;
    }

    private void OnDisable()
    {
        //m_OpenInventory.OnEventRaised -= DisplayInventory;
        //m_ExitInventory.OnEventRaised -= HideInventory;
        //m_ExitInventory.OnEventRaised -= HideStoreStock;
        //m_ExitInventory.OnEventRaised -= HideConfirmGroup;
        //m_RefreshInventory.OnEventRaised -= RefreshInventory;
        //m_RestockStore.OnEventRaised -= OnRestockStore;
        
        //m_ItemStocked.OnEventRaised -= AddItemToStoreStock;
        m_ItemDiscarded.OnEventRaised -= OnItemDiscarded;
        //m_ItemSelected.OnEventRaised -= ShowSelectedItemDetails;

        //m_TryUseItemAt.OnEventRaised -= OnTryUseItemAt;

        //m_FullInventory.OnEventRaised -= OnFullInventory;

        _gameStateEvent.OnEventRaised -= UpdateUIFromGameState;
    }

    private void SpawnItemsInInventory(EntityData ed)
    {
        List<ItemStats> playerInventory = ed.Inventory;
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
            //item.GetComponent<InventorySelectionHandler>().UpdateItemInfo(ed, itemToSpawn);
            //item.GetComponent<InventorySelectionHandler>().itemIndex = i;

            if (item.TryGetComponent<InventorySelectionHandler>(out InventorySelectionHandler ish))
            {
                ish.Initialize(ed, this);
                ish.UpdateItemInfo(itemToSpawn);
                ish.itemIndex = i;
                heldItemHolders.Add(ish);
            }

            if (itemToSpawn != null)
                item.name = $"Held Item #{i + 1}_{itemToSpawn.itemName}";
            else
                item.name = $"Held Item #{i + 1}_EMPTY";

            //heldItemHolders.Add(ish);

            if (i == 0)
            {
                Debug.Log("wtf how");
                _eventSystem.SetSelectedGameObject(item);
            }
        }
    }

    private void DisplayInventory(EntityData ed)
    {
        //Debug.Log("wtf how");

        FadeTo(inventoryGroup, 1, 0.25f);

        inventoryGroup.interactable = true;
        inventoryInputPromptText.text = "<sprite=0> Use Item\r\n<sprite=1> Exit";

        //inventoryCanvas.enabled = true;
        HideSelectedItemDetails();

        SpawnItemsInInventory(ed);
        //currentPlayerIndex = ps.Id;
    }

    private void RefreshInventorySelection()
    {
        foreach(var selection in heldItemHolders)
        {
            selection.UpdateItemInfo(selection.HeldItem);
        }
    }

    private void HideInventory()
    {
        FadeTo(inventoryGroup, 0, 0.25f);
        inventoryGroup.interactable = false;
        instructionGroup.alpha = 0;
        //inventoryCanvas.enabled = false;

        DestroyAllHeldItems();
    }

    private void RefreshInventory(EntityData ed)
    {
        //DestroyAllHeldItems();
        //SpawnItemsInInventory(entity.inventory);

        if(CurrentState is InventoryState.Restock)
        {
            _emptyStorestockCount = 0;
            UpdateRestockInstruction();
        }

        var playerInventory = ed.Inventory;
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
        //_eventSystem.SetSelectedGameObject(heldItemHolders[0]);
        _eventSystem.SetSelectedGameObject(heldItemHolders[0].gameObject);
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

    public void AddToExpectedItemsToStock(int index, ItemStats item)
    {
        //var item = currentPlayer.inventory[index];
        // Restocking Items in an owned store.
        int storeTotalIndex = 0;

        // Find the first empty spot/item in the store's inventory
        for (int i = 0; i < _storestockHolder.Count; i++)
        {
            if (_storestockHolder[i] == null)
            {
                _storestockHolder[i] = item;
                storeTotalIndex = i;
                Debug.Log($"Found a null slot at {storeTotalIndex}");
                break;
            }
        }

        _expectedItemsToStock.Add(index, item);
        // Visually update that spot in the Restock UI
        storestockIcons[storeTotalIndex].sprite = item.itemSprite;
        storestockIcons[storeTotalIndex].enabled = true;
        storestockNames[storeTotalIndex].text = $"{item.l_itemName.GetLocalizedString()}";
        storestockPrices[storeTotalIndex].text = $"<sprite=\"Coin Icon\" index=0>{(int)(item.basePrice * _selectedStoreData.StorePriceMultiplier)}";

        _emptyStorestockCount--;
        UpdateRestockInstruction();

        if(_emptyStorestockCount == 0)
        {
            _gameplayTest.QueueCommand(new UICancelCommand());
            //PromptRestockItems();
        }
    }

    public void ShowSelectedItemDetails(ItemStats item)
    {
        if (item == null)
            return;

        selectedItemIcon.sprite = item.itemSprite;
        selectedItemIcon.enabled = true;

        selectedItemInfo[0].text = $"{item.l_itemName.GetLocalizedString()}";
        selectedItemInfo[1].text = $"<sprite=\"Coin Icon\" index=0>{item.basePrice}";
        selectedItemInfo[2].text = $"{item.l_effect.GetLocalizedString()}";
        selectedItemInfo[3].text = $"{item.l_flavor.GetLocalizedString()}";

        //selectedItemRect
        var cardScaleMotion = LMotion.Create(Vector3.one * 0.95f, Vector3.one, .2f)
            .WithEase(Ease.OutBack)
            .BindToLocalScale(selectedItemRect);
    }

    public void HideSelectedItemDetails()
    {
        //selectedItemIcon.sprite = null;
        selectedItemIcon.enabled = false;

        selectedItemInfo[0].text = "";
        selectedItemInfo[1].text = "";
        selectedItemInfo[2].text = "";
        selectedItemInfo[3].text = "";
    }

    private void UpdateRestockInstruction()
    {
        instructionText.text = $"<color=#FFEC75>Store resupply!\r\nStock up to {_emptyStorestockCount} more items.";
    }

    public void SetupRestockStore(StorefrontData sd)
    {
        _expectedItemsToStock.Clear();
        _emptyStorestockCount = 0;
        //var store = node.GetComponent<StoreManager>();

        //DisplayInventory(sd.OwnerId);
        ShowStoreStock(sd);

        inventoryInputPromptText.text = "<sprite=0> Stock Item\r\n<sprite=1> Finish Stocking";
        instructionGroup.alpha = 1;

        UpdateRestockInstruction();
    }

    public void ShowStoreStock(StorefrontData sd)
    {
        Debug.Log("Show store stock");
        FadeTo(storestockGroup, 1, 0.25f);

        storestockLevelIndicator.text = "";
        for (int i = 0; i < sd.StoreLevel; i++)
        {
            storestockLevelIndicator.text += "*";
        }
        //selectedStore = node.GetComponent<StoreManager>();
        //var storeInventory = node.GetComponent<StoreManager>().storeInventory;
        var index = 0;
        _storestockHolder.Clear();
        //storeInv.Clear();
        //_emptyStorestockCount = 0;
        foreach (ItemStats item in sd.Storestock)
        {
            _storestockHolder.Add(item);
            //storeInv.Add(item);
            index++;
        }

        //DisplayInventory(node.GetComponent<StoreManager>().playerOwner);

        ItemStats storeItem = null;

        for (int i = 0; i < storestockNames.Count; i++)
        {
            if (i < _storestockHolder.Count)
            {
                storeItem = _storestockHolder[i];
            }

            if (storeItem == null || i >= _storestockHolder.Count)
            {
                _emptyStorestockCount++;

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
                //storestockPrices[i].text = $"<sprite=\"Coin Icon\" index=0>{storeItem.basePrice}";
                storestockPrices[i].text = $"<sprite=\"Coin Icon\" index=0>{(int)(storeItem.basePrice * sd.StorePriceMultiplier)}";

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
        //DOTween.Kill(group.gameObject);
        //DOTween.To(() => group.alpha, x => group.alpha = x, alphaValue, duration);

        var motion = LMotion.Create(group.alpha, alphaValue, duration)
                    .WithEase(Ease.OutQuad)
                    .Bind(x => group.alpha = x);
    }

    public void OnTryUseItemAt(int index, ItemStats item)
    {
        ShowUseItemConfirmGroup();

        _useItemconfirmText.text = $"Use {item.l_itemName.GetLocalizedString()}?";

        _selectedItemIndex = index;
        _selectedItem = item;

        _eventSystem.SetSelectedGameObject(_useItemConfirmButtonHolders[0]);
    }

    private void PromptRestockItems()
    {
        ShowRestockConfirmGroup();
        _eventSystem.SetSelectedGameObject(_restockConfirmButtonHolders[0]);
    }

    #region Button Functions - Inventory
    public void ConfirmUseItem()
    {
        HideUseItemConfirmGroup();
        OnUseItemAt(_selectedItemIndex, _selectedItem);
    }

    public void CancelUseItem()
    {
        HideUseItemConfirmGroup();

        _eventSystem.SetSelectedGameObject(heldItemHolders[_selectedItemIndex].gameObject);
    }

    public void ConfirmRestockItems()
    {
        HideRestockConfirmGroup();
        // Add ExpcectedItems to the storestock for real
        _gameplayTest.QueueCommand(new RestockStoreCommand(_expectedItemsToStock));
    }

    public void CancelRestockItems()
    {
        HideRestockConfirmGroup();
        // Reset 
        RefreshInventorySelection();
        SetupRestockStore(_selectedStoreData);
        _gameplayTest.QueueCommand(new UICancelCommand());
        _eventSystem.SetSelectedGameObject(heldItemHolders[_selectedItemIndex].gameObject);
    }
    #endregion Button Functions - Inventory
    public void OnUseItemAt(int index, ItemStats item)
    {
        HideUseItemConfirmGroup();

        //UseItem(selectedItemIndex);
        HideInventory();

        // **Figure out the type of item being used (this decides the behavior based on type)
        //Debug.Log($"UIInventoryManager | {item.itemName} used. | Item Type: {item.type}");
        //m_ItemUsed.RaiseEvent(index, item); // replace this
        _gameplayTest.QueueCommand(new InventoryUseItemCommand(index, item));

    }

    public void ShowUseItemConfirmGroup()
    {
        _useItemConfirmGroup.interactable = true;
        FadeTo(_useItemConfirmGroup, 1, 0.25f);
    }
    public void HideUseItemConfirmGroup()
    {
        _useItemConfirmGroup.interactable = false;
        FadeTo(_useItemConfirmGroup, 0, 0.25f);
    }
    
    public void ShowRestockConfirmGroup()
    {
        _restockConfirmGroup.interactable = true;
        FadeTo(_restockConfirmGroup, 1, 0.25f);
    }
    public void HideRestockConfirmGroup()
    {
        _restockConfirmGroup.interactable = false;
        FadeTo(_restockConfirmGroup, 0, 0.25f);
    }
    public void OnItemDiscarded(int index, ItemStats item)
    {
        discardedItemCount--;

        instructionText.text = $"<color=#FF8A8A>Inventory full!\r\nChoose {discardedItemCount} items to discard.</color>";

        if ( discardedItemCount <= 0)
        {
            instructionText.text = "";
            instructionGroup.alpha = 0;

            //m_ExitInventory.RaiseEvent();
            GameplayTest.instance.QueueCommand(new UICancelCommand()); // temporary
            //GameplayTest.instance.phase = GameplayTest.GamePhase.EndTurn; // CHANGE THIS LATER PLZ
        }
    }

    public void OnFullInventory(EntityData ed)
    {
        discardedItemCount = ed.Inventory.Count - ed.InventoryLimit;

        DisplayInventory(ed);

        inventoryInputPromptText.text = "<sprite=0> <color=#FF8A8A>Discard Item</color>";
        instructionGroup.alpha = 1;
        //m_RefreshInventory.RaiseEvent(ed);
        RefreshInventory(ed);

        if (discardedItemCount > 0) 
        {
            instructionText.text = $"<color=#FF8A8A>Inventory full!\r\nChoose {discardedItemCount} items to discard.</color>";
        }
    }
}
