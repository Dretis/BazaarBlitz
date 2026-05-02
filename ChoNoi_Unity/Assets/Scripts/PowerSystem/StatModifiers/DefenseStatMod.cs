using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/DefenseMod")]
public class DefenseStatMod : StatModifierChangerSO
{
    public float defenseModifier;

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.defenseModifier += defenseModifier;

        return currentStats;
    }
}
