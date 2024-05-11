using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/RandWarpToMapNodesMod")]
public class RandomWarpToMapNodesMod : StatModifierChangerSO
{
    public List<MapNode> nodes;

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        // Try this for now, might change to be more efficient later.
        List<MapNode> nodes = new List<MapNode>(GameObject.FindObjectsOfType<MapNode>());
        currentStats.warpDestination = nodes[Random.Range(0, nodes.Count)];

        return currentStats;
    }
}

