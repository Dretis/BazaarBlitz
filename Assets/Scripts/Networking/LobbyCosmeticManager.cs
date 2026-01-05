using HeathenEngineering.SteamworksIntegration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyCosmeticManager : MonoBehaviour
{
    public static LobbyCosmeticManager instance;

    [Header("Lobby Info")]
    [SerializeField] private LobbyManager lobbyManager;
    private LobbyMemberData me;

    [Header("Color Palette Selection")]
    public int currentIndex = 0;
    public List<PlayerColorPalettePreset> colorPalettes = new List<PlayerColorPalettePreset>();

    private void Start()
    {
        if(instance != null)
        {
            Destroy(this);
        }

        instance = this;

        Debug.Log("guh wtf " + lobbyManager.Lobby);
    }

    public void IncrementColorPaletteSelection()
    {
        me = lobbyManager.Lobby.Me;

        currentIndex++;

        if (currentIndex > colorPalettes.Count - 1) currentIndex = 0;

        me["color palette"] = currentIndex.ToString();
    }

    public void DecrementColorPaletteSelection()
    {
        me = lobbyManager.Lobby.Me;

        currentIndex--;

        if (currentIndex < 0) currentIndex = colorPalettes.Count - 1;

        me["color palette"] = currentIndex.ToString();
    }
}
