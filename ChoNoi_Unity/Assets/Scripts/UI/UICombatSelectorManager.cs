using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UICombatSelectorManager : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup selectorInstructionGroup;
    public CanvasGroup combatSelectorContainer;
    public GameObject fighterSelectorPrefab;

    [Header("Listen on Event Channel")]
    public PlayerListEventChannelSO m_LandOnMultipleEntities;
    public PlayerEventChannelSO m_FighterSelecteed;

    private void OnEnable()
    {
        m_LandOnMultipleEntities.OnEventRaised += OnLandOnMutipleEntities;
        m_FighterSelecteed.OnEventRaised += OnFighterSelected;
    }

    private void OnDisable()
    {
        m_LandOnMultipleEntities.OnEventRaised -= OnLandOnMutipleEntities;
        m_FighterSelecteed.OnEventRaised -= OnFighterSelected;
    }

    private void Start()
    {
        DeactivateSelectorContainer();

        combatSelectorContainer.alpha = 0.0f;
        combatSelectorContainer.interactable = false;
        combatSelectorContainer.blocksRaycasts = false;
    }

    private void OnLandOnMutipleEntities(List<EntityPiece> entities)
    {
        ActivateSelectorContainer();
        SpawnFighterSelectors(entities);
    }

    private void OnFighterSelected(EntityPiece entity)
    {
        DeactivateSelectorContainer();
        DestroyAllFighterSelectors();
    }

    private void DeactivateSelectorContainer()
    {
        selectorInstructionGroup.alpha = 0.0f;

        combatSelectorContainer.alpha = 0.0f;
        combatSelectorContainer.interactable = false;
        combatSelectorContainer.blocksRaycasts = false;
    }

    private void ActivateSelectorContainer()
    {
        selectorInstructionGroup.alpha = 1.0f;

        combatSelectorContainer.alpha = 1.0f;
        combatSelectorContainer.interactable = true;
        combatSelectorContainer.blocksRaycasts = true;
    }

    private void SpawnFighterSelectors(List<EntityPiece> entities)
    {
        var firstFighter = true;
        var selectorParentTransform = combatSelectorContainer.transform;
        // associatedEntity
        foreach (EntityPiece entity in entities)
        {
            var fighter = Instantiate(fighterSelectorPrefab, selectorParentTransform);
            fighter.GetComponent<CombatFighterSelectionHandler>().UpdateFighterInfo(entity);

            if (firstFighter)
            {
                EventSystem.current.SetSelectedGameObject(fighter);
                firstFighter = false;
            }
        }
    }

    private void DestroyAllFighterSelectors()
    {
        // Gets rid of all the held item containers in the inventory UI

        var selectorParentTransform = combatSelectorContainer.transform;

        for (int i = selectorParentTransform.childCount - 1; i >= 0; i--)
        {
            Destroy(selectorParentTransform.GetChild(i).gameObject);
        }
    }
}
