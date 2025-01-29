using UnityEngine;
using UnityEngine.InputSystem;

public class CombatInputSystem : MonoBehaviour
{
    public PlayerInput combatInput;
    private bool howToPlayOn = false;
    public Canvas howToPlayScreen;

    // Seems the on press stuff is something I'll have to do in person :(
    // I tried to make the update code self contained so it shouldn't be a hard port.
    bool player1Went = false;
    bool player2Went = false;
    
    bool player1Attacking;
    bool player2Attacking;

    EntityPiece player1;
    EntityPiece player2;


    [SerializeField] private PlayerInput player1input;
    [SerializeField] private PlayerInput player2input;
    [SerializeField] private bool sharedController = false;

    CombatManager combatManager;


    //public ????? m_ActionSelected;
    [Header("Listen on Event Channels")]
    public PlayerEventChannelSO m_SwapPhase; // Will be used to notify combatinputsystem when a turn is finished and new input is needed
    public VoidEventChannelSO m_EnteredOverworldScene;
    public EntityIntEventChannelSO m_PlayerActionSelected;

    void OnEnable() 
    {
        //combatInput.enabled = true;
        m_SwapPhase.OnEventRaised += PhasePassed;
        m_EnteredOverworldScene.OnEventRaised += DisableCombatInput;
        m_PlayerActionSelected.OnEventRaised += OnPlayerActionSelected;

        EnableCombatInput();
    }
    void OnDisable()
    {
        //combatInput.enabled = false;
        m_SwapPhase.OnEventRaised -= PhasePassed;
        m_EnteredOverworldScene.OnEventRaised -= DisableCombatInput;
        m_PlayerActionSelected.OnEventRaised -= OnPlayerActionSelected;
    }

    void Awake() {
        combatManager = FindObjectOfType<CombatManager>();
        player1Attacking = combatManager.player1Attacking;
        player2Attacking = combatManager.player2Attacking;

        player1 = combatManager.player1;
        player2 = combatManager.player2;

        // Set up inputs to the correct player side
        var player1Config = PlayerConfigurationManager.instance.GetPlayerConfig(combatManager.player1.id);
        player1input = player1Config.Input;

        if (!combatManager.player2.isEnemy)
        {
            // Only put a player input for P2 if in a pvp fight
            var player2Config = PlayerConfigurationManager.instance.GetPlayerConfig(combatManager.player2.id);
            player2input = player2Config.Input;
        }

        // Case when both players share a controller
        if(player1input == player2input)
        {
            sharedController = true;
        }
    }

    private void EnableCombatInput()
    {
        //combatInput.enabled = false;
        player1input.ActivateInput();
        player1input.currentActionMap.Enable();

        if (!combatManager.player2.isEnemy)
        {
            player2input.ActivateInput();
            player1input.currentActionMap.Enable();
        }
    }

    private void DisableCombatInput()
    {
        //combatInput.enabled = false;
        player1input.currentActionMap.Disable();
        if (!combatManager.player2.isEnemy) player2input.currentActionMap.Disable();
        // i need this scritp atm so controller input doesnt just disappear randomly after combat
    }

    /*
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            howToPlayScreen.enabled = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.A))
        {
            howToPlayScreen.enabled = false;
        }

        
        // ALL THESE WILL BE REPLACED WITH ON KEY PRESS FUNCTIONS (Once the input system is implemented)
        if (Input.GetKeyDown(KeyCode.W))
        {
            sendAction(true, 1); // Player 1 second element (melee)
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            sendAction(true, 0); // Player 1 first element (gun)
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            sendAction(true, 2); // Player 1 first element (magic)
        }


        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            sendAction(false, 1); // Player 2 second element (melee)
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            sendAction(false, 0); // Player 2 first element (gun)
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            sendAction(false, 2); // Player 2 first element (magic)
        }
        
    }
    */
    private void OnHowToCombat()
    {
        if (howToPlayOn)
        {
            howToPlayScreen.enabled = false;
            howToPlayOn = false;
        }
        else
        {
            howToPlayScreen.enabled = true;
            howToPlayOn = true;
        }
    }

    /*
    private void OnUpAction(PlayerInput input)
    {
        Debug.Log(input);
        //if()
        Debug.Log("P1 Up action pressed");
        sendAction(true, 1); // Player 1 second element (melee)
    }

    private void OnRightAction()
    {
        Debug.Log("P1 Right action pressed");
        sendAction(true, 0); // Player 1 first element (gun)
    }

    private void OnDownAction()
    {
        Debug.Log("P1 Down action pressed");
        sendAction(true, 2); // Player 1 first element (magic)
    }

    private void OnUpActionP2()
    {
        Debug.Log("P2 Upppp action pressed");
        sendAction(false, 1); // Player  second element (melee)
    }

    private void OnRightActionP2()
    {
        Debug.Log("P2 Righto action pressed");
        sendAction(false, 0); // Player 2 first element (gun)
    }

    private void OnDownActionP2()
    {
        Debug.Log("P2 Downo action pressed");
        sendAction(false, 2); // Player 2 first element (magic)
    }
    */

    void sendAction(bool isPlayer1, int actionID) {

        if (player1Went && isPlayer1) {
            return;
        } else if (player2Went && !isPlayer1) {
            return;
        } else if (!isPlayer1 && combatManager.player2.isEnemy) {
            return;
        } else if (combatManager.waitingForSelection == false || combatManager.pausingLock == true) {
            // Waiting for selection shouldn't matter anyway due to player1/2went, but its here just to be safe.
            Debug.Log("Turn in progress / combat pausing");
            return;
        }

        Debug.Log(combatManager.combatSceneIndex);

        //player1Attacking = combatManager.player1Attacking;
        //player2Attacking = combatManager.player2Attacking;

        //player1 = combatManager.player1;
        //player2 = combatManager.player2;

        Debug.Log(actionID);

        if (isPlayer1) {
            player1Went = true;
            Action action;
            if (player1Attacking) {
                action = player1.attackActions[actionID];
            }
            else {
                action = player1.defendActions[actionID];
            }
            combatManager.ActionSelected(player1, action);
            //m_ActionSelected.RaiseEvent(player1, action);
        } else {
            player2Went = true;
            Action action;
            if (player2Attacking) {
                action = player2.attackActions[actionID];
            }
            else {
                action = player2.defendActions[actionID];
            }
            combatManager.ActionSelected(player2, action);
            //m_ActionSelected.RaiseEvent(player2, action);
        }
    }
    private void OnPlayerActionSelected(EntityPiece entity, int actionID)
    {
        // please update this code later with the rest of this script plz
        if (entity == player1)
        {
            sendAction(true, actionID);
        }
        else if (entity == player2)
        {
            sendAction(false, actionID);
        }
        else
        {
            Debug.Log(entity + "is not in this combat scene. Something went wrong!!");
        }
    }

    private void PhasePassed(EntityPiece attacker) {
        player1Went = false;
        player2Went = false;

        if (player1Attacking) {
            player1Attacking = false;
        } else {
            player1Attacking = true;
        }

        if (player2Attacking) {
            player2Attacking = false;
        } else {
            player2Attacking = true;
        }
        
    }


}