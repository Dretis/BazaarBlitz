using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "StatMods/RandomWarpToEntitiesWarpMod")]
public class RandomWarpToEntitiesMod : StatModifierChangerSO
{

    public override EntityStatsModifiers ApplyStatModChanges(EntityStatsModifiers currentStats, int currentTurn)
    {
        List<GameObject> playerGameObjects = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        List<EntityPiece> players = new List<EntityPiece>();

        foreach (var gameObject in playerGameObjects) 
        {
            EntityPiece player = gameObject.GetComponent<EntityPiece>();
            if (player != GameplayTest.instance.currentPlayer)
                players.Add(player);
        }
        
        currentStats.warpDestination = players[Random.Range(0, players.Count)].occupiedNode;

        return currentStats;
    }
}

