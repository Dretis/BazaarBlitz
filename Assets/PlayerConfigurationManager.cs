using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerConfigurationManager : MonoBehaviour
{
    private List<PlayerConfiguration> playerConfigs;

    [SerializeField] private int maxPlayers = 4;

    public static PlayerConfigurationManager instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
            Debug.Log("[SINGLETON] Trying to create another PlayerConfigurationManager instance!!!");
        else
        {
            instance = this;
            DontDestroyOnLoad(instance);
            playerConfigs = new List<PlayerConfiguration>();
        }
    }

    public void SetPlayerColor(int index, Color color)
    {
        playerConfigs[index].PlayerColor = color;
    }

    public void ReadyPlayer(int index)
    {
        playerConfigs[index].IsReady = true;
        if(playerConfigs.Count == maxPlayers && playerConfigs.All(p=>p.IsReady ==true))
        {
            SceneManager.LoadScene("Overworld 2");
        }
    }

    public void HandlePlayerJoin(PlayerInput pi)
    {
        Debug.Log($"Player Joined {pi.playerIndex}");

        // Make sure we didn't already add this player
        if(!playerConfigs.Any(p=>p.PlayerIndex == pi.playerIndex))
        {
            pi.transform.SetParent(transform); // Becomes child of the ParentConfigManager to perist across scenes
            playerConfigs.Add(new PlayerConfiguration(pi));
        }
    }
}

public class PlayerConfiguration
{
    public PlayerInput Input { get; set; }

    public int PlayerIndex { get; set; }
    public bool IsReady { get; set; }

    public Color PlayerColor { get; set; }

    public PlayerConfiguration(PlayerInput pi)
    {
        PlayerIndex = pi.playerIndex;
        Input = pi;
    }
}