using UnityEditor;
using UnityEngine;

[CreateAssetMenu]
public class EntityInfo : ScriptableObject
{
    public Sprite entityIcon;

    public string entitySpecies;

    public Color defaultColor;

    [Header("Base Stats")]
    public int baseHealth;
    public EntityBaseStats entityBaseStats = new();

    public DieConfig StrDie => entityBaseStats.dieConfigs[(int)EntityBaseStats.DieTypes.Strength];
    public DieConfig DexDie => entityBaseStats.dieConfigs[(int)EntityBaseStats.DieTypes.Dex];
    public DieConfig IntDie => entityBaseStats.dieConfigs[(int)EntityBaseStats.DieTypes.Int];
    public DieConfig SpdDie => entityBaseStats.dieConfigs[(int)EntityBaseStats.DieTypes.Speed];

    [Space]

    [TextArea(3, 10)]
    public string flavorText;
}
