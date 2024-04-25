using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/RandEntityWarpMod")]
public class RandomEntityWarpMod : StatModifierChangerSO
{
    public List<EntityPiece> entities;

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.warpDestination = entities[Random.Range(0, entities.Count)].occupiedNode;

        return currentStats;
    }
}

