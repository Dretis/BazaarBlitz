using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/AllDiesMod")]
public class AllDiesFinalValueMod : StatModifierChangerSO
{
    public float flatModifier;
    public float multModifier;
    public bool consumable = false;

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats)
    {
        currentStats.dieModifiers[(int)EntityBaseStats.DieTypes.Strength].finalResultFlatModifier += flatModifier;
        currentStats.dieModifiers[(int)EntityBaseStats.DieTypes.Strength].finalResultMultModifier *= multModifier;
        currentStats.dieModifiers[(int)EntityBaseStats.DieTypes.Int].finalResultFlatModifier += flatModifier;
        currentStats.dieModifiers[(int)EntityBaseStats.DieTypes.Int].finalResultMultModifier *= multModifier;
        currentStats.dieModifiers[(int)EntityBaseStats.DieTypes.Dex].finalResultFlatModifier += flatModifier;
        currentStats.dieModifiers[(int)EntityBaseStats.DieTypes.Dex].finalResultMultModifier *= multModifier;

        if (consumable) {
            //currentStats.consumed = true;
        }


        return currentStats;
    }
}
