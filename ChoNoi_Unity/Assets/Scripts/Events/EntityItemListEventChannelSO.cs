using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Scriptable Object-based event that passes some scriptable object as a payload.
/// </summary>
[CreateAssetMenu(fileName = "EntityItemListEventChannel", menuName = "Events/EntityItemList EventChannelSO")]
public class EntityItemListEventChannelSO : GenericEventChannelSO<EntityPiece, List<ItemStats>>
{

}
