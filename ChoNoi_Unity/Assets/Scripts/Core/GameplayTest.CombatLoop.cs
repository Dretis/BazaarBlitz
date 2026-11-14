using Core;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using static Core.CombatInstance;
using static Core.FighterState;
using Random = System.Random;

// COMBAT LOOP
public partial class GameplayTest : MonoBehaviour
{
    public enum CombatPhase
    {
        None,
        EnterCombat,
        ActionSelection, // Idling
        ActionReveal,
        RollDie,

        ProcessCalculation,

        PlayOutSequence,
        DecideOutcome,
        SwapPhases,

        Results, // Victory or Stalemate | 1v2 = HighFive/Backstab
        ExitCombat
    }

    // for reference for now, delete later?
    public enum FighterPhase
    {
        None,
        ActionSelect,
        ActionSelected,

        DieRolling,
        DieRolled,

        Hurt,
        Dead
    }

    public enum FighterSide
    {
        //None,
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
                //newState.CurrentCombatInstance.NumberOfPlayers = 0;
                //newState.CurrentCombatInstance.NumberOfEnemies = 0;
                //newState.CurrentCombatInstance.PhaseSwapCount = 0;
                //foreach (var fighter in newState.CurrentCombatInstance.Fighters)
                //{
                //    if (fighter.EntityId >= 0) newState.CurrentCombatInstance.NumberOfPlayers++;
                //    else newState.CurrentCombatInstance.NumberOfEnemies++;
                //}

                //foreach (var team in newState.CurrentCombatInstance.Teams)
                //{
                //    foreach(var fighter in team.Fighters)
                //    {
                //        if (fighter.EntityId >= 0) newState.CurrentCombatInstance.NumberOfPlayers++;
                //        else newState.CurrentCombatInstance.NumberOfEnemies++;
                //    }
                //}

                //newState.CurrentCombatInstance.CombatSequenceDatas = new();
                newState.CurrentCombatInstance.CombatPhase = CombatPhase.ActionSelection;
                break;
            case CombatPhase.ActionSelection:
                // Allow all Fighters to pick an Action
                if (!isCmdToProcess) return;

                // Process the picked Action here
                if (cmd is CombatActionCommand combatActionCmd)
                {
                    var actionId = combatActionCmd.ActionId;
                    //var entityId = combatActionCmd.EntityId;
                    var ps = newState.FindEntityById(combatActionCmd.EntityId);
                    HandleCombatActionSelected(prevState, ref newState, actionId, ps);
                }

                //var numberOfPlayersActed = 0;
                foreach (var team in newState.CurrentCombatInstance.Teams)
                {
                    foreach (var fighter in team.Fighters)
                    {
                        if (fighter.EntityId >= 0 && fighter.FighterPhase is FighterPhase.ActionSelected)
                        {
                            newState.CurrentCombatInstance.NumberOfPlayersActed++;
                        }
                    }
                }

                //if all Player Fighters have picked
                if (newState.CurrentCombatInstance.NumberOfPlayersActed == newState.CurrentCombatInstance.NumberOfPlayers)
                {
                    // now have any Enemies pick their Action
                    foreach (var team in newState.CurrentCombatInstance.Teams)
                    {
                        foreach (var fighter in team.Fighters)
                        {
                            if (fighter.EntityId < 0)
                            {
                                var es = newState.FindEntityById(fighter.EntityId);
                                // Pick action based on their behavior
                                var actionId = es.EnemyData.DecideActionByBehavior(_rng);

                                HandleCombatActionSelected(prevState, ref newState, actionId, es);
                            }
                        }
                    }

                    // Everyone has picked, now go show what they picked
                    newState.CurrentCombatInstance.CombatPhase = CombatPhase.ActionReveal;
                }
                /*
                var numberOfPlayersActed = 0;
                //var numberOfEnemiesActed = 0;
                foreach (var fighter in newState.CurrentCombatInstance.Fighters)
                {
                    if(fighter.EntityId >= 0 && fighter.FighterPhase is FighterPhase.ActionSelected)
                    {
                        numberOfPlayersActed++;
                    }
                    //if (fighter.FighterPhase is not FighterPhase.ActionSelected)
                    //    return;
                }

                //if all Player Fighters have picked
                if (numberOfPlayersActed == newState.CurrentCombatInstance.NumberOfPlayers)
                {
                    // now have any Enemies pick their Action
                    foreach (var fighter in newState.CurrentCombatInstance.Fighters)
                    {
                        if (fighter.EntityId < 0)
                        {
                            var es = newState.FindEntityById(fighter.EntityId);
                            // Pick action based on their behavior
                            var actionId = es.EnemyData.DecideActionByBehavior(_rng);

                            HandleCombatActionSelected(prevState, ref newState, actionId, es);
                        }
                    }

                    // Everyone has picked, now go show what they picked
                    newState.CurrentCombatInstance.CombatPhase = CombatPhase.ActionReveal;
                }
                */
                //newState.CurrentCombatInstance.CombatPhase = CombatPhase.ActionReveal;
                break;
            case CombatPhase.ActionReveal:
                // Passing phase, allow UI to know this is up and show selected actions
                //for(int i = 0; i < newState.CurrentCombatInstance.Fighters.Length; i++)
                //{
                //    newState.CurrentCombatInstance.Fighters[i].FighterPhase = FighterPhase.DieRolling;
                //}
                foreach (var team in newState.CurrentCombatInstance.Teams)
                {
                    for (int i = 0; i < team.Fighters.Length; i++)
                    {
                        team.Fighters[i].FighterPhase = FighterPhase.DieRolling;
                    }
                }

                newState.CurrentCombatInstance.NumberOfPlayersActed = 0;
                newState.CurrentCombatInstance.CombatPhase = CombatPhase.RollDie;
                break;
            case CombatPhase.RollDie:
                // Allow all fighters to roll Die visually
                if (!isCmdToProcess) return;

                // Process that a Fighter confirmed they rolled their Die
                if (cmd is CombatRollCommand combatRollCmd)
                {
                    var ps = newState.FindEntityById(combatRollCmd.EntityId);
                    HandleCombatDieRolled(prevState, ref newState, ps);
                }

                foreach (var team in newState.CurrentCombatInstance.Teams)
                {
                    foreach (var fighter in team.Fighters)
                    {
                        if (fighter.EntityId >= 0 && fighter.FighterPhase is FighterPhase.DieRolled)
                        {
                            newState.CurrentCombatInstance.NumberOfPlayersActed++;
                        }
                    }
                }

                // Once all fighters rolled
                if (newState.CurrentCombatInstance.NumberOfPlayersActed == newState.CurrentCombatInstance.NumberOfPlayers)
                {
                    // now have any Enemies pick their Action
                    foreach (var team in newState.CurrentCombatInstance.Teams)
                    {
                        foreach (var fighter in team.Fighters)
                        {
                            if (fighter.EntityId < 0)
                            {
                                var es = newState.FindEntityById(fighter.EntityId);
                                HandleCombatDieRolled(prevState, ref newState, es);
                            }
                        }
                    }
                    newState.CurrentCombatInstance.NumberOfPlayersActed = 0;
                    newState.CurrentCombatInstance.CombatPhase = CombatPhase.ProcessCalculation;
                }
                //if (numberOfPlayersRolled == newState.CurrentCombatInstance.NumberOfPlayers)
                //{
                //    // now have all Enemies roll
                //    foreach (var fighter in newState.CurrentCombatInstance.Fighters)
                //    {
                //        if (fighter.EntityId < 0)
                //        {
                //            var es = newState.FindEntityById(fighter.EntityId);
                //            HandleCombatDieRolled(prevState, ref newState, es);
                //        }
                //    }
                //    // Once all fighters rolled, calculate everything next
                //    newState.CurrentCombatInstance.CombatPhase = CombatPhase.ProcessCalculation;
                //}
                break;
            case CombatPhase.ProcessCalculation:
                // Calculate everything based off the Fighters rolls
                // Should figure out the Result (dead or not) in this phase too
                HandleCombatCalculation(prevState, ref newState);
                // Then let the visual sequence play out
                newState.CurrentCombatInstance.CombatPhase = CombatPhase.PlayOutSequence;
                break;
            case CombatPhase.PlayOutSequence:
                // Play Animations and cutscenes based on PickedActionIDs here
                // Should have a whole wait time until then
                // then go to DecideOutcome after it finishes
                newState.CurrentCombatInstance.CombatPhase = CombatPhase.DecideOutcome;
                break;
            case CombatPhase.DecideOutcome:
                // Decide the Defender die or not (based off info from ProcessCalculation)
                // If dead, go to results as Victory
                if (newState.CurrentCombatInstance.CombatSequenceDatas[0].IsDamageLethal)
                {
                    newState.CurrentCombatInstance.CombatPhase = CombatPhase.Results;
                }

                if(newState.CurrentCombatInstance.PhaseSwapCount > 0)
                {
                    newState.CurrentCombatInstance.CombatPhase = CombatPhase.Results;
                }
                else newState.CurrentCombatInstance.CombatPhase = CombatPhase.SwapPhases;
                break;
            case CombatPhase.SwapPhases:
                // Attacking <=> Defending
                // HandleSwapActionPhase(INSERT_FIGHTERSTATE_HERE);

                //for (int i = 0; i < newState.CurrentCombatInstance.Fighters.Length; i++)
                //{
                //    newState.CurrentCombatInstance.Fighters[i].ResetSelectedAction();
                //    newState.CurrentCombatInstance.Fighters[i].SwapActionPhase();
                //    newState.CurrentCombatInstance.Fighters[i].FighterPhase = FighterPhase.ActionSelect;
                //}

                for (int i = 0; i < newState.CurrentCombatInstance.Teams.Length; i++)
                {
                    newState.CurrentCombatInstance.Teams[i].SwapActionPhase();

                    for (int j = 0; j < newState.CurrentCombatInstance.Teams[i].Fighters.Length; j++)
                    {
                        newState.CurrentCombatInstance.Teams[i].Fighters[j].ResetSelectedAction();
                        newState.CurrentCombatInstance.Teams[i].Fighters[j].FighterPhase = FighterPhase.ActionSelect;
                    }
                }

                newState.CurrentCombatInstance.PhaseSwapCount++;
                newState.CurrentCombatInstance.CombatPhase = CombatPhase.ActionSelection;
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
                //newState.CurrentCombatInstance.CombatPhase = CombatPhase.None;
                newState.CurrentCombatInstance.ResetCombatInstance();

                newState.GamePhase = GamePhase.EndTurn;
                break;
            default:
                Debug.Log($"Do nothing in this combat Phase! | {newState.GamePhase}");
                break;
        }
    }

    private ref FighterState FindFighter(GameState prevState, ref GameState newState, PlayerState ps)
    {
        foreach (var team in newState.CurrentCombatInstance.Teams)
        {
            for (int i = 0; i < team.Fighters.Length; i++)
            {
                if (team.Fighters[i].EntityId == ps.Id)
                {
                    return ref team.Fighters[i];
                }
            }
        }
        // should never return this
        return ref newState.CurrentCombatInstance.Teams[0].Fighters[0];
    }

    private int FindTeamSide(GameState prevState, ref GameState newState, PlayerState ps)
    {
        int side = 0;// = FighterSide.Left;

        foreach (var team in newState.CurrentCombatInstance.Teams)
        {
            for (int i = 0; i < team.Fighters.Length; i++)
            {
                if (team.Fighters[i].EntityId == ps.Id)
                {
                    side = (int)team.FighterSide;
                    break;
                }
            }
        }

        return side;
    }

    private void HandleCombatActionSelected(GameState prevState, ref GameState newState, int actionId, PlayerState ps)
    {
        // this is not what this function should be, rewrite it
        ref var combatInstance = ref newState.CurrentCombatInstance;

        ref FighterState fs = ref FindFighter(prevState, ref newState, ps);
        var side = FindTeamSide(prevState, ref newState, ps);

        Action selectedAction = ps.CosmeticData.AttackActions[0];
        switch (newState.CurrentCombatInstance.Teams[side].ActionPhase)
        {
            case Action.ActionPhase.Attack:
                selectedAction = ps.CosmeticData.AttackActions[actionId];
                break;
            case Action.ActionPhase.Defend:
                selectedAction = ps.CosmeticData.DefendActions[actionId];
                break;
        }

        // Selected Action Info
        Debug.Log($"<color=#{ps.EntityData.MainColor.ToHexString()}>{ps.PlayerName}</color> selected Action #{actionId} " +
            $"- {selectedAction.actionName} [{selectedAction.type}] [Weapon ID: {selectedAction.weaponID}]");
        fs.SelectedActionId = actionId;
        fs.WeaponActionId = selectedAction.weaponID;
        fs.DieType = selectedAction.type;

        // Roll for the Die face here too?
        //RollCombatDie(ref fs, ps);

        fs.FighterPhase = FighterPhase.ActionSelected;

        //throw new NotImplementedException();
    }

    private void RollCombatDie(Action.ActionPhase phase, ref FighterState fs, PlayerState ps)
    {
        var ed = ps.EntityData;
        var addRollMod = ed.CurrentStatsModifier.rollModifier;
        var timesToRoll = 1 + addRollMod;

        fs.CombatDieRolls = new CombatDieRoll[timesToRoll];

        for (int i = 0; i < timesToRoll; i++)
        {
            var faceIndex = _rng.Next(0, 6);
            fs.CombatDieRolls[i].DieFaceNumber = faceIndex;
            fs.CombatDieRolls[i].BaseDieRoll = ps.EntityData.GetDie(fs.DieType)[faceIndex];

            if (phase is Action.ActionPhase.Attack)
            {
                fs.CombatDieRolls[i].FinalDieRoll = ApplyAttackDieMods(i, ref fs, ps);
                Debug.Log($"<color=orange>ATK ROLL ({i + 1} of {timesToRoll})</color> | " +
                    $"<color=#{ps.EntityData.MainColor.ToHexString()}>{ps.PlayerName}</color> rolled {fs.DieType} " +
                    $"Die Face [{fs.CombatDieRolls[i].DieFaceNumber}] " +
                    $"| Base Roll = {fs.CombatDieRolls[i].BaseDieRoll} | Final Roll = {fs.CombatDieRolls[i].FinalDieRoll}");
            }
            else
            {
                fs.CombatDieRolls[i].FinalDieRoll = ApplyDefendDieMods(i, ref fs, ps);
                Debug.Log($"<color=cyan>DEF ROLL ({i + 1} of {timesToRoll})</color> " +
                    $"| <color=#{ps.EntityData.MainColor.ToHexString()}>{ps.PlayerName}</color> rolled {fs.DieType} " +
                    $"Die Face [{fs.CombatDieRolls[i].DieFaceNumber}] " +
                    $"| Base Roll = {fs.CombatDieRolls[i].BaseDieRoll} | Final Roll = {fs.CombatDieRolls[i].FinalDieRoll}");
            }
        }

        //for (int i = 0; i < timesToRoll; i++)
        //{
        //    var faceIndex = _rng.Next(0, 6);
        //    fs.DieFaceNumber[i] = faceIndex;
        //    fs.BaseDieRoll[i] = ps.EntityData.GetDie(fs.DieType)[faceIndex];

        //    if (fs.ActionPhase is Action.ActionPhase.Attack)
        //        fs.FinalDieRoll[i] = ApplyAttackDieMods(i, ref fs, ps);
        //    else
        //        fs.FinalDieRoll[i] = ApplyDefendDieMods(i, ref fs, ps);
        //}

        //fs.DieFaceNumber = _rng.Next(0, 6);
        //fs.BaseDieRoll = ps.EntityData.GetDie(fs.DieType)[fs.DieFaceNumber];

        //if(fs.ActionPhase is Action.ActionPhase.Attack)
        //    fs.FinalDieRoll = ApplyAttackDieMods(ref fs, ps);
        //else
        //    fs.FinalDieRoll = ApplyDefendDieMods(ref fs, ps);

        //Debug.Log($"{ps.PlayerName} rolled Face #{fs.DieFaceNumber} - | Base Roll = [{fs.BaseDieRoll}] | Final Roll = [{fs.FinalDieRoll}]");

        fs.FighterPhase = FighterPhase.DieRolled;
    }

    private int ApplyAttackDieMods(int index, ref FighterState fs, PlayerState ps)
    {
        //int rolledFaceIndex;// = Random.Range(0, 6) // Which dice index was selected
        //fs.FinalDieRoll = fs.BaseDieRoll;
        var finalAtkValue = 0;
        var ed = ps.EntityData;
        var ti = (int)fs.DieType; // type index

        //DieConfig die = ed.EntityStats.dieConfigs[ti];

        var statDieFlatMod = ed.CurrentStatsModifier.dieModifiers[ti].finalResultAtkFlatModifier;
        var statDieMultMod = ed.CurrentStatsModifier.dieModifiers[ti].finalResultAtkMultModifier;

        var addRollMod = ed.CurrentStatsModifier.rollModifier;
        var timesToRoll = 1 + addRollMod;

        finalAtkValue += (int)((fs.CombatDieRolls[index].BaseDieRoll * statDieMultMod) + statDieFlatMod);

        return finalAtkValue;
    }

    private int ApplyDefendDieMods(int index,ref FighterState fs, PlayerState ps)
    {
        var finalDefValue = 0;
        var ed = ps.EntityData;
        var ti = (int)fs.DieType; // type index

        var statDieFlatMod = ed.CurrentStatsModifier.dieModifiers[ti].finalResultDefFlatModifier;

        finalDefValue = (int)(fs.CombatDieRolls[index].BaseDieRoll + statDieFlatMod);

        return finalDefValue;
    }

    private void HandleCombatDieRolled(GameState prevState, ref GameState newState, PlayerState ps)
    {
        ref var combatInstance = ref newState.CurrentCombatInstance;
        //ref FighterState fs = ref combatInstance.Fighters[0];
        ref FighterState fs = ref FindFighter(prevState, ref newState, ps);
        var side = FindTeamSide(prevState, ref newState, ps);
        //for (int i = 0; i < combatInstance.Fighters.Length; i++)
        //{
        //    if (combatInstance.Fighters[i].EntityId == ps.Id)
        //    {
        //        fs = ref combatInstance.Fighters[i];
        //    }
        //}

        RollCombatDie(newState.CurrentCombatInstance.Teams[side].ActionPhase, ref fs, ps);
    }

    private void HandleCombatCalculation(GameState prevState, ref GameState newState)
    {
        ref var combatInstance = ref newState.CurrentCombatInstance;

        var attackers = combatInstance.FindTeamByPhase(Action.ActionPhase.Attack).Fighters;
        var defenders = combatInstance.FindTeamByPhase(Action.ActionPhase.Defend).Fighters;

        for(int i = 0; i < attackers.Length; i++)
        {
            for(int j = 0; j < defenders.Length; j++)
            {
                //Setup
                var attacker = attackers[i];
                var defender = defenders[j];

                ref var atkEntity = ref newState.FindEntityById(attacker.EntityId);
                ref var defEntity = ref newState.FindEntityById(defender.EntityId);

                var poop = new CombatSequenceData();

                poop.AttackerId = attacker.EntityId;
                poop.DefenderId = defender.EntityId;

                poop.DetermineTypeAdvantage(attacker.DieType, defender.DieType);
                poop.DetermineTypeAdvantageMultiplier(atkEntity.EntityData, defEntity.EntityData);
                poop.CalculateDamage(attacker, defender);

                // Actual logic stuffs
                defEntity.EntityData.Health -= poop.FinalDamageDealt;
                if(defEntity.EntityData.Health <= 0)
                {
                    // Add some overkill reward increase here?
                    defEntity.EntityData.Health = 0;
                    poop.IsDamageLethal = true;
                }
                else
                    poop.IsDamageLethal = false;

                //combatInstance.CombatSequenceDatas.Add(poop);

                // Reward time? move this eleswhere
                if(defEntity.EntityData.EntityType is EntityType.Enemy
                    && poop.IsDamageLethal)
                {
                    poop.MoneyGain = defEntity.EntityData.Money;
                    defEntity.EntityData.Money = 0;

                    poop.ExpGain = defEntity.EntityData.Experience;

                    poop.ItemDrops = new ItemStats[2];
                    for(int k = 0; k < poop.ItemDrops.Length; k++)
                    {
                        poop.ItemDrops[k] = HandleEnemyItemDrop(defEntity.EnemyData);
                        atkEntity.EntityData.Inventory.Add(poop.ItemDrops[k]); // temp
                    }
                }

                combatInstance.CombatSequenceDatas.Add(poop);
            }
        }

        foreach(var sequence in combatInstance.CombatSequenceDatas)
        {
            // ???
            ref var atkEntity = ref newState.FindEntityById(sequence.AttackerId);
            ref var defEntity = ref newState.FindEntityById(sequence.DefenderId);
        }
    }

    private ItemStats HandleEnemyItemDrop(EnemyData enemyData)
    {
        ItemStats drop = null;
        var weightTotal = 0;
        foreach (var potentialItem in enemyData.LootPool)
        {
            weightTotal += potentialItem.Weight;
        }

        var rng = _rng.Next(0, weightTotal);
        //Debug.Log($"RNG Number = {rng} | WeightTotal = {weightTotal}");

        //int selectedEnemyId = -1;
        for (int i = 0; i < enemyData.LootPool.Count; i++)
        {
            if (rng <= enemyData.LootPool[i].Weight)
            {
                //<color=#{newState.Enemies[i].EntityData.MainColor.ToHexString()}>{newState.Enemies[i].PlayerName}</color>
                //Debug.Log($"Item Dropped: {enemyData.LootPool[i].Item}");
                drop = enemyData.LootPool[i].Item;
                break;
            }
            else
                rng -= enemyData.LootPool[i].Weight;
        }

        Debug.Log($"RNG Number = {rng} | WeightTotal = {weightTotal} | Item Dropped: {drop}");
        return drop;
    }
}
