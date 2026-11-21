using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using LitMotion;
using LitMotion.Extensions;
using static GameplayTest;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;
using Core.Events;
using System;
using Core;

public class UIRenovateSelectorManager : MonoBehaviour
{
    [SerializeField] private Color _playerColor;
    private PlayerState _playerState;
    private GameplayTest _gameplayTest;

    [SerializeField] private MultiplayerEventSystem _eventSystem;
    public GameStateEventChannelSO _gameStateEvent;
    public enum RenovateSelectorPhase
    {
        AskPrompt,
        OwnedStoreboats,
        RenovateOptions
    }
    private EntityData _promptedEntityData;
    //private MapNode selectedNode;
    private BoardNodeData _selectedNodeData;
    private StorefrontData _selectedStoreData;
    private int selectedStorestock;

    [SerializeField] private RenovateSelectorPhase renovatePhase;

    [Header("UI Selection Elements")]
    [SerializeField] private GameObject storeboatSelectorPrefab;
    [Space]
    [SerializeField] private CanvasGroup selectorInstructionGroup;
    [SerializeField] private CanvasGroup restockSelectorContainer; // container of the player's boats
    
    [Header("UI Prompt Elements")]
    [SerializeField] private TextMeshProUGUI renovateAskText;// rename to Renovate
    [SerializeField] private CanvasGroup restockAskGroup;// rename to Renovate

    [SerializeField] private CanvasGroup restockAskButtonContainer;
    [SerializeField] private List<GameObject> restockAskButtonHolders = new List<GameObject>();// rename to RenovateAsk

    [SerializeField] private CanvasGroup renovateOptionsButtonContainer;
    [SerializeField] private List<GameObject> renovateOptionsButtonHolders = new List<GameObject>();

    //public TextMeshProUGUI restockAskText;
    [Header("Broadcast on Event Channel")]
    //public PlayerEventChannelSO m_TryRestockStore; // also listening
    public VoidEventChannelSO m_CancelRestockStore; // also listening
    public NodeEventChannelSO m_UpgradeStore;
    public NodeEventChannelSO m_TryRelocateStore;

    [Header("Listen on Event Channel")]
    public PlayerEventChannelSO m_AskRenovateStore; // originally to m_AskRestockStore
    public NodeEventChannelSO m_RestockStore; // delete
    //public NodeEventChannelSO m_UpgradeStore;

    //public NodeEventChannelSO m_PromptRenovateOptions;
    public VoidEventChannelSO m_BackRenovateStore;
    public void Initialize(PlayerState ps, GameplayTest gt)
    {
        _playerColor = ps.EntityData.MainColor;
        _playerState = ps;
        _gameplayTest = gt;
    }

    private void OnEnable()
    {
        //m_AskRenovateStore.OnEventRaised += OnAskRenovateStore;

        //m_TryRestockStore.OnEventRaised += OnTryRestockStore;
        m_CancelRestockStore.OnEventRaised += OnCancelRestockStore;

        m_RestockStore.OnEventRaised += OnRestockStore;

        //m_PromptRenovateOptions.OnEventRaised += OnPromptRenovateOptions;
        m_BackRenovateStore.OnEventRaised += OnBackRenovateStore;

        _gameStateEvent.OnEventRaised += UpdateUIFromGameState;
    }

    private void OnDisable()
    {
        //m_AskRenovateStore.OnEventRaised -= OnAskRenovateStore;

        //m_TryRestockStore.OnEventRaised -= OnTryRestockStore;
        m_CancelRestockStore.OnEventRaised -= OnCancelRestockStore;

        m_RestockStore.OnEventRaised -= OnRestockStore;

        //m_PromptRenovateOptions.OnEventRaised -= OnPromptRenovateOptions;
        m_BackRenovateStore.OnEventRaised -= OnBackRenovateStore;

        _gameStateEvent.OnEventRaised -= UpdateUIFromGameState;
    }

    private void UpdateUIFromGameState(FrameGameState gameState)
    {
        if (gameState.DidChangePhases())
        {
            if (gameState.NewState.GamePhase is GameplayTest.GamePhase.RenovateTime)
            {
                OnAskRenovateStore(gameState.NewState.CurrentPlayer().EntityData);
            }
        }
    }

    private void Start()
    {
        DeactivateSelectorContainer();
        DeactivateAskGroup();
    }

    #region Button Functions
    // Button functions
    public void ConfirmRenovateAskButton()
    {
        renovatePhase = RenovateSelectorPhase.OwnedStoreboats;

        DeactivateAskGroup();
        //m_TryRestockStore.RaiseEvent(promptedPlayerData); // this shows baots
        Debug.Log("m_TryRestockStore is supposed to be raised (This should be a TryRenovateStore thingy tho)");

        ActivateSelectorContainer();
        SpawnStoreboatSelectors(_promptedEntityData);

        _gameplayTest.QueueCommand(new UIConfirmRenovateCommand());
        //m_RestockStore.RaiseEvent(currentPlayer.occupiedNode);
    }

    public void CancelRenovateAskButton()
    {
        // temp functionality
        m_CancelRestockStore.RaiseEvent();
    }

    public void ConfirmRestockButton()
    {
        if (selectedStorestock < 3
            && _promptedEntityData.Inventory.Count != 0)
        {
            DeactivateAskGroup();
            DeactivateSelectorContainer();
            Debug.Log($"I am restocking Storefront with ID: {_selectedNodeData.Id}");
            _gameplayTest.QueueCommand(new UIRestockStoreCommand(_selectedNodeData.Id));
            //m_RestockStore.RaiseEvent(selectedNode);
        }
        else
        {
            ShakeAskGroup();
        }
    }

    public void ConfirmUpgradeButton()
    {
        if (_promptedEntityData.Money >= 150 &&
            _promptedEntityData.Level >= _selectedStoreData.StoreLevel)
        {
            DeactivateAskGroup();
            DeactivateSelectorContainer();
            _gameplayTest.QueueCommand(new UILevelUpStoreCommand(_selectedNodeData.Id));
            Debug.Log($"Leveling up Storefront with ID: {_selectedNodeData.Id}");
            //m_UpgradeStore.RaiseEvent(selectedNode);
        }
        else
        {
            ShakeAskGroup();
        }
    }

    public void ConfirmRelocateButton()
    {
        if (_promptedEntityData.CurrentStatsModifier.canUseRelocate
            && _promptedEntityData.Money >= 100)
        {
            DeactivateAskGroup();
            DeactivateSelectorContainer();
            Debug.Log($"I am trying to relocate Storefront with ID: {_selectedNodeData.Id}");
            //m_UpgradeStore.RaiseEvent(selectedNode);
            //m_TryRelocateStore.RaiseEvent(selectedNode);
        }
        else
        {
            ShakeAskGroup();
        }
    }

    public void HoverRestockButton()
    {
        if (selectedStorestock >= 3)
        {
            renovateAskText.text = "<color=grey>Supply additional items from your inventory.";
            renovateAskText.text += "\n<size=24><color=red>[!] Unable to stock, store is full.";
        }
        else if(_promptedEntityData.Inventory.Count == 0)
        {
            renovateAskText.text = "<color=grey>Supply additional items from your inventory.";
            renovateAskText.text += "\n<size=24><color=red>[!] Cannot stock due to lack of inventory items.";
        }
        else
            renovateAskText.text = "Supply additional items from your inventory.";
    }

    public void HoverUpgradeButton()
    {
        if (_promptedEntityData.Level < _selectedStoreData.StoreLevel)
        {
            renovateAskText.text = "<color=grey>Increase this storefront prices by 20% for 150<sprite=\"Coin Icon\" index=0 tint=1>.";
            renovateAskText.text += "\n<size=24><color=red>[!] Cannot exceed stars past your Level + 1.";
        }
        else if (_promptedEntityData.Money >= 150)
        {
            renovateAskText.text = "Increase this storefront prices by 20% for <color=yellow>150</color><sprite=\"Coin Icon\" index=0>.";
        }
        else
        {
            renovateAskText.text = "Increase this storefront prices by 20% for <color=red>150</color><sprite=\"Coin Icon\" index=0>.";
            //renovateAskText.text = "Spend <color=yellow>150</color><sprite=\"Coin Icon\" index=0> to increase prices by 20% for this storefront.";
        }
        /*
        renovateAskText.text = "Increase prices of this storefront by 20% for 150<sprite=\"Coin Icon\" index=0>.";
        if (promptedPlayerData.heldPoints < 150)
        {
            renovateAskText.text += "\n<size=24><color=red>[!] Unable to upgrade, lack of <sprite=\"Coin Icon\" index=0>.";
        }
        */
    }

    public void HoverRelocateButton()
    {
        if (!_promptedEntityData.CurrentStatsModifier.canUseRelocate) 
        {
            renovateAskText.text = "<color=grey>Move this storefront to an Open Space for 100<sprite=\"Coin Icon\" index=0 tint=1>.";
        } 
        else if(_promptedEntityData.Money < 100)
        {
            renovateAskText.text = "Move this storefront to an Open Space for <color=red>100</color><sprite=\"Coin Icon\" index=0>.";
        }
        else
        {
            renovateAskText.text = "Move this storefront to an Open Space for <color=yellow>100</color><sprite=\"Coin Icon\" index=0>.";
        }

        if (!_promptedEntityData.CurrentStatsModifier.canUseRelocate)
        {
            renovateAskText.text += "\n<size=24><color=red>[!] Requires the Blessing of Luxury.";
        }
    }
    #endregion Button Functions

    private void DeactivateAskGroup()
    {
        restockAskGroup.alpha = 0.0f;
        restockAskGroup.interactable = false;
        restockAskGroup.blocksRaycasts = false;
    }

    private void ActivateAskGroup()
    {
        restockAskGroup.alpha = 1.0f;
        restockAskGroup.interactable = true;
        restockAskGroup.blocksRaycasts = true;
    }
    private void ShakeAskGroup()
    {
        var rect = restockAskGroup.GetComponent<RectTransform>();

        LMotion.Shake.Create(0f, 10f, .5f)
                .WithFrequency(5)
                .BindToAnchoredPositionX(rect);
    }

    private void OnAskRenovateStore(EntityData ed)
    {
        renovatePhase = RenovateSelectorPhase.AskPrompt;

        Debug.Log("UIRestockSelectorManager | OnAskRenovateStore");
        renovateAskText.text = "Renovate one of your stores?";
        //restockAskText.text = "Restock one of your stores?";
        DeactivateSelectorContainer();
        DestroyAllStoreboatSelectors();

        _promptedEntityData = ed;

        ActivateAskGroup();

        renovateOptionsButtonContainer.gameObject.SetActive(false);
        renovateOptionsButtonContainer.interactable = false;

        restockAskButtonContainer.gameObject.SetActive(true);
        restockAskButtonContainer.interactable = true;

        _eventSystem.SetSelectedGameObject(restockAskButtonHolders[0]);
    }

    private void OnCancelRestockStore()
    {
        DeactivateAskGroup();
        DeactivateSelectorContainer();
        //GameplayTest.instance.phase = GamePhase.EndTurn;
    }

    private void OnRestockStore(MapNode node)
    {
        DeactivateSelectorContainer();
        DestroyAllStoreboatSelectors();
    }

    public  void PromptRenovateOptions(BoardNodeData bnd)
    {
        renovatePhase = RenovateSelectorPhase.RenovateOptions;
        _selectedNodeData = bnd;
        _selectedStoreData = bnd.StorefrontData;

        ActivateAskGroup();
        DeactivateSelectorContainer();

        restockAskButtonContainer.gameObject.SetActive(false);
        restockAskButtonContainer.interactable = false;

        renovateOptionsButtonContainer.gameObject.SetActive(true);
        renovateOptionsButtonContainer.interactable = true;

        //selectedNode = node;
        //selectedStore = node.GetComponent<StoreManager>();
        //selectedStorestock = 0;
        foreach (ItemStats item in _selectedStoreData.Storestock)
        {
            if (item != null) selectedStorestock++;
        }
        // Show prompt to Restock or Renovate!
        var restockOptionImage = renovateOptionsButtonHolders[0].GetComponent<Image>();
        var upgradeOptionImage = renovateOptionsButtonHolders[1].GetComponent<Image>();
        var relocateOptionImage = renovateOptionsButtonHolders[2].GetComponent<Image>();
        if (selectedStorestock < 3 && _promptedEntityData.Inventory.Count != 0)
        {
            restockOptionImage.color = Color.white;
        }
        else
        {
            restockOptionImage.color = Color.grey;
        }

        if(_promptedEntityData.Money >= 150
            && _promptedEntityData.Level >= _selectedStoreData.StoreLevel)
        {
            upgradeOptionImage.color = Color.white;
        }
        else
        {
            upgradeOptionImage.color = Color.grey;
        }

        if (_promptedEntityData.CurrentStatsModifier.canUseRelocate)
        {
            relocateOptionImage.color = Color.white;
        }
        else
        {
            relocateOptionImage.color = Color.grey;
        }


        _eventSystem.SetSelectedGameObject(renovateOptionsButtonHolders[0]);
    }

    private void OnBackRenovateStore()
    {
        switch(renovatePhase)
        {
            case RenovateSelectorPhase.AskPrompt:
                GameplayTest.instance.phase = GamePhase.EndTurn; // change this
                break;
            case RenovateSelectorPhase.OwnedStoreboats:
                //m_AskRenovateStore.RaiseEvent(promptedPlayerData);
                OnAskRenovateStore(_promptedEntityData);
                break;
            case RenovateSelectorPhase.RenovateOptions:
                DestroyAllStoreboatSelectors();
                ConfirmRenovateAskButton();
                break;
            default:
                Debug.Log("???");
                break;
        }
    }

    private void DeactivateSelectorContainer()
    {
        selectorInstructionGroup.alpha = 0.0f;

        restockSelectorContainer.alpha = 0.0f;
        restockSelectorContainer.interactable = false;
        restockSelectorContainer.blocksRaycasts = false;
    }

    private void ActivateSelectorContainer()
    {
        selectorInstructionGroup.alpha = 1.0f;

        restockSelectorContainer.alpha = 1.0f;
        restockSelectorContainer.interactable = true;
        restockSelectorContainer.blocksRaycasts = true;
    }

    private void SpawnStoreboatSelectors(EntityData ed)
    {
        var firstBoat = true;
        var index = 1;
        var selectorParentTransform = restockSelectorContainer.transform;

        // associatedEntity
        foreach (int id in ed.OwnedStrorefrontIds)
        {
            var node = _gameplayTest.BoardNodeManager.NodeDictionary[id].BoardNodeData;
            var store = _gameplayTest.BoardNodeManager.NodeDictionary[id].BoardNodeData.StorefrontData;

            var boatSelectorObj = Instantiate(storeboatSelectorPrefab, selectorParentTransform);
            var rbs = boatSelectorObj.GetComponent<RestockBoatSelectionHandler>();
            rbs.Initialize(index, node, this);
            rbs.UpdateStoreBoatVisual(index, store, _playerColor);
            index++;

            if (firstBoat)
            {
                _eventSystem.SetSelectedGameObject(boatSelectorObj);
                firstBoat = false;
            }
        }
    }

    private void DestroyAllStoreboatSelectors()
    {
        // Gets rid of all the held item containers in the inventory UI

        var selectorParentTransform = restockSelectorContainer.transform;

        for (int i = selectorParentTransform.childCount - 1; i >= 0; i--)
        {
            Destroy(selectorParentTransform.GetChild(i).gameObject);
        }
    }
}
