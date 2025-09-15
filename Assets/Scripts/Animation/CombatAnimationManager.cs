using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CombatAnimationManager : MonoBehaviour
{
    private Animator animator;
    private EntityPiece thisEntity;

    [SerializeField] private CombatUIManager.FightingPosition fightingPosition;
    private Action.PhaseTypes phaseType;

    [Header("Additional Objects")]
    [SerializeField] private GameObject boat;
    private Vector3 boatInitialPos;
    [SerializeField] private GameObject gunSniperCrosshair;

    [Header("Extra FX")]
    [SerializeField] private ParticleSystem coinDrop;
    [SerializeField] private ParticleSystem gunShotgun;
    [SerializeField] private ParticleSystem gunShrapnel;
    [SerializeField] private ParticleSystem magicSparkle;
    [SerializeField] private ParticleSystem magicExplosion;
    [SerializeField] private ParticleSystem magicExplosion2;
    [SerializeField] private ParticleSystem magicWard;
    //[SerializeField] private ParticleSystem gunSniper;
    

    [Header("Broadcast on Event Channels")]
    public VoidEventChannelSO m_AttackImpact;
    public VoidEventChannelSO m_MeleeWindup;
    public VoidEventChannelSO m_GunWindup;
    public VoidEventChannelSO m_MagicWindup;

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
    //public VoidEventChannelSO m_Stalemate; // Combat is suspended, no one died this time
    //public IntItemListEventChannelSO m_VictoryAgainstEnemy;

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

        boatInitialPos = boat.transform.position;
    }
    
    private void OnSwapPhase(EntityPiece entity)
    {
        //if (entity.fightingPosition != fightingPosition) return;

        animator.SetTrigger("ToIdle");
        animator.SetBool("Action Picked", false);
        animator.SetBool("IsAttacking", false);

        //.transform.position = boatInitialPos;
        ResetBoatPosition(.2f);
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

        Debug.Log(entity + " | " + action + " | " + action.type);

        // Prep which animation to play for attacking
        switch (action.type)
        {
            case Action.WeaponTypes.Melee:
                animator.SetInteger("Action Type ID", 1);
                break;
            case Action.WeaponTypes.Gun:
                animator.SetInteger("Action Type ID", 2);
                break;
            case Action.WeaponTypes.Magic:
                animator.SetInteger("Action Type ID", 3);
                break;
        }

        // Find out what the action anim to play
        animator.SetInteger("Weapon ID", action.weaponID);
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

    private void OnDamageTaken(EntityPiece entity, float damage, CombatManager.TypeAdvantage advantage)
    {
        if (entity.fightingPosition != fightingPosition) return;

        if(entity.health <= 0)
        {
            if (damage >= 50)
            {   
                m_SuperEffectiveAttack.RaiseEvent();
            }
            else if (damage <= 15)
            {
                m_IneffectiveAttack.RaiseEvent();
            }
            else
            {
                m_EffectiveAttack.RaiseEvent();
            }
            animator.SetTrigger("Death");
            m_SomeoneDied.RaiseEvent();
            return;
        }
        else
        {
            switch(advantage)
            {
                case CombatManager.TypeAdvantage.Resist:
                    animator.SetInteger("Damage ID", 1); // Ineffective
                    m_IneffectiveAttack.RaiseEvent();
                    break;
                case CombatManager.TypeAdvantage.Neutral:
                    animator.SetInteger("Damage ID", 2); // Regular
                    m_EffectiveAttack.RaiseEvent();
                    break;
                case CombatManager.TypeAdvantage.Strong:
                    animator.SetInteger("Damage ID", 3); // Effective
                    m_SuperEffectiveAttack.RaiseEvent();
                    break;
                default:
                    Debug.Log("No type advantage???");
                    break;
            }
        }
        animator.SetTrigger("Take Damage");
        /*
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
        */
    }

    private void OnEntityDied(EntityPiece entity, ItemStats item)
    {
        if (entity.fightingPosition != fightingPosition) return;
        animator.SetTrigger("Death");

        Debug.Log("I died lol");
    }

    // Helper functions
    private void MeleeWindupSFX(float volume)
    {
        m_MeleeWindup.RaiseEvent();
    }

    private void GunWindupSFX(float volume)
    {
        m_GunWindup.RaiseEvent();
    }

    private void MagicWindupSFX(float volume)
    {
        m_MagicWindup.RaiseEvent();
    }

    private void BurstCoinDropParticle()
    {
        coinDrop.Play();
    }

    private void BurstShotgun()
    {
        gunShotgun.Play();
    }

    private void BurstShrapnel()
    {
        gunShrapnel.Play();
    }

    private void BurstMagicSparkle()
    {
        magicSparkle.gameObject.SetActive(true);
        magicSparkle.Play();
    }

    private void BurstMagicExplosion()
    {
        magicExplosion.gameObject.SetActive(true);
        magicExplosion.Play();
    }

    private void BurstMagicExplosion2()
    {
        magicExplosion2.gameObject.SetActive(true);
        magicExplosion2.Play();
    }

    private void BurstMagicWard()
    {
        magicWard.gameObject.SetActive(true);
        magicWard.Play();
    }

    private void SniperScopeIn()
    {
        // m_SniperScopeIn.RaiseEvent(fightingPosition);
        gunSniperCrosshair.SetActive(true);
    }

    private void SniperShot()
    {
        gunSniperCrosshair.SetActive(false);
        gunShrapnel.Play();
    }

    private void CancelWardEffect()
    {
        magicWard.gameObject.SetActive(false);
    }

    private void MoveBoatToEntity(float duration)
    {
        if(boat == null) return;

        //Debug.Log(gameObject);
        boat.transform.DOMoveX(gameObject.transform.position.x, duration).From(boat.transform.position)
            .SetEase(Ease.OutQuad);
    }

    private void MoveBoatToX(float xPosition)
    {
        if (boat == null) return;

        //Debug.Log(gameObject);
        boat.transform.DOLocalMoveX(xPosition, 0.25f).From(boat.transform.position)
            .SetEase(Ease.InSine);
    }

    private void ResetBoatPosition(float duration)
    {
        if (boat == null) return;

        //Debug.Log(gameObject);
        boat.transform.DOMoveX(boatInitialPos.x, duration).From(boat.transform.position);
    }
}
