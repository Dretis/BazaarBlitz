using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/MovementDieMultValueMod")]
public class MovementDieMultValueMod : StatModifierChangerSO
{
    public int movementMultModifier;

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.movementMultModifier *= movementMultModifier;

        return currentStats;
    }
}
