using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public string playerName; // Name for the player

    public int playerID; // 1-4 for final game, 1-2 (where 1 is attacker for this).
    public int combatSceneIndex = -1; // -1 indicates player is not in battle
    public float[] strDie;
    public float[] dexDie; // Roll 0-5 index for all of these.
    public float[] intDie;
    public float health;
    public List<ItemStats> inventory; // spaghetti implementation: list of int IDs of items
    public bool isEnemy;
    public int favoredAttack;
    // LIST OF INVENTORY ITEM IDS:
    // 0 = probably some debug item or empty space slot
    // 1 = potato, can be thrown for ???
    // 2 = golden potato, can be thrown for ???
    // 3 = club (passive), grants reusable basic attack
    // 4 = shotgun (passive), grants + 3 damage to damage items
    // 5 = ???

    public List<Action> attackActions;
    public List<Action> defendActions;
    public CombatUIManager.FightingPosition fightingPosition; // Just for the combat, will change
}
