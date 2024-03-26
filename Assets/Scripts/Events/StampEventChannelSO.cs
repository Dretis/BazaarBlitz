using UnityEngine;

/// <summary>
/// A Scriptable Object-based event that passes some scriptable object as a payload.
/// </summary>
[CreateAssetMenu(fileName = "StampTypeEventChannel", menuName = "Events/StampType EventChannelSO")]
public class StampEventChannelSO : GenericEventChannelSO<Stamp.StampType>
{

}
