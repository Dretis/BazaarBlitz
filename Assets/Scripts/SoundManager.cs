using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<AudioClip> soundList;

    private EntityPiece stupidFuck;

    [Header("Listen on Event Channels")]
    public PlayerEventChannelSO m_PassByStamp;


    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

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
    }

    private void OnEnable()
    {
        //World Events
        m_PassByStamp.OnEventRaised += PlayStampSound;
        
        // m_DiceRollPrep += PlayDiceRollSound;
        // m_RollForMovement += PlayDiceHitSound;
        // m_ItemUsed += PlayUseItemSound;
        // m_UpdatePlayerScore += PlayCurrencyChangeSound;
        // m_ShowCombatBanner += PlayEnterBattleSound;


        // //Combat Events
        // m_ActionSelected += PlaySelectCombatActionSound;
        // m_UsedMeleeAttack += PlayMeleeAttackSound;
        // m_UsedMagicAttack += PlayMagicAttackSound;
        // m_UsedGunAttack += PlayGunAttackSound;
        // m_ReceivedNotEffectiveAttack += PlayNotEffectiveHitSound;
        // m_ReceivedEffectiveAttack += PlayEffectiveHitSound;
        // m_ReceivedSuperEffectiveAttack += PlaySuperEffectiveHitSound;
        // m_EntityDied += PlayDeathSound;


    }

    private void OnDisable()
    {
        //World Events
        m_PassByStamp.OnEventRaised -= PlayStampSound;
        // m_DiceRollPrep -= PlayDiceRollSound;
        // m_RollForMovement -= PlayDiceHitSound;
        // m_ItemUsed -= PlayUseItemSound;
        // m_UpdatePlayerScore -= PlayCurrencyChangeSound;
        // m_ShowCombatBanner -= PlayEnterBattleSound;


        // //Combat Events
        // m_ActionSelected -= PlaySelectCombatActionSound;
        // m_UsedMeleeAttack -= PlayMeleeAttackSound;
        // m_UsedMagicAttack -= PlayMagicAttackSound;
        // m_UsedGunAttack -= PlayGunAttackSound;
        // m_ReceivedNotEffectiveAttack -= PlayNotEffectiveHitSound;
        // m_ReceivedEffectiveAttack -= PlayEffectiveHitSound;
        // m_ReceivedSuperEffectiveAttack -= PlaySuperEffectiveHitSound;
        // m_EntityDied -= PlayDeathSound;
    }

    private void PlayStampSound(EntityPiece entity)
    {
        audioSource.PlayOneShot(soundList[0], 2f);
    }

    private void PlayDiceRollSound(EntityPiece entity)
    {
        audioSource.PlayOneShot(soundList[1], 1.2f);
    }

        private void PlayDiceHitSound(int diceValue)
    {
        audioSource.PlayOneShot(soundList[2], 2f);
    }

        private void PlayCurrencyChangeSound(int scoreDifference)
    {
        if (scoreDifference > 0)
            audioSource.PlayOneShot(soundList[3], 3f);
        else
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

        private void PlayNotEffectiveHitSound(EntityPiece entity)
    {
        audioSource.PlayOneShot(soundList[9], 3f);
    }

        private void PlayEffectiveHitSound(EntityPiece entity)
    {
        audioSource.PlayOneShot(soundList[10], 1.2f);
    }

        private void PlaySuperEffectiveHitSound(EntityPiece entity)
    {
        audioSource.PlayOneShot(soundList[11], 1.5f);
    }

        private void PlayDeathSound(EntityPiece entity)
    {
        audioSource.PlayOneShot(soundList[12], 2f);
    }

        private void PlaySelectCombatActionSound(EntityPiece entity)
    {
        audioSource.PlayOneShot(soundList[13], 2f);
    }

        private void PlayEnterBattleSound(EntityPiece entity)
    {
        audioSource.PlayOneShot(soundList[14], 1.2f);
    }


    


    
}
