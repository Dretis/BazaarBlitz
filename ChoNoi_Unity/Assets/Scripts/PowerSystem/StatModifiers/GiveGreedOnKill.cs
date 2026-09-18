using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/GiveGreedOnKill")]
public class GiveGreedOnKill : StatModifierChangerSO
{
    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.canGreedOnKill = true;

        return currentStats;
    }
}
