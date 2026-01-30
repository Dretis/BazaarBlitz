using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitMotion;
using UnityEngine.Rendering.Universal;

public class AnimationManager : MonoBehaviour
{
    // This script includes handling Animators/Animations
    [SerializeField] private Animator currentAnimator; //Current player's animator

    [SerializeField] public EntityPiece ep; //test variable delete later

    [Header("Listen On Event Channels")]
    public PlayerEventChannelSO m_NextPlayerTurn;
    public PlayerEventChannelSO m_DiceRollPrep;
    public IntEventChannelSO m_DiceThrown;
    public PlayerEventChannelSO m_ResetToIdle;
    public PlayerEventChannelSO m_EnterLevelUp;
    public VoidEventChannelSO m_ExitLevelUp;

    public PlayerEventChannelSO m_CheerForPlayer;
    public EntityIntEventChannelSO m_DamageTakenOnPlayer;

    private void OnEnable()
    {
        m_NextPlayerTurn.OnEventRaised += SetCurrentPlayerAnimator;
        m_DiceRollPrep.OnEventRaised += DiceIsRolling;
        m_DiceThrown.OnEventRaised += DiceIsThrown;
        m_ResetToIdle.OnEventRaised += ResetToIdleAnim;
        m_EnterLevelUp.OnEventRaised += OnEnterLevelUp;
        m_ExitLevelUp.OnEventRaised += OnExitLevelUp;

        m_CheerForPlayer.OnEventRaised += OnCheerForPlayer;
        m_DamageTakenOnPlayer.OnEventRaised += OnDamageTakenOnPlayer;
    }

    private void OnDisable()
    {
        m_NextPlayerTurn.OnEventRaised -= SetCurrentPlayerAnimator;
        m_DiceRollPrep.OnEventRaised -= DiceIsRolling;
        m_DiceThrown.OnEventRaised -= DiceIsThrown;
        m_ResetToIdle.OnEventRaised -= ResetToIdleAnim;
        m_EnterLevelUp.OnEventRaised -= OnEnterLevelUp;
        m_ExitLevelUp.OnEventRaised -= OnExitLevelUp;

        m_CheerForPlayer.OnEventRaised -= OnCheerForPlayer;
        m_DamageTakenOnPlayer.OnEventRaised -= OnDamageTakenOnPlayer;
    }

    private void SetCurrentPlayerAnimator(EntityPiece entity)
    {
        if (currentAnimator != null)
            ResetToIdleAnim(null);
        currentAnimator = entity.GetComponentInChildren<Animator>();
    }

    private void DiceIsRolling(EntityPiece entity)
    {
        // currentAnimator = entity.GetComponentInChildren<Animator>();

        if (currentAnimator != null)
            currentAnimator.SetBool("Dice Rolling", true);
    }

    private void DiceIsThrown(int roll)
    {
        // currentAnimator = entity.GetComponentInChildren<Animator>();

        if (currentAnimator != null)
        {
            currentAnimator.SetBool("Dice Thrown", true);
        }

        // Get current animation 
        var state = currentAnimator.GetCurrentAnimatorStateInfo(0);
        Debug.Log($"Current Animation State Length:{state.length}");

        // Send in the duration of the dice throw animation to eventually reset to idle animation
        StartCoroutine(ToBoingAnimAfter(state.length)); // placeholder, change later
    }

    private IEnumerator ToBoingAnimAfter(float duration)
    {
        yield return new WaitForSeconds(duration);

        ResetToAnim(null, "ToBoing");
    }

    private void ResetToAnim(EntityPiece entity, string trigger)
    {
        // currentAnimator = entity.GetComponentInChildren<Animator>();

        if (currentAnimator != null)
        {
            currentAnimator.SetTrigger(trigger);
            currentAnimator.SetBool("Dice Rolling", false);
            currentAnimator.SetBool("Dice Thrown", false);
            currentAnimator.SetBool("Moving", true);
        }
    }

    private void ResetToIdleAnim(EntityPiece entity)
    {
        // currentAnimator = entity.GetComponentInChildren<Animator>();

        if (currentAnimator != null)
        {
            currentAnimator.SetTrigger("ToIdle");
            currentAnimator.SetBool("Dice Rolling", false);
            currentAnimator.SetBool("Dice Thrown", false);
            currentAnimator.SetBool("Moving", false);
        }
    }

    private void OnEnterLevelUp(EntityPiece entity)
    {
        if (currentAnimator != null)
        {
            currentAnimator.SetBool("Dice Rolling", false);
            currentAnimator.SetTrigger("ToCheer");
            currentAnimator.SetBool("Extend Cheer", true);
        }

    }

    private void OnExitLevelUp()
    {
        if (currentAnimator != null)
        {
            currentAnimator.SetTrigger("ToIdle");
            currentAnimator.SetBool("Extend Cheer", false);
            //currentAnimator.SetTrigger("Exit Anim");
        }
    }

    private void OnCheerForPlayer(EntityPiece entity)
    {
        var animator = entity.GetComponentInChildren<Animator>();
        animator.SetTrigger("ToCheer");
    }

    private void OnDamageTakenOnPlayer(EntityPiece entity, int damageTaken)
    {
        var animator = entity.GetComponentInChildren<Animator>();
        animator.SetTrigger("TakeDamage");

        entity.hitParticle.Play();
        StartCoroutine(ShowDamageNumber(entity, damageTaken));
    }

    private IEnumerator ShowDamageNumber(EntityPiece entity, int damageTaken)
    {
        var damageLocalScale = entity.floatingDamageNumber.GetComponent<RectTransform>().localScale;
        damageLocalScale = Vector3.zero;

        LMotion.Create(damageLocalScale, Vector3.one, 0.2f)
            .WithEase(Ease.OutBack)
            .Bind(x => damageLocalScale = x);

        entity.floatingDamageNumber.ShowText($"{damageTaken}!");

        yield return new WaitForSeconds(.5f);

        LMotion.Create(damageLocalScale, Vector3.zero, 0.5f)
            .WithEase(Ease.OutQuad)
            .Bind(x => damageLocalScale = x);

        entity.floatingDamageNumber.StartDisappearingText();
        yield return null;
    }

}
