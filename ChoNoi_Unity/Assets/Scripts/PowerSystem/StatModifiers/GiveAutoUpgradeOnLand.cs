using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/GiveAutoUpgradeOnLand")]
public class GiveAutoUpgradeOnLand : StatModifierChangerSO
{
    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.canAutoUpgradeOnLand = true;

        return currentStats;
    }
}
