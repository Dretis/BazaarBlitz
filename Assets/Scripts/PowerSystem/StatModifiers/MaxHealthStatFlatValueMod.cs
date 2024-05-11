using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/MaxHealthStatFlatValueMod")]
public class MaxHealthStatFlatValueMod : StatModifierChangerSO
{
    public int maxHealthFlatModifier;

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.maxHealthFlatModifier += maxHealthFlatModifier;

        return currentStats;
    }
}
