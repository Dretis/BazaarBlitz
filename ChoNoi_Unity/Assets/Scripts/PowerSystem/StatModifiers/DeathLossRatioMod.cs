using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/DeathLossRatioMod")]
public class DeathLossRatioMod : StatModifierChangerSO
{
    [Range(-1, 1)] public float deathLossRatio;

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.deathLossRatio += deathLossRatio;

        return currentStats;
    }
}
