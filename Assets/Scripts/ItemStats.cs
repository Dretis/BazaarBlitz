using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[CreateAssetMenu]
public class Action : ScriptableObject
{
    public enum PhaseTypes { Attack, Defend, Both }
    public enum WeaponTypes { Melee, Gun, Magic, Special}

    public string actionName;

    [Tooltip("What phase is the item usable in")]
    public PhaseTypes phase;

    public int diesToRoll;
    public int bonusDamage;

    [Tooltip("What weapon type is the action")]
    public WeaponTypes type;

    public string flavorText;
}
