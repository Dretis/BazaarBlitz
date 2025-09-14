using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using static GameplayTest;

public class IncidentTrain : MonoBehaviour
{
    [SerializeField] private SpecialIncidents associatedIncident = SpecialIncidents.Train;
    [SerializeField] private List<MapNode> affectedNodes;

    [SerializeField] private PlayableDirector cutscene;
    [SerializeField] private GameObject trainObject;
    [SerializeField] private Vector3 startingPos;

    [Header("Broadcast on Event Channels")]
    //public VoidEventChannelSO m_IncidentStarted;
    public NodeListFloatEventChannelSO m_DamageAffectedNodes;

    [Header("Listen on Event Channels")]
    public VoidEventChannelSO m_ActivateIncidentTrain;

    private void OnEnable()
    {
        m_ActivateIncidentTrain.OnEventRaised += OnActivateIncidentTrain;
    }

    private void OnDisable()
    {
        m_ActivateIncidentTrain.OnEventRaised -= OnActivateIncidentTrain;
    }

    // Start is called before the first frame update
    void Start()
    {
        cutscene.GetComponent<PlayableDirector>();
    }

    private void OnActivateIncidentTrain()
    {
        trainObject.transform.position = startingPos;
        cutscene.Play();
    }

    //helper functions during the Timeline

    // Sends in the Max HP% of damage taken
    public void SignalDamageAffectedNodes(float damage)
    {
        m_DamageAffectedNodes.RaiseEvent(affectedNodes, damage);
    }

    public void SignalIncidentStarted()
    {
        Debug.Log("Cutscene Incident started");
        //m_IncidentStarted.RaiseEvent();
    }

    public void SignalIncidentOver()
    {
        Debug.Log("Cutscene Incident over");
        instance.incidentIsPlaying = false;
    }
}
