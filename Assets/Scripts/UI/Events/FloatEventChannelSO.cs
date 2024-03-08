using UnityEngine;

/// <summary>
/// A Scriptable Object-based event that passes Float scriptable object as a payload.
/// </summary>
[CreateAssetMenu(fileName = "FloatEventChannel", menuName = "Events/Float EventChannelSO")]
public class FloatEventChannelOS : GenericEventChannelSO<float>
{

}
