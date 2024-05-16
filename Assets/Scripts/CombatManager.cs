using UnityEngine;
using UnityEngine.SceneManagement;

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

    private bool waitingForSelection; // If true, players need to select their actions still. If not, combat's running and the input listener is ignored.
    private bool isFirstPhase; // If true, is true, then false for every combat. Mostly for convenience.







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
    //public ActionSelectEventChannelSO m_BothActionsSelected; // prep time to show what they picked, follow with the dice roll too
    public DamageEventChannelSO m_DiceRolled; // 2 floats
    public PlayerEventChannelSO m_PlayOutCombat; // play attack anim and defend anim
    public DamageEventChannelSO m_DamageTaken; //upon attack anim finishing, show floating dmg ontop of defender, play hurt anim
    public EntityItemEventChannelSO m_EntityDied; // someone's HP dropped to 0, Victory, show rewards
    public VoidEventChannelSO m_Stalemate; // Combat is suspended, no one died this time

    //public ????? m_ActionSelected; // I'm leaving this part till after the tuesday meeting, as it should use the same input system as the controller.
    // For now, a debug implementation with WASD and arrowkeys is in place that I'll soon replace. (I also had 1 controller so I couldn't debug at home)



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
            retaliator = player2;
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
        if (action.Phase == 'Attack') {
            attackerAction = action;
            m_ActionSelected.RaiseEvent(attacker, Action.PhaseTypes.Attack);
        } else if (action.Phase == 'Defend') {
            defenderAction = defend;
            m_ActionSelected.RaiseEvent(defender, Action.PhaseTypes.Defend);
        }

    }







    //   TWO COMBAT WRAP UP FUNCTIONS:

    // Suspends and resets the combat scene so that the next player can take a turn. Returns when its either fighter's turn.
    public void pauseCombat() 
    {
        m_Stalemate.RaiseEvent();

        // Now onto the main code:



        // Finally more scene management stuff

        // Pause combat scene and re-enable overworld scene
        // This does not remove the scene but makes all the game objects under the combat scene inactive.
        // Similarly, all game objects in the overworld scene are re-enabled.
        sceneManager.DisableScene(combatSceneIndex);
        sceneManager.EnableScene(0);

        // This changes the game phase in gameplay test remotely.
        sceneManager.ChangeGamePhase(GameplayTest.GamePhase.EndTurn);
    }

    // Decides the consequences of either the initiator/retaliator losing before destroying the scene
    public void endCombat()
    {

        m_EntityDied.RaiseEvent(retaliator, null);


        
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
        sceneManager.overworldScene.m_UpdatePlayerScore.RaiseEvent(initiator.id);
        if (!retaliator.isEnemy) {
            sceneManager.overworldScene.m_UpdatePlayerScore.RaiseEvent(retaliator.id);
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


