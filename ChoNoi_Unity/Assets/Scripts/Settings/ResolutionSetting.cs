using UnityEngine;

[System.Serializable]
public struct ResolutionSetting
{
    public int width;   // Horiz res
    public int height;  // Vertical res

    // Contructor that initializes resolution property values
    public ResolutionSetting(int width, int height)
    {
        this.width = width;
        this.height = height;
    }

    // Represents the width x height in string format
    public override string ToString()
    {
        return $"{width}x{height}";
    }
}
