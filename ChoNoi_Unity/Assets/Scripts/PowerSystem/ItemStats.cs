using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu]
public class ItemStats : ScriptableObject, IStatModifierChanger
{
    /*
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
    */
    // Item 'Type' is based on what it does
    public enum ItemType
    {
        Generic,
        Blessing,
        TargetSelect,
        Deployable, // for traps
        Buff,
        Debuff,
        Heal
    }

    // Item 'Category' is based on what it looks like
    public enum ItemCategory
    {
        Wealth,
        Power,
        Speed,
        Misc,
        Cube,
        Fruit,
        Drink,
        Food,
        Flower,
    }
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Legendary
    }

    public int Duration => duration;
    public bool IsPermanent => isPermanent;
    public bool ClearAfterCombat => clearAfterCombat;

    public string itemName;
    [SerializeField] private int duration = 1;
    [SerializeField] private bool isPermanent;
    public bool showAsEffect = true;
    public bool usableInCombat = true;
    public bool clearAfterCombat = false;

    [Header("Grouping Info")]
    public ItemType type;
    public ItemCategory category;
    //public ItemRarity rarity;

    [Header("Shop and Visual Information")]
    public Sprite itemSprite;


    [TextArea (2, 10)]
    public string effectDescription;
    [TextArea(2, 10)]
    public string flavorText;

    public LocalizedString l_itemName;
    public LocalizedString l_effect;
    public LocalizedString l_flavor;

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
            if ((currentTurn >= effectStartTurn && currentTurn <= effectEndTurn) || isPermanent)
            {
                Debug.Log("Current Turn: " + currentTurn);
                Debug.Log("My effect is activating.");
                currentStats = modifier.ApplyStatModChanges(currentStats, currentTurn);
            }
                
        }

        return currentStats;
    }
}
