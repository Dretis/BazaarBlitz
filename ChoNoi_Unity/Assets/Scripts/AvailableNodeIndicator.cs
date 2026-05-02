using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AvailableNodeIndicator : MonoBehaviour
{
    private Transform target;
    [SerializeField] private EntityPiece currentPlayer;
    [SerializeField] private MapNode targetNode;
    [SerializeField] private MapNode prevTargetNode;
    //private List<MapNode> nearbyNodes = new List<MapNode>();

    [SerializeField] private List<Transform> nodeIndicators = new List<Transform>();
    [SerializeField] private List<SpriteRenderer> nodeVisualIndicators = new List<SpriteRenderer>();
    [SerializeField] private List<MapNode> nearbyNodes = new List<MapNode>();
    [SerializeField] private Vector3 nearbyNodesOffset;

    //[SerializeField] private Transform northTarget;
    //[SerializeField] private Transform eastTarget;
    //[SerializeField] private Transform southTarget;
    //[SerializeField] private Transform westTarget;

    [Header("Listen on Event Channels")]
    public PlayerEventChannelSO m_NextPlayerTurn;

    public VoidEventChannelSO m_DiceRolled;
    public VoidEventChannelSO m_PlayerMovedOnBoard;
    public VoidEventChannelSO m_PlayerUndidSomething; // is this going back while moving??

    public VoidEventChannelSO m_IncidentStarted;

    public PlayerEventChannelSO m_PlayerWon;

    private void OnEnable()
    {
        m_NextPlayerTurn.OnEventRaised += OnNextPlayerTurn;
        m_DiceRolled.OnEventRaised += OnDiceRolled;
        m_PlayerMovedOnBoard.OnEventRaised += OnPlayerMovedOnBoard;
        m_PlayerUndidSomething.OnEventRaised += OnPlayerUndidSomething;

        m_IncidentStarted.OnEventRaised += OnIncidentStarted;

        m_PlayerWon.OnEventRaised += OnPlayerWon;
    }

    private void OnDisable()
    {
        m_NextPlayerTurn.OnEventRaised -= OnNextPlayerTurn;
        m_DiceRolled.OnEventRaised -= OnDiceRolled;
        m_PlayerMovedOnBoard.OnEventRaised -= OnPlayerMovedOnBoard;
        m_PlayerUndidSomething.OnEventRaised -= OnPlayerUndidSomething;

        m_IncidentStarted.OnEventRaised -= OnIncidentStarted;

        m_PlayerWon.OnEventRaised -= OnPlayerWon;
    }

    // Update is called once per frame
    void Update()
    {
        //transform.LookAt(target, Vector3.down);
        //transform.right = target.position - transform.position;
    }

    private void HideNodeIndicators()
    {
        foreach(SpriteRenderer renderer in nodeVisualIndicators)
        {
            renderer.enabled = false;
        }
    }

    private void ShowNodeIndicators()
    {
        transform.position = target.position + nearbyNodesOffset;

        var existingNodes = new List<MapNode>();

        for (int i = 0; i < nodeIndicators.Count; i++)
        {
            //Debug.Log($"nearbyNodes[{i}] = { nearbyNodes[i]}");
            if (nearbyNodes[i] == null || existingNodes.Contains(nearbyNodes[i])) 
            {
                // non-existent direction or already node is already indicated
                nodeVisualIndicators[i].enabled = false;
            }
            else if(nearbyNodes[i] == prevTargetNode || nearbyNodes[i] == currentPlayer.previousNode)
            {
                // don't show either if its the node you came from
                nodeVisualIndicators[i].enabled = false;
            }
            else
            {
                nodeVisualIndicators[i].enabled = true;
                nodeIndicators[i].right = nearbyNodes[i].transform.position - nodeIndicators[i].position + nearbyNodesOffset;
                existingNodes.Add(nearbyNodes[i]);
            }
        }
    }

    private void SetTargetNode(MapNode node)
    {
        if(target != null)
        {
            // Track the previous node in the list
            prevTargetNode = currentPlayer.traveledNodes[^1];
        }

        targetNode = node;
        target = node.transform;

        nearbyNodes.Clear();

        TryAddNearbyNode(node.north);
        TryAddNearbyNode(node.east);
        TryAddNearbyNode(node.south);
        TryAddNearbyNode(node.west);

        //targetNode.north;
    }

    private void TryAddNearbyNode(MapNode node) 
    {
        if (node == null)
            nearbyNodes.Add(null);
        else
            nearbyNodes.Add(node);
    }


    private void OnNextPlayerTurn(EntityPiece player)
    {
        currentPlayer = player;

        HideNodeIndicators();
        SetTargetNode(player.occupiedNode);
    }

    private void OnDiceRolled()
    {
        //throw new NotImplementedException();
        //ShowNodeIndicators();
    }

    private void OnPlayerMovedOnBoard()
    {
        SetTargetNode(currentPlayer.occupiedNode);
        if (currentPlayer.movementLeft <= 0)
            HideNodeIndicators();
        else
            ShowNodeIndicators();
    }

    private void OnPlayerUndidSomething()
    {
        SetTargetNode(currentPlayer.occupiedNode);
        ShowNodeIndicators();
    }

    private void OnIncidentStarted()
    {
        HideNodeIndicators();
    }

    private void OnPlayerWon(EntityPiece winner)
    {
        HideNodeIndicators();
    }
}
