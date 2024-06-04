using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/StopOnStoreOnPassByMod")]
public class StopOnStoreOnPassByMod : StatModifierChangerSO
{
    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.canStopOnStoreOnPassBy = true;

        return currentStats;
    }
}
