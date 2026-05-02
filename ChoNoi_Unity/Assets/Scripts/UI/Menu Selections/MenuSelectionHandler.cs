using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Febucci.UI.Core;
using LitMotion;

public class MenuSelectionHandler : MonoBehaviour, ISubmitHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public bool selectable = true;
    public string helpInfo;
    public TextMeshProUGUI helpText;
    [Space]
    public RectTransform rect;
    private Button butt;

    [Header("UI Elements")]
    public Image menuPanel;

    public Image menuVisual;
    public Image menuBumper;
    public TextMeshProUGUI menuBumperText;

    [Header("Colors")]
    public Color baseMenuColor;
    public Color unselectableMenuColor;
    public Color selectedMenuColor;

    private void Start()
    {
        rect = GetComponent<RectTransform>();
        butt = GetComponent<Button>();

        var help = GameObject.FindWithTag("HelpText");

        if(help != null)
            helpText = help.GetComponent<TextMeshProUGUI>();

        if (!selectable || !butt.interactable)
        {
            menuVisual.color = Color.grey;
            menuBumper.color = unselectableMenuColor;

            helpInfo += " [UNAVAILABLE]";
        }
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
    public virtual void OnSubmit(BaseEventData eventData)
    {

    }

    public virtual void OnSelect(BaseEventData eventData)
    {
        if(helpText != null)
            helpText.text = helpInfo;

        if (!selectable || !butt.interactable)
        {
            menuBumper.color = unselectableMenuColor;
            return;
        }
        menuBumper.color = selectedMenuColor;

        //UIScreenManager.instance.selected = EventSystem.current.currentSelectedGameObject;
    }

    public virtual void OnDeselect(BaseEventData eventData)
    {
        if (helpText != null)
            helpText.text = "";

        if (!selectable || !butt.interactable) 
        {
            menuBumper.color = unselectableMenuColor;
            return; 
        }

        menuBumper.color = baseMenuColor;
    }
}
