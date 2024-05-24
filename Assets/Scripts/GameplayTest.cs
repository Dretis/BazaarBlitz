using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Linq;

public class GameplayTest : MonoBehaviour
{
    public static GameplayTest instance;
    // Tile Data and Shit IGNORE THIS SECTION FOR NOW
    [SerializeField]
    private List<EntityPiece> playerUnits = new List<EntityPiece>();
    [SerializeField]
    private List<EntityPiece> nextPlayers = new List<EntityPiece>();
    public EntityPiece currentPlayer;
    public MapNode currentPlayerInitialNode;

    public Dictionary<Vector2Int, GameObject> map = new Dictionary<Vector2Int, GameObject>();
    public Dictionary<Vector2Int, GameObject> unitPos = new Dictionary<Vector2Int, GameObject>();

    public enum GamePhase
    {
        InitialTurnMenu,
        ItemSelection,
        RaycastTargetSelection,
        Inventory,
        RollDice,
        PickDirection,
        MoveAround,
        PassBy,
        EncounterTime,
        StockStore,
        OverturnStore,
        RockPaperScissors,
        CombatTime,
        ConfirmContinue,
        EndTurn,
        EndGame
    }

    public int turn = 1;
    public GamePhase phase = GamePhase.RollDice;
    public int diceRoll;

    public TextMeshProUGUI rollText;
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
    private bool playerUsedItem = false; // please change these down the line
    public bool isStockingStore = false;

    //SOUND SHIT
    public AudioClip moveSFX;
    public AudioClip reverseSFX;
    public AudioSource audioSource;

    [SerializeField] private List<Stamp.StampType> oldStamps = new List<Stamp.StampType>();
    private int oldPoints = 0;

    // ui stuff for levelup;
    /*private int attSelected = 1;
    private int diceSelected = 1;
    private int pointsLeft = 0;*/
    //ui to remove for levelup - Nam
    //public Canvas levelUpScreen;
    //public TextMeshProUGUI remainingSP;
    //public TextMeshProUGUI upgradeTooltip;
    //public List<TextMeshProUGUI> playerDiceNumbers = new List<TextMeshProUGUI>();
    public GameObject diceStats;

    public TextMeshProUGUI storestockTooltip;

    public Canvas howToPlayScreen;


    // Event Channels
    [Header("Broadcast on Event Channels")]
    public VoidEventChannelSO m_EnableFreeview;
    public PlayerEventChannelSO m_DiceRollUndo;
    public PlayerEventChannelSO m_DiceRollPrep;
    public IntEventChannelSO m_RollForMovement;
    public IntEventChannelSO m_UpdatePlayerScore;

    public PlayerEventChannelSO m_PassByStamp;
    public StampEventChannelSO m_UndoPassByStamp;

    public VoidEventChannelSO m_PassByPawnShop;
    public PlayerEventChannelSO m_UndoPassByPawnShop;

    public PlayerEventChannelSO m_NextPlayerTurn;
    public PlayerEventChannelSO m_EncounterDecision;

    public PlayerEventChannelSO m_OpenInventory; // JASPER OR RUSSELL PLEASE USE THIS EVENT TO ACCESS THE INVENTORY
    public NodeEventChannelSO m_RestockStore;
    public VoidEventChannelSO m_ExitInventory;

    public PlayerEventChannelSO m_OverturnOpportunity;

    // Store-based Event Channels
    public NodeEventChannelSO m_LandOnStorefront;
    public VoidEventChannelSO m_ExitStorefront;

    // Pass-by Event Channels
    public PlayerEventChannelSO m_StealOnPassBy;
    public PlayerEventChannelSO m_InitiateCombatOnPassBy;
    public VoidEventChannelSO m_StopOnStoreOnPassBy;

    public VoidEventChannelSO m_EnterRaycastTargetSelection;
    public VoidEventChannelSO m_ExitRaycastTargetSelection;

    [Header("Listen on Event Channels")]
    public ItemEventChannelSO m_ItemBought; //Listening to this one
    public IntEventChannelSO m_ItemUsed; //Listening to this one
    public VoidEventChannelSO m_DisableFreeview;

    // Placeholder code, basis items for storefront
    public List<ItemStats> tempItems;

    private void OnEnable()
    {
        m_ItemBought.OnEventRaised += _PlaceholderChangeAndContinue;
        m_ItemUsed.OnEventRaised += RemoveItemInPlayerInventory;
        m_UpdatePlayerScore.OnEventRaised += RemoveDeathsRow;
        m_DisableFreeview.OnEventRaised += DisableFreeview;

        m_StealOnPassBy.OnEventRaised += StealFromPlayer;
        m_InitiateCombatOnPassBy.OnEventRaised += InitiateCombatOnPlayer;
        m_StopOnStoreOnPassBy.OnEventRaised += StopOnStore;
    }

    private void OnDisable()
    {
        m_ItemBought.OnEventRaised -= _PlaceholderChangeAndContinue;
        m_ItemUsed.OnEventRaised -= RemoveItemInPlayerInventory;
        m_UpdatePlayerScore.OnEventRaised -= RemoveDeathsRow;
        m_DisableFreeview.OnEventRaised -= DisableFreeview;

        m_StealOnPassBy.OnEventRaised -= StealFromPlayer;
        m_InitiateCombatOnPassBy.OnEventRaised -= InitiateCombatOnPlayer;
        m_StopOnStoreOnPassBy.OnEventRaised -= StopOnStore;
    }

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
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
        }

        // Get the player at the start of the list.
        currentPlayer = nextPlayers[0];
        //currentPlayer = nextPlayers[playerUnits.Count - 1];
        currentPlayerInitialNode = currentPlayer.occupiedNode;

        turnText.text = currentPlayer.entityName + "'s Turn!";
        turnText.color = currentPlayer.playerColor;
    }

    private void Start()
    {
        m_NextPlayerTurn.RaiseEvent(currentPlayer);
    }

    // Update is called once per frame
    void Update()
    {
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
                InitialTurnMenu(currentPlayer);
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
        foreach(Stamp.StampType s in oldStamps)
        {
            Debug.Log(s);
        }

        p.UpdateStatModifiers();

        ApplyItemEffectsOnTurnStart(p);

        phase = GamePhase.InitialTurnMenu;
    }

    private void InitialTurnMenu(EntityPiece p)
    {
        if (freeviewEnabled)
            return;

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

            audioSource.PlayOneShot(moveSFX, 2f);

            // End of listener code

            phase = GamePhase.RollDice;
        }
        if (playerUsedItem == false && (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)))
        {
            // Item Inventory
            // We chose to open inventory, tell listeners about it

            m_OpenInventory.RaiseEvent(p);

            // Put these in their own listener script

            audioSource.PlayOneShot(moveSFX, 2f);

            // End of listener code

            phase = GamePhase.Inventory;
        }
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
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Mouse1))
        {
            // Undo rolling, back to menu
            m_ExitInventory.RaiseEvent();

            audioSource.PlayOneShot(reverseSFX, 2f);

            phase = GamePhase.InitialTurnMenu;
        }
    }

    void RollDice(EntityPiece p)
    {
        // For now level up happens right before you roll dice
        if (p.canLevelUp() && p.combatSceneIndex != -1) {
            /*pointsLeft = 5;
            p.maxHealth += 10;
            p.health += 10;
            p.RenownLevel += 1;
            // SomeLevelUpObjectYouInitialize.StartLevelUp(p);
            UpdatePlayerDiceStats(p, diceStats);
            levelUpScreen.enabled = true;
            remainingSP.text = $"{pointsLeft} SP left.";
            upgradeTooltip.text = "Use [WASD] or [Arrows] to select dice faces.";
            phase = GamePhase.LevelUp;
            Debug.Log("Levelup screen!");*/
        }
        if(p.combatSceneIndex == -1)
        {
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

                // We just rolled for movement, tell listeners about it
                m_RollForMovement.RaiseEvent(diceRoll);

                // Put these in their own listener script
                rollText.text = "" + diceRoll;
                p.movementTotal = p.movementLeft = diceRoll;

                audioSource.PlayOneShot(moveSFX, 2f);

                // End of listener code

                phase = GamePhase.PickDirection;
            }
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Mouse1))
            {
                // Undo rolling, back to menu
                m_DiceRollUndo.RaiseEvent(p);

                audioSource.PlayOneShot(reverseSFX, 2f);

                phase = GamePhase.InitialTurnMenu;
            }
        }
        else
        {
            // In Combat
            phase = GamePhase.EncounterTime;
        }
    }

    void PickDirection(EntityPiece p)
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            wantedNode = p.occupiedNode.north;

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            wantedNode = p.occupiedNode.east;

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            wantedNode = p.occupiedNode.south;

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            wantedNode = p.occupiedNode.west;

        if (wantedNode != null)
        {
            phase = GamePhase.MoveAround;
        }
    }

    void MoveAround(EntityPiece p)
    {
        var lastEle = p.traveledNodes.Count;
        var lastNode = p.traveledNodes[lastEle - 1];

        if (wantedNode == lastNode) // The direction you picked was the node you just came from (Redo)
        {
            Stamp stampCollected = p.occupiedNode.gameObject.GetComponent<Stamp>();

            // Undo moves (not sure if this actually undoes multiple stamp collections)
            if (p.occupiedNode.CompareTag("Castle"))
            {
                p.stamps = new List<Stamp.StampType>(oldStamps);
                p.heldPoints = oldPoints;

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
            rollText.text = "" + p.movementLeft;

            p.transform.DOMove(lastNode.transform.position, .25f)
                .SetEase(Ease.OutQuint);

            wantedNode = null;
            audioSource.PlayOneShot(reverseSFX, 1.2f);

            phase = GamePhase.PickDirection; // Go back to picking direction
        }
        else if(wantedNode != null) // Go to that new node and occupy it
        {
            p.traveledNodes.Add(p.occupiedNode);
            p.occupiedNode = wantedNode;

            p.movementLeft--;
            rollText.text = "" + p.movementLeft;
            p.transform.DOMove(wantedNode.transform.position, .25f)
                .SetEase(Ease.OutQuint);

            wantedNode = null;
            audioSource.PlayOneShot(moveSFX, 1.2f);

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
                p.ReputationPoints += (75 * Mathf.Pow(1.5f, p.stamps.Count-1));
                p.heldPoints += (int)(150 * Mathf.Pow(2, p.stamps.Count-1));
                m_UpdatePlayerScore.RaiseEvent(currentPlayer.id);
                m_PassByPawnShop.RaiseEvent(); // change this later
            }
            p.stamps.Clear();
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

                    m_InitiateCombatOnPassBy.RaiseEvent(otherPlayer);

                    p.traveledNodes.Clear();
                    p.traveledNodes.Add(p.occupiedNode);

                    // Need NAM to disable input prompt (the number that shows up on top of the screen on roll).
                    // If enter combat before it fades, it persists on next player's turn.
                    return;
                }
            }
        }
        else if (m.CompareTag("Store") && p.currentStatsModifier.canStopOnStoreOnPassBy)
        {
            m_StopOnStoreOnPassBy.RaiseEvent();

            // Deactivate all active effects of items that end on store.
            p.RemoveItemEffectOnUse(ItemLists.StopOnStoreOnPassBy);
        }       
        
        // Change phase.
        if (p.heldPoints >= 4000)
        {
            phase = GamePhase.EndGame;
        }
        else if (p.movementLeft <= 0)
        {
            p.traveledNodes.Clear(); 
            p.traveledNodes.Add(p.occupiedNode); 

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
            sceneManager.DisableScene(0);
            sceneManager.EnableScene(p.combatSceneIndex);
        }
        // Player has not started combat
        else
        {
            var otherPlayer = p.occupiedNode.playerOccupied;
            if (m.CompareTag("Store")) // Forced to buy item(s)
            {
                // Have the node be occupied by the current player.
                m.playerOccupied = p;
                // Update portions of this code later
                GameObject tile = m.gameObject;
                StoreManager store = tile.GetComponent<StoreManager>();
                if (store.playerOwner != currentPlayer)
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
                        phase = GamePhase.ConfirmContinue;
                    }
                }
                else
                {
                    isStockingStore = true;
                    m_RestockStore.RaiseEvent(m);
                    //m_OpenInventory.RaiseEvent(p); // COMMENT THIS OUT WHEN RAISING THE RESTOCK EVENT
                    storestockTooltip.enabled = true; // PROBABLY PUT THIS IN UI AS WELL
                    phase = GamePhase.StockStore;                  
                }
            }
            else if (m.CompareTag("Castle")) //Stash your points
            {
                encounterScreen.SetActive(true);
                p1fight.text = "";
                p2fight.text = "";
                resultInfo.text = "<size=45>[PAWN SHOP]</size>\nLanded on pawn shop. All held stamps have been converted to points.\n<size=30> [SPACE] to continue.</size>";

                encounterOver = true;
                phase = GamePhase.ConfirmContinue;
            }
            else if (m.CompareTag("Stamp"))
            {
                // Placeholder visual for clarity
                encounterScreen.SetActive(true);
                p1fight.text = "";
                p2fight.text = "";
                resultInfo.text = "<size=45>[SAFE SPACE]</size>\nLanded on a stamp space.\nYou are safe from combat on this space.";
                resultInfo.text += "\n<size=24>[SPACE] to continue</size>";

                encounterOver = true;
                phase = GamePhase.ConfirmContinue;
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
                    encounterStarted = true;

                    // Set IDs of players entering combat.
                    sceneManager.player1ID = currentPlayer.id;
                    sceneManager.player2ID = otherPlayer.id;
                    sceneManager.LoadCombatScene();
                }
                else
                {
                    phase = GamePhase.EndTurn;
                }
            }
            else if (m.CompareTag("Encounter")) // Regular Encounter
            {
                // If unable to buy a store, skip the prompt and immediately enter combat.
                if (p.heldPoints < 200 || p.storeCount >= 4)
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
            }
        }
    }

    void OverturnStore(EntityPiece p, MapNode m)
    {
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
                store.playerOwner = currentPlayer;
                p.storeCount++;

                p.heldPoints -= 600;

                m_UpdatePlayerScore.RaiseEvent(currentPlayer.id);

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
    }

    void RockPaperScissors(EntityPiece p)
    {
        encounterStarted = true;

        // Set IDs of players entering combat.
        sceneManager.player1ID = p.id;

        bool lookingForTarget = true;
        while (lookingForTarget) {
            var monsterType = Random.Range(-13, 0); // int from -6 to -1

            sceneManager.player2ID = monsterType;

            var enemy = sceneManager.entities.Find(entity => sceneManager.player2ID == entity.id);
            if (enemy.combatSceneIndex == -1)
                lookingForTarget = false;
        }

        sceneManager.LoadCombatScene();
    }



    void ConfirmContinue(EntityPiece p)
    {
        if (encounterOver && Input.GetKeyDown(KeyCode.Space))
        {
            phase = GamePhase.EndTurn;
            encounterOver = false;
            encounterScreen.SetActive(false);
            storeScreen.SetActive(false);
            m_UpdatePlayerScore.RaiseEvent(currentPlayer.id);
            m_ExitStorefront.RaiseEvent();
        }
    }

    void EndOfTurn(EntityPiece p)
    {
        if (currentPlayerInitialNode.playerOccupied == currentPlayer)
        {
            currentPlayerInitialNode.playerOccupied = null;
        }

        // Reset temp values.
        oldStamps.Clear();
        oldPoints = 0;

        p.occupiedNode.playerOccupied = p; // update to have that player on that node now
        isStockingStore = false; // let next player access inventory
        playerUsedItem = false; // let next player access inventory

        m_UpdatePlayerScore.RaiseEvent(0);

        // Change to the next player in the list.

        nextPlayers.Remove(currentPlayer);
        nextPlayers.Add(currentPlayer);
        currentPlayer = nextPlayers[0];

        m_NextPlayerTurn.RaiseEvent(currentPlayer);

        turnText.text = currentPlayer.entityName + "'s Turn!";
        turnText.color = currentPlayer.playerColor;

        currentPlayerInitialNode = currentPlayer.occupiedNode;
        oldStamps = new List<Stamp.StampType>(currentPlayer.stamps); // keeping track of stamps for next player
        phase = GamePhase.ItemSelection;
    }

    void EndGame()
    {
        // The player with the most points wins!
        var winningPlayer = playerUnits.OrderBy(playerUnit => playerUnit.heldPoints).LastOrDefault();

        // UPDATE WITH ACTUAL END GAME UI, AND MAKE IN DIFFERENT SCRIPT WITH EVENT RAISED HERE.
        Debug.Log(winningPlayer.entityName + " is the KING OF THE MARKET!");
        encounterScreen.SetActive(true);
        resultInfo.text = $"{winningPlayer.entityName} WINS!!!";
    }

    private void _PlaceholderChangeAndContinue(ItemStats item)
    {
        // When an item is bought, allow confirmation via SPACE bar to continue the game
        if (item != null)
        {
            encounterOver = true;
            currentPlayer.heldPoints -= item.basePrice;

            m_UpdatePlayerScore.RaiseEvent(currentPlayer.id);
        }
    }

    private void RemoveItemInPlayerInventory(int index)
    {
        // This should be in its own script
        if (isStockingStore)
        {
            var store = currentPlayer.occupiedNode.GetComponent<StoreManager>();
            //Debug.Log(index);
            store.AddItem(currentPlayer.inventory[index]);
            currentPlayer.inventory.RemoveAt(index);
            m_OpenInventory.RaiseEvent(currentPlayer);
        } 
        else
        {
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
                m_ExitInventory.RaiseEvent();
                // Raise free view event I guess?
                m_EnableFreeview.RaiseEvent();
                // FOR NAM: USE THIS EVENT TO SHOW SELECT TILE/PLAYER UI.
                m_EnterRaycastTargetSelection.RaiseEvent();
                freeviewEnabled = true;

                phase = GamePhase.RaycastTargetSelection;
            }
        }        
    }

    private void ApplyItemEffectsOnTurnStart(EntityPiece p)
    {
        // Regenerate health from active effects.
        p.health = Mathf.Min(p.maxHealth * p.currentStatsModifier.maxHealthMultModifier 
            + p.currentStatsModifier.maxHealthFlatModifier, 
            p.health + p.currentStatsModifier.healthRegen);
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
        if (id > -1 && playerUnits[id].heldPoints >= 0 && playerUnits[id].isInDeathsRow)
        {
            Debug.Log(playerUnits[id].entityName + " is no longer in Death's Row");
            playerUnits[id].isInDeathsRow = false;
        }
    }

    private void StockStore(EntityPiece p, MapNode m)
    {
        GameObject tile = m.gameObject;
        StoreManager store = tile.GetComponent<StoreManager>();
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Mouse1)
                || !store.storeInventory.Exists(x => x == null))
        {
            // Exit store restocking.
            m_ExitInventory.RaiseEvent(); // REMOVE STOCKING STORE UI FROM SCREEN TOO
            storestockTooltip.enabled = false; // DO THIS IN THE UI
            audioSource.PlayOneShot(reverseSFX, 2f);

            phase = GamePhase.EndTurn;
        }
    }


    public void DisableFreeview()
    {
        freeviewEnabled = false;
    }

    public void StealFromPlayer(EntityPiece otherPlayer)
    {
        Debug.Log("StealFromPlayer");
        int indexToSteal = Random.Range(0, otherPlayer.inventory.Count);
        currentPlayer.inventory.Add(otherPlayer.inventory[indexToSteal]);

        if (currentPlayer.inventory.Count > 6)
        {
            // Raise event to drop items.
        }

        otherPlayer.inventory.RemoveAt(indexToSteal);

        // Raise event to show UI of item stolen. Not sure what to do if other player has no items to steal.
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
    }  

    public void SelectRaycastTarget(EntityPiece p)
    {
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
        }
    }

    private void WarpConfirmed(EntityPiece p)
    {
        m_DisableFreeview.RaiseEvent();

        // FOR NAM: USE THIS EVENT TO HIDE SELECT TILE/PLAYER UI
        m_ExitRaycastTargetSelection.RaiseEvent();
        p.currentStatsModifier.warpDestination = RaycastTiles.tileSelected;
        ApplyItemEffectsOnTargetSelection(p);
        phase = GamePhase.InitialTurnMenu;
    }
}
