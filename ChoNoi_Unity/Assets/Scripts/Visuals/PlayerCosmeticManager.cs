using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCosmeticManager : MonoBehaviour
{
    public string playerName;
    public Color playerColor;

    [SerializeField] private PlayerColorPalettePreset palettePreset;
    public PlayerColorPalettePreset PalettePreset { get { return palettePreset; } set { palettePreset = value; } }

    [Header("Color Palettes")]
    [SerializeField] private int baggiePaletteColorCount = 8;
    public List<Color> baggieColorPalette = new List<Color>();

    [Space]
    [SerializeField] private int boatPaletteColorCount = 8;
    public List<Color> boatColorPalette = new List<Color>();
    /*
    [Header("Combat Actions")]
    public List<Action> attackActions;
    public List<Action> defendActions;
    */

    #if UNITY_EDITOR
    private void OnValidate()
    {
        //Debug.Log("PlayerCosmeticManager | OnValidate");
        if (palettePreset is null) return;
        //Debug.Log("PlayerCosmeticManager | i am changing shit");

        var colorPal = new List<Color>(palettePreset.baggieColorPalette);
        var boatPal = new List<Color>(palettePreset.boatColorPalette);

        var baggiePaletteCount = baggieColorPalette.Count;
        baggieColorPalette.Clear();

        for (int i = 0; i < baggiePaletteColorCount; i++)
        {
            baggieColorPalette.Add(colorPal[i]);
        }

        var boatPaletteCount = boatColorPalette.Count;
        boatColorPalette.Clear();

        for (int i = 0; i < boatPaletteColorCount; i++)
        {
            boatColorPalette.Add(boatPal[i]);
        }

        playerColor = palettePreset.mainColor;
        playerName = palettePreset.presetName;
    }
    
    public void ManualValidate()
    {
        OnValidate();
    }
    #endif
}
