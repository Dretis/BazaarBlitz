using UnityEngine;

/// <summary>
/// A Scriptable Object-based event that passes some scriptable object as a payload.
/// </summary>
[CreateAssetMenu(fileName = "BooleanEventChannel", menuName = "Events/Bool EventChannelSO")]
public class BooleanEventChannelSO : GenericEventChannelSO<bool>
{

}
