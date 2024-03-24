using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/Lifesteal")]
public class Lifesteal : StatModifierChangerSO
{
    public float lifestealMult;

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats)
    {
        currentStats.lifestealMult += lifestealMult;



        return currentStats;
    }
}
