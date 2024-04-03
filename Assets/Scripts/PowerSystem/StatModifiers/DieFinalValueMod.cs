using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/DieMod")]
public class DieFinalValueMod : StatModifierChangerSO
{
    public EntityBaseStats.DieTypes targetDie;
    public float flatModifier;
    public float multModifier = 1;
    public int rollModifier = 1;
    public bool consumable = false;

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats)
    {
        currentStats.dieModifiers[(int)targetDie].finalResultFlatModifier += flatModifier;
        currentStats.dieModifiers[(int)targetDie].finalResultMultModifier *= multModifier;

        currentStats.rollModifier = rollModifier;

        if (consumable) {
          //currentStats.consumed = true;
        }



        return currentStats;
    }
}
