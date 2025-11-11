using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitMotion;
using TMPro;

public class CombatDiceInfoHolder : MonoBehaviour
{
    //[SerializeField] private Transform additionalDiceParent;
    //[SerializeField] private List<GameObject> additionalDice = new List<GameObject>();
    [SerializeField] private bool isChildDie = false;

    [Header("Motion Info")]
    [SerializeField] private float yOffset = 2;
    [SerializeField] private float zOffset = -2;
    [SerializeField] private float scaleDuration;
    [SerializeField] private float endScale = 1.25f;
    private MotionHandle currentMotion;

    [Header("Die Info")]
    [SerializeField] private Action.WeaponTypes statType;
    [SerializeField] private TMP_ColorGradient defaultStatGradient;
    [SerializeField] private TMP_ColorGradient buffedStatGradient;
    [SerializeField] private List<TextMeshPro> diceStatNumbers; // TMP for 3D objects

    [SerializeField] private GameObject effectUsePrefab;

    public EntityPiece associatedEntity;

    [Header("Listen on Event Channels")]
    public PlayerEventChannelSO m_CombatDiceRolled;
    //public PlayerFloatActionTypeEventChannelSO m_StoreDiceRolled;
    //public EntityActionEventChannelSO m_BothActionsSelected;

    private void OnEnable()
    {
        m_CombatDiceRolled.OnEventRaised += OnCombatDiceRolled;
        //m_BothActionsSelected.OnEventRaised += OnBothActionsSelected;
    }

    private void OnDisable()
    {
        m_CombatDiceRolled.OnEventRaised -= OnCombatDiceRolled;
        //m_BothActionsSelected.OnEventRaised -= OnBothActionsSelected;
    }

    private void OnCombatDiceRolled(EntityPiece entity)
    {
        if (entity != associatedEntity) return;

        StartCoroutine(DelayDiceDestroy(0.08f));
        //DiceDisappear(scaleDuration);
    }

    private IEnumerator DelayDiceDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (effectUsePrefab)
        {
            var vfx = Instantiate(effectUsePrefab);
            vfx.transform.position = transform.position;
            Destroy(vfx, .5f);

            //if (isChildDie) 
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = Vector3.zero;
    }

    private void DiceAppear(float duration)
    {
        if (currentMotion.IsActive()) currentMotion.Cancel();

        currentMotion = LMotion.Create(transform.localScale, Vector3.one * endScale, duration)
            .WithEase(Ease.InQuad)
            .Bind(x => transform.localScale = x)
            .AddTo(this.gameObject);
    }

    private void DiceResizeTo(float scale, float duration)
    {
        if (currentMotion.IsActive()) currentMotion.Cancel();

        currentMotion = LMotion.Create(transform.localScale, Vector3.one * scale, duration)
            .WithEase(Ease.InQuad)
            .Bind(x => transform.localScale = x)
            .AddTo(this.gameObject);
    }

    private void DiceDisappear(float duration)
    {
        //Debug.Log("disappearing dice...");
        if (currentMotion.IsActive()) currentMotion.Cancel();

        currentMotion = LMotion.Create(transform.localScale, Vector3.zero, duration)
            .WithEase(Ease.OutQuad)
            .Bind(x => transform.localScale = x)
            .AddTo(this.gameObject);
    }

    public void UpdateDiceNumbers(Action.PhaseTypes phase)
    {
        bool attacking;
        switch (phase)
        {
            case Action.PhaseTypes.Attack:
                attacking = true;
                break;

            case Action.PhaseTypes.Defend:
                attacking = false;
                break;
            default:
                attacking = true;
                break;

        }

        var p = associatedEntity;
        // Change the die face numbers based on the player's movement mods
        var ti = (int)statType; // type index

        DieConfig die = p.entityStats.dieConfigs[ti];

        var statDieFlatMod = p.currentStatsModifier.dieModifiers[ti].finalResultFlatModifier;
        var statDieMultMod = p.currentStatsModifier.dieModifiers[ti].finalResultMultModifier;

        TMP_ColorGradient gradient;

        if ((statDieFlatMod == 0 && statDieMultMod == 1) || !attacking)
        {
            gradient = defaultStatGradient;
        }
        else
        {
            gradient = buffedStatGradient;
        }

        //Debug.Log($"Flat Mode: {statDieFlatMod} | Mult Mod: {statDieMultMod}");

        for (int i = 0; i < diceStatNumbers.Count; i++)
        {
            // $"{(int)((entity.intDie[faceIndex] * intDieMultMod) + intDieFlatMod)}";
            int number = (int)((die[i] * statDieMultMod) + statDieFlatMod); ;// (((i + 1) * multMoveMod) + flatMoveMod);

            diceStatNumbers[i].colorGradientPreset = gradient;

            if(attacking)
                diceStatNumbers[i].text = "" + number;
            else
                diceStatNumbers[i].text = "" + die[i];
        }
    }

    public void SetupCombatDice(EntityPiece p, Action action)
    {
        associatedEntity = p;
        UpdateDiceNumbers(action.phase);
        DiceAppear(scaleDuration);

        if (p.currentStatsModifier.rollModifier > 0
            && action.phase != Action.PhaseTypes.Defend)
        {
            for (int i = 0; i < p.currentStatsModifier.rollModifier; i++)
            {
                var newDie = Instantiate(this.gameObject, transform.parent);
                newDie.GetComponent<CombatDiceInfoHolder>().isChildDie = true;
                newDie.GetComponent<CombatDiceInfoHolder>().DiceAppear(scaleDuration);

                var x = i * 4 - 2;
                var targetPos = transform.position + new Vector3(x, -.4f, 0);
                newDie.transform.position = targetPos;

                //LMotion.Create(newDie.transform.position, targetPos, .15f)
                //    .WithEase(Ease.InQuad)
                //    .Bind(x => newDie.transform.position = x)
                //    .AddTo(this.gameObject);
            }
        }
    }
}
