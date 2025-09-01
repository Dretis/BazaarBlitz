using UnityEngine;

[CreateAssetMenu]
public class Action : ScriptableObject
{
    public enum PhaseTypes 
    { 
        Attack, 
        Defend, 
        Both 
    }

    public enum WeaponTypes 
    {
        Melee, 
        Gun, 
        Magic, 
        Special 
    }

    public string actionName;

    [Tooltip("What phase the action is used in")]
    public PhaseTypes phase;

    [Tooltip("What weapon type is the action")]
    public WeaponTypes type;

    [Tooltip("What is the ID associated with this action's animation/type")]
    public int weaponID;

    [Range(0, 10)]
    public int diesToRoll;
    [Range(0, 100)]
    public int bonusDamage;


    [TextArea(3,10)]
    public string flavorText;

    // add associated animation (for player only)
}
