using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitMotion;
using TMPro;
using static UnityEngine.RuleTile.TilingRuleOutput;
using System.Linq;

public class SpinDiceInfoHolder : MonoBehaviour
{
    //[SerializeField] private Transform additionalDiceParent;
    //[SerializeField] private List<GameObject> additionalDice = new List<GameObject>();
    [SerializeField] private EntityPiece assignedPlayer;

    [Header("Motion Info")]
    [SerializeField] private bool isChildDie = false;
    [SerializeField] private float yOffset = 2;
    [SerializeField] private float zOffset = -2;
    [SerializeField] private float scaleDuration;
    [SerializeField] private float endScale = 1.25f;
    private MotionHandle currentMotion;

    [Header("Die Info")]
    [SerializeField] private Action.WeaponTypes statType;
    //[SerializeField] private bool isSpeedDie = false;
    [SerializeField] private int index; // 0 = default, 1 = speed
    [SerializeField] private bool currentSelectedDie = true;
    [SerializeField] private TMP_ColorGradient defaultStatGradient;
    [SerializeField] private TMP_ColorGradient buffedStatGradient;
    [SerializeField] private List<TextMeshPro> diceStatNumbers; // TMP for 3D objects

    [SerializeField] GameObject effectUsePrefab;

    [Header("Listen on Event Channels")]
    public PlayerEventChannelSO m_NextPlayerTurn;
    public PlayerEventChannelSO m_DiceRollPrep;
    public PlayerEventChannelSO m_DiceRollUndo;
    public IntEventChannelSO m_RollForMovement;
    public IntEventChannelSO m_ChangeToMoveDie;

    public PlayerEventChannelSO m_EnterLevelUp; //maybe temp

    private void OnEnable()
    {
        m_NextPlayerTurn.OnEventRaised += OnNextPlayerTurn;
        m_DiceRollPrep.OnEventRaised += OnDiceRollPrep;
        m_DiceRollUndo.OnEventRaised += OnDiceRollUndo;
        m_RollForMovement.OnEventRaised += OnRollForMovement;
        m_ChangeToMoveDie.OnEventRaised += OnChangeToMoveDie;


        m_EnterLevelUp.OnEventRaised += OnEnterLevelUp;
    }

    private void OnDisable()
    {
        //m_NextPlayerTurn.OnEventRaised -= OnNextPlayerTurn;
        m_DiceRollPrep.OnEventRaised -= OnDiceRollPrep;
        m_DiceRollUndo.OnEventRaised -= OnDiceRollUndo;
        m_RollForMovement.OnEventRaised -= OnRollForMovement;
        m_ChangeToMoveDie.OnEventRaised -= OnChangeToMoveDie;

        m_EnterLevelUp.OnEventRaised -= OnEnterLevelUp;

        transform.localScale = Vector3.zero;
    }

    private void OnNextPlayerTurn(EntityPiece p)
    {
        if (GameplayTest.instance.currentPlayer != assignedPlayer) return;

        switch (index)
        {
            case 0:
                currentSelectedDie = true;
                break;
            case 1:
                currentSelectedDie = false;
                break;
        }
    }

    private void OnDiceRollPrep(EntityPiece p)
    {
        if (GameplayTest.instance.currentPlayer != assignedPlayer) return;

        UpdateDiceNumbers(p);

        // Make more dice based on roll mods
        if (p.currentStatsModifier.rollModifier > 0)
        {
            for (int i = 0; i < p.currentStatsModifier.rollModifier; i++)
            {
                var newDie = Instantiate(this.gameObject);
                newDie.GetComponent<SpinDiceInfoHolder>().isChildDie = true;
                newDie.GetComponent<SpinDiceInfoHolder>().DiceAppear(scaleDuration);

                var x = i * 2 - 1;
                var targetPos = transform.position + new Vector3(x, -.2f, 0);
                newDie.transform.SetParent(transform.parent);
                newDie.transform.position = targetPos;

                //LMotion.Create(newDie.transform.position, targetPos, .15f)
                //    .WithEase(Ease.InQuad)
                //    .Bind(x => newDie.transform.position = x)
                //    .AddTo(this.gameObject);
            }
        }
        if (!currentSelectedDie) return;

        //transform.position = p.transform.position + new Vector3(0, yOffset, zOffset);
        DiceAppear(scaleDuration);
    }

    private void OnDiceRollUndo(EntityPiece p)
    {
        if (GameplayTest.instance.currentPlayer != assignedPlayer) return;

        if (isChildDie)
        {
            Destroy(gameObject);
        }
        else
            DiceDisappear(scaleDuration);
    }

    private void OnRollForMovement(int roll)
    {
        if (GameplayTest.instance.currentPlayer != assignedPlayer) return;
        if (!currentSelectedDie)
        {
            if (isChildDie) Destroy(gameObject);
            return;
        }

        if (effectUsePrefab)
        {
            var vfx = Instantiate(effectUsePrefab);
            vfx.transform.position = transform.position;
            Destroy(vfx, .5f);

            if (isChildDie) Destroy(gameObject, .5f);
        }

        DiceDisappear(scaleDuration);
    }

    private void OnChangeToMoveDie(int i)
    {
        if (GameplayTest.instance.currentPlayer != assignedPlayer) return;

        if (i == index)
        {
            currentSelectedDie = true;
            DiceAppear(0.5f);
        }
        else
        {
            currentSelectedDie = false;
            DiceDisappear(0.2f);
        }
    }

    private void OnEnterLevelUp(EntityPiece p)
    {
        if (GameplayTest.instance.currentPlayer != assignedPlayer) return;
        DiceDisappear(scaleDuration);
    }

    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = Vector3.zero;
    }

    private void DiceAppear(float duration)
    {
        if(!currentSelectedDie && isChildDie)
        {
            return;
        }

        if (currentMotion.IsActive()) currentMotion.Cancel();

        currentMotion = LMotion.Create(transform.localScale, Vector3.one * endScale, duration)
            //.WithEase(Ease.InQuad)
            .WithEase(Ease.OutBack)
            .Bind(x => transform.localScale = x)
            .AddTo(this.gameObject);

        GetComponent<DiceSpin>().canSpin = true;
    }

    private void DiceDisappear(float duration)
    {
        //Debug.Log("disappearing dice... 1");
        if (currentMotion.IsActive()) currentMotion.Cancel();

        currentMotion = LMotion.Create(transform.localScale, Vector3.zero, duration)
            .WithEase(Ease.OutQuad)
            .Bind(x => transform.localScale = x)
            .AddTo(this.gameObject);

        GetComponent<DiceSpin>().canSpin = false;
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

        var ti = (int)statType;
        var die = new List<int> { 1,2,3,4,5,6 };
        if (statType == Action.WeaponTypes.Speed)
        {
            die = p.entityStats.dieConfigs[(int)Action.WeaponTypes.Speed].GetAllFaceValues().ToList();
        }

        for(int i = 0; i < diceStatNumbers.Count; i++)
        {
            int number = ((die[i] * multMoveMod) + flatMoveMod);

            diceStatNumbers[i].colorGradientPreset = gradient;
            diceStatNumbers[i].text = "" + number;
        }
    }
}
