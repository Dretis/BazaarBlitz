using Febucci.UI.Core;
using System;
using System.Collections.Generic;
using Core.Events;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Core;
using DG.Tweening;

/// <summary>
/// Handles the visual aspects of a Player Baggie present on the board.
/// </summary>
public class EntityPiece : MonoBehaviour
{
    // TO-DO
    // - Remove all logic variables and functions
    // - Move animation functions/script into this?
    // - Rename to 'EntityVisual' or 'EntityOverworldVisual'
    private GameplayTest _gameplayTest;
    [SerializeField] private TimelineSignalResponder _timelineSignalResponder;

    public enum State
    {
        Alive,
        Dead,
        Fighting,
        FightingParty,
        InsideVendor,
        Invulernable,
        DeathsRow,
    }

    //public bool isInDeathsRow = false; //replace this with the 'State' later

    public List<State> currentStates = new List<State>();
    public string entityName;
    public int id;
    public Color playerColor; // idk man
    public EntityInfo entityInfo; // Holds info about this specific being
    public SpriteRenderer playerSprite; // idk man
    public RuntimeAnimatorController combatAnimatorController; // to use and send in combat

    [Header("Player Score Info")]
    public int heldPoints = 0;
    public int health = 75;
    public int maxHealth = 75;

    [Space]
    [Range(1,99)]public int RenownLevel = 1; // Used to calculate the next level threshold.
    public float ReputationPoints = 0; // For enemies: how much rep they give on kill. For players: they're total exp
    //public float levelThreshold = 100;

    public List<Stamp.StampType> stamps = new List<Stamp.StampType>();

    [Header("Additional Info")]
    [Range(1, 10)] public int inventoryLimit = 8;
    [Range(0, 6)] public int storeCount = 0;
    public int storestockTotal = 0;

    [Header("Overworld Info")]
    public int movementTotal;
    public int movementLeft;

    public MapNode occupiedNode; // Node player is currently on
    public MapNode occupiedNodeCopy; // Node player's initial node at the start of the turn
    public MapNode previousNode = null; // Node player just walked on last turn. They can't go back this way.
    public int unspentLevelUpPoints = 0;
    public List<MapNode> traveledNodes = new List<MapNode>(); // Tracks the nodes the player has gone to

    public List<StoreManager> ownedStores = new List<StoreManager>();

    [Header("P Boat")]
    public PlayerPaletteLoader pBoatLoader;

    [Header("Particle Effects")]
    public GameObject dustCloud;
    public ParticleSystem coinDrop;
    public ParticleSystem coinSucking;
    public ParticleSystem levelUpRays;
    public ParticleSystem hitParticle;

    [Header("Visual Text Effects")]
    public TypewriterCore floatingDamageNumber;
    public TypewriterCore coinGainNumber;

    [Header("Cosmetic Dice")]
    public GameObject moveDieCosmeticPrefab;
    public GameObject strDieCosmeticPrefab;
    public GameObject dexDieCosmeticPrefab;
    public GameObject intDieCosmeticPrefab;

    // Dice faces initialization.
    public DieConfig strDie => entityStats.dieConfigs[(int)EntityBaseStats.DieTypes.Strength];
    public DieConfig dexDie => entityStats.dieConfigs[(int)EntityBaseStats.DieTypes.Dex];
    public DieConfig intDie => entityStats.dieConfigs[(int)EntityBaseStats.DieTypes.Int];
    public DieConfig spdDie => entityStats.dieConfigs[(int)EntityBaseStats.DieTypes.Speed];

    [Header("Combat Info")]
    //public int combatSceneIndex = -1; // -1 indicates player is not in battle
    //public List<CombatManager> involvedCombatManagers = new List<CombatManager>();
    public bool isEnemy;
    public int favoredAttack;
    public float spawnRarityModifier = 1; // 1 (full odds) to 0 (never spawns), set to a decimal percent to make the enemy spawn less. 
    // Ex, 0.33 means the enemy is 3x rarer

    public EntityBaseStats entityStats = new();

    public List<Action> attackActions;
    public List<Action> defendActions;
    public CombatUIManager.FightingPosition fightingPosition; // Just for the combat, will change

    public GameStateEventChannelSO _gameStateEvent;

    public void Initialize(PlayerState ps, GameplayTest gt)
    {
        _gameplayTest = gt;
        _timelineSignalResponder.Initialize(ps, gt);

        // Setup all the visual shit for this player in-game
        var colorPal = ps.EntityData.EntityPalette;
        var boatPal = ps.EntityData.BoatPalette;

        playerColor = colorPal[0];

        gameObject.GetComponent<PlayerPaletteLoader>().SetInspectorPalette(colorPal);
        dustCloud.GetComponent<PlayerPaletteLoader>().SetInspectorPalette(colorPal);

        // Boat Colors
        pBoatLoader.SetInspectorPalette(boatPal);

        transform.position = ps.EntityData.InitialNode.transform.position;
    }

    private void DisplayEntityFromGameState(FrameGameState gameState)
    {
        //if(gameState.NewState.CurrPlayerId != id) return;
        if (isEnemy) return;
        if (!gameState.DidChangePhases()) return;

        if (gameState.NewState.GamePhase is GameplayTest.GamePhase.StartTurn)
        {
            //Debug.Log($"SetEntityVisualPostCombat | ID: {id} | Object = {this.gameObject.name} ");
            SetEntityVisualPostCombat(gameState.NewState.Players[id].EntityData);
        }

        if (gameState.NewState.CurrPlayerId == id)
        {
            if (gameState.PrevState.GamePhase is GameplayTest.GamePhase.PickDirection)
            {
                var prevOccupiedNode = gameState.PrevState.Players[id].EntityData.OccupiedNode;


            }
            if (gameState.PrevState.GamePhase is GameplayTest.GamePhase.MoveAround)
            {
                MoveEntityToNode(gameState.NewState.Players[id].EntityData);
            }
        }
    }

    private void SetEntityVisualPostCombat(EntityData ed)
    {
        if (!ed.IsEnemy) // temp
        {
            if (ed.Statuses.Contains(Status.Fighting) || ed.Statuses.Contains(Status.FightingParty))
            {
                // In combat, dust cloud around players
                Debug.Log($"{ed.EntityName} is still in combat");
                dustCloud.gameObject.SetActive(true);
                //dustCloud.Play();
            }
            else dustCloud.gameObject.SetActive(false);

            floatingDamageNumber.ShowText("");
            coinGainNumber.ShowText("");
        }
    }

    private void MoveEntityToNode(EntityData ed)
    {
        var assignedPlayerNode = ed.OccupiedNode;
        transform.DOMove(assignedPlayerNode.transform.position, .25f)
            .SetEase(Ease.OutQuint);

        Debug.Log($"EntityVisual | Current player moved to {assignedPlayerNode}");
    }

    private void OnEnable()
    {
        _gameStateEvent.OnEventRaised += DisplayEntityFromGameState;
    }

    private void OnDisable()
    {
        _gameStateEvent.OnEventRaised -= DisplayEntityFromGameState;
    }

    public EntityStatsModifiers currentStatsModifier;

    public List<ItemStats> inventory = new();
    public List<int> lootOdds;

    [Serializable, Inspectable]
    public class ActiveEffect
    {
        public ItemStats originalItem;
        public int turnsRemaining;
        public bool isPermanent;
        public bool clearAfterCombat;
    }

    [SerializeField]
    public List<ActiveEffect> activeEffects = new();

    [Header("Blessing Category Count")]
    public List<int> blessingCategoryCounter; // 0 = wealth, 1 = power, 2 = speed

    [Header("Broadcast On Event Channels")]
    public PlayerEventChannelSO m_RefreshedActiveEffects;

    #region Item Active Effect Functions
    /// <summary>
    /// Add a specified item to this player's list of active stat modifier effects
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="item"></param>
    public void AddItemToActiveEffects(int duration, ItemStats item)
    {

        var sameEffect = activeEffects.Find(activeEffect => (UnityEngine.Object) activeEffect.originalItem == item);

        // Refresh effect if same item has been used before. Otherwise, add new effect.
        if (sameEffect != null && !item.CanStack)
        {
            sameEffect.turnsRemaining = duration;
        }
        else
        {
            activeEffects.Add(new ActiveEffect
            {
                originalItem = item,
                turnsRemaining = duration,
                isPermanent = item.IsPermanent,
                clearAfterCombat = item.ClearAfterCombat,
            });
        }
    }

    /// <summary>
    /// Tick down active effects by one turn and recalculate this player's stat modifiers
    /// </summary>
    public void UpdateStatModifiers()
    {
        TickDownActiveEffects();
        RefreshStatModifiers();
    }

    public void UpdateStatModifier(ActiveEffect effect)
    {
        TickDownActiveEffect(effect);
        RefreshStatModifiers();
    }

    public void RefreshStatModifiers()
    {
        currentStatsModifier = new EntityStatsModifiers();

        foreach (var item in activeEffects)
        {
            currentStatsModifier = item.originalItem.ApplyStatModChanges(currentStatsModifier, item.originalItem.Duration - item.turnsRemaining);
        }

        m_RefreshedActiveEffects.RaiseEvent(this);
    }

    private void TickDownActiveEffects()
    {
        for (int i = 0; i < activeEffects.Count; i++)
        {
            activeEffects[i].turnsRemaining--;

            if ((activeEffects[i].turnsRemaining < 0) && !activeEffects[i].isPermanent)
            {
                activeEffects.RemoveAt(i);
                i--;
            }
        }
    }

    private void TickDownActiveEffect(ActiveEffect effect)
    {
        var effectIndex = activeEffects.FindIndex(activeEffect => activeEffect.originalItem == effect.originalItem);
        if (effectIndex != -1)
        {
            activeEffects[effectIndex].turnsRemaining--;

            if (activeEffects[effectIndex].turnsRemaining < 0 && !activeEffects[effectIndex].isPermanent)
            {
                activeEffects.RemoveAt(effectIndex);
            }
        }
        else
        {
            Debug.Log("Effect not found");
        }
    }

    public void ClearAfterCombatActiveEffects()
    {
        for (int i = 0; i < activeEffects.Count; i++)
        {
            if (activeEffects[i].clearAfterCombat)
            {
                activeEffects.RemoveAt(i);
                i--;
            }
        }

        RefreshStatModifiers();
    }

    public void RemoveItemEffectOnUse(HashSet<string> itemNames)
    {
        List<EntityPiece.ActiveEffect> effectsToRemove = new List<EntityPiece.ActiveEffect>();

        foreach (var effect in activeEffects)
        {
            if (itemNames.Contains(effect.originalItem.itemName))
            {
                Debug.Log(effect.originalItem.name + "'s effect is removed!");
                effectsToRemove.Add(effect);
            }
        }

        activeEffects.RemoveAll(effect => effectsToRemove.Contains(effect));
        RefreshStatModifiers();
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        if (playerSprite != null && !this.TryGetComponent<PlayerPaletteLoader>(out var paletteLoader))
        {
            playerSprite.color = playerColor;
        }
        /*
        if (occupiedNode != null)
        {
            transform.position = occupiedNode.transform.position;
            occupiedNodeCopy = occupiedNode;
            traveledNodes.Add(occupiedNode);
        }
        */
    }

    public void CalculateStorestockTotal()
    {
        var finalItemPrice = 0;
        storestockTotal = 0;

        foreach (var store in ownedStores)
        {
            foreach(var item in store.storeInventory)
            {
                if (item == null) return; 

                finalItemPrice = (int)(item.basePrice * store.storePriceMultiplier);
                storestockTotal += finalItemPrice;
            }
        }

        Debug.Log($"Calulcated Storestock Total = {storestockTotal}");
    }

    // for use on enemies
    public void resetStats() {

        for (int l = RenownLevel; l > 1; l--) {
            for (int i = 0; i < 6; i++) {
                strDie.dieFaces[i] -= 1;
            }
            for (int i = 0; i < 6; i++) {
                dexDie.dieFaces[i] -= 1;
            }
            for (int i = 0; i < 6; i++) {
                intDie.dieFaces[i] -= 1;
            }
        }
        
        RenownLevel = 1;

    }

    public void raiseAllStats() {



        for (int i = 0; i < 6; i++) {
            strDie.dieFaces[i] += 1;
        }
        for (int i = 0; i < 6; i++) {
            dexDie.dieFaces[i] += 1;
        }
        for (int i = 0; i < 6; i++) {
            intDie.dieFaces[i] += 1;
        }
        
    }
}
