using UnityEngine;

/// <summary>
/// A Scriptable Object-based event that passes some scriptable object as a payload.
/// </summary>
[CreateAssetMenu(fileName = "PlayerFloatActionTypeEventChannel", menuName = "Events/PlayerFloatActionType EventChannelSO")]
public class PlayerFloatActionTypeEventChannelSO : GenericEventChannelSO<EntityPiece, float, Action.WeaponTypes>
{

}
