using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Febucci.UI.Core;
using LitMotion;
using LitMotion.Extensions;

public class BlessingSelectionHandler : MonoBehaviour, ISubmitHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    private RectTransform rec;
    [SerializeField] private bool isSelectable;
    [SerializeField] private bool isDisabled = false; // temp

    [Header("Blessing Info")]
    [SerializeField] private ItemStats associatedItem; // Blessings function like items but permanent.
    [SerializeField] private Image blessingIcon;
    [SerializeField] private TextMeshProUGUI blessingName;
    [SerializeField] private TMP_ColorGradient blessingNameGradient;

    public TMP_ColorGradient BlessingNameGradient => blessingNameGradient;



    [Header("Required Blessing Categories")]
    [SerializeField] private List<int> prereqBlessingCounter; // 0 = wealth, 1 = power, 2 = speed
    //[SerializeField] private bool isWealthRequired;
    //[SerializeField] private bool isPowerRequired;
    //[SerializeField] private bool isSpeedRequired;

    [Header("Broadcast on Event Channels")]
    public ItemEventChannelSO m_PlayerPickedFirstBlessing;
    public ItemEventChannelSO m_BlessingSelected;
    public ItemEventChannelSO m_HoverInBlessing;

    private void Start()
    {
        rec = GetComponent<RectTransform>();

        blessingIcon.sprite = associatedItem.itemSprite;
        blessingName.text = associatedItem.l_itemName.GetLocalizedString();
        blessingName.colorGradientPreset = blessingNameGradient;
    }

    public void OnPointerClick(PointerEventData eventData)
    {

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        eventData.selectedObject = gameObject;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //eventData.selectedObject = null;
    }
    public void OnSubmit(BaseEventData eventData)
    {
        if(!isSelectable) return;
        var p = GameplayTest.instance.currentPlayer;
        if (PlayerCanRecieveBlessing(p))
        {
            if (IsPlayersFirstBlessing(p))
            {
                m_PlayerPickedFirstBlessing.RaiseEvent(associatedItem);
            }

            Debug.Log($"Chosen Blessing of {associatedItem.itemName}");
            m_BlessingSelected.RaiseEvent(associatedItem);
            // move this line somewhere else
            p.blessingCategoryCounter[((int)associatedItem.category)]++;
        }
        else
        {
            // idk shake the screen or something
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        m_HoverInBlessing.RaiseEvent(associatedItem);
        //rec.localScale = Vector3.one * 1.1f;

        var stripParentMotion = LMotion.Create(rec.localScale, Vector3.one * 1.25f, .2f)
            .WithEase(Ease.OutBack)
            .BindToLocalScale(rec);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        //rec.localScale = Vector3.one;

        var stripParentMotion = LMotion.Create(rec.localScale, Vector3.one, .2f)
            .WithEase(Ease.OutBack)
            .BindToLocalScale(rec);
    }

    private bool PlayerCanRecieveBlessing(EntityPiece p)
    {
        for (int i = 0; i < 3; i++)
        {
            if (p.blessingCategoryCounter[i] >= prereqBlessingCounter[i] &&
                prereqBlessingCounter[i] != -1) // -1 means doesnt need it
            {
                return true;
            }
        }
        return false;
        /*
        if((p.hasWealthBlessings && isWealthRequired) ||
           (p.hasPowerBlessings && isPowerRequired) ||
           (p.hasSpeedBlessings && isSpeedRequired))
        {
            return true;
        }
        else
            return false;
        */
    }

    private bool IsPlayersFirstBlessing(EntityPiece p)
    {
        for (int i = 0; i < 3; i++)
        {
            if (p.blessingCategoryCounter[i] != 0)
            {
                return false;
            }
        }
        return true;
    }
}
