using UnityEngine;

/// <summary>
/// A Scriptable Object-based event that passes some scriptable object as a payload.
/// </summary>
[CreateAssetMenu(fileName = "EntityItemEventChannel", menuName = "Events/EntityItem EventChannelSO")]
public class EntityItemEventChannelSO : GenericEventChannelSO<EntityPiece, ItemStats>
{

}
