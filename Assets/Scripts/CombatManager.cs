using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public static bool isAggressorTurn = false;
    public static bool aggressorAttacking = false;
    public static bool retaliatorAttacking = false;
    public static bool combatActive = false;

    [SerializeField] public PlayerStats player1;
    [SerializeField] public PlayerStats player2;
    //[SerializeField] private PlayerStats player3;
    //[SerializeField] private PlayerStats player4;

    // Aggressor / Retaliator = original starter and victim of the combat
    // Attacker / Defender = current one attacking / defending
    private PlayerStats aggressor;
    private PlayerStats retaliator;
    private PlayerStats attacker;
    private PlayerStats defender;
    private PlayerStats curTarget;

    private List<int> itemsQueuedAttack;
    private List<int> itemsQueuedDefend;
    public int attackerAction;
    public int defenderAction;



    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
        }
        Instance = this;
    }

    public void initializeCombat() {
      combatActive = true;
      isAggressorTurn = true;
      aggressorAttacking = true;
      retaliatorAttacking = false;

      aggressor = player1; // Temporary, in final ver it'll be selected dynamically
      retaliator = player2;
      attacker = player1; // These are the current atk/defenders, changes every turn unlike above
      defender = player2;

    }

    public void passTurn() {
      isAggressorTurn = toggleBool(isAggressorTurn);

      if(isAggressorTurn) { // Both players acted. Play the turn, then toggle attacker and defender
        playTurn();

        aggressorAttacking = toggleBool(aggressorAttacking);
        retaliatorAttacking = toggleBool(retaliatorAttacking);

        if (aggressorAttacking) {
          attacker = aggressor;
          defender = retaliator;
        } else {
          attacker = retaliator;
          defender = aggressor;
        }

      }

    }

    public void playTurn() {
      foreach (int itemID in itemsQueuedDefend) {
        activateItem(itemID, true);
      }
      itemsQueuedDefend.Clear();

      foreach (int itemID in itemsQueuedAttack) {
        activateItem(itemID, false); // Can defenders use items?
      }
      itemsQueuedAttack.Clear();

      playActions(attackerAction, defenderAction);


      if (aggressor.health <= 0) {
        endCombat(false);
      } else if (retaliator.health <= 0) {
        endCombat(true);
      }



    }


    public void endCombat(bool aggressorWon) {
      combatActive = false;
      if (aggressorWon) {
        Debug.Log("Attacker Wins!");
      } else {
        Debug.Log("Defender Wins!");
      }
      // wrap up the scene and transition back to board in the final game.
    }

    private void playActions(int attackerAction, int defenderAction) {
      // run through the actions taken by both parties, dealing damage accordingly
    }

    // Here attacker means the current guy attacking, not original attacker
    private void activateItem(int itemID, bool isFromAttacker) {

      if (isFromAttacker) { // If attacker uses items
        curTarget = defender;
        switch (itemID)
        {
          case 1:
            //throwpotato
            break;
          case 2:
            //throwSUPERpotato
            break;
          default:
            print("Used a blank item.");
            break;
        }
      }

      else { // If defender uses items (seperated because they might act differently)
        curTarget = attacker;
        switch (itemID)
        {
          case 1:
            //throwpotato
            break;
          case 2:
            //throwSUPERpotato
            break;
          default:
            print("Used a blank item.");
            break;
        }
      }

    }

    public void addItem(int itemID, bool isAttacker) { // Call whenever someone chooses to use one
      if (isAttacker) {
        itemsQueuedAttack.Add(itemID);
      } else {
        itemsQueuedDefend.Add(itemID);
      }
    }

    public void chooseAction(int ActionID, bool isAttacker) {
      if (isAttacker) {
        attackerAction = ActionID;
      } else {
        defenderAction = ActionID;
      }
    }




    private bool toggleBool(bool boolToToggle) { // helper function
      if (boolToToggle) {
        return false;
      } else {
        return true;
      }
    }
}
