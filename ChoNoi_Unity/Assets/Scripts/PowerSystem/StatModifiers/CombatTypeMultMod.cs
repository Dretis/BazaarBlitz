using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/CombatTypeMultMod")]
public class CombatTypeMultMod : StatModifierChangerSO
{
    public float strongMultModifier = 0;
    public float neutralMultModifier = 0;
    public float resistMultModifier = 0;

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.strongMultModifier += strongMultModifier;
        currentStats.neutralMultModifier += neutralMultModifier;
        currentStats.resistMultModifier += resistMultModifier;

        return currentStats;
    }
}
