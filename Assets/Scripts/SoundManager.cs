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
    public IntEventChannelSO m_UpdatePlayerScore;
    public IntEventChannelSO m_ItemUsed;
    public IntEventChannelSO m_ItemBought;
    public PlayerEventChannelSO m_UsedMeleeAttack;
    public PlayerEventChannelSO m_UsedMagicAttack;
    public PlayerEventChannelSO m_UsedGunAttack;

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
        m_UpdatePlayerScore.OnEventRaised += PlayCurrencyChangeSound;
        m_ItemBought.OnEventRaised += PlayItemBoughtSound;
        m_NextPlayerTurn.OnEventRaised += PlayNextPlayerTurnSound;
        //m_ShowCombatBanner.OnEventRaised += PlayEnterBattleSound;


        // //Combat Events
        m_ActionSelected.OnEventRaised += PlaySelectCombatActionSound;
        // m_UsedMeleeAttack += PlayMeleeAttackSound;
        // m_UsedMagicAttack += PlayMagicAttackSound;
        // m_UsedGunAttack += PlayGunAttackSound;
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
        m_UpdatePlayerScore.OnEventRaised -= PlayCurrencyChangeSound;
        m_NextPlayerTurn.OnEventRaised -= PlayNextPlayerTurnSound;
        //m_ShowCombatBanner.OnEventRaised -= PlayEnterBattleSound;


        // //Combat Events
        m_ActionSelected.OnEventRaised -= PlaySelectCombatActionSound;
        // m_UsedMeleeAttack -= PlayMeleeAttackSound;
        // m_UsedMagicAttack -= PlayMagicAttackSound;
        // m_UsedGunAttack -= PlayGunAttackSound;
        m_IneffectiveAttack.OnEventRaised -= PlayNotEffectiveHitSound;
        m_EffectiveAttack.OnEventRaised -= PlayEffectiveHitSound;
        m_SuperEffectiveAttack.OnEventRaised -= PlaySuperEffectiveHitSound;
        m_SomeoneDied.OnEventRaised -= PlayDeathSound;
    }

    private void PlayStampSound(EntityPiece entity)
    {
        audioSource.PlayOneShot(soundList[0], 2f);
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
        audioSource.PlayOneShot(soundList[2], 2f);
    }

    private void PlayCurrencyChangeSound(int scoreDifference)
    {
        if (scoreDifference > 0)
            audioSource.PlayOneShot(soundList[3], 3f);
        else
            audioSource.PlayOneShot(soundList[4], 3f);
    }

    private void PlayItemBoughtSound(int itemValue)
    {
        audioSource.PlayOneShot(soundList[4], 3f);
    }

    private void PlayUseItemSound(int item)
    {
        audioSource.PlayOneShot(soundList[5], 1.5f);
    }

    private void PlayMeleeAttackSound(EntityPiece entity)
    {
        audioSource.PlayOneShot(soundList[6], 2f);
    }

    private void PlayMagicAttackSound(EntityPiece entity)
    {
        audioSource.PlayOneShot(soundList[7], 1.2f);
    }

    private void PlayGunAttackSound(EntityPiece entity)
    {
        audioSource.PlayOneShot(soundList[8], 2f);
    }

    private void PlayNotEffectiveHitSound()
    {
        audioSource.PlayOneShot(soundList[9], 2f);
    }

    private void PlayEffectiveHitSound()
    {
        audioSource.PlayOneShot(soundList[10], 2f);
    }

    private void PlaySuperEffectiveHitSound()
    {
        audioSource.PlayOneShot(soundList[11], 2.5f);
    }

    private void PlayDeathSound()
    {
        audioSource.PlayOneShot(soundList[12], 2f);
    }

    private void PlaySelectCombatActionSound(EntityPiece entity, Action.PhaseTypes phase)
    {
        audioSource.PlayOneShot(soundList[13], 2.5f);
    }

    private void PlayEnterBattleSound(EntityPiece entity)
    {
        audioSource.PlayOneShot(soundList[14], 1.2f);
    }

    private void PlayNextPlayerTurnSound(EntityPiece entity)
    {
        audioSource.PlayOneShot(soundList[15], 2.5f);
        Debug.Log("played next player sound");
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
