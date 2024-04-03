using UnityEngine;
using Cinemachine;

public class CameraObserver : MonoBehaviour
{
    private CinemachineVirtualCamera vcam;
    [SerializeField] private float minOrthDistance = 1.04f;
    [SerializeField] private float maxOrthDistance = 5.04f;

    [Header("Listen on Event Channels")]
    public PlayerEventChannelSO m_NextPlayerTurn;
    // public PlayerEventChannelSO m_FreeLook; // For when the player wants to look around the map via the menu option

    private void OnEnable()
    {
        m_NextPlayerTurn.OnEventRaised += SwitchTargetFocus;
    }

    private void OnDisable()
    {
        m_NextPlayerTurn.OnEventRaised -= SwitchTargetFocus;
    }

    // Start is called before the first frame update
    void Start()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
    }

    void SwitchTargetFocus(EntityPiece entity)
    {
        vcam.Follow = entity.transform;
    }

    private void LateUpdate()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            vcam.m_Lens.OrthographicSize += 0.1f;
            if (vcam.m_Lens.OrthographicSize >= maxOrthDistance)
            {
                vcam.m_Lens.OrthographicSize = maxOrthDistance;
            }
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            vcam.m_Lens.OrthographicSize -= 0.1f;
            if (vcam.m_Lens.OrthographicSize <= minOrthDistance)
            {
                vcam.m_Lens.OrthographicSize = minOrthDistance;
            }
        }
    }
}
