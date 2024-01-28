using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int playerID; // 1-4 for final game, 1-2 (where 1 is attacker for this).
    public float[] strDie;
    public float[] dexDie; // Roll 0-5 index for all of these.
    public float[] intDie;
    public float health;
    public List<int> inventory; // spaghetti implementation: list of int IDs of items
    public bool isEnemy;
    public int favoredAttack;
    // LIST OF INVENTORY ITEM IDS:
    // 0 = probably some debug item or empty space slot
    // 1 = potato, can be thrown for ???
    // 2 = golden potato, can be thrown for ???
    // 3 = club (passive), grants reusable basic attack
    // 4 = shotgun (passive), grants + 3 damage to damage items
    // 5 = ???



}
