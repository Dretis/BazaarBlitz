using Core;
using Core.Events;
using System.Collections.Generic;
using UnityEngine;

public class AvailableNodeIndicator : MonoBehaviour
{
    private GameplayTest _gameplayTest;
    private Transform target;

    [SerializeField] private List<Transform> nodeIndicators = new List<Transform>();
    [SerializeField] private List<SpriteRenderer> nodeVisualIndicators = new List<SpriteRenderer>();
    [SerializeField] private List<NearbyNodeData> nearbyNodes = new List<NearbyNodeData>();
    [SerializeField] private Vector3 nearbyNodesOffset;

    [Header("Listen on Event Channels")]
    public GameStateEventChannelSO _gameStateEventChannel;

    public void Initialize(GameplayTest gt) 
    {
        _gameplayTest = gt;
    }

    private void OnEnable()
    {
        _gameStateEventChannel.OnEventRaised += HandleGameState;
    }

    private void OnDisable()
    {
        _gameStateEventChannel.OnEventRaised -= HandleGameState;
    }

    private void HandleGameState(FrameGameState state)
    {
        if (!state.DidChangePhases())
        {
            return;
        }

        if (state.NewState.GamePhase is GameplayTest.GamePhase.InitialTurnMenu)
        {
            HideNodeIndicators();
        }

        if (state.NewState.GamePhase is GameplayTest.GamePhase.RollDice)
        {
            //ShowNodeIndicators();
        }
        else if (state.PrevState.GamePhase is GameplayTest.GamePhase.RollDice)
        {
            //HideNodeIndicators();
        }

        if (state.NewState.GamePhase is GameplayTest.GamePhase.PickDirection)
        {
            //OnPlayerMovedOnBoard();
            //SetTargetNode(state.NewState);
            if (state.NewState.MovementLeft <= 0)
                HideNodeIndicators();
            else
                ShowNodeIndicators(state.NewState);
        }
        if (state.NewState.GamePhase is GameplayTest.GamePhase.IncidentHappening)
        {
            HideNodeIndicators();
        }

        if (state.NewState.GamePhase is GameplayTest.GamePhase.EncounterTime)
        {
            HideNodeIndicators();
        }
        
        if (state.NewState.GamePhase is GameplayTest.GamePhase.GameOver)
        {
            HideNodeIndicators();
        }
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

    private void ShowNodeIndicators(GameState state)
    {
        var ed = state.CurrentPlayer().EntityData;
        var nodeId = ed.OccupiedNodeId;

        var occupiedNode = _gameplayTest.BoardNodeManager.NodeDictionary[ed.OccupiedNodeId];

        target = occupiedNode.transform;
        transform.position = target.position + nearbyNodesOffset;

        nearbyNodes.Clear();
        nearbyNodes.Add(occupiedNode.BoardNodeData.NorthNode);
        nearbyNodes.Add(occupiedNode.BoardNodeData.EastNode);
        nearbyNodes.Add(occupiedNode.BoardNodeData.SouthNode);
        nearbyNodes.Add(occupiedNode.BoardNodeData.WestNode);

        var existingNearbyNodeIds = new List<int>();

        for (int i = 0; i < nodeIndicators.Count; i++)
        {
            var lastEle = state.TraveledNodeIds.Count;
            var lastNodeId = state.TraveledNodeIds[^1];

            if (lastEle > 1)
                lastNodeId = state.TraveledNodeIds[^2];

            //if (nearbyNodes[i] == null || existingNodes.Contains(nearbyNodes[i])) 
            if (existingNearbyNodeIds.Contains(nearbyNodes[i].Id) ||
                nearbyNodes[i].Id == -1 || nearbyNodes[i].IsOneWay || 
                nearbyNodes[i].Id == ed.PreviousNodeId ||
                nearbyNodes[i].Id == lastNodeId) 
            {
                // non-existent direction or already node is already indicated
                nodeVisualIndicators[i].enabled = false;
            }
            else
            {
                var id = nearbyNodes[i].Id;

                nodeVisualIndicators[i].enabled = true;
                nodeIndicators[i].right = _gameplayTest.BoardNodeManager.NodeDictionary[id].transform.position
                                            - nodeIndicators[i].position + nearbyNodesOffset;
                existingNearbyNodeIds.Add(nearbyNodes[i].Id);
            }
        }
    }
}
