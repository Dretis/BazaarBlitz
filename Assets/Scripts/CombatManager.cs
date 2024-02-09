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

    private List<ItemStats> itemsQueuedAttack;
    private List<ItemStats> itemsQueuedDefend;
    private int attackerAction;
    private int defenderAction;

    private int attackerBuffDamage = 0;
    private int defenderBonusRoll = 0;
    private int attackerBonusRoll = 0;

    private bool isFightingAI = false;
    private int playersLastAttack = 0;

    public CombatUIManager combatUIManager;

    private int numPhases = 0;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
        }
        SceneGameManager sceneManager = GameObject.FindWithTag("SceneManager").GetComponent<SceneGameManager>();
        if (sceneManager) {
            foreach(var player in sceneManager.players)
            {
                if(sceneManager.player1ID == player.id)
                {
                    player1 = player.combatStats;
                }
                else if(sceneManager.player2ID == player.id)
                {
                    player2 = player.combatStats;
                }
            }
        }

        Instance = this;
        initializeCombat();

        foreach (GameObject a in sceneManager.overworldSceneGameObjects)
        {
            if (a != sceneManager.gameObject)
                a.SetActive(false);
        }

        itemsQueuedAttack = new List<ItemStats>();
        itemsQueuedDefend = new List<ItemStats>();

        combatUIManager = GetComponent<CombatUIManager>();
    }

    public void initializeCombat()
    {
        player1.fightingPosition = CombatUIManager.FightingPosition.Left;
        player2.fightingPosition = CombatUIManager.FightingPosition.Right;

        combatActive = true;
        isAggressorTurn = true;
        aggressorAttacking = true;
        retaliatorAttacking = false;
        playersLastAttack = 0;

        if (player2.isEnemy)
        {
            aggressor = player1;
            retaliator = player2;
            isFightingAI = true;
            curEnemy = player2;
        }
        else if (player1.isEnemy)
        {
            aggressor = player2;
            retaliator = player1;
            isFightingAI = true;
            curEnemy = player1;
        }
        else
        {
            int whosFirst = Random.Range(0, 2); // Add button prompt later

            if (whosFirst == 0)
            {
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

    public void passTurn()
    {
        isAggressorTurn = toggleBool(isAggressorTurn);

        if ((!isAggressorTurn && aggressorAttacking) && isFightingAI)
        {
            defenderAction = decideAttackAI();
            isAggressorTurn = toggleBool(isAggressorTurn); // Now well just finish the turn.
        }

        if(isAggressorTurn)
        {
            // Both players acted. Play the turn, then toggle attacker and defender
            playTurn();

            aggressorAttacking = toggleBool(aggressorAttacking);
            retaliatorAttacking = toggleBool(retaliatorAttacking);

            if (aggressorAttacking)
            {
                attacker = aggressor;
                defender = retaliator;
            }
            else
            {
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
      foreach (int item in itemsQueuedDefend) {
        activateItem(item, true);
      }
      itemsQueuedDefend.Clear();

      foreach (int item in itemsQueuedAttack) {
        activateItem(item, false); // Can defenders use items?
      }
      itemsQueuedAttack.Clear();

      playActions(attackerAction, defenderAction);

      numPhases++;
        
      if (aggressor.health <= 0) {
        endCombat(false);
      } else if (retaliator.health <= 0) {
        endCombat(true);
      }
      else if (numPhases == 2)
      {
            numPhases = 0;
            Debug.Log("I'm in scene" + combatSceneIndex);
            // Pause combat scene and re-enable overworld scene
            sceneManager.DisableScene(combatSceneIndex);
            sceneManager.EnableScene(0);

            sceneManager.ChangeGamePhase(GameplayTest.GamePhase.EndTurn);
            Debug.Log("I have left scene" + combatSceneIndex);
      }

    }

    public void endCombat(bool aggressorWon) {

      SceneGameManager sceneManager = GameObject.FindWithTag("SceneManager").GetComponent<SceneGameManager>();
        //combatActive = false;
      if (aggressorWon) {
        Debug.Log("Attacker Wins!");
      } else {
        Debug.Log("Defender Wins!");
      }
        foreach (GameObject a in sceneManager.overworldSceneGameObjects)
            a.SetActive(true);
        sceneManager.ChangeGamePhase(GameplayTest.GamePhase.EndTurn);
        sceneManager.UnloadCombatScene();
      // wrap up the scene and transition back to board in the final game.
    }

    private void playActions(int attackerAction, int defenderAction) {
        // run through the actions taken by both parties, dealing damage accordingly

        if (player1 == attacker)
        {
            combatUIManager.UpdateActionAnimation(attackerAction, player1.fightingPosition);
            combatUIManager.UpdateActionAnimation(defenderAction + 4, player2.fightingPosition);
        }
        else
        {
            combatUIManager.UpdateActionAnimation(attackerAction, player2.fightingPosition);
            combatUIManager.UpdateActionAnimation(defenderAction + 4, player1.fightingPosition);
        }


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
    private void activateItem(ItemStats item, bool isFromAttacker) {

      if (isFromAttacker) { // If attacker uses items

        if ( !(item.phase == PhaseTypes.both || item.phase == PhaseTypes.Attack) ) {
          Debug.Log(itemName + " can't be used in this attack phase!");
          return;
        } else if ( !(attacker.inventory.Contains(item)) ) {
          Debug.Log("You don't have that!");
          return;
        } else {
          attacker.inventory.Remove(item);
        }

        //Min-Max heal, ex 0-0
        attacker.health -= Random.Range(playerDamageMin, playerDamageMax+1);
        // subtract opponent hp for armor piercing
        attackerBuffDamage = Random.Range(bonusDamageMin, bonusDamageMax+1);
        attackerBonusRoll = diesToRoll;

        Debug.Log("Potato Eaten!");

        // //For special effects
        // switch (item.specialID)
        // {
        // case "SomeSuperCoolProperty":
        //   Debug.Log("Used an item!");
        //   attackerBuffDamage = 9999;
        //   break;
        // default:
        //   Debug.Log("Used a blank item?");
        //   break;
        // }

      }

      else { // If defender uses items (seperated because they might act differently)


        if ( !(item.phase == PhaseTypes.both || item.phase == PhaseTypes.Defend) ) {
          Debug.Log(itemName + " can't be used in this defend phase!");
          return;
        } else if ( !(defender.inventory.Contains(item)) ) {
          Debug.Log("You don't have that!");
          return;
        } else {
          defender.inventory.Remove(item);
        }

        //Min-Max heal, ex 0-0
        defender.health -= Random.Range(playerDamageMin, playerDamageMax+1);
        // subtract opponent hp for armor piercing
        attacker.health -= Random.Range(bonusDamageMin, bonusDamageMax+1); // Defender attack items just deal damage
        attackerBonusRoll = diesToRoll;

        switch (item)
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

    public void addItem(ItemStats item, bool isAttacker) { // Call whenever someone chooses to use one
      if (isAttacker) {
        if (itemsQueuedAttack.Count < 1) {
          itemsQueuedAttack.Add(item);
        } else {
          itemsQueuedAttack.Clear();
          itemsQueuedAttack.Add(item);
        }
      } else {
        if (itemsQueuedDefend.Count < 1) {
          itemsQueuedDefend.Add(item);
        } else {
          itemsQueuedDefend.Clear();
          itemsQueuedDefend.Add(item);
        }
      }
    }

    public void chooseAction(int ActionID, bool isAttacker) {
        combatUIManager.UpdateActionAnimation(0, player1.fightingPosition);
        combatUIManager.UpdateActionAnimation(0, player2.fightingPosition);
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
