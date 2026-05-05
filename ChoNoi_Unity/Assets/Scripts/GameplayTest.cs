using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Linq;
using Febucci.UI.Core;
using UnityEngine.UI;
using System.Collections; 

public class GameplayTest : MonoBehaviour
{
    [SerializeField] private int seed = -1;

    [Space]
    public GameBoard board;
    public GamePhase phase = GamePhase.RollDice;
    public GamePhase expectedPhase = GamePhase.EndTurn;
    public EntityPiece winner;

    [Header("Debugging")]
    [SerializeField] private TextMeshProUGUI debugPhaseText;
    [SerializeField] private bool turnOffMonsterEncounters = false;

    public static GameplayTest instance;
    // Tile Data and Shit IGNORE THIS SECTION FOR NOW

    [SerializeField]
    public List<EntityPiece> playerUnits = new List<EntityPiece>();
    [SerializeField]
    private List<EntityPiece> nextPlayers = new List<EntityPiece>();

    [Header("Current Turn Info")]
    public EntityPiece currentPlayer;
    public MapNode currentPlayerInitialNode;
    [SerializeField] private List<EntityPiece> playersOnCurrentNode = new List<EntityPiece>();
    private MapNode currentRestockNode;

    //public Dictionary<Vector2Int, GameObject> map = new Dictionary<Vector2Int, GameObject>();
    //public Dictionary<Vector2Int, GameObject> unitPos = new Dictionary<Vector2Int, GameObject>();

    public enum GameBoard
    {
        CentralMarket,
        TrainStreet,
        CoconutCanal,
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

        BuildingStore,
        InStore, // new for input system
        PreStockStore,
        StockStore,
        OverturnStore,

        InVendor,
        RockPaperScissors,
        LevelUp,
        CombatSelector,
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
        CoconutTree,
        AirRaid,
        None,
    }

    [Header("Game Match Info")]
    public GameplayRules currentRuleset;
    //public int targetGoal = 4000;
    //public int storeLimit = 4;
    //public int playerCount = 4;

    public int turnRound = 1; // Round based on every player has had a turn
    private int playersActed = 0; // goes up every time a unique players turn is done

    public List<SpecialIncidents> matchIncidents; // list of periodic incidents for this specific board
    private List<SpecialIncidents> incidentsToActivate = new List<SpecialIncidents>(); // list of periodic incidents for this specific board
    public bool incidentIsPlaying = false;

    public int diceRoll;
    [Header("UI Additional Variables")]
    public List<ItemStats> recentStockedItems;
    public int emptyStockCount = 3;

    [SerializeField] private TypewriterCore rollTypewriter;
    public TextMeshProUGUI topLeftText;
    public TextMeshProUGUI turnText;

    public GameObject encounterScreen;
    public TextMeshProUGUI p1fight;
    public TextMeshProUGUI p2fight;
    public TextMeshProUGUI resultInfo;
    //public bool encounterOver = false;

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
    private float oldRep = 0;

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
    //public TextMeshProUGUI storestockTooltip;
    public int[] costArray = { 0, 1, 1, 2, 2, 2, 3, 3, 3, 4, 5, 999 };

    public Canvas howToPlayScreen;


    // Event Channels
    [Header("Special Event Channels")]
    public VoidEventChannelSO m_IncidentStarted; // broadcasting
    public VoidEventChannelSO m_ActivateIncidentTrain; // broadcasting
    public NodeListFloatEventChannelSO m_DamageAffectedNodes; // listening

    [Header("Broadcast on Event Channels")]
    public VoidEventChannelSO m_GameStart;
    public PlayerEventChannelSO m_PlayerWon;
    public PlayerListEventChannelSO m_ResultFinalScores;

    public IntEventChannelSO m_NextTurnRound;

    public PlayerEventChannelSO m_EnterLevelUp;

    // Start of Game Event Channels
    public PlayerEventChannelSO m_AssignPlayerToController;

    [Header("BC - Generic Events")]
    public VoidEventChannelSO m_PlayerMovedOnBoard;
    public VoidEventChannelSO m_PlayerUndidSomething;

    public IntEventChannelSO m_UpdatePlayerScore;
    public IntEventChannelSO m_PlayerScoreDecreased;
    public IntEventChannelSO m_PlayerScoreIncreased;

    public EntityIntEventChannelSO m_DamageTakenOnPlayer;

    [Header("BC - ITM Events")]
    public VoidEventChannelSO m_EnableFreeview;
    //public PlayerEventChannelSO m_DiceRollUndo;
    public PlayerEventChannelSO m_DiceRollPrep;
    public IntEventChannelSO m_RollForMovement;

    [Header("BC - Inventory Events")]
    public PlayerEventChannelSO m_OpenInventory; // JASPER OR RUSSELL PLEASE USE THIS EVENT TO ACCESS THE INVENTORY
    public PlayerEventChannelSO m_RefreshInventory;
    public PlayerEventChannelSO m_FullInventory;

    //public NodeEventChannelSO m_RestockStore;

    public VoidEventChannelSO m_ExitInventory;
    public NodeEventChannelSO m_WarpByItem;

    [Header("BC - Board Moving Events")]
    public PlayerEventChannelSO m_PassByStamp;
    public StampEventChannelSO m_UndoPassByStamp;

    public VoidEventChannelSO m_PassByPawnShop;
    public PlayerEventChannelSO m_UndoPassByPawnShop;

    //public EntityItemListEventChannelSO m_DropItems; // FOR NAM

    public PlayerEventChannelSO m_OverturnOpportunity; // old, delete this

    [Header("BC - Board Tile Events")]
    public NodeEventChannelSO m_LandOnStorefront;
    public VoidEventChannelSO m_ExitStorefront;

    public NodeEventChannelSO m_LandOnVendor;
    public NodeEventChannelSO m_LandOnCoconutTree;
    public NodeEventChannelSO m_LandOnWaterCoconut;

    public PlayerListEventChannelSO m_LandOnMultipleEntities;
    public PlayerEventChannelSO m_AskRestockStore;

    // Pass-by Event Channels
    [Header("BC - Board Passby Events")]
    public PlayerEventChannelSO m_StealOnPassBy;
    public PlayerEventChannelSO m_InitiateCombatOnPassBy;
    public VoidEventChannelSO m_StopOnStoreOnPassBy;

    public VoidEventChannelSO m_EnterRaycastTargetSelection;
    public VoidEventChannelSO m_ExitRaycastTargetSelection;

    [Header("BC - Board End-Of-Move Events")]
    public PlayerEventChannelSO m_NextPlayerTurn;
    public PlayerEventChannelSO m_EncounterDecision;
    public VoidEventChannelSO m_EnteredCombatScene;

    //public PlayerEventChannelSO m_TransitionIntoCombat;

    [Header("Listen on Event Channels")]
    public PlayerEventChannelSO m_TryDiceRollPrep;
    public VoidEventChannelSO m_DiceRolled;

    [Header("LS - Item Usage Events")]
    public IntItemEventChannelSO m_TryUseItemAt;
    private int selectedItemIndex; // CHANGE THIS PART LATER

    public IntItemEventChannelSO m_ItemUsed; // after confirm use
    public VoidEventChannelSO m_FinishedUsedItem; // after UseItem timeline is done

    public IntItemEventChannelSO m_ItemStocked;
    public IntItemEventChannelSO m_ItemDiscarded;
    public ItemEventChannelSO m_ItemBought; //Listening to this one

    [Header("LS - ITM Events")]
    public PlayerEventChannelSO m_BuildStore; //Listening to this one
    public NodeEventChannelSO m_RestockStore;
    public PlayerEventChannelSO m_FinishStockingStore;

    public VoidEventChannelSO m_ExitRaycastedTile; //Listening to this one
    public VoidEventChannelSO m_DisableFreeview;

    [Header("LS - Warp Events")]
    public NodeEventChannelSO m_LandOnWarpNode;
    public NodeEventChannelSO m_WarpToNode;

    [Header("LS - etc Events")]
    public WeaponTypeIntEventChannel m_TryAugmentDieFaceValue; // lvl up

    public IntItemEventChannelSO m_RecieveVendorItem;

    public PlayerEventChannelSO m_FighterSelected;

    private void SetSeed(int seed)
    {
        if(seed == -1)
        {
            seed = System.DateTime.Now.Millisecond;
        }

        Random.InitState(seed);

        Debug.Log($"Random State = {seed} ");//| (Seed) = {Random.seed}");

        //SystemRandomUniTest(seed);
    }

    private void SystemRandomUniTest(int seed)
    {
        //int s = System.Random.Next(2,55);

        var ran = new System.Random(seed);

        Debug.Log($"** System.Random = {ran} | Seed = {seed} **");
        for (int i = 1; i <= 100; i++)
        {
            Debug.Log($"Number #{i} = {ran.Next(0, 6)}");
            //Debug.Log($"Number #{i} = {ran.Next(0, 121)}");
            //Debug.Log($"Number #{i} = {ran.Next(1, 100)}");
        }
        //ran.
        //System.Random.seed;
    }

    private void OnEnable()
    {
        m_NextTurnRound.OnEventRaised += OnNextTurnRound;

        m_TryDiceRollPrep.OnEventRaised += OnTryDiceRollPrep;
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

        m_LandOnWarpNode.OnEventRaised += OnLandOnWarpNode;
        m_WarpToNode.OnEventRaised += OnWarpToNode;

        m_StealOnPassBy.OnEventRaised += StealFromPlayer;
        m_InitiateCombatOnPassBy.OnEventRaised += InitiateCombatOnPlayer;
        m_StopOnStoreOnPassBy.OnEventRaised += StopOnStore;

        m_RecieveVendorItem.OnEventRaised += OnRecieveVendorItem;

        m_DamageAffectedNodes.OnEventRaised += OnDamageAffectedNodes;

        m_FighterSelected.OnEventRaised += OnFighterSelected;
        //m_EnterLevelUp.OnEventRaised += OnEnterLevelUp;
        //m_ExitLevelUp.OnEventRaised += OnExitLevelUp;
        //m_TryAugmentDieFaceValue.OnEventRaised += OnTryAugmentDieFaceValue;
    }

    private void OnDisable()
    {
        m_NextTurnRound.OnEventRaised -= OnNextTurnRound;

        m_TryDiceRollPrep.OnEventRaised -= OnTryDiceRollPrep;
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

        m_LandOnWarpNode.OnEventRaised -= OnLandOnWarpNode;
        m_WarpToNode.OnEventRaised -= OnWarpToNode;

        m_StealOnPassBy.OnEventRaised -= StealFromPlayer;
        m_InitiateCombatOnPassBy.OnEventRaised -= InitiateCombatOnPlayer;
        m_StopOnStoreOnPassBy.OnEventRaised -= StopOnStore;

        m_RecieveVendorItem.OnEventRaised -= OnRecieveVendorItem;

        m_DamageAffectedNodes.OnEventRaised -= OnDamageAffectedNodes;

        m_FighterSelected.OnEventRaised -= OnFighterSelected;
        //m_EnterLevelUp.OnEventRaised -= OnEnterLevelUp;
        //m_ExitLevelUp.OnEventRaised -= OnExitLevelUp;
        //m_TryAugmentDieFaceValue.OnEventRaised -= OnTryAugmentDieFaceValue;
    }

    // Start is called before the first frame update
    void Awake()
    {
        SetSeed(seed);

        debugPhaseText.text = "";

        instance = this;
        sceneManager = GameObject.FindGameObjectWithTag("SceneManager").GetComponent<SceneGameManager>();
        //playerUnits.AddRange(FindObjectsOfType<EntityPiece>());
        //nextPlayers = playerUnits;
        encounterScreen.SetActive(false);
        if(PlayerConfigurationManager.instance != null)
        {
            Debug.Log("[ :) ] Started game from PrepScene (Correct)");
            currentRuleset = PlayerConfigurationManager.instance.ruleset;

            //playerUnits.Count = 2;
        }
        else
        {
            Debug.Log("[ :( ] Started game from UnityEditor (Wrong in the real game)");
            //currentRuleset.numberOfPlayers = playerUnits.Count;
        }

        Debug.Log($"Number of Players in this Game: {currentRuleset.numberOfPlayers}");
        topLeftText.text = $"Collect <color=yellow><sprite=\"Coin Icon\" index=0>{currentRuleset.pointGoal}</color> and return\r\nto the EXCHANGE space to win!";

        for (int i = 4; i > currentRuleset.numberOfPlayers; i--)
        {
            Debug.Log($"playerUnits[^1] = {playerUnits[^1]}");
            // turn off the unused player slots
            playerUnits[^1].gameObject.SetActive(false);
            playerUnits[^1].enabled = false;

            playerUnits.Remove(playerUnits[^1]);
        }

        foreach (var player in playerUnits)
        {
            nextPlayers.Add(player);

            // Allow starting nodes to detect the player on them.
            var initialNode = player.occupiedNode;
            //initialNode.playerOccupied = player;
            initialNode.playersOccupied.Add(player);

            m_AssignPlayerToController.RaiseEvent(player);
        }

        // Get the player at the start of the list.
        currentPlayer = nextPlayers[0];
        //currentPlayer = nextPlayers[playerUnits.Count - 1];
        currentPlayerInitialNode = currentPlayer.occupiedNode;
        //turnText.text = currentPlayer.entityName + "'s Turn!";
        //turnText.color = currentPlayer.playerColor;

        //m_GameStart.RaiseEvent();
        //Debug.Log("m_GameStart raised!!");
    }

    private void Start()
    {
        m_GameStart.RaiseEvent();

        m_NextPlayerTurn.RaiseEvent(currentPlayer);
        currentPlayerInitialNode = currentPlayer.occupiedNode;
        oldStamps = new List<Stamp.StampType>(currentPlayer.stamps);

        foreach(EntityPiece player in playerUnits)
        {
            if (player.occupiedNode != null)
            {
                player.transform.position = player.occupiedNode.transform.position;
                player.occupiedNodeCopy = player.occupiedNode;
                player.traveledNodes.Add(player.occupiedNode);
            }

            if (currentPlayer)
            {
                currentPlayer.transform.position -= new Vector3(0, 0, .05f);
            }
        }
        //m_NextTurnRound.RaiseEvent(turnRound);
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        debugPhaseText.text = "" + phase;

        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            if (turnOffMonsterEncounters) turnOffMonsterEncounters = false;
            else turnOffMonsterEncounters = true;

            Debug.Log($"Toggling | Monster Enounters off is {turnOffMonsterEncounters}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentPlayer.movementLeft = 1;
            rollTypewriter.ShowText(""+currentPlayer.movementLeft);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentPlayer.movementLeft = 2;
            rollTypewriter.ShowText("" + currentPlayer.movementLeft);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentPlayer.movementLeft = 3;
            rollTypewriter.ShowText("" + currentPlayer.movementLeft);
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            currentPlayer.movementLeft = 9;
            rollTypewriter.ShowText("" + currentPlayer.movementLeft);
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            currentPlayer.movementLeft = 100;
            rollTypewriter.ShowText("" + currentPlayer.movementLeft);
        }
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
                //RollDice(currentPlayer);
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

    //ORIGINALLY: void RollDice(EntityPiece p)
    private void OnTryDiceRollPrep(EntityPiece p)
    {
        // For now level up happens right before you roll dice
        if (p.canLevelUp() && !ThisPlayerMustFight(p)) {

            m_EnterLevelUp.RaiseEvent(p);
        }
        else if (!ThisPlayerMustFight(p))
        {
            // Regular behavior, roll for movement
            m_DiceRollPrep.RaiseEvent(p);
        }
        else
        {

            //TEMPORARY, REMOVE THIS LATER
            //m_DiceRollUndo.RaiseEvent(p);

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

    #region Node-based Functions
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
                /*
                p.stamps = new List<Stamp.StampType>(oldStamps);
                p.heldPoints = oldPoints;
                p.ReputationPoints = oldRep;
                p.health = currentPlayerInitialHealth; // revert healing back

                //oldStamps.Remove(stampCollected.stampType);
                //oldPoints = 0;

                m_UpdatePlayerScore.RaiseEvent(currentPlayer.id);
                m_UndoPassByPawnShop.RaiseEvent(p);
                */
                p.occupiedNode.UndoPassByThisNode(p);
            }
            else if (p.occupiedNode.CompareTag("Stamp"))
            {
                p.occupiedNode.UndoPassByThisNode(p);
                // Stamp was not collected before
                /*
                if (p.stamps.Contains(stampCollected.stampType) && !oldStamps.Contains(stampCollected.stampType))
                {
                    Debug.Log("undo stamp");
                    Debug.Log($"Old Stamp Contains {stampCollected.stampType} is {oldStamps.Contains(stampCollected.stampType)}");

                    p.stamps.Remove(stampCollected.stampType);
                    m_UndoPassByStamp.RaiseEvent(stampCollected.stampType); // shit code fix later
                }
                */
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

    private List<EntityPiece> GetOtherPlayersOnNode(EntityPiece p)
    {
        var otherPlayers = playersOnCurrentNode;

        if (otherPlayers.Contains(p))
        {
            otherPlayers.Remove(p); // get rid of the currentplayer from the list
        }

        otherPlayers = otherPlayers.Where(player => !(player.currentStates.Contains(EntityPiece.State.InsideVendor) 
                                                || player.currentStates.Contains(EntityPiece.State.Invulernable))).ToList();

        return otherPlayers;
    }

    void PassBy(EntityPiece p, MapNode m)
    {
        playersOnCurrentNode = m.playersOccupied;
        var otherPlayers = GetOtherPlayersOnNode(p);

        // Cash in Stamps
        if (m.CompareTag("Castle"))
        {
            /*
            oldRep = p.ReputationPoints;
            oldPoints = p.heldPoints;
            oldStamps = new List<Stamp.StampType>(p.stamps);

            var repGained = 75 * Mathf.Pow(1.5f, p.stamps.Count - 1);
            var pointsGained = (int)(150 * Mathf.Pow(2, p.stamps.Count - 1));

            if (p.stamps.Count != 0)
            {
                p.ReputationPoints += repGained;
                p.heldPoints += pointsGained;
                //m_UpdatePlayerScore.RaiseEvent(currentPlayer.id);
                m_PlayerScoreIncreased.RaiseEvent(pointsGained);
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

            if (p.heldPoints >= currentRuleset.pointGoal)
            {
                Debug.Log("BRO HE WON");
                winner = p;
                phase = GamePhase.EndGame; // Finish game if player w/ enough points passes by Pawn Shop
                return;
            }
            */
            m.PassByThisNode(p);
        }
        else if (m.CompareTag("Stamp"))
        {
            m.PassByThisNode(p);
        }

        //if (m.playerOccupied != null && m.playerOccupied != p)
        // There's other players on the space you pass
        if (CanFightPlayers(otherPlayers))
        {
            //EntityPiece otherPlayer = m.playerOccupied;
            Debug.Log("hello");
            // Check if can steal item from player.
            //if (p.currentStatsModifier.canStealOnPassBy && otherPlayer.inventory.Count > 0)
            if (p.currentStatsModifier.canStealOnPassBy)
            {
                foreach (EntityPiece otherPlayer in otherPlayers)
                {
                    Debug.Log("yoink Steal");
                    m_StealOnPassBy.RaiseEvent(otherPlayer);
                }

                // Deactivate all active effects of items that end on stealing.
                p.RemoveItemEffectOnUse(ItemLists.StealOnPassByItemNames); // ?? change this it shouldn't go away
            }

            // Check if can initiate combat.
            if (p.currentStatsModifier.canInitiateCombatOnPassBy)
            {
                Debug.Log("Combat");
                p.RemoveItemEffectOnUse(ItemLists.CombatOnPassByItemNames);

                InitiateCombat(p, m);
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

        if (p.movementLeft <= 0)
        {
            p.previousNode = p.traveledNodes[p.traveledNodes.Count - 1];
            p.traveledNodes.Clear();
            p.traveledNodes.Add(p.occupiedNode);

            if (turnOffMonsterEncounters)
                phase = GamePhase.EndTurn;
            else
                phase = GamePhase.EncounterTime; // next phase
        }
        else if (phase != GamePhase.EndGame)
            phase = GamePhase.PickDirection; // Go back to picking direction
        else
        {
            Debug.Log("?? Something went wrong in PassBy");
        }
    }

    void EncounterTime(EntityPiece p, MapNode m)
    {
        // Player has started combat
        if (ThisPlayerMustFight(p))
        {
            InitiateCombat(p, m);
        }
        // Player has not started combat
        else
        {
            //var otherPlayer = p.occupiedNode.playerOccupied;

            playersOnCurrentNode = m.playersOccupied;
            var otherPlayers = GetOtherPlayersOnNode(p);

            // Player Storefront
            if (m.TryGetComponent<StoreManager>(out StoreManager component))
            {
                // Update portions of this code later
                GameObject tile = m.gameObject;
                StoreManager store = component;

                if (CanFightPlayers(otherPlayers)) // Fight ppl on store
                {
                    InitiateCombat(p, m);
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
                    // Restock any store

                    Debug.Log($"Ask if {p.entityName} wants to restock");
                    m_AskRestockStore.RaiseEvent(p);
                }
            }
            else if (m.CompareTag("Castle"))
            {
                m.LandOnThisNode(p);
            }
            else if (m.CompareTag("Vendor"))
            {
                Debug.Log("On vendor");

                List<EntityPiece> exposedPlayersOnVendor = new List<EntityPiece>();

                foreach(EntityPiece playerOnVendor in otherPlayers)
                {
                    if (!playerOnVendor.currentStates.Contains(EntityPiece.State.InsideVendor))
                    {
                        exposedPlayersOnVendor.Add(playerOnVendor);
                    }
                }

                if (CanFightPlayers(exposedPlayersOnVendor))
                {
                    Debug.Log("person here");

                    Debug.Log("fight person on vendor here");
                    InitiateCombat(p, m);
                }
                else
                {
                    m_LandOnVendor.RaiseEvent(m);
                    phase = GamePhase.InVendor;
                }
            }
            else if (m.CompareTag("Stamp"))
            {
                // Clear your direction so you can choose next turn (TEMPORARY)
                m.LandOnThisNode(p);
            }
            else if (m.CompareTag("MoveAgain"))
            {
                oldStamps = new List<Stamp.StampType>(currentPlayer.stamps);
                oldPoints = 0;
                oldRep = 0;

                p.occupiedNode.playersOccupied.Add(p); // update to have that player in that node now
                p.occupiedNodeCopy = p.occupiedNode;
                p.traveledNodes.Clear();
                p.traveledNodes.Add(p.occupiedNode);

                m_DiceRollPrep.RaiseEvent(p);
            }
            else if (m.CompareTag("CoconutTree"))
            {
                // get bonked idiot
                m_LandOnCoconutTree.RaiseEvent(m);
                phase = GamePhase.IncidentHappening;
            }
            else if (m.CompareTag("WaterCoconut"))
            {
                // Dice "minigame' time
                m_LandOnWaterCoconut.RaiseEvent(m);
                phase = GamePhase.IncidentHappening;
            }
            else if (m.CompareTag("Encounter") && CanFightPlayers(otherPlayers)) // Player Fight
            {
                // If current player is not in combat scene and other player is not in combat scene, begin combat.
                // If current player is in combat scene, skip roll dice and load back here
                Debug.Log("PVP Fight");
                InitiateCombat(p, m);
            }
            else if (m.CompareTag("Encounter")) // Regular Encounter
            {
                Debug.Log("enemy Fight");
                StartCoroutine(InitiateCombatOnEnemy(.05f, p));

                phase = GamePhase.RockPaperScissors;
            }
            else // Generic MapNode Landing Function
            {
                Debug.Log("Generic landed on this node.");
                m.LandOnThisNode(p);
            }
        }
    }
    #endregion

    #region Combat-Based Functions
    private bool CanFightPlayers(List<EntityPiece> otherPlayers)
    {
        // otherPlayer != null && otherPlayer != currentPlayer
        return otherPlayers.Count != 0;
    }

    private bool ThisPlayerMustFight(EntityPiece entity)
    {
        return entity.currentStates.Contains(EntityPiece.State.Fighting) || entity.currentStates.Contains(EntityPiece.State.FightingParty);
    }

    private void InitiateCombat(EntityPiece p, MapNode m)
    {
        rollTypewriter.ShowText("");
        playersOnCurrentNode = m.playersOccupied;
        var otherPlayers = GetOtherPlayersOnNode(p);

        // Temporary behavior
        // Randomly pick a player on the passed tile to fight
        //EntityPiece selectedPlayer = otherPlayers[Random.Range(0, otherPlayers.Count)];

        if(otherPlayers.Count > 1)
        {
            //phase = GamePhase.CombatSelector;
            foreach(EntityPiece entity in m.playersOccupied)
            {
                // Indicate this is a third-party situation
                if(!entity.currentStates.Contains(EntityPiece.State.FightingParty))
                    entity.currentStates.Add(EntityPiece.State.FightingParty);
            }

            if (!p.currentStates.Contains(EntityPiece.State.FightingParty))
                p.currentStates.Add(EntityPiece.State.FightingParty);

            m_LandOnMultipleEntities.RaiseEvent(otherPlayers);
        }
        else
        {
            BeginCombat(otherPlayers.First());
        }

        /*
        if (selectedPlayer.combatSceneIndex == -1)
        {
            phase = GamePhase.CombatTime;
            encounterStarted = true;

            StartCoroutine(StartTransitionIntoCombat(.5f, selectedPlayer));
            //m_EnteredCombatScene.RaiseEvent();
            //m_InitiateCombatOnPassBy.RaiseEvent(otherPlayer);

            p.traveledNodes.Clear();
            p.traveledNodes.Add(p.occupiedNode);

            // Need NAM to disable input prompt (the number that shows up on top of the screen on roll).
            // If enter combat before it fades, it persists on next player's turn.
            return;
        }
        else
        {
            //temporary end turn
            phase = GamePhase.EndTurn;
        }
        */
    }

    private void BeginCombat(EntityPiece selectedEntity)
    {
        if(!currentPlayer.currentStates.Contains(EntityPiece.State.Fighting)) currentPlayer.currentStates.Add(EntityPiece.State.Fighting);
        if(!selectedEntity.currentStates.Contains(EntityPiece.State.Fighting)) selectedEntity.currentStates.Add(EntityPiece.State.Fighting);

        phase = GamePhase.CombatTime;
        encounterStarted = true;

        StartCoroutine(StartTransitionIntoCombat(.5f, selectedEntity));
        //m_EnteredCombatScene.RaiseEvent();
        //m_InitiateCombatOnPassBy.RaiseEvent(otherPlayer);

        currentPlayer.traveledNodes.Clear();
        currentPlayer.traveledNodes.Add(currentPlayer.occupiedNode);
    }

    private void OnFighterSelected(EntityPiece entity)
    {
        BeginCombat(entity);
    }
    #endregion

    #region Deprecated?
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
    #endregion

    public void ConfirmContinue()
    {
        phase = GamePhase.EndTurn;
        //encounterOver = false;
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

    #region End-of-Turn Functions
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
            //if (currentPlayerInitialNode.playerOccupied == currentPlayer)
            if (currentPlayerInitialNode.playersOccupied.Contains(currentPlayer))
            {
                currentPlayerInitialNode.playersOccupied.Remove(currentPlayer);
            }

            // Reset temp values.
            oldStamps.Clear();
            oldPoints = 0;
            oldRep = 0;

            p.occupiedNode.playersOccupied.Add(p); // update to have that player in that node now
            p.occupiedNodeCopy = p.occupiedNode;

            // Was in a third-party situation and is the last one standing!!
            if (p.occupiedNode.playersOccupied.Count == 1 &&
                p.currentStates.Contains(EntityPiece.State.FightingParty))
            {
                Debug.Log($"Wait {p.entityName} is the goat wtf?");
                p.currentStates.Remove(EntityPiece.State.FightingParty);
                p.dustCloud.SetActive(false);
            }

            isStockingStore = false; // let next player access inventory
            playerUsedItem = false; // let next player access inventory

            m_UpdatePlayerScore.RaiseEvent(currentPlayer.id);
            rollTypewriter.ShowText("");


            playersActed++;
            if (playersActed == currentRuleset.numberOfPlayers)
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
        // Put current player back to normal pos
        currentPlayer.transform.position += new Vector3(0, 0, .05f);
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

        if (currentPlayer.occupiedNode.playersOccupied.Count == 1 &&
                currentPlayer.currentStates.Contains(EntityPiece.State.FightingParty))
        {
            Debug.Log($"Wait {currentPlayer.entityName} is actually goated wtf?");
            currentPlayer.currentStates.Remove(EntityPiece.State.FightingParty);

            currentPlayer.dustCloud.gameObject.SetActive(false);
        }

        // put next player's sprite in front of the others
        currentPlayer.transform.position -= new Vector3(0,0,.05f);

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

    public void EndGame()
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
    #endregion

    private void ConfirmPurchase(ItemStats item)
    {
        // When an item is bought, allow confirmation via SPACE bar to continue the game
        if (item != null)
        {
            UpdateStorefrontVisual(currentPlayer.occupiedNode.GetComponent<MapNode>());
            //encounterOver = true;
            currentPlayer.heldPoints -= item.basePrice;
            currentPlayer.ReputationPoints += 20 + (item.basePrice / 10);

            if (currentPlayer.heldPoints < 0)
            {
                currentPlayer.currentStates.Add(EntityPiece.State.DeathsRow);
            }

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
                //encounterOver = true;
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

            currentPlayer.inventory.RemoveAt(index);
            playerUsedItem = true;

            ApplyItemEffectsOnTurnStart(currentPlayer);

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
            //WarpToMapNode(p, p.currentStatsModifier.warpDestination);
            expectedPhase = GamePhase.InitialTurnMenu;
            m_WarpByItem.RaiseEvent(p.currentStatsModifier.warpDestination);
        }
    }

    private void ApplyItemEffectsOnTargetSelection(EntityPiece p)
    {
        // Warp player to specified destination.
        if (p.currentStatsModifier.warpDestination != null)
        {
            //WarpToMapNode(p, p.currentStatsModifier.warpDestination);

            expectedPhase = GamePhase.InitialTurnMenu;
            m_WarpByItem.RaiseEvent(p.currentStatsModifier.warpDestination);
        }
    }

    public void WarpToMapNode(EntityPiece p, MapNode d)
    {
        p.occupiedNode = d;

        p.transform.position = p.occupiedNode.transform.position;
        p.occupiedNodeCopy = p.occupiedNode;
        p.traveledNodes.Clear();
        p.traveledNodes.Add(p.occupiedNode);

        //p.transform.position = p.occupiedNode.transform.position;
    }

    private void OnLandOnWarpNode(MapNode m)
    {
        /*
        // temp
        if(m is WarpNode) 
        {
            var w = m as WarpNode;
            WarpToMapNode(currentPlayer, w.GetDestinationNode());
            phase = expectedPhase;
        }
        */
        phase = GamePhase.IncidentHappening;
        expectedPhase = GamePhase.EndTurn;
    }

    private void OnWarpToNode(MapNode m)
    {
        WarpToMapNode(currentPlayer, m);
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
    #region Store Related Functions
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
        p.ownedStores.Add(store);

        //isStockingStore = true;

        StartCoroutine(DelayRestockStore(p, 1f));
        //m_RestockStore.RaiseEvent(p.occupiedNode);
    }

    private IEnumerator DelayRestockStore(EntityPiece p, float delay)
    {
        yield return new WaitForSeconds(delay);

        m_RestockStore.RaiseEvent(p.occupiedNode);
    }

    public void OnRestockStore(MapNode node)
    {
        isStockingStore = true;

        emptyStockCount = 0;
        var storeInventory = node.GetComponent<StoreManager>().storeInventory;

        currentRestockNode = node;
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
        StoreManager store = currentRestockNode.GetComponent<StoreManager>();
        foreach (ItemStats item in recentStockedItems)
        {
            store.AddItem(item);
        }

        // Visual Item Stock Indicator Update
        UpdateStorefrontVisual(currentRestockNode);

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
    #endregion Store Related Functions
    public void StealFromPlayer(EntityPiece otherPlayer)
    {

        int indexToSteal = Random.Range(0, otherPlayer.inventory.Count);
        currentPlayer.inventory.Add(otherPlayer.inventory[indexToSteal]);
        Debug.Log($"Stole '{otherPlayer.inventory[indexToSteal]}' from {otherPlayer.entityName}");
        // Only remove the item from a real player
        if (!otherPlayer.isEnemy)
            otherPlayer.inventory.RemoveAt(indexToSteal);

        // Raise event to show UI of item stolen. Not sure what to do if other player has no items to steal.
    }

    public IEnumerator InitiateCombatOnEnemy(float delay, EntityPiece p)
    {
        EntityPiece enemy = null;
        // Set IDs of players entering combat.
        sceneManager.player1ID = p.id;

        bool lookingForTarget = true;
        while (lookingForTarget)
        {
            var monsterType = Random.Range(-8, 0); // int from -6 to -1
            //monsterType = -9;
            sceneManager.player2ID = monsterType;

            enemy = sceneManager.entities.Find(entity => sceneManager.player2ID == entity.id);
            //if (enemy.combatSceneIndex == -1)
            if (!ThisPlayerMustFight(enemy))
            {
                float spawnChance = Random.Range(0f, 1f);

                if (spawnChance < enemy.spawnRarityModifier)
                {
                    lookingForTarget = false;
                }
            }
        }

        enemy.occupiedNode = p.occupiedNode;
        p.occupiedNode.playersOccupied.Add(enemy);

        yield return new WaitForSeconds(delay);

        BeginCombat(enemy);

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

        string roll = "";
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
            if (RaycastTiles.tileSelected.playersOccupied.Count != 0)
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
            /*
            else
            {
                m_ExitRaycastTargetSelection.RaiseEvent(); // prevents getting stuck, but we should probably add a warning
                phase = GamePhase.InitialTurnMenu;
            }
            */
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
                int damageTaken = (int)(player.maxHealth * damageRatio);
                player.health -= damageTaken;
                if(player.health < 0)
                {
                    player.health = 1;
                }

                m_DamageTakenOnPlayer.RaiseEvent(player, damageTaken);
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
