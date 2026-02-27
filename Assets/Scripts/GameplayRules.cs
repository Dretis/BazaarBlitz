
using HeathenEngineering.SteamworksIntegration;
using UnityEngine;

/// <summary>
/// Struct that houses all provided/given information for a given game session of Cho Noi
/// </summary>
[System.Serializable]
public struct GameplayRules
{
    public GameplayTest.GameBoard board;

    [Range(2000, 6000)]
    public int pointGoal;

    [Range(0, 10)]
    public int storeLimit;

    [Range(1, 4)]
    public int numberOfPlayers;

    public GameplayRules(LobbyData lobbyData)
    {
        UnityEngine.Debug.Log($"Constructing GameplayRules from Lobby '{lobbyData.Name}'");
        //UnityEngine.Debug.Log($"lobbyData[\"game board\"] = '{lobbyData["game board"]}'");

        var boardIndex = int.Parse(lobbyData["game board"]);
        board = (GameplayTest.GameBoard) boardIndex;

        pointGoal = 4000;
        storeLimit = 4; 
        numberOfPlayers = lobbyData.MaxMembers;
    }

    public GameplayRules(int boardIndex, int g, int sl, int players)
    {
        UnityEngine.Debug.Log($"Constructing GameplayRules from player set rules.'");

        board = (GameplayTest.GameBoard)boardIndex;

        pointGoal = g;
        storeLimit = sl;
        numberOfPlayers = players;
    }

    public GameplayRules(GameplayRules ruleset)
    {
        board = ruleset.board;

        pointGoal = ruleset.pointGoal;
        storeLimit = ruleset.storeLimit;
        numberOfPlayers = ruleset.numberOfPlayers;
    }

    public void FinishConfigureGameplayRules()
    {

    }
}
