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
    [SerializeField] private GameplayTest.GamePhase previousGamePhase = GamePhase.InitialTurnMenu;

    [Header("Freeview Variables")]
    [SerializeField] private GameObject freeviewReticle;
    [SerializeField] private int freeviewSpeed;
    private Rigidbody2D freeviewRb;
    private Vector2 freeviewMoveInput;

    [Header("Broadcast on Event Channels")]
    public VoidEventChannelSO m_EnableFreeview; // also listening
    public VoidEventChannelSO m_DisableFreeview;
    public Vector2EventChannelSO m_TryExamineTile;
    public PlayerEventChannelSO m_DiceRollUndo;
    public PlayerEventChannelSO m_DiceRollPrep;
    public VoidEventChannelSO m_DiceRolled;
    public IntEventChannelSO m_RollForMovement;
    public PlayerEventChannelSO m_OpenInventory; 
    public PlayerEventChannelSO m_BuildStore; 
    public PlayerEventChannelSO m_FinishStockingStore; 
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

        freeviewRb = freeviewReticle.GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        freeviewReticle.SetActive(false);
        m_EnableFreeview.OnEventRaised += FreeviewEnabled;
        m_NextPlayerTurn.OnEventRaised += SetCurrentPlayer;
        m_RestockStore.OnEventRaised += OnRestockStore;
        m_FinishStockingStore.OnEventRaised += OnFinishStockingStore;
    }

    private void OnDisable()
    {
        m_EnableFreeview.OnEventRaised -= FreeviewEnabled;
        m_NextPlayerTurn.OnEventRaised -= SetCurrentPlayer;
        m_RestockStore.OnEventRaised -= OnRestockStore;
        m_FinishStockingStore.OnEventRaised -= OnFinishStockingStore;
    }

    private void FixedUpdate()
    {
        freeviewRb.velocity = freeviewMoveInput * freeviewSpeed; // Moving reticle during Freeview
    }

    #region 'Inital Turn Menu' Action Map
    private void OnView()
    {
        //Debug.Log("menu item pressed as message");
        previousGamePhase = GamePhase.InitialTurnMenu;
        m_EnableFreeview.RaiseEvent();
        //FreeviewEnabled(GamePhase.InitialTurnMenu);
    }

    private void OnRoll()
    {
        //Debug.Log("menu item pressed as message");
        previousGamePhase = GamePhase.InitialTurnMenu;
        m_DiceRollPrep.RaiseEvent(currentPlayer);
        SwitchActionMap(GamePhase.RollDice);
    }

    private void OnInv()
    {
        Debug.Log("menu item pressed as message");
        if (!GameplayTest.instance.playerUsedItem)
        {
            m_OpenInventory.RaiseEvent(currentPlayer);
            SwitchActionMap(GamePhase.Inventory);
        }
        else
        {
            // play some nuh-uh sound
        }
    }

    private void OnBuild()
    {
        var p = currentPlayer;
        if (p.occupiedNode.tag == "Encounter"
            && p.storeCount < 4)
        {
            previousGamePhase = GamePhase.InitialTurnMenu;
            Debug.Log("Built a store");
            // Build a store in the current tile
            m_BuildStore.RaiseEvent(currentPlayer);
            m_RestockStore.RaiseEvent(currentPlayer.occupiedNode);
            //SwitchActionMap(GamePhase.StockStore);

        }
        else
        {
            Debug.Log("Can't build store");
            // Give some notification/play some nuh-uh sound
        }

    }
    #endregion

    # region 'UI' Action Map
    private void OnCancel()
    {
        switch (GameplayTest.instance.phase)
        {
            case GamePhase.StockStore:
                m_FinishStockingStore.RaiseEvent(currentPlayer);
                break;
            case GamePhase.Inventory:
                m_ExitInventory.RaiseEvent();
                break;
        }
        //SwitchActionMap(previousGamePhase);
    }

    #endregion

    #region 'Moving' Action Map
    private void OnMove(InputValue value)
    {
        var gp = GameplayTest.instance;
        var p = currentPlayer;
        Debug.Log($"{value.Get<Vector2>()}");
        var x = value.Get<Vector2>().x;
        var y = value.Get<Vector2>().y;

        switch (x, y)
        {
            case (0, 1):
                gp.wantedNode = p.occupiedNode.north;
                break;
            case (1, 0):
                gp.wantedNode = p.occupiedNode.east;
                break;
            case (0, -1):
                gp.wantedNode = p.occupiedNode.south;
                break;
            case (-1, 0):
                gp.wantedNode = p.occupiedNode.west;
                break;

        }
        if(gp.wantedNode != null && !(gp.wantedNode == p.previousNode && p.traveledNodes.Count <= 1))
        {
            GameplayTest.instance.phase = GamePhase.MoveAround;
            // prob replace this part w/ an event call
        }
    }

    private void OnToggleFreeview()
    {
        previousGamePhase = GamePhase.PickDirection;
        m_EnableFreeview.RaiseEvent();
    }
    #endregion

    #region 'Freeview' Action Map
    private void OnFreeviewMove(InputValue value)
    {
        freeviewMoveInput = value.Get<Vector2>();
    }

    private void OnFreeviewExamine()
    {
        m_TryExamineTile.RaiseEvent((Vector2) freeviewReticle.transform.position);
    }

    private void OnFreeviewExit()
    {
        freeviewReticle.SetActive(false);
        SwitchActionMap(previousGamePhase); // Should be whatever the one it was before
        m_DisableFreeview.RaiseEvent();
        //SwitchActionMap(GamePhase.InitialTurnMenu); // Should be whatever the one it was before
    }
    #endregion

    #region 'Confirmation' Action Map
    private void OnYes()
    {
        switch (GameplayTest.instance.phase)
        {
            case GamePhase.RollDice:
                //m_DiceRolled.RaiseEvent(); // Player proceeds with rolling the dice to move
                m_RollForMovement.RaiseEvent(1);
                currentPlayer.movementTotal = currentPlayer.movementLeft = 1;
                SwitchActionMap(GamePhase.PickDirection);
                break;
            case GamePhase.ConfirmContinue:
                Debug.Log("confirm to continued!");
                GameplayTest.instance.ConfirmContinue();
                break;
        }
    }

    private void OnNo()
    {
        switch (GameplayTest.instance.phase)
        {
            case GamePhase.RollDice:
                Debug.Log("undo confirm pressed");
                m_DiceRollUndo.RaiseEvent(currentPlayer);
                SwitchActionMap(GamePhase.InitialTurnMenu);
                break;
            case GamePhase.ConfirmContinue:
                GameplayTest.instance.ConfirmContinue();
                break;
        }
    }
    #endregion

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

            case GamePhase.Freeview:
                playerInput.SwitchCurrentActionMap("Freeview");
                break;

            case GamePhase.Inventory:
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

    private void FreeviewEnabled()
    {
        freeviewReticle.SetActive(true);
        freeviewReticle.transform.position = currentPlayer.transform.position + new Vector3(0, 0.5f,0);
        SwitchActionMap(GamePhase.Freeview);
    }

    private void OnRestockStore(MapNode node)
    {
        previousGamePhase = instance.phase;
        Debug.Log("prev phase = "+previousGamePhase);
        SwitchActionMap(GamePhase.StockStore);
    }

    private void OnFinishStockingStore(EntityPiece ps)
    {
        if(previousGamePhase == GamePhase.EncounterTime)
        {
            Debug.Log("What the fc");
            instance.phase = GamePhase.EndTurn;
            return;
        }
        SwitchActionMap(previousGamePhase);
    }
}
