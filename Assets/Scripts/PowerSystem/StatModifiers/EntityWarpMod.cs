using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/EntityWarpMod")]
public class EntityWarpMod : StatModifierChangerSO
{
    public EntityPiece entity;

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        currentStats.warpDestination = entity.occupiedNode;

        return currentStats;
    }
}

