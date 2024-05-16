using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastTiles : MonoBehaviour
{
    Camera cam;
    private bool isTileSelected = false;
    private bool freeviewEnabled = false;
    private bool mustSelectTarget = false;

    public static MapNode tileSelected = null;

    [Header("Broadcast on Event Channels")]
    public NodeEventChannelSO m_EnterRaycastedTile;
    public VoidEventChannelSO m_ExitRaycastedTile;


    [Header("Listen on Event Channels")]
    public VoidEventChannelSO m_EnableFreeview;
    public VoidEventChannelSO m_DisableFreeview;
    public VoidEventChannelSO m_EnterRaycastTargetSelection;
    public VoidEventChannelSO m_ExitRaycastTargetSelection;

    private void OnEnable()
    {
        m_EnableFreeview.OnEventRaised += EnableRaycasting;
        m_DisableFreeview.OnEventRaised += DisableRaycasting;
        m_EnterRaycastTargetSelection.OnEventRaised += EnableMustSelectTarget;
        m_ExitRaycastTargetSelection.OnEventRaised += DisableMustSelectTarget;
    }

    private void OnDisable()
    {
        m_EnableFreeview.OnEventRaised -= EnableRaycasting;
        m_DisableFreeview.OnEventRaised -= DisableRaycasting;
        m_EnterRaycastTargetSelection.OnEventRaised -= EnableMustSelectTarget;
        m_ExitRaycastTargetSelection.OnEventRaised -= DisableMustSelectTarget;
    }

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (freeviewEnabled)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("click");
                Vector2 rayPos = new Vector2(cam.ScreenToWorldPoint(Input.mousePosition).x, cam.ScreenToWorldPoint(Input.mousePosition).y);
                RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0f);

                if (hit)
                {
                    Debug.Log($"hit {hit.transform.name}");
                    tileSelected = hit.transform.GetComponent<MapNode>();

                    isTileSelected = true;
                    m_EnterRaycastedTile.RaiseEvent(tileSelected);
                }
                else
                {
                    m_ExitRaycastedTile.RaiseEvent();
                    tileSelected = null;
                    isTileSelected = false;
                }
            }
            else if (!mustSelectTarget && Input.GetMouseButtonDown(1))
            {
               
                isTileSelected = false;
                m_DisableFreeview.RaiseEvent();
            }
        }
    }

    public void EnableRaycasting()
    {
        freeviewEnabled = true;
    }

    public void DisableRaycasting()
    {
        m_ExitRaycastedTile.RaiseEvent();
        freeviewEnabled = false;
    }

    public void EnableMustSelectTarget()
    {
        mustSelectTarget = true;
    }

    public void DisableMustSelectTarget()
    {
        mustSelectTarget = false;
    }
}
