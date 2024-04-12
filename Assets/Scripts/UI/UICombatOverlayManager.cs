using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UICombatOverlayManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private RectTransform vsHeader;

    [Header("Dice Information")]
    [SerializeField] private RectTransform diceInfo;

    [SerializeField] private GameObject leftDiceStat;
    [SerializeField] private GameObject rightDiceStat;

    [SerializeField] private List<TextMeshProUGUI> diceNumbers = new List<TextMeshProUGUI>();

    [Header("UI Inputs")]
    [SerializeField] private CanvasGroup leftAttackPrompt;
    [SerializeField] private CanvasGroup leftDefendPrompt;

    [SerializeField] private CanvasGroup rightAttackPrompt;
    [SerializeField] private CanvasGroup rightDefendPrompt;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI leftHealthPoints;
    [SerializeField] private TextMeshProUGUI rightHealthPoints;

    [Header("UI Positions")]
    [SerializeField] private Vector2 vsInitialPosition;
    [SerializeField] private Vector2 vsHidePosition;

    [SerializeField] private Vector2 diceInitialPosition;
    [SerializeField] private Vector2 diceHidePosition;
    // private RectTransform vsHeaderRectTransform;

    [Header("Test variables")]
    public EntityPiece entity1;
    public EntityPiece entity2;

    // Start is called before the first frame update
    void Awake()
    {
        //vsHeaderRectTransform = vsHeader.GetComponent<RectTransform>();
        vsInitialPosition = vsHeader.anchoredPosition;
        diceInitialPosition = diceInfo.anchoredPosition;


        vsHeader.anchoredPosition = vsHidePosition;
        diceInfo.anchoredPosition = diceHidePosition;

        UpdateDiceStats(CombatManager.Instance.player1, leftDiceStat);
        UpdateDiceStats(CombatManager.Instance.player2, rightDiceStat);

        ShowVSHeader();
        ShowDiceInfo();
    }

    private void Update()
    {
        // Testing functions
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ShowVSHeader();
            ShowDiceInfo();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            HideVSHeader();
            HideDiceInfo();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            // Left attacking | right defending
            ShowInputPrompt(rightDefendPrompt, 0.25f);
            ShowInputPrompt(leftAttackPrompt, 0.25f);

            HideInputPrompt(leftDefendPrompt, 0.25f);
            HideInputPrompt(rightAttackPrompt, 0.25f);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            // Left defend | right attack
            ShowInputPrompt(leftDefendPrompt, 0.25f);
            ShowInputPrompt(rightAttackPrompt, 0.25f);

            HideInputPrompt(rightDefendPrompt, 0.25f);
            HideInputPrompt(leftAttackPrompt, 0.25f);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            // fill left side's dice stats
            UpdateDiceStats(entity1, leftDiceStat);
            UpdateDiceStats(entity2, rightDiceStat);
        }
    }

    public void ShowVSHeader()
    {
        vsHeader.DOAnchorPos(vsInitialPosition, 0.5f, false).SetEase(Ease.OutSine);
    }

    public void HideVSHeader()
    {
        vsHeader.DOAnchorPos(vsHidePosition, 0.5f, false).SetEase(Ease.InFlash);
    }

    public void ShowDiceInfo()
    {
        diceInfo.DOAnchorPos(diceInitialPosition, 0.5f, false).SetEase(Ease.OutSine);
    }

    public void HideDiceInfo()
    {
        diceInfo.DOAnchorPos(diceHidePosition, 0.5f, false).SetEase(Ease.InFlash);
    }

    public void ShowInputPrompt(CanvasGroup inputPrompt, float duration)
    {
        DOTween.To(()=> inputPrompt.alpha, x=> inputPrompt.alpha =  x, 1, duration); 
    }

    public void HideInputPrompt(CanvasGroup inputPrompt, float duration)
    {
        DOTween.To(() => inputPrompt.alpha, x => inputPrompt.alpha = x, 0, duration);
    }

    public void UpdateDiceStats(EntityPiece entity, GameObject diceStats)
    {
        // Visually updates the dice stats ui based on the entity and side
        diceNumbers.Clear();

        // Goes through the diceStats UI List and finds the text components
        foreach (Transform child in diceStats.transform)
        {
            diceNumbers.Add(child.GetComponentInChildren<TextMeshProUGUI>());
        }

        // Updates each individual dice from the text list based on the type
        // the following code is ABSOLUTELY DISGUSTING
        var faceIndex = 0;

        for (int i = 0; i < 6; i++)
        {
            diceNumbers[i].text = $"{entity.strDie[faceIndex]}";
            faceIndex++;
        }

        faceIndex = 0;

        for (int i = 6; i < 12; i++)
        {
            diceNumbers[i].text = $"{entity.dexDie[faceIndex]}";
            faceIndex++;
        }

        faceIndex = 0;

        for (int i = 12; i < 18; i++)
        {
            diceNumbers[i].text = $"{entity.intDie[faceIndex]}";
            faceIndex++;
        }
    }
}
