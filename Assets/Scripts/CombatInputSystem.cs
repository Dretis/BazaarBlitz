using UnityEngine;

public class CombatInputSystem : MonoBehaviour
{
    public Canvas howToPlayScreen;
<<<<<<< Updated upstream
    // Update is called once per frame
=======

    // Seems the on press stuff is something I'll have to do in person :(
    // I tried to make the update code self contained so it shouldn't be a hard port.
    bool player1Went = false;
    bool player2Went = false;

    bool player1Attacking;
    bool player2Attacking;

    EntityPiece player1;
    EntityPiece player2;

    //public ????? m_ActionSelected;

    public PlayerEventChannelSO m_SwapPhase; // Will be used to notify combatinputsystem when a turn is finished and new input is needed

    void OnEnable() {
        


        m_SwapPhase.OnEventRaised += PhasePassed;
    }

>>>>>>> Stashed changes
    void Update()
    {
        bool currentPlayer = CombatManager.Instance.isInitiatorTurn;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            CombatManager.Instance.chooseAction(3, currentPlayer);
            CombatManager.Instance.passSelectionTurn();
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            CombatManager.Instance.chooseAction(2, currentPlayer);
            CombatManager.Instance.passSelectionTurn();
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            CombatManager.Instance.chooseAction(4, currentPlayer);
            CombatManager.Instance.passSelectionTurn();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            howToPlayScreen.enabled = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.A))
        {
            howToPlayScreen.enabled = false;
        }
    }
<<<<<<< Updated upstream
=======

    void sendAction(bool isPlayer1, int actionID) {
        player1Attacking = CombatManager.Instance.player1Attacking;
        player2Attacking = CombatManager.Instance.player2Attacking;

        player1 = CombatManager.Instance.player1;
        player2 = CombatManager.Instance.player2;

        if (player1Went && isPlayer1) { // Players who already went have to commit
            return;
        } else if (player2Went && !isPlayer1) {
            return;
        } else if (!isPlayer1 && player2.isEnemy) { // No manual enemy control
            return;
        }

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


>>>>>>> Stashed changes
}
