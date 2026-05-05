using System.Collections.Generic;
using UnityEngine;
using static GameplayTest;
using static Stamp;

/// <summary>
/// Script for the Central Exchange Node
/// </summary>
public class PawnShop : MapNode
{
    [Header("Current Player Tracking")]
    [SerializeField] private List<int> snapshotHealths;
    [SerializeField] private List<int> snapshotPoints;
    [SerializeField] private List<float> snapshotReps;

    [SerializeField] private List<List<StampType>> snapshotStamps = new List<List<StampType>>();

    [Header("Broadcast on Event Channels")]
    public IntEventChannelSO m_UpdatePlayerScore;
    public IntEventChannelSO m_PlayerScoreIncreased;

    public VoidEventChannelSO m_PassByPawnShop;
    public PlayerEventChannelSO m_UndoPassByPawnShop;

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
        Debug.Log($"Landed on PawnShop - {name}");

        // Clear your direction so you can choose next turn
        p.previousNode = null;

        //encounterOver = true;
        var gp = GameplayTest.instance;
        gp.phase = GamePhase.EndTurn;
    }

    public override void PassByThisNode(EntityPiece p)
    {
        // Current player passed this node
        //if (p.movementLeft == 0) return;

        var gp = GameplayTest.instance;

        Debug.Log($"Passed MapNode - {name}");

        SnapshotEntity(p);

        var repGained = 75 * Mathf.Pow(1.5f, p.stamps.Count - 1);
        var pointsGained = (int)(150 * Mathf.Pow(2, p.stamps.Count - 1));

        if (p.stamps.Count != 0)
        {
            p.ReputationPoints += repGained;
            p.heldPoints += pointsGained;
            //m_UpdatePlayerScore.RaiseEvent(currentPlayer.id);
            m_PlayerScoreIncreased.RaiseEvent(pointsGained);
            m_PassByPawnShop.RaiseEvent(); // change this later

            p.stamps.Clear();
        }

        // Heal player by 33%
        //currentPlayerInitialHealth = p.health; // track health before, in case they undo
        p.health += p.maxHealth / 3;
        if (p.health > p.maxHealth)
        {
            p.health = p.maxHealth;
        }
        m_UpdatePlayerScore.RaiseEvent(p.id);

        if (p.heldPoints >= gp.currentRuleset.pointGoal)
        {
            Debug.Log("BRO HE WON");
            gp.winner = p;
            gp.phase = GameplayTest.GamePhase.EndGame; // Finish game if player w/ enough points passes by Pawn Shop
            gp.EndGame();
            return;
        }
    }

    public override void UndoPassByThisNode(EntityPiece p)
    {
        // Current player is undo'd passing this node

        Debug.Log($"Undo Passing MapNode - {name}");

        //p.stamps = new List<Stamp.StampType>(oldStamps);
        //p.heldPoints = oldPoints;
        //p.ReputationPoints = oldRep;
        //p.health = currentPlayerInitialHealth; // revert healing back

        //if (snapshotStamps == null) return;

        p.health = snapshotHealths[^1];
        p.heldPoints = snapshotPoints[^1];
        p.ReputationPoints = snapshotReps[^1];
        DecrementSnapshotEntity();

        if (snapshotStamps[^1] == null)
        {
            //Debug.Log($"Player had 0 stamps before.");
            p.stamps.Clear();
            snapshotStamps.RemoveAt(snapshotStamps.Count - 1);
        }
        else
        {
            //Debug.Log($"Removed last player's stamps from snapshotStamps | # of stamps = {snapshotStamps[^1]}");
            //DebugShowSnapshotStamps(snapshotStamps[^1]);

            p.stamps = new List<StampType>(snapshotStamps[^1]);
            snapshotStamps.RemoveAt(snapshotStamps.Count - 1);
        }

        m_UpdatePlayerScore.RaiseEvent(p.id);
        m_UndoPassByPawnShop.RaiseEvent(p);
    }

    private void SnapshotEntity(EntityPiece p)
    {
        snapshotReps.Add(p.ReputationPoints);
        snapshotPoints.Add(p.heldPoints);

        if (p.stamps.Count == 0)
        {
            Debug.Log($"Snapshoted player having 0 stamps.");
            snapshotStamps.Add(null);
        }
        else
        {
            snapshotStamps.Add(new List<StampType>(p.stamps));
        }

        snapshotHealths.Add(p.health);
    }

    private void DecrementSnapshotEntity()
    {
        snapshotReps.RemoveAt(snapshotReps.Count - 1);
        snapshotPoints.RemoveAt(snapshotPoints.Count - 1);
        snapshotHealths.RemoveAt(snapshotHealths.Count - 1);
    }

    public void OnNextPlayerTurn(EntityPiece entity)
    {
        snapshotReps.Clear();
        snapshotPoints.Clear();
        snapshotHealths.Clear();
        snapshotStamps.Clear();
    }
}
