using UnityEngine;

/// <summary>
/// A Scriptable Object-based event that passes some scriptable object as a payload.
/// </summary>
[CreateAssetMenu(fileName = "EntityActionEventChannel", menuName = "Events/EntityAction EventChannelSO")]
public class EntityActionEventChannelSO : GenericEventChannelSO<EntityPiece, Action>
{

}
