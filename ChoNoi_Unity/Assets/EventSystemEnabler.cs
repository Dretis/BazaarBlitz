using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class EventSystemEnabler : MonoBehaviour
{
    //[SerializeField] private InputActionAsset actionAssetToUse;
    private InputSystemUIInputModule _inputSystemUIInputModule;
    private MultiplayerEventSystem _eventSystem;

    [Header("PlayerRoots")]
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private GameObject storefrontCanvas;

    [Header("Listen on Event Channels")]
    public NodeEventChannelSO m_LandOnStorefront;
    public VoidEventChannelSO m_ExitStorefront;

    private void OnEnable()
    {
        m_LandOnStorefront.OnEventRaised += OnLandOnStorefront;
        m_ExitStorefront.OnEventRaised += OnExitStorefront;
    }

    private void OnDisable()
    {
        m_LandOnStorefront.OnEventRaised -= OnLandOnStorefront;
        m_ExitStorefront.OnEventRaised -= OnExitStorefront;
    }

    void Start()
    {
        _inputSystemUIInputModule = GetComponent<InputSystemUIInputModule>();
        _eventSystem = GetComponent<MultiplayerEventSystem>();
    }

    private void ChangePlayerRoot(GameObject o)
    {
        _eventSystem.playerRoot = o;
    }

    private void OnLandOnStorefront(MapNode node)
    {
        ChangePlayerRoot(storefrontCanvas);
    }

    private void OnExitStorefront()
    {
        ChangePlayerRoot(mainCanvas);
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