using HeathenEngineering.SteamworksIntegration;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BootstrapNetworkManager : MonoBehaviour
{
    private static BootstrapNetworkManager instance;

    [SerializeField] private GameplayRules gameplayRules;
    private LobbyData lobby;
    
    private void Awake() => instance = this;
    
    private void ConfigureGameplayRules()
    {
        lobby = FindObjectOfType<LobbyManager>().Lobby;
        gameplayRules = new GameplayRules(lobby);
    }

    private void OnFinishConfigureGameplayRules()
    {
        //if (IsValid)
        lobby.SetGameServer();
    }
    /*
    public static void ChangeNetworkScene(string sceneName, string[] scenesToClose)
    {
        instance.CloseScenes(scenesToClose);

        SceneLoadData sld = new SceneLoadData(sceneName);
        NetworkConnection[] conns = instance.ServerManager.Clients.Values.ToArray();
        instance.SceneManager.LoadConnectionScenes(conns, sld);
    }

    [ServerRpc(RequireOwnership =false)]
    void CloseScenes(string[] scenesToClose)
    {
        CloseScenesObserver(scenesToClose);
    }

    [ObserversRpc]
    void CloseScenesObserver(string[] scenesToClose)
    {
        foreach (var sceneName in scenesToClose)
        {
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);
        }
    }
    */
}
