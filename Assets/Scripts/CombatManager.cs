using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;


    // EXTREMELY IMPORTANT COMBAT VARIABLES

    // Mostly used to load player data and calculate the results of combat, the script refers primarily to attacker/defender.
    [SerializeField] public EntityPiece player1; // Not necessarily the person who attacked first.
    [SerializeField] public EntityPiece player2; // However if its a wild encounter, we know this is the enemy.

    public EntityPiece attacker; // another reference to the person attacking THIS TURN. Swaps every phase (attack/defend).
    public EntityPiece defender; // Every combat round lasts 1-2 phases until either character dies.

    public bool player1Attacking;
    public bool player2Attacking; // If false, they're defending. Flipped every phase.

    private bool isFightingAI = false; // If true, then its a wild encounter and player 2 is automated by an AI (plus they always attack second)


    // MODERATE IMPORTANCE VARIABLES
    private Action attackerAction; // Temporarily stores either fighter's action until the phase can progress
    private Action defenderAction; // We receive this from the listener to combatInputManager and reset it next turn (when it corresponds to swapped players)

    public bool waitingForSelection = false; // If true, players need to select their actions still. If not, combat's running and the input listener is ignored.
    public bool pausingLock = false; // Like the above, if true input is ignored. Is enabled in the waiting period while combat ends / is paused, disabled last frame.
    private bool isFirstPhase; // If true, is true, then false for every combat. Mostly for convenience.
    private bool onePlayerSelected = false; // Set to true when someone chooses an action. Next time an animation finishes, progress combat till next phase

    private int finalDamage;





    //SOUND SHIT
    public AudioClip smackSFX;
    public AudioClip clankSFX;
    public AudioClip explosionSFX;
    public AudioClip shootSFX;
    AudioSource audioSource;

    // Scene stuff
    public int combatSceneIndex;
    public CombatUIManager combatUIManager;
    public SceneGameManager sceneManager;

    // Events
    [Header("Broadcast on Event Channels")]
    public PlayerEventChannelSO m_DecidedTurnOrder; // pass in the attacker
    public PlayerEventChannelSO m_SwapPhase; // void event
    public EntityActionPhaseEventChannelSO m_ActionSelected; // Entity, check side and phase | Either the attacker or defender picked an action
    public EntityActionEventChannelSO m_BothActionsSelected; // prep time to show what they picked, follow with the dice roll too
    public DamageEventChannelSO m_DiceRolled; // 2 floats
    public PlayerEventChannelSO m_PlayOutCombat; // play attack anim and defend anim
    public DamageEventChannelSO m_DamageTaken; //upon attack anim finishing, show floating dmg ontop of defender, play hurt anim
    public EntityItemEventChannelSO m_EntityDied; // someone's HP dropped to 0, Victory, show rewards
    public VoidEventChannelSO m_Stalemate; // Combat is suspended, no one died this time
    public ItemListEventChannelSO m_VictoryAgainstEnemy;

    //public ????? m_ActionSelected; // I'm leaving this part till after the tuesday meeting, as it should use the same input system as the controller.
    // For now, a debug implementation with WASD and arrowkeys is in place that I'll soon replace. (I also had 1 controller so I couldn't debug at home)

    [Header("Listen on Event Channels")]
    public VoidEventChannelSO m_AttackImpact;

    private void OnEnable()
    {
        m_AttackImpact.OnEventRaised += OnAttackImpact;
    }

    private void OnDisable()
    {
        m_AttackImpact.OnEventRaised -= OnAttackImpact;
    }
    // Code from the old combat manager to set things up. Russell wrote most of this so I mostly copied over in the revamp, with slight edits.
    private void Awake()
    {
        combatUIManager = GetComponent<CombatUIManager>();

        audioSource = GetComponent<AudioSource>();

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

        // Indicate which combat scene each player is in.
        player1.combatSceneIndex = combatSceneIndex;
        player2.combatSceneIndex = combatSceneIndex;

        Instance = this;

        // Onto main combat setup:
        
        player1.fightingPosition = CombatUIManager.FightingPosition.Left;
        player2.fightingPosition = CombatUIManager.FightingPosition.Right;

        initializeCombat();

        // Has to be done after randomly deciding turn order

        combatUIManager.UpdateActionText(attacker, Action.PhaseTypes.Attack);
        combatUIManager.UpdateActionText(defender, Action.PhaseTypes.Defend);

        m_DecidedTurnOrder.RaiseEvent(attacker);
        m_ActionSelected.RaiseEvent(attacker, Action.PhaseTypes.Attack);

        
    }

    // Main logic code for setting up combat (deciding turn order, setting parameters, all that stuff)
    // After this and the stuff in awake, combat is only continued from the button press event signal when both players select an action
    public void initializeCombat()
    {
        waitingForSelection = false; // Will be true after setup, when the attacker/defender have to choose an action

        isFirstPhase = true; // True for first half of combat

        if (player2.isEnemy) { // scene manager guarantees an wild encounter will be loaded in player 2's slot. This decides the kind of combat this is.
            attacker = player1;
            defender = player2;
            player1Attacking = true;
            player2Attacking = false;
            isFightingAI = true;
        }
        else {
            isFightingAI = false;
            
            int whosFirst = Random.Range(0, 2); // In the final version of the game this should show a button prompt, which will make this another waiting period
            // This shouldn't be hard to add later, though I'll keep the old functionality for now as we have no animation (also other things are more urgent)

            randomlyDecidePlayers(whosFirst); // Add a coroutine / wait call for this when we have an animation.
        }

        // The input event stuff isn't in yet (I have to talk about that during capstone), but with a tweak to combatinput and uncommenting this it should be easy
        //m_ActionSelected.OnEventRaised += playerSelected;

        waitingForSelection = true; // Now we'll continue from the event listener.


        
    }

    public void randomlyDecidePlayers(int firstPlayer) {
        if (firstPlayer == 0) {
            attacker = player1;
            defender = player2;
            player1Attacking = true;
            player2Attacking = false;
        } else {
            defender = player1;
            attacker = player2;
            player1Attacking = false;
            player2Attacking = true;
        }
    }

    // Called when m_ActionSelected is raised with 
    public void ActionSelected(EntityPiece player, Action action) {
        Debug.Log(action);

        if (action.phase == Action.PhaseTypes.Attack) {
            attackerAction = action;
            m_ActionSelected.RaiseEvent(attacker, Action.PhaseTypes.Attack);
        } else if (action.phase == Action.PhaseTypes.Defend) {
            defenderAction = action;
            m_ActionSelected.RaiseEvent(defender, Action.PhaseTypes.Defend);
        }

        if (isFightingAI && action.phase == Action.PhaseTypes.Attack) {
            defenderAction = decideAttackAI();
            m_ActionSelected.RaiseEvent(defender, Action.PhaseTypes.Defend);
            onePlayerSelected = true;
        } else if (isFightingAI && action.phase == Action.PhaseTypes.Defend) {
            attackerAction = decideAttackAI();
            m_ActionSelected.RaiseEvent(attacker, Action.PhaseTypes.Attack);
            onePlayerSelected = true;
        }

        // float animationLength = action.animationLength;

        StartCoroutine(SelectionAnimation(0.5f));

    }

    public void ShowChoices() {
        waitingForSelection = false;

        m_BothActionsSelected.RaiseEvent(attacker, attackerAction); // Call animation players to run through event
        m_BothActionsSelected.RaiseEvent(defender, defenderAction); // Call animation players to run through event

        // float animationLength = some constant probably;

        StartCoroutine(ShowChoiceAnimation(0.75f));

    }
    
    // This script begins the actual combat calculations up to the point of deciding the RNG values of both sides.
    public void DiceRolls() {

        int rolledFaceAttack = Random.Range(0, 6); // Which dice index was selected
        int damageRoll = 0; // Intermediate value used to get the damage dice total
        int damage = 0; // total dice pips rolled after counting effects

        // less important variables for occasional item effects
        int attackerBonusRoll = 0; // May be set to something other than 1 in the for loop (if an item effect for this is active)

        Action attack = attackerAction;
        Action defend = defenderAction;

        switch (attack.type)
        {
            case Action.WeaponTypes.Melee:

                for (int i = 0; i < attackerBonusRoll + attack.diesToRoll + 1; i++) // Account for an attack's bonus rolls, or item bonuses
                {
                    damageRoll = (int)attacker.strDie[rolledFaceAttack]; // Get the base pip value of the dice respecting upgrades

                    // Important note: the below function is what adds damageRoll to damage (after applying item multipliers and boosts)
                    damage += (int)attacker.currentStatsModifier.ApplyDieModifier(EntityBaseStats.DieTypes.Strength, damageRoll); // counting items, add that damage

                    attackerBonusRoll = (int)attacker.currentStatsModifier.rollModifier; // and end most of the time, unless we have extra combat rolls.
                    
                }

                
                
                break;
            case Action.WeaponTypes.Gun:

                for (int i = 0; i < attackerBonusRoll + attack.diesToRoll + 1; i++)
                {
                    damageRoll = (int)attacker.dexDie[rolledFaceAttack];

                    damage += (int)attacker.currentStatsModifier.ApplyDieModifier(EntityBaseStats.DieTypes.Dex, damageRoll);

                    attackerBonusRoll = (int)attacker.currentStatsModifier.rollModifier;
                }
                
                
                break;
            case Action.WeaponTypes.Magic:

                for (int i = 0; i < attackerBonusRoll + attack.diesToRoll + 1; i++)
                {
                    damageRoll = (int)attacker.intDie[rolledFaceAttack];

                    damage += (int)attacker.currentStatsModifier.ApplyDieModifier(EntityBaseStats.DieTypes.Int, damageRoll);

                    attackerBonusRoll = (int)attacker.currentStatsModifier.rollModifier;
                }
                
                
                break;
            default:

                damageRoll = 0;
                damage = 0;
                break;
        }

        damage += (attack.bonusDamage); // Final damage dice pips. Not multiplied by 10 yet.
        

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
            default:
                defenseScore = 0;
                break;
        }

        
        

        
        m_DiceRolled.RaiseEvent(attacker, damage); // Send events so that everyone can see what was rolled on either side after a moment. Assumed to start the animation

        m_DiceRolled.RaiseEvent(defender, defenseScore);

        // float animationLength = some constant probably;

        StartCoroutine(DiceRollAnimation(0.5f, damage, (int)defenseScore));

    }

    public void PlayOutPhase(int damage, int defenseScore) {
        Action attack = attackerAction;
        Action defend = defenderAction;

        float damageTypeMultiplier = 1f; // Attacker deals this much times more damage
        if (attack.type == defend.type)
        {
            damageTypeMultiplier = 1.5f; // Neutral
        }
        else if ( (attack.type == Action.WeaponTypes.Melee && defend.type == Action.WeaponTypes.Gun)
               || (attack.type == Action.WeaponTypes.Gun && defend.type == Action.WeaponTypes.Magic)
               || (attack.type == Action.WeaponTypes.Magic && defend.type == Action.WeaponTypes.Melee) )
        {
            damageTypeMultiplier = 1f; // Incorrect defense option
        }
        else
        {
            damageTypeMultiplier = 2f; // Super effective
        }

        damage = damage * 10 - (int)defender.currentStatsModifier.defenseModifier;

        if (damage < 0) { // negative defense mod debuff?   
            damage = 0;
        }

        finalDamage = (int)(damage * (1 - (0.1f * defenseScore)) * damageTypeMultiplier);
        
        // if something like cloth was used, it seperately reduces damage by a percent (ex 20 means 20% of damage is negated).


        m_PlayOutCombat.RaiseEvent(attacker); // Set attack animation to play early. Also play defense animation (prob from dice roll event?)

        //float animationLength = attack.animationLength;

        //StartCoroutine(AttackAndDefendAnimation(0.5f, finalDamage)); Got replaced w/ OnAttackAnimationFished listener function

    }

    private void OnAttackImpact()
    {
        Debug.Log("animation attack heard and calling damamge deal");
        dealDamage(finalDamage);
    }

    // Final parts of the turn here, where we actually deal damage and soon after flip phases
    private void dealDamage(int damageToDeal) {
        // damage was set earlier in combat calulations.

        Debug.Log("Final Damage: " + damageToDeal);

        defender.health -= (int)damageToDeal; // Apply final damage

        attacker.health += (int)(damageToDeal * attacker.currentStatsModifier.lifestealMult); // Attacker heals if they have lifesteal

        m_DamageTaken.RaiseEvent(defender, damageToDeal);

        combatUIManager.UpdateActionText(attacker, Action.PhaseTypes.Attack);
        combatUIManager.UpdateActionText(defender, Action.PhaseTypes.Defend);

        StartCoroutine(PhaseEndDelay(0.75f));
    }

    private void endPhase() {
        waitingForSelection = true;
        if (player1.health <= 0 || player2.health <= 0) // before we even swap, see if someone died.
        {
            pausingLock = true;
            StartCoroutine(EndCombatDelay(1.5f)); // These aren't based on animations so they're constants.
            return;
        }

        // Now we should swap around the attacker and defender.
        EntityPiece tempDefender = defender;
        defender = attacker;
        attacker = tempDefender;

        if (player1Attacking) {
            player1Attacking = false;
            player2Attacking = true;
        } else {
            player1Attacking = true;
            player2Attacking = false;
        }

        m_SwapPhase.RaiseEvent(attacker);



        // With that done, iterate isFirstPhase to see if we should pause combat.
        if (isFirstPhase == true) { // iterate to next phase
            isFirstPhase = false;
        } else { // but pause combat if we looped twice
            isFirstPhase = true;
            pausingLock = true;
            StartCoroutine(PauseCombatDelay(1f));
        }
        

        
        

    }







    //   TWO COMBAT WRAP UP FUNCTIONS:

    // Suspends and resets the combat scene so that the next player can take a turn. Returns when its either fighter's turn.
    public void pauseCombat() 
    {
        pausingLock = false;
        // Mostly scene management stuff here

        // Pause combat scene and re-enable overworld scene
        // This does not remove the scene but makes all the game objects under the combat scene inactive.
        // Similarly, all game objects in the overworld scene are re-enabled.
        sceneManager.DisableScene(combatSceneIndex);
        sceneManager.EnableScene(0);

        // This changes the game phase in gameplay test remotely.
        sceneManager.ChangeGamePhase(GameplayTest.GamePhase.EndTurn);
    }

    // Decides the consequences of either player 1 or player 2 losing before destroying the scene
    public void endCombat()
    {
        EntityPiece winner;
        EntityPiece loser;

        bool player1Wins;
        if (player2.health <= 0) { // Player 1 wins
            player1Wins = true;
            m_EntityDied.RaiseEvent(player2, null);
            player2.health = player2.maxHealth;

            winner = player1;
            loser = player2;

        } else { // Player 2 wins
            player1Wins = false;
            m_EntityDied.RaiseEvent(player1, null);
            player1.health = player1.maxHealth;

            loser = player1;
            winner = player2;
        }

        if ( (isFightingAI && player1Wins) == false ) {
            loser.occupiedNode = sceneManager.spawnPoint;
            loser.transform.position = loser.occupiedNode.transform.position;
            loser.occupiedNodeCopy = loser.occupiedNode;
            loser.traveledNodes.Clear();
            loser.traveledNodes.Add(loser.occupiedNode);

            // Add defender's points to attacker's points 
            float points = 0.5f * loser.heldPoints;
            winner.heldPoints += Mathf.FloorToInt(points);
            loser.heldPoints -= Mathf.CeilToInt(points);

            // Base 100 xp, times 2 for every level the opponent is above you.
            float reputationGain = 100 * Mathf.Pow(2, loser.RenownLevel - winner.RenownLevel);

            if (reputationGain < 100)
            {
                reputationGain = 0; // Should it just be 0 if the opponent is lower level?
            }
            
            if (player2.isEnemy) { // Enemies dont increase the amount of xp they give
                reputationGain = 0;
            }

            winner.ReputationPoints += reputationGain;
            Debug.Log("Gained " + reputationGain + " reputation points! Now at rep: " + winner.ReputationPoints);

            // Remember to throw in that new level up UI in here soon!
        } else {
            int loot = Random.Range(0, 6);
            player1.inventory.Add(player2.inventory[loot]); // Enemy inventories are effectively static loot tables (they always have 6 items)
            loot = Random.Range(0, 6);
            player1.inventory.Add(player2.inventory[loot]);
            player1.ReputationPoints += player2.ReputationPoints; // a monster's rep is just its exp yield.
            Debug.Log("Gained " + player2.ReputationPoints + " reputation points from monster! Now at rep: " + player1.ReputationPoints);

            
        }

        
        // If the player defeated is in Death's Row, end the game. Otherwise, go to the next player's turn.
        if (!loser.isEnemy && loser.isInDeathsRow)
            sceneManager.ChangeGamePhase(GameplayTest.GamePhase.EndGame);
        else
        {
            if (loser.heldPoints < 0)
                loser.isInDeathsRow = true;
            sceneManager.ChangeGamePhase(GameplayTest.GamePhase.EndTurn);
        }


        
        // Rest is (mostly) scene management stuff / events

        // Players exit combat. A combatSceneIndex of -1 indicates they are out of combat. Otherwise, the scene index
        //  variable takes the current sceneIndex of the scene.
        player1.combatSceneIndex = -1;
        player2.combatSceneIndex = -1;

        audioSource.PlayOneShot(explosionSFX, 2f);
            
        // Deletes the current combat scene.
        sceneManager.UnloadCombatScene(SceneManager.GetSceneAt(combatSceneIndex), combatSceneIndex);

        // Re-enable scene
        sceneManager.EnableScene(0);

        // Update player scores.
        sceneManager.overworldScene.m_UpdatePlayerScore.RaiseEvent(player1.id);
        if (!player2.isEnemy) {
            sceneManager.overworldScene.m_UpdatePlayerScore.RaiseEvent(player2.id);
        }
    }

    private Action decideAttackAI()
    {
        int roll = Random.Range(0, 6); // 0 - 5

        if (player2Attacking) {
            switch (roll)
            {
            case 5:
                return player2.attackActions[2];
            case 4:
                return player2.attackActions[1];
            case 3:
                return player2.attackActions[0];
            default:
                return player2.attackActions[player2.favoredAttack - 1];
            }
        } else {
            switch (roll)
            {
            case 5:
                return player2.defendActions[2];
            case 4:
                return player2.defendActions[1];
            case 3:
                return player2.defendActions[0];
            default:
                return player2.defendActions[player2.favoredAttack - 1];
            }
        }
    }

    // IENUMERATORS (Coroutines that delay code so animations can run)

    // Allows waiting and then going back to code
    public IEnumerator SelectionAnimation(float animationLength)
    {
        // Selection animation done within the previously called event

        Debug.Log($"A player selected!");
        yield return new WaitForSeconds(animationLength);
        Debug.Log($"Done Selecting");
        
        if (onePlayerSelected) // Means both players decided
        {
            onePlayerSelected = false; // For next phase
            ShowChoices(); // Progress combat through showing choices until the phase plays out.
            
        } else {
            onePlayerSelected = true;
        }
    }

    public IEnumerator ShowChoiceAnimation(float animationTime)
    {
        
        Debug.Log($"Revealing Actions!");
        yield return new WaitForSeconds(animationTime);
        Debug.Log($"Attack: {attackerAction.type}");
        Debug.Log($"Defend: {defenderAction.type}");
    
        
        DiceRolls(); // Acts out the turn (most of the combat logic here, takes a while to get back)
    }

    public IEnumerator DiceRollAnimation(float animationTime, int damageRoll, int defenseRoll)
    {
        //Play your dice rolling animation here (assuming they were started by the event)

        yield return new WaitForSeconds(animationTime);
        
        Debug.Log($"Damage roll: {damageRoll}");
        Debug.Log($"Defend roll: {defenseRoll}");

        //m_DiceRolled.RaiseEvent(attacker, damageRoll); // Damage roll is the total of all rolled damage dice (if thats modified), and damage boosts, but no defense or type advantage involved.

        //m_DiceRolled.RaiseEvent(defender, defenseRoll);
        
        PlayOutPhase(damageRoll, defenseRoll); // Now that we know the base damage, we can simply progress with combat after counting type advantage.

    }

    public IEnumerator AttackAndDefendAnimation(float animationTime, int damageToDeal)
    {
        // It was mentioned some stuff like the defense animation could be tuned in the animation timeline, but this
        // moment of the method represents the start of the attack animation.

        yield return new WaitForSeconds(animationTime);
        
        dealDamage(damageToDeal);

    }

    
    public IEnumerator PhaseEndDelay(float animationTime) // Exists so there's a half a second or something before the phase ends
    {
        // It was mentioned some stuff like the defense animation could be tuned in the animation timeline, but this
        // moment of the method represents the start of the attack animation.

        yield return new WaitForSeconds(animationTime);
        
        endPhase();
    }

    public IEnumerator PauseCombatDelay(float animationTime) // Exists so there's a half a second or something before the phase ends
    {
        // It was mentioned some stuff like the defense animation could be tuned in the animation timeline, but this
        // moment of the method represents the start of the attack animation.
        m_Stalemate.RaiseEvent();
        yield return new WaitForSeconds(animationTime);
        
        pauseCombat();

    }

    public IEnumerator EndCombatDelay(float animationTime) // Exists so there's a half a second or something before the phase ends
    {
        // It was mentioned some stuff like the defense animation could be tuned in the animation timeline, but this
        // moment of the method represents the start of the attack animation.

        if (isFightingAI && player2.health <= 0)
        {
            int loot1 = Random.Range(0, 6);
            player1.inventory.Add(player2.inventory[loot1]); // Enemy inventories are effectively static loot tables (they always have 6 items)
            int loot2 = Random.Range(0, 6);
            player1.inventory.Add(player2.inventory[loot2]);

            List<ItemStats> newlyGainedItems = new List<ItemStats>();
            newlyGainedItems.Add(player2.inventory[loot1]); 
            newlyGainedItems.Add(player2.inventory[loot2]);

            player1.ReputationPoints += player2.ReputationPoints; // a monster's rep is just its exp yield.

            m_VictoryAgainstEnemy.RaiseEvent(newlyGainedItems);
        }

        yield return new WaitForSeconds(animationTime);
        
        endCombat();

    }


}


