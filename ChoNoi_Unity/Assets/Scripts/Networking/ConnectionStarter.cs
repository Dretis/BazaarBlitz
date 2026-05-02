using FishNet;
using FishNet.Transporting;
using FishNet.Transporting.Tugboat;
using HeathenEngineering.SteamworksIntegration;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConnectionStarter : MonoBehaviour
{
    //private static BootstrapLogic instance;

    //[SerializeField] private NetworkManager networkManager;
    [SerializeField] private TMP_InputField connectionInput;
    [SerializeField] private FishySteamworks.FishySteamworks _fishySteamworks; // transport
    [SerializeField] private Tugboat _tugboat; // transport

    private string _hostHex;

    //protected Callback<LobbyCreated_t> LobbyCreated;
    //protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    //protected Callback<LobbyEnter_t> LobbyEntered;

    //private void Awake() => instance = this;
    private void OnEnable()
    {
        InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
    }

    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        if(args.ConnectionState == LocalConnectionState.Stopping)
        {
            //UnityEditor.EditorApplication.isPlaying = false;
        }
    }

    private void Start()
    {
        if(TryGetComponent(out Tugboat t))
        {
            _tugboat = t;
        }
        else
        {
            Debug.LogError("Can't find tugboat!");
            return;
        }

        //LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        //JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        //LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void StartHost()
    {
        var user = UserData.Get();
        _hostHex = user.ToString();

        _fishySteamworks.StartConnection(true);
        _fishySteamworks.StartConnection(false);
    }

    public void StartConnection()
    {
        _hostHex = connectionInput.ToString();
        var hostUser = UserData.Get(_hostHex);

        if (!hostUser.IsValid)
        {
            Debug.LogError("HostUser is not valid.");
            return;
        }

        _fishySteamworks.SetClientAddress(hostUser.id.ToString()); // we need full Steam ID
        _fishySteamworks.StartConnection(false); // not starting as a server, only a client
    }
}
