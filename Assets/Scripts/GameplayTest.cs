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
        Inventory,
        RollDice,
        PickDirection,
        MoveAround,
        PassBy,
        EncounterTime,
        StockStore,
        OverturnStore,
        RockPaperScissors,
        LevelUp,
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
    private int attSelected = 1;
    private int diceSelected = 1;
    private int pointsLeft = 0;
    //ui to remove for levelup - Nam
    public Canvas levelUpScreen;
    public TextMeshProUGUI remainingSP;
    public TextMeshProUGUI upgradeTooltip;
    public GameObject diceStats;
    public List<TextMeshProUGUI> playerDiceNumbers = new List<TextMeshProUGUI>();
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

    [Header("Listen on Event Channels")]
    public ItemEventChannelSO m_ItemBought; //Listening to this one
    public IntEventChannelSO m_ItemUsed; //Listening to this one
    public VoidEventChannelSO m_ExitRaycastedTile; //Listening to this one

    // Placeholder code, basis items for storefront
    public List<ItemStats> tempItems;

    private void OnEnable()
    {
        m_ItemBought.OnEventRaised += _PlaceholderChangeAndContinue;
        m_ItemUsed.OnEventRaised += RemoveItemInPlayerInventory;
        m_UpdatePlayerScore.OnEventRaised += RemoveDeathsRow;
        m_ExitRaycastedTile.OnEventRaised += DisableFreeview;
    }

    private void OnDisable()
    {
        m_ItemBought.OnEventRaised -= _PlaceholderChangeAndContinue;
        m_ItemUsed.OnEventRaised -= RemoveItemInPlayerInventory;
        m_UpdatePlayerScore.OnEventRaised -= RemoveDeathsRow;
        m_ExitRaycastedTile.OnEventRaised -= DisableFreeview;
    }

    // Start is called before the first frame update
    void Start()
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

        m_NextPlayerTurn.RaiseEvent(currentPlayer);

        turnText.text = currentPlayer.entityName + "'s Turn!";
        turnText.color = currentPlayer.playerColor;
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
        foreach(Stamp.StampType s in oldStamps)
        {
            Debug.Log(s);
        }


        p.UpdateStatModifiers();

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
        if (p.canLevelUp()) {
            pointsLeft = 5;
            p.maxHealth += 10;
            p.health += 10;
            p.RenownLevel += 1;
            UpdatePlayerDiceStats(p, diceStats);
            levelUpScreen.enabled = true;
            remainingSP.text = $"{pointsLeft} SP left.";
            upgradeTooltip.text = "Use [WASD] or [Arrows] to select dice faces.";
            phase = GamePhase.LevelUp;
            Debug.Log("Levelup screen!");
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

        // Change phase.
        if (p.heldPoints >= 4000)
        {
            phase = GamePhase.EndGame;
        }
        else if (p.movementLeft <= 0)
        {
            p.traveledNodes.Clear(); // Forget all the nodes traveled to
            p.traveledNodes.Add(p.occupiedNode); //i need this i guess

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
                    /*
                    // Placeholder restock your store on landing

                    for (int i = 0; i < 3; i++)
                    {
                        var randomNum = Random.Range(0, tempItems.Count);
                        if (store.storeInventory[i] == null)
                        {
                            store.storeInventory[i] = (tempItems[randomNum]);
                        }
                    }

                    // Placeholder restock
                    storeListings.text = "";
                    storeListingsLabel.text = "You restock your empty listings. This store now contains: ";
                    foreach (var listing in store.storeInventory)
                    {
                        if (listing != null) storeListings.text += listing.itemName + "\n";
                    }
                    storeScreen.SetActive(true);

                    encounterOver = true;
                    phase = GamePhase.ConfirmContinue;
                    */
                }
            }
            else if (m.CompareTag("Castle")) //Stash your points
            {
                encounterScreen.SetActive(true);
                p1fight.text = "";
                p2fight.text = "";
                /*
                resultInfo.text = "<size=45>[STASHING]</size>\n" + p.heldPoints + " point(s) have been stashed. \n<size=30> You are now at " + p.finalPoints + ". </size>";

                if (p.heldPoints != 0)
                    p.finalPoints += p.heldPoints;
                p.heldPoints = 0;
                */
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
                // instance combat scene
                // Combat scene gets a reference to GameplayTest
                // Combat scene calls function in gameplay test when someone wins, with winner
                //   and player objects (so items stay used)
                // That returns the winner, and points are stolen accordingly w/ phase++;encounterOver
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

                    //m_OpenInventory.RaiseEvent(p);
                    m_RestockStore.RaiseEvent(m);
                    /*
                    // randomly pick 3 items to put into the base store stock
                    for (int i = 0; i < 3; i++)
                    {
                        var randomNum = Random.Range(0, tempItems.Count);
                        store.storeInventory.Add(tempItems[randomNum]);
                    }

                    storeListings.text = "";
                    storeListingsLabel.text = "Store purchased! It's stocked up with:";

                    foreach (var listing in store.storeInventory)
                    {
                        if (listing != null) storeListings.text += listing.itemName + "\n";
                    }
                    storeScreen.SetActive(true);
                    encounterOver = true;
                    phase = GamePhase.ConfirmContinue;
                    */
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
                /*
                // Stock store
                for (int i = 0; i < 3; i++)
                {
                    var randomNum = Random.Range(0, tempItems.Count);
                    if (store.storeInventory[i] == null)
                    {
                        store.storeInventory[i] = (tempItems[randomNum]);
                    }
                }

                phase = GamePhase.EndTurn;
                */
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
            var monsterType = Random.Range(-6, 0); // int from -6 to -1

            sceneManager.player2ID = monsterType;

            var enemy = sceneManager.entities.Find(entity => sceneManager.player2ID == entity.id);
            if (enemy.combatSceneIndex == -1)
                lookingForTarget = false;
        }

        sceneManager.LoadCombatScene();
    }

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

    private void LevelUp(EntityPiece p)
    {
        // current attribute selected is marked by the int attSelected. 1 is attack, 2 is gun, 3 is magic.
        // diceSelected represents the current dice face. All of these have -1 applied in arrays.
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

        // Change to the next player in the list (if their turn is not skipped).

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
            currentPlayer.AddItemToActiveEffects(currentPlayer.inventory[index].duration, currentPlayer.inventory[index]);
            currentPlayer.UpdateStatModifiers();
            currentPlayer.inventory.RemoveAt(index);
            playerUsedItem = true;
        }        
    }

    private void RemoveDeathsRow(int id)
    {
        if (playerUnits[id].heldPoints >= 0)
        {
            Debug.Log(playerUnits[id].entityName + " is no longer in Death's Row");
            playerUnits[id].isInDeathsRow = false;
        }
    }

    public void PlayAudio(AudioClip clip)
    {
        audioSource.PlayOneShot(clip, 2f);
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

    // I ripped this from another script, delete this later
    public void UpdatePlayerDiceStats(EntityPiece entity, GameObject diceStats)
    {
        // Visually updates the dice stats ui based on the entity and side
        playerDiceNumbers.Clear();

        // Goes through the diceStats UI List and finds the text components
        foreach (Transform child in diceStats.transform)
        {
            playerDiceNumbers.Add(child.GetComponentInChildren<TextMeshProUGUI>());
        }

        // Updates each individual dice from the text list based on the type
        // the following code is ABSOLUTELY DISGUSTING
        var faceIndex = 0;

        for (int i = 0; i < 6; i++)
        {
            playerDiceNumbers[i].text = $"{entity.strDie[faceIndex]}";
            faceIndex++;
        }

        faceIndex = 0;

        for (int i = 6; i < 12; i++)
        {
            playerDiceNumbers[i].text = $"{entity.dexDie[faceIndex]}";
            faceIndex++;
        }

        faceIndex = 0;

        for (int i = 12; i < 18; i++)
        {
            playerDiceNumbers[i].text = $"{entity.intDie[faceIndex]}";
            faceIndex++;
        }
    }

    public void DisableFreeview()
    {
        freeviewEnabled = false;
    }
}
