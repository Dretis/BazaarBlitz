using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIRestockSelectorManager : MonoBehaviour
{
    private EntityPiece promptedPlayer;

    [Header("UI Selection Elements")]
    [SerializeField] private GameObject storeboatSelectorPrefab;
    [Space]
    [SerializeField] private CanvasGroup selectorInstructionGroup;
    [SerializeField] private CanvasGroup restockSelectorContainer;
    
    [Header("UI Prompt Elements")]
    [SerializeField] private CanvasGroup restockAskGroup;
    [SerializeField] private List<GameObject> restockAskButtonHolders = new List<GameObject>();

    //public TextMeshProUGUI restockAskText;
    [Header("Broadcast on Event Channel")]
    public PlayerEventChannelSO m_TryRestockStore; // also listening
    public VoidEventChannelSO m_CancelRestockStore; // also listening

    [Header("Listen on Event Channel")]
    public PlayerEventChannelSO m_AskRestockStore;
    public NodeEventChannelSO m_RestockStore;

    private void OnEnable()
    {
        m_AskRestockStore.OnEventRaised += OnAskRestockStore;

        m_TryRestockStore.OnEventRaised += OnTryRestockStore;
        m_CancelRestockStore.OnEventRaised += OnCancelRestockStore;

        m_RestockStore.OnEventRaised += OnRestockStore;
    }

    private void OnDisable()
    {
        m_AskRestockStore.OnEventRaised -= OnAskRestockStore;

        m_TryRestockStore.OnEventRaised -= OnTryRestockStore;
        m_CancelRestockStore.OnEventRaised -= OnCancelRestockStore;

        m_RestockStore.OnEventRaised -= OnRestockStore;
    }

    private void Start()
    {
        DeactivateSelectorContainer();
        DeactivateAskGroup();
    }

    // Button functions
    public void ConfirmRestockAskButton()
    {
        DeactivateAskGroup();
        m_TryRestockStore.RaiseEvent(promptedPlayer);
        //m_RestockStore.RaiseEvent(currentPlayer.occupiedNode);
    }

    public void CancelRestockAskButton()
    {
        // temp functionality
        m_CancelRestockStore.RaiseEvent();
    }

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

    private void OnAskRestockStore(EntityPiece entity)
    {
        //restockAskText.text = "Restock one of your stores?";
        DeactivateSelectorContainer();
        DestroyAllStoreboatSelectors();

        promptedPlayer = entity;

        ActivateAskGroup();

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
