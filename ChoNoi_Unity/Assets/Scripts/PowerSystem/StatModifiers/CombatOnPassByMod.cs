using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/CombatOnPassByMod")]
public class CombatOnPassByMod : StatModifierChangerSO
{
    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.canInitiateCombatOnPassBy = true;

        return currentStats;
    }
}