using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntityStatsModifiers 
{
    public EntityStatsModifiers()
    {
        for(int i = 0;i < dieModifiers.Length;i++)
        {
            dieModifiers[i] = new();
        }
    }

    // List of all stats
    public DieModifier[] dieModifiers = new DieModifier[(int)EntityBaseStats.DieTypes.Count];

    public float defenseModifier;

    public float ApplyDieModifier(EntityBaseStats.DieTypes dieType, float baseRollValue)
    {
        Debug.Log(baseRollValue);

        var dieMod = dieModifiers[(int)dieType];
        baseRollValue *= dieMod.finalResultMultModifier;
        baseRollValue += dieMod.finalResultFlatModifier;

        Debug.Log(baseRollValue);

        return baseRollValue;
    }
}

[System.Serializable]
public class DieModifier
{
    // Offset the final result by this value
    public float finalResultFlatModifier = 0;
    public float finalResultMultModifier = 1;
}
