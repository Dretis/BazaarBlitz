using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[CreateAssetMenu]
public class ItemStats : ScriptableObject
{
    public enum PhaseTypes { Attack, Defend, Both }
    public enum WeaponTypes { Melee, Gun, Magic, Special}

    public string itemName;

    [Tooltip("What phase is the item usable in")]
    public PhaseTypes phase;

    public int diesToRoll;
    public int bonusDamageMin;
    public int bonusDamageMax;
    public int playerDamageMin; // can be negative!
    public int playerDamageMax;
    public bool blocksWithBonusDamage;

    public string specialID; // Will be checked in a switch statement for specific stuff.

    [Header("Shop and Visual Information")]
    public Sprite itemSprite;
    [TextArea (2, 10)]
    public string effectDescription;
    [TextArea(2, 10)]
    public string flavorText;
    public int basePrice;
}
