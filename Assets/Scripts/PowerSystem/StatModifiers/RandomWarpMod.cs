using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/RandWarpMod")]
public class RandomWarpMod : StatModifierChangerSO
{
    public List<MapNode> nodes;

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.warpDestination = nodes[Random.Range(0, nodes.Count)];

        return currentStats;
    }
}

