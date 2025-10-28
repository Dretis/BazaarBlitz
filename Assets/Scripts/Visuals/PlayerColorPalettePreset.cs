using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PlayerColorPalettePreset : ScriptableObject
{
    public string presetName;

    public Color mainColor = Color.white;

    public List<Color> baggieColorPalette = new List<Color>();

    public List<Color> boatColorPalette = new List<Color>();

    private void OnValidate()
    {
        var baggiePaletteCount = baggieColorPalette.Count;

        for (int i = baggiePaletteCount; i < 8; i++)
        {
            baggieColorPalette.Add(Color.white);
        }

        var boatPaletteCount = boatColorPalette.Count;

        for (int i = boatPaletteCount; i < 8; i++)
        {
            boatColorPalette.Add(Color.grey);
        }
    }
}
