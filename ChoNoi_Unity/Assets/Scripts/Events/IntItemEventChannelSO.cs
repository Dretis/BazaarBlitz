using UnityEngine;

/// <summary>
/// A Scriptable Object-based event that passes some scriptable object as a payload.
/// </summary>
[CreateAssetMenu(fileName = "IntItemEventChannel", menuName = "Events/IntItem EventChannelSO")]
public class IntItemEventChannelSO : GenericEventChannelSO<int, ItemStats>
{

}
