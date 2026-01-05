using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using HeathenEngineering.SteamworksIntegration;
using UnityEngine;
using UnityEngine.InputSystem;

// Inherit from NetworkBehaviour instead of MonoBehaviour
public class OnlinePlayerInputController : NetworkBehaviour
{
    public static OnlinePlayerInputController Instance { get; private set; }
    [SerializeField] private PlayerInputController pic;
    private LobbyData lobby;

    private void Start()
    {

        Debug.Log($"OnlinePlayerInputController | OnStartNetwork().");
        Debug.Log($"OnlinePlayerInputController | HasAuthority = {HasAuthority}");
        Debug.Log($"OnlinePlayerInputController | IsHost = {IsHost}");
        Debug.Log($"OnlinePlayerInputController | IsHostStarted = {IsHostStarted}");
        Debug.Log($"OnlinePlayerInputController | IsHostInitialized = {IsHostInitialized}");
        Debug.Log($"OnlinePlayerInputController | IsServerStarted = {InstanceFinder.IsServerStarted}");
        Debug.Log($"OnlinePlayerInputController | IsServerOnlyStarted = {InstanceFinder.IsServerOnlyStarted}");

        if (HasAuthority && IsHost)
        {
            Debug.Log($"OPIC | I am the host and have authority! Network start!");
            //ConfigureStuff() lol
            ConfigureGameplayRules();
        }
    }

    private void ConfigureGameplayRules()
    {
        lobby = FindObjectOfType<LobbyManager>().Lobby;

    }

    private void OnFinishConfigureGameplayRules()
    {
        //if (IsValid)
            lobby.SetGameServer();
    }

    public override void OnStartClient()
    {
        if (!IsOwner) return;

        GetComponent<PlayerInput>().enabled = true;
        Instance = this;

        Debug.Log($"Hello gamers!");
    }
}