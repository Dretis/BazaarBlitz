using UnityEngine;

/// <summary>
/// A Scriptable Object-based event that passes some scriptable object as a payload.
/// </summary>
[CreateAssetMenu(fileName = "PlayerEventChannel", menuName = "Events/Player EventChannelSO")]
public class PlayerEventChannelSO : GenericEventChannelSO<PlayerData>
{

}
