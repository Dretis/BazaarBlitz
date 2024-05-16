using UnityEngine;

public class CombatInputSystem : MonoBehaviour
{
    public Canvas howToPlayScreen;

    // Seems the on press stuff is something I'll have to do in person :(
    // I tried to make the update code self contained so it shouldn't be a hard port.
    bool player1Went = false;
    bool player2Went = false;
    //public ????? m_ActionSelected;

    //public PlayerEventChannelSO m_SwapPhase; // Will be used to notify combatinputsystem when a turn is finished and new input is needed

    void OnEnable() {
        //m_SwapPhase.OnEventRaised += PhasePassed;
    }

    void Update()
    {
        bool player1Attacking = CombatManager.Instance.player1Attacking;
        bool player2Attacking = CombatManager.Instance.player2Attacking;
        
        EntityPiece player1 = CombatManager.Instance.player1;
        EntityPiece player2 = CombatManager.Instance.player2;


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

    void sendAction(bool isPlayer1, int actionID) {
        if (player1Went && isPlayer1) {
            return;
        } else if (player2Went && !isPlayer1) {
            return;
        }

        if (isPlayer1) {
            player1Went = true;
            Action action;
            if (player1Attacking) {
                action = player1.attackActions[actionID];
            }
            else {
                action = player1.defendActions[actionID];
            }
            CombatManager.Instance.ActionSelected(player1, action);
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
            CombatManager.Instance.ActionSelected(player2, action);
            //m_ActionSelected.RaiseEvent(player2, action);
        }
    }

    void PhasePassed() {
        player1Went = false;
        player2Went = false;
    }


}


