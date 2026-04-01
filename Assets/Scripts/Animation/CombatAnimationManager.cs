using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine.UIElements;
//using DG.Tweening;

public class CombatAnimationManager : MonoBehaviour
{
    private Animator animator;
    private EntityPiece thisEntity;

    [SerializeField] private CombatUIManager.FightingPosition fightingPosition;
    private Action.PhaseTypes phaseType;
    private int rolledNumber;

    private MotionHandle currentMotion;

    [Header("Additional Objects")]
    [SerializeField] private GameObject boatGroup; // the whole ass group 
    [SerializeField] private GameObject boat;
    [SerializeField] private BoatMotionManager boatMotionManager;
    [SerializeField] private PlayerPaletteLoader combatBoatPaletteLoader;
    private Vector3 boatInitialPos;
    [SerializeField] private GameObject gunSniperCrosshair;

    [Header("Extra FX")]
    [SerializeField] private ParticleSystem coinDrop;
    [SerializeField] private ParticleSystem gunShotgun;
    [SerializeField] private ParticleSystem gunSniperShoot;
    [SerializeField] private ParticleSystem gunShrapnel;

    [SerializeField] private ParticleSystem magicSparkle;
    [SerializeField] private ParticleSystem magicAura;

    [SerializeField] private ParticleSystem magicExplosion;
    [SerializeField] private ParticleSystem magicExplosion2;
    [SerializeField] private ParticleSystem magicExplosion3;

    [SerializeField] private ParticleSystem magicWard;
    [SerializeField] private ParticleSystem windRing;
    //[SerializeField] private ParticleSystem gunSniper;
    

    [Header("Broadcast on Event Channels")]
    public VoidEventChannelSO m_AttackImpact;
    public VoidEventChannelSO m_MeleeWindup;
    public VoidEventChannelSO m_GunWindup;
    public VoidEventChannelSO m_MagicWindup;

    public VoidEventChannelSO m_Shotgun2Windup;
    public VoidEventChannelSO m_Shotgun2Shoot;

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

    public PlayerEventChannelSO m_CombatDiceRolled;
    public PlayerFloatActionTypeEventChannelSO m_StoreDiceRolled;
    public PlayerEventChannelSO m_PlayOutCombat; // play attack anim and defend anim

    public DamageEventChannelSO m_DamageTaken; //upon attack anim finishing, show floating dmg ontop of defender, play hurt anim
    public EntityItemEventChannelSO m_EntityDied; // someone's HP dropped to 0, Victory, show rewards
    //public VoidEventChannelSO m_Stalemate; // Combat is suspended, no one died this time
    //public IntItemListEventChannelSO m_VictoryAgainstEnemy;
    public VoidEventChannelSO m_EnteredOverworldScene;

    private void OnEnable()
    {
        m_SwapPhase.OnEventRaised += OnSwapPhase;

        m_ActionSelected.OnEventRaised += OnActionSelected;
        m_BothActionsSelected.OnEventRaised += OnBothActionsSelected;

        m_CombatDiceRolled.OnEventRaised += OnCombatDiceRolled;
        m_StoreDiceRolled.OnEventRaised += OnStoreDiceRolled;

        m_PlayOutCombat.OnEventRaised += OnPlayOutCombat;

        m_DamageTaken.OnEventRaised += OnDamageTaken;
        m_EntityDied.OnEventRaised += OnEntityDied;

        m_EnteredOverworldScene.OnEventRaised += OnEnteredOverworldScene;
    }

    private void OnDisable()
    {
        m_SwapPhase.OnEventRaised -= OnSwapPhase;

        m_ActionSelected.OnEventRaised -= OnActionSelected;
        m_BothActionsSelected.OnEventRaised -= OnBothActionsSelected;

        m_CombatDiceRolled.OnEventRaised -= OnCombatDiceRolled;
        m_StoreDiceRolled.OnEventRaised -= OnStoreDiceRolled;

        m_PlayOutCombat.OnEventRaised -= OnPlayOutCombat;

        m_DamageTaken.OnEventRaised -= OnDamageTaken;
        m_EntityDied.OnEventRaised -= OnEntityDied;

        m_EnteredOverworldScene.OnEventRaised -= OnEnteredOverworldScene;
    }


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        boatInitialPos = boat.transform.position;
    }

    public void SetCombatBoatPalette(EntityPiece entity)
    {
        if (entity.pBoatLoader.TryGetComponent<PlayerPaletteLoader>(out PlayerPaletteLoader pal))
        {
            combatBoatPaletteLoader.SetInspectorPalette(pal.GetInspectorPalette());
        }
    }


    private void OnSwapPhase(EntityPiece entity)
    {
        //if (entity.fightingPosition != fightingPosition) return;

        animator.SetTrigger("ToIdle");
        animator.SetBool("Action Picked", false);
        animator.SetBool("IsAttacking", false);
        animator.ResetTrigger("Reveal Roll");

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
        //animator.SetTrigger("Reveal Roll");

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

        // spawn associated dice by type
        SpawnCombatDie(entity, action);
    }

    private void SpawnCombatDie(EntityPiece entity, Action action)
    {
        GameObject diePrefab;
        switch (action.type)
        {
            case Action.WeaponTypes.Melee:
                diePrefab = entity.strDieCosmeticPrefab;
                break;

            case Action.WeaponTypes.Gun:
                diePrefab = entity.dexDieCosmeticPrefab;
                break;

            case Action.WeaponTypes.Magic:
                diePrefab = entity.intDieCosmeticPrefab;
                break;

            default:
                Debug.Log("??? action is special or not any type | defaulting to melee die");
                diePrefab = entity.strDieCosmeticPrefab;
                break;
        }

        var die = Instantiate(diePrefab, transform);
        die.transform.position += new Vector3(0, 5, -0.5f);
        die.transform.localScale = Vector3.zero;
        die.GetComponent<CombatDiceInfoHolder>().SetupCombatDice(entity, action);
    }

    private void OnCombatDiceRolled(EntityPiece entity)
    {
        if (entity.fightingPosition != fightingPosition) return;

        animator.SetTrigger("Reveal Roll");
        //animator.ResetTrigger("Reveal Roll");
    }

    private void OnStoreDiceRolled(EntityPiece entity, float roll, Action.WeaponTypes type)
    {
        if (entity.fightingPosition != fightingPosition) return;

        rolledNumber = (int)roll;
        animator.SetInteger("Rolled Number", rolledNumber);
    }

    private IEnumerator DelayActionAnimation(float duration)
    {
        yield return new WaitForSeconds(duration);
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

    private void OnEnteredOverworldScene()
    {
        boatMotionManager.StopPBoatMotion();
    }

    // Helper functions, called by Animation Events
    #region SFX Call Event Functions
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

    private void Shotgun2WindupSFX(float volume)
    {
        m_Shotgun2Windup.RaiseEvent();
    }

    private void Shotgun2ShootSFX(float volume)
    {
        m_Shotgun2Shoot.RaiseEvent();
    }
    #endregion

    #region VFX Particle Functions
    private void BurstCoinDropParticle()
    {
        coinDrop.Play();
    }

    private void BurstShotgun()
    {
        var numOfPellets = rolledNumber * 2;
        var shotgunBurst = new ParticleSystem.Burst(0f, numOfPellets); //float_time, short_count
        //gunShotgun.Emit(numOfPellets);
        //Debug.Log($"shotgunBurst.count = {shotgunBurst.count}");
        //shotgunBurst.count = numOfPellets;
        //Debug.Log($"shotgunBurst.count after = {shotgunBurst.count}");
        gunShotgun.emission.SetBurst(0, shotgunBurst);
        gunShotgun.Play();
    }

    private void BurstSniperShoot()
    {
        gunSniperShoot.Play();
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

    private void BurstMagicAura()
    {
        // "feels the aura"
        magicAura.gameObject.SetActive(true);
        magicAura.Play();
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

    private void BurstMagicExplosion3()
    {
        // big ass explosion
        magicExplosion3.gameObject.SetActive(true);
        magicExplosion3.Play();
    }

    private void BurstMagicWard()
    {
        magicWard.gameObject.SetActive(true);
        magicWard.Play();
    }

    private void BurstWindRingFX()
    {
        windRing.gameObject.SetActive(true);
        windRing.Play();
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
    #endregion

    #region Boat Positional Functions
    private void MoveBoatToEntity(float duration)
    {
        if(boatGroup == null) return;

        //Debug.Log(gameObject);
        //boat.transform.DOMoveX(gameObject.transform.position.x, duration).From(boat.transform.position)
        //    .SetEase(Ease.OutQuad);

        var bt = boatGroup.transform;

        if (currentMotion.IsActive()) currentMotion.Cancel();

        currentMotion = LMotion.Create(bt.position.x, gameObject.transform.position.x, duration)
            .WithEase(Ease.OutQuad)
            .BindToPositionX(bt);
    }

    private void MoveBoatToX(float xPosition)
    {
        if (boatGroup == null) return;

        //Debug.Log(gameObject);
        //boat.transform.DOLocalMoveX(xPosition, .5f).From(boat.transform.position);

        var bt = boatGroup.transform;

        if (currentMotion.IsActive()) currentMotion.Cancel();
        
        currentMotion = LMotion.Create(bt.localPosition.x, xPosition, .2f)
            .WithEase(Ease.OutCubic)
            .BindToLocalPositionX(bt);
    }

    private void MoveBoatToEnemy(float duration)
    {
        if (boatGroup == null) return;

        //Debug.Log(gameObject);
        //boat.transform.DOLocalMoveX(xPosition, .5f).From(boat.transform.position);

        var bt = boatGroup.transform;

        if (currentMotion.IsActive()) currentMotion.Cancel();

        currentMotion = LMotion.Create(bt.localPosition.x, -5, duration)
            .WithEase(Ease.OutCubic)
            .BindToLocalPositionX(bt);
    }

    private void MoveBoatToY(float yPosition, float duration)
    {
        if (boatGroup == null) return;

        //Debug.Log(gameObject);
        //boat.transform.DOLocalMoveX(xPosition, .5f).From(boat.transform.position);

        var bt = boatGroup.transform;

        if (currentMotion.IsActive()) currentMotion.Cancel();

        currentMotion = LMotion.Create(bt.localPosition.x, yPosition, .15f)
            .WithEase(Ease.OutBack)
            .BindToLocalPositionY(bt);
    }

    private void ResetBoatPosition(float duration)
    {
        if (boatGroup == null) return;

        //Debug.Log(gameObject);
        //boat.transform.DOMoveX(boatInitialPos.x, duration).From(boat.transform.position);

        var bt = boatGroup.transform;

        if (currentMotion.IsActive()) currentMotion.Cancel();

        currentMotion = LMotion.Create(bt.position.x, boatInitialPos.x, duration)
            //.WithEase(Ease.OutQuad)
            .BindToPositionX(bt);
    }
    #endregion
}
