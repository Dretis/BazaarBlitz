using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Febucci.UI.Core;

public class PlayerColorSelectionHandler : MonoBehaviour, ISubmitHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    private Image buttonImage;

    [SerializeField] private PlayerSetupMenuController setup;
    [SerializeField] private Image baggieVisual;
    [SerializeField] private TypewriterCore baggieVisualName;
    [SerializeField] private Color color;
    [SerializeField] private string baggieColorName;

    // Start is called before the first frame update
    void Start()
    {
        buttonImage = GetComponent<Image>();
        buttonImage.color = color;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //eventData.Use();
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
        Debug.Log("Submitted | "+eventData);
        setup.SetColor(color);
        setup.SetName(baggieColorName);
        // set Baggie name here too with baggieColorName
    }

    public void OnSelect(BaseEventData eventData)
    {
        // Change color of the Baggie visual
        baggieVisual.color = color;
        baggieVisualName.ShowText(baggieColorName);
    }
    public void OnDeselect(BaseEventData eventData)
    {
        //
    }
}
