using UnityEngine;

public interface IStatModifierChanger
{
    public int Duration
    {
        get;
    }

    public EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn);

}

public abstract class StatModifierChangerSO : ScriptableObject, IStatModifierChanger
{
    // Activate effect on activateEffectStartTurn, end effect on activateEffectEndTurn.
    public int Duration
    {
        get;
    }

    // Minimum of 1.
    public int activateEffectStartTurn;

    // Maximum of Duration.
    public int activateEffectEndTurn;

    public abstract EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn);
}