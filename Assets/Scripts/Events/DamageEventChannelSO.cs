using UnityEngine;

/// <summary>
/// A Scriptable Object-based event that passes some scriptable object as a payload.
/// </summary>
[CreateAssetMenu(fileName = "DamageEventChannel", menuName = "Events/Damage EventChannelSO")]
public class DamageEventChannelSO : GenericEventChannelSO<EntityPiece, float>
{

}
