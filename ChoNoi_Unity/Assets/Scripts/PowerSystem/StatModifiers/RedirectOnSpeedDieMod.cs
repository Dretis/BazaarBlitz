using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/RedirectOnSpeedDieMod")]
public class RedirectOnSpeedDieMod : StatModifierChangerSO
{
    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.redirectOnSpeedDie = true;

        return currentStats;
    }
}
