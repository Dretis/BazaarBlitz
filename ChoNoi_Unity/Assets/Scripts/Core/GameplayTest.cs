using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using Core.Events;
using FishNet.Demo.AdditiveScenes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// Basically the whole ass game
/// </summary>
public partial class GameplayTest : MonoBehaviour
{
    [SerializeField] private MapNode _respawnNode;
    [SerializeField] private int _respawnNodeId = 1;
    public BoardNodeManager BoardNodeManager;
    [SerializeField] private AvailableNodeIndicator _availableNodeIndicator;

    [Space]
    //
    [SerializeField] private int seed = -1;

    [Space]
    public GameBoard board;
    public GamePhase phase = GamePhase.RollDice;
    public GamePhase expectedPhase = GamePhase.EndTurn;
    public EntityPiece winner;

    [Header("Debugging")]
    [SerializeField] private bool _isPlayerConfigurationManagerMissing = false;
    [SerializeField] private List<PlayerColorPalettePreset> debugPresetPalettes;
    [SerializeField] private TextMeshProUGUI debugPhaseText;
    [SerializeField] private bool turnOffMonsterEncounters = false;
    [SerializeField] private int debugMoneyGain;
    [SerializeField] private int debugMoneyLoss;
    [SerializeField] private ItemStats debugItem1;
    [SerializeField] private ItemStats debugItem2;

    public static GameplayTest instance;

    [SerializeField]
    public List<EntityPiece> playerUnits = new List<EntityPiece>();
    [SerializeField]
    private List<EntityPiece> nextPlayers = new List<EntityPiece>();

    [Header("Current Turn Info")]
    public EntityPiece currentPlayer;
    private MapNode currentRestockNode;
    [Space]
    private int selectedItemIndex = -1; // -1 = currently not remembering an item in use | CHANGE THIS PART LATER
    //public bool playerUsedItem = false; // please change these down the line


    public enum GameBoard
    {
        CentralMarket,
        TrainStreet,
        CoconutCanal,
        RiceTerrace,
        Waterfalls
    }

    public enum GamePhase
    {
        GameStart,
        StartTurn,

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
        StockingStore,
        PrepStockStore,

        InVendor,
        RockPaperScissors,
        LevelUp, // change name to 'StatAllocation' ?
        BlessingTree,
        CombatSelector,
        CombatTime,

        ConfirmContinue,
        EndTurn,
        StartRound,
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

    //public int turnRound = 1; // Round based on every player has had a turn
    private int playersActed = 0; // goes up every time a unique players turn is done

    public List<SpecialIncidents> matchIncidents; // list of periodic incidents for this specific board
    private List<SpecialIncidents> incidentsToActivate = new List<SpecialIncidents>(); // list of periodic incidents for this specific board
    public bool incidentIsPlaying = false;

    //public int diceRoll;
    [Header("UI Additional Variables")]
    public List<ItemStats> recentStockedItems;
    public int emptyStockCount = 3;

    //[SerializeField] private TypewriterCore rollTypewriter;
    //public TextMeshProUGUI topLeftText;
    //public TextMeshProUGUI turnText;

    //public MapNode wantedNode;
    private SceneGameManager sceneManager;

    public bool encounterStarted = false;
    //public bool isStockingStore = false;

    // ui stuff for levelup;
    private int currentPlayerInitialHealth = 0; // for pawn shop healing
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
    //public VoidEventChannelSO m_StopOnStoreOnPassBy;

    public VoidEventChannelSO m_EnterRaycastTargetSelection;
    public VoidEventChannelSO m_ExitRaycastTargetSelection;

    [Header("BC - Board End-Of-Move Events")]
    public PlayerEventChannelSO m_NextPlayerTurn;
    public PlayerEventChannelSO m_EncounterDecision;
    public VoidEventChannelSO m_EnteredCombatScene;

    //public PlayerEventChannelSO m_TransitionIntoCombat;

    [Header("Listen on Event Channels")]
    public PlayerEventChannelSO m_TryDiceRollPrep;
    //public VoidEventChannelSO m_DiceRolled;
    public IntEventChannelSO m_ChangeToMoveDie;

    [Header("LS - Item Usage Events")]
    //public IntItemEventChannelSO m_TryUseItemAt;

    //public IntItemEventChannelSO m_ItemUsed; // after confirm use
    //public IntItemEventChannelSO m_RaycastItemUsed; // after confirm use
    //public VoidEventChannelSO m_FinishedUsedItem; // after UseItem timeline is done

    public IntItemEventChannelSO m_ItemStocked;
    public IntItemEventChannelSO m_ItemDiscarded;
    public ItemEventChannelSO m_ItemBought; //Listening to this one

    [Header("LS - ITM Events")]
    //public PlayerEventChannelSO m_BuildStore; //Listening to this one
    public NodeEventChannelSO m_RestockStore;
    public NodeEventChannelSO m_UpgradeStore;
    public NodeEventChannelSO m_TryRelocateStore;
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

    private void SetSeed()
    {
        if (seed == -1)
        {
            seed = DateTime.Now.Millisecond;
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
        //m_DiceRolled.OnEventRaised += CalculateDiceRoll;
        m_ItemBought.OnEventRaised += ConfirmPurchase;

        //m_TryUseItemAt.OnEventRaised += OnUseItemAt;

        //m_ItemUsed.OnEventRaised += OnItemUsed;
        //m_RaycastItemUsed.OnEventRaised += OnRaycastItemUsed;

        //m_FinishedUsedItem.OnEventRaised += OnFinishedUsedItem;

        m_UpdatePlayerScore.OnEventRaised += RemoveDeathsRow;
        //m_ExitRaycastedTile.OnEventRaised += OnDisableFreeview;

        //m_BuildStore.OnEventRaised += BuildStore;

        m_RestockStore.OnEventRaised += OnRestockStore;
        m_UpgradeStore.OnEventRaised += OnUpgradeStore;
        m_TryRelocateStore.OnEventRaised += OnTryRelocateStore;

        m_FinishStockingStore.OnEventRaised += AddRecentStockIntoStore;
        m_ItemStocked.OnEventRaised += TrackItemFromPlayerInventory;

        m_ItemDiscarded.OnEventRaised += OnItemDiscarded;

        m_DisableFreeview.OnEventRaised += OnDisableFreeview;

        m_LandOnWarpNode.OnEventRaised += OnLandOnWarpNode;
        m_WarpToNode.OnEventRaised += OnWarpToNode;

        m_StealOnPassBy.OnEventRaised += StealFromPlayer;
        m_InitiateCombatOnPassBy.OnEventRaised += InitiateCombatOnPlayer;
        //m_StopOnStoreOnPassBy.OnEventRaised += StopOnStore;

        m_RecieveVendorItem.OnEventRaised += OnRecieveVendorItem;

        m_DamageAffectedNodes.OnEventRaised += OnDamageAffectedNodes;

        m_FighterSelected.OnEventRaised += OnFighterSelected;
        //m_EnterLevelUp.OnEventRaised += OnEnterLevelUp;
        //m_ExitLevelUp.OnEventRaised += OnExitLevelUp;
        //m_TryAugmentDieFaceValue.OnEventRaised += OnTryAugmentDieFaceValue;
    }

    private void OnDisable()
    {
        //m_DiceRolled.OnEventRaised -= CalculateDiceRoll;
        m_ItemBought.OnEventRaised -= ConfirmPurchase;

        //m_TryUseItemAt.OnEventRaised -= OnUseItemAt;

        //m_ItemUsed.OnEventRaised -= OnItemUsed;
        //m_RaycastItemUsed.OnEventRaised -= OnRaycastItemUsed;

        //m_FinishedUsedItem.OnEventRaised -= OnFinishedUsedItem;

        m_UpdatePlayerScore.OnEventRaised -= RemoveDeathsRow;
        //m_ExitRaycastedTile.OnEventRaised -= OnDisableFreeview;

        //m_BuildStore.OnEventRaised -= BuildStore;
        m_RestockStore.OnEventRaised -= OnRestockStore;
        m_UpgradeStore.OnEventRaised -= OnUpgradeStore;
        m_TryRelocateStore.OnEventRaised -= OnTryRelocateStore;

        m_FinishStockingStore.OnEventRaised -= AddRecentStockIntoStore;
        m_ItemStocked.OnEventRaised -= TrackItemFromPlayerInventory;

        m_ItemDiscarded.OnEventRaised -= OnItemDiscarded;

        m_DisableFreeview.OnEventRaised -= OnDisableFreeview;

        m_LandOnWarpNode.OnEventRaised -= OnLandOnWarpNode;
        m_WarpToNode.OnEventRaised -= OnWarpToNode;

        m_StealOnPassBy.OnEventRaised -= StealFromPlayer;
        m_InitiateCombatOnPassBy.OnEventRaised -= InitiateCombatOnPlayer;
        //m_StopOnStoreOnPassBy.OnEventRaised -= StopOnStore;

        m_RecieveVendorItem.OnEventRaised -= OnRecieveVendorItem;

        m_DamageAffectedNodes.OnEventRaised -= OnDamageAffectedNodes;

        m_FighterSelected.OnEventRaised -= OnFighterSelected;
        //m_EnterLevelUp.OnEventRaised -= OnEnterLevelUp;
        //m_ExitLevelUp.OnEventRaised -= OnExitLevelUp;
        //m_TryAugmentDieFaceValue.OnEventRaised -= OnTryAugmentDieFaceValue;
    }

    // Start is called before the first frame update
    private void SetupGame()
    {
        SetSeed();

        debugPhaseText.text = "";

        instance = this;
        sceneManager = GameObject.FindGameObjectWithTag("SceneManager").GetComponent<SceneGameManager>();
        //playerUnits.AddRange(FindObjectsOfType<EntityPiece>());
        //nextPlayers = playerUnits;

        if (PlayerConfigurationManager.instance != null)
        {
            Debug.Log("[ :) ] Started game from PrepScene (Correct)");
            currentRuleset = PlayerConfigurationManager.instance.ruleset;

            //playerUnits.Count = 2;
        }
        else
        {
            Debug.Log("[ :( ] Started game from UnityEditor (Wrong in the real game)");
            _isPlayerConfigurationManagerMissing = true;
            //currentRuleset.numberOfPlayers = playerUnits.Count;
        }

        // Board Setup
        BoardNodeManager.Initialize();
        _availableNodeIndicator.Initialize(this);
        BoardNodeData[] boardNodes = new BoardNodeData[BoardNodeManager.NodeDictionary.Count];

        for (int i = 0; i < BoardNodeManager.NodeDictionary.Count; i++)
        {
            boardNodes[i] = BoardNodeManager.NodeDictionary[i].BoardNodeData;
        }
        _currentGameState.BoardNodes = boardNodes;

        Debug.Log($"Number of Players in this Game: {currentRuleset.numberOfPlayers}");
        //topLeftText.text = $"Collect <color=yellow><sprite=\"Coin Icon\" index=0>{currentRuleset.pointGoal}</color> and return\r\nto the EXCHANGE space to win!";

        _inputControllers = new PlayerInputController[currentRuleset.numberOfPlayers];

        //_currentGameState.Players = new List<PlayerState>(currentRuleset.numberOfPlayers);
        _currentGameState.Players = new PlayerState[currentRuleset.numberOfPlayers];
        //_currentGameState.NextPlayers = new List<PlayerState>();

        for (int i = 0; i < currentRuleset.numberOfPlayers; i++)
        {
            if (_isPlayerConfigurationManagerMissing)
            {
                _currentGameState.Players[i] = new PlayerState(i, debugPresetPalettes[i]);
                _currentGameState.Players[i].EntityData.OccupiedNodeId = _respawnNodeId;
                _currentGameState.Players[i].EntityData.InitialNodeId = _respawnNodeId;

                //_currentGameState.Players[i].EntityData.OccupiedNode = _respawnNode;
                //_currentGameState.Players[i].EntityData.InitialNode = _currentGameState.Players[i].EntityData.OccupiedNode;
                //_currentGameState.Players[i].EntityData.TraveledNodes.Add(_currentGameState.Players[i].EntityData.OccupiedNode);
            }
            else
            {
                // Grab the Player Cosmetic Manager's shit for this
                //_currentGameState.Players[i] = new PlayerState(i);
                var pcm = PlayerConfigurationManager.instance.GetPlayerCosmeticManager(i);

                _currentGameState.Players[i] = new PlayerState(i, pcm);
                _currentGameState.Players[i].EntityData.OccupiedNodeId = _respawnNodeId;
                _currentGameState.Players[i].EntityData.InitialNodeId = _respawnNodeId;

                //_currentGameState.Players[i].EntityData.OccupiedNode = _respawnNode;
                //_currentGameState.Players[i].EntityData.InitialNode = _currentGameState.Players[i].EntityData.OccupiedNode;
                //_currentGameState.Players[i].EntityData.TraveledNodes.Add(_currentGameState.Players[i].EntityData.OccupiedNode);
            }

            playerUnits[i].Initialize(_currentGameState.Players[i], this); // EntityPiece
            //_currentGameState.NextPlayers.Add(_currentGameState.Players[i]);

            //Debug.Log($"ps.EntityData.OccupiedNode = {_currentGameState.Players[i].EntityData.OccupiedNode}");
            //var initialNode = _currentGameState.Players[i].EntityData.OccupiedNode;
            //initialNode.occupiedEntities.Add(_currentGameState.Players[i].EntityData);

            var playerObject = Instantiate(_playerDevicePrefab);

            playerObject.name = $"Player Device [{i}]";
            //if (inputController.TryGetComponent<PlayerInputController>(out PlayerInputController pic))
            if (playerObject.TryGetComponent<PlayerDevice>(out PlayerDevice pd))
            {
                pd.Initialize(_currentGameState.Players[i], this);
                //pic.AssignPlayerToController(player, this);
                _inputControllers[i] = pd.playerInputController;

                //TEMP
                DontDestroyOnLoad(playerObject);
            }
        }

        for (int i = 4; i > currentRuleset.numberOfPlayers; i--)
        {
            // Change this Later
            Debug.Log($"playerUnits[^1] = {playerUnits[^1]}");
            // turn off the unused player slots
            playerUnits[^1].gameObject.SetActive(false);
            playerUnits[^1].enabled = false;

            playerUnits.Remove(playerUnits[^1]);
        }
        /*
        foreach (var player in playerUnits)
        {
            nextPlayers.Add(player);

            // Allow starting nodes to detect the player on them.
            var initialNode = player.occupiedNode;
            //initialNode.playerOccupied = player;
            initialNode.playersOccupied.Add(player);

            //m_AssignPlayerToController.RaiseEvent(player);

            var playerObject = Instantiate(_playerDevicePrefab);

            playerObject.name = $"Player Input Controller [{player.id}]";
            //if (inputController.TryGetComponent<PlayerInputController>(out PlayerInputController pic))
            if (playerObject.TryGetComponent<Player>(out Player pl))
            {
                #if UNITY_EDITOR
                if (_isPlayerConfigurationManagerMissing)
                {
                    var playerCosmeticManager = pic.GetComponent<PlayerCosmeticManager>();
                    playerCosmeticManager.PalettePreset = debugPresetPalettes[player.id];
                    playerCosmeticManager.ManualValidate();


                }
                #endif

                pl.Initialize();
                //pic.AssignPlayerToController(player, this);
                _inputControllers[player.id] = pl.playerInputController;

                //TEMP
                DontDestroyOnLoad(_inputControllers[player.id]);
            }
        }
        */

        currentPlayer = playerUnits[0]; // temp, remove
        //currentPlayerInitialNode = currentPlayer.occupiedNode; // remove
        _currentGameState.CurrentPlayerObj = playerUnits[0];
        _currentGameState.CurrentPlayerObj.transform.position -= new Vector3(0, 0, .05f);

        _currentGameState.CurrPlayerId = _currentGameState.Players[0].Id;
        _currentGameState.CurrentPlayer().EntityData.InitialNodeId = _currentGameState.CurrentPlayer().EntityData.OccupiedNodeId;

        _currentGameState.Round = 1;
        _currentGameState.TraveledNodeIds.Add(_currentGameState.CurrentPlayer().EntityData.OccupiedNodeId);

        _uiScoreManager.Initialize(ref _currentGameState.Players, this);

        _currentGameState.GamePhase = GamePhase.GameStart; // finished game prep, lets go
    }
    void Awake()
    {
        SetupGame();
    }

    private void Start()
    {
        m_GameStart.RaiseEvent();

        m_NextPlayerTurn.RaiseEvent(currentPlayer);

        foreach (var inputController in _inputControllers)
        {
            inputController.SetCurrentPlayer(_currentGameState.CurrPlayerId);
        }
        //currentPlayerInitialNode = currentPlayer.occupiedNode;

        //m_NextTurnRound.RaiseEvent(turnRound);
    }

    // Update is called once per frame
    void Update()
    {
        TickGame();
    }
    private void TestFunctions(GameState prevState, ref GameState newState)
    {
#if UNITY_EDITOR
        debugPhaseText.text = $"{phase}";
        debugPhaseText.text += $"\n{_currentGameState.GamePhase}";

        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            Debug.Log($"DEBUG | Gave {_currentGameState.CurrentPlayer().EntityData.EntityName} a {debugItem1.itemName}!");
            newState.CurrentPlayer().EntityData.Inventory.Add(debugItem1);
        }
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            Debug.Log($"DEBUG | Gave {_currentGameState.CurrentPlayer().EntityData.EntityName} a {debugItem2.itemName}!");
            newState.CurrentPlayer().EntityData.Inventory.Add(debugItem2);
        }
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            Debug.Log($"DEBUG | {newState.CurrentPlayer().EntityData.EntityName} has lost -{debugMoneyLoss} Ding!");
            newState.CurrentPlayer().EntityData.Money -= debugMoneyLoss;
        }
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            Debug.Log($"DEBUG | {newState.CurrentPlayer().EntityData.EntityName} was given +{debugMoneyGain} Ding!");
            newState.CurrentPlayer().EntityData.Money += debugMoneyGain;
            //newState.Players[newState.CurrPlayerId].EntityData.Money += 100;
        }
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            if (turnOffMonsterEncounters) turnOffMonsterEncounters = false;
            else turnOffMonsterEncounters = true;

            Debug.Log($"Toggling | Monster Enounters off is {turnOffMonsterEncounters}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            //currentPlayer.movementLeft = 1;
            //rollTypewriter.ShowText(""+currentPlayer.movementLeft);
            newState.MovementLeft = 1;
            //rollTypewriter.ShowText($"{_currentGameState.MovementLeft}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            newState.MovementLeft = 2;
            //rollTypewriter.ShowText($"{_currentGameState.MovementLeft}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            newState.MovementLeft = 3;
            //rollTypewriter.ShowText($"{_currentGameState.MovementLeft}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            newState.MovementLeft = 4;
            //rollTypewriter.ShowText($"{_currentGameState.MovementLeft}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            newState.MovementLeft = 5;
            //rollTypewriter.ShowText($"{_currentGameState.MovementLeft}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            newState.MovementLeft = 6;
            //rollTypewriter.ShowText($"{_currentGameState.MovementLeft}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            newState.MovementLeft = 9;
            //rollTypewriter.ShowText($"{_currentGameState.MovementLeft}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            newState.MovementLeft = 100;
            //rollTypewriter.ShowText($"{_currentGameState.MovementLeft}");
        }
#endif
    }

    #region Sample Partial Rewrite

    [Header("Rewrite")]
    [SerializeField]
    private GameStateEventChannelSO _gameStateUpdatedEventChannel;

    [SerializeField]
    private ScoreManager _uiScoreManager;
    [SerializeField]
    private GameObject _playerDevicePrefab;

    [SerializeField]
    private PlayerInputController[] _inputControllers;

    [SerializeField]
    private GameState _currentGameState = new()
    {
        GamePhase = GamePhase.StartTurn
    };

    private Queue<ICommand> _commandQueue = new();
    private Queue<ICutscene> _cutsceneQueue = new();
    private ICutscene _currentCutscene;

    public void QueueCommand(ICommand cmd)
    {
        _commandQueue.Enqueue(cmd);
    }
    public void QueueCutscene(ICutscene newCutscene)
    {
        _cutsceneQueue.Enqueue(newCutscene);
    }

    // TEMPORARY, DELETE LATER
    public PlayerInputController GetInputController(int id)
    {
        return _inputControllers[id];
    }

    private void TickGame()
    {
        if (_currentCutscene != null &&
            _currentCutscene.IsPlaying())
        {
            // skip doing core logic, state doesn't change
            // maybe have a cutscene phase, but then you'll need to keep the desired phase like you do already I think so you know what to return to
            Debug.Log("TickGame | Cutscene is playing...");
            return;
        }
        else
        {
            //Debug.Log("TickGame | Cutscene is over.");
            _currentCutscene = null;
        }

        // SYNC IN (connect rewrite system to the actual system so the rewrite demo works)
        //_currentGameState.GamePhase = phase;
        //_currentGameState.CurrentPlayer = currentPlayer;

        // Have the new state just copy the previous state, but it gets modified by the 
        GameState newState = _currentGameState;
        GameState prevState = newState.Copy();

        // Process one command per frame to keep things simple
        bool isCmdToProcess = _commandQueue.TryDequeue(out ICommand cmd);

        // CORE GAME LOGIC
        CoreGameLogic(prevState, ref newState, isCmdToProcess, cmd);
#if UNITY_EDITOR
        TestFunctions(prevState, ref newState);
#endif
        FrameGameState frameGameState = new FrameGameState()
        {
            NewState = newState,
            PrevState = prevState
        };

        // Input
        foreach (var inputController in _inputControllers)
        {
            inputController.SwitchActionMapRewrite(_currentGameState.GamePhase);
        }
        // Handle all the UI and presentation stuff here or in the below event
        //_uiPromptManager.UpdateUI(frameGameState);

        // Can also use events I guess
        // ANYTHING THAT LISTENS HERE SHOULD NEVERRRRRR CHANGE THE GAME STATE IN ANY WAY
        // GAME STATE IS DONNNNE AFTER CORE LOGIC RUNS
        // THE ORDER THAT THINGS EXECUTE FROM THIS EVENT SHOULDN'T MATTER
        // BECAUSE THE ORDER IS NON DETERMINISTIC IF YOU SUBSCRIBE IN AWAKE
        // SO RACE CONDITIONS ARE BADDDDDDDDDDDDDDDDDDDDDDD
        _gameStateUpdatedEventChannel?.RaiseEvent(frameGameState);

        //Debug.Log($"FrameState | Prev Phase: {frameGameState.PrevState.GamePhase} | New Phase: {frameGameState.NewState.GamePhase}");
        _currentGameState = newState;

        // SYNC OUT (connect rewritten system to the existing system so the rewrite demo works)
        //phase = _currentGameState.GamePhase;

        // Cutscene Time
        if (_currentCutscene == null && _cutsceneQueue.Count != 0)
        {
            Debug.Log("Playing cutscene");
            _currentCutscene = _cutsceneQueue.Dequeue();
            _currentCutscene.Play(prevState, newState);
        }
    }

    private void CoreGameLogic(GameState prevState, ref GameState newState, bool isCmdToProcess, ICommand cmd)
    {
        switch (newState.GamePhase)
        {
            case GamePhase.GameStart:
                // Should only be called once
                Debug.Log("CoreGameLogic | Game Start!!");
                newState.GamePhase = GamePhase.StartTurn;
                break;
            case GamePhase.StartTurn:
                // Just indicates the start of a player's turn
                Debug.Log("CoreGameLogic | StartTurn");
                //newState.CurrentPlayer().EntityData.TickDownActiveEffects();
                newState.GamePhase = GamePhase.ItemSelection;
                break;

            // Checks item effects on player
            case GamePhase.ItemSelection:
                //newState.LastGamePhase = GamePhase.InitialTurnMenu;
                newState.GamePhase = HandleItemSelection(prevState, ref newState);
                break;

            // Pick choices
            case GamePhase.InitialTurnMenu:
                // Can move this whole block into a separate function if you want to keep functions small (matter of opinion i guess)
                if (!isCmdToProcess)
                {
                    break;
                }

                if (cmd is ITMCommand itmCommand)
                {
                    switch (itmCommand.Action)
                    {
                        case ITMCommand.ITMAction.View:
                            HandleTryFreeview(prevState, ref newState);
                            break;
                        case ITMCommand.ITMAction.Roll:
                            HandleTryDiceRoll(prevState, ref newState);
                            break;
                        case ITMCommand.ITMAction.Inv:
                            HandleTryOpenInventory(prevState, ref newState);
                            break;
                        case ITMCommand.ITMAction.Build:
                            HandleTryBuildStore(prevState, ref newState);
                            break;
                    }
                }
                break;

            case GamePhase.Freeview:
                if (!isCmdToProcess)
                {
                    break;
                }

                // Insert FreeviewMoveCommand here

                if (cmd is FreeviewExitCommand)
                {
                    HandleTryFreeviewExit(prevState, ref newState);
                }
                break;

            case GamePhase.RaycastTargetSelection:
                if (!isCmdToProcess)
                {
                    break;
                }

                // Insert FreeviewMoveCommand here

                // Insert FreeviewExamineCommand here

                if (cmd is FreeviewExamineCommand)
                {
                    HandleTryFreeviewExamine(prevState, ref newState);
                }

                if (cmd is FreeviewExitCommand)
                {
                    HandleTryFreeviewExit(prevState, ref newState);
                }
                break;

            case GamePhase.Inventory:
                if (!isCmdToProcess)
                {
                    break;
                }

                if (cmd is UICancelCommand)
                {
                    // basically what m_ExitInventory was
                    HandleInventoryExit(prevState, ref newState);
                    //newState.GamePhase = newState.LastGamePhase;
                }

                if (cmd is InventoryUseItemCommand invUseItemCommand)
                {
                    var index = invUseItemCommand.Index;
                    var item = invUseItemCommand.Item;
                    // basically what m_ExitInventory was
                    HandleInventoryUseItem(prevState, ref newState, index, item);
                }
                // UI Submit/Confirm Command
                //if (cmd is InventoryCommand)
                //{
                //    HandleTryUseItem(prevState, ref newState);
                //}

                // UI Cancel Command

                //if (cmd.Type is GameCommandType.InventoryExit)
                //{
                //    HandleInventoryExit(prevState, ref newState);
                //}
                break;

            case GamePhase.BuildingStore:
                if (!isCmdToProcess)
                {
                    break;
                }

                if (cmd is UICancelCommand)
                {
                    // basically what m_CancelBuildStore was
                    //HandleBuildCancel(prevState, ref newState);
                    newState.GamePhase = newState.LastGamePhase;
                }

                if (cmd is BuildStoreCommand)
                {
                    HandleBuildStore(prevState, ref newState);
                }
                break;

            // Roll Phase 
            case GamePhase.RollDice:
                if (!isCmdToProcess)
                {
                    break;
                }

                if (cmd is ConfirmationYesCommand)
                {
                    HandleRollMoveDie(prevState, ref newState);
                }

                if (cmd is ConfirmationNoCommand)
                {
                    HandleCancelMoveDie(prevState, ref newState);
                }

                if (cmd is ConfirmationChangeCommand)
                {
                    HandleChangeMoveDie(prevState, ref newState);
                }
                break;

            // Pick Direction to Go Phase - Moving
            case GamePhase.PickDirection:
                if (!isCmdToProcess)
                {
                    break;
                }

                if (cmd is MovingCommand movingCommand)
                {
                    var movingDirection = movingCommand.Direction;

                    HandlePickDirection(prevState, ref newState, movingDirection);
                }

                if (cmd is ITMCommand)
                {
                    Debug.Log("ITMCmd | Toggle Freeview");
                    HandleTryFreeview(prevState, ref newState);
                }
                break;

            // Move-to Node Phase
            // Why do we need different phases for pass by and move around?
            case GamePhase.MoveAround:
                newState.GamePhase = HandleMoveAround(prevState, ref newState);
                break;

            case GamePhase.PassBy:
                newState.GamePhase = HandlePassBy(prevState, ref newState);
                break;

            // Battle-Event Phase
            case GamePhase.EncounterTime:
                HandleEncounterTime(prevState, ref newState);
                break;

            case GamePhase.InStore: // NEW
                if (!isCmdToProcess)
                {
                    break;
                }
                // UI Submit/Confirm Command required
                // HandleTryBuyItem(prevState, ref newState);
                break;

            case GamePhase.StockingStore:
                if (!isCmdToProcess)
                {
                    break;
                }
                //if(cmd is RestockStoreCommand restockStoreCommand)
                //{
                //    var itemsToStock = restockStoreCommand.ItemsToStock;

                //    HandleStockItemsToStore(prevState, ref newState, itemsToStock);
                //}

                // UI Cancel Command required
                if (cmd is UICancelCommand)
                {
                    // bring up the finish stocking "Looks good" prompt in UIInventoryManager
                    // outdated, update this
                    //ItemStats[] itemsToAdd = new ItemStats[3]; // chang ethis
                    //HandleFinishStockStore(prevState, ref newState, itemsToAdd);
                    newState.GamePhase = GamePhase.PrepStockStore;
                    //PromptRestockItems();
                }
                break;
            case GamePhase.PrepStockStore:
                if (cmd is RestockStoreCommand restockStoreCommand)
                {
                    var itemsToStock = restockStoreCommand.ItemsToStock;

                    HandleStockItemsToStore(prevState, ref newState, itemsToStock);
                }
                if (cmd is UICancelCommand)
                {
                    newState.GamePhase = GamePhase.StockingStore;
                }
                break;

            case GamePhase.RockPaperScissors:
                RockPaperScissors(currentPlayer); // useless
                break;

            case GamePhase.LevelUp:
                if (!isCmdToProcess)
                {
                    break;
                }
                // UI
                if (cmd is ConfirmationYesCommand)
                {
                    //HandleLevelUpExit
                }

                HandleLevelUp(prevState, ref newState);
                break;

            // Confirmation Phase
            case GamePhase.ConfirmContinue:
                // probably also doesn't work
                if (!isCmdToProcess)
                {
                    break;
                }

                if (cmd is ConfirmationYesCommand)
                {
                    //HandleConfirmYes(prevState, ref newState);
                }

                if (cmd is ConfirmationNoCommand)
                {
                    //HandleConfirmNo(prevState, ref newState);
                }
                //ConfirmContinue(currentPlayer);
                break;

            // End of turn, next player!
            case GamePhase.EndTurn:
                // probably also doesn't work
                newState.GamePhase = EndOfTurn(prevState, ref newState);
                break;
            case GamePhase.StartRound:
                // Just indicates the start of a player's turn
                newState.GamePhase = HandleNextTurnRound(prevState, ref newState);
                break;
            case GamePhase.IncidentHappening:
                HandlePostCutscene(prevState, ref newState);
                break;
            // Game over! Someone has won!
            case GamePhase.EndGame:
                // probably also doesn't work
                EndGame(prevState, ref newState);
                break;
        }
    }
    private void HandleTryFreeview(GameState prevState, ref GameState newState)
    {
        Debug.Log($"HandleTryFreeview | prevState.GamePhase = {prevState.GamePhase}");
        //Debug.Log($"HandleTryFreeview | newState.GamePhase = {newState.GamePhase}");
        newState.LastGamePhase = prevState.GamePhase;
        newState.GamePhase = GamePhase.Freeview;
    }

    private void HandleTryFreeviewExit(GameState prevState, ref GameState newState)
    {
        newState.GamePhase = newState.LastGamePhase;
    }

    private void HandleTryDiceRoll(GameState prevState, ref GameState newState)
    {
        //var p = newState.CurrentPlayer();
        var p = newState.CurrentPlayer();

        if (p.EntityData.CanLevelUp() && !ThisPlayerMustFight(p.EntityData)) {

            //expectedPhase = GamePhase.LevelUp;
            // not part of rewrite so go back to using the events
            //m_EnterLevelUp.RaiseEvent(p);
            // should play the Level Up cutscene
            newState.LastGamePhase = GamePhase.LevelUp;
            newState.GamePhase = GamePhase.IncidentHappening;
        }
        else if (!ThisPlayerMustFight(p.EntityData))
        {
            // no events just update the state
            // We can roll dice yay
            newState.LastGamePhase = GamePhase.InitialTurnMenu;
            newState.GamePhase = GamePhase.RollDice;
        }
        else
        {
            // In Combat
            newState.LastGamePhase = prevState.GamePhase;
            newState.GamePhase = GamePhase.EncounterTime;
        }
    }
    private void HandleTryOpenInventory(GameState prevState, ref GameState newState)
    {
        if (!newState.CurrentPlayerHasUsedItem)
        {
            var p = newState.CurrentPlayer();

            //playerInput.uiInputModule = FindObjectOfType<InputSystemUIInputModule>();
            //playerInput.uiInputModule.actionsAsset = playerInput.actions;
            newState.LastGamePhase = prevState.GamePhase;
            newState.GamePhase = GamePhase.Inventory;

            //SwitchActionMap(GamePhase.Inventory);
            //m_OpenInventory.RaiseEvent(p);
        }
        else
        {
            // play some nuh-uh sound? no thats not apart of logic
        }
    }

    private void HandleTryBuildStore(GameState prevState, ref GameState newState)
    {
        var p = newState.CurrentPlayer().EntityData;
        var n = newState.BoardNodes[p.OccupiedNodeId];

        if (n.Type is NodeType.OpenSpace
            && p.StoreCount <= currentRuleset.storeLimit
            && !p.Statuses.Contains(Status.Fighting))
        {
            newState.LastGamePhase = prevState.GamePhase;
            newState.GamePhase = GamePhase.BuildingStore;
            //playerInput.uiInputModule = FindObjectOfType<InputSystemUIInputModule>();
            //playerInput.uiInputModule.actionsAsset = playerInput.actions;

            //previousGamePhase = GamePhase.InitialTurnMenu;

            //m_TryBuildStore.RaiseEvent(p);
            //SwitchActionMap(GamePhase.BuildingStore);
            //Debug.Log("Built a store");
            // Build a store in the current tile
            //m_BuildStore.RaiseEvent(currentPlayer);
            //m_RestockStore.RaiseEvent(currentPlayer.occupiedNode);
            //SwitchActionMap(GamePhase.StockStore);

        }
        else
        {
            Debug.Log("Can't build store");
            // Give some notification/play some nuh-uh sound
        }
    }

    private void HandleRollMoveDie(GameState prevState, ref GameState newState)
    {
        newState.MoveDieRoll = newState.MovementLeft = CalculateMoveDieRoll(prevState, ref newState);

        newState.SnapshotCurrentPlayerData();
        newState.GamePhase = GamePhase.PickDirection;
    }

    private void HandleCancelMoveDie(GameState prevState, ref GameState newState)
    {
        var p = newState.CurrentPlayer();

        newState.GamePhase = newState.LastGamePhase;
    }

    private void HandleChangeMoveDie(GameState prevState, ref GameState newState)
    {
        var p = newState.CurrentPlayer();
        if (p.EntityData.CurrentStatsModifier.canUseSpeedDie && p.EntityData.Health > 25)
        {
            // swap to the other die not being shown
            // temp
            if (newState.UsingSpeedDieToMove)
            {
                newState.UsingSpeedDieToMove = false;
                m_ChangeToMoveDie.RaiseEvent(0); // have the SpinDie itself listen to _gameStateUpdatedEventChannel
                // Change to a struct variable?
            }
            else
            {
                newState.UsingSpeedDieToMove = true;
                m_ChangeToMoveDie.RaiseEvent(1); // have the SpinDie itself listen to _gameStateUpdatedEventChannel
            }
        }
    }

    private void HandleInventoryExit(GameState prevState, ref GameState newState)
    {
        newState.GamePhase = newState.LastGamePhase; // maybe to an ExpectedPhase instead?
    }

    private void HandleInventoryUseItem(GameState prevState, ref GameState newState, int index, ItemStats item)
    {
        Debug.Log($"HandleInventoryUseItem | Use {item} at index {index}");
        ref var p = ref newState.CurrentPlayer().EntityData;
        //newState.CurrentPlayer.inventory.RemoveAt(index);
        //newState.CurrentPlayer.AddItemToActiveEffects();
        if (item.type == ItemStats.ItemType.TargetSelect ||
             item.type == ItemStats.ItemType.Deployable)
        {
            Debug.Log("Used target select item");
            p.AddItemToActiveEffects(p.Inventory[index].Duration, p.Inventory[index]);
            p.RefreshStatModifiers();
            /*
            p.UpdateStatModifier(new ActiveEffect
            {
                OriginalItem = p.Inventory[index],
                TurnsRemaining = p.Inventory[index].Duration - 1
            });
            */
            ApplyItemEffectsOnTurnStart(ref p);

            //m_ExitInventory.RaiseEvent();
            // Raise free view event I guess?
            //m_EnableFreeview.RaiseEvent();
            // FOR NAM: USE THIS EVENT TO SHOW SELECT TILE/PLAYER UI.
            m_EnterRaycastTargetSelection.RaiseEvent();

            //phase = GamePhase.RaycastTargetSelection;
            //newState.GamePhase = GamePhase.RaycastTargetSelection;

            newState.UsedItem = item;
            newState.LastGamePhase = GamePhase.Inventory;
            newState.GamePhase = GamePhase.RaycastTargetSelection;
        }
        else
        {
            //newState.CurrentPlayer().EntityData.AddItemToActiveEffects(p.Inventory[index].Duration, p.Inventory[index]);
            //newState.CurrentPlayer().EntityData.RefreshStatModifiers();
            //ApplyItemEffectsOnTurnStart(ref newState.CurrentPlayer().EntityData);

            p.AddItemToActiveEffects(p.Inventory[index].Duration, p.Inventory[index]);
            p.RefreshStatModifiers();
            //p.ApplyItemEffectsOnTurnStart();
            ApplyItemEffectsOnTurnStart(ref p);
            //ApplyItemEffectsOnTurnStart(ref p);
            p.Inventory.RemoveAt(index);

            // Signals TimelineSignalResponder to play the Use Item cutscene i hope
            newState.UsedItem = item;
            newState.CurrentPlayerHasUsedItem = true;
            newState.LastGamePhase = GamePhase.InitialTurnMenu;
            newState.GamePhase = GamePhase.IncidentHappening; // play the cutscene
            //QueueCutscene(new UseItemCutscene());
        }
        // newState.GamePhase = GamePhase.IncidentHappening; // play the cutscene
    }
    private void HandleBuildStore(GameState prevState, ref GameState newState)
    {
        ref var p = ref newState.CurrentPlayer().EntityData;
        ref var n = ref newState.BoardNodes[p.OccupiedNodeId];
        p.StoreCount++;
        //p.OwnedStores.Add(p.OccupiedNode);
        n.Type = NodeType.Storefront;
        n.StorefrontData = new StorefrontData(newState.CurrentPlayer());

        newState.SelectedBoardNodeId = p.OccupiedNodeId;
        newState.LastGamePhase = GamePhase.StockingStore;
        newState.GamePhase = GamePhase.IncidentHappening; // play the cutscene
    }

    private void HandleStockItemsToStore(GameState prevState, ref GameState newState, Dictionary<int, ItemStats> itemsToStock)
    {
        ref var p = ref newState.CurrentPlayer().EntityData;
        ref var n = ref newState.BoardNodes[newState.SelectedBoardNodeId];
        //n.StorefrontData.Storestock = newItems;
        //for(int i = 0; i < itemsToStock.Count; i++)StorefrontData.STORESTOCK_CAP
        var incomingItemIndex = 0;
        for(int i = 0; i < StorefrontData.STORESTOCK_CAP; i++)
        {
            //var a = Array.FindIndex(n.StorefrontData.Storestock, null);
            if (n.StorefrontData.Storestock[i] == null && incomingItemIndex < itemsToStock.Count)
            {
                var a = itemsToStock.ElementAt(incomingItemIndex);
                n.StorefrontData.Storestock[i] = a.Value;
                p.Inventory.Remove(a.Value);
                incomingItemIndex++;
            }
        }

        //newState.SelectedBoardNodeId = -1;
        newState.GamePhase = GamePhase.InitialTurnMenu; // This should be an expected phase
    }

    private void HandleFinishStockStore(GameState prevState, ref GameState newState, ItemStats[] newItems)
    {
        // Put the items in the store now
        ref var n = ref newState.BoardNodes[newState.SelectedBoardNodeId];
        n.StorefrontData.Storestock = newItems;


        newState.SelectedBoardNodeId = -1;
        newState.GamePhase = GamePhase.InitialTurnMenu; // This should be an expected phase
    }

    private void HandleSomething(GameState prevState, ref GameState newState)
    {
        // DO NOT USE
        Debug.Log("ha ha handle something");
    }

    #endregion

    private GamePhase HandleItemSelection(GameState prevState, ref GameState newState)
    {
        ref var p = ref newState.CurrentPlayer().EntityData;
        p.UpdateStatModifiers();
        //p.RefreshStatModifiers();
        ApplyItemEffectsOnTurnStart(ref p);

        return GamePhase.InitialTurnMenu;
    }

    int CalculateMoveDieRoll(GameState prevState, ref GameState newState)
    {
        var faceRoll = Random.Range(0, 6); // Roll from 0 to 5
        Debug.Log($"Movement | Rolled [{faceRoll}]");
        var ed = newState.CurrentPlayer().EntityData;
        if (newState.UsingSpeedDieToMove)
        {
            // use speed die face values
            ed.Health -= 25;

            DieConfig die = ed.EntityStats.dieConfigs[(int)(Action.WeaponTypes.Speed)];

            int speedRollValue = die[faceRoll];
            int additionalRollValue;

            var rollsRemaining = currentPlayer.currentStatsModifier.rollModifier;
            while (rollsRemaining > 0)
            {
                //Debug.Log($"Rolls Left{rollsRemaining}");
                additionalRollValue = Random.Range(0, 6); // roll again until there's no more
                speedRollValue += die[additionalRollValue];
                rollsRemaining--;
            }

            speedRollValue *= currentPlayer.currentStatsModifier.movementMultModifier;
            speedRollValue += currentPlayer.currentStatsModifier.movementFlatModifier;

            faceRoll = speedRollValue;

            //currentPlayer.movementTotal = currentPlayer.movementLeft = speedRollValue;

            if (ed.CurrentStatsModifier.redirectOnSpeedDie)
            {
                //ed.PreviousNode = null; // remove this
                ed.PreviousNodeId = -1;
            }

            m_UpdatePlayerScore.RaiseEvent(currentPlayer.id); // make ScoreManager listen to this
            //m_RollForMovement.RaiseEvent(speedRollValue);
            //m_PlayerMovedOnBoard.RaiseEvent(); // idk why this has to be a seperate event
        }
        else
        {
            faceRoll += 1; // Roll from 1 to 6

            var rollsRemaining = ed.CurrentStatsModifier.rollModifier;
            while (rollsRemaining > 0)
            {
                Debug.Log($"Rolls Left{rollsRemaining}");
                faceRoll += 1 + Random.Range(0, 6); // roll again until there's no more
                rollsRemaining--;
            }

            // Apply movement item effects.
            faceRoll *= ed.CurrentStatsModifier.movementMultModifier;
            faceRoll += ed.CurrentStatsModifier.movementFlatModifier;

            //currentPlayer.movementTotal = currentPlayer.movementLeft = faceRoll;
            //m_RollForMovement.RaiseEvent(faceRoll);
            //m_PlayerMovedOnBoard.RaiseEvent(); // idk why this has to be a seperate event
        }

        return faceRoll;
    }

    

    #region Combat-Based Functions
    private bool CanFightPlayers(List<int> otherPlayers)
    {
        // otherPlayer != null && otherPlayer != currentPlayer
        return otherPlayers.Count != 0;
    }

    private bool ThisPlayerMustFight(EntityData entity)
    {
        //return entity.currentStates.Contains(EntityPiece.State.Fighting) || entity.currentStates.Contains(EntityPiece.State.FightingParty);
        return entity.Statuses.Contains(Status.Fighting) || entity.Statuses.Contains(Status.FightingParty);
    }

    private GamePhase InitiateCombat(EntityData entity, MapNode m)
    {
        // REWORK THIS
        /*
        //rollTypewriter.ShowText("");
        //playersOnCurrentNode = m.playersOccupied;
        var otherPlayers = GetOtherPlayersOnNode(entity, m);

        // Temporary behavior
        // Randomly pick a player on the passed tile to fight
        //EntityPiece selectedPlayer = otherPlayers[Random.Range(0, otherPlayers.Count)];

        if (otherPlayers.Count > 1)
        {
            //phase = GamePhase.CombatSelector;
            foreach(EntityData e in m.occupiedEntities)
            {
                // Indicate this is a third-party situation
                if(!e.Statuses.Contains(Status.FightingParty))
                    e.Statuses.Add(Status.FightingParty);
            }

            if (!entity.Statuses.Contains(Status.FightingParty))
                entity.Statuses.Add(Status.FightingParty);

            //m_LandOnMultipleEntities.RaiseEvent(otherPlayers);
            // UI Combat Selector should just grab the OccupiedNode of the current player 
            return GamePhase.CombatSelector;
        }
        else
        {
            BeginCombat(otherPlayers.First());
            return GamePhase.CombatTime;
        }
        */
        return GamePhase.CombatTime;
    }
    
    private void HandleInitiateCombat(GameState prevState, ref GameState newState)
    {
        var p = newState.Players[newState.CurrPlayerId].EntityData;
        ref var n = ref newState.BoardNodes[p.OccupiedNodeId];


        newState.GamePhase = GamePhase.CombatSelector;
        newState.GamePhase = GamePhase.CombatTime;
    }

    private void BeginCombat(EntityData selectedEntity)
    {
        //_currentGameState.CurrentPlayer
        var p = _currentGameState.CurrentPlayer();
        if (!p.EntityData.Statuses.Contains(Status.Fighting))
            p.EntityData.Statuses.Add(Status.Fighting);

        if(!selectedEntity.Statuses.Contains(Status.Fighting)) 
            selectedEntity.Statuses.Add(Status.Fighting);

        //phase = GamePhase.CombatTime;
        

        StartCoroutine(StartTransitionIntoCombat(.5f, selectedEntity));
        //m_EnteredCombatScene.RaiseEvent();
        //m_InitiateCombatOnPassBy.RaiseEvent(otherPlayer);

        currentPlayer.traveledNodes.Clear();
        currentPlayer.traveledNodes.Add(currentPlayer.occupiedNode);

        encounterStarted = true;
        //return GamePhase.CombatTime;
    }

    private void OnFighterSelected(EntityPiece entity)
    {
        Debug.Log("GameplayTest | OnFighterSelected");
        //BeginCombat(entity);
    }
    #endregion

    #region Deprecated?

    void RockPaperScissors(EntityPiece p)
    {
        Debug.Log($"RockPaperScissors | {currentPlayer} has entered a PvE fight");
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

    private void HandleLevelUp(GameState prevState, ref GameState newState)
    {
        // current attribute selected is marked by the int attSelected. 1 is attack, 2 is gun, 3 is magic.
        // diceSelected represents the current dice face. All of these have -1 applied in arrays.
        var p = newState.CurrentPlayer().EntityData;

        p.UnspentSP += 6;
        p.MaxHealth += 5;
        p.BaseMaxHealth += 5;
        p.Health += 5;
        p.Level += 1;
        p.ExpThreshold = p.LevelUpFormula();

        // poopy
        switch (p.Level)
        {
            case 3:
                newState.LastGamePhase = GamePhase.BlessingTree;
                break;
            case 6:
                newState.LastGamePhase = GamePhase.BlessingTree;
                break;
            default:
                //newState.LastGamePhase = GamePhase.StatAllocation;
                break;
        }
        // Invoke / call for the Level Up cutscene
        newState.GamePhase = GamePhase.IncidentHappening;
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
        //phase = GamePhase.EndTurn;
        //encounterOver = false;
        //storeScreen.SetActive(false);
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
    GamePhase EndOfTurn(GameState prevState, ref GameState newState)
    {
        var p = newState.CurrentPlayer();
        if(p.EntityData.Inventory.Count > p.EntityData.InventoryLimit)
        {
            // Player has too many items
            Debug.Log($"{p.EntityData.EntityName} HAS TOO MANY ITEMS");
            //phase = GamePhase.DiscardItem;
            //StartCoroutine(DelayFullInventory(p, .25f)); // Rework this function
            newState.LastGamePhase = prevState.GamePhase;
            return GamePhase.DiscardItem;
            //m_FullInventory.RaiseEvent(p);
            //phase = GamePhase.DiscardItem;
        }
        else
        {
            // Interest logic here TEMPORARY
            p.EntityData.Money += (int)(p.EntityData.CurrentStatsModifier.interestRate * p.EntityData.StorestockTotal);

            // Regular turn end logic
            //if (currentPlayerInitialNode.playerOccupied == currentPlayer)
            //if (currentPlayerInitialNode.playersOccupied.Contains(currentPlayer))
            //{
            //    currentPlayerInitialNode.playersOccupied.Remove(currentPlayer);
            //}
            var occupiedNode = newState.BoardNodes[p.EntityData.OccupiedNodeId];
            var initalNode = newState.BoardNodes[p.EntityData.InitialNodeId];

            if (initalNode.occupiedEntities.Contains(p.Id))
            {
                initalNode.occupiedEntities.Remove(p.Id);
            }

            // Was in a third-party situation and is the last one standing!!
            if (occupiedNode.occupiedEntities.Count == 1 &&
                p.EntityData.Statuses.Contains(Status.FightingParty))
            {
                Debug.Log($"Wait {p.EntityData.EntityName} is the goat wtf?");
                p.EntityData.Statuses.Remove(Status.FightingParty);
                //p.dustCloud.SetActive(false); // change this in EntityPieceVisual
            }

            //isStockingStore = false; // let next player access inventory
            newState.UsingSpeedDieToMove = false;
            //playerUsedItem = false; // let next player access inventory
            newState.CurrentPlayerHasUsedItem = false;
            //selectedItemIndex = -1;

            m_UpdatePlayerScore.RaiseEvent(currentPlayer.id);
            //rollTypewriter.ShowText("");


            playersActed++;
            if (playersActed == currentRuleset.numberOfPlayers)
            {
                Debug.Log($"Round {newState.Round} over.");
                //turnRound++;
                playersActed = 0;

                newState.Round++;
                // Raise event that round is over with the current round #.
                return GamePhase.StartRound;
            }
            else
            {
                //StartCoroutine(DelaySetupNextPlayer(.25f));
                SetupNextPlayer(prevState, ref newState);
                return GamePhase.StartTurn;
            }
        }
    }

    public GamePhase HandleNextTurnRound(GameState prevState, ref GameState newState)
    {
        bool active = false;
        foreach (SpecialIncidents incidents in matchIncidents)
        {
            switch (incidents)
            {
                case SpecialIncidents.Train:
                    if (newState.Round != 1 && newState.Round % 6 == 0)
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
            //phase = GamePhase.IncidentHappening;
            m_IncidentStarted.RaiseEvent();
            StartCoroutine(PlayOutActiveIncidents());
            return GamePhase.IncidentHappening;
        }
        else
        {
            //DelaySetupNextPlayer(0.25f);
            SetupNextPlayer(prevState, ref newState);
            return GamePhase.StartTurn;
        }
    }

    public void SetupNextPlayer(GameState prevState, ref GameState newState)
    {
        var p1 = newState.CurrentPlayer().EntityData;
        ref var n = ref newState.BoardNodes[p1.OccupiedNodeId];
        // previous player is put on the spot they landed at
        //currentPlayer.occupiedNode.playersOccupied.Add(currentPlayer); // update to have that player in that node now
        //currentPlayer.occupiedNodeCopy = currentPlayer.occupiedNode;

        //p.OccupiedNode.occupiedEntities.Add(p);
        //p.InitialNode = p.OccupiedNode;

        n.occupiedEntities.Add(newState.CurrPlayerId);
        p1.InitialNodeId = p1.OccupiedNodeId;

        //_inputControllers[currentPlayer.id].SwitchActionMapRewrite(_currentGameState.GamePhase);
        _inputControllers[newState.CurrPlayerId].SwitchActionMapRewrite(_currentGameState.GamePhase);

        // Put current player back to normal pos
        //currentPlayer.transform.position += new Vector3(0, 0, .05f);
        //newState.CurrentPlayerObj.transform.position += new Vector3(0, 0, .05f);

        // Change to the next player in the list.
        nextPlayers.Remove(currentPlayer);
        nextPlayers.Add(currentPlayer);
        currentPlayer = nextPlayers[0];

        // Go to next player ID
        newState.CurrPlayerId++;
        if (newState.CurrPlayerId >= currentRuleset.numberOfPlayers) newState.CurrPlayerId = 0;

        newState.CurrentPlayerObj = playerUnits[newState.CurrPlayerId];
        var p2 = newState.CurrentPlayer().EntityData;

        // Done eating and chillin, get out of the estalbishment!
        if (p2.Statuses.Contains(Status.InsideVendor))
        {
            p2.Statuses.Remove(Status.InsideVendor);
            newState.CurrentPlayerObj.playerSprite.enabled = true; // move this in EntityVisual
        }

        if (newState.BoardNodes[p2.OccupiedNodeId].occupiedEntities.Count == 1 &&
            p2.Statuses.Contains(Status.FightingParty))
        {
            Debug.Log($"Wait {p2.EntityName} is actually goated wtf?");
            p2.Statuses.Remove(Status.FightingParty);

            newState.CurrentPlayerObj.dustCloud.gameObject.SetActive(false); // move this in EntityVisual
        }

        // put next player's sprite in front of the others
        newState.CurrentPlayerObj.transform.position -= new Vector3(0,0,.05f);
        newState.TraveledNodeIds.Add(p2.OccupiedNodeId);

        newState.CurrentPlayerDataStates = new EntityData[0];
        newState.SnapshotCurrentPlayerData();

        m_NextPlayerTurn.RaiseEvent(currentPlayer);

        foreach (var inputController in _inputControllers)
        {
            inputController.SetCurrentPlayer(newState.CurrPlayerId);
        }
        //currentPlayerInitialNode = currentPlayer.occupiedNode;
        //phase = GamePhase.ItemSelection;
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
        //SetupNextPlayer(prevState, ref newState);
    }

    public void HandlePostCutscene(GameState prevState, ref GameState newState)
    {
        Debug.Log($"Cutscene over, going to {newState.LastGamePhase} phase");
        newState.UsedItem = null;
        newState.GamePhase = newState.LastGamePhase; // maybe expectedphase instead?
    }

    public void EndGame(GameState prevState, ref GameState newState)
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
        newState.GamePhase = GamePhase.GameOver;
        //phase = GamePhase.GameOver;
    }
    #endregion

    private void ConfirmPurchase(ItemStats item)
    {
        var store = currentPlayer.occupiedNode.GetComponent<MapNode>().GetComponent<StoreManager>();
        // When an item is bought, allow confirmation via SPACE bar to continue the game
        if (item != null)
        {
            UpdateStorefrontVisual(currentPlayer.occupiedNode.GetComponent<MapNode>());
            //encounterOver = true;
            //currentPlayer.heldPoints -= (int)(item.basePrice * store.storePriceMultiplier); *Now in UIStoreManager using BuyItem
            //currentPlayer.ReputationPoints += 20 + ((item.basePrice * store.storePriceMultiplier) / 10); *Now in UIStoreManager using BuyItem

            if (currentPlayer.heldPoints < 0)
            {
                currentPlayer.currentStates.Add(EntityPiece.State.DeathsRow);
            }

            m_UpdatePlayerScore.RaiseEvent(currentPlayer.id);
        }
    }

    private void OnUseItemAt(int index, ItemStats item)
    {
        // Recieved from InvSelHand to try using an item
        selectedItemIndex = index;
    }

    private void ApplyItemEffectsOnTurnStart(ref EntityData p)
    {
        //Debug.Log($"{p.Health} + {p.CurrentStatsModifier.healthRegen} = adda");
        // Regenerate health from active effects.
        p.MaxHealth = p.BaseMaxHealth * p.CurrentStatsModifier.maxHealthMultModifier + p.CurrentStatsModifier.maxHealthFlatModifier;
        p.Health = Mathf.Min(p.MaxHealth, p.Health + p.CurrentStatsModifier.healthRegen);
        //Debug.Log($"{p.Health} + {p.CurrentStatsModifier.healthRegen} = adda");

        if (p.Health < 1)
        {
            p.Health = 1;
        }

        //m_UpdatePlayerScore.RaiseEvent(p.id);

        // Warp player to specified destination.
        if (p.CurrentStatsModifier.warpDestination != null)
        {
            //WarpToMapNode(p, p.currentStatsModifier.warpDestination);
            expectedPhase = GamePhase.InitialTurnMenu;
            m_WarpByItem.RaiseEvent(p.CurrentStatsModifier.warpDestination);
        }
    }

    private void ApplyItemEffectsOnTargetSelection(EntityData entity)
    {
        // Warp player to specified destination.
        if (entity.CurrentStatsModifier.warpDestination != null)
        {
            //WarpToMapNode(p, p.currentStatsModifier.warpDestination);

            expectedPhase = GamePhase.InitialTurnMenu; // change this to newstate
            m_WarpByItem.RaiseEvent(entity.CurrentStatsModifier.warpDestination);
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

    public void OnDisableFreeview()
    {
        if(currentPlayer.currentStatsModifier.warpMode != EntityStatsModifiers.WarpMode.None)
        {
            currentPlayer.currentStatsModifier.warpMode = EntityStatsModifiers.WarpMode.None;
            //selectedItemIndex = -1;
        }
    }
    #region Store Related Functions
    // Old function, delete
    public void OldBuildStore(EntityData p)
    {
        p.StoreCount++;
        //p.heldPoints -= 200;

        ShowStore(p, p.OccupiedNode); 

        GameObject tileObject = p.OccupiedNode.gameObject;
        StoreManager store = tileObject.AddComponent<StoreManager>();
        //store.playerOwner = p;
        store.owner = p;
        //p.OwnedStores.Add(store);

        StartCoroutine(DelayRestockStore(p, 1f));
        //m_RestockStore.RaiseEvent(p.occupiedNode);
    }

    // move into the map node itself?
    private void ShowStore(EntityData p, MapNode m)
    {
        Debug.Log("I am a store");
        GameObject tileObject = m.gameObject;
        tileObject.tag = "Store";

        //tileObject.GetComponent<SpriteRenderer>().color = p.playerColor;
        var node = tileObject.GetComponent<MapNode>();
        node.GetComponent<SpriteRenderer>().color = p.MainColor;
        //Show store boat now
        node.storefrontVisual.enabled = true;
        node.storefrontVisual.GetComponent<SpriteRenderer>().color = p.MainColor;

        node.stockGroup.alpha = 1;
        node.stockGroup.GetComponent<Image>().color = p.MainColor;
        node.stockGroup.GetComponent<Image>().color -= new Color(0, 0, 0, .25f);
    }

    private void RemoveStore(MapNode m)
    {
        Debug.Log("I no longer a store");
        GameObject tileObject = m.gameObject;
        tileObject.tag = "Encounter";

        tileObject.GetComponent<SpriteRenderer>().color = Color.white;
        var node = tileObject.GetComponent<MapNode>();

        //Show store boat now
        node.storefrontVisual.enabled = false;
        node.storefrontVisual.GetComponent<SpriteRenderer>().color = Color.white;

        node.stockGroup.alpha = 0;
        node.stockGroup.GetComponent<Image>().color = Color.white;
        node.stockGroup.GetComponent<Image>().color -= new Color(0, 0, 0, .25f);

        var store = node.GetComponent<StoreManager>();
        Destroy(store);
    }

    private IEnumerator DelayRestockStore(EntityData p, float delay)
    {
        yield return new WaitForSeconds(delay);

        m_RestockStore.RaiseEvent(p.OccupiedNode);
    }

    public void OnRestockStore(MapNode node)
    {
        //isStockingStore = true;

        emptyStockCount = 0;
        var storeInventory = node.GetComponent<StoreManager>().storeInventory;

        currentRestockNode = node;
        foreach (ItemStats item in storeInventory)
        {
            if (item == null)
                emptyStockCount++;
        }
    }

    public void OnUpgradeStore(MapNode node)
    {
        var store = node.GetComponent<StoreManager>();

        // prob have a cutscene play instead
        store.LevelUpStore();
        currentPlayer.CalculateStorestockTotal();
        currentPlayer.heldPoints -= 150;
        m_UpdatePlayerScore.RaiseEvent(store.playerOwner.id);

        //phase = GamePhase.EndTurn;
        phase = GamePhase.IncidentHappening;
        expectedPhase = GamePhase.EndTurn;

        node.pd_nUpgradeStore.Play();
    }
    
    public void OnTryRelocateStore(MapNode node)
    {
        //var store = node.GetComponent<StoreManager>();
        currentRestockNode = node;
        currentPlayer.currentStatsModifier.warpMode = EntityStatsModifiers.WarpMode.OpenSpaces;

        m_EnableFreeview.RaiseEvent();
        m_EnterRaycastTargetSelection.RaiseEvent(); // change this

        //phase = GamePhase.EndTurn;
        phase = GamePhase.RaycastTargetSelection;
        expectedPhase = GamePhase.EndTurn;
    }

    public void TrackItemFromPlayerInventory(int index, ItemStats itemStats)
    {
        
        // Keep track of items stocked
        // this is to make sure if player cancels their selection you can give back the items
        /*
        recentStockedItems.Add(itemStats);
        emptyStockCount--;

        currentPlayer.inventory.RemoveAt(index);
        m_RefreshInventory.RaiseEvent(currentPlayer);
        Debug.Log("m_RefreshInventory raised, rewrite this");

        if (emptyStockCount <= 0)
            m_FinishStockingStore.RaiseEvent(currentPlayer);
        */
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

        //isStockingStore = false;

        recentStockedItems.Clear();
        m_ExitInventory.RaiseEvent();

        // temporary
        player.CalculateStorestockTotal();
    }

    // This should be in StoreManager
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
        Debug.Log("m_RefreshInventory raised, rewrite this");
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

    public IEnumerator InitiateCombatOnEnemy(float delay, EntityData entity)
    {
        EntityData enemy = entity; // chagne this stupid ass
        /*
        // Set IDs of players entering combat.
        sceneManager.player1ID = entity.id;

        // Rework this completetly for Scaling Enemy System
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

        if(enemy.occupiedNode != null)
            enemy.occupiedNode.playersOccupied.Remove(enemy);

        enemy.occupiedNode = p.occupiedNode;
        p.occupiedNode.playersOccupied.Add(enemy);
        // End rework


        //enemy.occupiedNode = p.occupiedNode;
        //p.occupiedNode.playersOccupied.Add(enemy);
        */
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

    // originally OnSelectRaycastTarget()
    public void HandleTryFreeviewExamine(GameState prevState, ref GameState newState)
    {
        StoreManager store = null;
        if (RaycastTiles.tileSelected.TryGetComponent<StoreManager>(out StoreManager s))
        {
            Debug.Log($"There is a store at {RaycastTiles.tileSelected}. Owned by { s.playerOwner}");
            store = s;
        }
        var p = newState.Players[newState.CurrPlayerId].EntityData;

        switch (p.CurrentStatsModifier.warpMode)
        {
            case EntityStatsModifiers.WarpMode.Tiles:
                if (RaycastTiles.tileSelected != null)
                    WarpConfirmed(p);
                break;
            case EntityStatsModifiers.WarpMode.Players:
                if (RaycastTiles.tileSelected.playersOccupied.Count != 0)
                    WarpConfirmed(p);
                break;
            case EntityStatsModifiers.WarpMode.Marigold:
                if (RaycastTiles.tileSelected.GetComponent<StoreManager>() && 
                    RaycastTiles.tileSelected.modifier == MapNode.Modifier.None)
                {
                    PlantConfirmed(p, MapNode.Modifier.Marigold);
                }
                break;
            case EntityStatsModifiers.WarpMode.Rafflesia:
                if (RaycastTiles.tileSelected.modifier == MapNode.Modifier.None)
                {
                    PlantConfirmed(p, MapNode.Modifier.Rafflesia);
                }
                break;
            case EntityStatsModifiers.WarpMode.OpenSpaces:
                if (RaycastTiles.tileSelected.tag == "Encounter")
                {
                    RelocateStoreConfirmed(p, RaycastTiles.tileSelected);
                }
                break;

        }
    }

    private void WarpConfirmed(EntityData entity)
    {
        // FOR NAM: USE THIS EVENT TO HIDE SELECT TILE/PLAYER UI
        m_ExitRaycastTargetSelection.RaiseEvent();
        entity.CurrentStatsModifier.warpDestination = RaycastTiles.tileSelected;
        ApplyItemEffectsOnTargetSelection(entity);

        m_DisableFreeview.RaiseEvent();
        phase = GamePhase.InitialTurnMenu;
    }

    private void PlantConfirmed(EntityData entity, MapNode.Modifier modifier)
    {
        // FOR NAM: USE THIS EVENT TO HIDE SELECT TILE/PLAYER UI
        m_ExitRaycastTargetSelection.RaiseEvent();
        entity.CurrentStatsModifier.warpDestination = RaycastTiles.tileSelected;
        PlantItemOnSpaceSelection(entity, modifier);

        m_DisableFreeview.RaiseEvent();
        phase = GamePhase.InitialTurnMenu; // change this to newstate 
    }

    private void RelocateStoreConfirmed(EntityData p, MapNode node2)
    {
        p.Money -= 100;
        //currentRestockNode
        // Copy the store manager over to the new selected node
        var originalStore = currentRestockNode.GetComponent<StoreManager>();
        var newStoreLocation = node2;

        var newStore = node2.gameObject.AddComponent<StoreManager>();
        newStore.SetStore(originalStore);

        //p.OwnedStores.Remove(originalStore);
        //p.OwnedStores.Add(newStore);

        //currentRestockNode.gameObject.tag = "Encounter";
        RemoveStore(currentRestockNode);
        ShowStore(p, node2);
        UpdateStorefrontVisual(node2);
        //node2.GetComponent<SpriteRenderer>().color = p.playerColor;

        m_ExitRaycastTargetSelection.RaiseEvent();
        m_DisableFreeview.RaiseEvent();
        // put cutscene  event raise event here??
        phase = expectedPhase;
    }

    private void PlantItemOnSpaceSelection(EntityData entity, MapNode.Modifier modifier)
    {
        // Warp player to specified destination.
        if (entity.CurrentStatsModifier.warpDestination != null)
        {
            entity.CurrentStatsModifier.warpDestination.modifier = modifier;
            entity.CurrentStatsModifier.warpDestination.modifierOwner = entity;

            if(selectedItemIndex != -1)
            {
                // deployed something due to item usage!
                Debug.Log("PlantItemOnSpaceSelection | deployed something due to item usage!");
                // call the event to play the animation here!
                // temp behavior, just to get rid of the item
                //m_FinishedUsedItem.RaiseEvent();
                //m_ItemUsed.RaiseEvent(selectedItemIndex, currentPlayer.inventory[selectedItemIndex]);
            }

            switch(modifier)
            {
                case MapNode.Modifier.Marigold:
                    RaycastTiles.tileSelected.flowerTrapVisual.enabled = true;
                    RaycastTiles.tileSelected.flowerTrapVisual.color = currentPlayer.playerColor;
                    RaycastTiles.tileSelected.flowerTrapVisual.sprite = RaycastTiles.tileSelected.flowerTrapSprites[0];
                    break;
                case MapNode.Modifier.Rafflesia:
                    RaycastTiles.tileSelected.flowerTrapVisual.enabled = true;
                    RaycastTiles.tileSelected.flowerTrapVisual.color = currentPlayer.playerColor;
                    RaycastTiles.tileSelected.flowerTrapVisual.sprite = RaycastTiles.tileSelected.flowerTrapSprites[1];
                    break;
                default:
                    Debug.Log("PlantItemOnSpaceSelection | No MapNode.modifier! Umm, something went wrong?");
                    break;
            }
        }
    }

    IEnumerator StartTransitionIntoCombat(float delay, EntityData entity)
    {
        // This is like kinda only for player v player
        //m_TransitionIntoCombat.RaiseEvent(entity);
        m_EnteredCombatScene.RaiseEvent();
        yield return new WaitForSeconds(delay);

        //m_EnteredCombatScene.RaiseEvent();
        //m_InitiateCombatOnPassBy.RaiseEvent(entity);

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
        //foreach (PlayerState player in newState.Players)
        //{
        //    // this dude is chilling, he aint getting hit
        //    if (player.EntityData.Statuses.Contains(Status.InsideVendor))
        //    {
        //        Debug.Log($"{player.EntityData.EntityName} is in a vendor, no damage taken.");
        //    }
        //    else if (affectedNodes.Contains(player.EntityData.OccupiedNode))
        //    {
        //        Debug.Log($"{player.EntityData.EntityName} in the streets. GOOBYE!!");
        //        int damageTaken = (int)(player.EntityData.MaxHealth * damageRatio);

        //        player.EntityData.Health -= damageTaken;

        //        if (player.EntityData.Health < 0)
        //        {
        //            player.EntityData.Health = 1;
        //        }

        //        //m_DamageTakenOnPlayer.RaiseEvent(player, damageTaken);
        //        //m_UpdatePlayerScore.RaiseEvent(player.id);
        //    }
        //}
    }
    
    private void HandleDamageAffectedNodes(GameState prevState, ref GameState newState, List<MapNode> affectedNodes, float damageRatio)
    {
        for(int i = 0; i < newState.Players.Length; i++)
        {
            // this dude is chilling, he aint getting hit
            if (newState.Players[i].EntityData.Statuses.Contains(Status.InsideVendor))
            {
                Debug.Log($"{newState.Players[i].EntityData.EntityName} is in a vendor, no damage taken.");
            }
            else if (affectedNodes.Contains(newState.Players[i].EntityData.OccupiedNode))
            {
                Debug.Log($"{newState.Players[i].EntityData.EntityName} in the streets. GOOBYE!!");
                int damageTaken = (int)(newState.Players[i].EntityData.MaxHealth * damageRatio);

                newState.Players[i].EntityData.Health -= damageTaken;

                if (newState.Players[i].EntityData.Health < 0)
                {
                    newState.Players[i].EntityData.Health = 1;
                }

                //m_DamageTakenOnPlayer.RaiseEvent(player, damageTaken);
                //m_UpdatePlayerScore.RaiseEvent(player.id);
            }
        }
    }

    private void TryCalculateTempoBuff(GameState prevState, ref GameState newState)
    {
        ref var p = ref newState.Players[newState.CurrPlayerId].EntityData;

        if (p.CurrentStatsModifier.movementTempoScale == 0) return;

        //var tempoFormula = p.currentStatsModifier.movementTempoScale * (int)((p.movementTotal - p.movementLeft) / 2);
        var tempoFormula = p.CurrentStatsModifier.movementTempoScale * (int)((newState.MoveDieRoll - newState.MovementLeft) / 2);

        if (tempoFormula == 0) return;

        Debug.Log($"TryCalculateTempoBuff | ATK Buff = {(int)((newState.MoveDieRoll - newState.MovementLeft) / p.CurrentStatsModifier.movementTempoScale)}");
        var tempoTempItem = ScriptableObject.CreateInstance<ItemStats>();

        tempoTempItem.name = "TempoItem";
        tempoTempItem.showAsEffect = false;
        tempoTempItem.modifiers = new List<StatModifierChangerSO>();

        var tempoAtkBuff = ScriptableObject.CreateInstance<AllDiesFinalValueMod>();

        tempoAtkBuff.flatAtkModifier = tempoFormula;
        tempoAtkBuff.multModifier = 1;
        tempoAtkBuff.activateEffectStartTurn = 1;
        tempoAtkBuff.activateEffectEndTurn = 1;
        tempoTempItem.name = $"TempoAtkBuff +{tempoAtkBuff.flatAtkModifier}";

        tempoTempItem.modifiers.Add(tempoAtkBuff);

        p.AddItemToActiveEffects(tempoTempItem.Duration, tempoTempItem);

        p.UpdateStatModifier(new ActiveEffect
        {
            OriginalItem = tempoTempItem,
            TurnsRemaining = 1,
            IsPermanent = false,
            ClearAfterCombat = false
        });


    }
}
