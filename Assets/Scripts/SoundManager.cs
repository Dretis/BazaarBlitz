using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<AudioClip> soundList;
    private AudioClip currentClip;

    private EntityPiece stupidFuck;

    [Header("Listen on Event Channels")]
    public PlayerEventChannelSO m_PassByStamp;
    public PlayerEventChannelSO m_DiceRollPrep;
    public IntEventChannelSO m_RollForMovement;

    public IntEventChannelSO m_PlayerScoreIncreased;
    public IntEventChannelSO m_PlayerScoreDecreased;
    public IntEventChannelSO m_ItemUsed;
    public IntEventChannelSO m_ItemBought;

    public VoidEventChannelSO m_UsedMeleeAttack;
    public VoidEventChannelSO m_UsedMagicAttack;
    public VoidEventChannelSO m_UsedGunAttack;

    public VoidEventChannelSO m_IneffectiveAttack;
    public VoidEventChannelSO m_EffectiveAttack;
    public VoidEventChannelSO m_SuperEffectiveAttack;
    public VoidEventChannelSO m_SomeoneDied;

    public VoidEventChannelSO m_AttackImpact;

    public EntityItemEventChannelSO m_EntityDied;
    public EntityActionPhaseEventChannelSO m_ActionSelected;
    public DamageEventChannelSO m_DamageTaken;
    public PlayerEventChannelSO m_ShowCombatBanner;
    public PlayerEventChannelSO m_NextPlayerTurn;




    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    
    private void OnEnable()
    {
        //World Events
        m_PassByStamp.OnEventRaised += PlayStampSound;
        
        m_DiceRollPrep.OnEventRaised += PlayDiceRollSound;
        m_RollForMovement.OnEventRaised += PlayDiceHitSound;
        m_ItemUsed.OnEventRaised += PlayUseItemSound;
        m_PlayerScoreDecreased.OnEventRaised += PlayCurrencyDecreasedSound;
        m_PlayerScoreIncreased.OnEventRaised += PlayCurrencyIncreasedSound;
        m_ItemBought.OnEventRaised += PlayItemBoughtSound;
        m_NextPlayerTurn.OnEventRaised += PlayNextPlayerTurnSound;
        //m_ShowCombatBanner.OnEventRaised += PlayEnterBattleSound;


        // //Combat Events
        m_ActionSelected.OnEventRaised += PlaySelectCombatActionSound;
        m_UsedMeleeAttack.OnEventRaised += PlayMeleeAttackSound;
        m_UsedMagicAttack.OnEventRaised += PlayMagicAttackSound;
        m_UsedGunAttack.OnEventRaised += PlayGunAttackSound;
        m_IneffectiveAttack.OnEventRaised += PlayNotEffectiveHitSound;
        m_EffectiveAttack.OnEventRaised += PlayEffectiveHitSound;
        m_SuperEffectiveAttack.OnEventRaised += PlaySuperEffectiveHitSound;
        m_SomeoneDied.OnEventRaised += PlayDeathSound;


    }

    private void OnDisable()
    {
        //World Events
        m_PassByStamp.OnEventRaised -= PlayStampSound;
        m_DiceRollPrep.OnEventRaised -= PlayDiceRollSound;
        m_RollForMovement.OnEventRaised -= PlayDiceHitSound;
        m_ItemUsed.OnEventRaised -= PlayUseItemSound;
        m_PlayerScoreDecreased.OnEventRaised -= PlayCurrencyDecreasedSound;
        m_PlayerScoreIncreased.OnEventRaised -= PlayCurrencyIncreasedSound;
        m_NextPlayerTurn.OnEventRaised -= PlayNextPlayerTurnSound;
        //m_ShowCombatBanner.OnEventRaised -= PlayEnterBattleSound;


        // //Combat Events
        m_ActionSelected.OnEventRaised -= PlaySelectCombatActionSound;
        m_UsedMeleeAttack.OnEventRaised -= PlayMeleeAttackSound;
        m_UsedMagicAttack.OnEventRaised -= PlayMagicAttackSound;
        m_UsedGunAttack.OnEventRaised -= PlayGunAttackSound;
        m_IneffectiveAttack.OnEventRaised -= PlayNotEffectiveHitSound;
        m_EffectiveAttack.OnEventRaised -= PlayEffectiveHitSound;
        m_SuperEffectiveAttack.OnEventRaised -= PlaySuperEffectiveHitSound;
        m_SomeoneDied.OnEventRaised -= PlayDeathSound;
    }

    private void PlayStampSound(EntityPiece entity)
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/");
    }

    private void PlayDiceRollSound(EntityPiece entity)
    {
        audioSource.clip = soundList[1];
        audioSource.Play();
        currentClip = soundList[1];
    }

    private void PlayDiceHitSound(int diceValue)
    {
        audioSource.Stop();
        audioSource.clip = null;
        FMODUnity.RuntimeManager.PlayOneShot("event:/");
    }

    private void PlayCurrencyDecreasedSound(int scoreDifference)
    {
        Debug.Log("This is the score difference " + scoreDifference);
        FMODUnity.RuntimeManager.PlayOneShot("event:/");
    }

    private void PlayCurrencyIncreasedSound(int scoreDifference)
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/");
    }

    private void PlayItemBoughtSound(int itemValue)
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/");
    }

    private void PlayUseItemSound(int item)
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/");
    }

    private void PlayMeleeAttackSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/");
    }

    private void PlayMagicAttackSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/");
    }

    private void PlayGunAttackSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/");
    }

    private void PlayNotEffectiveHitSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/");
    }

    private void PlayEffectiveHitSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/");
    }

    private void PlaySuperEffectiveHitSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/");
    }

    private void PlayDeathSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/");
    }

    private void PlaySelectCombatActionSound(EntityPiece entity, Action.PhaseTypes phase)
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/");
    }

    private void PlayEnterBattleSound(EntityPiece entity)
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/");
    }

    private void PlayNextPlayerTurnSound(EntityPiece entity)
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/");
    }

    private void PlayMoveSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Move");
    }

    /*
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            PlayStampSound(stupidFuck);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PlayDiceRollSound(stupidFuck);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            PlayDiceHitSound(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            PlayCurrencyChangeSound(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            PlayCurrencyChangeSound(-1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            PlayUseItemSound(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            PlayMeleeAttackSound(stupidFuck);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            PlayMagicAttackSound(stupidFuck);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            PlayGunAttackSound(stupidFuck);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            PlayNotEffectiveHitSound(stupidFuck);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            PlayEffectiveHitSound(stupidFuck);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            PlaySuperEffectiveHitSound(stupidFuck);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            PlaySelectCombatActionSound(stupidFuck);
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            PlayEnterBattleSound(stupidFuck);
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            PlayDeathSound(stupidFuck);
        }
        else if (Input.GetKeyDown(KeyCode.Y))
        {
            PlayNextPlayerTurnSound(stupidFuck);
        }
    }
    */


    
}
