using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    //FMOD stuff
    public FMOD.Studio.EventInstance diceRollInstance;
    public FMOD.Studio.EventInstance overworldThemeInstance;
    public FMOD.Studio.EventInstance battleThemeInstance;

    public float musicVolume = 0.8f;
    public float SFXVolume = 0.8f;



    private EntityPiece stupidFuck;

    [Header("Listen on Event Channels")]
    public VoidEventChannelSO m_EnteredOverworldScene;
    public VoidEventChannelSO m_EnteredCombatScene;

    public VoidEventChannelSO m_PlayerMovedOnBoard;
    public VoidEventChannelSO m_PlayerUndidSomething;
    public PlayerEventChannelSO m_PassByStamp;
    public PlayerEventChannelSO m_DiceRollPrep;
    public IntEventChannelSO m_RollForMovement;

    public IntEventChannelSO m_PlayerScoreIncreased;
    public IntEventChannelSO m_PlayerScoreDecreased;
    public IntEventChannelSO m_ItemUsed;
    public IntEventChannelSO m_ItemBought;

    public PlayerEventChannelSO m_ShowCombatBanner;
    public PlayerEventChannelSO m_NextPlayerTurn;

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

    

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        overworldThemeInstance = FMODUnity.RuntimeManager.CreateInstance("event:/KatamariTheme");
        diceRollInstance = FMODUnity.RuntimeManager.CreateInstance("event:/RollDice");
        musicVolume = 0.8f;
        SFXVolume = 0.8f;
        overworldThemeInstance.setParameterByName("MusicVolume", musicVolume);
        overworldThemeInstance.start();
    }

    private void OnEnable()
    {
        //World Events
        m_EnteredCombatScene.OnEventRaised += StopOverworldMusic;
        m_EnteredOverworldScene.OnEventRaised += PlayOverworldMusic;
        m_PassByStamp.OnEventRaised += PlayStampSound;
        m_PlayerMovedOnBoard.OnEventRaised += PlayMoveSound;
        m_PlayerUndidSomething.OnEventRaised += PlayUndoSound;
        m_DiceRollPrep.OnEventRaised += PlayDiceRollSound;
        m_RollForMovement.OnEventRaised += PlayDiceHitSound;
        m_ItemUsed.OnEventRaised += PlayUseItemSound;
        m_PlayerScoreDecreased.OnEventRaised += PlayCurrencyDecreasedSound;
        m_PlayerScoreIncreased.OnEventRaised += PlayCurrencyIncreasedSound;
        m_ItemBought.OnEventRaised += PlayItemBoughtSound;
        m_NextPlayerTurn.OnEventRaised += PlayNextPlayerTurnSound;
        //m_ShowCombatBanner.OnEventRaised += PlayEnterBattleSound;

        //Combat Events
        m_EnteredCombatScene.OnEventRaised += PlayCombatMusic;
        m_EnteredOverworldScene.OnEventRaised += StopCombatMusic;
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
        m_EnteredCombatScene.OnEventRaised -= StopOverworldMusic;
        m_EnteredOverworldScene.OnEventRaised -= PlayOverworldMusic;
        m_PassByStamp.OnEventRaised -= PlayStampSound;
        m_PlayerMovedOnBoard.OnEventRaised -= PlayMoveSound;
        m_PlayerUndidSomething.OnEventRaised -= PlayUndoSound;
        m_DiceRollPrep.OnEventRaised -= PlayDiceRollSound;
        m_RollForMovement.OnEventRaised -= PlayDiceHitSound;
        m_ItemUsed.OnEventRaised -= PlayUseItemSound;
        m_PlayerScoreDecreased.OnEventRaised -= PlayCurrencyDecreasedSound;
        m_PlayerScoreIncreased.OnEventRaised -= PlayCurrencyIncreasedSound;
        m_NextPlayerTurn.OnEventRaised -= PlayNextPlayerTurnSound;
        //m_ShowCombatBanner.OnEventRaised -= PlayEnterBattleSound;

        //Combat Events
        m_EnteredCombatScene.OnEventRaised -= PlayCombatMusic;
        m_EnteredOverworldScene.OnEventRaised -= StopCombatMusic;
        m_ActionSelected.OnEventRaised -= PlaySelectCombatActionSound;
        m_UsedMeleeAttack.OnEventRaised -= PlayMeleeAttackSound;
        m_UsedMagicAttack.OnEventRaised -= PlayMagicAttackSound;
        m_UsedGunAttack.OnEventRaised -= PlayGunAttackSound;
        m_IneffectiveAttack.OnEventRaised -= PlayNotEffectiveHitSound;
        m_EffectiveAttack.OnEventRaised -= PlayEffectiveHitSound;
        m_SuperEffectiveAttack.OnEventRaised -= PlaySuperEffectiveHitSound;
        m_SomeoneDied.OnEventRaised -= PlayDeathSound;
    }

    private void PlayOverworldMusic()
    {
        overworldThemeInstance.setParameterByName("MusicVolume", musicVolume);
        overworldThemeInstance.start();
    }

    private void StopOverworldMusic()
    {
        overworldThemeInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        diceRollInstance.release();
    }

    private void PlayCombatMusic()
    {
        battleThemeInstance = FMODUnity.RuntimeManager.CreateInstance("event:/BattleTheme");
        battleThemeInstance.setParameterByName("MusicVolume", musicVolume);
        battleThemeInstance.start();
    }

    private void StopCombatMusic()
    {
        battleThemeInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        diceRollInstance.release();
    }

    private void PlayStampSound(EntityPiece entity)
    {
        AudioHelper.PlayOneShotWithParameters("event:/Stamp", this.transform.position, ("SoundVolume", SFXVolume));
    }

    private void PlayDiceRollSound(EntityPiece entity)
    {
        diceRollInstance = FMODUnity.RuntimeManager.CreateInstance("event:/RollDice");
        diceRollInstance.setParameterByName("SoundVolume", SFXVolume);
        diceRollInstance.start();
    }

    private void PlayDiceHitSound(int diceValue)
    {
        diceRollInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        diceRollInstance.release();
        AudioHelper.PlayOneShotWithParameters("event:/HitDice", this.transform.position, ("SoundVolume", SFXVolume));
    }

    private void PlayCurrencyDecreasedSound(int scoreDifference)
    {
        AudioHelper.PlayOneShotWithParameters("event:/LoseMoney", this.transform.position, ("SoundVolume", SFXVolume));
    }

    private void PlayCurrencyIncreasedSound(int scoreDifference)
    {
        AudioHelper.PlayOneShotWithParameters("event:/GainMoney", this.transform.position, ("SoundVolume", SFXVolume));
    }

    private void PlayItemBoughtSound(int itemValue)
    {
        //MAKE NEW SOUND
        AudioHelper.PlayOneShotWithParameters("event:/LoseMoney", this.transform.position, ("SoundVolume", SFXVolume));
    }

    private void PlayUseItemSound(int item)
    {
        //DIFFERENTIATE
        AudioHelper.PlayOneShotWithParameters("event:/UseItem(Food)", this.transform.position, ("SoundVolume", SFXVolume));
    }

    private void PlayEnterBattleSound(EntityPiece entity)
    {
        AudioHelper.PlayOneShotWithParameters("event:/EnterCombat", this.transform.position, ("SoundVolume", SFXVolume));
    }

    private void PlayNextPlayerTurnSound(EntityPiece entity)
    {
        AudioHelper.PlayOneShotWithParameters("event:/NextPlayer", this.transform.position, ("SoundVolume", SFXVolume));
    }

    private void PlayMoveSound()
    {
        AudioHelper.PlayOneShotWithParameters("event:/Move", this.transform.position, ("SoundVolume", SFXVolume));
    }

    private void PlayUndoSound()
    {
        AudioHelper.PlayOneShotWithParameters("event:/Undo", this.transform.position, ("SoundVolume", SFXVolume));
    }

        private void PlayMeleeAttackSound()
    {
        AudioHelper.PlayOneShotWithParameters("event:/MeleeWindup", this.transform.position, ("SoundVolume", SFXVolume));
    }

    private void PlayMagicAttackSound()
    {
        AudioHelper.PlayOneShotWithParameters("event:/MagicWindup", this.transform.position, ("SoundVolume", SFXVolume));
    }

    private void PlayGunAttackSound()
    {
        AudioHelper.PlayOneShotWithParameters("event:/GunWindup", this.transform.position, ("SoundVolume", SFXVolume));
    }

    private void PlayNotEffectiveHitSound()
    {
        AudioHelper.PlayOneShotWithParameters("event:/NotEffectiveHit", this.transform.position, ("SoundVolume", SFXVolume));
    }

    private void PlayEffectiveHitSound()
    {
        AudioHelper.PlayOneShotWithParameters("event:/EffectiveHit", this.transform.position, ("SoundVolume", SFXVolume));
    }

    private void PlaySuperEffectiveHitSound()
    {
        AudioHelper.PlayOneShotWithParameters("event:/SuperEffectiveHit", this.transform.position, ("SoundVolume", SFXVolume));
    }

    private void PlayDeathSound()
    {
        AudioHelper.PlayOneShotWithParameters("event:/Death", this.transform.position, ("SoundVolume", SFXVolume));
    }

    private void PlaySelectCombatActionSound(EntityPiece entity, Action.PhaseTypes phase)
    {
        AudioHelper.PlayOneShotWithParameters("event:/SelectCombatAction", this.transform.position, ("SoundVolume", SFXVolume));
    }

    
}
