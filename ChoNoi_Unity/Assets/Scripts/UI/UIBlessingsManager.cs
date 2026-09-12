using System.Collections;
using System.Collections.Generic;
using TMPro;
using Febucci.UI.Core;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using LitMotion;
using LitMotion.Extensions;
using System;

public class UIBlessingsManager : MonoBehaviour
{
    private EntityPiece associatedPlayer;
    [SerializeField] private BlessingSelectionHandler centerBlessingCircle;

    [Header("UI Elements")]
    //[SerializeField] BlessingSelectionHandler centerBlessingCircle;
    [SerializeField] private CanvasGroup blessingsTreeGroup;
    [SerializeField] private CanvasGroup backgroundStripGroup;
    [SerializeField] private RectTransform backgroundStripTransform;

    [Header("Blessing Card Selector UI Elements")]
    [SerializeField] private RectTransform hoveredBlessingTransform;
    [SerializeField] private Image hoveredBlessingIcon;
    [SerializeField] private TextMeshProUGUI hoveredBlessingName;
    [SerializeField] private TextMeshProUGUI hoveredBlessingEffect;

    [Header("Broadcast on Event Channels")]
    public PlayerEventChannelSO m_EnterStatAllocation;

    [Header("Listen on Event Channels")]
    public PlayerEventChannelSO m_EnterBlessingsTree;
    public VoidEventChannelSO m_ExitBlessingsTree;
    public ItemEventChannelSO m_BlessingSelected;
    public ItemEventChannelSO m_HoverInBlessing;

    private void OnEnable()
    {
        m_EnterBlessingsTree.OnEventRaised += OnEnterBlessingsTree;
        m_ExitBlessingsTree.OnEventRaised += OnExitBlessingsTree;

        m_BlessingSelected.OnEventRaised += OnBlessingSelected;
        m_HoverInBlessing.OnEventRaised += OnHoverInBlessing;
    }

    private void OnDisable()
    {
        m_EnterBlessingsTree.OnEventRaised -= OnEnterBlessingsTree;
        m_ExitBlessingsTree.OnEventRaised -= OnExitBlessingsTree;

        m_BlessingSelected.OnEventRaised -= OnBlessingSelected;
        m_HoverInBlessing.OnEventRaised -= OnHoverInBlessing;
    }

    // Start is called before the first frame update
    void Start()
    {
        blessingsTreeGroup.alpha = 0;
        blessingsTreeGroup.interactable = false;
        blessingsTreeGroup.blocksRaycasts = false;

        backgroundStripGroup.alpha = 0;
        backgroundStripGroup.interactable = false;
        backgroundStripGroup.blocksRaycasts = false;
    }
    private void OnEnterBlessingsTree(EntityPiece p)
    {
        associatedPlayer = p;

        ShowBlessingsTree();

        EventSystem.current.SetSelectedGameObject(centerBlessingCircle.gameObject);
    }

    private void OnExitBlessingsTree()
    {
        blessingsTreeGroup.interactable = false;
        blessingsTreeGroup.blocksRaycasts = false;

        //backgroundStripGroup.alpha = 0;
        blessingsTreeGroup.blocksRaycasts = false;

        var blessingsTreeMotion = LMotion.Create(blessingsTreeGroup.alpha, 0, 0.25f)
            .WithEase(Ease.OutQuad)
            .Bind(x => blessingsTreeGroup.alpha = x);

        var stripAlphaMotion = LMotion.Create(backgroundStripGroup.alpha, 1, 0.15f)
            .WithEase(Ease.OutQuad)
            .Bind(x => backgroundStripGroup.alpha = x);

        var backgroundStripMotion = LMotion.Create(Vector3.one, new Vector3(1, 0, 1), .15f)
            .WithEase(Ease.OutQuad)
            .BindToLocalScale(backgroundStripGroup.GetComponent<RectTransform>());
    }

    private void OnBlessingSelected(ItemStats b)
    {
        var p = associatedPlayer;
        p.AddItemToActiveEffects(b.Duration, b);

        p.UpdateStatModifier(new EntityPiece.ActiveEffect
        {
            originalItem = b,
            turnsRemaining = b.Duration - 1
        });

        m_ExitBlessingsTree.RaiseEvent();

        m_EnterStatAllocation.RaiseEvent(p);
        GameplayTest.instance.phase = GameplayTest.GamePhase.LevelUp;
        //ApplyItemEffectsOnTurnStart(p);
    }

    private void OnHoverInBlessing(ItemStats b)
    {
        hoveredBlessingIcon.sprite = b.itemSprite;
        var blessingHeader = $"<{b.l_itemName.GetLocalizedString()}>";
        hoveredBlessingName.text = blessingHeader;
        var gradient = EventSystem.current.currentSelectedGameObject.GetComponent<BlessingSelectionHandler>().BlessingNameGradient;
        hoveredBlessingName.colorGradientPreset = gradient;

        hoveredBlessingEffect.text = b.l_effect.GetLocalizedString();

        var cardMotion = LMotion.Create(Vector3.one * 0.95f, Vector3.one, .2f)
            .WithEase(Ease.OutBack)
            .BindToLocalScale(hoveredBlessingTransform);
    }

    private void ShowBlessingsTree()
    {
        //blessingsTreeGroup.alpha = 1;
        blessingsTreeGroup.interactable = true;
        blessingsTreeGroup.blocksRaycasts = true;

        backgroundStripGroup.alpha = 1;
        blessingsTreeGroup.blocksRaycasts = true;

        var blessingsTreeMotion = LMotion.Create(blessingsTreeGroup.alpha, 1, 0.35f)
            .WithEase(Ease.OutQuad)
            .Bind(x => blessingsTreeGroup.alpha = x);

        var stripAlphaMotion = LMotion.Create(backgroundStripGroup.alpha, 1, 0.45f)
            .WithEase(Ease.OutQuad)
            .Bind(x => backgroundStripGroup.alpha = x);

        var backgroundStripMotion = LMotion.Create(new Vector3(1,0,1), Vector3.one, .35f)
            .WithEase(Ease.OutBack)
            .BindToLocalScale(backgroundStripGroup.GetComponent<RectTransform>());
    }
}
