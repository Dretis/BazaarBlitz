using UnityEngine;

/// <summary>
/// A Scriptable Object-based event that passes some scriptable object as a payload.
/// </summary>
[CreateAssetMenu(fileName = "NodeEventChannel", menuName = "Events/Node EventChannelSO")]
public class NodeEventChannelSO : GenericEventChannelSO<MapNode>
{

}
