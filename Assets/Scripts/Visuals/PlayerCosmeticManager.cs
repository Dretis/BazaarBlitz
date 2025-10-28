using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCosmeticManager : MonoBehaviour
{
    public string playerName;
    public Color playerColor;

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
    /*
    private void OnValidate()
    {
        var baggiePaletteCount = baggieColorPalette.Count;

        for (int i = baggiePaletteCount; i < baggiePaletteColorCount; i++)
        {
           baggieColorPalette.Add(Color.white);
        }

        var boatPaletteCount = boatColorPalette.Count;

        for (int i = boatPaletteCount; i < boatPaletteColorCount; i++)
        {
            boatColorPalette.Add(Color.grey);
        }
    }
    */
}
