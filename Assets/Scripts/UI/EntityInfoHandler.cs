using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EntityInfoHandler : MonoBehaviour
{
    [SerializeField] EntityPiece entity;

    [Header("UI Info")]
    [SerializeField] private CanvasGroup infoGroup;
    [SerializeField] private Image entityIcon;
    [SerializeField] private TextMeshProUGUI entityName;
    [SerializeField] private TextMeshProUGUI entityScore;
    [SerializeField] private TextMeshProUGUI entityHP;

    [SerializeField] private GameObject entityDiceStatHolder;
    [SerializeField] private List<TextMeshProUGUI> entityDiceNumbers = new List<TextMeshProUGUI>();

    [Header("Listen on Event Channels")]
    //public PlayerEventChannelSO m_PlayerOnSelectedTile;
    public NodeEventChannelSO m_EnterRaycastedTile;
    public VoidEventChannelSO m_ExitRaycastedTile;
    public VoidEventChannelSO m_DisableFreeview;

    private void OnEnable()
    {
        //m_PlayerOnSelectedTile.OnEventRaised += OnPlayerOnSelectedTile;
        m_EnterRaycastedTile.OnEventRaised += OnEnterRaycastedTile;
        m_ExitRaycastedTile.OnEventRaised += HideEntityInfo;
        m_DisableFreeview.OnEventRaised += HideEntityInfo;
    }

    private void OnDisable()
    {
        //m_PlayerOnSelectedTile.OnEventRaised -= OnPlayerOnSelectedTile;
        m_EnterRaycastedTile.OnEventRaised -= OnEnterRaycastedTile;
        m_ExitRaycastedTile.OnEventRaised -= HideEntityInfo;
        m_DisableFreeview.OnEventRaised -= HideEntityInfo;
    }

    // Start is called before the first frame update
    void Start()
    {
        infoGroup.alpha = 0f;
    }

    private void OnEnterRaycastedTile(MapNode node)
    {
        // Attempt to get a player on the tile (if there is one)
        entity = node.playerOccupied;

        if (entity != null)
        {
            OnPlayerOnSelectedTile();
        }
        else FadeTo(infoGroup, 0, 0.25f);
    }

    private void OnPlayerOnSelectedTile()
    {
        FadeTo(infoGroup, 1, 0.25f);

        SetEntityIconColor();
        SetEntityName();
        SetMoney();
        SetHealth();
        SetEntityDiceStats(entityDiceStatHolder);
    }

    private void HideEntityInfo()
    {
        FadeTo(infoGroup, 0, 0.25f);
    }

    private void SetEntityIconColor()
    {
        entityIcon.color = entity.playerColor;
    }

    private void SetEntityName()
    {
        entityName.text = $"{entity.entityName}";
    }

    private void SetMoney()
    {
        if (entity.heldPoints < 0)
        {
            // Red numbers when negative balance
            entityScore.text = $"<color=red> {entity.heldPoints}</color>";
        }
        else
        {
            entityScore.text = $"{entity.heldPoints}";

        }
    }

    private void SetHealth()
    {
        entityHP.text = $"<color=#4DCF56>HP</color> {entity.health}<size=18>/" +
            $"{entity.maxHealth * entity.currentStatsModifier.maxHealthMultModifier + entity.currentStatsModifier.maxHealthFlatModifier}</size>";
    }

    public void SetEntityDiceStats(GameObject diceStats)
    {
        // Visually updates the dice stats ui based on the entity and side
        entityDiceNumbers.Clear();

        // Goes through the diceStats UI List and finds the text components
        foreach (Transform child in diceStats.transform)
        {
            entityDiceNumbers.Add(child.GetComponentInChildren<TextMeshProUGUI>());
        }

        // Updates each individual dice from the text list based on the type
        // the following code is ABSOLUTELY DISGUSTING
        var faceIndex = 0;

        for (int i = 0; i < 6; i++)
        {
            entityDiceNumbers[i].text = $"{entity.strDie[faceIndex]}";
            faceIndex++;
        }

        faceIndex = 0;

        for (int i = 6; i < 12; i++)
        {
            entityDiceNumbers[i].text = $"{entity.dexDie[faceIndex]}";
            faceIndex++;
        }

        faceIndex = 0;

        for (int i = 12; i < 18; i++)
        {
            entityDiceNumbers[i].text = $"{entity.intDie[faceIndex]}";
            faceIndex++;
        }
    }

    public void FadeTo(CanvasGroup group, float alphaValue, float duration) // ishould prob consolide to one function like this
    {
        DOTween.Kill(group.gameObject);
        DOTween.To(() => group.alpha, x => group.alpha = x, alphaValue, duration);
    }
}
