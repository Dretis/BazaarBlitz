using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/MaxHealthStatMultValueMod")]
public class MaxHealthStatMultValueMod : StatModifierChangerSO
{
    public int maxHealthMultModifier;

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.maxHealthMultModifier *= maxHealthMultModifier;

        return currentStats;
    }
}
