using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeviewReticleManager : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    //[SerializeField] private CircleCollider2D cc;
    private bool hasSelectedNode;

    [Header("Broadcast on Event Channels")]
    public VoidEventChannelSO m_ExitRaycastedTile;

    [Header("Broadcast on Event Channels")]
    public NodeEventChannelSO m_EnterRaycastedTile;

    private void OnEnable()
    {
        m_EnterRaycastedTile.OnEventRaised += OnEnterRaycastedTile;
    }

    private void OnDisable()
    {
        m_EnterRaycastedTile.OnEventRaised -= OnEnterRaycastedTile;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        //cc = GetComponent<CircleCollider2D>();
    }

    private void OnEnterRaycastedTile(MapNode node)
    {
        hasSelectedNode = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<MapNode>(out MapNode node) && hasSelectedNode)
        {
            //if(node = selectedNode)
            Debug.Log("Exitting tile");
            m_ExitRaycastedTile.RaiseEvent();
            hasSelectedNode = false;
        }
    }
}
