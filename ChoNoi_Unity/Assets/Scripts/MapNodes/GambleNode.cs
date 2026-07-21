using System.Collections.Generic;
using UnityEngine;
using static GameplayTest;
using static Stamp;

/// <summary>
/// Script for the Gamble Map Node, makes everyone play "Lucky Dice" / Bau Cua Ca Cop
/// </summary>
public class GambleNode : MapNode
{
    //[Header("THIS Specific Variable")]

    //[Header("Broadcast on Event Channels")]

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
        //Debug.Log($"Landed on this Node - {name}");
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
}
