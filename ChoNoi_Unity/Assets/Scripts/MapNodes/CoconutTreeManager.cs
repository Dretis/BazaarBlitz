using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CoconutTreeManager : MonoBehaviour
{
    [Header("Coconut Tree Stuff")]
    [SerializeField] private PlayableDirector cutscene;
    [SerializeField] private Transform affectedPlayer;
    [SerializeField] private SpriteRenderer coconutSprite;

    [SerializeField] private ItemStats coconutItem;

    [SerializeField] private List<MapNode> affectedNodes;

    private List<MapNode> affectedCurrentNode = new List<MapNode>();

    [Header("Broadcast on Event Channels")]
    public NodeListFloatEventChannelSO m_DamageAffectedNodes;

    [Header("Listen on Event Channels")]
    public NodeEventChannelSO m_LandOnCoconutTree;
    //public PlayerEventChannelSO m_BuyFromVendor;
    //public VoidEventChannelSO m_ExitVendor; // also broadcasting

    //public PlayerEventChannelSO m_NextPlayerTurn;

    //public VoidEventChannelSO m_IncidentStarted;


    private void OnEnable()
    {
        m_LandOnCoconutTree.OnEventRaised += OnLandOnCoconutTree;

        //m_NextPlayerTurn.OnEventRaised += OnNextPlayerTurn;

        //m_IncidentStarted.OnEventRaised += OnIncidentStarted;
    }

    private void OnDisable()
    {
        m_LandOnCoconutTree.OnEventRaised -= OnLandOnCoconutTree;

        //m_NextPlayerTurn.OnEventRaised -= OnNextPlayerTurn;

        //m_IncidentStarted.OnEventRaised -= OnIncidentStarted;
    }
    private void OnLandOnCoconutTree(MapNode node)
    {
        affectedPlayer = GameplayTest.instance.currentPlayer.occupiedNode.transform;
        affectedCurrentNode.Clear();
        affectedCurrentNode.Add(node);

        coconutSprite.transform.position = affectedPlayer.transform.position + new Vector3(0,3.5f,0);
        cutscene.Play();
    }

    private void OnIncidentStarted()
    {
        throw new NotImplementedException();
    }

    private void OnNextPlayerTurn(EntityPiece arg0)
    {
        throw new NotImplementedException();
    }

    public void GiveCoconutToCurrentPlayer()
    {
        GameplayTest.instance.currentPlayer.inventory.Add(coconutItem);
    }

    public void SignalDamageAffectedNodes(float damage)
    {
        GiveCoconutToCurrentPlayer();
        m_DamageAffectedNodes.RaiseEvent(affectedCurrentNode, damage);
    }

    public void SignalCutsceneStarted()
    {
        Debug.Log("Cutscene Incident started");
        //m_IncidentStarted.RaiseEvent();

    }

    public void SignalCutsceneOver()
    {
        Debug.Log("Cutscene Incident over");
        //GameplayTest.instance.incidentIsPlaying = false;
        GameplayTest.instance.phase = GameplayTest.GamePhase.EndTurn;
    }
}
