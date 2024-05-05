using UnityEngine;

public class MaxHealthStatFlatValueMod : StatModifierChangerSO
{
    public int maxHealthFlatModifier;

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.movementFlatModifier += maxHealthFlatModifier;

        return currentStats;
    }
}
