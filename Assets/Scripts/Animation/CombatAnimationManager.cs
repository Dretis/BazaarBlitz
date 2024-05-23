using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatAnimationManager : MonoBehaviour
{
    private Animator animator;
    private EntityPiece thisEntity;

    [SerializeField] private CombatUIManager.FightingPosition fightingPosition;
    private Action.WeaponTypes weaponType;

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
    }

    private void OnActionSelected(EntityPiece entity, Action.PhaseTypes type)
    {
        if (entity.fightingPosition != fightingPosition) return; 

        // Animation plays: Baggie stuffs face inward
        if(type == Action.PhaseTypes.Attack) 
            animator.SetBool("IsAttacking", true);
        else 
            animator.SetBool("IsAttacking", false);

        animator.SetBool("Action Picked", true);
    }

    private void OnBothActionsSelected(EntityPiece entity, Action action)
    {
        if (entity.fightingPosition != fightingPosition) return;

        // play reveal roll anim
        animator.SetTrigger("Reveal Roll");

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

        var state = animator.GetCurrentAnimatorStateInfo(0);
        Debug.Log($"Current Animation State Length:{state.length}");

    }

    private IEnumerator DelayActionAnimation(float duration)
    {
        yield return new WaitForSeconds(duration);
        animator.SetTrigger("Play Action");
    }

    private void OnPlayOutCombat(EntityPiece entity)
    {
        if (entity.fightingPosition != fightingPosition) return;
        animator.SetTrigger("Play Action");
    }

    private void OnDamageTaken(EntityPiece entity, float damage)
    {
        if (entity.fightingPosition != fightingPosition) return;
        if (damage >= 50)
            animator.SetTrigger("Take Damage Strong");
        else 
            animator.SetTrigger("Take Damage");
    }

    private void OnEntityDied(EntityPiece entity, ItemStats item)
    {
        if (entity.fightingPosition != fightingPosition) return;
        animator.SetTrigger("Death");
    }
}
