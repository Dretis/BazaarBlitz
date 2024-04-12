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

    [SerializeField] private GameObject leftActionPrompt;
    [SerializeField] private GameObject rightActionPrompt;

    [SerializeField] private GameObject leftDiceStat;
    [SerializeField] private GameObject rightDiceStat;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI leftHealthPoints;
    [SerializeField] private TextMeshProUGUI rightHealthPoints;

    [Header("UI Positions")]
    [SerializeField] private Vector2 vsInitialPosition;
    [SerializeField] private Vector2 vsHidePosition;
    // private RectTransform vsHeaderRectTransform;


    // Start is called before the first frame update
    void Start()
    {
        //vsHeaderRectTransform = vsHeader.GetComponent<RectTransform>();
        vsInitialPosition = vsHeader.anchoredPosition;

        vsHeader.anchoredPosition = vsHidePosition;
        ShowVSHeader();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ShowVSHeader();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            HideVSHeader();
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
}
