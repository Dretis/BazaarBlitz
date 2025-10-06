using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Linq;
using Febucci.UI.Core;
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.Analytics;

public class GameplayTest : MonoBehaviour
{
    public GameBoard board;
    public GamePhase phase = GamePhase.RollDice;

    [Header("Debugging")]
    [SerializeField] private TextMeshProUGUI debugPhaseText;
    [SerializeField] private bool turnOffMonsterEncounters = false;

    public static GameplayTest instance;
    // Tile Data and Shit IGNORE THIS SECTION FOR NOW
    [SerializeField]
    private List<EntityPiece> playerUnits = new List<EntityPiece>();
    [SerializeField]
    private List<EntityPiece> nextPlayers = new List<EntityPiece>();
    public EntityPiece currentPlayer;
    private EntityPiece winner;
    public MapNode currentPlayerInitialNode;

    //public Dictionary<Vector2Int, GameObject> map = new Dictionary<Vector2Int, GameObject>();
    //public Dictionary<Vector2Int, GameObject> unitPos = new Dictionary<Vector2Int, GameObject>();

    public enum GameBoard
    {
        CentralMarket,
        TrainStreet,
        RiceTerrace,
    }

    public enum GamePhase
    {
        InitialTurnMenu,
        ItemSelection,
        RaycastTargetSelection,
        Freeview, // for new input system
        Inventory,
        DiscardItem,
        RollDice,
        PickDirection,
        MoveAround,
        PassBy,
        EncounterTime,
        InStore, // new for input system
        StockStore,
        OverturnStore,
        InVendor,
        RockPaperScissors,
        LevelUp,
        CombatTime,
        ConfirmContinue,
        EndTurn,
        IncidentHappening,
        EndGame,
        GameOver
    }

    public enum SpecialIncidents
    {
        Train,
        AirRaid,
        None,
    }

    [Header("Game Match Info")]
    public int targetGoal = 4000;
    public int playerCount = 4;
    public int turnRound = 1; // Round based on every player has had a turn
    private int playersActed = 0; // goes up every time a unique players turn is done

    public List<SpecialIncidents> matchIncidents; // list of periodic incidents for this specific board
    private List<SpecialIncidents> incidentsToActivate = new List<SpecialIncidents>(); // list of periodic incidents for this specific board
    public bool incidentIsPlaying = false;

    public int diceRoll;
    [Header("UI Additional Variables")]
    public List<ItemStats> recentStockedItems;
    public int emptyStockCount = 0;

    [SerializeField] private TypewriterCore rollTypewriter;
    public TextMeshProUGUI turnText;

    public GameObject encounterScreen;
    public TextMeshProUGUI p1fight;
    public TextMeshProUGUI p2fight;
    public TextMeshProUGUI resultInfo;
    public bool encounterOver = false;

    public GameObject storeScreen;
    public TextMeshProUGUI storeListings;
    public TextMeshProUGUI storeListingsLabel;

    public MapNode wantedNode;
    private SceneGameManager sceneManager;

    private bool freeviewEnabled = false;
    public bool encounterStarted = false;
    public bool playerUsedItem = false; // please change these down the line
    public bool isStockingStore = false;

    [SerializeField] private List<Stamp.StampType> oldStamps = new List<Stamp.StampType>();
    private int oldPoints = 0;

    // ui stuff for levelup;
    private int attSelected = 1;
    private int diceSelected = 1;
    private int pointsLeft = 0;
    private int currentPlayerInitialHealth = 0; // for pawn shop healing

    //ui to remove for levelup - Nam
    //public Canvas levelUpScreen;
    //public TextMeshProUGUI remainingSP;
    //public TextMeshProUGUI upgradeTooltip;
    //public GameObject diceStats;
    //public List<DiceStatSelectionHandler> playerDiceNumbers = new List<DiceStatSelectionHandler>();
    public TextMeshProUGUI storestockTooltip;
    public int[] costArray = { 0, 1, 1, 2, 2, 2, 3, 3, 3, 4, 5, 999 };

    public Canvas howToPlayScreen;


    // Event Channels
    [Header("Special Event Channels")]
    public VoidEventChannelSO m_IncidentStarted; // broadcasting
    public VoidEventChannelSO m_ActivateIncidentTrain; // broadcasting
    public NodeListFloatEventChannelSO m_DamageAffectedNodes; // listening

    [Header("Broadcast on Event Channels")]
    public PlayerEventChannelSO m_PlayerWon;
    public PlayerListEventChannelSO m_ResultFinalScores;

    public IntEventChannelSO m_NextTurnRound;

    public VoidEventChannelSO m_EnableFreeview;
    public PlayerEventChannelSO m_DiceRollUndo;
    public PlayerEventChannelSO m_DiceRollPrep;
    public IntEventChannelSO m_RollForMovement;
    public VoidEventChannelSO m_PlayerMovedOnBoard;
    public VoidEventChannelSO m_PlayerUndidSomething;
    public IntEventChannelSO m_UpdatePlayerScore;
    public IntEventChannelSO m_PlayerScoreDecreased;
    public IntEventChannelSO m_PlayerScoreIncreased;


    public PlayerEventChannelSO m_PassByStamp;
    public StampEventChannelSO m_UndoPassByStamp;

    public VoidEventChannelSO m_PassByPawnShop;
    public PlayerEventChannelSO m_UndoPassByPawnShop;

    public PlayerEventChannelSO m_NextPlayerTurn;
    public PlayerEventChannelSO m_EncounterDecision;
    public VoidEventChannelSO m_EnteredCombatScene;

    public PlayerEventChannelSO m_OpenInventory; // JASPER OR RUSSELL PLEASE USE THIS EVENT TO ACCESS THE INVENTORY
    public PlayerEventChannelSO m_RefreshInventory;
    public PlayerEventChannelSO m_FullInventory;

    public NodeEventChannelSO m_RestockStore;
    public VoidEventChannelSO m_ExitInventory;

    //public EntityItemListEventChannelSO m_DropItems; // FOR NAM

    public PlayerEventChannelSO m_OverturnOpportunity;

    // Tile-based Event Channels
    public NodeEventChannelSO m_LandOnStorefront;
    public VoidEventChannelSO m_ExitStorefront;

    public NodeEventChannelSO m_LandOnVendor;

    // Pass-by Event Channels
    public PlayerEventChannelSO m_StealOnPassBy;
    public PlayerEventChannelSO m_InitiateCombatOnPassBy;
    public VoidEventChannelSO m_StopOnStoreOnPassBy;

    public VoidEventChannelSO m_EnterRaycastTargetSelection;
    public VoidEventChannelSO m_ExitRaycastTargetSelection;

    public PlayerEventChannelSO m_EnterLevelUp; // also listening to this
    //public VoidEventChannelSO m_ExitLevelUp; // also listening to this
    //public VoidEventChannelSO m_AugmentedDieFaceValue;
    //public VoidEventChannelSO m_FailAugmentDieFaceValue; 

    // Start of Game Event Channels
    public PlayerEventChannelSO m_AssignPlayerToController;

    //public PlayerEventChannelSO m_TransitionIntoCombat;

    [Header("Listen on Event Channels")]
    public VoidEventChannelSO m_DiceRolled;
    public ItemEventChannelSO m_ItemBought; //Listening to this one

    public IntItemEventChannelSO m_TryUseItemAt;
    private int selectedItemIndex; // CHANGE THIS PART LATER

    public IntItemEventChannelSO m_ItemUsed; // after confirm use
    public VoidEventChannelSO m_FinishedUsedItem; // after UseItem timeline is done

    public IntItemEventChannelSO m_ItemStocked;
    public IntItemEventChannelSO m_ItemDiscarded;

    public VoidEventChannelSO m_ExitRaycastedTile; //Listening to this one
    public PlayerEventChannelSO m_BuildStore; //Listening to this one
    public PlayerEventChannelSO m_FinishStockingStore;
    public VoidEventChannelSO m_DisableFreeview;

    public WeaponTypeIntEventChannel m_TryAugmentDieFaceValue; // lvl up

    public IntItemEventChannelSO m_RecieveVendorItem;

    private void OnEnable()
    {
        m_NextTurnRound.OnEventRaised += OnNextTurnRound;

        m_DiceRolled.OnEventRaised += CalculateDiceRoll;
        m_ItemBought.OnEventRaised += ConfirmPurchase;

        m_TryUseItemAt.OnEventRaised += OnUseItemAt;
        m_ItemUsed.OnEventRaised += OnItemUsed;
        m_FinishedUsedItem.OnEventRaised += OnFinishedUsedItem;

        m_UpdatePlayerScore.OnEventRaised += RemoveDeathsRow;
        m_ExitRaycastedTile.OnEventRaised += DisableFreeview;

        m_BuildStore.OnEventRaised += BuildStore;
        m_RestockStore.OnEventRaised += OnRestockStore;
        m_FinishStockingStore.OnEventRaised += AddRecentStockIntoStore;
        m_ItemStocked.OnEventRaised += TrackItemFromPlayerInventory;

        m_ItemDiscarded.OnEventRaised += OnItemDiscarded;

        m_DisableFreeview.OnEventRaised += DisableFreeview;

        m_StealOnPassBy.OnEventRaised += StealFromPlayer;
        m_InitiateCombatOnPassBy.OnEventRaised += InitiateCombatOnPlayer;
        m_StopOnStoreOnPassBy.OnEventRaised += StopOnStore;

        m_RecieveVendorItem.OnEventRaised += OnRecieveVendorItem;

        m_DamageAffectedNodes.OnEventRaised += OnDamageAffectedNodes;
        //m_EnterLevelUp.OnEventRaised += OnEnterLevelUp;
        //m_ExitLevelUp.OnEventRaised += OnExitLevelUp;
        //m_TryAugmentDieFaceValue.OnEventRaised += OnTryAugmentDieFaceValue;
    }

    private void OnDisable()
    {
        m_NextTurnRound.OnEventRaised -= OnNextTurnRound;

        m_DiceRolled.OnEventRaised -= CalculateDiceRoll;
        m_ItemBought.OnEventRaised -= ConfirmPurchase;

        m_TryUseItemAt.OnEventRaised -= OnUseItemAt;
        m_ItemUsed.OnEventRaised -= OnItemUsed;
        m_FinishedUsedItem.OnEventRaised -= OnFinishedUsedItem;

        m_UpdatePlayerScore.OnEventRaised -= RemoveDeathsRow;
        m_ExitRaycastedTile.OnEventRaised -= DisableFreeview;

        m_BuildStore.OnEventRaised -= BuildStore;
        m_RestockStore.OnEventRaised -= OnRestockStore;
        m_FinishStockingStore.OnEventRaised -= AddRecentStockIntoStore;
        m_ItemStocked.OnEventRaised -= TrackItemFromPlayerInventory;

        m_ItemDiscarded.OnEventRaised -= OnItemDiscarded;

        m_DisableFreeview.OnEventRaised -= DisableFreeview;

        m_StealOnPassBy.OnEventRaised -= StealFromPlayer;
        m_InitiateCombatOnPassBy.OnEventRaised -= InitiateCombatOnPlayer;
        m_StopOnStoreOnPassBy.OnEventRaised -= StopOnStore;

        m_RecieveVendorItem.OnEventRaised -= OnRecieveVendorItem;

        m_DamageAffectedNodes.OnEventRaised -= OnDamageAffectedNodes;
        //m_EnterLevelUp.OnEventRaised -= OnEnterLevelUp;
        //m_ExitLevelUp.OnEventRaised -= OnExitLevelUp;
        //m_TryAugmentDieFaceValue.OnEventRaised -= OnTryAugmentDieFaceValue;
    }

    // Start is called before the first frame update
    void Awake()
    {
        debugPhaseText.text = "";

        instance = this;
        sceneManager = GameObject.FindGameObjectWithTag("SceneManager").GetComponent<SceneGameManager>();
        //playerUnits.AddRange(FindObjectsOfType<EntityPiece>());
        //nextPlayers = playerUnits;
        encounterScreen.SetActive(false);

        foreach (var player in playerUnits)
        {
            nextPlayers.Add(player);

            // Allow starting nodes to detect the player on them.
            var initialNode = player.occupiedNode;
            initialNode.playerOccupied = player;

            m_AssignPlayerToController.RaiseEvent(player);
        }

        playerCount = playerUnits.Count;

        // Get the player at the start of the list.
        currentPlayer = nextPlayers[0];
        //currentPlayer = nextPlayers[playerUnits.Count - 1];
        currentPlayerInitialNode = currentPlayer.occupiedNode;

        //turnText.text = currentPlayer.entityName + "'s Turn!";
        //turnText.color = currentPlayer.playerColor;
    }

    private void Start()
    {
        m_NextPlayerTurn.RaiseEvent(currentPlayer);
        //m_NextTurnRound.RaiseEvent(turnRound);
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        debugPhaseText.text = "" + phase;
#endif
        switch (phase)
        {
            // Checks item effects on player
            case GamePhase.ItemSelection:
                SelectItem(currentPlayer);
                break;

            case GamePhase.RaycastTargetSelection:
                SelectRaycastTarget(currentPlayer);
                break;

            // Pick choices
            case GamePhase.InitialTurnMenu:
                InitialTurnMenu(currentPlayer, currentPlayer.occupiedNode);
                break;

            case GamePhase.Inventory:
                OpenInventory(currentPlayer);
                break;

            // Roll Phase 
            case GamePhase.RollDice:
                RollDice(currentPlayer);
                break;

            // Pick Direction to Go Phase
            case GamePhase.PickDirection:
                PickDirection(currentPlayer);
                break;

            // Move-to Node Phase
            case GamePhase.MoveAround:
                MoveAround(currentPlayer);
                break;

            case GamePhase.PassBy:
                PassBy(currentPlayer, currentPlayer.occupiedNode);
                break;

            // Battle-Event Phase
            case GamePhase.EncounterTime:
                EncounterTime(currentPlayer, currentPlayer.occupiedNode);
                break;

            case GamePhase.StockStore:
                StockStore(currentPlayer, currentPlayer.occupiedNode);
                break;

            case GamePhase.OverturnStore:
                OverturnStore(currentPlayer, currentPlayer.occupiedNode);
                break;

            case GamePhase.RockPaperScissors:
                RockPaperScissors(currentPlayer);
                break;

            case GamePhase.LevelUp:
                LevelUp(currentPlayer);
                break;

            // Confirmation Phase
            case GamePhase.ConfirmContinue:
                ConfirmContinue(currentPlayer);
                break;

            // End of turn, next player!
            case GamePhase.EndTurn:
                EndOfTurn(currentPlayer);
                break;

            // Game over! Someone has won!
            case GamePhase.EndGame:
                EndGame();
                break;
        }
    }

    private void SelectItem(EntityPiece p)
    {
        /*
        // all items active for debug
        foreach(var item in p.inventory)
        {
            p.AddItemToActiveEffects(item.duration, item);
            p.inventory.Remove(item);
        }
        */
        foreach (Stamp.StampType s in oldStamps)
        {
            Debug.Log(s);
        }

        p.UpdateStatModifiers();

        ApplyItemEffectsOnTurnStart(p);

        phase = GamePhase.InitialTurnMenu;
    }

    private void InitialTurnMenu(EntityPiece p, MapNode m)
    {
        if (freeviewEnabled)
            return;
        /*
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            // This should let you look around the map freely.
            m_EnableFreeview.RaiseEvent();
            freeviewEnabled = true;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {

            // We chose to begin rolling for movement, tell listeners about it
            m_DiceRollPrep.RaiseEvent(p);

            // Put these in their own listener script

            m_PlayerMovedOnBoard.RaiseEvent();

            // End of listener code

            phase = GamePhase.RollDice;
        }
        if (m.tag == "Encounter"
            && p.heldPoints >= 200 && p.storeCount < 4 
            && (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)))
        {
            /*
            p.storeCount++;
            p.heldPoints -= 200;

            m_UpdatePlayerScore.RaiseEvent(p.id);
            // Raise an eventchannel for BuildAStore to replace the code in here, replace ALOT OF THE CODE EHRE PLEASE
            Debug.Log("I am a store");
            GameObject tile = m.gameObject;
            tile.tag = "Store";

            tile.GetComponent<SpriteRenderer>().color = p.playerColor;

            StoreManager store = tile.AddComponent<StoreManager>();
            store.playerOwner = p;

            isStockingStore = true;

            m_RestockStore.RaiseEvent(m);

            storestockTooltip.enabled = true;
            phase = GamePhase.StockStore;
            
        }
        if (playerUsedItem == false && (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)))
        {
            // Item Inventory
            // We chose to open inventory, tell listeners about it

            m_OpenInventory.RaiseEvent(p);

            // Put these in their own listener script

            m_PlayerMovedOnBoard.RaiseEvent();

            // End of listener code

            phase = GamePhase.Inventory;
        }
        */

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            howToPlayScreen.enabled = true;
        }
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            howToPlayScreen.enabled = false;
        }
    }

    private void OpenInventory(EntityPiece p)
    {
        /*
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Mouse1))
        {
            // Undo rolling, back to menu
            m_ExitInventory.RaiseEvent();

            m_PlayerUndidSomething.RaiseEvent();

            phase = GamePhase.InitialTurnMenu;
        }
        */
    }

    void RollDice(EntityPiece p)
    {
        // For now level up happens right before you roll dice
        if (p.canLevelUp() && p.combatSceneIndex == -1) {

            m_EnterLevelUp.RaiseEvent(p);
        }
        if (p.combatSceneIndex == -1)
        {
            /*
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0))
            {
                diceRoll = Random.Range(1, 7); // Roll from 1 to 6

                var rollsRemaining = currentPlayer.currentStatsModifier.rollModifier;
                while (rollsRemaining > 0)
                {
                    Debug.Log($"Rolls Left{rollsRemaining}");
                    diceRoll += Random.Range(1, 7); // roll again until there's no more
                    rollsRemaining--;
                }

                // Apply movement item effects.
                diceRoll *= currentPlayer.currentStatsModifier.movementMultModifier;
                diceRoll += currentPlayer.currentStatsModifier.movementFlatModifier;    

                
                //diceRoll += 10;   

                // We just rolled for movement, tell listeners about it
                m_RollForMovement.RaiseEvent(diceRoll);

                // Put these in their own listener script
                rollText.text = "" + diceRoll;
                p.movementTotal = p.movementLeft = diceRoll;

                m_PlayerMovedOnBoard.RaiseEvent();

                // End of listener code

                phase = GamePhase.PickDirection;
            }
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Mouse1))
            {
                // Undo rolling, back to menu
                m_DiceRollUndo.RaiseEvent(p);

                m_PlayerUndidSomething.RaiseEvent();

                phase = GamePhase.InitialTurnMenu;
            }
            */
        }
        else
        {

            //TEMPORARY, REMOVE THIS LATER
            m_DiceRollUndo.RaiseEvent(p);

            // In Combat
            phase = GamePhase.EncounterTime;
        }
    }

    void CalculateDiceRoll()
    {
        diceRoll = Random.Range(1, 7); // Roll from 1 to 6

        var rollsRemaining = currentPlayer.currentStatsModifier.rollModifier;
        while (rollsRemaining > 0)
        {
            Debug.Log($"Rolls Left{rollsRemaining}");
            diceRoll += Random.Range(1, 7); // roll again until there's no more
            rollsRemaining--;
        }

        // Apply movement item effects.
        diceRoll *= currentPlayer.currentStatsModifier.movementMultModifier;
        diceRoll += currentPlayer.currentStatsModifier.movementFlatModifier;

        currentPlayer.movementTotal = currentPlayer.movementLeft = diceRoll;
        m_RollForMovement.RaiseEvent(diceRoll);
        m_PlayerMovedOnBoard.RaiseEvent(); // idk why this has to be a seperate event
    }

    void PickDirection(EntityPiece p)
    {
        /*
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            wantedNode = p.occupiedNode.north;

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            wantedNode = p.occupiedNode.east;

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            wantedNode = p.occupiedNode.south;

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            wantedNode = p.occupiedNode.west;

        if (wantedNode != null && !(wantedNode == p.previousNode && p.traveledNodes.Count <= 1) )
        {
            phase = GamePhase.MoveAround;
        }
        */
    }

    void MoveAround(EntityPiece p)
    {
        var lastEle = p.traveledNodes.Count;
        var lastNode = p.traveledNodes[lastEle - 1];
        lastNode = p.traveledNodes[^1];

        if (wantedNode == lastNode) // The direction you picked was the node you just came from (Redo)
        {
            Stamp stampCollected = p.occupiedNode.gameObject.GetComponent<Stamp>();

            // Undo moves (not sure if this actually undoes multiple stamp collections)
            if (p.occupiedNode.CompareTag("Castle"))
            {
                p.stamps = new List<Stamp.StampType>(oldStamps);
                p.heldPoints = oldPoints;
                p.health = currentPlayerInitialHealth; // revert healing back

                //oldStamps.Remove(stampCollected.stampType);
                //oldPoints = 0;

                m_UpdatePlayerScore.RaiseEvent(currentPlayer.id);
                m_UndoPassByPawnShop.RaiseEvent(p);
            }
            else if (stampCollected != null)
            {
                // Stamp was not collected before
                if (p.stamps.Contains(stampCollected.stampType) && !oldStamps.Contains(stampCollected.stampType))
                {
                    Debug.Log("undo stamp");
                    Debug.Log($"Old Stamp Contains {stampCollected.stampType} is {oldStamps.Contains(stampCollected.stampType)}");

                    p.stamps.Remove(stampCollected.stampType);
                    m_UndoPassByStamp.RaiseEvent(stampCollected.stampType); // shit code fix later
                }
            }

            p.traveledNodes.Remove(lastNode);
            p.occupiedNode = lastNode;

            p.movementLeft++;

            string roll = "" + p.movementLeft;

            rollTypewriter.ShowText(roll);

            p.transform.DOMove(lastNode.transform.position, .25f)
                .SetEase(DG.Tweening.Ease.OutQuint);

            wantedNode = null;
            m_PlayerUndidSomething.RaiseEvent();

            phase = GamePhase.PickDirection; // Go back to picking direction
        }
        else if (wantedNode != null) // Go to that new node and occupy it
        {
            if (wantedNode.modifier == MapNode.Modifier.Rafflesia) {
                wantedNode.modifier = MapNode.Modifier.None;
                p.movementLeft = 0;

                rollTypewriter.ShowText("");

                p.previousNode = p.traveledNodes[p.traveledNodes.Count - 1];

                wantedNode.flowerTrapVisual.color = new Color32(0, 0, 0, 0);
                wantedNode = null;
                //audioSource.PlayOneShot(moveSFX, 1.2f);


                p.traveledNodes.Clear();
                p.traveledNodes.Add(p.occupiedNode);
                phase = GamePhase.EncounterTime; // next phase
                return;

            }
            p.traveledNodes.Add(p.occupiedNode);
            p.occupiedNode = wantedNode;

            p.movementLeft--;

            string roll = "" + p.movementLeft;
            if (p.movementLeft == 0)
                rollTypewriter.ShowText("");
            else
                rollTypewriter.ShowText(roll);

            p.transform.DOMove(wantedNode.transform.position, .25f)
                .SetEase(DG.Tweening.Ease.OutQuint);


            wantedNode = null;
            m_PlayerMovedOnBoard.RaiseEvent();

            phase = GamePhase.PassBy;
        }

    }

    void PassBy(EntityPiece p, MapNode m)
    {
        // Cash in Stamps
        if (m.CompareTag("Castle"))
        {
            oldPoints = p.heldPoints;
            oldStamps = new List<Stamp.StampType>(p.stamps);

            if (p.stamps.Count != 0)
            {
                p.ReputationPoints += (75 * Mathf.Pow(1.5f, p.stamps.Count - 1));
                p.heldPoints += (int)(150 * Mathf.Pow(2, p.stamps.Count - 1));
                //m_UpdatePlayerScore.RaiseEvent(currentPlayer.id);
                m_PlayerScoreIncreased.RaiseEvent((int)(150 * Mathf.Pow(2, p.stamps.Count - 1)));
                m_PassByPawnShop.RaiseEvent(); // change this later

                p.stamps.Clear();
            }

            // Heal player by 33%
            currentPlayerInitialHealth = p.health; // track health before, in case they undo
            p.health += p.maxHealth / 3;
            if (p.health > p.maxHealth)
            {
                p.health = p.maxHealth;
            }
            m_UpdatePlayerScore.RaiseEvent(currentPlayer.id);

            if (p.heldPoints >= targetGoal)
            {
                Debug.Log("BRO HE WON");
                winner = p;
                phase = GamePhase.EndGame; // Finish game if player w/ enough points passes by Pawn Shop
                return;
            }
        }
        else if (m.CompareTag("Stamp"))
        {
            Stamp.StampType stampToBeCollected = m.gameObject.GetComponent<Stamp>().stampType;
            if (!p.stamps.Contains(stampToBeCollected))
            {
                Debug.Log($"Collect {stampToBeCollected} stamp passed");
                oldStamps = new List<Stamp.StampType>(p.stamps);
                p.stamps.Add(stampToBeCollected);
                //m_UpdatePlayerScore.RaiseEvent(currentPlayer.id); // Change this to a different event
                m_PassByStamp.RaiseEvent(p);
            }
        }

        if (m.playerOccupied != null && m.playerOccupied != p)
        {
            EntityPiece otherPlayer = m.playerOccupied;
            Debug.Log("hello");
            // Check if can steal item from player.
            if (p.currentStatsModifier.canStealOnPassBy && otherPlayer.inventory.Count > 0)
            {
                Debug.Log("Steal");
                m_StealOnPassBy.RaiseEvent(otherPlayer);

                // Deactivate all active effects of items that end on stealing.
                p.RemoveItemEffectOnUse(ItemLists.StealOnPassByItemNames);
            }

            // Check if can initiate combat.
            if (p.currentStatsModifier.canInitiateCombatOnPassBy)
            {
                Debug.Log("Combat");
                p.RemoveItemEffectOnUse(ItemLists.CombatOnPassByItemNames);

                if (otherPlayer.combatSceneIndex == -1)
                {
                    phase = GamePhase.CombatTime;

                    StartCoroutine(StartTransitionIntoCombat(.5f, otherPlayer));
                    //m_EnteredCombatScene.RaiseEvent();
                    //m_InitiateCombatOnPassBy.RaiseEvent(otherPlayer);

                    p.traveledNodes.Clear();
                    p.traveledNodes.Add(p.occupiedNode);

                    // Need NAM to disable input prompt (the number that shows up on top of the screen on roll).
                    // If enter combat before it fades, it persists on next player's turn.
                    return;
                }
            }


        }
        if (m.CompareTag("Store") && p.currentStatsModifier.canStopOnStoreOnPassBy)
        {
            p.previousNode = p.traveledNodes[p.traveledNodes.Count - 1];
            m_StopOnStoreOnPassBy.RaiseEvent();

            // Deactivate all active effects of items that end on store.
            p.RemoveItemEffectOnUse(ItemLists.StopOnStoreOnPassBy);
        }
        else if (m.CompareTag("Store") && (m.modifier == MapNode.Modifier.Marigold && m.modifierOwner != p)) {
            m.flowerTrapVisual.color = new Color32(0, 0, 0, 0);
            p.previousNode = p.traveledNodes[p.traveledNodes.Count - 1];
            m_StopOnStoreOnPassBy.RaiseEvent();

            m.modifier = MapNode.Modifier.None;
        }

        // Change phase.

        else if (p.movementLeft <= 0)
        {
            p.previousNode = p.traveledNodes[p.traveledNodes.Count - 1];
            p.traveledNodes.Clear();
            p.traveledNodes.Add(p.occupiedNode);

            if (turnOffMonsterEncounters)
                phase = GamePhase.EndTurn;
            else
                phase = GamePhase.EncounterTime; // next phase
        }
        else
            phase = GamePhase.PickDirection; // Go back to picking direction
    }

    void EncounterTime(EntityPiece p, MapNode m)
    {
        // Player has started combat
        if (p.combatSceneIndex != -1)
        {
            phase = GamePhase.CombatTime;
            m_EnteredCombatScene.RaiseEvent();
            sceneManager.DisableScene(0);
            sceneManager.EnableScene(p.combatSceneIndex);
        }
        // Player has not started combat
        else
        {
            var otherPlayer = p.occupiedNode.playerOccupied;
            if (m.TryGetComponent<StoreManager>(out StoreManager component)) // Forced to buy item(s)
            {
                // Have the node be occupied by the current player.
                m.playerOccupied = p;
                // Update portions of this code later
                GameObject tile = m.gameObject;
                StoreManager store = component;
                if (otherPlayer != null && otherPlayer != currentPlayer) // temp player fight on store
                {
                    if (otherPlayer.combatSceneIndex == -1)
                    {
                        phase = GamePhase.CombatTime;

                        //Debug.Log("Your Player: " + currentPlayer.nickname);
                        //Debug.Log("Other Player: " + otherPlayer.nickname);
                        //m_EnteredCombatScene.RaiseEvent();
                        encounterStarted = true;

                        // Set IDs of players entering combat.
                        //sceneManager.player1ID = currentPlayer.id;
                        //sceneManager.player2ID = otherPlayer.id;

                        StartCoroutine(StartTransitionIntoCombat(.5f, otherPlayer));
                        //m_EnteredCombatScene.RaiseEvent();
                        //m_InitiateCombatOnPassBy.RaiseEvent(otherPlayer);

                        //sceneManager.LoadCombatScene();
                    }
                }
                else if (store.playerOwner != currentPlayer)
                {

                    // Forced to buy item(s) from another player's store
                    Debug.Log("Landed on " + store.playerOwner + " store");

                    // If there are no items left, give the player the option to overturn.
                    if (store.storeInventory.Find(x => x != null) == null)
                    {
                        m_OverturnOpportunity.RaiseEvent(store.playerOwner);
                        phase = GamePhase.OverturnStore;
                    }
                    else
                    {
                        m_LandOnStorefront.RaiseEvent(m);
                        phase = GamePhase.InStore;
                    }
                }
                else
                {
                    Debug.Log("Restock the store on landing on it plaz");
                    isStockingStore = true;
                    m_RestockStore.RaiseEvent(m);
                    //m_OpenInventory.RaiseEvent(p); // COMMENT THIS OUT WHEN RAISING THE RESTOCK EVENT
                    storestockTooltip.enabled = true; // PROBABLY PUT THIS IN UI AS WELL
                    phase = GamePhase.StockStore;
                }
            }
            else if (m.CompareTag("Castle"))
            {
                // Clear your direction so you can choose next turn
                p.previousNode = null;

                encounterOver = true;
                phase = GamePhase.EndTurn;
            }
            else if (m.CompareTag("Vendor"))
            {
                Debug.Log("On vendor");
                if (otherPlayer != null && otherPlayer != currentPlayer
                    && otherPlayer.combatSceneIndex == -1)
                {
                    Debug.Log("person here");
                    if (!otherPlayer.currentStates.Contains(EntityPiece.State.InsideVendor))
                    {
                        Debug.Log("fight person on vendor here");
                        phase = GamePhase.CombatTime;
                        StartCoroutine(StartTransitionIntoCombat(.5f, otherPlayer));

                        encounterStarted = true;
                    }
                    else
                    {
                        m_LandOnVendor.RaiseEvent(m);
                        phase = GamePhase.InVendor;
                    }
                }
                else
                {
                    m_LandOnVendor.RaiseEvent(m);
                    phase = GamePhase.InVendor;
                }
            }
            else if (m.CompareTag("Stamp"))
            {
                /*
                encounterScreen.SetActive(true);
                p1fight.text = "";
                p2fight.text = "";
                resultInfo.text = "<size=45>[SAFE SPACE]</size>\nLanded on a stamp space.\nYou are safe from combat on this space.";
                resultInfo.text += "\n<size=24>[SPACE] to continue</size>";

                encounterOver = true;
                phase = GamePhase.ConfirmContinue;
                */

                // Clear your direction so you can choose next turn (TEMPORARY)
                p.previousNode = null;

                encounterOver = true;
                phase = GamePhase.EndTurn;
            }
            else if (m.CompareTag("Encounter") && otherPlayer != null && otherPlayer != currentPlayer) // Player Fight
            {
                // If current player is not in combat scene and other player is not in combat scene, begin combat.
                // If current player is in combat scene, skip roll dice and load back here

                if (otherPlayer.combatSceneIndex == -1)
                {
                    phase = GamePhase.CombatTime;

                    //Debug.Log("Your Player: " + currentPlayer.nickname);
                    //Debug.Log("Other Player: " + otherPlayer.nickname);
                    StartCoroutine(StartTransitionIntoCombat(.5f, otherPlayer));
                    //m_EnteredCombatScene.RaiseEvent();
                    //m_InitiateCombatOnPassBy.RaiseEvent(otherPlayer);

                    encounterStarted = true;

                    // Set IDs of players entering combat.
                    //sceneManager.player1ID = currentPlayer.id;
                    //sceneManager.player2ID = otherPlayer.id;
                    //sceneManager.LoadCombatScene();
                }
                else
                {
                    phase = GamePhase.EndTurn;
                }
            }
            else if (m.CompareTag("Encounter")) // Regular Encounter
            {
                /*
                // If unable to buy a store, skip the prompt and immediately enter combat.
                if (p.storeCount >= 4)
                {
                    Debug.Log("You got no money to build a store, dipshit!");
                    phase = GamePhase.RockPaperScissors;
                    return;
                }

                m_EncounterDecision.RaiseEvent(currentPlayer);

                // Monster Encounter
                if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Space))
                {
                    phase = GamePhase.RockPaperScissors;
                }
                // Build a Store
                else if (Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.RightShift) || Input.GetKeyDown(KeyCode.LeftShift))
                {
                    p.storeCount++;
                    //p.heldPoints -= 200;

                    m_UpdatePlayerScore.RaiseEvent(p.id);
                    //TEMPORARY, later put in build store event and sound
                    //m_PlayerScoreDecreased.RaiseEvent(-200);
                    // Raise an eventchannel for BuildAStore to replace the code in here, replace ALOT OF THE CODE EHRE PLEASE
                    Debug.Log("I am a store");
                    GameObject tile = m.gameObject;
                    tile.tag = "Store";

                    tile.GetComponent<SpriteRenderer>().color = p.playerColor;

                    StoreManager store = tile.AddComponent<StoreManager>();
                    store.playerOwner = p;

                    isStockingStore = true;

                    m_RestockStore.RaiseEvent(m);
                    
                    storestockTooltip.enabled = true;
                    phase = GamePhase.StockStore;

                }
                */
                StartCoroutine(InitiateCombatOnEnemy(.45f, p));

                phase = GamePhase.RockPaperScissors;
            }
        }
    }

    void OverturnStore(EntityPiece p, MapNode m)
    {
        phase = GamePhase.EndTurn; //TEMPORARY UNTIL I FIX THIS

        /*
        // No money to overturn or at store cap.
        if (p.heldPoints < 600 || p.storeCount >= 4)
        {
            phase = GamePhase.EndTurn;
        }
        else
        {
            // Overturn.
            if (Input.GetKeyDown(KeyCode.RightShift) || Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Mouse1))
            {
                GameObject tile = m.gameObject;
                tile.GetComponent<SpriteRenderer>().color = currentPlayer.playerColor;
                StoreManager store = tile.GetComponent<StoreManager>();
               
                // Ownership changes.
                store.playerOwner.storeCount--;
                store.playerOwner.heldPoints += 600;
                store.playerOwner = currentPlayer;
                p.storeCount++;

                p.heldPoints -= 600;

                m_UpdatePlayerScore.RaiseEvent(currentPlayer.id);
                //TEMPORARY, later replace with overturn store event and sound
                m_PlayerScoreDecreased.RaiseEvent(-600);


                isStockingStore = true;

                m_RestockStore.RaiseEvent(m);
                storestockTooltip.enabled = true; // PROBABLY PUT THIS IN UI AS WELL
                phase = GamePhase.StockStore;
            }
            else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0)) // Don't overturn.
            {
                phase = GamePhase.EndTurn;
            }
        }
        */
    }

    void RockPaperScissors(EntityPiece p)
    {
        /*
        //Raise event for moving to combat scene
        m_EnteredCombatScene.RaiseEvent();
        encounterStarted = true;

        // Set IDs of players entering combat.
        sceneManager.player1ID = p.id;

        bool lookingForTarget = true;
        while (lookingForTarget) {
            var monsterType = Random.Range(-8, 0); // int from -6 to -1

            sceneManager.player2ID = monsterType;

            var enemy = sceneManager.entities.Find(entity => sceneManager.player2ID == entity.id);
            if (enemy.combatSceneIndex == -1) {
                float spawnChance = Random.Range(0f, 1f);

                if (spawnChance < enemy.spawnRarityModifier) {
                    lookingForTarget = false;
                }
            }
                
        }
        //phase = GamePhase.CombatTime;
        sceneManager.LoadCombatScene();
        */

    }

    /*
    private void printIndex(int row, int col, EntityPiece p) { // Temporary function, delete later.
        int[] costArray = { -1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 5, 999 };
        if (row == 1) {
            int diceValue = (int)(p.strDie.dieFaces[diceSelected-1]);
            int diceUpgradeCost = costArray[diceValue];
            Debug.Log("Strength dice face " + col + " selected. Current value: " + diceValue + ". Upgrade cost " + diceUpgradeCost);
            upgradeTooltip.text = $"<sprite=0> <color=red>[{diceValue}]</color> costs {diceUpgradeCost} SP to upgrade.";
        } else if (row == 2) {
            int diceValue = (int)(p.dexDie.dieFaces[diceSelected-1]);
            int diceUpgradeCost = costArray[diceValue];
            Debug.Log("Dex dice face " + col + " selected. Current value: " + diceValue + ". Upgrade cost " + diceUpgradeCost);
            upgradeTooltip.text = $"<sprite=1> <color=blue>[{diceValue}]</color> costs {diceUpgradeCost} SP to upgrade.";

        } else {
            int diceValue = (int)(p.intDie.dieFaces[diceSelected-1]);
            int diceUpgradeCost = costArray[diceValue];
            Debug.Log("Magic dice face " + col + " selected. Current value: " + diceValue + ". Upgrade cost " + diceUpgradeCost);
            upgradeTooltip.text = $"<sprite=2> <color=purple>[{diceValue}]</color> costs {diceUpgradeCost} SP to upgrade.";

        }
    }
    */

    private void LevelUp(EntityPiece p)
    {
        // current attribute selected is marked by the int attSelected. 1 is attack, 2 is gun, 3 is magic.
        // diceSelected represents the current dice face. All of these have -1 applied in arrays.
        /*
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            if (attSelected > 1) {
                attSelected -= 1;
            }
            printIndex(attSelected, diceSelected, p);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            if (attSelected < 3) {
                attSelected += 1;
            }
            printIndex(attSelected, diceSelected, p);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {

            if (diceSelected < 6) {
                diceSelected += 1;
            }
            printIndex(attSelected, diceSelected, p);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {

            if (diceSelected > 1) {
                diceSelected -= 1;
            }
            printIndex(attSelected, diceSelected, p);
        }


        if (pointsLeft <= 0 || Input.GetKeyDown(KeyCode.Escape)) // Esc to leave
        {
            Debug.Log("Out of points, level up done");
            levelUpScreen.enabled = false;
            p.unspentLevelUpPoints = pointsLeft;
            phase = GamePhase.RollDice;
        }
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)) // E to leave till I find a good exit method that doesn't get you stuck.
        {
            int[] costArray = { -1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 5, 999 }; // 0-1 (0 indexing issues), 1-2, 2-3, etc. 10-11 isnt possible.
            if (attSelected == 1) {
                int diceUpgradeCost = costArray[ (int)(p.strDie.dieFaces[diceSelected-1]) ];
                if (pointsLeft >= diceUpgradeCost) {
                    p.strDie.dieFaces[diceSelected-1] += 1;
                    pointsLeft -= diceUpgradeCost;
                    Debug.Log ("Strength Die upgraded on face " + diceSelected + " making it " + p.strDie.dieFaces[diceSelected-1]);
                    Debug.Log (pointsLeft + " points left after paying " + diceUpgradeCost);
                    remainingSP.text = $"{pointsLeft} SP left.";
                    UpdatePlayerDiceStats(p, diceStats);
                } else {
                    upgradeTooltip.text = "Not enough points. Costs " + diceUpgradeCost + " SP but you have only " + pointsLeft;
                    Debug.Log("Not enough points. Costs " + diceUpgradeCost + ", but you have only " + pointsLeft);
                }
            } else if (attSelected == 2) {
                int diceUpgradeCost = costArray[ (int)(p.dexDie.dieFaces[diceSelected-1]) ];
                if (pointsLeft >= diceUpgradeCost) {
                    p.dexDie.dieFaces[diceSelected-1] += 1;
                    pointsLeft -= diceUpgradeCost;
                    Debug.Log ("Dex Die upgraded on face " + diceSelected + " making it " + p.dexDie.dieFaces[diceSelected-1]);
                    Debug.Log (pointsLeft + " points left after paying " + diceUpgradeCost);
                    remainingSP.text = $"{pointsLeft} SP left.";
                    UpdatePlayerDiceStats(p, diceStats);
                } else {
                    upgradeTooltip.text = "Not enough points. Costs " + diceUpgradeCost + " SP but you have only " + pointsLeft;
                    Debug.Log("Not enough points. Costs " + diceUpgradeCost + ", but you have only " + pointsLeft);
                }
            } else if (attSelected == 3) {
                int diceUpgradeCost = costArray[ (int)(p.intDie.dieFaces[diceSelected-1]) ];
                if (pointsLeft >= diceUpgradeCost) {
                    p.intDie.dieFaces[diceSelected-1] += 1;
                    pointsLeft -= diceUpgradeCost;
                    Debug.Log ("Magic Die upgraded on face " + diceSelected + " making it " + p.intDie.dieFaces[diceSelected-1]);
                    Debug.Log (pointsLeft + " points left after paying " + diceUpgradeCost);
                    remainingSP.text = $"{pointsLeft} SP left.";
                    UpdatePlayerDiceStats(p, diceStats);
                } else {
                    upgradeTooltip.text = "Not enough points. Costs " + diceUpgradeCost + " SP but you have only " + pointsLeft;
                    Debug.Log("Not enough points. Costs " + diceUpgradeCost + ", but you have only " + pointsLeft);
                }
            } else {
                Debug.Log("Uh oh, you just tried to upgrade a dice you dont have.");
            }
        }
        */
    }

    void ConfirmContinue(EntityPiece p)
    {
        /*
        if (encounterOver && Input.GetKeyDown(KeyCode.Alpha9))
        {
            phase = GamePhase.EndTurn;
            encounterOver = false;
            encounterScreen.SetActive(false);
            storeScreen.SetActive(false);
            m_UpdatePlayerScore.RaiseEvent(currentPlayer.id);
            m_ExitStorefront.RaiseEvent();
        }
        */
    }

    public void ConfirmContinue()
    {
        phase = GamePhase.EndTurn;
        encounterOver = false;
        encounterScreen.SetActive(false);
        storeScreen.SetActive(false);
        //m_UpdatePlayerScore.RaiseEvent(currentPlayer.id);
        m_ExitStorefront.RaiseEvent();
    }

    public IEnumerator DelayFullInventory(EntityPiece p, float delay)
    {
        yield return new WaitForSeconds(delay);
        m_FullInventory.RaiseEvent(p);
        yield return null;
    }

    void EndOfTurn(EntityPiece p)
    {
        if(p.inventory.Count > p.inventoryLimit)
        {
            // Player has too many items
            Debug.Log($"{p.entityName} HAS TOO MANY ITEMS");
            phase = GamePhase.DiscardItem;
            StartCoroutine(DelayFullInventory(p, .25f));
            //m_FullInventory.RaiseEvent(p);
            //phase = GamePhase.DiscardItem;
        }
        else
        {
            // Regular turn end logic
            if (currentPlayerInitialNode.playerOccupied == currentPlayer)
            {
                currentPlayerInitialNode.playerOccupied = null;
            }

            // Reset temp values.
            oldStamps.Clear();
            oldPoints = 0;

            p.occupiedNode.playerOccupied = p; // update to have that player on that node now

            rollTypewriter.ShowText("");

            isStockingStore = false; // let next player access inventory
            playerUsedItem = false; // let next player access inventory

            m_UpdatePlayerScore.RaiseEvent(currentPlayer.id);
            rollTypewriter.ShowText("");


            playersActed++;
            if (playersActed == playerCount)
            {
                Debug.Log($"Round {turnRound} over.");
                turnRound++;
                playersActed = 0;

                // Raise event that round is over with the current round #.
                m_NextTurnRound.RaiseEvent(turnRound);
                //SetupNextPlayer();
            }
            else
            {
                SetupNextPlayer();
            }
        }
    }

    public void OnNextTurnRound(int round)
    {
        bool active = false;
        foreach (SpecialIncidents incidents in matchIncidents)
        {
            switch (incidents)
            {
                case SpecialIncidents.Train:
                    if (round % 6 == 0)
                    {
                        //m_ActivateIncidentTrain.RaiseEvent();
                        incidentsToActivate.Add(SpecialIncidents.Train);
                        active = true;
                    }
                    break;
                default:
                    Debug.Log("default, idk what incident this is");
                    break;
            }
        }

        if (active)
        {
            Debug.Log("There's incidents to make, play them out");
            phase = GamePhase.IncidentHappening;
            m_IncidentStarted.RaiseEvent();
            StartCoroutine(PlayOutActiveIncidents());
        }
        else
        {
            SetupNextPlayer();
        }
    }

    public void SetupNextPlayer()
    {
        // Change to the next player in the list.
        nextPlayers.Remove(currentPlayer);
        nextPlayers.Add(currentPlayer);
        currentPlayer = nextPlayers[0];

        // Done eating and chillin, get out of the estalbishment!
        if (currentPlayer.currentStates.Contains(EntityPiece.State.InsideVendor))
        {
            currentPlayer.currentStates.Remove(EntityPiece.State.InsideVendor);
            currentPlayer.playerSprite.enabled = true;
        }

        m_NextPlayerTurn.RaiseEvent(currentPlayer);

        currentPlayerInitialNode = currentPlayer.occupiedNode;
        oldStamps = new List<Stamp.StampType>(currentPlayer.stamps); // keeping track of stamps for next player
        phase = GamePhase.ItemSelection;
    }

    public IEnumerator PlayOutActiveIncidents()
    {
        foreach (SpecialIncidents incidents in incidentsToActivate)
        {
            incidentIsPlaying = true;
            switch (incidents)
            {
                case SpecialIncidents.Train:
                    m_ActivateIncidentTrain.RaiseEvent();
                    break;
                default:
                    Debug.Log($"Some {incidents} Incident should play here.");
                    break;

            }
            yield return new WaitUntil(() => incidentIsPlaying == false);
        }
        incidentsToActivate.Clear();
        SetupNextPlayer();
    }

    void EndGame()
    {
        // The player with the most points wins!
        if (winner == null)
        {
            // This will happen when someone dies during Death's Row
            winner = playerUnits.OrderBy(playerUnit => playerUnit.heldPoints).LastOrDefault();
        }
        m_PlayerWon.RaiseEvent(winner);

        var playersByPlacement = playerUnits.OrderBy(playerUnit => playerUnit.heldPoints).ToList();
        m_ResultFinalScores.RaiseEvent(playersByPlacement);
        
        /*
        // UPDATE WITH ACTUAL END GAME UI, AND MAKE IN DIFFERENT SCRIPT WITH EVENT RAISED HERE.
        Debug.Log(winner.entityName + " is the KING OF THE MARKET!");
        encounterScreen.SetActive(true);
        resultInfo.text = $"{winner.entityName} is the \nWINNER!!!";
        */

        phase = GamePhase.GameOver;
    }

    private void ConfirmPurchase(ItemStats item)
    {
        // When an item is bought, allow confirmation via SPACE bar to continue the game
        if (item != null)
        {
            UpdateStorefrontVisual(currentPlayer.occupiedNode.GetComponent<MapNode>());
            //encounterOver = true;
            currentPlayer.heldPoints -= item.basePrice;
            currentPlayer.ReputationPoints += 20 + (item.basePrice / 10);

            m_UpdatePlayerScore.RaiseEvent(currentPlayer.id);

            if (currentPlayer.inventory.Count > currentPlayer.inventoryLimit)
            {
                List<ItemStats> incomingItems = new List<ItemStats> { item };

                // FOR NAM
                // Raise drop item event, display drop item UI.
                // Set encounterOver = true when player has dropped enough items and close drop item UI (maybe in same function?).

            }
            else
            {
                encounterOver = true;
            }
        }
    }

    private void OnUseItemAt(int index, ItemStats item)
    {
        // Recieved from InvSelHand to try using an item
        selectedItemIndex = index;
    }

    private void OnItemUsed(int index, ItemStats item)
    {
        //RemoveItemInPlayerInventory(index);
    }

    private void OnFinishedUsedItem()
    {
        Debug.Log("Finished used item");
        RemoveItemInPlayerInventory(selectedItemIndex);
    }

    public void RemoveItemInPlayerInventory(int index)
    {
        // This should be in its own script
        if (isStockingStore)
        {
            Debug.Log("remove item when isStockingStore");
            /*
            var store = currentPlayer.occupiedNode.GetComponent<StoreManager>();
            //Debug.Log(index);
            store.AddItem(currentPlayer.inventory[index]);
            currentPlayer.inventory.RemoveAt(index);
            */
            //m_OpenInventory.RaiseEvent(currentPlayer);
        }
        else
        {
            // Ask player for confirmation to use item [YES/NO]
            // Raise some event here to show prompt
            // ...

            // For when player says YES (PUT THIS IN A SEPERATE FUNCTION)
            // play the PD_UseItem timeline asset here

            // get signalled from the end of timeline sequence to actually give item effect (SEPERATE FUNCTION)
            currentPlayer.AddItemToActiveEffects(currentPlayer.inventory[index].Duration, currentPlayer.inventory[index]);

            currentPlayer.UpdateStatModifier(new EntityPiece.ActiveEffect
            {
                originalItem = currentPlayer.inventory[index],
                turnsRemaining = currentPlayer.inventory[index].Duration - 1
            });

            ApplyItemEffectsOnTurnStart(currentPlayer);

            currentPlayer.inventory.RemoveAt(index);
            playerUsedItem = true;

            if (currentPlayer.currentStatsModifier.warpMode != EntityStatsModifiers.WarpMode.None)
            {
                Debug.Log("Used target select item");
                m_ExitInventory.RaiseEvent();
                // Raise free view event I guess?
                m_EnableFreeview.RaiseEvent();
                // FOR NAM: USE THIS EVENT TO SHOW SELECT TILE/PLAYER UI.
                m_EnterRaycastTargetSelection.RaiseEvent();
                freeviewEnabled = true;

                phase = GamePhase.RaycastTargetSelection;
            }
            else
            {
                // only get rid of item if its not a target selection one
                //currentPlayer.inventory.RemoveAt(index);
                //playerUsedItem = true;
                Debug.Log("Used normal item");
                m_ExitInventory.RaiseEvent();
            }
        }
    }

    private void DropItemInPlayerInventory(int index)
    {
        // FOR NAM, to remove item from inventory upon click.
        // probably want to track how many items have been removed too and close the drop item UI
        // when it reaches the necessary amount.
        currentPlayer.inventory.RemoveAt(index);
    }

    private void ApplyItemEffectsOnTurnStart(EntityPiece p)
    {
        // Regenerate health from active effects.
        p.health = Mathf.Min(p.maxHealth * p.currentStatsModifier.maxHealthMultModifier
            + p.currentStatsModifier.maxHealthFlatModifier,
            p.health + p.currentStatsModifier.healthRegen);

        if(p.health < 1)
        {
            p.health = 1;
        }

        m_UpdatePlayerScore.RaiseEvent(p.id);

        // Warp player to specified destination.
        if (p.currentStatsModifier.warpDestination != null)
        {
            p.occupiedNode = p.currentStatsModifier.warpDestination;
            p.transform.position = p.occupiedNode.transform.position;
            p.occupiedNodeCopy = p.occupiedNode;
            p.traveledNodes.Clear();
            p.traveledNodes.Add(p.occupiedNode);
        }
    }

    private void ApplyItemEffectsOnTargetSelection(EntityPiece p)
    {
        // Warp player to specified destination.
        if (p.currentStatsModifier.warpDestination != null)
        {
            p.occupiedNode = p.currentStatsModifier.warpDestination;
            p.transform.position = p.occupiedNode.transform.position;
            p.occupiedNodeCopy = p.occupiedNode;
            p.traveledNodes.Clear();
            p.traveledNodes.Add(p.occupiedNode);
        }
    }

    private void RemoveDeathsRow(int id)
    {
        if (id > -1 && playerUnits[id].heldPoints >= 0 && playerUnits[id].currentStates.Contains(EntityPiece.State.DeathsRow))
        {
            Debug.Log(playerUnits[id].entityName + " is no longer in Death's Row");
            //playerUnits[id].isInDeathsRow = false;
            playerUnits[id].currentStates.Remove(EntityPiece.State.DeathsRow);
        }
    }

    private void StockStore(EntityPiece p, MapNode m)
    {
        /*
        StoreManager store = p.occupiedNode.GetComponent<StoreManager>();
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Mouse1)
                || !store.storeInventory.Exists(x => x == null))
        {
            // Exit store restocking.
            m_ExitInventory.RaiseEvent(); // REMOVE STOCKING STORE UI FROM SCREEN TOO
            storestockTooltip.enabled = false; // DO THIS IN THE UI
            m_PlayerUndidSomething.RaiseEvent();

            phase = GamePhase.EndTurn;
        }
        */
    }

    public void DisableFreeview()
    {
        freeviewEnabled = false;
    }

    public void BuildStore(EntityPiece p)
    {
        p.storeCount++;
        //p.heldPoints -= 200;

        m_UpdatePlayerScore.RaiseEvent(p.id);
        // Raise an eventchannel for BuildAStore to replace the code in here, replace ALOT OF THE CODE EHRE PLEASE
        Debug.Log("I am a store");
        GameObject tileObject = p.occupiedNode.gameObject;
        tileObject.tag = "Store";

        tileObject.GetComponent<SpriteRenderer>().color = p.playerColor;
        var node = tileObject.GetComponent<MapNode>();

        StoreManager store = tileObject.AddComponent<StoreManager>();
        store.playerOwner = p;

        //Show store boat now
        node.storefrontVisual.enabled = true;
        node.storefrontVisual.GetComponent<SpriteRenderer>().color = p.playerColor;

        node.stockGroup.alpha = 1;
        node.stockGroup.GetComponent<Image>().color = p.playerColor;
        node.stockGroup.GetComponent<Image>().color -= new Color(0, 0, 0, .25f);


        isStockingStore = true;

        //m_RestockStore.RaiseEvent(p.occupiedNode);

        storestockTooltip.enabled = true;
    }

    public void OnRestockStore(MapNode node)
    {
        emptyStockCount = 0;
        var storeInventory = node.GetComponent<StoreManager>().storeInventory;
        foreach (ItemStats item in storeInventory)
        {
            if (item == null)
                emptyStockCount++;
        }
    }

    public void TrackItemFromPlayerInventory(int index, ItemStats itemStats)
    {
        // Keep track of items stocked
        // this is to make sure if player cancels their selection you can give back the items
        recentStockedItems.Add(itemStats);
        emptyStockCount--;

        currentPlayer.inventory.RemoveAt(index);
        m_RefreshInventory.RaiseEvent(currentPlayer);

        if (emptyStockCount <= 0)
            m_FinishStockingStore.RaiseEvent(currentPlayer);
    }

    public void AddRecentStockIntoStore(EntityPiece player)
    {
        StoreManager store = player.occupiedNode.GetComponent<StoreManager>();
        foreach (ItemStats item in recentStockedItems)
        {
            store.AddItem(item);
        }

        // Visual Item Stock Indicator Update
        MapNode node = player.occupiedNode.GetComponent<MapNode>();

        UpdateStorefrontVisual(node);
        /*
        for (int i = 0; i < 3; i++)
        {
            if(i < recentStockedItems.Count)
            {
                node.stockItems[i].gameObject.SetActive(true);
                node.stockItems[i].sprite = recentStockedItems[i].itemSprite;
            }
            else
            {
                node.stockItems[i].gameObject.SetActive(false);
            }
        }
        */

        isStockingStore = false;

        recentStockedItems.Clear();
        m_ExitInventory.RaiseEvent();
    }

    public void UpdateStorefrontVisual(MapNode node)
    {
        var storeInventory = node.GetComponent<StoreManager>().storeInventory;
        int emptyStockCheck = 0;

        node.soldoutText.text = "";

        for (int i = 0; i < 3; i++)
        {
            if (storeInventory[i] == null)
            {
                Debug.Log($"[]Store has blank spot");
                node.stockItems[i].gameObject.SetActive(false);
                emptyStockCheck++;
            }
            else
            {
                Debug.Log($"()Store has a {storeInventory[i]}");
                node.stockItems[i].gameObject.SetActive(true);
                node.stockItems[i].sprite = storeInventory[i].itemSprite;
                //node.stockItems[i].sprite = recentStockedItems[i].itemSprite;
            }
        }

        if (emptyStockCheck >= 3)
        {
            node.soldoutText.text = "SOLD OUT";
        }
    }

    private void OnItemDiscarded(int index, ItemStats item)
    {
        currentPlayer.inventory.RemoveAt(index);
        m_RefreshInventory.RaiseEvent(currentPlayer);
    }

    public void StealFromPlayer(EntityPiece otherPlayer)
    {
        Debug.Log("StealFromPlayer");
        int indexToSteal = Random.Range(0, otherPlayer.inventory.Count);
        currentPlayer.inventory.Add(otherPlayer.inventory[indexToSteal]);

        if (currentPlayer.inventory.Count > currentPlayer.inventoryLimit)
        {
            // Raise event to drop items.
        }

        otherPlayer.inventory.RemoveAt(indexToSteal);

        // Raise event to show UI of item stolen. Not sure what to do if other player has no items to steal.
    }

    public IEnumerator InitiateCombatOnEnemy(float delay, EntityPiece p)
    {
        //Raise event for moving to combat scene
        m_EnteredCombatScene.RaiseEvent();

        encounterStarted = true;

        // Set IDs of players entering combat.
        sceneManager.player1ID = p.id;

        bool lookingForTarget = true;
        while (lookingForTarget)
        {
            var monsterType = Random.Range(-8, 0); // int from -6 to -1

            sceneManager.player2ID = monsterType;

            var enemy = sceneManager.entities.Find(entity => sceneManager.player2ID == entity.id);
            if (enemy.combatSceneIndex == -1)
            {
                float spawnChance = Random.Range(0f, 1f);

                if (spawnChance < enemy.spawnRarityModifier)
                {
                    lookingForTarget = false;
                }
            }

        }

        yield return new WaitForSeconds(delay);

        phase = GamePhase.CombatTime;
        sceneManager.LoadCombatScene();

        yield return null;
    }

    public void InitiateCombatOnPlayer(EntityPiece otherPlayer)
    {
        Debug.Log("InitiateCombatOnPlayer");
        encounterStarted = true;

        // Set IDs of players entering combat.
        sceneManager.player1ID = currentPlayer.id;
        sceneManager.player2ID = otherPlayer.id;
        sceneManager.LoadCombatScene();
    }

    public void StopOnStore()
    {
        Debug.Log("StopOnStore");
        currentPlayer.movementLeft = 0;

        string roll = "" + currentPlayer.movementLeft;
        rollTypewriter.ShowText(roll);
    }

    public void SelectRaycastTarget(EntityPiece p)
    {
        /*
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (p.currentStatsModifier.warpMode == EntityStatsModifiers.WarpMode.Tiles)
            {
                if (RaycastTiles.tileSelected != null)
                    WarpConfirmed(p);
            }
            else if (p.currentStatsModifier.warpMode == EntityStatsModifiers.WarpMode.Players)
            {
                if (RaycastTiles.tileSelected.playerOccupied != null
                && RaycastTiles.tileSelected.playerOccupied != p)
                    WarpConfirmed(p);
            }
            else if (p.currentStatsModifier.warpMode == EntityStatsModifiers.WarpMode.Marigold)
            {
                if (RaycastTiles.tileSelected.CompareTag("Store") && RaycastTiles.tileSelected.modifier == MapNode.Modifier.None)
                {
                    PlantConfirmed(p, MapNode.Modifier.Marigold);
                    RaycastTiles.tileSelected.flowerTrapVisual.color = currentPlayer.playerColor;
                }
                else 
                {
                    m_ExitRaycastTargetSelection.RaiseEvent(); // prevents getting stuck, but we should probably add a warning
                    phase = GamePhase.InitialTurnMenu;
                }
            }
            else if (p.currentStatsModifier.warpMode == EntityStatsModifiers.WarpMode.Rafflesia)
            {
                if (RaycastTiles.tileSelected.modifier == MapNode.Modifier.None)
                {
                    RaycastTiles.tileSelected.flowerTrapVisual.color = currentPlayer.playerColor;
                    PlantConfirmed(p, MapNode.Modifier.Rafflesia);
                }
            }
        }
        */
    }

    public void OnSelectRaycastTarget()
    {
        if (currentPlayer.currentStatsModifier.warpMode == EntityStatsModifiers.WarpMode.Tiles)
        {
            if (RaycastTiles.tileSelected != null)
                WarpConfirmed(currentPlayer);
        }
        else if (currentPlayer.currentStatsModifier.warpMode == EntityStatsModifiers.WarpMode.Players)
        {
            if (RaycastTiles.tileSelected.playerOccupied != null
            && RaycastTiles.tileSelected.playerOccupied != currentPlayer)
                WarpConfirmed(currentPlayer);
        }
        else if (currentPlayer.currentStatsModifier.warpMode == EntityStatsModifiers.WarpMode.Marigold)
        {
            if (RaycastTiles.tileSelected.CompareTag("Store") && RaycastTiles.tileSelected.modifier == MapNode.Modifier.None)
            {
                RaycastTiles.tileSelected.flowerTrapVisual.color = currentPlayer.playerColor;
                RaycastTiles.tileSelected.flowerTrapVisual.sprite = RaycastTiles.tileSelected.flowerTrapSprites[0];
                PlantConfirmed(currentPlayer, MapNode.Modifier.Marigold);
            }
            else
            {
                m_ExitRaycastTargetSelection.RaiseEvent(); // prevents getting stuck, but we should probably add a warning
                phase = GamePhase.InitialTurnMenu;
            }
        }
        else if (currentPlayer.currentStatsModifier.warpMode == EntityStatsModifiers.WarpMode.Rafflesia)
        {
            if (RaycastTiles.tileSelected.modifier == MapNode.Modifier.None)
            {
                RaycastTiles.tileSelected.flowerTrapVisual.color = currentPlayer.playerColor;
                RaycastTiles.tileSelected.flowerTrapVisual.sprite = RaycastTiles.tileSelected.flowerTrapSprites[1];
                PlantConfirmed(currentPlayer, MapNode.Modifier.Rafflesia);
            }
        }
    }

    private void WarpConfirmed(EntityPiece p)
    {
        // FOR NAM: USE THIS EVENT TO HIDE SELECT TILE/PLAYER UI
        m_ExitRaycastTargetSelection.RaiseEvent();
        p.currentStatsModifier.warpDestination = RaycastTiles.tileSelected;
        ApplyItemEffectsOnTargetSelection(p);

        m_DisableFreeview.RaiseEvent();
        phase = GamePhase.InitialTurnMenu;
    }

    private void PlantConfirmed(EntityPiece p, MapNode.Modifier modifier)
    {
        // FOR NAM: USE THIS EVENT TO HIDE SELECT TILE/PLAYER UI
        m_ExitRaycastTargetSelection.RaiseEvent();
        p.currentStatsModifier.warpDestination = RaycastTiles.tileSelected;
        PlantItemOnSpaceSelection(p, modifier);

        m_DisableFreeview.RaiseEvent();
        phase = GamePhase.InitialTurnMenu;
    }

    private void PlantItemOnSpaceSelection(EntityPiece p, MapNode.Modifier modifier)
    {
        // Warp player to specified destination.
        if (p.currentStatsModifier.warpDestination != null)
        {
            p.currentStatsModifier.warpDestination.modifier = modifier;
            p.currentStatsModifier.warpDestination.modifierOwner = p;
        }
    }

    IEnumerator StartTransitionIntoCombat(float delay, EntityPiece entity)
    {
        // This is like kinda only for player v player
        //m_TransitionIntoCombat.RaiseEvent(entity);
        m_EnteredCombatScene.RaiseEvent();
        yield return new WaitForSeconds(delay);

        //m_EnteredCombatScene.RaiseEvent();
        m_InitiateCombatOnPassBy.RaiseEvent(entity);

        yield return null;
    }

    private void OnRecieveVendorItem(int salePrice, ItemStats gainedItem)
    {
        currentPlayer.inventory.Add(gainedItem);
        currentPlayer.heldPoints -= salePrice;
        m_UpdatePlayerScore.RaiseEvent(currentPlayer.id);
        m_PlayerScoreDecreased.RaiseEvent(salePrice); //this event is so fucking stupid
    }

    private void OnDamageAffectedNodes(List<MapNode> affectedNodes, float damageRatio)
    {
        foreach(EntityPiece player in playerUnits)
        {
            // this dude is chilling, he aint getting hit
            if (player.currentStates.Contains(EntityPiece.State.InsideVendor)) 
            {
                Debug.Log($"{player.entityName} is in a vendor, no damage taken.");
            }
            else if (affectedNodes.Contains(player.occupiedNode))
            {
                Debug.Log($"{player.entityName} in the streets. GOOBYE!!");

                player.health -= (int)(player.maxHealth * damageRatio);
                if(player.health < 0)
                {
                    player.health = 1;
                }

                m_UpdatePlayerScore.RaiseEvent(player.id);
            }
        }
    }

    // Old Level up stuff
    /*
    private void OnEnterLevelUp(EntityPiece p)
    {
        pointsLeft = 5;
        p.unspentLevelUpPoints += 5;
        p.maxHealth += 10;
        p.health += 10;
        p.RenownLevel += 1;

        UpdatePlayerDiceStats(p, diceStats);
        m_UpdatePlayerScore.RaiseEvent(p.id);

        levelUpScreen.enabled = true; // we need to put the UI stuff in its own script
        remainingSP.text = $"Remaining SP: {p.unspentLevelUpPoints}";

    }

    private void OnExitLevelUp()
    {
        // make this shit cooler
        levelUpScreen.enabled = false;
    }

    // I ripped this from another script, delete this later
    public void UpdatePlayerDiceStats(EntityPiece entity, GameObject diceStats)
    {
        // Visually updates the dice stats ui based on the entity and side
        playerDiceNumbers.Clear();

        // Goes through the diceStats UI List and finds the text components
        foreach (Transform child in diceStats.transform)
        {
            playerDiceNumbers.Add(child.GetComponentInChildren<DiceStatSelectionHandler>());

            //EventSystem.current.SetSelectedGameObject(child.gameObject);
        }

        EventSystem.current.SetSelectedGameObject(playerDiceNumbers[0].gameObject);

        UpdatePlayerDiceStatsInLevelUp(entity);
    }

    public void UpdatePlayerDiceStatsInLevelUp(EntityPiece entity)
    {
        // like the other function but it doesn't reset the button position
        var faceIndex = 0;

        for (int i = 0; i < 6; i++)
        {
            playerDiceNumbers[i].SetDieFaceValue(entity.strDie[faceIndex]);
            faceIndex++;

        }

        faceIndex = 0;

        for (int i = 6; i < 12; i++)
        {
            playerDiceNumbers[i].SetDieFaceValue(entity.dexDie[faceIndex]);
            faceIndex++;
        }

        faceIndex = 0;

        for (int i = 12; i < 18; i++)
        {
            playerDiceNumbers[i].SetDieFaceValue(entity.intDie[faceIndex]);
            faceIndex++;
        }
    }

    private void OnTryAugmentDieFaceValue(Action.WeaponTypes diceType, int diceIndex)
    {
        Debug.Log($"checking if can upgradfe | {diceType} Dice at id{diceIndex} is []");
       // Debug.Log($"cpsts {costArray[diceIndex]} SP, player has {currentPlayer.unspentLevelUpPoints}");
        // Check if current player has enough SP to augment this die face

        var selectedDie = currentPlayer.strDie[diceIndex];

        switch (diceType)
        {
            case Action.WeaponTypes.Melee:
                selectedDie = currentPlayer.strDie[diceIndex];
                break;

            case Action.WeaponTypes.Gun:
                selectedDie = currentPlayer.dexDie[diceIndex];
                break;

            case Action.WeaponTypes.Magic:
                selectedDie = currentPlayer.intDie[diceIndex];
                break;
        }

        Debug.Log($"Selected Die [{selectedDie}]");

        if (costArray[selectedDie] <= currentPlayer.unspentLevelUpPoints)
        {
            Debug.Log("it can!!!");
            currentPlayer.unspentLevelUpPoints -= costArray[selectedDie];

            switch (diceType)
            {
                case Action.WeaponTypes.Melee:
                    currentPlayer.strDie[diceIndex]++;
                    break;

                case Action.WeaponTypes.Gun:
                    currentPlayer.dexDie[diceIndex]++;
                    break;

                case Action.WeaponTypes.Magic:
                    currentPlayer.intDie[diceIndex]++;
                    break;
            }

            // broadcast that it did in fact upgrade
            m_AugmentedDieFaceValue.RaiseEvent();
            UpdatePlayerDiceStatsInLevelUp(currentPlayer);
            remainingSP.text = $"Remaining SP: {currentPlayer.unspentLevelUpPoints}";
            if(currentPlayer.unspentLevelUpPoints <= 0)
            {
                m_ExitLevelUp.RaiseEvent();
            }
        }
        else
        {
            // Can't augment, fail L bozo
            Debug.Log("failed to augment wtf how");
            m_FailAugmentDieFaceValue.RaiseEvent();
        }
    }
    */
}
