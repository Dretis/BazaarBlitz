public class MovementDieFlatValueMod : StatModifierChangerSO
{
    public int movementFlatModifier;

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.movementFlatModifier += movementFlatModifier;

        return currentStats;
    }
}
