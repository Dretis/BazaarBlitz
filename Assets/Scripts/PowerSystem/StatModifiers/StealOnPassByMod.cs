using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/StealOnPassByMod")]
public class StealOnPassByMod : StatModifierChangerSO
{
    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.canStealOnPassBy = true;

        return currentStats;
    }
}
