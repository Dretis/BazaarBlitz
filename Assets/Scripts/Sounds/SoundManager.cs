using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    //FMOD stuff
    public StudioEventEmitter overworldBGM;
    public FMOD.Studio.EventInstance diceRollInstance;
    public FMOD.Studio.EventInstance overworldThemeInstance;
    public FMOD.Studio.EventInstance battleThemeInstance;

    [RangeAttribute(0, 1)]
    public float musicVolume;

    [RangeAttribute(0, 1)]
    public float SFXVolume;

    private EntityPiece stupidFuck;

    [Header("Listen on Event Channels")]
    public VoidEventChannelSO m_ReturnToMainMenu;
    public VoidEventChannelSO m_EnteredOverworldScene;
    public VoidEventChannelSO m_EnteredCombatScene;
    [Space]
    public VoidEventChannelSO m_PlayerMovedOnBoard;
    public VoidEventChannelSO m_PlayerUndidSomething;
    public PlayerEventChannelSO m_PassByStamp;
    public PlayerEventChannelSO m_DiceRollPrep;
    public IntEventChannelSO m_RollForMovement;
    public PlayerEventChannelSO m_DiceRollUndo;

    [Space]
    public VoidEventChannelSO m_EnableFreeview;
    public VoidEventChannelSO m_DisableFreeview;

    [Space]
    public PlayerEventChannelSO m_OpenInventory;
    public VoidEventChannelSO m_ExitInventory;
    public ItemEventChannelSO m_ItemSelected;

    [Space]
    public PlayerEventChannelSO m_TryBuildStore;
    public VoidEventChannelSO m_CancelBuildStore;

    [Space]
    public Vector2EventChannelSO m_TryExamineTile;
    public IntEventChannelSO m_PlayerScoreIncreased;
    public IntEventChannelSO m_PlayerScoreDecreased;
    public IntEventChannelSO m_ItemUsed;
    public IntEventChannelSO m_ItemBought;

    [Space]
    public NodeEventChannelSO m_LandOnStorefront;
    public VoidEventChannelSO m_ExitStorefront;
    public ItemEventChannelSO m_HoverItemInStorefront;
    public IntEventChannelSO m_TryBuyItemAt;

    [Space]
    public PlayerEventChannelSO m_EnterLevelUp;
    public VoidEventChannelSO m_AugmentedDieFaceValue;
    public VoidEventChannelSO m_FailAugmentDieFaceValue;

    [Space]
    public PlayerEventChannelSO m_ShowCombatBanner;
    public PlayerEventChannelSO m_NextPlayerTurn;

    [Space]
    public VoidEventChannelSO m_UsedMeleeAttack;
    public VoidEventChannelSO m_UsedMagicAttack;
    public VoidEventChannelSO m_UsedGunAttack;

    [Space]
    public VoidEventChannelSO m_IneffectiveAttack;
    public VoidEventChannelSO m_EffectiveAttack;
    public VoidEventChannelSO m_SuperEffectiveAttack;
    public VoidEventChannelSO m_SomeoneDied;

    public VoidEventChannelSO m_AttackImpact;

    [Space]
    public EntityItemEventChannelSO m_EntityDied;
    public EntityActionPhaseEventChannelSO m_ActionSelected;
    public PlayerFloatActionTypeEventChannelSO m_StoreDiceRolled;
    public DamageEventChannelSO m_DamageTaken;

    private bool startedBattleMusic = false;


    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        overworldThemeInstance = FMODUnity.RuntimeManager.CreateInstance("event:/BGM_Counterflow");
        diceRollInstance = FMODUnity.RuntimeManager.CreateInstance("event:/RollDice");
        battleThemeInstance = FMODUnity.RuntimeManager.CreateInstance("event:/BattleTheme");
        //musicVolume = 0.8f;
        //SFXVolume = 0.8f;
        //overworldThemeInstance.setParameterByName("MusicVolume", musicVolume);
        //overworldThemeInstance.start();

        //Debug.Log(overworldBGM.EventReference);
        //Debug.Log(overworldBGM.EventInstance);
        overworldBGM.Play();
    }

    private void OnEnable()
    {
        //World Events
        m_ReturnToMainMenu.OnEventRaised += OnReturnToMainMenu;
        m_EnteredCombatScene.OnEventRaised += StopOverworldMusic;
        m_EnteredOverworldScene.OnEventRaised += PlayOverworldMusic;
        m_PassByStamp.OnEventRaised += PlayStampSound;
        m_PlayerMovedOnBoard.OnEventRaised += PlayMoveSound;
        m_PlayerUndidSomething.OnEventRaised += PlayUndoSound;
        m_DiceRollPrep.OnEventRaised += PlayDiceRollSound;
        m_DiceRollUndo.OnEventRaised += StopDiceRollSound;
        m_RollForMovement.OnEventRaised += PlayDiceHitSound;
        m_ItemUsed.OnEventRaised += PlayUseItemSound;
        m_PlayerScoreDecreased.OnEventRaised += PlayCurrencyDecreasedSound;
        m_PlayerScoreIncreased.OnEventRaised += PlayCurrencyIncreasedSound;
        m_ItemBought.OnEventRaised += PlayItemBoughtSound;
        m_NextPlayerTurn.OnEventRaised += PlayNextPlayerTurnSound;
        //m_ShowCombatBanner.OnEventRaised += PlayEnterBattleSound;
        m_EnableFreeview.OnEventRaised += PlayMoveSound;
        m_DisableFreeview.OnEventRaised += PlayUndoSound;

        m_OpenInventory.OnEventRaised += OnOpenInventory;
        m_ExitInventory.OnEventRaised += OnExitInventory;
        m_ItemSelected.OnEventRaised += OnItemSelected;

        m_TryBuildStore.OnEventRaised += OnTryBuildStore;
        m_CancelBuildStore.OnEventRaised += OnCancelBuildStore;

        m_TryExamineTile.OnEventRaised += PlayMoveSoundWithVector2;

        m_LandOnStorefront.OnEventRaised += OnLandOnStorefront;
        m_ExitStorefront.OnEventRaised += OnExitStorefront;

        m_HoverItemInStorefront.OnEventRaised += OnHoverItemInStorefront;
        m_TryBuyItemAt.OnEventRaised += OnTryBuyItemAt;

        m_EnterLevelUp.OnEventRaised += OnEnterLevelUp;
        m_AugmentedDieFaceValue.OnEventRaised += PlayStampSound;
        m_FailAugmentDieFaceValue.OnEventRaised += PlayNotEffectiveHitSound;

        //Combat Events
        m_EnteredCombatScene.OnEventRaised += PlayCombatMusic;
        m_EnteredOverworldScene.OnEventRaised += StopCombatMusic;

        m_ActionSelected.OnEventRaised += PlaySelectCombatActionSound;
        m_StoreDiceRolled.OnEventRaised += OnStoreDiceRolled;

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
        m_ReturnToMainMenu.OnEventRaised -= OnReturnToMainMenu;
        m_EnteredCombatScene.OnEventRaised -= StopOverworldMusic;
        m_EnteredOverworldScene.OnEventRaised -= PlayOverworldMusic;
        m_PassByStamp.OnEventRaised -= PlayStampSound;
        m_PlayerMovedOnBoard.OnEventRaised -= PlayMoveSound;
        m_PlayerUndidSomething.OnEventRaised -= PlayUndoSound;
        m_DiceRollPrep.OnEventRaised -= PlayDiceRollSound;
        m_DiceRollUndo.OnEventRaised -= StopDiceRollSound;
        m_RollForMovement.OnEventRaised -= PlayDiceHitSound;
        m_ItemUsed.OnEventRaised -= PlayUseItemSound;
        m_PlayerScoreDecreased.OnEventRaised -= PlayCurrencyDecreasedSound;
        m_PlayerScoreIncreased.OnEventRaised -= PlayCurrencyIncreasedSound;
        m_NextPlayerTurn.OnEventRaised -= PlayNextPlayerTurnSound;
        //m_ShowCombatBanner.OnEventRaised -= PlayEnterBattleSound;
        m_EnableFreeview.OnEventRaised -= PlayMoveSound;
        m_DisableFreeview.OnEventRaised -= PlayUndoSound;

        m_OpenInventory.OnEventRaised -= OnOpenInventory;
        m_ExitInventory.OnEventRaised -= OnExitInventory;
        m_ItemSelected.OnEventRaised -= OnItemSelected;

        m_TryBuildStore.OnEventRaised -= OnTryBuildStore;
        m_CancelBuildStore.OnEventRaised -= OnCancelBuildStore;

        m_TryExamineTile.OnEventRaised -= PlayMoveSoundWithVector2;

        m_LandOnStorefront.OnEventRaised -= OnLandOnStorefront;
        m_ExitStorefront.OnEventRaised -= OnExitStorefront;

        m_HoverItemInStorefront.OnEventRaised -= OnHoverItemInStorefront;
        m_TryBuyItemAt.OnEventRaised -= OnTryBuyItemAt;

        m_EnterLevelUp.OnEventRaised -= OnEnterLevelUp;
        m_AugmentedDieFaceValue.OnEventRaised -= PlayStampSound;
        m_FailAugmentDieFaceValue.OnEventRaised -= PlayNotEffectiveHitSound;

        //Combat Events
        m_EnteredCombatScene.OnEventRaised -= PlayCombatMusic;
        m_EnteredOverworldScene.OnEventRaised -= StopCombatMusic;

        m_ActionSelected.OnEventRaised -= PlaySelectCombatActionSound;
        m_StoreDiceRolled.OnEventRaised -= OnStoreDiceRolled;

        m_UsedMeleeAttack.OnEventRaised -= PlayMeleeAttackSound;
        m_UsedMagicAttack.OnEventRaised -= PlayMagicAttackSound;
        m_UsedGunAttack.OnEventRaised -= PlayGunAttackSound;
        m_IneffectiveAttack.OnEventRaised -= PlayNotEffectiveHitSound;
        m_EffectiveAttack.OnEventRaised -= PlayEffectiveHitSound;
        m_SuperEffectiveAttack.OnEventRaised -= PlaySuperEffectiveHitSound;
        m_SomeoneDied.OnEventRaised -= PlayDeathSound;
    }
    private void OnReturnToMainMenu()
    {
        Destroy(this.gameObject);
    }
    private void PlayOverworldMusic()
    {
        //overworldThemeInstance.setPaused(false);
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameState", 0);
    }

    private void StopOverworldMusic()
    {
        //overworldThemeInstance.setPaused(true);
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameState", 1);
    }

    private void PlayCombatMusic()
    {
        /*
        if (startedBattleMusic)
        {
            battleThemeInstance.setPaused(false);
        }
        else
        {
            battleThemeInstance.setParameterByName("MusicVolume", musicVolume);
            battleThemeInstance.start();
            startedBattleMusic = true;
        }
        */
    }

    private void StopCombatMusic()
    {
        //battleThemeInstance.setPaused(true);
    }

    private void PlayStampSound()
    {
        AudioHelper.PlayOneShotWithParameters("event:/Stamp", this.transform.position, ("SoundVolume", SFXVolume));
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

    private void StopDiceRollSound()
    {
        Debug.Log("stopped dice roll sound");
        diceRollInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        diceRollInstance.release();
    }

    private void StopDiceRollSound(EntityPiece entity)
    {
        Debug.Log("stopped dice roll sound");
        diceRollInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        diceRollInstance.release();
    }

    private void PlayDiceHitSound(int diceValue)
    {
        diceRollInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        diceRollInstance.release();
        
        AudioHelper.PlayOneShotWithParameters("event:/HitDice", this.transform.position, ("SoundVolume", SFXVolume), ("RolledNumber", Mathf.Min(diceValue,15)));
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

    public void PlayEnterStoreSound()
    {
        AudioHelper.PlayOneShotWithParameters("event:/EnterStore", this.transform.position, ("SoundVolume", SFXVolume));
    }

    public void PlayEnterBattleSound()
    {
        AudioHelper.PlayOneShotWithParameters("event:/EnterCombat", this.transform.position, ("SoundVolume", SFXVolume));
    }

    private void PlayEnterBattleSound(EntityPiece entity)
    {
        AudioHelper.PlayOneShotWithParameters("event:/EnterCombat", this.transform.position, ("SoundVolume", SFXVolume));
    }

    public void PlayNextPlayerTurnSound()
    {
        AudioHelper.PlayOneShotWithParameters("event:/NextPlayer", this.transform.position, ("SoundVolume", SFXVolume));
    }

    private void PlayNextPlayerTurnSound(EntityPiece entity)
    {
        AudioHelper.PlayOneShotWithParameters("event:/NextPlayer", this.transform.position, ("SoundVolume", SFXVolume));
    }

    private void PlayMoveSound()
    {
        AudioHelper.PlayOneShotWithParameters("event:/Move", this.transform.position, ("SoundVolume", SFXVolume));
    }

    private void PlayMoveSoundWithDude()
    {
        AudioHelper.PlayOneShotWithParameters("event:/Move", this.transform.position, ("SoundVolume", SFXVolume));
    }

    private void PlayMoveSoundWithVector2(Vector2 vector2)
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

    private void PlayLevelUpSound()
    {
        AudioHelper.PlayOneShotWithParameters("event:/LevelUp", this.transform.position, ("SoundVolume", SFXVolume));
    }

    private void OnOpenInventory(EntityPiece entity)
    {
        //PlayMoveSound();
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameState", 2);
    }

    private void OnExitInventory()
    {
        PlayUndoSound();
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameState", 0);
    }

    private void OnItemSelected(ItemStats item)
    {
        PlayMoveSound();
    }

    private void OnTryBuildStore(EntityPiece entity)
    {
        PlayMoveSound();
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameState", 2);
    }

    private void OnCancelBuildStore()
    {
        PlayUndoSound();
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameState", 0);
    }

    private void OnLandOnStorefront(MapNode node)
    {
        PlayEnterStoreSound();
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameState", 2);
    }

    private void OnExitStorefront()
    {
        //PlayEnterStoreSound();
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameState", 0);
    }

    private void OnHoverItemInStorefront(ItemStats item)
    {
        if (item == null)
            PlayUndoSound();
        else
            PlayMoveSound();
    }

    private void OnTryBuyItemAt(int index)
    {
        PlayUseItemSound(index);
    }

    private void OnEnterLevelUp(EntityPiece entity)
    {
        StopDiceRollSound(entity);
        //PlayCurrencyIncreasedSound(0);
        PlayLevelUpSound();
    }
    /*
    private void OnCombatDiceRolled(EntityPiece entity)
    {
        // dont play sound if enemy rolls, prevents 2x volume
        if (entity.isEnemy) return;

        PlayDiceHitSound(0);
    }
    */
    private void OnStoreDiceRolled(EntityPiece entity, float rolledNumber, Action.WeaponTypes type)
    {
        if (entity.isEnemy) return;

        PlayDiceHitSound((int)rolledNumber);
    }
}
