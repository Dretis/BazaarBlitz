using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Febucci.UI.Core;
using UnityEngine.Localization;

public class BoardSelectionHandler : MenuSelectionHandler
{
    [Header("Board Selection UI Elements")]
    //[TextArea]
    //[SerializeField] private string boardInformation;
    [SerializeField] private LocalizedString boardAboutInfo;
    [Space]
    [SerializeField] private Image selectedBoardImage;
    [SerializeField] private TextMeshProUGUI boardInformation;

    public override void OnSubmit(BaseEventData eventData)
    {

    }

    public override void OnSelect(BaseEventData eventData)
    {
        if (helpText != null)
            helpText.text = helpInfo;

        if (!selectable)
        {
            menuBumper.color = unselectableMenuColor;
            return;
        }

        menuBumper.color = selectedMenuColor;

        selectedBoardImage.sprite = menuVisual.sprite;
        boardInformation.text = boardAboutInfo.GetLocalizedString();
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        if (helpText != null)
            helpText.text = "";

        if (!selectable)
        {
            menuBumper.color = unselectableMenuColor;
            return;
        }

        menuBumper.color = baseMenuColor;
    }
}
