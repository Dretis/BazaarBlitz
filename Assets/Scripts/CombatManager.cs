using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    public bool isAggressorTurn = false;
    public bool aggressorAttacking = false;
    public bool retaliatorAttacking = false;
    public bool combatActive = false;

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
    private int attackerAction;
    private int defenderAction;



    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
        }
        Instance = this;
        initializeCombat();
    }

    public void initializeCombat() {
      combatActive = true;
      isAggressorTurn = true;
      aggressorAttacking = true;
      retaliatorAttacking = false;

      int whosFirst = Random.Range(0, 2); // Add button prompt later

      if (whosFirst == 0) {
        aggressor = player1;
        retaliator = player2;
      } else {
        aggressor = player2;
        retaliator = player1;
      }

      attacker = aggressor; // These are the current atk/defenders, changes every turn unlike above
      defender = retaliator;

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

      Debug.Log("Player1 Health = " + player1.health);
      Debug.Log("Player2 Health = " + player2.health);



    }


    public void endCombat(bool aggressorWon) {
      //combatActive = false;
      if (aggressorWon) {
        Debug.Log("Attacker Wins!");
      } else {
        Debug.Log("Defender Wins!");
      }
      // wrap up the scene and transition back to board in the final game.
    }

    private void playActions(int attackerAction, int defenderAction) {
      // run through the actions taken by both parties, dealing damage accordingly
      float defendMultiplier = 0.5f;
      if (attackerAction == defenderAction) {
        defendMultiplier = 1f;
      } else if (attackerAction == 1 && defenderAction == 3) {
        defendMultiplier = 0.25f;
      } else if (attackerAction == 2 && defenderAction == 1) {
        defendMultiplier = 0.25f;
      } else if (attackerAction == 3 && defenderAction == 2) {
        defendMultiplier = 0.25f;
      } else {
        defendMultiplier = 0.5f;
      }



      float damage = 0;

      int roll = Random.Range(0, 6);
      switch (attackerAction)
      {
        case 1:
          damage += attacker.strDie[roll];
          break;
        case 2:
          damage += attacker.dexDie[roll];
          break;
        case 3:
          damage += attacker.intDie[roll];
          break;
        case 4:
          damage += attacker.strDie[roll];
          roll = Random.Range(0, 6);
          damage += attacker.dexDie[roll];
          roll = Random.Range(0, 6);
          damage += attacker.intDie[roll];
          break;
        default:
          damage += 0;
          break;
      }
      Debug.Log("Attacker Roll: " + damage);

      roll = Random.Range(0, 6);
      switch (defenderAction)
      {
        case 1:
          damage -= defender.strDie[roll] * defendMultiplier;
          break;
        case 2:
          damage -= defender.dexDie[roll] * defendMultiplier;
          break;
        case 3:
          damage -= defender.intDie[roll] * defendMultiplier;
          break;
        case 4:
          if (attackerAction == 4) {
            attacker.health -= damage;
            damage = 0;
          } else {
            damage -= 1;
          }
          break;
        default:
          damage -= 0;
          break;
      }
      if (damage < 0) {
        damage = 0;
      }
      Debug.Log("- Defend roll now: " + damage + " w/ multiplier of " + defendMultiplier);

      defender.health -= damage;

    }

    // Here attacker means the current guy attacking, not original attacker
    private void activateItem(int itemID, bool isFromAttacker) {

      if (isFromAttacker) { // If attacker uses items
        curTarget = defender;

        if (itemsQueuedAttack.Contains(itemID)) {
          itemsQueuedAttack.Remove(itemID);
        } else {
          Debug.Log("You don't have that!");
          return;
        }

        switch (itemID)
        {
          case 1:
            Debug.Log("Potato Thrown!");
            break;
          case 2:
            Debug.Log("Super Potato thrown!");
            break;
          default:
            print("Used a blank item.");
            break;
        }
      }

      else { // If defender uses items (seperated because they might act differently)
        curTarget = attacker;

        if (itemsQueuedDefend.Contains(itemID)) {
          itemsQueuedDefend.Remove(itemID);
        } else {
          Debug.Log("You don't have that!");
          return;
        }

        switch (itemID)
        {
          case 1:
            Debug.Log("Potato Thrown!");
            break;
          case 2:
            Debug.Log("Super Potato thrown!");
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
