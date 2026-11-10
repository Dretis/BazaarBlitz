using Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

// COMBAT LOOP
public partial class GameplayTest : MonoBehaviour
{
    public enum CombatPhase
    {
        None,
        EnterCombat,
        ActionSelect, // Idling
        ActionReveal,
        RollDie,

        ProcessCalculation,

        PlayOutCombat,
        DecideOutcome,
        SwapFighterPhase,

        Results, // Victory or Stalemate | 1v2 = HighFive/Backstab
        ExitCombat
    }

    // for reference for now, delete later?
    public enum FighterPhase
    {
        None,
        SelectingAction,
        SelectedAction,

        RollingDie,
        RolledDie,

        Hurt,
        Dead
    }

    public enum FighterSide
    {
        None,
        Left,
        Right
    }

    private void CombatGameLogic(GameState prevState, ref GameState newState, bool isCmdToProcess, ICommand cmd)
    {
        //var a = newState.CurrentCombatInstance;
        switch (newState.CurrentCombatInstance.CombatPhase)
        {
            case CombatPhase.EnterCombat:
                // Setup combat stuffs here

                newState.CurrentCombatInstance.CombatPhase = CombatPhase.ActionSelect;
                break;
            case CombatPhase.ActionSelect:
                // Allow all Fighters to pick an Action
                if (!isCmdToProcess) return;

                // Process the picked Action here
                if (cmd is CombatActionCommand combatActionCmd)
                {
                    var actionId = combatActionCmd.ActionId;
                    var entityId = combatActionCmd.EntityId;
                    HandleCombatActionSelected(prevState, ref newState, actionId, entityId);
                }

                foreach (var fighter in newState.CurrentCombatInstance.Fighters)
                {
                    if (fighter.FighterPhase is not FighterPhase.SelectedAction)
                        return;
                }
                //if all Fighters have picked
                newState.CurrentCombatInstance.CombatPhase = CombatPhase.ActionReveal;
                break;
            case CombatPhase.ActionReveal:
                // Passing phase, allow UI to know this is up and show selected actions
                newState.CurrentCombatInstance.CombatPhase = CombatPhase.RollDie;
                break;
            case CombatPhase.RollDie:
                // Allow all fighters to roll Die visually
                if (!isCmdToProcess) return;

                // Process that a Fighter confirmed they rolled their Die
                //if (cmd is CombatDieRolledCommand)
                //{
                //    HandleCombatDieRolled(prevState, ref newState);
                //}

                foreach (var fighter in newState.CurrentCombatInstance.Fighters)
                {
                    if (fighter.FighterPhase is not FighterPhase.RolledDie)
                        return;
                }

                // Once all fighters rolled, calculate everything PlayOutCombat
                newState.CurrentCombatInstance.CombatPhase = CombatPhase.ProcessCalculation;
                break;
            case CombatPhase.ProcessCalculation:
                // Calculate everything based off the Fighters rolls
                // Should figure out the Result (dead or not) in this phase too

                // Then let the visual sequence play out
                newState.CurrentCombatInstance.CombatPhase = CombatPhase.PlayOutCombat;
                break;
            case CombatPhase.PlayOutCombat:
                // Play Animations and cutscenes based on PickedActionID here
                // then go to DecideOutcome after it finishes
                newState.CurrentCombatInstance.CombatPhase = CombatPhase.DecideOutcome;
                break;
            case CombatPhase.DecideOutcome:
                // Decide the Defender die or not (based off info from ProcessCalculation)
                // If dead, go to results as Victory
                newState.CurrentCombatInstance.CombatPhase = CombatPhase.Results;
                // else go to SwapFighterPhase
                newState.CurrentCombatInstance.CombatPhase = CombatPhase.SwapFighterPhase;
                break;
            case CombatPhase.SwapFighterPhase:
                // Attacking <=> Defending
                // fs.ResetSelectedAction();
                // HandleSwapActionPhase(INSERT_FIGHTERSTATE_HERE);
                newState.CurrentCombatInstance.CombatPhase = CombatPhase.ActionSelect;
                break;
            case CombatPhase.Results:
                // Victory or Stalemate
                // if Victory,
                //  PvE - Player Win, Players obtains 2 Items and EXP based on the Enemy defeated
                //  PvE - Enemy win, Enemy steals 1/2 Money from Player and +1 to all Die Faces
                //  PvP - Winner steals 1/2 Money from Loser and EXP based on Level difference

                //if stalemate, give Fighting Status to all Entity Fighters
                newState.CurrentCombatInstance.CombatPhase = CombatPhase.ExitCombat;
                break;
            case CombatPhase.ExitCombat:
                newState.CurrentCombatInstance.CombatPhase = CombatPhase.None;
                newState.GamePhase = GamePhase.EndTurn;
                break;
            default:
                Debug.Log($"Do nothing in this combat Phase! | {newState.GamePhase}");
                break;
        }
    }

    private void HandleCombatActionSelected(GameState prevState, ref GameState newState, int actionId, int entityId)
    {
        // this is not what this function should be, rewrite it
        ref var combatInstance = ref newState.CurrentCombatInstance;
        //ref FighterState fs = ref new();


        for (int i = 0; i < combatInstance.Fighters.Count; i++)
        {
            if (combatInstance.Fighters[i].EntityId == entityId)
            {
                var fs = combatInstance.Fighters[i];
                //var action = fs.AtkAction1;
                fs.SelectedActionId = actionId;

                fs.FighterPhase = FighterPhase.SelectedAction;
            }
        }


        throw new NotImplementedException();
    }

    private Action.ActionPhase HandleSwapActionPhase(FighterState fs)
    {
        // Attacking <=> Defending
        if(fs.ActionPhase is Action.ActionPhase.Attack)
        {
            return Action.ActionPhase.Defend;
        }
        else if(fs.ActionPhase is Action.ActionPhase.Defend)
        {
            return Action.ActionPhase.Attack;
        }

        Debug.Log("???");
        return Action.ActionPhase.Both;
    }
}
