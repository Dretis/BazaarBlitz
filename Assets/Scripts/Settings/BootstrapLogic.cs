using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using HeathenEngineering.SteamworksIntegration;
using AppClient = HeathenEngineering.SteamworksIntegration.API.App.Client;
using FishNet.Managing;
using Steamworks;

public class BootstrapLogic : MonoBehaviour
{
    private static BootstrapLogic instance;

    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private FishySteamworks.FishySteamworks fishySteamworks;

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
