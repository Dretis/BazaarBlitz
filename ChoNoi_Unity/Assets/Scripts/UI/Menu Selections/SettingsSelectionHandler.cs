using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Febucci.UI.Core;
using LitMotion;

public class SettingSelectionHandler : MenuSelectionHandler
{
    [Header("Submenu UI Elements")]
    //[SerializeField] private CanvasGroup submenuGroup;
    public UIScreen submenuScreen;

    private void Start()
    {
        rect = GetComponent<RectTransform>();

        var help = GameObject.FindWithTag("HelpText");

        if (help != null)
            helpText = help.GetComponent<TextMeshProUGUI>();

        if (!selectable)
        {
            menuVisual.color = Color.grey;
            menuBumper.color = unselectableMenuColor;

            helpInfo += " [CURRENTLY UNAVAILABLE]";
        }
        HideSubmenu();
    }

    public override void OnSubmit(BaseEventData eventData)
    {
        //UIScreenManager.instance.GoToScreen(submenuScreen);
        ShowSubmenu();
    }

    public override void OnSelect(BaseEventData eventData)
    {
        if (helpText != null)
            helpText.text = helpInfo;

        ShowSubmenu();
        if (!selectable) return;

        menuBumper.color = selectedMenuColor;
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        if (helpText != null)
            helpText.text = "";

        HideSubmenu();

        if (!selectable) return;

        menuBumper.color = baseMenuColor;
    }

    public void ShowSubmenu()
    {
        //Debug.Log("showing submenu");
        submenuScreen.Show();
    }

    public void HideSubmenu()
    {
        //Debug.Log("hiding submenu");
        submenuScreen.Hide();
    }
}
