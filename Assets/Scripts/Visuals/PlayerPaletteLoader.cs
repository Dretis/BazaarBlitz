using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerPaletteLoader : MonoBehaviour
{
    [SerializeField]
    private Renderer ren;

    [SerializeField]
    private int paletteColorCount;

    [Header("Inspector Palette"), SerializeField]
    private bool useInspectorPalette;

    [SerializeField]
    private List<Color> inspectorPalette;

    private int paletteTexId = Shader.PropertyToID("_Palette");

    private Texture2D runtimePaletteTex;

    private MaterialPropertyBlock propBlock;

    private void Awake()
    {
        InitializePalette();

        if(useInspectorPalette)
            UpdatePaletteColors(inspectorPalette);
    }

    private void OnValidate()
    {
        if (useInspectorPalette)
        {
            var paletteCount = inspectorPalette.Count;

            for (int i = paletteCount; i < paletteColorCount; i++)
            {
                inspectorPalette.Add(Color.white);
            }

            UpdatePaletteColors(inspectorPalette);
        }
           
    }

    private void InitializePalette()
    {
        // Create Palette tex
        runtimePaletteTex = new Texture2D(paletteColorCount, 1);
        runtimePaletteTex.filterMode = FilterMode.Point;

        propBlock = new MaterialPropertyBlock();
    }

    public void UpdatePaletteColors(IEnumerable<Color> colors)
    {
        if(runtimePaletteTex == null)
        {
            return;
        }

        var paletteArray = colors.ToArray();
        runtimePaletteTex.SetPixels(0, 0, paletteColorCount, 1, paletteArray);
        runtimePaletteTex.Apply();

        ren.GetPropertyBlock(propBlock);

        propBlock.SetTexture(paletteTexId, runtimePaletteTex);

        ren.SetPropertyBlock(propBlock);
    }
}
