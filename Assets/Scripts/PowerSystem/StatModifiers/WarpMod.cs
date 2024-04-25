using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/WarpMod")]
public class WarpMod : StatModifierChangerSO
{
    public MapNode node;

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.warpDestination = node;

        return currentStats;
    }
}

