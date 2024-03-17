using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    public bool isAggressorTurn = false;
    public bool aggressorAttacking = false;
    public bool retaliatorAttacking = false;
    public bool combatActive = false;

    [SerializeField] public EntityPiece player1;
    [SerializeField] public EntityPiece player2;
    //[SerializeField] private PlayerStats player3;
    //[SerializeField] private PlayerStats player4;

    // Aggressor / Retaliator = original starter and victim of the combat
    // Attacker / Defender = current one attacking / defending
    private EntityPiece aggressor;
    private EntityPiece retaliator;
    private EntityPiece attacker;
    private EntityPiece defender;
    private EntityPiece curTarget;
    private EntityPiece curEnemy;

    private List<ItemStats> itemsQueuedAttack;
    private List<ItemStats> itemsQueuedDefend;
    private int attackerAction;
    private int defenderAction;

    private int attackerBuffDamage = 0;
    private int defenderBonusRoll = 0;
    private int attackerBonusRoll = 0;

    private bool isFightingAI = false;
    private int playersLastAttack = 0;

    //SOUND SHIT
    public AudioClip smackSFX;
    public AudioClip clankSFX;
    public AudioClip explosionSFX;
    public AudioClip shootSFX;
    AudioSource audioSource;

    public int combatSceneIndex;


    public CombatUIManager combatUIManager;
    public SceneGameManager sceneManager;

    private int phaseCount = 0;

    private void Awake()
    {
        sceneManager = GameObject.FindWithTag("SceneManager").GetComponent<SceneGameManager>();
        if (sceneManager)
        {

            Debug.Log("Finding player");

            player1 = sceneManager.entities.Find(entity => sceneManager.player1ID == entity.id);
            player2 = sceneManager.entities.Find(entity => sceneManager.player2ID == entity.id);
        }

        // Keep a track of combat managers.
        if (!sceneManager.combatManagers.Contains(this))
        {
            sceneManager.combatManagers.Add(this);
        }

        // Get the scene index of the combat scene and set it to the active scene.
        combatSceneIndex = SceneManager.sceneCount - 1;
        // SceneManager.SetActiveScene(SceneManager.GetSceneAt(combatSceneIndex));

        // Indicate which combat scene each player is in.
        player1.combatSceneIndex = combatSceneIndex;
        player2.combatSceneIndex = combatSceneIndex;

        Instance = this;
        initializeCombat();

        itemsQueuedAttack = new List<ItemStats>();
        itemsQueuedDefend = new List<ItemStats>();

        combatUIManager = GetComponent<CombatUIManager>();
    }


    public void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        Instance = this;
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
            //isFightingAI = true;
            curEnemy = player2;
        }
        else if (player1.isEnemy)
        {
            aggressor = player2;
            retaliator = player1;
            //isFightingAI = true;
            curEnemy = player1;
        }
        else
        {
            int whosFirst = Random.Range(0, 2); // Add button prompt later

            if (whosFirst == 0)
            {
                aggressor = player1;
                retaliator = player2;
            }
            else
            {
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

        if (isAggressorTurn)
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

        if ((!isAggressorTurn && !aggressorAttacking) && isFightingAI)
        {
            attackerAction = decideAttackAI();
            passTurn();
        }
    }

    public void playTurn()
    {

        foreach (ItemStats item in itemsQueuedDefend)
        {
            activateItem(item, true);
        }
        itemsQueuedDefend.Clear();

        foreach (ItemStats item in itemsQueuedAttack)
        {
            activateItem(item, false); // Can defenders use items?
        }
        itemsQueuedAttack.Clear();

        playActions(attackerAction, defenderAction);

        phaseCount++;

        if (aggressor.health <= 0)
        {
            endCombat(false);
        }
        else if (retaliator.health <= 0)
        {
            endCombat(true);
        }
        else if (phaseCount == 2)
        {
            phaseCount = 0;
            Debug.Log("I'm in scene" + combatSceneIndex);
            // Pause combat scene and re-enable overworld scene
            sceneManager.DisableScene(combatSceneIndex);
            sceneManager.EnableScene(0);

            sceneManager.ChangeGamePhase(GameplayTest.GamePhase.EndTurn);
            Debug.Log("I have left scene" + combatSceneIndex);
        }
    }

    public void endCombat(bool aggressorWon)
    {
        //combatActive = false;
        if (aggressorWon)
        {
            Debug.Log("Attacker Wins!");

            // Reset health of defender.
            player2.health = player2.maxHealth;

            if (player2.isEnemy) 
            {
                int loot = Random.Range(0, 6);
                attacker.inventory.Add(player2.inventory[loot]); // Enemy inventories are loot tables
            }
            else
            {
                /* TODO: Add player2's points to player1's points */
                
            }
        }
        else
        {
            Debug.Log("Defender Wins!");

            player1.health = player1.maxHealth;

            /* TODO: Add player1's points to player2's points */
        }
        audioSource.PlayOneShot(explosionSFX, 2f);

        // Players exit combat.
        player1.combatSceneIndex = -1;
        player2.combatSceneIndex = -1;



        sceneManager.ChangeGamePhase(GameplayTest.GamePhase.EndTurn);
        sceneManager.UnloadCombatScene(SceneManager.GetSceneAt(combatSceneIndex), combatSceneIndex);

        // Re-enable scene
        sceneManager.EnableScene(0);
        // wrap up the scene and transition back to board in the final game.
    }

    private void playActions(int attackerAction, int defenderAction)
    {
        Debug.Log("I'm fighting in scene" + combatSceneIndex);
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
        if (attack.type == defend.type)
        {
            defendMultiplier = 1f;
        }
        else if (attack.type == Action.WeaponTypes.Melee && defend.type == Action.WeaponTypes.Magic)
        {
            defendMultiplier = 0.25f;
        }
        else if (attack.type == Action.WeaponTypes.Gun && defend.type == Action.WeaponTypes.Melee)
        {
            defendMultiplier = 0.25f;
        }
        else if (attack.type == Action.WeaponTypes.Magic && defend.type == Action.WeaponTypes.Gun)
        {
            defendMultiplier = 0.25f;
        }
        else if (attack.type == Action.WeaponTypes.Special)
        {
            defendMultiplier = 1f; // Slight nerf to hammer, average 2x die damage vs 2.5
        }
        else
        {
            defendMultiplier = 0.5f;
        }

        int rawRoll = Random.Range(0, 6);
        int roll = 0;
        switch (attack.type)
        {
            case Action.WeaponTypes.Melee:

                for (int i = 0; i < attackerBonusRoll + attack.diesToRoll + 1; i++)
                {
                    roll = (int)attacker.strDie[rawRoll];
                    damage += (int)attacker.currentStatsModifier.ApplyDieModifier(EntityBaseStats.DieTypes.Strength, roll);
                }

                audioSource.PlayOneShot(smackSFX, 1f);
                //Debug.Log("MeleeAttack");
                break;
            case Action.WeaponTypes.Gun:

                for (int i = 0; i < attackerBonusRoll + attack.diesToRoll + 1; i++)
                {
                    roll = (int)attacker.dexDie[rawRoll];
                    damage += (int)attacker.currentStatsModifier.ApplyDieModifier(EntityBaseStats.DieTypes.Dex, roll);
                }
                audioSource.PlayOneShot(shootSFX, 1f);
                //Debug.Log("GunAttack");
                break;
            case Action.WeaponTypes.Magic:

                for (int i = 0; i < attackerBonusRoll + attack.diesToRoll + 1; i++)
                {
                    roll = (int)attacker.intDie[rawRoll];
                    damage += (int)attacker.currentStatsModifier.ApplyDieModifier(EntityBaseStats.DieTypes.Int, roll);
                }
                audioSource.PlayOneShot(clankSFX, 1f);
                //Debug.Log("MagicAttack");
                break;
            case Action.WeaponTypes.Special:
                roll = rawRoll;
                damage += attacker.strDie[roll];
                roll = Random.Range(0, 6);
                damage += attacker.dexDie[roll];
                roll = Random.Range(0, 6);
                damage += attacker.intDie[roll];
                for (int i = 0; i < attackerBonusRoll + attack.diesToRoll; i++)
                {
                    roll = Random.Range(0, 6);
                    damage += attacker.intDie[roll];
                }
                audioSource.PlayOneShot(explosionSFX, 1f);
                //Debug.Log("HammerAttack");
                break;
            default:
                damage += 0;
                break;
        }
        damage += (attackerBuffDamage + attack.bonusDamage);
        Debug.Log($"Attacker role: Raw {roll}, modified {damage}");

        roll = Random.Range(0, 6);
        switch (defend.type)
        {
            case Action.WeaponTypes.Melee:
                damage -= defender.strDie[roll] * defendMultiplier;
                for (int i = 0; i < defenderBonusRoll; i++)
                {
                    roll = Random.Range(0, 6);
                    damage -= attacker.strDie[roll] * defendMultiplier;
                }
                //Debug.Log("MeleeDefense");
                break;
            case Action.WeaponTypes.Gun:
                damage -= defender.dexDie[roll] * defendMultiplier;
                for (int i = 0; i < defenderBonusRoll; i++)
                {
                    roll = Random.Range(0, 6);
                    damage -= attacker.dexDie[roll] * defendMultiplier;
                }
                //Debug.Log("GunDefense");
                break;
            case Action.WeaponTypes.Magic:
                damage -= defender.intDie[roll] * defendMultiplier;
                for (int i = 0; i < defenderBonusRoll; i++)
                {
                    roll = Random.Range(0, 6);
                    damage -= attacker.intDie[roll] * defendMultiplier;
                }
                //Debug.Log("MagicDefense");
                break;
            case Action.WeaponTypes.Special:
                if (attack.type == Action.WeaponTypes.Special)
                {
                    attacker.health -= damage;
                    damage = 0;
                }
                else
                {
                    damage -= 1;
                }
                //Debug.Log("HammerDefense");
                break;
            default:
                damage -= 0;
                break;
        }
        if (damage < 0)
        {
            damage = 0;
        }

        defender.health -= damage;

        attackerBonusRoll = 0;
        defenderBonusRoll = 0;
        attackerBuffDamage = 0;

        if (aggressorAttacking && isFightingAI)
        {
            playersLastAttack = attackerAction;
        }


    }

    // Here attacker means the current guy attacking, not original attacker
    private void activateItem(ItemStats item, bool isFromAttacker)
    {

        if (isFromAttacker)
        { // If attacker uses items

            if ( !(item.phase == ItemStats.PhaseTypes.Both || item.phase == ItemStats.PhaseTypes.Attack) ) {
              Debug.Log(item.itemName + " can't be used in this attack phase!");
              return;
            } else if ( !(attacker.inventory.Contains(item)) ) {
              Debug.Log("You don't have that!");
              return;
            } else {
              attacker.inventory.Remove(item);
            }

            //Min-Max heal, ex 0-0
            attacker.health -= Random.Range(item.playerDamageMin, item.playerDamageMax+1);
            // subtract opponent hp for armor piercing
            attackerBuffDamage = Random.Range(item.bonusDamageMin, item.bonusDamageMax+1);
            attackerBonusRoll = item.diesToRoll;

            Debug.Log("Used " + item.itemName);

            // //For special effects
            // switch (item.specialID)
            // {
            // case "SomeSuperCoolProperty":
            //   Debug.Log("Used an item!");
            //   attackerBuffDamage = 9999;
            //   break;
            // default:
            //   Debug.Log("Used something?");
            //   break;
            // }
        }

        else
        { // If defender uses items (seperated because they might act differently)
          if ( !(item.phase == ItemStats.PhaseTypes.Both || item.phase == ItemStats.PhaseTypes.Defend) ) {
            Debug.Log(item.itemName + " can't be used in this defend phase!");
            return;
          } else if ( !(defender.inventory.Contains(item)) ) {
            Debug.Log("You don't have that!");
            return;
          } else {
            defender.inventory.Remove(item);
          }

          //Min-Max heal, ex 0-0
          defender.health -= Random.Range(item.playerDamageMin, item.playerDamageMax+1);
          // subtract opponent hp for armor piercing
          attacker.health -= Random.Range(item.bonusDamageMin, item.bonusDamageMax+1); // Defender attack items just deal damage
          attackerBonusRoll = item.diesToRoll;

          // //For special effects
          // switch (item.specialID)
          // {
          // case "SomeSuperCoolProperty":
          //   Debug.Log("Used an item!");
          //   attackerBuffDamage = -9999;
          //   break;
          // default:
          //   Debug.Log("Used a blank item?");
          //   break;
          // }
        }

    }

    public void addItem(ItemStats item, bool isAttacker)
    { // Call whenever someone chooses to use one
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

    public void chooseAction(int ActionID, bool isAttacker)
    {
        combatUIManager.UpdateActionAnimation(0, player1.fightingPosition);
        combatUIManager.UpdateActionAnimation(0, player2.fightingPosition);
        if (isAttacker)
        {
            attackerAction = ActionID;
        }
        else
        {
            defenderAction = ActionID;
        }
    }

    private int decideAttackAI()
    {
        int roll = Random.Range(0, 8);
        if (roll < 4)
        {
            return curEnemy.favoredAttack;
        }
        else if (roll < 5)
        {
            return 1;
        }
        else if (roll < 6)
        {
            return 2;
        }
        else if (roll < 7)
        {
            return 3;
        }
        else
        {
            return 4;
        }
    }

    private int decideDefendAI()
    {
        int roll = Random.Range(0, 8);
        if (roll < 4)
        {
            if (playersLastAttack == 1)
            {
                return 1; // I forgot the type advantage system and so did the enemies
            }
            else if (playersLastAttack == 2)
            {
                return 2;
            }
            else if (playersLastAttack == 3)
            {
                return 3;
            }
            else
            {
                return 4;
            }
        }
        else if (roll < 5)
        {
            return 1;
        }
        else if (roll < 6)
        {
            return 2;
        }
        else if (roll < 7)
        {
            return 3;
        }
        else
        {
            return 4;
        }
    }




    private bool toggleBool(bool boolToToggle)
    { // helper function
        if (boolToToggle)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
