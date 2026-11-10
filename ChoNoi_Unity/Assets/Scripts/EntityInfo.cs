using Core;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu]
public class EntityInfo : ScriptableObject
{
    public Sprite entityIcon;

    public string entitySpecies;
    public LocalizedString l_entityName; // kinda used for the Enemys on the local end

    public Color defaultColor;

    [TextArea(3, 10)]
    public string flavorText;
    [Space]

    //public int Id;
    //public RuntimeAnimatorController CombatAnimatorController;
    //public EntityData DefaultEntityData;

    public PlayerState DefaultEntityState;
}
