using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/HealthRegenMod")]
public class HealthRegenMod : StatModifierChangerSO
{
    public int healthRegen;

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.healthRegen += healthRegen;

        return currentStats;
    }
}

