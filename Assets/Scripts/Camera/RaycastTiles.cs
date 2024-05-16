using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastTiles : MonoBehaviour
{
    Camera cam;
    private bool tileSelected = false;
    private bool freeviewEnabled = false;

    [Header("Broadcast on Event Channels")]
    public NodeEventChannelSO m_EnterRaycastedTile;
    public VoidEventChannelSO m_ExitRaycastedTile;


    [Header("Listen on Event Channels")]
    public VoidEventChannelSO m_EnableFreeview;
    public VoidEventChannelSO m_DisableFreeview;

    private void OnEnable()
    {
        m_EnableFreeview.OnEventRaised += EnableRaycasting;
    }

    private void OnDisable()
    {
        m_EnableFreeview.OnEventRaised -= EnableRaycasting;
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
                    var node = hit.transform.GetComponent<MapNode>();

                    tileSelected = true;
                    m_EnterRaycastedTile.RaiseEvent(node);
                }
                else
                {
                    m_ExitRaycastedTile.RaiseEvent();
                    tileSelected = false;
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                m_ExitRaycastedTile.RaiseEvent();
                tileSelected = false;
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
        freeviewEnabled = false;
    }
}
