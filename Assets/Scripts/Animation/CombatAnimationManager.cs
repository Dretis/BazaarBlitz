using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatAnimationManager : MonoBehaviour
{
    private Animator animator;
    private EntityPiece thisEntity;

    [SerializeField] private CombatUIManager.FightingPosition fightingPosition;
    private Action.PhaseTypes phaseType;

    [Header("Broadcast on Event Channels")]
    public VoidEventChannelSO m_AttackImpact;

    // The following 4 events are temporary?
    public VoidEventChannelSO m_IneffectiveAttack;
    public VoidEventChannelSO m_EffectiveAttack;
    public VoidEventChannelSO m_SuperEffectiveAttack;
    public VoidEventChannelSO m_SomeoneDied;

    [Header("Listen on Event Channels")]
    //public PlayerEventChannelSO m_DecidedTurnOrder; // pass in the attacker
    public PlayerEventChannelSO m_SwapPhase; // void event
    public EntityActionPhaseEventChannelSO m_ActionSelected; // Entity, check side and phase | Either the attacker or defender picked an action
    public EntityActionEventChannelSO m_BothActionsSelected; // prep time to show what they picked, follow with the dice roll too
    public DamageEventChannelSO m_DiceRolled; // 2 floats
    public PlayerEventChannelSO m_PlayOutCombat; // play attack anim and defend anim
    public DamageEventChannelSO m_DamageTaken; //upon attack anim finishing, show floating dmg ontop of defender, play hurt anim
    public EntityItemEventChannelSO m_EntityDied; // someone's HP dropped to 0, Victory, show rewards
    public VoidEventChannelSO m_Stalemate; // Combat is suspended, no one died this time
    public ItemListEventChannelSO m_VictoryAgainstEnemy;

    private void OnEnable()
    {
        m_SwapPhase.OnEventRaised += OnSwapPhase;
        m_ActionSelected.OnEventRaised += OnActionSelected;
        m_BothActionsSelected.OnEventRaised += OnBothActionsSelected;
        m_PlayOutCombat.OnEventRaised += OnPlayOutCombat;
        m_DamageTaken.OnEventRaised += OnDamageTaken;
        m_EntityDied.OnEventRaised += OnEntityDied;
    }

    private void OnDisable()
    {
        m_SwapPhase.OnEventRaised -= OnSwapPhase;
        m_ActionSelected.OnEventRaised -= OnActionSelected;
        m_BothActionsSelected.OnEventRaised -= OnBothActionsSelected;
        m_PlayOutCombat.OnEventRaised -= OnPlayOutCombat;
        m_DamageTaken.OnEventRaised -= OnDamageTaken;
        m_EntityDied.OnEventRaised -= OnEntityDied;
    }


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }
    
    private void OnSwapPhase(EntityPiece entity)
    {
        //if (entity.fightingPosition != fightingPosition) return;

        animator.SetTrigger("ToIdle");
        animator.SetBool("Action Picked", false);
        animator.SetBool("IsAttacking", false);
    }

    private void OnActionSelected(EntityPiece entity, Action.PhaseTypes type)
    {
        if (entity.fightingPosition != fightingPosition) return;

        // Track if attacking or defending here
        phaseType = type;
        if(phaseType == Action.PhaseTypes.Attack) 
            animator.SetBool("IsAttacking", true);
        else 
            animator.SetBool("IsAttacking", false);

        // Animation plays: Baggie stuffs face inward
        animator.SetBool("Action Picked", true);
    }

    private void OnBothActionsSelected(EntityPiece entity, Action action)
    {
        if (entity.fightingPosition != fightingPosition) return;

        // play reveal roll anim
        animator.SetTrigger("Reveal Roll");

        // Prep which animation to play for attacking
        switch (action.type)
        {
            case Action.WeaponTypes.Melee:
                animator.SetInteger("Action ID", 1);
                break;
            case Action.WeaponTypes.Gun:
                animator.SetInteger("Action ID", 2);
                break;
            case Action.WeaponTypes.Magic:
                animator.SetInteger("Action ID", 3);
                break;
        }
    }

    private IEnumerator DelayActionAnimation(float duration)
    {
        yield return new WaitForSeconds(duration);
        animator.SetTrigger("Play Action");
    }

    private void OnPlayOutCombat(EntityPiece entity)
    {
        animator.SetTrigger("Play Action");
        /*
        if (phaseType == Action.PhaseTypes.Attack)
        {
            var animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
            Debug.Log($"AttackAnimation Length:{animationLength}");
            StartCoroutine(WaitForAttackAnimation(animationLength));
        }*/
    }

    private IEnumerator WaitForAttackAnimation(float duration)
    {
        yield return new WaitForSeconds(duration);
        m_AttackImpact.RaiseEvent();
    }

    public void AttackImpact()
    {
        // Call function in Animator as an animation event
        // Basically the moment of impact
        m_AttackImpact.RaiseEvent();
    }

    private void OnDamageTaken(EntityPiece entity, float damage)
    {
        if (entity.fightingPosition != fightingPosition) return;

        if(entity.health <= 0)
        {
            animator.SetTrigger("Death");
            m_SomeoneDied.RaiseEvent();
            return;
        }
        else if (damage >= 50)
        {
            animator.SetInteger("Damage ID", 3); // Effective
            m_SuperEffectiveAttack.RaiseEvent();
        }
        else if (damage <= 15)
        {
            animator.SetInteger("Damage ID", 1); // Ineffective
            m_IneffectiveAttack.RaiseEvent();
        }
        else
        {
            animator.SetInteger("Damage ID", 2); // Regular
            m_EffectiveAttack.RaiseEvent();
        }
        animator.SetTrigger("Take Damage");
    }

    private void OnEntityDied(EntityPiece entity, ItemStats item)
    {
        if (entity.fightingPosition != fightingPosition) return;
        animator.SetTrigger("Death");
    }
}
