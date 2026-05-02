using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/Lifesteal")]
public class Lifesteal : StatModifierChangerSO
{
    public float lifestealMult;

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.lifestealMult += lifestealMult;

        return currentStats;
    }
}
