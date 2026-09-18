using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/MovementTempoScaleMod")]
public class MovementTempoScaleMod : StatModifierChangerSO
{
    public int movementTempoScale;

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.movementTempoScale += movementTempoScale;

        return currentStats;
    }
}
