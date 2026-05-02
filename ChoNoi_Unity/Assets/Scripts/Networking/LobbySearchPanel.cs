using System.Collections;
using System.Collections.Generic;
using Febucci.UI.Core;
using HeathenEngineering.SteamworksIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class LobbySearchPanel : MonoBehaviour
{
    [SerializeField] private LobbyData lobby;

    [Header("UI Elements")]
    [SerializeField] private RawImage avatarImage; // owner image specifically
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI membersText;
    [SerializeField] private TextMeshProUGUI selectedBoardText;
    //[SerializeField] private TypewriterCore userReadyTypewriter;
    //[SerializeField] private TextMeshProUGUI userReadyText;

    public void Initialize(LobbyData lobbyData)
    {
        lobby = lobbyData;
        lobby.Owner.user.LoadAvatar(SetAvatar);
        lobbyNameText.text = lobbyData.Name;

        membersText.text = $"{lobbyData.MemberCount}/{lobbyData.MaxMembers}";
        if (lobby.Full)
        {
            membersText.text += " FULL";
            membersText.color = Color.red;
        }

        if (!lobby["game board"].Equals(""))
            selectedBoardText.text = lobby["game board"];
        else
            selectedBoardText.text = $"Selected_Board";
    }

    public void TryJoinLobby()
    {
        // Temporary
        var lobbyManager = FindAnyObjectByType<LobbyManager>();
        if (lobbyManager != null)
            lobbyManager.Join(lobby);
    }

    private void SetAvatar(Texture2D userImage)
    {
        avatarImage.texture = userImage;
    }
}
