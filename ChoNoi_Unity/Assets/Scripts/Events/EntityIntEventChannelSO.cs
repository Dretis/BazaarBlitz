using UnityEngine;

/// <summary>
/// A Scriptable Object-based event that passes some scriptable object as a payload.
/// </summary>
[CreateAssetMenu(fileName = "EntityIntEventChannel", menuName = "Events/EntityInt EventChannelSO")]
public class EntityIntEventChannelSO : GenericEventChannelSO<EntityPiece, int>
{

}
