using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Scriptable Object-based event that passes an ItemStats scriptable object as a payload.
/// </summary>
[CreateAssetMenu(fileName = "ItemEventChannel", menuName = "Events/ItemList EventChannelSO")]
public class ItemListEventChannelSO : GenericEventChannelSO<List<ItemStats>>
{
    
}
