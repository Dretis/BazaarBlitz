using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/WarpMod")]
public class SelectTileWarpMod : StatModifierChangerSO
{

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.warpMode = EntityStatsModifiers.WarpMode.Tiles;

        return currentStats;
    }
}

