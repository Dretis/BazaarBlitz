using UnityEngine;
using Cinemachine;
using static UnityEngine.EventSystems.EventTrigger;

public class CameraObserver : MonoBehaviour
{
    //[SerializeField] private CinemachineComposer composer;
    private EntityPiece currentPlayer;

    [SerializeField] private float minOrthDistance = 1.04f;
    [SerializeField] private float maxOrthDistance = 5.04f;
    [SerializeField] private Vector3 defaultFollowOffset = new Vector3(0, 0.5f, 0);
    [SerializeField] private GameObject freeviewReticle;
    [SerializeField] private float freeviewSpeed = 6;

    private Vector3 selectedNodeOffset = new Vector3(0, 0.25f, 0);
    private Rigidbody2D freeviewRb;

    private CinemachineVirtualCamera vcam;

    [Header("Extra Cameras")]
    [SerializeField] private CinemachineVirtualCamera zoomedInCam;
    [SerializeField] private CinemachineVirtualCamera levelUpCam;
    [SerializeField] private CinemachineVirtualCamera combatZoomCam;

    [Header("Listen on Event Channels")]
    public PlayerEventChannelSO m_NextPlayerTurn;
    public VoidEventChannelSO m_EnableFreeview;
    public VoidEventChannelSO m_DisableFreeview;

    public NodeEventChannelSO m_EnterRaycastedTile;
    public VoidEventChannelSO m_ExitRaycastedTile;

    public Vector2EventChannelSO m_FreeviewReticleMove;

    public IntEventChannelSO m_RollForMovement;
    public PlayerEventChannelSO m_DiceRollPrep;
    public PlayerEventChannelSO m_DiceRollUndo;

    public PlayerEventChannelSO m_EnterLevelUp;
    public VoidEventChannelSO m_ExitLevelUp;

    public VoidEventChannelSO m_EnteredCombatScene;

    private void OnEnable()
    {
        freeviewReticle.SetActive(false);
        m_NextPlayerTurn.OnEventRaised += SwitchTargetFocus;
        m_EnableFreeview.OnEventRaised += SwitchFocusToReticle;
        m_DisableFreeview.OnEventRaised += ReturnTargetFocusToCurrentPlayer;
        m_FreeviewReticleMove.OnEventRaised += OnFreeviewReticleMove;

        m_RollForMovement.OnEventRaised += OnRollForMovement;
        m_DiceRollPrep.OnEventRaised += OnDiceRollPrep;
        m_DiceRollUndo.OnEventRaised += OnDiceRollUndo;

        m_EnterLevelUp.OnEventRaised += OnEnterLevelUp;
        m_ExitLevelUp.OnEventRaised += OnExitLevelUp;
        m_EnterRaycastedTile.OnEventRaised += OnEnterRaycastedTile;
        m_ExitRaycastedTile.OnEventRaised += OnExitRaycastedTile;

        m_EnteredCombatScene.OnEventRaised += OnEnteredCombatScene;
    }

    private void OnDisable()
    {
        m_NextPlayerTurn.OnEventRaised -= SwitchTargetFocus;
        m_EnableFreeview.OnEventRaised -= SwitchFocusToReticle;
        m_DisableFreeview.OnEventRaised -= ReturnTargetFocusToCurrentPlayer;
        m_FreeviewReticleMove.OnEventRaised -= OnFreeviewReticleMove;

        m_RollForMovement.OnEventRaised -= OnRollForMovement;
        m_DiceRollPrep.OnEventRaised -= OnDiceRollPrep;
        m_DiceRollUndo.OnEventRaised -= OnDiceRollUndo;

        m_EnterLevelUp.OnEventRaised -= OnEnterLevelUp;
        m_ExitLevelUp.OnEventRaised -= OnExitLevelUp;
        m_EnterRaycastedTile.OnEventRaised -= OnEnterRaycastedTile;
        m_ExitRaycastedTile.OnEventRaised -= OnExitRaycastedTile;

        m_EnteredCombatScene.OnEventRaised -= OnEnteredCombatScene;
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
        zoomedInCam.enabled = false;
        combatZoomCam.enabled = false;

        currentPlayer = entity;
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

    void OnEnterRaycastedTile(MapNode node)
    {
        freeviewReticle.transform.position = node.transform.position + selectedNodeOffset;
    }

    void OnExitRaycastedTile()
    {
        // follow the freeview reticle again after de-selecting tile
        vcam.Follow = freeviewReticle.transform;
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


    private void OnRollForMovement(int roll)
    {
        zoomedInCam.enabled = false;
    }

    private void OnDiceRollPrep(EntityPiece player)
    {
        zoomedInCam.Follow = player.transform;

        zoomedInCam.enabled = true;
    }

    private void OnDiceRollUndo(EntityPiece player)
    {
        zoomedInCam.enabled = false;
    }

    private void OnEnterLevelUp(EntityPiece player)
    {
        // Activate zoomed offset level up camera

        levelUpCam.Follow = player.transform;

        levelUpCam.enabled = true;
    }

    private void OnExitLevelUp()
    {
        // Deactivate zoomed offset level up camera, go back to normal
        levelUpCam.enabled = false;
    }

    private void OnEnteredCombatScene()
    {
        combatZoomCam.Follow = currentPlayer.transform;
        combatZoomCam.enabled = true;
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
