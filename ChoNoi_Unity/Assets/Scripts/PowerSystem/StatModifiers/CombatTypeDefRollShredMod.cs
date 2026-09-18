using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/CombatTypeDefRollShredMod")]
public class CombatTypeDefRollShredMod : StatModifierChangerSO
{
    public int strongDefRollShred = 0;
    public int neutralDefRollShred = 0;
    public int resistDefRollShred = 0;

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.strongDefRollShred += strongDefRollShred;
        currentStats.neutralDefRollShred += neutralDefRollShred;
        currentStats.resistDefRollShred += resistDefRollShred;

        return currentStats;
    }
}
