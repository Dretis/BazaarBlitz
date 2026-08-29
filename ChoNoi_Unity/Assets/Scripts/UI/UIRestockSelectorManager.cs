using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static GameplayTest;

public class UIRestockSelectorManager : MonoBehaviour
{
    public enum RenovateSelectorPhase
    {
        AskPrompt,
        OwnedStoreboats,
        RenovateOptions
    }
    private EntityPiece promptedPlayer;
    private MapNode selectedNode;
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
    public PlayerEventChannelSO m_TryRestockStore; // also listening
    public VoidEventChannelSO m_CancelRestockStore; // also listening

    [Header("Listen on Event Channel")]
    public PlayerEventChannelSO m_AskRenovateStore; // originally to m_AskRestockStore
    public NodeEventChannelSO m_RestockStore;
    public NodeEventChannelSO m_UpgradeStore;

    public NodeEventChannelSO m_PromptRenovateOptions;
    public VoidEventChannelSO m_BackRenovateStore;

    private void OnEnable()
    {
        m_AskRenovateStore.OnEventRaised += OnAskRenovateStore;

        m_TryRestockStore.OnEventRaised += OnTryRestockStore;
        m_CancelRestockStore.OnEventRaised += OnCancelRestockStore;

        m_RestockStore.OnEventRaised += OnRestockStore;

        m_PromptRenovateOptions.OnEventRaised += OnPromptRenovateOptions;
        m_BackRenovateStore.OnEventRaised += OnBackRenovateStore;
    }

    private void OnDisable()
    {
        m_AskRenovateStore.OnEventRaised -= OnAskRenovateStore;

        m_TryRestockStore.OnEventRaised -= OnTryRestockStore;
        m_CancelRestockStore.OnEventRaised -= OnCancelRestockStore;

        m_RestockStore.OnEventRaised -= OnRestockStore;

        m_PromptRenovateOptions.OnEventRaised -= OnPromptRenovateOptions;
        m_BackRenovateStore.OnEventRaised -= OnBackRenovateStore;
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
        m_TryRestockStore.RaiseEvent(promptedPlayer);
        //m_RestockStore.RaiseEvent(currentPlayer.occupiedNode);
    }

    public void CancelRenovateAskButton()
    {
        // temp functionality
        m_CancelRestockStore.RaiseEvent();
    }

    public void ConfirmRestockButton()
    {
        if (selectedStorestock < 3)
        {
            DeactivateAskGroup();
            DeactivateSelectorContainer();
            m_RestockStore.RaiseEvent(selectedNode);
        }
    }

    public void ConfirmUpgradeButton()
    {
        if (promptedPlayer.heldPoints >= 150)
        {
            DeactivateAskGroup();
            DeactivateSelectorContainer();
            m_UpgradeStore.RaiseEvent(selectedNode);
        }
    }

    public void HoverRestockButton()
    {
        renovateAskText.text = "Supply additional items from your inventory.";

        if(selectedStorestock >= 3)
        {
            renovateAskText.text += "\n<size=24><color=red>[!] Unable to stock, store is full.";
        }
    }

    public void HoverUpgradeButton()
    {
        if (promptedPlayer.heldPoints < 150)
        {
            renovateAskText.text = "Increase this storefront prices by 20% for <color=red>150</color><sprite=\"Coin Icon\" index=0>.";
        }
        else
        {
            renovateAskText.text = "Increase this storefront prices by 20% for <color=yellow>150</color><sprite=\"Coin Icon\" index=0>.";
            //renovateAskText.text = "Spend <color=yellow>150</color><sprite=\"Coin Icon\" index=0> to increase prices by 20% for this storefront.";
        }
        /*
        renovateAskText.text = "Increase prices of this storefront by 20% for 150<sprite=\"Coin Icon\" index=0>.";
        if (promptedPlayer.heldPoints < 150)
        {
            renovateAskText.text += "\n<size=24><color=red>[!] Unable to upgrade, lack of <sprite=\"Coin Icon\" index=0>.";
        }
        */
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

    private void OnAskRenovateStore(EntityPiece entity)
    {
        renovatePhase = RenovateSelectorPhase.AskPrompt;

        Debug.Log("UIRestockSelectorManager | OnAskRenovateStore");
        renovateAskText.text = "Renovate one of your stores?";
        //restockAskText.text = "Restock one of your stores?";
        DeactivateSelectorContainer();
        DestroyAllStoreboatSelectors();

        promptedPlayer = entity;

        ActivateAskGroup();

        renovateOptionsButtonContainer.gameObject.SetActive(false);
        renovateOptionsButtonContainer.interactable = false;

        restockAskButtonContainer.gameObject.SetActive(true);
        restockAskButtonContainer.interactable = true;

        EventSystem.current.SetSelectedGameObject(restockAskButtonHolders[0]);
    }

    private void OnTryRestockStore(EntityPiece entity)
    {
        ActivateSelectorContainer();
        SpawnStoreboatSelectors(entity);
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

    private void OnPromptRenovateOptions(MapNode node)
    {
        renovatePhase = RenovateSelectorPhase.RenovateOptions;

        ActivateAskGroup();
        DeactivateSelectorContainer();

        restockAskButtonContainer.gameObject.SetActive(false);
        restockAskButtonContainer.interactable = false;

        renovateOptionsButtonContainer.gameObject.SetActive(true);
        renovateOptionsButtonContainer.interactable = true;

        selectedNode = node;
        var selectedStore = node.GetComponent<StoreManager>();
        selectedStorestock = 0;
        foreach (ItemStats item in selectedStore.storeInventory)
        {
            if (item != null) selectedStorestock++;
        }
        // Show prompt to Restock or Renovate!
        if (selectedStorestock >= 3)
        {
            // Grey out restock option
            Debug.Log("you can't restock a full store!!");

        }

        EventSystem.current.SetSelectedGameObject(renovateOptionsButtonHolders[0]);
    }

    private void OnBackRenovateStore()
    {
        switch(renovatePhase)
        {
            case RenovateSelectorPhase.AskPrompt:
                GameplayTest.instance.phase = GamePhase.EndTurn;
                break;
            case RenovateSelectorPhase.OwnedStoreboats:
                m_AskRenovateStore.RaiseEvent(promptedPlayer);
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

    private void SpawnStoreboatSelectors(EntityPiece player)
    {
        var firstBoat = true;
        var index = 1;
        var selectorParentTransform = restockSelectorContainer.transform;

        // associatedEntity
        foreach (StoreManager store in player.ownedStores)
        {
            var fighter = Instantiate(storeboatSelectorPrefab, selectorParentTransform);
            fighter.GetComponent<RestockBoatSelectionHandler>().UpdateBoatInfo(index, store);
            index++;

            if (firstBoat)
            {
                EventSystem.current.SetSelectedGameObject(fighter);
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
