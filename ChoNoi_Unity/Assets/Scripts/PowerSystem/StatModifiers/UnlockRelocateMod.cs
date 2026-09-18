using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/UnlockRelocateMod")]
public class UnlockRelocateMod : StatModifierChangerSO
{
    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.canUseRelocate = true;

        return currentStats;
    }
}
