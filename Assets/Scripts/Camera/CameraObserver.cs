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
    [SerializeField] private float freeviewSpeed = 6;
    private Rigidbody2D freeviewRb;

    [Header("Listen on Event Channels")]
    public PlayerEventChannelSO m_NextPlayerTurn;
    public VoidEventChannelSO m_EnableFreeview;
    public VoidEventChannelSO m_DisableFreeview;
    public VoidEventChannelSO m_ExitRaycastedTile;
    public Vector2EventChannelSO m_FreeviewReticleMove;

    private void OnEnable()
    {
        freeviewReticle.SetActive(false);
        m_NextPlayerTurn.OnEventRaised += SwitchTargetFocus;
        m_EnableFreeview.OnEventRaised += SwitchFocusToReticle;
        m_DisableFreeview.OnEventRaised += ReturnTargetFocusToCurrentPlayer;
        m_FreeviewReticleMove.OnEventRaised += OnFreeviewReticleMove;
        //m_ExitRaycastedTile.OnEventRaised += SwitchFocusToReticle;
    }

    private void OnDisable()
    {
        m_NextPlayerTurn.OnEventRaised -= SwitchTargetFocus;
        m_EnableFreeview.OnEventRaised -= SwitchFocusToReticle;
        m_DisableFreeview.OnEventRaised -= ReturnTargetFocusToCurrentPlayer;
        m_FreeviewReticleMove.OnEventRaised -= OnFreeviewReticleMove;
        //m_ExitRaycastedTile.OnEventRaised -= SwitchFocusToReticle;
    }

    // Start is called before the first frame update
    void Awake()
    {
        freeviewRb = freeviewReticle.GetComponent<Rigidbody2D>();
        vcam = GetComponentInChildren<CinemachineVirtualCamera>();
        //composer = vcam.GetCinemachineComponent<CinemachineComposer>();
    }

    void SwitchTargetFocus(EntityPiece entity)
    {
        vcam.Follow = entity.transform;
        freeviewReticle.SetActive(false);
        //var composer = vcam.GetCinemachineComponent<CinemachineComposer>();
        //composer.m_TrackedObjectOffset = new Vector3(0, 0.5f, 0);
        //composer.m_TrackedObjectOffset = defaultFollowOffset;

    }

    void SwitchFocusToReticle()
    {
        Debug.Log("focus on reticle");
        freeviewReticle.SetActive(true);
        freeviewReticle.transform.position = GameplayTest.instance.currentPlayer.transform.position + new Vector3(0, 0.5f, 0);
        vcam.Follow = freeviewReticle.transform;
        //var composer = vcam.GetCinemachineComponent<CinemachineComposer>();
        //composer.m_TrackedObjectOffset = Vector3.zero;
    }

    void ReturnTargetFocusToCurrentPlayer()
    {
        Debug.Log("Back to player focus");
        freeviewReticle.SetActive(false);
        vcam.Follow = GameplayTest.instance.currentPlayer.transform;
    }

    void OnFreeviewReticleMove(Vector2 moveInput)
    {
        freeviewRb.velocity = moveInput * freeviewSpeed; // Moving reticle during Freeview
    }
    /*
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
    */
}
