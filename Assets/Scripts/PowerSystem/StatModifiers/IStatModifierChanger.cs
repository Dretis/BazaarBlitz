using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStatModifierChanger
{
    public EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats);

}

public abstract class StatModifierChangerSO : ScriptableObject, IStatModifierChanger
{
    public abstract EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats);
}