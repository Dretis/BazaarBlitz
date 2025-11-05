using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Febucci.UI.Core;

public class CombatFighterSelectionHandler : MonoBehaviour, ISubmitHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private EntityPiece associatedEntity;

    [Header("UI Elements")]
    [SerializeField] private Image fighterIcon;
    [SerializeField] private TextMeshProUGUI fighterName;
    [SerializeField] private TextMeshProUGUI fighterLevel;
    [SerializeField] private TextMeshProUGUI fighterCurrentHP;
    [SerializeField] private TextMeshProUGUI fighterMaxHP;

    [SerializeField] private List<TMP_ColorGradient> levelColorGradients;

    [Header("Broadcast on Event Channel")]
    public PlayerEventChannelSO m_FighterSelected;

    public void OnPointerClick(PointerEventData eventData)
    {

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        eventData.selectedObject = gameObject;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        eventData.selectedObject = null;
    }
    public void OnSubmit(BaseEventData eventData)
    {
        Debug.Log("Submitted | " + eventData);
        // Initiate Combat on this entity
        m_FighterSelected.RaiseEvent(associatedEntity);
    }

    public void OnSelect(BaseEventData eventData)
    {
        fighterName.color = Color.yellow;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        fighterName.color = Color.white;
    }

    public void UpdateFighterInfo(EntityPiece entity)
    {
        associatedEntity = entity;

        fighterIcon.sprite = entity.entityInfo.entityIcon;

        fighterIcon.color = entity.playerColor;
        fighterName.text = "" + entity.entityName;
        fighterCurrentHP.text = "" + entity.health;
        fighterMaxHP.text = "/" + entity.maxHealth;
        UpdateFighterLevel();
    }

    private void UpdateFighterLevel()
    {
        //levelColorGradients
        var lvl = associatedEntity.RenownLevel;

        if (lvl >= 10)
        {
            fighterLevel.colorGradientPreset = levelColorGradients[levelColorGradients.Count - 1];
        }
        else
        {
            fighterLevel.colorGradientPreset = levelColorGradients[lvl - 1];
        }

        fighterLevel.text = "*\n" + lvl;
    }
}
