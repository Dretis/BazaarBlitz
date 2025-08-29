using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitMotion;
using TMPro;
using LitMotion.Extensions;

public class SpinDiceInfoHolder : MonoBehaviour
{
    [Header("Motion Info")]
    [SerializeField] private float yOffset = 2;
    [SerializeField] private float zOffset = -2;
    [SerializeField] private float scaleDuration;
    [SerializeField] private float endScale = 1.25f;

    [Header("Die Info")]
    [SerializeField] private Action.WeaponTypes statType;
    [SerializeField] private TMP_ColorGradient defaultStatGradient;
    [SerializeField] private TMP_ColorGradient buffedStatGradient;
    [SerializeField] private List<TextMeshPro> diceStatNumbers; // TMP for 3D objects

    [SerializeField] GameObject effectUsePrefab;

    [Header("Listen on Event Channels")]
    public PlayerEventChannelSO m_DiceRollPrep;
    public PlayerEventChannelSO m_DiceRollUndo;
    public IntEventChannelSO m_RollForMovement;

    public PlayerEventChannelSO m_EnterLevelUp; //maybe temp

    private void OnEnable()
    {
        m_DiceRollPrep.OnEventRaised += OnDiceRollPrep;
        m_DiceRollUndo.OnEventRaised += OnDiceRollUndo;
        m_RollForMovement.OnEventRaised += OnRollForMovement;

        m_EnterLevelUp.OnEventRaised += OnEnterLevelUp;
    }

    private void OnDisable()
    {
        m_DiceRollPrep.OnEventRaised -= OnDiceRollPrep;
        m_DiceRollUndo.OnEventRaised -= OnDiceRollUndo;
        m_RollForMovement.OnEventRaised -= OnRollForMovement;

        m_EnterLevelUp.OnEventRaised -= OnEnterLevelUp;
    }

    private void OnDiceRollPrep(EntityPiece p)
    {
        transform.position = p.transform.position + new Vector3(0, yOffset, zOffset);

        UpdateDiceNumbers(p);
        DiceAppear(scaleDuration);
    }
    
    private void OnDiceRollUndo(EntityPiece p)
    {
        DiceDisappear(scaleDuration);
    }

    private void OnRollForMovement(int roll)
    {
        if(effectUsePrefab)
        {
            var vfx = Instantiate(effectUsePrefab);
            vfx.transform.position = transform.position;
            Destroy(vfx, .5f);
        }

        DiceDisappear(scaleDuration);
    }

    private void OnEnterLevelUp(EntityPiece p)
    {
        DiceDisappear(scaleDuration);
    }

    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = Vector3.zero;
    }

    private void DiceAppear(float duration)
    {
        LMotion.Create(transform.localScale, Vector3.one * endScale, duration)
            .WithEase(Ease.InQuad)
            .Bind(x => transform.localScale = x);
    }

    private void DiceDisappear(float duration)
    {
        //Debug.Log("disappearing dice...");
        LMotion.Create(transform.localScale, Vector3.zero, duration)
            .WithEase(Ease.OutQuad)
            .Bind(x => transform.localScale = x);
    }

    private void UpdateDiceNumbers(EntityPiece p)
    {
        // Change the die face numbers based on the player's movement mods
        var flatMoveMod = p.currentStatsModifier.movementFlatModifier;
        var multMoveMod = p.currentStatsModifier.movementMultModifier;

        TMP_ColorGradient gradient;

        if( flatMoveMod == 0 && multMoveMod == 1)
        {
            gradient = defaultStatGradient;
        }
        else
        {
            gradient = buffedStatGradient;
        }

        Debug.Log($"Flat Mode: {flatMoveMod} | Mult Mod: {multMoveMod}");

        for(int i = 0; i < diceStatNumbers.Count; i++)
        {
            int number = (((i + 1) * multMoveMod) + flatMoveMod);

            diceStatNumbers[i].colorGradientPreset = gradient;
            diceStatNumbers[i].text = "" + number;
        }
    }
}
