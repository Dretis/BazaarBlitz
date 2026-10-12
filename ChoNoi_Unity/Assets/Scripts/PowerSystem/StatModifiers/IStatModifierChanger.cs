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
    public bool canStack = false;
    // Minimum of 0.
    /// <summary>
    /// The number of turns before this Modifier is in effect.
    /// 0 is active immediately.
    /// </summary>
    [Range(0, 10)] public int activateEffectStartTurn;

    // Maximum of Duration.
    /// <summary>
    /// The number of turns before this Modifier is no longer in effect.
    /// </summary>
    [Range(0, 10)] public int activateEffectEndTurn;
    [Space]
    public Sprite effectIcon;

    public abstract EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn);
}