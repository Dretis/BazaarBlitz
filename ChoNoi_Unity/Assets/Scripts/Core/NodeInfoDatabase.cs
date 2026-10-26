using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "New NodeInfoDatabase", menuName = "Scriptable Objects/Database/NodeInfoDatabase")]
public class NodeInfoDatabase : ScriptableObject
{
    [SerializeField] private List<NodeInfo> _nodeInfos = new List<NodeInfo>();

    public Dictionary<Core.NodeType, NodeInfo> NodeDatabase { get; private set; } = new Dictionary<Core.NodeType, NodeInfo>();

    [SerializeField] private List<StampInfo> stampInfo;

    [System.Serializable]
    private struct StampInfo
    {
        public Color color;
        public Sprite visualSprite;
        public Sprite nodeSprite;
        public LocalizedString l_colorString;
    }

    public void InitalizeDatabase()
    {
        NodeDatabase.Clear();

        foreach (var nodeInfo in _nodeInfos)
        {
            NodeDatabase.Add(nodeInfo.NodeType, nodeInfo);
        }
    }

    public NodeInfo GetNodeInfo(Core.NodeType type)
    {
        if (NodeDatabase.ContainsKey(type))
        {
            return NodeDatabase[type];
        }
        else
        {
            Debug.Log($"Cannot find NodeInfo for {type}");
            return null;
        }
    }
    
    public Color GetStampColor(Core.StampType stampType)
    {
        //return stampColors[(int)stampType];
        return stampInfo[(int)stampType].color;
    }
    
    public Sprite GetStampVisualSprite(Core.StampType stampType)
    {
        return stampInfo[(int)stampType].visualSprite;
    }
    public Sprite GetStampNodeSprite(Core.StampType stampType)
    {
        return stampInfo[(int)stampType].nodeSprite;
    }
}
