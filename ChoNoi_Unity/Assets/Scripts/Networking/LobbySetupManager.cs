using FishNet;
using FishNet.Managing;
using FishNet.Managing.Transporting;
using FishNet.Transporting.Multipass;
using HeathenEngineering.SteamworksIntegration;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbySetupManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI lobbyTitle;
    [SerializeField] private Button startGameButton;
    [SerializeField] private MenuSelectionHandler readyButton;

    [Header("Lobby Info")]
    [SerializeField] private LobbyManager lobbyManager;
    [SerializeField] private LobbyData currentLobby;
    //[SerializeField] private int id;
    [Space]
    [Header("User Lobby Setup")]
    [SerializeField] private LobbyUserPanel lobbyUserPanelPrefab;
    [SerializeField] private Transform lobbyUserHolder;

    [Header("Lobby Searching Elements")]
    [SerializeField] private LobbySearchPanel lobbySearchPanelPrefab;
    [SerializeField] private Transform lobbySearchHolder;

    [Header("Test Elements")]
    [SerializeField] private TextMeshProUGUI testGameStartText;

    private Dictionary<UserData, LobbyUserPanel> _lobbyUserPanels = new Dictionary<UserData, LobbyUserPanel>();
    private Dictionary<LobbyData, LobbySearchPanel> _lobbySearchPanels = new Dictionary<LobbyData, LobbySearchPanel>();

    [SerializeField] private NetworkManager _networkManager;

    // Start is called before the first frame update
    private void Awake()
    {
        HeathenEngineering.SteamworksIntegration.API.Overlay.Client.EventGameLobbyJoinRequested.AddListener(OverlayJoinButton);

        _networkManager = InstanceFinder.NetworkManager;//FindAnyObjectByType<NetworkManager>(); // tempfind
    }

    public void OnLobbyCreated(LobbyData lobbyData)
    {
        Debug.Log($"Lobby created");

        ClearCards();
        //lobbyData.Name = UserData.Me.Name + "'s Lobby";
        lobbyData.Name = lobbyData.Me.user.Name + "'s Lobby";
        lobbyTitle.text = lobbyData.Name;

        if (lobbyData.AllPlayersReady && lobbyData.IsOwner)
        {
            startGameButton.gameObject.SetActive(true);
        }
        else
        {
            startGameButton.gameObject.SetActive(false);
        }

        Debug.Log($"!!! Game Board (default) = {lobbyData["game board"]}");
        lobbyData["game board"] = "0";
        Debug.Log($"!!! Game Board (after) = {lobbyData["game board"]}");

        Debug.Log($"!!! GameplayTest.GameBoard (0) = {(GameplayTest.GameBoard)0}");
        Debug.Log($"!!! GameplayTest.GameBoard (1) = {(GameplayTest.GameBoard)1}");
        Debug.Log($"!!! GameplayTest.GameBoard (2) = {(GameplayTest.GameBoard)2}");

        var me = lobbyData.Me;
        me["color palette"] = LobbyCosmeticManager.instance.currentIndex.ToString();

        SetupCard(lobbyData.Me);
    }

    public void OnLobbyJoined(LobbyData lobbyData)
    {
        Debug.Log($"You joined a lobby");
        ClearCards();

        var me = lobbyData.Me;
        me["color palette"] = LobbyCosmeticManager.instance.currentIndex.ToString();

        lobbyTitle.text = lobbyData.Name;

        ReportLobbyDetails(lobbyData);

        foreach (var member in lobbyData.Members)
        {
            SetupCard(member);
            if (member.IsReady)
            {
                _lobbyUserPanels[member.user].ShowReadyText();
            }
        }
    }

    public void OnLobbyLeave()
    {
        ClearCards();
    }

    private void OverlayJoinButton(LobbyData lobbyData, UserData user)
    {
        lobbyManager.Join(lobbyData);
    }

    public void OnUserJoin(UserData userData)
    {
        Debug.Log($"{userData.Name} joined the lobby");

        var lobby = lobbyManager.Lobby;

        ReportLobbyDetails(lobby);

        if (lobby.AllPlayersReady && lobby.IsOwner)
        {
            startGameButton.gameObject.SetActive(true);
        }
        else
        {
            startGameButton.gameObject.SetActive(false);
        }

        if (lobby.GetMember(userData, out LobbyMemberData memberData))
        {
            SetupCard(memberData);
            //The user is a member of the lobby memberData is valid
        }
        else
        {
            //The user is not a member of this lobby, member data is not valid
            Debug.LogError($"??? {userData.Name} is not a member of this lobby?");
        }
    }

    public void OnUserLeft(UserLobbyLeaveData userLeaveData)
    {
        var lobby = lobbyManager.Lobby;

        if (!_lobbyUserPanels.TryGetValue(userLeaveData.user, out LobbyUserPanel panel))
        {
                Debug.LogError($"Tried to remove user {userLeaveData.user.Name} that doesn't exist!");
                return;
        }
        
        Destroy(panel.gameObject);
        _lobbyUserPanels.Remove(userLeaveData.user);
        Debug.Log($"{userLeaveData.user.Name} left");
    }

    public void OnLobbySearch(LobbyData[] lobbies)
    {
        ClearSearchedLobbies();

        if (lobbies.Length == 0)
            Debug.Log($"no lobbies found (the game is dead lol)");
        else
        {
            Debug.Log($"Found {lobbies.Count()} lobbies in Cho Noi (YOOO!!!)");
            // 'Show' and 'Go To' the Lobby Search Results UI Screen/Canvas here
            foreach (LobbyData lobby in lobbies)
            {
                Debug.Log($"Lobby Name: {lobby.Name} | Owner: {lobby.Owner.user.Name} | Max Members: {lobby.MaxMembers} | SteamID: {lobby.SteamId} | ");
                // Instantiate the prefab that shows the lobby info as a list/grid group
                SetupSearchedLobby(lobby);
                //var lobbySearchPanel = Instantiate(lobbySearchPanelPrefab, lobbyUserHolder);
                //lobbySearchPanel.Initialize(lobbyData);
            }
        }
    }

    public void OnMetadataUpdated(LobbyDataUpdateEventData dataUpdated)
    {
        
        //Debug.Log($"The lobby's metadata changed!");
        //Debug.Log($"eventData.lobby = {dataUpdated.lobby}");
        //Debug.Log($"Was it a member that changed? = {dataUpdated.member.HasValue}");

        var lobby = lobbyManager.Lobby;

        if (dataUpdated.lobby == lobby)
        {
            if (!dataUpdated.member.HasValue)
            {
                //It was lobby data that was updated
                if (lobby.AllPlayersReady && lobby.IsOwner)
                {
                    Debug.Log($"You can now start. :)");
                    startGameButton.gameObject.SetActive(true);
                }
                else
                {
                    Debug.Log($"You can no longer start. :(");
                    startGameButton.gameObject.SetActive(false);
                }

                _lobbyUserPanels[lobby.Owner.user].SetOwnerIndicator(lobby.Owner.IsOwner);
            }
            else
            {
                //It was this member that updated (dataUpdated.member.Value)
                var updatedMember = dataUpdated.member.Value;

                SetReadyStatusForMember(updatedMember);
                _lobbyUserPanels[updatedMember.user].SetOwnerIndicator(updatedMember.IsOwner);
                _lobbyUserPanels[updatedMember.user].SetBaggieColor(updatedMember["color palette"]);
            }
        }
    }
    #region Start Game
    public void OnSessionConnectionUpdated(LobbyGameServer lobbyGameServer)
    {
        // Handle Game Server
        Debug.Log($"Session Connection Updated!");
        Debug.Log($"lobbyGameServer | {lobbyGameServer}");

        Debug.Log($"lobbyGameServer CSTEAMID = {lobbyGameServer.id}");
        
        testGameStartText.text = "Game should start now \r\ngo into network scene next :)";


        Debug.Log($"\n-- [NETWORK MANAGER VARIABLES] --");
        Debug.Log($"IsHost = {_networkManager.IsHost}");
        Debug.Log($"IsHostStarted = {_networkManager.IsHostStarted}");

        Debug.Log($"IsClient = {_networkManager.IsClient}");
        Debug.Log($"IsClientOnly = {_networkManager.IsClientOnly}");
        Debug.Log($"IsClientOnlyStarted = {_networkManager.IsClientOnlyStarted}");


        if (_networkManager.IsClientOnlyStarted)
        {
            Debug.Log("I am a client, setting up the network connection");
            BootstrapLogic.instance.SetClientConnection(lobbyGameServer.id);
            //FishySteamworks.FishySteamworks fishyTransport = _networkManager.GetComponent<FishySteamworks.FishySteamworks>();
            //fishyTransport.SetClientAddress(lobbyGameServer.id.ToString());
            //fishyTransport.StartConnection(false);
            //_networkManager.ClientManager.StartConnection();
        }
        else
        {
            Debug.Log("Clients have been notified of Server!");
            //BootstrapLogic.instance.EnterOnlineGame(lobbyManager.Lobby["game board"]);
            //_networkManager.ServerManager.GetAuthenticator();
        }
    }

    // Button function to 'start game'
    public void StartOnlineGame()
    {
        Debug.Log($"Game is starting!!");

        // Logic that takes all members in the lobby and gathers them into a network scene
        // Make sure the lobby owner is the host/listen server
        // Assign a Network Object for each player that takes their inputs during gameplay
        // Once this is all setup correctly, send them into the real game board to play Cho Noi
        var lobby = lobbyManager.Lobby;

        // probably add lobby.Full here too?
        if (lobby.AllPlayersNotReady) return;

        lobby.SetJoinable(false);

        //var n = NetworkManager.Instances;
        BootstrapLogic.instance.SetHostConnection();
        
        //lobby.SetGameServer();
    }


    public void SetUpServerIsDone()
    {
        var lobby = lobbyManager.Lobby;
        lobby.SetGameServer();
    }
    #endregion

    public void ReportLobbyDetails(LobbyData lobbyData)
    {
        if (lobbyData == null) return;

        var owner = lobbyData.Owner;
        Debug.Log($"Lobby Name: {lobbyData.Name}");
        Debug.Log($"Game Board = {lobbyData["game board"]}");

        Debug.Log($"List of lobby members:");
        foreach (var member in lobbyManager.Lobby.Members)
        {
            if (member.IsOwner)
                Debug.Log($"[OWNER] {member.user.Name} | Ready = {member.IsReady}");
            else
                Debug.Log($"{member.user.Name} | Ready = {member.IsReady}");
        }
    }

    public void SetReadyStatusForMember(LobbyMemberData member)
    {
        if (member.IsReady)
        {
            ReadyOnCard(member.user);
        }
        else
        {
            UnreadyOnCard(member.user);
        }
    }

    public void TestMetaDataShit(UserData userData, LobbyMemberData memberData)
    {
        var lobby = lobbyManager.Lobby;
        lobby.SetMemberMetadata("a","09");
        var a = lobby.GetMetadata();
        Debug.Log($"Lobby Metadata: {a}");
        Debug.Log($"Lobby Metadata: {lobby.GetMemberMetadata("a")}");
        Debug.Log($"Lobby Metadata: {lobby.GetMemberMetadata(userData,"a")}");
        //lobby[UserData.Me].IsOwner;
        memberData["a"] = "a";
    }

    public void SetReadyStatus()
    {
        var lobby = lobbyManager.Lobby;
        if (!lobby.IsReady)
        {
            PlayerIsReady();
            readyButton.menuBumperText.text = "Unready";
            readyButton.helpInfo = "Let others know you are no longer ready.";
        }
        else
        {
            PlayerIsUnready();
            readyButton.menuBumperText.text = "Ready?";
            readyButton.helpInfo = "Let others know you are ready to play!";
        }
    }

    public void ResetReadyButton()
    {
        readyButton.menuBumperText.text = "Ready?";
        readyButton.helpInfo = "Let others know you are ready to play!";
    }

    public void PlayerIsReady()
    {
        if (lobbyManager.Lobby != null)
        {
            var lobby = lobbyManager.Lobby;
            lobby.IsReady = true;
        }
    }

    public void PlayerIsUnready()
    {
        if (lobbyManager.Lobby != null)
        {
            var lobby = lobbyManager.Lobby;
            lobby.IsReady = false;

            //UnreadyOnCard(lobby.Me.user);
        }
    }

    public void ReadyOnCard(UserData userData)
    {
        var lobby = lobbyManager.Lobby;

        //Debug.Log($"{userData.Name} has ready'd up.");
        _lobbyUserPanels[userData].ShowReadyText();
    }

    public void UnreadyOnCard(UserData userData)
    {
        var lobby = lobbyManager.Lobby;

        //Debug.Log($"{userData.Name} has unready'd up. man.");
        _lobbyUserPanels[userData].HideReadyText();
    }

    public void HideGameStartText()
    {
        testGameStartText.text = "";
    }

    private void ClearCards()
    {
        foreach (Transform child in lobbyUserHolder)
        {
            Destroy(child.gameObject);
        }

        _lobbyUserPanels.Clear();
    }

    //private void SetupCard(UserData userData)
    private void SetupCard(LobbyMemberData memberData)
    {
        var userPanel = Instantiate(lobbyUserPanelPrefab, lobbyUserHolder);
        userPanel.Initialize(memberData);

        //if() setbaggiecolor her for pre-existing lobby members

        _lobbyUserPanels.TryAdd(memberData.user, userPanel);
        /* [Same thing as above]
        if (_lobbyUserPanels.ContainsKey(userData))
        {
            if (_lobbyUserPanels[userData])
                Destroy(_lobbyUserPanels[userData]);
            _lobbyUserPanels.Remove(userData);
        }

        _lobbyUserPanels.Add(userData, userPanel);
        */
    }

    private void ClearSearchedLobbies()
    {
        foreach (Transform child in lobbySearchHolder)
        {
            Destroy(child.gameObject);
        }

        _lobbyUserPanels.Clear();
    }

    private void SetupSearchedLobby(LobbyData lobby)
    {
        var lobbySearchPanel = Instantiate(lobbySearchPanelPrefab, lobbySearchHolder);
        lobbySearchPanel.Initialize(lobby);

        _lobbySearchPanels.TryAdd(lobby, lobbySearchPanel);
    }
}
