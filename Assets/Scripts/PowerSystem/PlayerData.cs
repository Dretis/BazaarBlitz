using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    // cringe
    public float health;
    public float maxHealth;

    public DieConfig strDie => entityStats.dieConfigs[(int)EntityBaseStats.DieTypes.Strength];
    public DieConfig intDie => entityStats.dieConfigs[(int)EntityBaseStats.DieTypes.Dex];
    public DieConfig dexDie => entityStats.dieConfigs[(int)EntityBaseStats.DieTypes.Int];



    public string playerName; // Name for the player

    public int playerID; // 1-4 for final game, 1-2 (where 1 is attacker for this).
    public int combatSceneIndex = -1; // -1 indicates player is not in battle

    public bool isEnemy;
    public int favoredAttack;

    public EntityBaseStats entityStats = new();

    public List<Action> attackActions;
    public List<Action> defendActions;
    public CombatUIManager.FightingPosition fightingPosition; // Just for the combat, will change


    class ActiveEffect
    {
        public IStatModifierChanger statMod;
        public int turnsRemaining;
    }

    public EntityStatsModifiers currentStatsModifier;

    public List<ItemStats> inventory = new();

    private List<ActiveEffect> activeEffects = new();

    /// <summary>
    /// Add a specified item to this player's list of active stat modifier effects
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="item"></param>
    public void AddItemToActiveEffects(int duration, ItemStats item)
    {
        activeEffects.Add(new ActiveEffect
        {
            statMod = item,
            turnsRemaining = duration
        });
    }

    /// <summary>
    /// Tick down active effects by one turn and recalculate this player's stat modifiers
    /// </summary>
    public void UpdateStatModifiers()
    {
        TickDownActiveEffects();

        currentStatsModifier = new EntityStatsModifiers();

        foreach(var item in activeEffects)
        {
            currentStatsModifier = item.statMod.ApplyStatModChanges(currentStatsModifier);
        }
    }

    private void TickDownActiveEffects()
    {
        for(int i = 0;i < activeEffects.Count;i++)
        {
            activeEffects[i].turnsRemaining--;

            if(activeEffects[i].turnsRemaining < 0)
            {
                activeEffects.RemoveAt(i);
                i--;
            }
        }
    }
}
