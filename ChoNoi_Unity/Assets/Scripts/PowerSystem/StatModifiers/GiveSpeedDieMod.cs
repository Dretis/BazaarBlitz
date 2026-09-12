using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/GiveSpeedDieMod")]
public class GiveSpeedDieMod : StatModifierChangerSO
{
    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.canUseSpeedDie = true;

        return currentStats;
    }
}
