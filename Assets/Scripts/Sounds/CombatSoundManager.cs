using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatSoundManager : MonoBehaviour
{
    private EntityPiece stupidFuck;

    [Header("Listen on Event Channels")]
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

    
    private void OnEnable()
    {
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
        //Combat Events
        m_ActionSelected.OnEventRaised -= PlaySelectCombatActionSound;
        m_UsedMeleeAttack.OnEventRaised -= PlayMeleeAttackSound;
        m_UsedMagicAttack.OnEventRaised -= PlayMagicAttackSound;
        m_UsedGunAttack.OnEventRaised -= PlayGunAttackSound;
        m_IneffectiveAttack.OnEventRaised -= PlayNotEffectiveHitSound;
        m_EffectiveAttack.OnEventRaised -= PlayEffectiveHitSound;
        m_SuperEffectiveAttack.OnEventRaised -= PlaySuperEffectiveHitSound;
        m_SomeoneDied.OnEventRaised -= PlayDeathSound;
    }

    private void PlayMeleeAttackSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/MeleeWindup");
    }

    private void PlayMagicAttackSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/MagicWindup");
    }

    private void PlayGunAttackSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/GunWindup");
    }

    private void PlayNotEffectiveHitSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/NotEffectiveHit");
    }

    private void PlayEffectiveHitSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/EffectiveHit");
    }

    private void PlaySuperEffectiveHitSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/SuperEffectiveHit");
    }

    private void PlayDeathSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Death");
    }

    private void PlaySelectCombatActionSound(EntityPiece entity, Action.PhaseTypes phase)
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/SelectCombatAction");
    }

    
}
