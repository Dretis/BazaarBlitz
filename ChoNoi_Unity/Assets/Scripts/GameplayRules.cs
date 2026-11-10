
using HeathenEngineering.SteamworksIntegration;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Struct that houses all provided/given information for a given game session of Cho Noi
/// </summary>
[System.Serializable]
public struct GameplayRules
{
    public GameplayTest.GameBoard Board;

    [Range(2000, 6000)]
    public int PointGoal;

    [Range(0, 10)]
    public int StoreLimit;

    [Range(1, 4)]
    public int NumberOfPlayers;

    [Header("Enemy Spawn ")]
    public List<EnemyPool> EnemyPools;
    //public GameplayRules(LobbyData lobbyData)
    //{
    //    UnityEngine.Debug.Log($"Constructing GameplayRules from Lobby '{lobbyData.Name}'");
    //    //UnityEngine.Debug.Log($"lobbyData[\"game board\"] = '{lobbyData["game board"]}'");

    //    var boardIndex = int.Parse(lobbyData["game board"]);
    //    board = (GameplayTest.GameBoard) boardIndex;

    //    PointGoal = 4000;
    //    storeLimit = 4; 
    //    numberOfPlayers = lobbyData.MaxMembers;
    //}

    //public GameplayRules(int boardIndex, int g, int sl, int players)
    //{
    //    UnityEngine.Debug.Log($"Constructing GameplayRules from player set rules.'");

    //    board = (GameplayTest.GameBoard)boardIndex;

    //    PointGoal = g;
    //    storeLimit = sl;
    //    numberOfPlayers = players;
    //}

    //public GameplayRules(GameplayRules ruleset)
    //{
    //    board = ruleset.board;

    //    PointGoal = ruleset.PointGoal;
    //    storeLimit = ruleset.storeLimit;
    //    numberOfPlayers = ruleset.numberOfPlayers;
    //}
}

[System.Serializable]
public struct EnemyPool
{
    [SerializeField] private string _poolName; //unused
    public float AverageLevelReq; // when the avg of all player Levels is higher, make this the current enemypool
    public EnemyEncounterInfo[] EnemyEncounterInfos;
}

[System.Serializable]
public struct EnemyEncounterInfo
{
    [Range(-12,-1)]public int EnemyId;
    public int EncounterWeight;

    public EnemyEncounterInfo(int enemyId, int encounterWeight)
    {
        EnemyId = enemyId;
        EncounterWeight = encounterWeight;
    }
}
