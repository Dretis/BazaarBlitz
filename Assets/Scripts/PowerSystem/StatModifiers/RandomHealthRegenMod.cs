using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/RandHealthRegenMod")]
public class RandomHealthRegenMod : StatModifierChangerSO
{
    public int healthRegenMin;
    public int healthRegenMax;

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.healthRegen += Random.Range(healthRegenMin, healthRegenMax);

        return currentStats;
    }
}

