using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntityBaseStats
{
    public enum DieTypes
    {
        Strength,
        Dex,
        Int,
        Count
    }

    public EntityBaseStats()
    {
        for (int i = 0; i < dieConfigs.Length; i++)
        {
            dieConfigs[i] = new DieConfig();
        }
    }

    public DieConfig[] dieConfigs = new DieConfig[(int)DieTypes.Count];
}

[System.Serializable]
public class DieConfig
{
    [SerializeField]
    public int[] dieFaces = new int[6];

    public int this[int faceIndex]
    {
        get => GetFaceValue(faceIndex);
        set => SetFace(faceIndex, value);
    }

    public IEnumerable<int> GetAllFaceValues()
    {
        return dieFaces;
    }

    public void SetFace(int faceIndex, int value)
    {
        if (faceIndex < 0 || faceIndex > 5)
        {
            Debug.LogError("Tried to set out of bounds die face");
            return;
        }

        dieFaces[faceIndex] = value;
    }

    public int GetFaceValue(int faceIndex)
    {
        if (faceIndex < 0 || faceIndex > 5)
        {
            Debug.LogError("Tried to set out of bounds die face");
            return -1;
        }

        return dieFaces[faceIndex];
    }
}
