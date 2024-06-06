using UnityEngine;
using Cinemachine;

public class CameraObserver : MonoBehaviour
{
    private CinemachineVirtualCamera vcam;
    //[SerializeField] private CinemachineComposer composer;

    [SerializeField] private float minOrthDistance = 1.04f;
    [SerializeField] private float maxOrthDistance = 5.04f;
    [SerializeField] private Vector3 defaultFollowOffset = new Vector3(0, 0.5f, 0);
    [SerializeField] private GameObject freeviewReticle;

    [Header("Listen on Event Channels")]
    public PlayerEventChannelSO m_NextPlayerTurn;
    public VoidEventChannelSO m_EnableFreeview;
    public VoidEventChannelSO m_DisableFreeview;
    // public PlayerEventChannelSO m_FreeLook; // For when the player wants to look around the map via the menu option

    private void OnEnable()
    {
        m_NextPlayerTurn.OnEventRaised += SwitchTargetFocus;
        m_EnableFreeview.OnEventRaised += SwitchFocusToReticle;
        m_DisableFreeview.OnEventRaised += ReturnTargetFocusToCurrentPlayer;
    }

    private void OnDisable()
    {
        m_NextPlayerTurn.OnEventRaised -= SwitchTargetFocus;
        m_EnableFreeview.OnEventRaised -= SwitchFocusToReticle;
        m_DisableFreeview.OnEventRaised -= ReturnTargetFocusToCurrentPlayer;
    }

    // Start is called before the first frame update
    void Awake()
    {
        vcam = GetComponentInChildren<CinemachineVirtualCamera>();
        //composer = vcam.GetCinemachineComponent<CinemachineComposer>();
    }

    void SwitchTargetFocus(EntityPiece entity)
    {
        vcam.Follow = entity.transform;
        //var composer = vcam.GetCinemachineComponent<CinemachineComposer>();
        //composer.m_TrackedObjectOffset = new Vector3(0, 0.5f, 0);
        //composer.m_TrackedObjectOffset = defaultFollowOffset;
        
    }

    void SwitchFocusToReticle()
    {
        vcam.Follow = freeviewReticle.transform;
        //var composer = vcam.GetCinemachineComponent<CinemachineComposer>();
        //composer.m_TrackedObjectOffset = Vector3.zero;
    }

    void ReturnTargetFocusToCurrentPlayer()
    {
        vcam.Follow = GameplayTest.instance.currentPlayer.transform;
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
