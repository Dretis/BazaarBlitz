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

    public string itemName;
    public int duration = 1;

    [Header("Shop and Visual Information")]
    public Sprite itemSprite;
    [TextArea (2, 10)]
    public string effectDescription;
    [TextArea(2, 10)]
    public string flavorText;
    public int basePrice;

    public List<StatModifierChangerSO> modifiers;

    public EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats)
    {
        foreach(var modifier in modifiers)
        {
            currentStats = modifier.ApplyStatModChanges(currentStats);
        }

        return currentStats;
    }
}
