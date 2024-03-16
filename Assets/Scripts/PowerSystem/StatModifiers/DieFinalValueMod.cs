using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/DieMod")]
public class DieFinalValueMod : StatModifierChangerSO
{
    public EntityBaseStats.DieTypes targetDie;
    public float flatModifier;
    public float multModifier;

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats)
    {
        currentStats.dieModifiers[(int)targetDie].finalResultFlatModifier += flatModifier;
        currentStats.dieModifiers[(int)targetDie].finalResultMultModifier *= multModifier;

        return currentStats;
    }
}
