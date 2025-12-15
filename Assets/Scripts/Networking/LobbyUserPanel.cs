using System.Collections;
using System.Collections.Generic;
using Febucci.UI.Core;
using HeathenEngineering.SteamworksIntegration;
using HeathenEngineering.SteamworksIntegration.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class LobbyUserPanel : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private RawImage avatarImage;
    [SerializeField] private Image hostIndicator;

    [SerializeField] private TextMeshProUGUI userNameText;
    [SerializeField] private TypewriterCore userReadyTypewriter;

    [Header("Player Elements")]
    [SerializeField] private Color playerColor;
    [SerializeField] private TextMeshProUGUI emptyslotText;
    //[SerializeField] private Image baggieVisual;
    [SerializeField] private PlayerPaletteLoader baggiePaletteLoader;
    [Space]
    [SerializeField] private PlayerColorPalettePreset colorPalettePreset;

    //public void Initialize(UserData userData)
    public void Initialize(LobbyMemberData memberData)
    {
        var user = memberData.user;
        user.LoadAvatar(SetAvatar);

        SetOwnerIndicator(memberData.IsOwner);

        if (user.IsMe)
            userNameText.text = $"<color=#69CDFF>{user.Name}";
        else
            userNameText.text = user.Name;

        if (!memberData["color palette"].Equals(""))
        {
            Debug.Log($"{user.Name} | existing memberData[\"color palette\"] = {memberData["color palette"]}");
            SetBaggieColor(memberData["color palette"]);
        }
    }

    public void InitializeEmpty()
    {
        userNameText.text = "";
        emptyslotText.text = "NO\nSLOT";
        //baggieVisual.enabled = false;
    }

    public void SetOwnerIndicator(bool isOwner)
    {
        if (isOwner)
        {
            hostIndicator.enabled = true;
        }
        else
            hostIndicator.enabled = false;
    }

    public void SetBaggieColor(string colorPalette)
    {
        int index = int.Parse(colorPalette);
        colorPalettePreset = LobbyCosmeticManager.instance.colorPalettes[index];

        //baggieVisual.color = colorPalettePreset.mainColor;
        playerColor = colorPalettePreset.mainColor;

        baggiePaletteLoader.SetInspectorPalette(colorPalettePreset.baggieColorPalette);
    }

    public void ShowReadyText()
    {
        userReadyTypewriter.ShowText("Ready!");
        //userReadyText.text = "Ready!";
    }

    public void HideReadyText()
    {
        userReadyTypewriter.ShowText("");
        //userReadyText.text = "";
    }

    private void SetAvatar(Texture2D userImage)
    {
        avatarImage.texture = userImage;
    }
}
