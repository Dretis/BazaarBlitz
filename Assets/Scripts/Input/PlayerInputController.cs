using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.XInput;
using UnityEngine.InputSystem.DualShock;
using static GameplayTest;
using UnityEngine.InputSystem.Switch;

public class PlayerInputController : MonoBehaviour
{
    public enum CurrentDevice
    {
        Keyboard,
        Xbox,
        PS,
        Switch
    }
    //private PlayerInputController instance;
    //[SerializeField] private List<PlayerConfiguration> playerConfigs;
    private PlayerInputActions playerInputActions; 

    [SerializeField] private CurrentDevice device;
    [SerializeField] private EntityPiece currentPlayer;
    [SerializeField] private EntityPiece assignedPlayer;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameplayTest.GamePhase previousGamePhase = GamePhase.InitialTurnMenu;

    [SerializeField] private InputActionMap currActionMap;

    [Header("Freeview Variables")]
    [SerializeField] private GameObject freeviewReticle;
    [SerializeField] private int freeviewSpeed;
    private Rigidbody2D freeviewRb;
    private Vector2 freeviewMoveInput;

    [Header("Broadcast on Event Channels")]
    public VoidEventChannelSO m_EnableFreeview; // also listening
    public VoidEventChannelSO m_DisableFreeview;
    public Vector2EventChannelSO m_TryExamineTile;
    public Vector2EventChannelSO m_FreeviewReticleMove;

    public PlayerEventChannelSO m_DiceRollUndo;
    public PlayerEventChannelSO m_DiceRollPrep;
    public PlayerEventChannelSO m_TryDiceRollPrep;

    public VoidEventChannelSO m_DiceRolled;
    public IntEventChannelSO m_RollForMovement;
    public PlayerEventChannelSO m_OpenInventory;

    public PlayerEventChannelSO m_TryBuildStore;
    public PlayerEventChannelSO m_BuildStore; 
    public PlayerEventChannelSO m_FinishStockingStore;
    public VoidEventChannelSO m_CancelRestockStore; // also listening
    public NodeEventChannelSO m_RestockStore; // also listening

    public VoidEventChannelSO m_ExitInventory; // also listening
    public VoidEventChannelSO m_ExitLevelUp;

    public VoidEventChannelSO m_HoldPlayerInfo;
    public VoidEventChannelSO m_ReleasePlayerInfo;

    public PlayerEventChannelSO m_BuyFromVendor;
    public VoidEventChannelSO m_ExitVendor;

    public EntityIntEventChannelSO m_PlayerActionSelected;
    public PlayerEventChannelSO m_CombatDiceRolled;

    [Header("Listen on Event Channels")]
    public PlayerEventChannelSO m_AssignPlayerToController;
    public PlayerEventChannelSO m_NextPlayerTurn;
    public IntEventChannelSO m_NextTurnRound;

    // Store-based Event Channels
    public VoidEventChannelSO m_CancelBuildStore;
    public PlayerEventChannelSO m_AskRestockStore;
    public PlayerEventChannelSO m_TryRestockStore;

    public NodeEventChannelSO m_LandOnStorefront;
    public VoidEventChannelSO m_ExitStorefront;
    public ItemEventChannelSO m_ItemBought;

    public PlayerEventChannelSO m_FullInventory;

    public NodeEventChannelSO m_LandOnVendor;

    public PlayerListEventChannelSO m_LandOnMultipleEntities;

    public VoidEventChannelSO m_EnteredCombatScene;
    public PlayerEventChannelSO m_InitiateCombatOnPassBy;

    public PlayerEventChannelSO m_EnterLevelUp;

    public EntityActionEventChannelSO m_BothActionsSelected;
    public PlayerEventChannelSO m_SwapPhase;
    //public PlayerEventChannelSO m_CombatDiceRolled;

    public PlayerEventChannelSO m_PlayerWon;

    public void InitializePlayer(PlayerConfiguration pc)
    {
        //playerConfig = pc;
        // set color palette for player based on player config here
        //playerConfig.Input.onActionTriggered
    }

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();

        Debug.Log(playerInput.currentActionMap);
        currActionMap = playerInput.currentActionMap;
    }

    private void OnEnable()
    {
        m_DiceRollPrep.OnEventRaised += OnDiceRollPrep;

        m_EnableFreeview.OnEventRaised += FreeviewEnabled;
        m_DisableFreeview.OnEventRaised += FreeviewDisabled;

        m_NextPlayerTurn.OnEventRaised += SetCurrentPlayer;
        m_NextTurnRound.OnEventRaised += OnNextTurnRound;

        m_CancelBuildStore.OnEventRaised += OnCancelBuildStore;

        m_AskRestockStore.OnEventRaised += OnAskRestockStore;
        m_TryRestockStore.OnEventRaised += OnTryRestockStore;

        m_CancelRestockStore.OnEventRaised += OnCancelRestockStore;
        m_RestockStore.OnEventRaised += OnRestockStore;
        m_FinishStockingStore.OnEventRaised += OnFinishStockingStore;

        m_LandOnStorefront.OnEventRaised += OnLandOnStorefront;
        m_ExitStorefront.OnEventRaised += OnExitStorefront;
        m_ItemBought.OnEventRaised += OnItemBought;

        m_LandOnVendor.OnEventRaised += OnLandOnVendor;

        m_LandOnMultipleEntities.OnEventRaised += OnLandOnMultipleEntities;

        m_EnteredCombatScene.OnEventRaised += OnEnteredCombatScene;
        m_InitiateCombatOnPassBy.OnEventRaised += OnInitiateCombatOnPassBy;

        m_AssignPlayerToController.OnEventRaised += OnAssignPlayerToController;

        m_EnterLevelUp.OnEventRaised += OnEnterLevelUp;
        m_ExitLevelUp.OnEventRaised += OnExitLevelUp;

        m_ExitInventory.OnEventRaised += OnExitInventory;
        m_FullInventory.OnEventRaised += OnFullInventory;

        m_BothActionsSelected.OnEventRaised += OnBothActionsSelected;
        m_SwapPhase.OnEventRaised += OnSwapPhase;
        
        m_PlayerWon.OnEventRaised += OnPlayerWon;
    }

    private void OnDisable()
    {
        m_DiceRollPrep.OnEventRaised -= OnDiceRollPrep;

        m_EnableFreeview.OnEventRaised -= FreeviewEnabled;
        m_DisableFreeview.OnEventRaised -= FreeviewDisabled;

        m_NextPlayerTurn.OnEventRaised -= SetCurrentPlayer;
        m_NextTurnRound.OnEventRaised -= OnNextTurnRound;

        m_CancelBuildStore.OnEventRaised -= OnCancelBuildStore;

        m_AskRestockStore.OnEventRaised -= OnAskRestockStore;
        m_TryRestockStore.OnEventRaised -= OnTryRestockStore;

        m_CancelRestockStore.OnEventRaised -= OnCancelRestockStore;
        m_RestockStore.OnEventRaised -= OnRestockStore;
        m_FinishStockingStore.OnEventRaised -= OnFinishStockingStore;

        m_LandOnStorefront.OnEventRaised -= OnLandOnStorefront;
        m_ExitStorefront.OnEventRaised -= OnExitStorefront;
        m_ItemBought.OnEventRaised -= OnItemBought;

        m_LandOnVendor.OnEventRaised -= OnLandOnVendor;

        m_LandOnMultipleEntities.OnEventRaised -= OnLandOnMultipleEntities;

        m_EnteredCombatScene.OnEventRaised -= OnEnteredCombatScene;
        m_InitiateCombatOnPassBy.OnEventRaised -= OnInitiateCombatOnPassBy;

        m_AssignPlayerToController.OnEventRaised -= OnAssignPlayerToController;

        m_EnterLevelUp.OnEventRaised -= OnEnterLevelUp;
        m_ExitLevelUp.OnEventRaised -= OnExitLevelUp;

        m_ExitInventory.OnEventRaised -= OnExitInventory;
        m_FullInventory.OnEventRaised -= OnFullInventory;

        m_BothActionsSelected.OnEventRaised -= OnBothActionsSelected;
        m_SwapPhase.OnEventRaised -= OnSwapPhase;

        m_PlayerWon.OnEventRaised -= OnPlayerWon;
    }

    private void FixedUpdate()
    {
        //freeviewRb.velocity = freeviewMoveInput * freeviewSpeed; // Moving reticle during Freeview
    }

    #region 'Inital Turn Menu' Action Map
    private void OnView()
    {
        Debug.Log("menu item pressed as message");
        previousGamePhase = GamePhase.InitialTurnMenu;
        m_EnableFreeview.RaiseEvent();
        //FreeviewEnabled(GamePhase.InitialTurnMenu);
    }

    private void OnRoll()
    {
        Debug.Log("roll pressed as message");
        previousGamePhase = GamePhase.InitialTurnMenu;
        m_TryDiceRollPrep.RaiseEvent(currentPlayer);
        //SwitchActionMap(GamePhase.RollDice);
    }

    private void OnInv()
    {
        Debug.Log("inventorty pressed as message");
        if (!GameplayTest.instance.playerUsedItem)
        {
            playerInput.uiInputModule = FindObjectOfType<InputSystemUIInputModule>();
            playerInput.uiInputModule.actionsAsset = playerInput.actions;

            previousGamePhase = GamePhase.InitialTurnMenu;

            SwitchActionMap(GamePhase.Inventory);
            m_OpenInventory.RaiseEvent(currentPlayer);
        }
        else
        {
            // play some nuh-uh sound
        }
    }

    private void OnBuild()
    {
        Debug.Log("Build presed");
        var p = currentPlayer;
        if (p.occupiedNode.tag == "Encounter"
            && p.storeCount < 4)
        {
            playerInput.uiInputModule = FindObjectOfType<InputSystemUIInputModule>();
            playerInput.uiInputModule.actionsAsset = playerInput.actions;

            previousGamePhase = GamePhase.InitialTurnMenu;

            m_TryBuildStore.RaiseEvent(currentPlayer);
            SwitchActionMap(GamePhase.BuildingStore);
            //Debug.Log("Built a store");
            // Build a store in the current tile
            //m_BuildStore.RaiseEvent(currentPlayer);
            //m_RestockStore.RaiseEvent(currentPlayer.occupiedNode);
            //SwitchActionMap(GamePhase.StockStore);

        }
        else
        {
            Debug.Log("Can't build store");
            // Give some notification/play some nuh-uh sound
        }

    }

    // This function is used in the following Action Maps (Moving, Freeview)
    private void OnInfo(InputValue value)
    {
        var hold = value.isPressed;
        if (hold)
        {
            // Lift up scoreboard to show additional info
            m_HoldPlayerInfo.RaiseEvent();
        }
        else
        {
            // Bring scoreboard back down to normal
            m_ReleasePlayerInfo.RaiseEvent();
        }
    }
    #endregion

    # region 'UI' Action Map
    private void OnCancel()
    {
        if (GameplayTest.instance == null) return;

        switch (GameplayTest.instance.phase)
        {
            case GamePhase.StockStore:
                m_FinishStockingStore.RaiseEvent(currentPlayer);
                break;
            case GamePhase.Inventory:
                m_ExitInventory.RaiseEvent();
                break;
            case GamePhase.PreStockStore:
                m_CancelRestockStore.RaiseEvent();
                break;
            case GamePhase.BuildingStore:
                m_CancelBuildStore.RaiseEvent();
                break;
            case GamePhase.LevelUp:
                m_ExitLevelUp.RaiseEvent();
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

        var x = value.Get<Vector2>().x;
        var y = value.Get<Vector2>().y;

        float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

        Debug.Log($"MOVING | {value.Get<Vector2>()} | Angle: {angle}\'");
        // Angles | 0 = Right, 90 = Up, 180 = Left, -90 = Down

        if (angle <= 135 && angle >= 45)
        {
            //Debug.Log("MOVE UP/NORTH");
            gp.wantedNode = p.occupiedNode.north;
        }
        else if (angle < 45 && angle > -45 && x != 0)
        {
            //Debug.Log("MOVE RIGHT/EAST");
            gp.wantedNode = p.occupiedNode.east;
            currentPlayer.playerSprite.flipX = true;
        }
        else if (angle <= -45 && angle >= -135)
        {
            //Debug.Log("MOVE DOWN/SOUTH");
            gp.wantedNode = p.occupiedNode.south;
        }
        else if (angle < -135 || angle > 135)
        {
            //Debug.Log("MOVE LEFT/WEST");
            gp.wantedNode = p.occupiedNode.west;
            currentPlayer.playerSprite.flipX = false;
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
        m_FreeviewReticleMove.RaiseEvent(freeviewMoveInput);
    }

    private void OnFreeviewExamine()
    {
        m_TryExamineTile.RaiseEvent(GameObject.Find("Freeview Reticle").transform.position); //change this code later
        if (GameplayTest.instance.phase == GamePhase.RaycastTargetSelection)
        {
            GameplayTest.instance.OnSelectRaycastTarget();
        }
    }

    private void OnFreeviewExit()
    {
        //SwitchActionMap(previousGamePhase);
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
                m_DiceRolled.RaiseEvent(); // Player proceeds with rolling the dice to move
                //m_RollForMovement.RaiseEvent(33);
                //currentPlayer.movementTotal = currentPlayer.movementLeft = 33;
                SwitchActionMap(GamePhase.PickDirection);
                break;
            case GamePhase.InVendor:
                Debug.Log($"{assignedPlayer} buys from vendor");
                m_BuyFromVendor.RaiseEvent(assignedPlayer);
                //GameplayTest.instance.phase = GamePhase.EndTurn;
                GameplayTest.instance.phase = GamePhase.ConfirmContinue;
                break;
            case GamePhase.ConfirmContinue:
                Debug.Log("confirm to continued!");
                GameplayTest.instance.ConfirmContinue();
                break;
            case GamePhase.GameOver:
                Debug.Log("YES | Game Finished, go to results scene?");
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
            case GamePhase.InVendor:
                Debug.Log($"{assignedPlayer} leaves the vendor");
                m_ExitVendor.RaiseEvent();
                GameplayTest.instance.phase = GamePhase.EndTurn;
                break;
            case GamePhase.ConfirmContinue:
                GameplayTest.instance.ConfirmContinue();
                break;
            case GamePhase.GameOver:
                Debug.Log("NO | Game Finished, go to results scene?");
                break;
        }
    }
    #endregion

    #region 'Combat' Action Map
    private void OnUpAction()
    {
        //if()
        Debug.Log("CONFIG[" + playerInput.playerIndex + "] | " + "P1 Up action pressed");
        m_PlayerActionSelected.RaiseEvent(assignedPlayer, 1);
        TryRumbling(0.5f, .25f, .15f);
        //sendAction(true, 1); // Player 1 second element (melee)
    }

    private void OnRightAction()
    {
        Debug.Log("CONFIG["+ playerInput.playerIndex + "] | "+"P1 Right action pressed");
        m_PlayerActionSelected.RaiseEvent(assignedPlayer, 0);
        TryRumbling(0.5f, .25f, .15f);
        //sendAction(true, 0); // Player 1 first element (gun)
    }

    private void OnDownAction()
    {
        Debug.Log("CONFIG[" + playerInput.playerIndex + "] | " + "P1 Down action pressed");
        m_PlayerActionSelected.RaiseEvent(assignedPlayer, 2);
        TryRumbling(0.5f, .25f, .15f);
        //sendAction(true, 2); // Player 1 first element (magic)
    }

    private void OnUpActionP2()
    {
        Debug.Log("CONFIG[" + playerInput.playerIndex + "] | " + "P2 Upppp action pressed");
        m_PlayerActionSelected.RaiseEvent(assignedPlayer, 1);
        //sendAction(false, 1); // Player  second element (melee)
    }

    private void OnRightActionP2()
    {
        Debug.Log("CONFIG[" + playerInput.playerIndex + "] | " + "P2 Righto action pressed");
        m_PlayerActionSelected.RaiseEvent(assignedPlayer, 0);
        //sendAction(false, 0); // Player 2 first element (gun)
    }

    private void OnDownActionP2()
    {
        Debug.Log("CONFIG[" + playerInput.playerIndex + "] | " + "P2 Downo action pressed");
        m_PlayerActionSelected.RaiseEvent(assignedPlayer, 2);
        //sendAction(false, 2); // Player 2 first element (magic)
    }
    #endregion

    #region 'Combat Roll' Action Map
    private void OnCombatRoll()
    {
        Debug.Log("CONFIG[" + playerInput.playerIndex + "] | " + " Rolling combat die");
        m_CombatDiceRolled.RaiseEvent(assignedPlayer);
        playerInput.currentActionMap.Disable();
        TryRumbling(0.5f, .25f, .15f);
    }

    // This is not apart of the action map but its an event func
    private void OnBothActionsSelected(EntityPiece entity, Action action)
    {
        if (entity != assignedPlayer) return;
        //if (!playerInput.inputIsActive) return;

        // shitter switch action map w/o the phase change
        playerInput.currentActionMap.Disable();
        playerInput.SwitchCurrentActionMap("Combat Roll");

        playerInput.currentActionMap.Enable();
        currActionMap = playerInput.currentActionMap;
    }

    private void OnSwapPhase(EntityPiece entity)
    {
        //if (entity != assignedPlayer) return;
        if (!(assignedPlayer == CombatManager.Instance.attacker
            || assignedPlayer == CombatManager.Instance.defender))
            return;

        //playerInput.currentActionMap.Disable();
        playerInput.SwitchCurrentActionMap("Combat");
        playerInput.currentActionMap.Enable();
        currActionMap = playerInput.currentActionMap;
    }

    #endregion

    public void SwitchActionMap(GameplayTest.GamePhase phase)
    {
        if (!playerInput.inputIsActive) return;

        m_ReleasePlayerInfo.RaiseEvent(); //temporary fix for holding down

        playerInput.currentActionMap.Disable();
        GameplayTest.instance.phase = phase;
        switch (phase)
        {
            // Pick choices
            case GamePhase.InitialTurnMenu:
                playerInput.SwitchCurrentActionMap("Initial Turn Menu");
                break;

            case GamePhase.RaycastTargetSelection:
                playerInput.SwitchCurrentActionMap("Freeview");
                break;

            case GamePhase.Freeview:
                playerInput.SwitchCurrentActionMap("Freeview");
                break;

            case GamePhase.Inventory:
                playerInput.SwitchCurrentActionMap("UI");
                break;

            case GamePhase.DiscardItem:
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

            case GamePhase.BuildingStore:
                playerInput.SwitchCurrentActionMap("UI");
                break;

            case GamePhase.InStore:
                playerInput.SwitchCurrentActionMap("UI");
                break;

            case GamePhase.PreStockStore:
                playerInput.SwitchCurrentActionMap("UI");
                break;

            case GamePhase.StockStore:
                playerInput.SwitchCurrentActionMap("UI");
                break;

            case GamePhase.InVendor:
                playerInput.SwitchCurrentActionMap("Confirmation");
                break;

            case GamePhase.EndTurn:
               playerInput.SwitchCurrentActionMap("Combat");
               break;

            case GamePhase.LevelUp:
                //LevelUp(currentPlayer);
                playerInput.SwitchCurrentActionMap("UI");
                break;

            // Confirmation Phase
            case GamePhase.ConfirmContinue:
                playerInput.SwitchCurrentActionMap("Confirmation");
                break;

            case GamePhase.CombatSelector:
                playerInput.SwitchCurrentActionMap("UI");
                break;

            case GamePhase.CombatTime:
                playerInput.SwitchCurrentActionMap("Combat");
                break;

            case GamePhase.GameOver:
                playerInput.SwitchCurrentActionMap("Confirmation");
                break;
        }

        playerInput.currentActionMap.Enable();
        currActionMap = playerInput.currentActionMap;
        Debug.Log($"Player [{playerInput.playerIndex}] | {playerInput.currentActionMap}");
        Debug.Log($"currActionMap | {currActionMap}");
    }

    private void SetCurrentPlayer(EntityPiece player)
    {
        currentPlayer = player;

        if (assignedPlayer != currentPlayer)
        {
            playerInput.DeactivateInput();
            Debug.Log($"Player ID: {playerInput.playerIndex} deactivated input.");
        }
        else
        {
            Debug.Log($"Player ID: {playerInput.playerIndex} activated input!");
            playerInput.ActivateInput();
            playerInput.SwitchCurrentActionMap("UI"); // FUCK YOU
            playerInput.currentActionMap.Disable();
            playerInput.SwitchCurrentActionMap("Initial Turn Menu");
            playerInput.currentActionMap.Enable();

            TryRumbling(0.25f, .5f, .25f);
        }
    }

    private void OnNextTurnRound(int round)
    {
        if (assignedPlayer == currentPlayer)
        {
            playerInput.DeactivateInput();
            Debug.Log($"NextTurnRound | Player ID: {playerInput.playerIndex} deactivated input.");
        }
    }

    private void TryRumbling(float lowFreq, float highFreq, float delay)
    {
        if (playerInput.currentControlScheme == "Gamepad")
        {
            var gamepad = playerInput.GetDevice<Gamepad>();

            gamepad.SetMotorSpeeds(lowFreq, highFreq);

            StartCoroutine(StopRumbling(delay, gamepad));
        }
    }

    IEnumerator StopRumbling(float delay, Gamepad gamepad)
    {
        yield return new WaitForSeconds(delay);
        gamepad.SetMotorSpeeds(0, 0);
    }

    private void OnDiceRollPrep(EntityPiece player)
    {
        if (!playerInput.inputIsActive) return;

        SwitchActionMap(GamePhase.RollDice);
    }

    private void FreeviewEnabled()
    {
        if (!playerInput.inputIsActive) return;
        //freeviewReticle.SetActive(true);
        //freeviewReticle.transform.position = currentPlayer.transform.position + new Vector3(0, 0.5f,0);
        SwitchActionMap(GamePhase.Freeview);
    }

    private void FreeviewDisabled()
    {
        if (!playerInput.inputIsActive) return;
        //freeviewReticle.SetActive(false);
        SwitchActionMap(previousGamePhase); // Should be whatever the one it was before
    }

    private void OnCancelBuildStore()
    {
        if (!playerInput.inputIsActive) return;

        SwitchActionMap(previousGamePhase);
    }

    private void OnAskRestockStore(EntityPiece ps)
    {
        if (!playerInput.inputIsActive) return;

        playerInput.uiInputModule = FindObjectOfType<InputSystemUIInputModule>();
        playerInput.uiInputModule.actionsAsset = playerInput.actions;

        if (previousGamePhase == GamePhase.PreStockStore) previousGamePhase = GamePhase.EncounterTime;
        else previousGamePhase = instance.phase;

        Debug.Log("prev phase = " + previousGamePhase);
        SwitchActionMap(GamePhase.PreStockStore);
    }

    private void OnTryRestockStore(EntityPiece ps)
    {
        previousGamePhase = GamePhase.PreStockStore;
    }

    private void OnCancelRestockStore()
    {
        if (!playerInput.inputIsActive) return;

        if (previousGamePhase == GamePhase.PreStockStore)
        {
            // undo to ask after already tring to restock
            m_AskRestockStore.RaiseEvent(currentPlayer);
        }
        else
            GameplayTest.instance.phase = GamePhase.EndTurn;
    }

    private void OnRestockStore(MapNode node)
    {
        if (!playerInput.inputIsActive) return;

        playerInput.uiInputModule = FindObjectOfType<InputSystemUIInputModule>();
        playerInput.uiInputModule.actionsAsset = playerInput.actions;

        previousGamePhase = instance.phase;
        Debug.Log("prev phase = "+previousGamePhase);
        SwitchActionMap(GamePhase.StockStore);
    }

    private void OnFinishStockingStore(EntityPiece ps)
    {
        if (!playerInput.inputIsActive) return;

        if (previousGamePhase == GamePhase.EncounterTime ||
            previousGamePhase == GamePhase.PreStockStore)
        {
            Debug.Log("What the fc");
            GameplayTest.instance.phase = GamePhase.EndTurn;
            previousGamePhase = GamePhase.EndTurn;
            //SwitchActionMap(GamePhase.StockStore);
            return;
        }

        Debug.Log("finish stocking, go back to ITM plz");
        previousGamePhase = GamePhase.InitialTurnMenu;
        SwitchActionMap(GamePhase.InitialTurnMenu);
        //SwitchActionMap(previousGamePhase);
    }

    private void OnLandOnStorefront(MapNode node)
    {
        if (!playerInput.inputIsActive) return;

        //playerInput.uiInputModule.GetComponent<MultiplayerEventSystem>().playerRoot = 
        playerInput.uiInputModule = FindObjectOfType<InputSystemUIInputModule>();
        playerInput.uiInputModule.actionsAsset = playerInput.actions;
        SwitchActionMap(GamePhase.InStore);
    }

    private void OnItemBought(ItemStats item)
    {
        if (!playerInput.inputIsActive) return;
        SwitchActionMap(GamePhase.ConfirmContinue);
    }

    private void OnExitStorefront()
    {

    }

    private void OnLandOnVendor(MapNode node)
    {
        if (!playerInput.inputIsActive) return;

        SwitchActionMap(GamePhase.InVendor);
    }

    private void OnLandOnMultipleEntities(List<EntityPiece> entities)
    {
        if (!playerInput.inputIsActive) return;

        playerInput.uiInputModule = FindObjectOfType<InputSystemUIInputModule>();
        playerInput.uiInputModule.actionsAsset = playerInput.actions;
        SwitchActionMap(GamePhase.CombatSelector);
    }

    private void OnEnteredCombatScene()
    {
        if (!playerInput.inputIsActive) return;

        SwitchActionMap(GamePhase.CombatTime);
    }

    private void OnEnterLevelUp(EntityPiece entity)
    {
        if (!playerInput.inputIsActive) return;

        previousGamePhase = GamePhase.RollDice;

        playerInput.uiInputModule = FindObjectOfType<InputSystemUIInputModule>();
        playerInput.uiInputModule.actionsAsset = playerInput.actions;
        SwitchActionMap(GamePhase.LevelUp);

        // May want to put this somewhere else
        assignedPlayer.levelUpRays.gameObject.SetActive(true);
        assignedPlayer.levelUpRays.Play();
    }

    private void OnExitLevelUp()
    {
        if (!playerInput.inputIsActive) return;

        SwitchActionMap(previousGamePhase);

        // May want to put this somewhere else
        assignedPlayer.levelUpRays.gameObject.SetActive(false);
        assignedPlayer.levelUpRays.Stop();
    }

    private void OnExitInventory()
    {
        if (!playerInput.inputIsActive) return;

        SwitchActionMap(previousGamePhase);
    }

    private void OnFullInventory(EntityPiece entity)
    {
        if (!playerInput.inputIsActive) return;

        playerInput.uiInputModule = FindObjectOfType<InputSystemUIInputModule>();
        playerInput.uiInputModule.actionsAsset = playerInput.actions;

        previousGamePhase = GamePhase.EndTurn;
        //m_OpenInventory.RaiseEvent(currentPlayer);
        SwitchActionMap(GamePhase.DiscardItem);
    }

    private void OnPlayerWon(EntityPiece winner)
    {
        if (winner != assignedPlayer) playerInput.DeactivateInput();

        playerInput.ActivateInput();
        SwitchActionMap(GamePhase.GameOver);
    }

    private void OnInitiateCombatOnPassBy(EntityPiece entity)
    {
        // Enable this player's input controls
        if (entity == assignedPlayer)
        {
            playerInput.ActivateInput();
            SwitchActionMap(GamePhase.CombatTime);
        }
    }

    private void OnAssignPlayerToController(EntityPiece entity)
    {
        //assign entitypiece to the controller w/ the same ID
        if (entity.id == playerInput.playerIndex)
        {
            assignedPlayer = entity; // This should never change after this

            // the following should be in its own function in case we need to change the player input mid-game
            var playerConfig = PlayerConfigurationManager.instance.GetPlayerConfig(playerInput.playerIndex);

            assignedPlayer.entityName = playerConfig.PlayerName;
            assignedPlayer.playerColor = playerConfig.PlayerColor;

            // Set main color of Baggie body in the palette
            //assignedPlayer.gameObject.GetComponent<PlayerPaletteLoader>().SetInspectorPaletteColor(0, playerConfig.PlayerColor);

            var playerCosmeticManager = PlayerConfigurationManager.instance.GetPlayerCosmeticManager(playerInput.playerIndex);
            var colorPal = new List<Color>(playerCosmeticManager.baggieColorPalette);

            assignedPlayer.gameObject.GetComponent<PlayerPaletteLoader>().SetInspectorPalette(colorPal);
            assignedPlayer.dustCloud.GetComponent<PlayerPaletteLoader>().SetInspectorPalette(colorPal);
        }
    }

    //
    private void OnControlsChanged()
    {
        Debug.Log($"Player[{playerInput.playerIndex}] Control Scheme is: {playerInput.currentControlScheme}");
        // Figure out what kind of Gamepad this Player has
        if (playerInput.currentControlScheme == "Gamepad")
        {
            var gamepad = playerInput.GetDevice<Gamepad>();
            //Debug.Log($"Player[{playerInput.playerIndex}] Gamepad: {gamepad}");
            //Debug.Log($"Player [{playerInput.playerIndex}] Device: " + playerInput.GetDevice<Gamepad>());
            if (gamepad is DualShockGamepad)
            {
                Debug.Log($"Player[{playerInput.playerIndex}] is using PS");
                device = CurrentDevice.PS;
            }
            else if (gamepad is XInputController)
            {
                Debug.Log($"Player[{playerInput.playerIndex}] is using XBOX");
                device = CurrentDevice.Xbox;
            }
            else if (gamepad is SwitchProControllerHID)
            {
                Debug.Log($"Player[{playerInput.playerIndex}] is using Switch Pro");
                device = CurrentDevice.Switch;
            }
            else
            {
                Debug.Log($"Heck bro, Player[{playerInput.playerIndex}] is none of the above");
                device = CurrentDevice.Xbox; // just set it to XBOX as default
            }
        }
        else
        {
            // It's a Keyboard
            Debug.Log($"Player[{playerInput.playerIndex}] is not using gamepad.");
            device = CurrentDevice.Keyboard;
        }
    }
}
