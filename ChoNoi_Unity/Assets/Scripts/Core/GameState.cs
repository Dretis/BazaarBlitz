using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Ideally will be serializable over network (so no unity objects that are not network objects)
    /// For things like players you can just keep the index and do a lookup locally
    /// Or make players networkobjects idk either could work
    /// </summary>
    [System.Serializable]
    public struct GameState
    {
        public GameplayTest.GamePhase LastGamePhase;
        public GameplayTest.GamePhase GamePhase;

        public EntityPiece CurrentPlayer;

        public List<CombatState> CurrentCombats;
        public List<PlayerState> Players; // player state

        public bool CurrentPlayerHasUsedItem;

        public int MoveDieRoll; // originally diceRoll | aka MovementTotal
        public int MovementLeft; // originally in EntityPiece
    }

    /// <summary>
    /// State of an ongoing combat instance
    /// </summary>
    public struct CombatState
    {
        // asdfasdf
    }

    public struct PlayerState
    {
        public EntityPieceThing EntityPiece;
        public Color playerColor; // idk man
    }

    public enum PlayerAnims
    {
        Idle,
        Moving,
        Rolling
    }

    public struct WildCreature
    {
        public bool isEnemy;
        public int favoredAttack;
        public float spawnRarityModifier;

        public EntityPieceThing EntityPiece;
    }

    // Change into a class instead of struct?
    public struct EntityPieceThing
    {
        //public bool isEnemy;
        //public int favoredAttack;
        //public float spawnRarityModifier;
        public string EntityName; // may be unneeded
        public int Id;

        public List<Status> Statuses; // originally 'currentStates'

        [Header("Score Stats")]
        public int Money; // originally 'heldPoints'
        public int Health;
        public int MaxHealth;
        public List<Stamp.StampType> Stamps;

        [Header("Storefront Info")]
        [Range(1, 10)] public int InventoryLimit; // Default = 8
        [Range(0, 6)] public int StoreCount; // Defaults - 2P = 8, 3P = 6, 4P = 4,
        public int StorestockTotal;

        [Header("Overworld Info")] // move to GameplayTest or GameState?
        public MapNode OccupiedNode;     // Node player is currently on
        public MapNode OccupiedNodeCopy; // Node player's initial node at the start of the turn
        public MapNode PreviousNode;     // Node player just walked on last turn. They can't go back this way.
        public List<MapNode> TraveledNodes; // Tracks the nodes the player has gone to

        [Header("Augment/Die Info")]
        [Range(1, 99)] public int Level; // Default = 1
        public float Experience; // Default = 0
        public float ExpThreshold;// Default = 100, originally 'levelThreshold'
        public int UnspentSP; // originally 'unspentLevelUpPoints'

        public EntityBaseStats entityStats;
        public DieConfig StrDie => entityStats.dieConfigs[(int)EntityBaseStats.DieTypes.Strength];
        public DieConfig DexDie => entityStats.dieConfigs[(int)EntityBaseStats.DieTypes.Dex];
        public DieConfig IntDie => entityStats.dieConfigs[(int)EntityBaseStats.DieTypes.Int];
        public DieConfig SpdDie => entityStats.dieConfigs[(int)EntityBaseStats.DieTypes.Speed];
        //public Sprite entityIcon;

        //public string entitySpecies;

        //public Color defaultColor;

        //[TextArea(3, 10)]
        //public string flavorText;
    }

    // Player statuses
    public enum Status
    {
        Alive,
        Dead,
        Fighting,
        FightingParty,
        InsideVendor,
        Invulernable,
        DeathsRow,
    }
}