using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/SelectTilePlantMod")]
public class SelectTilePlantMod : StatModifierChangerSO
{
    [SerializeField] EntityStatsModifiers.WarpMode plantType;

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.warpMode = plantType;

        return currentStats;
    }
}

