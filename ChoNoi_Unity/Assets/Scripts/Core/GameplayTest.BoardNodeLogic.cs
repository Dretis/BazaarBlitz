using Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class GameplayTest : MonoBehaviour
{
    private void HandlePickDirection(GameState prevState, ref GameState newState, Vector2 direction)
    {
        //var p = currentPlayer;
        var p = newState.Players[newState.CurrPlayerId].EntityData;

        var x = direction.x;
        var y = direction.y;

        float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

        if (direction == Vector2.zero) return;

        NearbyNodeData wantedNode;

        if (angle <= 135 && angle >= 45)
        {
            //Debug.Log("MOVE UP/NORTH");
            //wantedNode = p.OccupiedNode.north;
            wantedNode = newState.BoardNodes[p.OccupiedNodeId].NorthNode;

            if (!wantedNode.IsOneWay)
                newState.WantedNodeId = newState.BoardNodes[p.OccupiedNodeId].NorthNode.Id;
        }
        else if (angle < 45 && angle > -45 && x != 0)
        {
            //Debug.Log("MOVE RIGHT/EAST");
            //wantedNode = p.OccupiedNode.east;
            wantedNode = newState.BoardNodes[p.OccupiedNodeId].EastNode;

            if (!wantedNode.IsOneWay)
                newState.WantedNodeId = newState.BoardNodes[p.OccupiedNodeId].EastNode.Id;

            newState.CurrentPlayerObj.playerSprite.flipX = true; // put this in the new EntityVisual
        }
        else if (angle <= -45 && angle >= -135)
        {
            //Debug.Log("MOVE DOWN/SOUTH");
            //wantedNode = p.OccupiedNode.south;
            wantedNode = newState.BoardNodes[p.OccupiedNodeId].SouthNode;

            if (!wantedNode.IsOneWay)
                newState.WantedNodeId = newState.BoardNodes[p.OccupiedNodeId].SouthNode.Id;
        }
        else if (angle < -135 || angle > 135)
        {
            //Debug.Log("MOVE LEFT/WEST");
            //wantedNode = p.OccupiedNode.west;
            wantedNode = newState.BoardNodes[p.OccupiedNodeId].WestNode;

            if(!wantedNode.IsOneWay)
                newState.WantedNodeId = newState.BoardNodes[p.OccupiedNodeId].WestNode.Id;

            newState.CurrentPlayerObj.playerSprite.flipX = false; // put this in the new EntityVisual
        }

        //Debug.Log($"MOVING | {direction} | Angle: {angle}\' | Going from ({p.OccupiedNodeId}) to ({newState.WantedNodeId})");
        // Angles | 0 = Right, 90 = Up, 180 = Left, -90 = Down

        if (newState.WantedNodeId != -1 && newState.WantedNodeId != p.PreviousNodeId)
        {
            //phase = GamePhase.MoveAround;
            newState.GamePhase = GamePhase.MoveAround;
        }
        else
        {
            newState.WantedNodeId = -1;
        }
    }

    #region Node-based Functions
    GamePhase HandleMoveAround(GameState prevState, ref GameState newState)
    {
        //var p = currentPlayer;
        ref var p = ref newState.Players[newState.CurrPlayerId].EntityData;
        //ref var p = ref newState.CurrentPlayerData();

        //var lastEle = p.TraveledNodes.Count;
        //var lastNode = p.TraveledNodes[^1];
        var lastEle = newState.TraveledNodeIds.Count;
        var lastNodeId = newState.TraveledNodeIds[^1];

        if (lastEle > 1)
            lastNodeId = newState.TraveledNodeIds[^2];

        if (newState.WantedNodeId == lastNodeId) // The direction you picked was the node you just came from (Redo)
        {
            newState.TraveledNodeIds.RemoveAt(lastEle - 1);
            p.OccupiedNodeId = lastNodeId;

            newState.MovementLeft++;
            newState.WantedNodeId = -1;

            newState.RevertCurrentPlayerData();
            return GamePhase.PickDirection;
        }
        else if (newState.WantedNodeId != -1) // Go to that new node and occupy it
        {
            ref var desiredBoardNode = ref newState.BoardNodes[newState.WantedNodeId];

            if (desiredBoardNode.Deployable is Deployable.Rafflesia)
            {
                desiredBoardNode.Deployable = Deployable.None;

                TryCalculateTempoBuff(prevState, ref newState);
                //p.movementLeft = 0;
                newState.MovementLeft = 0;

                //rollTypewriter.ShowText("");

                //p.PreviousNode = p.TraveledNodes[p.TraveledNodes.Count - 1];

                //wantedNode.flowerTrapVisual.color = Color.white;
                //wantedNode.flowerTrapVisual.enabled = false;

                newState.WantedNodeId = -1;

                //p.TraveledNodes.Clear();
                //p.TraveledNodes.Add(p.OccupiedNode);

                p.OccupiedNodeId = desiredBoardNode.Id;

                newState.TraveledNodeIds.Clear();
                newState.TraveledNodeIds.Add(p.OccupiedNodeId);

                return GamePhase.EncounterTime;
            }

            //p.TraveledNodes.Add(p.OccupiedNode);
            //p.OccupiedNode = wantedNode;

            p.OccupiedNodeId = desiredBoardNode.Id;
            newState.TraveledNodeIds.Add(p.OccupiedNodeId);

            newState.MovementLeft--;
            newState.WantedNodeId = -1;

            return GamePhase.PassBy;
        }

        newState.SnapshotCurrentPlayerData();
        return GamePhase.PickDirection;
    }

    private List<int> GetOtherPlayersOnNode(GameState newState, PlayerState ps, BoardNodeData bnd)
    {
        var otherPlayers = bnd.occupiedEntities;

        if (otherPlayers.Contains(ps.Id))
        {
            otherPlayers.Remove(ps.Id); // get rid of the currentplayer from the list
        }

        //otherPlayers = otherPlayers.Where(player => !(player.Statuses.Contains(Status.InsideVendor)
        //                                        || player.Statuses.Contains(Status.Invulernable))).ToList();

        return otherPlayers;
    }

    GamePhase HandlePassBy(GameState prevState, ref GameState newState)
    {
        ref var p = ref newState.CurrentPlayer().EntityData;
        ref var n = ref newState.BoardNodes[p.OccupiedNodeId];

        // Whole ass logic here
        PassByNode(prevState, ref newState);

        // Conditional Things here
        var otherPlayers = GetOtherPlayersOnNode(newState, newState.CurrentPlayer(), n);

        // There's other players on the space you pass
        if (CanFightPlayers(otherPlayers))
        {
            //EntityPiece otherPlayer = m.playerOccupied;
            Debug.Log("hello");
            // Check if can steal item from player.
            //if (p.currentStatsModifier.canStealOnPassBy && otherPlayer.inventory.Count > 0)
            if (p.CurrentStatsModifier.canStealOnPassBy)
            {
                foreach (int otherPlayer in otherPlayers)
                {
                    Debug.Log("Should be a yoink Steal");
                    //m_StealOnPassBy.RaiseEvent(otherPlayer);
                }

                // Deactivate all active effects of items that end on stealing.
                p.RemoveItemEffectOnUse(ItemLists.StealOnPassByItemNames); // ?? change this it shouldn't go away
            }

            // Check if can initiate combat via pass, like Cobra Whiskey.
            // and there is a player (not enemy) on that tile
            /*
            if (p.CurrentStatsModifier.canInitiateCombatOnPassBy &&
                otherPlayers.Exists(op => !op.IsEnemy))
            //otherPlayers.Exists(op => op.EntityType is not EntityType.Enemy))
            {
                Debug.Log("Combat via Passing");
                p.RemoveItemEffectOnUse(ItemLists.CombatOnPassByItemNames);

                return InitiateCombat(p, m);
            }
            */

        }
        //else if (m.CompareTag("Store") && (m.modifier == MapNode.Modifier.Marigold && m.modifierOwner is p)) 
        //else if (m.CompareTag("Store") && (m.modifier == MapNode.Modifier.Marigold && m.modifierOwnerId == newState.CurrPlayerId))
        else if (n.Type is NodeType.Storefront && (n.Deployable is Deployable.HoaMai && n.DeployableOwnerId == newState.CurrPlayerId))
        {
            //m.flowerTrapVisual.color = Color.white; // put this in BoardNode Visual
            //m.flowerTrapVisual.enabled = false;

            //p.PreviousNode = p.TraveledNodes[p.TraveledNodes.Count - 1];
            p.PreviousNodeId = newState.TraveledNodeIds[^1];

            newState.MovementLeft = 0;

            //m.modifier = MapNode.Modifier.None;
            n.Deployable = Deployable.None;
        }

        // Change phase.

        //if (p.movementLeft <= 0)
        if (newState.MovementLeft <= 0)
        {
            //p.PreviousNode = p.TraveledNodes[p.TraveledNodes.Count - 1];
            //p.TraveledNodes.Clear();
            //p.TraveledNodes.Add(p.OccupiedNode);

            p.PreviousNodeId = newState.TraveledNodeIds[^2];
            newState.TraveledNodeIds.Clear();
            //newState.TraveledNodeIds.Add(p.OccupiedNodeId);

            if (turnOffMonsterEncounters)
                return GamePhase.EndTurn;
            else
                return GamePhase.EncounterTime; // next phase
        }
        else
        {
            newState.SnapshotCurrentPlayerData();
            return GamePhase.PickDirection;
        }
    }

    private void HandleEncounterTime(GameState prevState, ref GameState newState)
    {
        var p = newState.Players[newState.CurrPlayerId].EntityData;
        //var m = newState.Players[newState.CurrPlayerId].EntityData.OccupiedNode; // DELETE
        ref var n = ref newState.BoardNodes[p.OccupiedNodeId];

        // In-Combat Already
        if (ThisPlayerMustFight(p))
        {
            HandleInitiateCombat(prevState, ref newState);
        }
        else
        {
            TryCalculateTempoBuff(prevState, ref newState);
            HandleLandOnNode(prevState, ref newState);
        }
    }

    private void HandleLandOnNode(GameState prevState, ref GameState newState)
    {
        ref var p = ref newState.Players[newState.CurrPlayerId].EntityData;
        ref var n = ref newState.BoardNodes[p.OccupiedNodeId];

        var otherPlayers = n.occupiedEntities;

        switch (n.Type)
        {
            case NodeType.OpenSpace:
                if (CanFightPlayers(otherPlayers))
                {
                    HandleInitiateCombat(prevState, ref newState);
                }
                else
                {
                    Debug.Log("enemy Fight");
                    StartCoroutine(InitiateCombatOnEnemy(.05f, p)); // CHANGE THIS

                    newState.GamePhase = GamePhase.RockPaperScissors; // empty phase
                }
                break;
            case NodeType.Storefront:
                //StorefrontComponent.HandleLanding(ref newState);
                ref var store = ref n.StorefrontData;
                if (CanFightPlayers(n.occupiedEntities))
                {
                    HandleInitiateCombat(prevState, ref newState);
                }
                else if(store.OwnerId == newState.CurrPlayerId)
                {
                    // Renovate Time
                    newState.GamePhase = GamePhase.PreStockStore;
                }
                else if(store.IsEmpty())
                {
                    // Buying Time
                    newState.GamePhase = GamePhase.EndTurn;
                }
                else
                {
                    newState.GamePhase = GamePhase.InStore;
                }
                break;
            case NodeType.Exchange:
                // Reset direction - PrevNode
                p.PreviousNodeId = -1;
                newState.GamePhase = GamePhase.EndTurn;
                break;
            case NodeType.Stamp:
                // Reset direction - PrevNode
                // Renovate
                p.PreviousNodeId = -1;
                newState.GamePhase = GamePhase.PreStockStore;
                break;
            case NodeType.Wildcard:
                // Reset direction - PrevNode
                // Renovate
                p.PreviousNodeId = -1;
                newState.GamePhase = GamePhase.PreStockStore;
                break;
            case NodeType.MoveAgain:
                //p.TraveledNodes.Clear();
                //p.TraveledNodes.Add(p.OccupiedNode);

                newState.TraveledNodeIds.Clear();
                newState.TraveledNodeIds.Add(p.OccupiedNodeId);

                newState.CurrentPlayerDataStates = new EntityData[0];
                newState.SnapshotCurrentPlayerData();

                if (p.CurrentStatsModifier.canUseSpeedDie && p.Health <= 25)
                {
                    // swap to the other die not being shown
                    // temp
                    if (newState.UsingSpeedDieToMove)
                    {
                        newState.UsingSpeedDieToMove = false;
                        m_ChangeToMoveDie.RaiseEvent(0);
                    }
                }

                newState.GamePhase = GamePhase.RollDice;
                break;
            case NodeType.Warp:
                // Wawrp to destination node id
                break;
            case NodeType.Vendor:
                // update this
                List<int> exposedPlayersOnVendor = new List<int>();

                foreach (var playerOnVendor in otherPlayers)
                {
                    if (!newState.Players[playerOnVendor].EntityData.Statuses.Contains(Status.InsideVendor))
                    {
                        exposedPlayersOnVendor.Add(playerOnVendor);
                    }
                }

                if (CanFightPlayers(exposedPlayersOnVendor))
                {
                    Debug.Log("person here");

                    Debug.Log("fight person on vendor here");
                    HandleInitiateCombat(prevState, ref newState);
                }
                else
                {
                    newState.GamePhase = GamePhase.InVendor;
                }
                break;
            case NodeType.LuckyDice:
                // NOT IMPLEMENTED
                break;
            case NodeType.CoconutTree:
                //m_LandOnCoconutTree.RaiseEvent(m);
                newState.GamePhase = GamePhase.IncidentHappening;
                break;
            default:
                Debug.Log("Invalid Node type!");
                newState.GamePhase = GamePhase.EndTurn;
                break;
        }
    }
    public void PassByNode(GameState prevState, ref GameState newState)
    {
        ref var p = ref newState.Players[newState.CurrPlayerId].EntityData;
        ref var n = ref newState.BoardNodes[p.OccupiedNodeId];

        var otherPlayers = n.occupiedEntities;
        var stampIndex = -1;

        switch (n.Type)
        {
            case NodeType.OpenSpace:
                // Nothing
                break;
            case NodeType.Storefront:
                //StorefrontComponent.HandlePassing(ref newState);
                break;
            case NodeType.Exchange:
                // Turn in stamps for money and EXP
                var stampCount = p.GetStampCount();
                var expGained = (int)(75 * Mathf.Pow(1.5f, stampCount - 1));
                var moneyGained = (int)(250 * Mathf.Pow(2, stampCount - 1)); // was 150

                if (stampCount != 0)
                {
                    p.Experience += expGained;
                    p.Money += moneyGained;
                    p.Stamps = new StampType[p.Stamps.Length];
                }

                // Heal player by 33%
                p.Health += p.MaxHealth / 3;
                if (p.Health > p.MaxHealth)
                {
                    p.Health = p.MaxHealth;
                }


                if (p.Money >= currentRuleset.pointGoal)
                {
                    Debug.Log("BRO HE WON");
                    //winner = p; // Finish game if player w/ enough points passes by Pawn Shop
                    newState.GamePhase = GamePhase.EndGame;
                }
                break;
            case NodeType.Stamp:
                // Grab color stamp if unowned
                //if (!p.Stamps.Contains(n.StampData.CurrentStampType))
                stampIndex = (int) (n.StampData.CurrentStampType - 1);
                if (p.Stamps[stampIndex] is StampType.None)
                {
                    Debug.Log($"{p.EntityName} doesn't have {n.StampData.CurrentStampType}, grabbing it on pass!");
                    //p.Stamps.Add(n.StampData.CurrentStampType);
                    //p.Add(ref p.Stamps, (n.StampData.CurrentStampType));
                    p.AddStamp(n.StampData.CurrentStampType);
                    //p.Stamps[stampIndex] = n.StampData.CurrentStampType;
                }
                break;
            case NodeType.Wildcard:
                // Grab color stamp if unowned
                // Change color stamp to the next one
                //if (!p.Stamps.Contains(n.StampData.CurrentStampType))
                stampIndex = (int)(n.StampData.CurrentStampType - 1);
                if (p.Stamps[stampIndex] is StampType.None)
                {
                    Debug.Log($"{p.EntityName} doesn't have {n.StampData.CurrentStampType}, grabbing it on pass!");
                    //p.Stamps.Add(n.StampData.CurrentStampType);
                    //p.Add(ref p.Stamps, (n.StampData.CurrentStampType));
                    p.AddStamp(n.StampData.CurrentStampType);
                    //System.Array.Clear(p.Stamps, 0, p.Stamps.Length);
                }
                n.StampData.CurrentStampType = n.StampData.GetNextStampType();
                break;
            case NodeType.MoveAgain:
                // Nothing
                break;
            case NodeType.Warp:
                // Nothing
                break;
            case NodeType.Vendor:
                // Nothing
                break;
            case NodeType.LuckyDice:
                // Nothing
                break;
            case NodeType.CoconutTree:
                // Nothing
                break;
            default:
                Debug.Log("Invalid Node type!");
                break;
        }
    }
    public void UndoPassByNode(GameState prevState, ref GameState newState)
    {
        ref var p = ref newState.Players[newState.CurrPlayerId].EntityData;
        ref var n = ref newState.BoardNodes[p.OccupiedNodeId];

        var otherPlayers = n.occupiedEntities;
        switch (n.Type)
        {
            case NodeType.OpenSpace:
                // Nothing
                break;
            case NodeType.Storefront:
                // Do nothing
                break;
            case NodeType.Exchange:
                // Revert stamps, money, and HP
                break;
            case NodeType.Stamp:
                // Give back stamp if didnt have it
                break;
            case NodeType.Wildcard:
                // Give back stamp if didnt have it
                n.StampData.CurrentStampType = n.StampData.GetPrevStampType();
                break;
            case NodeType.MoveAgain:
                // Do nothing
                break;
            case NodeType.Warp:
                // Do nothing
                break;
            case NodeType.Vendor:
                // Do nothing
                break;
            case NodeType.LuckyDice:
                // Do nothing
                break;
            case NodeType.CoconutTree:
                // Do nothing
                break;
            default:
                Debug.Log("Invalid Node type!");
                break;
        }
    }
    #endregion
}
