using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/AllDiesMod")]
public class AllDiesFinalValueMod : StatModifierChangerSO
{
    public float flatAtkModifier;
    public float flatDefModifier;
    public float multModifier;
    public bool consumable = false;

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.dieModifiers[(int)EntityBaseStats.DieTypes.Strength].finalResultAtkFlatModifier += flatAtkModifier;
        currentStats.dieModifiers[(int)EntityBaseStats.DieTypes.Strength].finalResultDefFlatModifier += flatDefModifier;
        currentStats.dieModifiers[(int)EntityBaseStats.DieTypes.Strength].finalResultAtkMultModifier *= multModifier;

        currentStats.dieModifiers[(int)EntityBaseStats.DieTypes.Int].finalResultAtkFlatModifier += flatAtkModifier;
        currentStats.dieModifiers[(int)EntityBaseStats.DieTypes.Int].finalResultDefFlatModifier += flatDefModifier;
        currentStats.dieModifiers[(int)EntityBaseStats.DieTypes.Int].finalResultAtkMultModifier *= multModifier;

        currentStats.dieModifiers[(int)EntityBaseStats.DieTypes.Dex].finalResultAtkFlatModifier += flatAtkModifier;
        currentStats.dieModifiers[(int)EntityBaseStats.DieTypes.Dex].finalResultDefFlatModifier += flatDefModifier;
        currentStats.dieModifiers[(int)EntityBaseStats.DieTypes.Dex].finalResultAtkMultModifier *= multModifier;

        if (consumable) {
            //currentStats.consumed = true;
        }


        return currentStats;
    }
}
