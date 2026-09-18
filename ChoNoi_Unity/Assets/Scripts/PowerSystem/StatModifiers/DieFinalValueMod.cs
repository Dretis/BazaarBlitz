using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/DieMod")]
public class DieFinalValueMod : StatModifierChangerSO
{
    public EntityBaseStats.DieTypes targetDie;
    public float flatAtkModifier;
    public float flatDefModifier;
    public float multModifier = 1;
    public int rollModifier = 1;
    public bool consumable = false;

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.dieModifiers[(int)targetDie].finalResultAtkFlatModifier += flatAtkModifier;
        currentStats.dieModifiers[(int)targetDie].finalResultDefFlatModifier += flatDefModifier;
        currentStats.dieModifiers[(int)targetDie].finalResultMultModifier *= multModifier;

        currentStats.rollModifier = rollModifier;

        if (consumable) {
          //currentStats.consumed = true;
        }



        return currentStats;
    }
}
