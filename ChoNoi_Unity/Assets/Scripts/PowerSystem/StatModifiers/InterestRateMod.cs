using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/InterestRateMod")]
public class InterestRateMod : StatModifierChangerSO
{
    public float interestRate;

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.interestRate += interestRate;

        return currentStats;
    }
}
