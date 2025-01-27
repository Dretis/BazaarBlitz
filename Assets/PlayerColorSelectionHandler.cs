using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerColorSelectionHandler : MonoBehaviour, ISubmitHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    private Image buttonImage;

    [SerializeField] private PlayerSetupMenuController setup;
    [SerializeField] private Image baggieVisual;
    [SerializeField] private Color color;

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
    }

    public void OnSelect(BaseEventData eventData)
    {
        // Change color of the Baggie visual
        baggieVisual.color = color;
    }
    public void OnDeselect(BaseEventData eventData)
    {
        //
    }
}
