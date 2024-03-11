using UnityEngine;

/// <summary>
/// A Scriptable Object-based event that passes an ItemStats scriptable object as a payload.
/// </summary>
[CreateAssetMenu(fileName = "ItemEventChannel", menuName = "Events/Item EventChannelSO")]
public class ItemEventChannelSO : GenericEventChannelSO<ItemStats>
{

}
