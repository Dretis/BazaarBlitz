using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ItemStats : ScriptableObject, IStatModifierChanger
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

    public int Duration => duration;

    public string itemName;
    [SerializeField]
    private int duration = 1;

    [Header("Shop and Visual Information")]
    public Sprite itemSprite;
    [TextArea (2, 10)]
    public string effectDescription;
    [TextArea(2, 10)]
    public string flavorText;
    public int basePrice;

    public List<StatModifierChangerSO> modifiers;

    public EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        foreach (var modifier in modifiers)
        {
            // Error checking.
            int effectStartTurn = Mathf.Max(1, modifier.activateEffectStartTurn); // Must be at least 1.
            int effectEndTurn = Mathf.Max(1, modifier.activateEffectEndTurn); // Must be at least 1.
            effectEndTurn = Mathf.Min(effectEndTurn, Duration); // Must be less than or equal to duration.
            effectStartTurn = Mathf.Min(effectStartTurn, effectEndTurn); // Must be less than or equal to effectEndTurn.

            // Apply mod effect if in range of start and end turns.
            if (currentTurn >= effectStartTurn && currentTurn <= effectEndTurn)
            {
                Debug.Log("Current Turn: " + currentTurn);
                Debug.Log("My effect is activating.");
                currentStats = modifier.ApplyStatModChanges(currentStats, currentTurn);
            }
                
        }

        return currentStats;
    }
}
