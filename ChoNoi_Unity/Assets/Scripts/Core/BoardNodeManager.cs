using Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardNodeManager : MonoBehaviour
{
    private GameplayTest _gameplayTest; // Unneeded?

    public NodeInfoDatabase Database; // may need to be static? shouldnt ever change after initialize
    [SerializeField]
    public Dictionary<int, BoardNode> NodeDictionary = new Dictionary<int, BoardNode>();

    [Header("Debug")]
    [SerializeField] private bool isDebugOn;
    public void Initialize()
    {
        Database.InitalizeDatabase();

        foreach (Transform child in transform)
        {
            var node = child.GetComponent<BoardNode>();

            if (!NodeDictionary.ContainsKey(node.BoardNodeData.Id))
            {
                NodeDictionary.Add(node.BoardNodeData.Id, node);
                node.Initialize(this);
                Debug.Log($"{node.name} added into dictionary");
            }
            else
            {
                Debug.LogWarning($"!! | {node.name} ID is duplicate!");
            }
        }
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        // DEBUG EDITOR ONLY
        if (isDebugOn)
        {
            foreach (Transform child in transform)
            {
                var node = child.GetComponent<BoardNode>();

                node.debugTypeText.enabled = true;
                node.debugIdText.enabled = true;
                if(node.BoardNodeData.Type is NodeType.Stamp)
                {
                    node.debugTypeText.color = Database.GetStampColor(node.BoardNodeData.StampData.CurrentStampType);
                }
                else
                {
                    node.debugTypeText.color = Database.GetNodeInfo(node.BoardNodeData.Type).nodeHoverColor;
                }
                //node.debugTypeText.color = Color.green;
            }
        }
        else
        {
            foreach (Transform child in transform)
            {
                var node = child.GetComponent<BoardNode>();

                node.debugTypeText.enabled = false;
                node.debugIdText.enabled = false;
                //node.debugIdText.color = Database.GetNodeInfo(node.BoardNodeData.Type).nodeHoverColor;
            }
        }
    }
    #endif
}
