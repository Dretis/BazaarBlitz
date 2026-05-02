using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.XInput;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Switch;
using UnityEngine.EventSystems;

public class MenuInputController : MonoBehaviour
{
    [SerializeField] private PlayerInputController.CurrentDevice device;
    [SerializeField] private PlayerInput playerInput;

    [Header("Broadcast on Event Channels")]
    public VoidEventChannelSO m_DoSomethingTest;
    // Start is called before the first frame update
    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    # region 'UI' Action Map
    private void OnCancel()
    {
        Debug.Log($"Menu | Cancel pressed");
        if(UIScreenManager.instance != null)
            UIScreenManager.instance.GoBack();
    }
    #endregion

    private void OnControlsChanged()
    {
        // Figure out what kind of Gamepad this Player has
        if (playerInput.currentControlScheme == "Gamepad")
        {
            var gamepad = playerInput.GetDevice<Gamepad>();
            //Debug.Log($"Player[{playerInput.playerIndex}] Gamepad: {gamepad}");
            //Debug.Log($"Player [{playerInput.playerIndex}] Device: " + playerInput.GetDevice<Gamepad>());
            if (gamepad is DualShockGamepad)
            {
                Debug.Log($"Player is using PS");
                device = PlayerInputController.CurrentDevice.PS;
            }
            else if (gamepad is XInputController)
            {
                Debug.Log($"Player is using XBOX");
                device = PlayerInputController.CurrentDevice.Xbox;
            }
            else if (gamepad is SwitchProControllerHID)
            {
                Debug.Log($"Player is using Switch Pro");
                device = PlayerInputController.CurrentDevice.Switch;
            }
            else
            {
                Debug.Log($"Heck bro, Player is none of the above");
                device = PlayerInputController.CurrentDevice.Xbox; // just set it to XBOX as default
            }
        }
        else
        {
            // It's a Keyboard
            Debug.Log($"Player[{playerInput.playerIndex}] is not using gamepad.");
            device = PlayerInputController.CurrentDevice.Keyboard;
        }
    }
}
