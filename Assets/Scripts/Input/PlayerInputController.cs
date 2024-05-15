using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.InputSystem;
using static GameplayTest;

public class PlayerInputController : MonoBehaviour
{
    //private PlayerInputController instance;

    [SerializeField] private EntityPiece currentPlayer;
    [SerializeField] private PlayerInput playerInput;

    [Header("Broadcast on Event Channels")]
    public VoidEventChannelSO m_EnableFreeview;
    public PlayerEventChannelSO m_DiceRollUndo;
    public PlayerEventChannelSO m_DiceRollPrep;
    public IntEventChannelSO m_RollForMovement;
    public PlayerEventChannelSO m_OpenInventory; 
    public PlayerEventChannelSO m_BuildStore; 
    public NodeEventChannelSO m_RestockStore;
    public VoidEventChannelSO m_ExitInventory;

    [Header("Listen on Event Channels")]
    public PlayerEventChannelSO m_NextPlayerTurn;

    // Start is called before the first frame update
    private void Start()
    {
        // Fuck ass work around to disable the UI action map
        playerInput.SwitchCurrentActionMap("UI"); // FUCK YOU
        playerInput.currentActionMap.Disable();
        playerInput.SwitchCurrentActionMap("Initial Turn Menu");
        playerInput.currentActionMap.Enable();
    }

    private void OnEnable()
    {
        m_NextPlayerTurn.OnEventRaised += SetCurrentPlayer;
    }

    private void OnDisable()
    {
        m_NextPlayerTurn.OnEventRaised -= SetCurrentPlayer;
    }

    #region 'Inital Turn Menu' Action Map
    private void OnView()
    {
        Debug.Log("menu item pressed as message");
        m_EnableFreeview.RaiseEvent();
    }

    private void OnRoll()
    {
        Debug.Log("menu item pressed as message");
        m_DiceRollPrep.RaiseEvent(currentPlayer);
        SwitchActionMap(GamePhase.RollDice);
    }

    private void OnInv()
    {
        Debug.Log("menu item pressed as message");
        m_OpenInventory.RaiseEvent(currentPlayer);
        SwitchActionMap(GamePhase.Inventory);
    }

    private void OnBuild()
    {
        var p = currentPlayer;
        if (p.occupiedNode.tag == "Encounter"
            && p.heldPoints >= 200 && p.storeCount < 4)
        {
            // Build a store in the current tile
            m_BuildStore.RaiseEvent(currentPlayer);
            //m_RestockStore.RaiseEvent(currentPlayer.occupiedNode);
            SwitchActionMap(GamePhase.StockStore);

        }
        else
        {
            // Give some notification
        }

    }
    #endregion

    # region 'UI' Action Map
    private void OnCancel()
    {
        Debug.Log("cancel inv as message");
        m_ExitInventory.RaiseEvent();
        SwitchActionMap(GamePhase.InitialTurnMenu);
    }

    #endregion

    #region 'Moving' Action Map
    private void OnMove(InputValue value)
    {
        Debug.Log($"{value.Get<Vector2>()}");

    }

    private void OnToggleFreeview()
    {
        m_EnableFreeview.RaiseEvent();
    }
    #endregion

    #region 'Confirmation' Action Map
    private void OnYes()
    {
        Debug.Log("Dice rolled!");
        m_RollForMovement.RaiseEvent(10);
        SwitchActionMap(GamePhase.PickDirection);
    }

    private void OnNo()
    {
        Debug.Log("undo confirm pressed");
        m_DiceRollUndo.RaiseEvent(currentPlayer);
        SwitchActionMap(GamePhase.InitialTurnMenu);
    }
    #endregion
    /*
    private void EnableFreeview(InputAction.CallbackContext context)
    {
        m_EnableFreeview.RaiseEvent();
    }
    
    public void OpenInventory(InputAction.CallbackContext context)
    {
        //SwitchActionMap(GameplayTest.GamePhase.Inventory);
        Debug.Log("menu item pressed as event");
        m_OpenInventory.RaiseEvent(currentPlayer);
        SwitchActionMap(GamePhase.Inventory);
    }

    public void ExitInventory(InputAction.CallbackContext context)
    {
        Debug.Log("exit inv pressed");
        //if (playerInput.currentActionMap.name == "UI")
        //{
            m_ExitInventory.RaiseEvent();
            SwitchActionMap(GamePhase.InitialTurnMenu);
        //}
    }

    public void PrepRoll(InputAction.CallbackContext context)
    {
        Debug.Log("prep roll pressed");
        m_DiceRollPrep.RaiseEvent(currentPlayer);
        SwitchActionMap(GamePhase.RollDice);
    }

    public void UndoPrepRoll(InputAction.CallbackContext context)
    {
        Debug.Log("undo confirm pressed");
        m_DiceRollUndo.RaiseEvent(currentPlayer);
        SwitchActionMap(GamePhase.InitialTurnMenu);
    }
    public void RollDice(InputAction.CallbackContext context)
    {
        Debug.Log("Dice rolled!");
        m_RollForMovement.RaiseEvent(10);
        SwitchActionMap(GamePhase.PickDirection);
    }

    public void Moving(InputAction.CallbackContext context)
    {
        //Debug.Log($"{context.ReadValue<Vector2>()}");
    }
    */

    public void SwitchActionMap(GameplayTest.GamePhase phase)
    {
        playerInput.currentActionMap.Disable();
        GameplayTest.instance.phase = phase;
        switch (phase)
        {
            // Pick choices
            case GamePhase.InitialTurnMenu:
                playerInput.SwitchCurrentActionMap("Initial Turn Menu");
                break;

            case GamePhase.Inventory:
                Debug.Log("yuh");
                playerInput.SwitchCurrentActionMap("UI");
                break;

            // Roll Phase 
            case GamePhase.RollDice:
                playerInput.SwitchCurrentActionMap("Confirmation");
                break;

            // Pick Direction to Go Phase
            case GamePhase.PickDirection:
                playerInput.SwitchCurrentActionMap("Moving");
                break;

            case GamePhase.StockStore:
                playerInput.SwitchCurrentActionMap("UI");
                break;

            case GamePhase.OverturnStore:
                playerInput.SwitchCurrentActionMap("Confirmation");
                break;

            case GamePhase.LevelUp:
                //LevelUp(currentPlayer);
                playerInput.SwitchCurrentActionMap("UI");
                break;

            // Confirmation Phase
            case GamePhase.ConfirmContinue:
                playerInput.SwitchCurrentActionMap("Confirmation");
                break;
        }

        playerInput.currentActionMap.Enable();
        Debug.Log(playerInput.currentActionMap);
    }

    private void SetCurrentPlayer(EntityPiece player)
    {
        currentPlayer = player;
        playerInput.SwitchCurrentActionMap("UI"); // FUCK YOU
        playerInput.currentActionMap.Disable();
        playerInput.SwitchCurrentActionMap("Initial Turn Menu");
        playerInput.currentActionMap.Enable();
    }
}
