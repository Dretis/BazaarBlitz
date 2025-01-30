using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Febucci.UI.Core;

public class DiceStatSelectionHandler : MonoBehaviour, ISubmitHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    private Image buttonImage;

    //[SerializeField] private PlayerSetupMenuController setup;
    //[SerializeField] private Image baggieVisual;
    //[SerializeField] private TypewriterCore baggieVisualName;
    //[SerializeField] private Color color;
    //[SerializeField] private string baggieColorName;

    [SerializeField] private TypewriterCore diceFaceText;
    [SerializeField] private int diceFaceIndex;
    [SerializeField] private int diceFaceValue;
    [SerializeField] private Action.WeaponTypes diceType; // Melee = 0, Gun = 1, Magic = 2
    [SerializeField] private TextMeshProUGUI tooltipText;

    //private int[] costArray = { -1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 5, 999 }; // SP Cost to augment

    [Header("Broadcast On Event Channel")]
    public WeaponTypeIntEventChannel m_TryAugmentDieFaceValue; // also listening to this one
    //public VoidEventChannelSO m_AugmentedDieFaceValue; // listening to this one?

    // Start is called before the first frame update
    void Start()
    {
        buttonImage = GetComponent<Image>();
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
        // Try to upgrade that die face (Dice Type (prob int index) and what Face Num (1-6)
        m_TryAugmentDieFaceValue.RaiseEvent(diceType, diceFaceIndex);
        //diceFaceText.ShowText($"{diceFaceValue + 1}");
    }

    public void OnSelect(BaseEventData eventData)
    {
        // Highlight selected die face
        // Show how much SP it costs to upgrade +1 Face Value
        //diceFaceText.ShowText($"<incr>{diceFaceValue}");
        tooltipText.text = $"Augmenting [{diceFaceValue}] costs {GameplayTest.instance.costArray[diceFaceValue]} SP.";
    }

    public void OnDeselect(BaseEventData eventData)
    {
        //diceFaceText.ShowText($"{diceFaceValue}");
    }

    public void SetDieFaceValue(int value)
    {
        diceFaceValue = value;
        diceFaceText.ShowText(diceFaceValue.ToString());
    }
}
