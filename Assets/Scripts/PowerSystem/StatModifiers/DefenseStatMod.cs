using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/DefenseMod")]
public class DefenseStatMod : StatModifierChangerSO
{
    public float defenseModifier;

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats)
    {
        currentStats.defenseModifier += defenseModifier;

        return currentStats;
    }
}
