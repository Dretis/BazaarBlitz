using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Scriptable Object-based event that passes some scriptable object as a payload.
/// </summary>
[CreateAssetMenu(fileName = "IntListItemListEventChannel", menuName = "Events/IntListItemList EventChannelSO")]
public class IntListItemListEventChannelSO : GenericEventChannelSO<List<int>,List<ItemStats>>
{

}
