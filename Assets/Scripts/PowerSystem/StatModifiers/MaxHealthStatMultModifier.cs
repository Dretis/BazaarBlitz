using UnityEngine;

public class MaxHealthStatMultValueMod : StatModifierChangerSO
{
    public int maxHealthMultModifier;

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.movementMultModifier += maxHealthMultModifier;

        return currentStats;
    }
}
