using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem.UI;
using static PlayerInputController;
using UnityEngine.InputSystem.XInput;

public class SpawnPlayerSetupMenu : MonoBehaviour
{
    public GameObject playerSetupMenuPrefab;
    public PlayerInput input;

    private void Awake()
    {
        var rootMenu = GameObject.Find("Canvas - Main Layout");
        if(rootMenu != null)
        {
            var menu = Instantiate(playerSetupMenuPrefab, rootMenu.transform);
            input.uiInputModule = menu.GetComponentInChildren<InputSystemUIInputModule>();
            menu.GetComponent<PlayerSetupMenuController>().SetPlayerIndex(input.playerIndex);

            var pad = input.GetDevice<Gamepad>();
            var device = CurrentDevice.Keyboard;

            if(pad != null)
            {
                if (pad is DualShockGamepad)
                {
                    //Debug.Log($"Player[{playerInput.playerIndex}] is using PS");
                    device = CurrentDevice.PS;
                }
                else if (pad is XInputController)
                {
                    //Debug.Log($"Player[{playerInput.playerIndex}] is using XBOX");
                    device = CurrentDevice.Xbox;
                }
                else if (pad is SwitchProControllerHID)
                {
                    //Debug.Log($"Player[{playerInput.playerIndex}] is using Switch Pro");
                    device = CurrentDevice.Switch;
                }
            }

            menu.GetComponent<PlayerSetupMenuController>().SetDevice(device);
        }
    }
}
