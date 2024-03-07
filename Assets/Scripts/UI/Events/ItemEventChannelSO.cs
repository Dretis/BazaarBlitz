using UnityEngine;

/// <summary>
/// A Scriptable Object-based event that passes an ItemStats scriptable object as a payload.
/// </summary>
[CreateAssetMenu(fileName = "ItemEventChannel", menuName = "Events/ItemEventChannelSO")]
public class ItemEventChannelSO : GenericEventChannelSO<ItemStats>
{

}
