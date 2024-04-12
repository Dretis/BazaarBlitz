using UnityEngine;

/// <summary>
/// A Scriptable Object-based event that passes some scriptable object as a payload.
/// </summary>
[CreateAssetMenu(fileName = "EntityActionPhaseEventChannel", menuName = "Events/EntityActionPhase EventChannelSO")]
public class EntityActionPhaseEventChannelSO : GenericEventChannelSO<EntityPiece, Action.PhaseTypes>
{

}
