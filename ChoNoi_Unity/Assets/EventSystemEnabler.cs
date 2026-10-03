using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class EventSystemEnabler : MonoBehaviour
{
    [SerializeField] private InputActionAsset actionAssetToUse;
    private InputSystemUIInputModule _inputSystemUIInputModule;
    private MultiplayerEventSystem _eventSystem;

    [Header("PlayerRoots")]
    [SerializeField] private GameObject overlayCanvas;
    [SerializeField] private GameObject cameraCanvas;

    [Header("Listen on Event Channels")]
    //public PlayerEventChannelSO m_NextPlayerTurn;
    public NodeEventChannelSO m_LandOnStorefront;
    public VoidEventChannelSO m_ExitStorefront;



    private void OnEnable()
    {
        //m_NextPlayerTurn.OnEventRaised += OnNextPlayerTurn;
        m_LandOnStorefront.OnEventRaised += OnLandOnStorefront;
        m_ExitStorefront.OnEventRaised += OnExitStorefront;

        _inputSystemUIInputModule.actionsAsset = actionAssetToUse;
    }

    private void OnDisable()
    {
        //m_NextPlayerTurn.OnEventRaised -= OnNextPlayerTurn;
        m_LandOnStorefront.OnEventRaised -= OnLandOnStorefront;
        m_ExitStorefront.OnEventRaised -= OnExitStorefront;
    }

    void Awake()
    {
        _inputSystemUIInputModule = GetComponent<InputSystemUIInputModule>();
        _eventSystem = GetComponent<MultiplayerEventSystem>();

        actionAssetToUse = _inputSystemUIInputModule.actionsAsset;
    }

    private void ChangePlayerRoot(GameObject o)
    {
        _eventSystem.playerRoot = o;
    }

    public void EnablePlayerCanvases()
    {
        overlayCanvas.SetActive(true);
        cameraCanvas.SetActive(true);
    }
    
    public void DisablePlayerCanvases()
    {
        overlayCanvas.SetActive(false);
        cameraCanvas.SetActive(false);
    }
    
    private void OnLandOnStorefront(MapNode node)
    {
        ChangePlayerRoot(cameraCanvas);
    }

    private void OnExitStorefront()
    {
        ChangePlayerRoot(overlayCanvas);
    }

    /*
    private void OnEnable()
    {
        StartCoroutine(Co_ActivateInputComponent());
    }

    private IEnumerator Co_ActivateInputComponent()
    {
        yield return new WaitForEndOfFrame();
        _inputSystemUIInputModule.enabled = false;
        //yield return new WaitForSeconds(0.1f);
        _inputSystemUIInputModule.enabled = true;
        _inputSystemUIInputModule.actionsAsset = actionAssetToUse;
    }
    */
}