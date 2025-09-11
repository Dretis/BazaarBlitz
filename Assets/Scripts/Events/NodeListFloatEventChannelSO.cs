using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Scriptable Object-based event that passes some scriptable object as a payload.
/// </summary>
[CreateAssetMenu(fileName = "NodeListFloatEventChannel", menuName = "Events/NodeList Float EventChannelSO")]
public class NodeListFloatEventChannelSO : GenericEventChannelSO<List<MapNode>, float>
{

}
