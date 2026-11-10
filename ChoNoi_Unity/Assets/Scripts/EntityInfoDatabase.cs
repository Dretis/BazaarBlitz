using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "New EntityInfoDatabase", menuName = "Scriptable Objects/Database/EntityInfoDatabase")]
public class EntityInfoDatabase : ScriptableObject
{
    [SerializeField] private List<EntityInfo> _entityInfos = new List<EntityInfo>();

    public Dictionary<int, EntityInfo> EntityDatabase { get; private set; } = new Dictionary<int, EntityInfo>();

    public void InitalizeDatabase()
    {
        EntityDatabase.Clear();

        foreach (var entityInfo in _entityInfos)
        {
            EntityDatabase.Add(entityInfo.DefaultEntityState.Id, entityInfo);
        }
    }

    public EntityInfo GetEntityInfo(int id)
    {
        if(EntityDatabase.TryGetValue(id, out EntityInfo ei))
        {
            Debug.Log($"Found EntityInfo with Id: {id} | {ei.DefaultEntityState.PlayerName}");
            return ei;
        }
        else
        {
            Debug.Log($"Cannot find EntityInfo with Id: {id}");
            return null;
        }
    }
}
