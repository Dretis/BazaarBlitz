using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using HeathenEngineering.SteamworksIntegration;
using AppClient = HeathenEngineering.SteamworksIntegration.API.App.Client;
using Steamworks;

public class BootstrapLogic : MonoBehaviour
{
    public static BootstrapLogic instance;

    //[SerializeField] private NetworkManager _networkManager;
    //[SerializeField] private FishySteamworks.FishySteamworks _fishySteamworks;

    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    protected Callback<LobbyEnter_t> LobbyEntered;

    private void Awake() => instance = this;

    private void Start()
    {
        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

        StartCoroutine(Validate());
    }

    private IEnumerator Validate()
    {
        yield return null;

        Debug.Log($"Waiting to initiaize SteamSettings...");

        yield return new WaitUntil(() => SteamSettings.Initialized);

        Debug.Log($"Steam API initialized as App '{AppClient.Id.ToString()}' | Starting scene load...");

        GoToScene("MainMenu");
        //var operation = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        //operation.allowSceneActivation = true;

    }
    public static void GoToScene(string sceneName)
    {
        var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        operation.allowSceneActivation = true;
    }

    public static void GoToSceneUnload(string sceneName)
    {

        var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        operation.allowSceneActivation = true;
    }

    public static void CreateLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 4);
    }

    public void SetHostConnection()
    {
        //Multipass mp = _networkManager.TransportManager.GetTransport<Multipass>();

        //mp.SetClientTransport<FishySteamworks.FishySteamworks>();
        //mp.SetClientAddress(SteamUser.GetSteamID().ToString());

        //mp.StartConnection(true, 1);
        //mp.StartConnection(false,1);
        //Debug.Log($"mp.GetClientAddress = {mp.GetClientAddress()}");

        //_networkManager.ServerManager.StartConnection(((ushort)lobby.GameServer.id));
        //_networkManager.ClientManager.StartConnection();

        //Debug.Log($"IsClientOnlyStarted = {_networkManager.ServerManager.}");
    }

    public void SetClientConnection(CSteamID steamID)
    {
        Debug.Log($"I am just a client in SetClientConnection! ");
        //Multipass mp = _networkManager.TransportManager.GetTransport<Multipass>();

        //mp.SetClientTransport<FishySteamworks.FishySteamworks>();
        //mp.SetClientAddress(steamID.ToString());

        //mp.StartConnection(false);
        //if(mp.conn)
        Debug.Log($"SetClientConnection | steamID = {steamID.ToString()}");
        //Debug.Log($"mp.GetClientAddress = {mp.GetClientAddress()}");
    }

    public void EnterOnlineGame(string selectedBoard)
    {
        string[] scenesToClose = new string[] { "MainMenu" };

        var boardSceneName = "overworld 0";
        switch (selectedBoard)
        {
            case "central market":
                boardSceneName = "overworld 2";
                break;
            case "train street":
                boardSceneName = "overworld 4";
                break;
            default:
                boardSceneName = "overworld 2";
                break;
        }

        //BootstrapNetworkManager.ChangeNetworkScene(boardSceneName, scenesToClose);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        Debug.Log("Starting lobby creation... | " + callback.m_eResult.ToString());
    }

    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {

    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {

    }
}
