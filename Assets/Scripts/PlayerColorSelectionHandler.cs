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
    private Button button;
    private RectTransform rect;

    [SerializeField] private PlayerSetupMenuController setup;
    //[SerializeField] private Image baggieVisual;
    //[SerializeField] private TypewriterCore baggieVisualName;
    [SerializeField] private PlayerColorPalettePreset colorPreset;
    //[SerializeField] private Color mainColor;
    //[SerializeField] private List<Color> colorPalette = new List<Color>();
    //[SerializeField] private string baggieColorName;

    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
        buttonImage.color = colorPreset.mainColor;
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
        Debug.Log("Submitted | " + eventData);
        setup.SetColor(colorPreset.mainColor);
        setup.SetPalette(colorPreset.baggieColorPalette);
        setup.SetName(colorPreset.presetName);
        setup.SetSelectedColorButton(button);
        // set Baggie name here too with baggieColorName
    }

    public void OnSelect(BaseEventData eventData)
    {
        // Change color of the Baggie visual
        setup.SetSelectedColor(colorPreset.mainColor);
        //setup.SetSelectedPalette(colorPreset.baggieColorPalette);
        setup.SetSelectedName(colorPreset.presetName);

        rect.localScale = Vector3.one * 1.15f;
    }
    public void OnDeselect(BaseEventData eventData)
    {
        //
        rect.localScale = Vector3.one;
    }
}
