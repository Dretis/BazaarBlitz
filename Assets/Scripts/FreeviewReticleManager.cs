using LitMotion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameplayTest;

public class FreeviewReticleManager : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;

    [Space]
    [SerializeField] private SpriteRenderer reticle;
    [SerializeField] private Color untargetedColor;
    [SerializeField] private Color invalidTargetColor;
    [SerializeField] private Color targetedColor;

    //[SerializeField] private CircleCollider2D cc;
    private bool hasSelectedNode;
    private Vector3 selectedNodeOffset = new Vector3(0, 0.25f, 0);

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
        reticle = GetComponentInChildren<SpriteRenderer>();
        //cc = GetComponent<CircleCollider2D>();
    }

    private void OnEnterRaycastedTile(MapNode node)
    {
        hasSelectedNode = true;
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<MapNode>(out MapNode node))
        {
            // Raycast Target - Reticle QOL *update this code later
            if (instance.phase == GamePhase.RaycastTargetSelection)
            {
                var p = instance.currentPlayer;
                var wm = p.currentStatsModifier.warpMode;

                if (wm == EntityStatsModifiers.WarpMode.Tiles) // Banh Mi
                {
                    reticle.color = targetedColor;
                }
                else if (wm == EntityStatsModifiers.WarpMode.Players // Banh Mi Sandwich
                    && node.playersOccupied.Count != 0)
                {
                        reticle.color = targetedColor;
                }
                else if (wm == EntityStatsModifiers.WarpMode.Marigold // Hoa Mai
                    && node.CompareTag("Store") && node.modifier == MapNode.Modifier.None)
                {
                        reticle.color = targetedColor;
                }
                else if (wm == EntityStatsModifiers.WarpMode.Rafflesia // Rafflesia duh
                    && node.modifier == MapNode.Modifier.None)
                {
                    reticle.color = targetedColor;
                }
                else 
                    reticle.color = invalidTargetColor;
            }
            else
                reticle.color = targetedColor;

            var o = collision.gameObject.transform.position + selectedNodeOffset;
            LMotion.Create(transform.position, o, 0.10f)
                .WithEase(Ease.InQuad)
                .Bind(x => transform.position = x);

            LMotion.Create(reticle.transform.localScale, Vector3.one, 0.10f)
                .WithEase(Ease.InOutBounce)
                .Bind(x => reticle.transform.localScale = x);

            m_EnterRaycastedTile.RaiseEvent(node);
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<MapNode>(out MapNode node))
        {
            if (hasSelectedNode)
            {
                Debug.Log("Exitting tile");
                m_ExitRaycastedTile.RaiseEvent();
                hasSelectedNode = false;
            }

            reticle.color = untargetedColor;

            LMotion.Create(reticle.transform.localScale, Vector3.one * .75f, 0.10f)
                .WithEase(Ease.InOutBounce)
                .Bind(x => reticle.transform.localScale = x);
        }
    }
}
