using UnityEngine;

/// <summary>
/// A Scriptable Object-based event that passes some scriptable object as a payload.
/// </summary>
[CreateAssetMenu(fileName = "Vector2EventChannel", menuName = "Events/Vector2 EventChannelSO")]
public class Vector2EventChannelSO : GenericEventChannelSO<Vector2>
{

}
