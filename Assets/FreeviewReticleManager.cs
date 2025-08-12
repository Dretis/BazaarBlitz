using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeviewReticleManager : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    //[SerializeField] private CircleCollider2D cc;
    private MapNode selectedNode;

    [Header("Broadcast on Event Channels")]
    //public NodeEventChannelSO m_EnterRaycastedTile;
    public VoidEventChannelSO m_ExitRaycastedTile;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        //cc = GetComponent<CircleCollider2D>();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<MapNode>(out MapNode node))
        {
            //if(node = selectedNode)
            //Debug.Log("Exitting tile");
            m_ExitRaycastedTile.RaiseEvent();
        }
    }
}
