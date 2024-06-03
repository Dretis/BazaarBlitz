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

    public float defenseModifier = 0;
    public float lifestealMult = 0;
    public int healthRegen = 0;
    public int rollModifier;
    public int maxHealthFlatModifier = 0;
    public int maxHealthMultModifier = 1;
    public int movementFlatModifier = 0;
    public int movementMultModifier = 1;
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
