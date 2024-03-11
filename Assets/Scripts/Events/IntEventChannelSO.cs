using UnityEngine;

/// <summary>
/// A Scriptable Object-based event that passes an int scriptable object as a payload.
/// </summary>
[CreateAssetMenu(fileName = "ItemEventChannel", menuName = "Events/Int EventChannelSO")]
public class IntEventChannelSO : GenericEventChannelSO<int>
{

}
