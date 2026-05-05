using System.Collections.Generic;
using UnityEngine;
using static GameplayTest;
using static Stamp;

/// <summary>
/// Script for the 'WarpNode' Node
/// </summary>
public class WarpNode : MapNode
{
    [Header("THIS Specific Variable")]
    [SerializeField] private MapNode destinationNode; // The associated node you warp to

    [Header("Broadcast on Event Channels")]
    public NodeEventChannelSO m_LandOnWarpNode;

    [Header("Listen on Event Channels")]
    public PlayerEventChannelSO m_NextPlayerTurn;

    protected override void OnEnable()
    {
        m_NextPlayerTurn.OnEventRaised += OnNextPlayerTurn;
    }

    protected override void OnDisable()
    {
        m_NextPlayerTurn.OnEventRaised -= OnNextPlayerTurn;
    }

    public override void LandOnThisNode(EntityPiece p)
    {
        // Current player landed on this node
        if (destinationNode == null) return;
        Debug.Log($"Landed on a Warp Node - {name}");

        GameplayTest.instance.phase = GamePhase.IncidentHappening;
        GameplayTest.instance.expectedPhase = GamePhase.EndTurn;

        m_LandOnWarpNode.RaiseEvent(this); // tell others this is where you landed
    }

    public override void PassByThisNode(EntityPiece p)
    {
        // Current player passed this node
        //if (p.movementLeft == 0) return;
    }

    public override void UndoPassByThisNode(EntityPiece p)
    {
        // Current player is undo'd passing this node
        //Debug.Log($"Undo Passing MapNode - {name}");
    }

    public void OnNextPlayerTurn(EntityPiece entity)
    {

    }

    public MapNode GetDestinationNode()
    {
        return destinationNode;
    }
    /*
    // May not be necessary
    public override string GetTileTypeString()
    {
        return tileTypeString.GetLocalizedString();
    }

    // May not be necessary
    public override string GetTileAboutString()
    {
        return tileAboutString.GetLocalizedString();
    }
    */
}
