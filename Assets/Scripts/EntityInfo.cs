using UnityEngine;

[CreateAssetMenu]
public class EntityInfo : ScriptableObject
{
    public Sprite entityIcon;

    public string entitySpecies;

    public Color defaultColor;

    [TextArea(3, 10)]
    public string flavorText;
}
