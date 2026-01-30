using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerConfigurationManager : MonoBehaviour
{
    private PlayerInputManager playerInputManager;
    private List<PlayerConfiguration> playerConfigs;
    [SerializeField] private List<PlayerCosmeticManager> playerCosmeticManagers;

    [SerializeField] private int maxPlayers = 4;

    [Header("Broadcast on Event Channels")]
    public VoidEventChannelSO m_AllPlayersReady;

    [Header("Listen on Event Channels")]
    public VoidEventChannelSO m_ReturnToMainMenu;

    public static PlayerConfigurationManager instance { get; private set; }
    private void OnEnable()
    {
        m_AllPlayersReady.OnEventRaised += OnAllPlayersReady;
        m_ReturnToMainMenu.OnEventRaised += OnReturnToMainMenu;
    }

    private void OnDisable()
    {
        m_AllPlayersReady.OnEventRaised -= OnAllPlayersReady;
        m_ReturnToMainMenu.OnEventRaised -= OnReturnToMainMenu;
    }

    private void Awake()
    {
        if (instance != null)
            Debug.Log("[SINGLETON] Trying to create another PlayerConfigurationManager instance!!!");
        else
        {
            instance = this;
            DontDestroyOnLoad(instance);
            playerConfigs = new List<PlayerConfiguration>();
            playerCosmeticManagers = new List<PlayerCosmeticManager>();
            playerInputManager = GetComponent<PlayerInputManager>();
        }
    }

    public List<PlayerConfiguration> GetPlayerConfigs()
    {
        return playerConfigs;
    }

    public PlayerConfiguration GetPlayerConfig(int index)
    {
        return playerConfigs[index];
    }

    public PlayerCosmeticManager GetPlayerCosmeticManager(int index)
    {
        return playerCosmeticManagers[index];
    }

    public void SetPlayerColor(int index, Color color)
    {
        playerConfigs[index].PlayerColor = color;
        playerCosmeticManagers[index].playerColor = color;
    }

    public void SetPlayerPalette(int index, List<Color> palette)
    {
        playerCosmeticManagers[index].baggieColorPalette = palette;
    }

    public void SetPlayerName(int index, string newName)
    {
        playerConfigs[index].PlayerName = newName;
        playerCosmeticManagers[index].playerName = newName;
    }

    public void ReadyPlayer(int index)
    {
        playerConfigs[index].IsReady = true;
        if(playerConfigs.Count == maxPlayers && playerConfigs.All(p=>p.IsReady ==true))
        {
            Debug.Log("All players ready, going to the board select!");


            m_AllPlayersReady.RaiseEvent();
            //SceneManager.LoadScene("Overworld 2");
        }
    }

    public void UnreadyPlayer(int index)
    {
        playerConfigs[index].IsReady = false;
    }

    public void EnablePlayer(int index)
    {
        playerConfigs[index].IsReady = true;
    }

    public void DisablePlayer(int index)
    {
        playerConfigs[index].IsReady = false;
    }

    public PlayerInput GetPlayerInput(int index)
    {
        return playerConfigs[index].Input;
    }

    public void HandlePlayerJoin(PlayerInput pi)
    {
        Debug.Log($"Player Joined {pi.playerIndex}");

        // Make sure we didn't already add this player
        if(!playerConfigs.Any(p=>p.PlayerIndex == pi.playerIndex))
        {
            pi.transform.SetParent(transform); // Becomes child of the ParentConfigManager to perist across scenes
            playerConfigs.Add(new PlayerConfiguration(pi));
            playerCosmeticManagers.Add(pi.GetComponent<PlayerCosmeticManager>());
            pi.gameObject.name = $"PLAYER CONFIG [{pi.playerIndex}]";
        }
    }

    public void OnAllPlayersReady()
    {
        playerInputManager.DisableJoining();
    }

    private void OnReturnToMainMenu()
    {
        Destroy(this.gameObject);
    }
}

public class PlayerConfiguration
{
    public PlayerInput Input { get; set; }

    public int PlayerIndex { get; set; }
    public bool IsReady { get; set; }
    public bool IsDisabled { get; set; }

    public Color PlayerColor { get; set; }
    public string PlayerName { get; set; }

    public PlayerConfiguration(PlayerInput pi)
    {
        PlayerIndex = pi.playerIndex;
        Input = pi;
        PlayerName = $"Baggie_{pi.playerIndex}";
    }
}