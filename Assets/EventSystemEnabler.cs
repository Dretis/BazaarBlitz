using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class EventSystemEnabler : MonoBehaviour
{
    private InputSystemUIInputModule _inputSystemUIInputModule;
    [SerializeField] private InputActionAsset actionAssetToUse;

    void Start()
    {
        _inputSystemUIInputModule = GetComponent<InputSystemUIInputModule>();
    }

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
}