using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/SelectEntityWarpMod")]
public class EntityWarpMod : StatModifierChangerSO
{

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.warpMode = EntityStatsModifiers.WarpMode.Players;

        return currentStats;
    }
}

