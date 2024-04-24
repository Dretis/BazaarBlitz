using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    // All 3 are toggled every attack. Mainly used for the turn order framework and supporting AI
    public bool isInitiatorTurn = false; 
    public bool initiatorAttacking = false;
    public bool retaliatorAttacking = false;

    // Mostly used to load player data, the script refers primarily to attacker/defender
    [SerializeField] public EntityPiece player1;
    [SerializeField] public EntityPiece player2; // If its a wild encounter, this is the enemy.

    // Represents the first / second to attack after the battle's start. Both never swap!
    
    private EntityPiece initiator;
    private EntityPiece retaliator;

    // Attacker / Defender represent the current player attacking / defending, swaps every turn.
    private EntityPiece attacker;
    private EntityPiece defender;

    private int attackerAction; // Stores the move chosen by either player before playTurn() actually plays them out
    private int defenderAction;

    private int attackerBuffDamage = 0; // Boosts the dice pip value of the attacker (+10 base damage) for each buff point
    private int defenderBonusRoll = 0; // Reduces the attacker's damage by an additive 10% for each buff point
    private int attackerBonusRoll = 0; // Causes another roll to be added to the attacker's damage for each buff point.

    
    private bool isFightingAI = false; // If true, Player2 (always player2!) has their turn automated (and uses their inventory as a loot table).
     
    private int playersLastAttack = -1; // Records Player1's last attack. An AI will more likely counter that attack after

    //SOUND SHIT
    public AudioClip smackSFX;
    public AudioClip clankSFX;
    public AudioClip explosionSFX;
    public AudioClip shootSFX;
    AudioSource audioSource;

    public int combatSceneIndex;

    private float endCombatSceneTimer = 0.0f;
    private bool endingCombat = false;
    private bool pausingCombat = false;
    private bool initiatorWon;

    public CombatUIManager combatUIManager;
    public SceneGameManager sceneManager;

    private int phaseCount = 0; // Increases after every turn. Once it hits 2, a round passed and unfinished combat is paused.

    [Header("Broadcast on Event Channels")]
    public PlayerEventChannelSO m_DecidedTurnOrder; // pass in the attacker
    public PlayerEventChannelSO m_SwapPhase; // void event

    public EntityActionPhaseEventChannelSO m_ActionSelected; // Entity, check side and phase | Either the attacker or defender picked an action
    //public ActionSelectEventChannelSO m_BothActionsSelected; // prep time to show what they picked, follow with the dice roll too
    public DamageEventChannelSO m_DiceRolled; // 2 floats

    public PlayerEventChannelSO m_PlayOutCombat; // play attack anim and defend anim

    public DamageEventChannelSO m_DamageTaken; //upon attack anim finishing, show floating dmg ontop of defender, play hurt anim

    public EntityItemEventChannelSO m_EntityDied; // someone's HP dropped to 0, Victory, show rewards
    public VoidEventChannelSO m_Stalemate; // Combat is suspended, no one died this time

    private void Awake()
    {
        sceneManager = GameObject.FindWithTag("SceneManager").GetComponent<SceneGameManager>();
        if (sceneManager)
        {

            Debug.Log("Finding player");

            // IDs are set in scene manager as the encounter starts, letting us know who to load.
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


        combatUIManager = GetComponent<CombatUIManager>();
    }

    void Update() 
    {
        // When either guy dies, endingCombat state is entered so that the animations are played out.
        if (endingCombat) 
        {
            endCombatSceneTimer -= Time.deltaTime; // 1.5 second timer
            if (endCombatSceneTimer <= 0.0f) 
            {
                endCombat();
            }
        }
        // Same for pausing combat
        else if (pausingCombat) 
        {
            endCombatSceneTimer -= Time.deltaTime;
            if (endCombatSceneTimer <= 0.0f) 
            {
                pauseCombat();
            }
        }
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


        isInitiatorTurn = true;
        initiatorAttacking = true;
        retaliatorAttacking = false;
        playersLastAttack = -1; // AI will defend randomly at first

        if (player2.isEnemy)
        {
            initiator = player1;
            retaliator = player2;
            isFightingAI = true;
        }
        else // If player 2 and 1 are both players, card draw who goes first
        {
            int whosFirst = Random.Range(0, 2); // Add button prompt later

            if (whosFirst == 0)
            {
                initiator = player1;
                retaliator = player2;
            }
            else
            {
                initiator = player2;
                retaliator = player1;
            }
        }

        attacker = initiator; // These are the current atk/defenders, changes every turn unlike above
        defender = retaliator;

        combatUIManager.UpdateActionText(attacker, Action.PhaseTypes.Attack);
        combatUIManager.UpdateActionText(defender, Action.PhaseTypes.Defend);

        m_DecidedTurnOrder.RaiseEvent(attacker);
        m_ActionSelected.RaiseEvent(attacker, Action.PhaseTypes.Attack);
        //combatUIManager.UpdateAction2Text(defender, Action.PhaseTypes.Defend);
    }

    // Whenever someone presses a button to decide how they'll attack/defend, this function is called.
    // It flips who's deciding their current action by toggling isIniatorTurn
    public void passSelectionTurn() 
    {
        if (pausingCombat || endingCombat) // Prevent people from messing with selected actions as combat is ending
        {
            return; 
        }

        // isInitatorTurn is used by CombatInputSystem.cs to decide who's selecting their action!
        isInitiatorTurn = toggleBool(isInitiatorTurn); // False on first selection, then true.

        // But if its false (first guy went) AND the player was attacking, decide the enemy's defence immediately
        if ((!isInitiatorTurn && initiatorAttacking) && isFightingAI) 
        {
            defenderAction = decideDefendAI(); // Enemy tends towards reacting to the last attack (or using their best dice on turn 1)

            isInitiatorTurn = toggleBool(isInitiatorTurn); // now set the flag to true so the next if statement finishes stuff
        }

        if (isInitiatorTurn) // Means both players decided
        {
            
            playTurn(); // Acts out the turn

            initiatorAttacking = toggleBool(initiatorAttacking);
            retaliatorAttacking = toggleBool(retaliatorAttacking);

            if (initiatorAttacking) // Should never happen
            {
                attacker = initiator;
                defender = retaliator;
            }
            else // Turn 2
            {
                attacker = retaliator;
                defender = initiator;
            }

            combatUIManager.UpdateActionText(attacker, Action.PhaseTypes.Attack);
            combatUIManager.UpdateActionText(defender, Action.PhaseTypes.Defend);

            m_DecidedTurnOrder.RaiseEvent(attacker);
            m_ActionSelected.RaiseEvent(attacker, Action.PhaseTypes.Attack);
        }

        // On turn 2 against a now attacking enemy, immediately decide its attack action.
        if ((!isInitiatorTurn && !initiatorAttacking) && isFightingAI)
        {
            attackerAction = decideAttackAI();
            passSelectionTurn();
        }
    }

    // Handles the meta stuff for playactions (the big function), notably checking for victory conditions
    public void playTurn()
    {
        if (pausingCombat || endingCombat) 
        {
            return; // Prevent people from trying to get an extra hit in before combat stops.
        }

        playActions(attackerAction, defenderAction);

        phaseCount++;

        if (initiator.health <= 0)
        {
            initiatorWon = false;
            endCombat();
        }
        else if (retaliator.health <= 0)
        {
            initiatorWon = true;
            endCombat();
        }
        else if (phaseCount == 2)
        {
            pauseCombat();
        }
    }

    // Suspends and resets the combat scene so that the next player can take a turn. Returns when its either fighter's turn.
    public void pauseCombat() 
    {
        m_Stalemate.RaiseEvent();
        if (pausingCombat == false) 
        { 
            // Return in 1 second from Update.
            pausingCombat = true;
            endCombatSceneTimer = 1.5f;
            return;
        }

        endCombatSceneTimer = 1.5f;
        pausingCombat = false;
        phaseCount = 0;
        Debug.Log("I'm in scene" + combatSceneIndex);
        // Pause combat scene and re-enable overworld scene
        sceneManager.DisableScene(combatSceneIndex);
        sceneManager.EnableScene(0);

        sceneManager.ChangeGamePhase(GameplayTest.GamePhase.EndTurn);
        Debug.Log("I have left scene" + combatSceneIndex);
    }

    // Decides the consequences of either the initiator/retaliator losing before destroying the scene
    public void endCombat()
    {
        m_EntityDied.RaiseEvent(retaliator, null);
        if (endingCombat == false) 
        {
            endCombatSceneTimer = 1.5f;
            endingCombat = true;
            return; 
            // We'll come back later from update with endingCombat = true
        }


        if (initiatorWon)
        {
            Debug.Log("Initiator Wins!");

            // Respawn defender at pawn shop with max health (or reset them as an enemy).
            retaliator.health = retaliator.maxHealth;

            // Enemies can only ever be retaliators. They cannot be the party to engage combat.
            if (retaliator.isEnemy) 
            {
                // Enemy inventories are 6 item loot tables. 2 items are given every combat. Rep
                int loot = Random.Range(0, 6);
                initiator.inventory.Add(retaliator.inventory[loot]);
                loot = Random.Range(0, 6);
                initiator.inventory.Add(retaliator.inventory[loot]);
                initiator.ReputationPoints += retaliator.ReputationPoints; // a monster's rep is just its exp yield.
                Debug.Log("Gained " + retaliator.ReputationPoints + " reputation points from monster! Now at rep: " + initiator.ReputationPoints);
            }
            else
            {

                // Reset position of losing retaliator to spawn point.
                retaliator.occupiedNode = sceneManager.spawnPoint;
                retaliator.transform.position = retaliator.occupiedNode.transform.position;
                retaliator.occupiedNodeCopy = retaliator.occupiedNode;
                retaliator.traveledNodes.Clear();
                retaliator.traveledNodes.Add(retaliator.occupiedNode);

                // Add defender's points to attacker's points 
                float points = 0.5f * retaliator.heldPoints;
                initiator.heldPoints += Mathf.FloorToInt(points);
                retaliator.heldPoints -= Mathf.CeilToInt(points);

                // Base 100 xp, times 2 for every level the opponent is above you.
                float pointgain = 100 * Mathf.Pow(2, retaliator.RenownLevel - initiator.RenownLevel);

                if (pointgain < 100) {
                    pointgain = 0; // Should it just be 0 if the opponent is lower level?
                }

                initiator.ReputationPoints += pointgain;
                Debug.Log("Gained " + pointgain + " reputation points! Now at rep: " + initiator.ReputationPoints);
            }
        }
        else
        {
            Debug.Log("Retaliator Wins!");

            // Respawn losing attacker at pawn shop with max health.
            initiator.health = initiator.maxHealth;

            initiator.occupiedNode = sceneManager.spawnPoint;
            initiator.transform.position = initiator.occupiedNode.transform.position;
            initiator.occupiedNodeCopy = initiator.occupiedNode;
            initiator.traveledNodes.Clear();
            initiator.traveledNodes.Add(initiator.occupiedNode);

            // Add attacker's points to defender's points 
            float points = 0.5f * initiator.heldPoints;
            retaliator.heldPoints += Mathf.FloorToInt(points);
            initiator.heldPoints -= Mathf.CeilToInt(points);

            float pointgain = 100 * Mathf.Pow(2, initiator.RenownLevel - retaliator.RenownLevel);

            if (retaliator.isEnemy) { // Enemies dont increase the amount of xp they give
                pointgain = 0;
            }

            if (pointgain < 100) { // Beating up lower level players doesn't give renown
                pointgain = 0;
            }

            retaliator.ReputationPoints += pointgain;
            Debug.Log("Gained " + pointgain + " reputation points! Now at rep: " + retaliator.ReputationPoints);
        }

        audioSource.PlayOneShot(explosionSFX, 2f);

        // Players exit combat.
        player1.combatSceneIndex = -1;
        player2.combatSceneIndex = -1;

        sceneManager.ChangeGamePhase(GameplayTest.GamePhase.EndTurn);
        sceneManager.UnloadCombatScene(SceneManager.GetSceneAt(combatSceneIndex), combatSceneIndex);

        // Re-enable scene
        sceneManager.EnableScene(0);

        // Update player scores.
        sceneManager.overworldScene.m_UpdatePlayerScore.RaiseEvent(initiator.id);
        if (!retaliator.isEnemy)
            sceneManager.overworldScene.m_UpdatePlayerScore.RaiseEvent(retaliator.id);
        // wrap up the scene and transition back to board.
    }

    // Main code behind the combat system. This function is called every time both players select and action, calculating
    // how much damage the defender receives.
    private void playActions(int attackerAction, int defenderAction)
    {
        Debug.Log("I'm fighting in scene" + combatSceneIndex);
        // run through the actions taken by both parties, dealing damage accordingly
        /*
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
        */

        int damage = 0;

        // 0= Unused, 1= Gun, 2= Melee, 3=Magic. 
        Action attack = attacker.attackActions[attackerAction-1];
        Action defend = defender.defendActions[defenderAction-1];

        
        // Melee beats magic, Magic beats gun, Gun beats melee. 1x, 1.5x, or 2x damage.

        float damageTypeMultiplier = 1f; // Attacker deals this much times more damage
        if (attack.type == defend.type)
        {
            damageTypeMultiplier = 1.5f; // Neutral
        }
        else if (attack.type == Action.WeaponTypes.Melee && defend.type == Action.WeaponTypes.Gun)
        {
            damageTypeMultiplier = 1f; // Weaknesses
        }
        else if (attack.type == Action.WeaponTypes.Gun && defend.type == Action.WeaponTypes.Magic)
        {
            damageTypeMultiplier = 1f;
        }
        else if (attack.type == Action.WeaponTypes.Magic && defend.type == Action.WeaponTypes.Melee)
        {
            damageTypeMultiplier = 1f;
        }
        else if (attack.type == Action.WeaponTypes.Special)
        {
            damageTypeMultiplier = 0f; // No fourth option right now
        }
        else
        {
            damageTypeMultiplier = 2f; // Super effective
        }

        int rolledFace = Random.Range(0, 6); // Which dice index was selected
        int damageRoll = 0; // Actual value in dice pips of the damage (this is later reused for defence)

        switch (attack.type)
        {
            case Action.WeaponTypes.Melee:

                for (int i = 0; i < attackerBonusRoll + attack.diesToRoll + 1; i++) // Account for an attack's bonus rolls, or item bonuses
                {
                    damageRoll += (int)attacker.strDie[rolledFace]; // Get the base pip value of the dice

                    // Important note: the below function is what adds damageRoll to damage (after applying item multipliers and boosts)
                    damage += (int)attacker.currentStatsModifier.ApplyDieModifier(EntityBaseStats.DieTypes.Strength, damageRoll); 
                    
                    attackerBonusRoll = (int)attacker.currentStatsModifier.rollModifier; // If we have extra rolls, set them so we can loop
                    
                }

                audioSource.PlayOneShot(smackSFX, 1f);
                //Debug.Log("MeleeAttack");
                break;
            case Action.WeaponTypes.Gun:

                for (int i = 0; i < attackerBonusRoll + attack.diesToRoll + 1; i++)
                {
                    damageRoll += (int)attacker.dexDie[rolledFace];

                    damage += (int)attacker.currentStatsModifier.ApplyDieModifier(EntityBaseStats.DieTypes.Dex, damageRoll);

                    attackerBonusRoll = (int)attacker.currentStatsModifier.rollModifier;
                }
                audioSource.PlayOneShot(shootSFX, 1f);
                //Debug.Log("GunAttack");
                break;
            case Action.WeaponTypes.Magic:

                for (int i = 0; i < attackerBonusRoll + attack.diesToRoll + 1; i++)
                {
                    damageRoll += (int)attacker.intDie[rolledFace];

                    damage += (int)attacker.currentStatsModifier.ApplyDieModifier(EntityBaseStats.DieTypes.Int, damageRoll);

                    attackerBonusRoll = (int)attacker.currentStatsModifier.rollModifier;
                }
                audioSource.PlayOneShot(clankSFX, 1f);
                //Debug.Log("MagicAttack");
                break;
            case Action.WeaponTypes.Special:

                damageRoll = 0;
                damage = 0;
                break;
            default:

                damageRoll = 0;
                damage = 0;
                break;
        }
        m_DiceRolled.RaiseEvent(attacker, damageRoll);

        damage += (attackerBuffDamage + attack.bonusDamage); // Add special boosts and intrinsic damage boosts from the attack stats
        damage = damage * 10; // final damage before defense is pip value x 10
        Debug.Log($"Attacker damage: Raw roll {damageRoll}, Type {attack.type}, modified {damage}");

        int rolledFaceDefense = Random.Range(0, 6);

        float defenseScore = 0; // Every point of this reduces damage by 10%. Can be increased by items or dice pips
        switch (defend.type)
        {
            case Action.WeaponTypes.Melee:
                defenseScore = defender.strDie[rolledFaceDefense];
                break;
            case Action.WeaponTypes.Gun:
                defenseScore = defender.dexDie[rolledFaceDefense];
                //Debug.Log("GunDefense");
                break;
            case Action.WeaponTypes.Magic:
                defenseScore = defender.intDie[rolledFaceDefense];
                //Debug.Log("MagicDefense");
                break;
            case Action.WeaponTypes.Special:
                defenseScore = 0;
                //Debug.Log("HammerDefense");
                break;
            default:
                defenseScore = 0;
                break;
        }

        Debug.Log($"Defend roll: {defend.type} {defenseScore}");
        m_DiceRolled.RaiseEvent(defender, defenseScore);
        defenseScore += 1f * defender.currentStatsModifier.defenseModifier; // Apply item effects like cloth

        // damage is reduced by 10% - 100% before a type advantage multiplier is applied
        damage = (damage * (1 - (0.1f * defenseScore)) * damageTypeMultiplier);

        if (damage < 0)
        {
            damage = 0;
        }

        Debug.Log("Final Damage: " + damage);

        defender.health -= (int)damage; // Apply final damage

        attacker.health += (int)(damage * attacker.currentStatsModifier.lifestealMult); // Attacker heals if they have lifesteal

        m_DamageTaken.RaiseEvent(defender, damage);

        attackerBonusRoll = 0; // Clear effects (as they're applied additively to support multiple at once)
        defenderBonusRoll = 0;
        attackerBuffDamage = 0;

        if (initiatorAttacking && isFightingAI)
        {
            playersLastAttack = attackerAction; // Used by the ai for defend behavior
        }
    }

    public void chooseAction(int ActionID, bool isAttacker)
    {
        //combatUIManager.UpdateActionAnimation(0, player1.fightingPosition);
        //combatUIManager.UpdateActionAnimation(0, player2.fightingPosition);
        
        if (isAttacker)
        {
            attackerAction = ActionID;
            m_ActionSelected.RaiseEvent(defender, Action.PhaseTypes.Defend);
        }
        else
        {
            defenderAction = ActionID;
        }
    }


    private int decideAttackAI()
    {
        int roll = Random.Range(0, 6); // 0 - 5
        
        if (roll < 3) // total 4/6 chance of using favorite attack
        {
            return player2.favoredAttack;
        }
        else if (roll < 4) // Decide randomly 1/2 the time
        {
            return 1;
        }
        else if (roll < 5)
        {
            return 2;
        }
        else
        {
            return 3;
        }
    }

    private int decideDefendAI()
    {
        int roll = Random.Range(0, 6);
        if (roll < 3) // Enemy acts reactively 1/2 of the time
        {
            if (playersLastAttack == 1) // Player used gun, so use magic
            {
                return 3; 
            }
            else if (playersLastAttack == 2) // Player used melee, so use gun
            {
                return 1;
            }
            else if (playersLastAttack == 3) // Player used magic, so use melee
            {
                return 2;
            }
            else // First turn, decide based on what the enemy's best at for attacking
            {
                // This will be what the AI relies on unless a combat drags on for multiple rounds.
                return decideAttackAI();
            }
        }
        else if (roll < 4)
        {
            return 1;
        }
        else if (roll < 5)
        {
            return 2;
        }
        else
        {
            return 3;
        }
    }



    // Helper function to keep code from getting even more cluttered
    private bool toggleBool(bool boolToToggle)
    { 
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
