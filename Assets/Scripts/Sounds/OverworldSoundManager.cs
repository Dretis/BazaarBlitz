using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldSoundManager : MonoBehaviour
{
    //FMOD stuff
    private FMOD.Studio.EventInstance diceRollInstance;



    private EntityPiece stupidFuck;

    [Header("Listen on Event Channels")]
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

    
    private void Start()
    {
        diceRollInstance = FMODUnity.RuntimeManager.CreateInstance("event:/RollDice");
    }

    private void OnEnable()
    {
        //World Events
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
    }

    private void OnDisable()
    {
        //World Events
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
    }

    private void PlayStampSound(EntityPiece entity)
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Stamp");
    }

    private void PlayDiceRollSound(EntityPiece entity)
    {
        diceRollInstance.start();
    }

    private void PlayDiceHitSound(int diceValue)
    {
        diceRollInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        diceRollInstance.release();
        FMODUnity.RuntimeManager.PlayOneShot("event:/HitDice");
    }

    private void PlayCurrencyDecreasedSound(int scoreDifference)
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/LoseMoney");
    }

    private void PlayCurrencyIncreasedSound(int scoreDifference)
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/GainMoney");
    }

    private void PlayItemBoughtSound(int itemValue)
    {
        //MAKE NEW SOUND
        FMODUnity.RuntimeManager.PlayOneShot("event:/LoseMoney");
    }

    private void PlayUseItemSound(int item)
    {
        //DIFFERENTIATE
        FMODUnity.RuntimeManager.PlayOneShot("event:/UseItem(Food)");
    }

    private void PlayEnterBattleSound(EntityPiece entity)
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/EnterCombat");
    }

    private void PlayNextPlayerTurnSound(EntityPiece entity)
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/NextPlayer");
    }

    private void PlayMoveSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Move");
    }

    private void PlayUndoSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Undo");
    }
    
}
