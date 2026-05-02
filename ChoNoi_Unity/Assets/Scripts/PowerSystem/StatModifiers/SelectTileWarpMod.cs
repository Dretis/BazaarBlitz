using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/SelectTileWarpMod")]
public class SelectTileWarpMod : StatModifierChangerSO
{

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.warpMode = EntityStatsModifiers.WarpMode.Tiles;

        return currentStats;
    }
}

