using UnityEngine;

[System.Serializable]
public class EntityStatsModifiers
{
    public EntityStatsModifiers()
    {
        for (int i = 0; i < dieModifiers.Length; i++)
        {
            dieModifiers[i] = new();
        }
    }

    // I know marigold isnt warping but it lets us not bloat gameplaytest even more.
    // considering this is the main way to select tiles, it should probably be renamed.
    public enum WarpMode
    {
        None,
        Tiles,
        Players,
        Marigold,
        Rafflesia
    }

    // List of all stats
    public DieModifier[] dieModifiers = new DieModifier[(int)EntityBaseStats.DieTypes.Count];

    [Header("General Mods")]
    [Range(0, 1)] public float deathLossRatio = 0.5f;
    public float interestRate = 0; // Wealth Blessing
    public int rollModifier;

    [Header("Movement-based Mods")]
    public int movementFlatModifier = 0;
    public int movementMultModifier = 1;
    public int movementTempoScale = 0; // Tempo-Velocity Blessing
    public bool canUseSpeedDie = false;

    [Header("Combat Mods")]
    public float strongMultModifier = 0; // Power Blessing
    public float neutralMultModifier = 0; // Power Blessing
    public float resistMultModifier = 0; // Power Blessing
    public bool canInflictWrath = false;
    public bool hasGreed = false;

    [Header("Survival-based Mods")]
    public float defenseModifier = 0;
    public float lifestealMult = 0;
    public int healthRegen = 0;

    public int maxHealthFlatModifier = 0;
    public int maxHealthMultModifier = 1;

    [Header("Trap-based Mods")]
    public MapNode warpDestination = null;
    public WarpMode warpMode = WarpMode.None;
    
    public bool canStealOnPassBy = false;
    public bool canInitiateCombatOnPassBy = false;
    public bool canStopOnStoreOnPassBy = false;


    public float ApplyDieModifier(EntityBaseStats.DieTypes dieType, float baseRollValue)
    {
        Debug.Log("Original Roll" + baseRollValue);

        var dieMod = dieModifiers[(int)dieType];
        baseRollValue *= dieMod.finalResultMultModifier;
        baseRollValue += dieMod.finalResultFlatModifier;

        Debug.Log("New Roll after item effect:" + baseRollValue);

        return baseRollValue;
    }
}

[System.Serializable]
public class DieModifier
{
    // Offset the final result by this value
    public float finalResultFlatModifier = 0;
    public float finalResultMultModifier = 1;
}
