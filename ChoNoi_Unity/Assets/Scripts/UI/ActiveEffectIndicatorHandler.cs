using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActiveEffectIndicatorHandler : MonoBehaviour
{
    [Header("Effect Info")]
    [SerializeField] private ItemStats originalItem;
    [SerializeField] private Image activeEffectIcon;
    [SerializeField] private TextMeshProUGUI durationLeft;

    public void UpdateEffectInfo(EntityPiece.ActiveEffect effect)
    {
        originalItem = effect.originalItem;
        activeEffectIcon.sprite = originalItem.itemSprite;

        var duration = effect.turnsRemaining + 1; // +1 buffer bc of 'activateEffectStartTurn'

        Debug.Log($"{originalItem} | Duration Left: {duration}");
        if (duration < 0)
        {
            Debug.Log("something went wrong, negative item duration");
            durationLeft.text = "?";
        }
        else if (duration == 0)
            durationLeft.text = "";
        else if (duration >= 100)
            durationLeft.text = "!!";
        else
            durationLeft.text = "" + duration;
    }
}
