using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu]
public class Action : ScriptableObject
{
    public enum ActionPhase 
    { 
        Attack, 
        Defend, 
        Both 
    }

    public enum WeaponType 
    {
        Melee, 
        Gun, 
        Magic, 
        Speed,
        None
    }

    public string actionName;
    public LocalizedString l_actionName;

    [Tooltip("What phase the action is used in")]
    public ActionPhase phase;

    [Tooltip("What weapon type is the action")]
    public WeaponType type;

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
