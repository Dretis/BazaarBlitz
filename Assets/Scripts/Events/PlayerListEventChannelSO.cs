using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Scriptable Object-based event that passes some scriptable object as a payload.
/// </summary>
[CreateAssetMenu(fileName = "PlayerListEventChannel", menuName = "Events/PlayerList EventChannelSO")]
public class PlayerListEventChannelSO : GenericEventChannelSO<List<EntityPiece>>
{

}
