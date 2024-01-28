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
    private PlayerStats curEnemy;

    private List<int> itemsQueuedAttack;
    private List<int> itemsQueuedDefend;
    private int attackerAction;
    private int defenderAction;

    private int attackerBuffDamage = 0;
    private int defenderBonusRoll = 0;
    private int attackerBonusRoll = 0;

    private bool isFightingAI = false;
    private int playersLastAttack = 0;

    public CombatUIManager combatUIManager;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
        }
        Instance = this;
        initializeCombat();

        itemsQueuedAttack = new List<int>();
        itemsQueuedDefend = new List<int>();

        combatUIManager = GetComponent<CombatUIManager>();
    }

    public void initializeCombat() {
        player1.fightingPosition = CombatUIManager.FightingPosition.Left;
        player2.fightingPosition = CombatUIManager.FightingPosition.Right;

      combatActive = true;
      isAggressorTurn = true;
      aggressorAttacking = true;
      retaliatorAttacking = false;
      playersLastAttack = 0;

      if (player2.isEnemy) {
        aggressor = player1;
        retaliator = player2;
        isFightingAI = true;
        curEnemy = player2;
      } else if (player1.isEnemy) {
        aggressor = player2;
        retaliator = player1;
        isFightingAI = true;
        curEnemy = player1;
      } else {
        int whosFirst = Random.Range(0, 2); // Add button prompt later

        if (whosFirst == 0) {
          aggressor = player1;
          retaliator = player2;
        } else {
          aggressor = player2;
          retaliator = player1;
        }
      }

      attacker = aggressor; // These are the current atk/defenders, changes every turn unlike above
      defender = retaliator;

      combatUIManager.UpdateActionText(attacker, Action.PhaseTypes.Attack);
      combatUIManager.UpdateActionText(defender, Action.PhaseTypes.Defend);

      //combatUIManager.UpdateAction2Text(defender, Action.PhaseTypes.Defend);
    }

    public void passTurn() {
      isAggressorTurn = toggleBool(isAggressorTurn);

      if ((!isAggressorTurn && aggressorAttacking) && isFightingAI) {
        defenderAction = decideAttackAI();
        isAggressorTurn = toggleBool(isAggressorTurn); // Now well just finish the turn.
      }

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
          combatUIManager.UpdateActionText(attacker, Action.PhaseTypes.Attack);
          combatUIManager.UpdateActionText(defender, Action.PhaseTypes.Defend);
        }

      if ((!isAggressorTurn && !aggressorAttacking) && isFightingAI) {
        attackerAction = decideAttackAI();
        passTurn();
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




      float damage = 0;

      Action attack = attacker.attackActions[attackerAction - 1];
      Action defend = defender.defendActions[defenderAction - 1];

      float defendMultiplier = 0.5f;
      if (attack.type == defend.type) {
        defendMultiplier = 1f;
      } else if (attack.type == Action.WeaponTypes.Melee && defend.type == Action.WeaponTypes.Magic) {
        defendMultiplier = 0.25f;
      } else if (attack.type == Action.WeaponTypes.Gun && defend.type == Action.WeaponTypes.Melee) {
        defendMultiplier = 0.25f;
      } else if (attack.type == Action.WeaponTypes.Magic && defend.type == Action.WeaponTypes.Gun) {
        defendMultiplier = 0.25f;
      } else if (attack.type == Action.WeaponTypes.Special ) {
        defendMultiplier = 1f; // Slight nerf to hammer, average 2x die damage vs 2.5
      } else  {
        defendMultiplier = 0.5f;
      }

      int roll = Random.Range(0, 6);
      switch (attack.type)
      {
        case Action.WeaponTypes.Melee:
          damage += attacker.strDie[roll];
          for (int i = 0; i < attackerBonusRoll + attack.diesToRoll; i++) {
            roll = Random.Range(0, 6);
            damage += attacker.strDie[roll];
          }
          Debug.Log("MeleeAttack");
          break;
        case Action.WeaponTypes.Gun:
          damage += attacker.dexDie[roll];
          for (int i = 0; i < attackerBonusRoll + attack.diesToRoll; i++) {
            roll = Random.Range(0, 6);
            damage += attacker.dexDie[roll];
          }
          Debug.Log("GunAttack");
          break;
        case Action.WeaponTypes.Magic:
          damage += attacker.intDie[roll];
          for (int i = 0; i < attackerBonusRoll + attack.diesToRoll; i++) {
            roll = Random.Range(0, 6);
            damage += attacker.intDie[roll];
          }
          Debug.Log("MagicAttack");
          break;
        case Action.WeaponTypes.Special:
          damage += attacker.strDie[roll];
          roll = Random.Range(0, 6);
          damage += attacker.dexDie[roll];
          roll = Random.Range(0, 6);
          damage += attacker.intDie[roll];
          for (int i = 0; i < attackerBonusRoll + attack.diesToRoll; i++) {
            roll = Random.Range(0, 6);
            damage += attacker.intDie[roll];
          }
          Debug.Log("HammerAttack");
          break;
        default:
          damage += 0;
          break;
      }
      damage += (attackerBuffDamage + attack.bonusDamage);
      Debug.Log("Attacker Roll: " + damage);

      roll = Random.Range(0, 6);
      switch (defend.type)
      {
        case Action.WeaponTypes.Melee:
          damage -= defender.strDie[roll] * defendMultiplier;
          for (int i = 0; i < defenderBonusRoll; i++) {
            roll = Random.Range(0, 6);
            damage -= attacker.strDie[roll] * defendMultiplier;
          }
          Debug.Log("MeleeDefense");
          break;
        case Action.WeaponTypes.Gun:
          damage -= defender.dexDie[roll] * defendMultiplier;
          for (int i = 0; i < defenderBonusRoll; i++) {
            roll = Random.Range(0, 6);
            damage -= attacker.dexDie[roll] * defendMultiplier;
          }
          Debug.Log("GunDefense");
          break;
        case Action.WeaponTypes.Magic:
          damage -= defender.intDie[roll] * defendMultiplier;
          for (int i = 0; i < defenderBonusRoll; i++) {
            roll = Random.Range(0, 6);
            damage -= attacker.intDie[roll] * defendMultiplier;
          }
          Debug.Log("MagicDefense");
          break;
        case Action.WeaponTypes.Special:
          if (attack.type == Action.WeaponTypes.Special) {
            attacker.health -= damage;
            damage = 0;
          } else {
            damage -= 1;
          }
          Debug.Log("HammerDefense");
          break;
        default:
          damage -= 0;
          break;
      }
      if (damage < 0) {
        damage = 0;
      }

      defender.health -= damage;

      attackerBonusRoll = 0;
      defenderBonusRoll = 0;
      attackerBuffDamage = 0;

      if (aggressorAttacking && isFightingAI) {
        playersLastAttack = attackerAction;
      }


    }

    // Here attacker means the current guy attacking, not original attacker
    private void activateItem(int itemID, bool isFromAttacker) {

      if (isFromAttacker) { // If attacker uses items

        if (attacker.inventory.Contains(itemID)) {
          attacker.inventory.Remove(itemID);
        } else {
          Debug.Log("You don't have that!");
          return;
        }

        switch (itemID)
        {
          case 1:
            Debug.Log("Potato Eaten!");
            attacker.health += Random.Range(1, 4); // 1-3 hp heal, 1,1,2,2,3,3 dice.
            break;
          case 2:
            Debug.Log("Mutated Potato Power!");
            attacker.health -= 1;
            attackerBuffDamage += 3;
            break;
          case 3:
            Debug.Log("It failed!");
            break;
          case 4:
            attackerBonusRoll = 1;
            print("Ice Dice attack x 2");
            break;
          default:
            print("Used a blank item.");
            break;
        }
      }

      else { // If defender uses items (seperated because they might act differently)
        curTarget = attacker;

        if (defender.inventory.Contains(itemID)) {
          defender.inventory.Remove(itemID);
        } else {
          Debug.Log("You don't have that!");
          return;
        }

        switch (itemID)
        {
          case 1:
            Debug.Log("Potato Eaten!");
            attacker.health += 1;
            break;
          case 2:
            Debug.Log("It failed!");
            break;
          case 3:
            attackerBuffDamage -= 2;
            break;
          case 4:
            defenderBonusRoll = 1;
            print("Ice Dice defend x 2");
            break;
          default:
            print("Used a blank item.");
            break;
        }
      }

    }

    public void addItem(int itemID, bool isAttacker) { // Call whenever someone chooses to use one
      if (isAttacker) {
        if (itemsQueuedAttack.Count < 1) {
          itemsQueuedAttack.Add(itemID);
        } else {
          itemsQueuedAttack.Clear();
          itemsQueuedAttack.Add(itemID);
        }
      } else {
        if (itemsQueuedDefend.Count < 1) {
          itemsQueuedDefend.Add(itemID);
        } else {
          itemsQueuedDefend.Clear();
          itemsQueuedDefend.Add(itemID);
        }
      }
    }

    public void chooseAction(int ActionID, bool isAttacker) {
      if (isAttacker) {
        attackerAction = ActionID;
      } else {
        defenderAction = ActionID;
      }
    }

    private int decideAttackAI() {
      int roll = Random.Range(0, 8);
      if (roll < 4) {
        return curEnemy.favoredAttack;
      } else if (roll < 5) {
        return 1;
      } else if (roll < 6) {
        return 2;
      } else if (roll < 7) {
        return 3;
      } else {
        return 4;
      }
    }

    private int decideDefendAI() {
      int roll = Random.Range(0, 8);
      if (roll < 4) {
        if (playersLastAttack == 1) {
          return 1; // I forgot the type advantage system and so did the enemies
        } else if (playersLastAttack == 2) {
          return 2;
        } else if (playersLastAttack == 3) {
          return 3;
        } else {
          return 4;
        }
      } else if (roll < 5) {
        return 1;
      } else if (roll < 6) {
        return 2;
      } else if (roll < 7) {
        return 3;
      } else {
        return 4;
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
