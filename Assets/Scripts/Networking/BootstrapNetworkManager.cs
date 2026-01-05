using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using HeathenEngineering.SteamworksIntegration;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BootstrapNetworkManager : NetworkBehaviour
{
    private static BootstrapNetworkManager instance;

    [SerializeField] private GameplayRules gameplayRules;
    private LobbyData lobby;
    
    private void Awake() => instance = this;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        Debug.Log($"BNM | OnStartNetwork().");
        Debug.Log($"BNM | HasAuthority = {HasAuthority}");
        Debug.Log($"BNM | IsHost = {IsHost}");
        Debug.Log($"BNM | IsHostStarted = {IsHostStarted}");
        Debug.Log($"BNM | IsHostInitialized = {IsHostInitialized}");
        Debug.Log($"BNM | IsServerStarted = {InstanceFinder.IsServerStarted}");
        Debug.Log($"BNM | IsServerOnlyStarted = {InstanceFinder.IsServerOnlyStarted}");

        Debug.Log($"BNM | base.Owner.IsLocalClient || (base.IsServerInitialized && !Owner.IsValid) = {base.Owner.IsLocalClient || (base.IsServerInitialized && !Owner.IsValid)}");

        if (base.Owner.IsLocalClient || (base.IsServerInitialized && !Owner.IsValid))
        {
            Debug.Log($"BNM | I have authority! Configure gameplay rules next!!");
            ConfigureGameplayRules();
        }

        /*
        if (HasAuthority && IsHost)
        {
            //ConfigureStuff() lol
            Debug.Log($"BNM | I am the host and have authority! Network start!");
            ConfigureGameplayRules();
        }
        */
    }
    
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
}
