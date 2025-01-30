using UnityEngine;

/// <summary>
/// A Scriptable Object-based event that passes some scriptable object as a payload.
/// </summary>
[CreateAssetMenu(fileName = "WeaponTypeIntEventChannel", menuName = "Events/WeaponType Int EventChannelSO")]
public class WeaponTypeIntEventChannel : GenericEventChannelSO<Action.WeaponTypes, int>
{

}
