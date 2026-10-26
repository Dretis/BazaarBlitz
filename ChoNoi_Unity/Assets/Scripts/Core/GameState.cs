using System;
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
        public int Round;

        public GameplayTest.GamePhase GamePhase;
        [Space]
        public GameplayTest.GamePhase LastGamePhase;
        //public GameplayTest.GamePhase ExpectedPhase;
        [Space]
        public int MoveDieRoll; // originally diceRoll | aka MovementTotal
        public int MovementLeft; // originally in EntityPiece
        [Space]

        public List<CombatState> CurrentCombats;
        public PlayerState[] Players; // player state
        public BoardNodeData[] BoardNodes; // all nodes on the current board
        //public List<PlayerState> NextPlayers; // Tracks remaining players that havent gotten a turn

        public EntityPiece CurrentPlayerObj; // will have to remove this later
        //public PlayerState CurrentPlayer;
        [Header("Current Turn Logic Variables")]
        public int CurrPlayerId;
        public List<int> TraveledNodeIds; // Tracks the node ids the current player has gone to
        [Space]
        public bool CurrentPlayerHasUsedItem;
        public bool UsingSpeedDieToMove;

        [Header("Temp Tracking Variables")]
        //public List<EntityData> CurrentPlayerDataStates; // for Undo'ing a move 
        public EntityData[] CurrentPlayerDataStates; // for Undo'ing a move 
        public int WantedNodeId; // node ids the current player wants to go to

        public ItemStats UsedItem;
        public int SelectedBoardNodeId;

        public ref PlayerState CurrentPlayer()
        {
            return ref (Players[CurrPlayerId]);
        }

        public GameState Copy()
        {
            GameState copy = this;

            //copy.BoardNodes = new BoardNodeData[BoardNodes.Length];
            //for (int i = 0; i < BoardNodes.Length; i++)
            //{
            //    copy.BoardNodes[i] = this.BoardNodes[i];
            //}

            copy.Players = new PlayerState[Players.Length];

            for(int i = 0; i < Players.Length; i++)
            {
                copy.Players[i] = new PlayerState(this.Players[i]);
            }

            return copy;
        }
        /// <summary>
        /// Adds the current player's EntityData into an array.
        /// Used when undo'ing a Move action to revert the player's data to the previous state.  
        /// </summary>
        public void SnapshotCurrentPlayerData()
        {
            //Debug.Log("SNAP!");
            Array.Resize(ref CurrentPlayerDataStates, CurrentPlayerDataStates.Length + 1);
            CurrentPlayerDataStates[^1] = CurrentPlayer().EntityData;
            //var currentPlayerDataCopy = new EntityData(CurrentPlayer().EntityData);
            //CurrentPlayerDataStates.Add(currentPlayerDataCopy);
        }

        public void RevertCurrentPlayerData()
        {
            //Debug.Log("REVERT!");
            Array.Resize(ref CurrentPlayerDataStates, CurrentPlayerDataStates.Length - 1);
            CurrentPlayer().EntityData = CurrentPlayerDataStates[^1];
            //CurrentPlayerDataStates.RemoveAt(CurrentPlayerDataStates.Count - 1);
        }

        public void Add<T>(ref T[] array, T thing)
        {
            Array.Resize(ref array, array.Length + 1);
            array[^1] = thing;
        }
    }

    /// <summary>
    /// State of an ongoing combat instance
    /// </summary>
    public struct CombatState
    {
        // asdfasdf
    }

    public interface IEntityState
    {
        EntityData EntityData { get; set; }
    }

    [System.Serializable]
    public struct PlayerState // : IEntityState
    {
        public int Id;
        public string PlayerName;
        public EntityData EntityData; // { get; set; }
        public PlayerState(PlayerState ps)
        {
            Id = ps.Id;
            PlayerName = ps.PlayerName;
            EntityData = new EntityData(ps.EntityData);
        }

        public PlayerState(int id, PlayerColorPalettePreset palettePreset)
        {
            Id = id;
            PlayerName = palettePreset.presetName;
            EntityData = new EntityData(id, palettePreset);
        }

        public PlayerState(int id, PlayerCosmeticManager pcm)
        {
            Id = id;
            PlayerName = pcm.playerName;
            EntityData = new EntityData(id, pcm);
        }

        public void SetEntityData(EntityData entityData)
        {
            EntityData = entityData;
        }
    }

    [System.Serializable]
    public struct EnemyState : IEntityState
    {
        // May need a different Name indicator bc its name should be based on the local language
        // Name is different between clients!!!
        //public bool isEnemy;
        public int favoredAttack;
        public float spawnRarityModifier;
        
        public EntityData EntityData { get; set; }
    }

    public enum PlayerAnims
    {
        Idle,
        Moving,
        Rolling
    }

    // Change into a class instead of struct?
    [System.Serializable]
    public struct EntityData
    {
        public string EntityName; // may be unneeded
        //public int Id;
        public EntityType EntityType;
        public bool IsEnemy; // remove this later

        public Color MainColor; // idk man

        [Header("Score Stats")]
        public int Money; // originally 'heldPoints'
        public int Health;
        public int BaseMaxHealth;
        public int MaxHealth;
        public StampType[] Stamps;
        public List<ItemStats> Inventory;

        [Header("Overworld Info")] // REMOVE THESE
        public MapNode OccupiedNode;     // Node player is currently on
        //public MapNode InitialNode; // Node player's initial node at the start of the turn
        //public MapNode PreviousNode;     // Node player just walked on last turn. They can't go back this way.
        //public List<MapNode> TraveledNodes; // Tracks the nodes the player has gone to

        [Header("Overworld Info 2")] // NEW
        public int OccupiedNodeId;     // Node player is currently on
        public int InitialNodeId; // Node player's initial node at the start of the turn
        public int PreviousNodeId;     // Node player just walked on last turn. They can't go back this way.
        //public List<int> TraveledNodeIds; // Tracks the nodes the player has gone to

        [Header("Storefront Info")]
        [Range(1, 10)] public int InventoryLimit; // Default = 8
        [Range(0, 6)] public int StoreCount; // Defaults - 2P = 8, 3P = 6, 4P = 4,
        public int StorestockTotal;
        //public List<StoreManager> OwnedStores;
        public int[] OwnedStrorefrontIds;

        [Header("Augment/Die Info")]
        [Range(1, 99)] public int Level; // Default = 1
        public int Experience; // Default = 0
        public int ExpThreshold;// Default = 100, originally 'levelThreshold'
        public int UnspentSP; // originally 'unspentLevelUpPoints'

        public EntityBaseStats EntityStats;
        public EntityStatsModifiers CurrentStatsModifier;
        public List<ActiveEffect> ActiveEffects; // Item Buffs/Debuffs
        public List<Status> Statuses; // originally 'currentStates'

        [Header("Palettes")]
        public List<Color> EntityPalette;
        public List<Color> BoatPalette;

        public DieConfig StrDie => EntityStats.dieConfigs[(int)EntityBaseStats.DieTypes.Strength];
        public DieConfig DexDie => EntityStats.dieConfigs[(int)EntityBaseStats.DieTypes.Dex];
        public DieConfig IntDie => EntityStats.dieConfigs[(int)EntityBaseStats.DieTypes.Int];
        public DieConfig SpdDie => EntityStats.dieConfigs[(int)EntityBaseStats.DieTypes.Speed];
        //public Sprite entityIcon;

        //public string entitySpecies;

        //public Color defaultColor;

        //[TextArea(3, 10)]
        //public string flavorText;
        public EntityData(EntityData ed)
        {
            EntityName = ed.EntityName;

            MainColor = ed.MainColor;
            EntityPalette = ed.EntityPalette;
            BoatPalette = ed.BoatPalette;

            //

            Statuses = ed.Statuses;
            ActiveEffects = ed.ActiveEffects;
            EntityType = ed.EntityType;
            IsEnemy = ed.IsEnemy;

            Money = ed.Money;
            Health = ed.Health;
            MaxHealth = ed.MaxHealth;
            BaseMaxHealth = MaxHealth;

            Stamps = ed.Stamps;
            Inventory = new List<ItemStats>(ed.Inventory);

            OccupiedNode = ed.OccupiedNode;

            OccupiedNodeId = ed.OccupiedNodeId;
            InitialNodeId = ed.InitialNodeId;
            PreviousNodeId = ed.PreviousNodeId;

            InventoryLimit = ed.InventoryLimit;
            StoreCount = ed.StoreCount;
            StorestockTotal = ed.StorestockTotal;
            //OwnedStores = ed.OwnedStores;
            OwnedStrorefrontIds = ed.OwnedStrorefrontIds;

            Level = ed.Level;
            Experience = ed.Experience;
            ExpThreshold = ed.ExpThreshold;
            UnspentSP = ed.UnspentSP;

            EntityStats = ed.EntityStats;
            CurrentStatsModifier = ed.CurrentStatsModifier;
        }

        public EntityData(int id, PlayerColorPalettePreset palettePreset)
        {
            //Id = id;
            EntityName = palettePreset.presetName;

            MainColor = palettePreset.mainColor;
            EntityPalette = palettePreset.baggieColorPalette;
            BoatPalette = palettePreset.boatColorPalette;
            
            //

            Statuses = new();
            ActiveEffects = new();
            EntityType = EntityType.Player;
            IsEnemy = false;

            Money = 1000;
            Health = 75;
            MaxHealth = 75;
            BaseMaxHealth = MaxHealth;

            Stamps = new StampType[4];
            Inventory = new();

            OccupiedNode = null;

            OccupiedNodeId = -1;
            InitialNodeId = -1;
            PreviousNodeId = -1;

            InventoryLimit = 8;
            StoreCount = 0;
            StorestockTotal = 0;
            //OwnedStores = new();
            OwnedStrorefrontIds = new int[0];

            Level = 1;
            Experience = 0;
            ExpThreshold = 100;
            UnspentSP = 0;

            EntityStats = new();
            CurrentStatsModifier = new();
            //SetDefaultData();
        }
        public EntityData(int id, PlayerCosmeticManager pcm)
        {
            //Id = id;
            EntityName = pcm.playerName;

            MainColor = pcm.playerColor;
            EntityPalette = pcm.baggieColorPalette;
            BoatPalette = pcm.boatColorPalette;

            //

            Statuses = new();
            ActiveEffects = new();
            EntityType = EntityType.Player;
            IsEnemy = false;

            Money = 1000;
            Health = 75;
            MaxHealth = 75;
            BaseMaxHealth = MaxHealth;

            Stamps = new StampType[4];
            Inventory = new();

            OccupiedNode = null;

            OccupiedNodeId = -1;
            InitialNodeId = -1;
            PreviousNodeId = -1;

            InventoryLimit = 8;
            StoreCount = 0;
            StorestockTotal = 0;
            //OwnedStores = new();
            OwnedStrorefrontIds = new int[0];

            Level = 1;
            Experience = 0;
            ExpThreshold = 100;
            UnspentSP = 0;

            EntityStats = new();
            CurrentStatsModifier = new();
            //SetDefaultData();
        }

        private void SetDefaultData()
        {
            Statuses = new();
            ActiveEffects = new();

            Money = 1000;
            Health = 75;
            MaxHealth = 75;
            BaseMaxHealth = MaxHealth;

            Stamps = new StampType[4];
            Inventory = new();

            OccupiedNode = null;
            InventoryLimit = 8;
            StoreCount = 0;
            StorestockTotal = 0;

            Level = 1;
            Experience = 0;
            ExpThreshold = 100;
            UnspentSP = 0;

            EntityStats = new();
            CurrentStatsModifier = new();
        }

        //public void Add<T>(ref T[] array, T thing)
        //{
        //    Array.Resize(ref array, array.Length + 1);
        //    array[^1] = thing;
        //}
        public void AddStamp(StampType newStamp)
        {
            //var index = Array.FindIndex(Stamps, s => s == StampType.None);
            //StampType[] newStamps = (StampType[])Stamps.Clone();

            var newStamps = new StampType[Stamps.Length];
            //Array.Copy(Stamps, newStamps, Stamps.Length);
            for (int i = 0; i < Stamps.Length; i++)
            {
                newStamps[i] = Stamps[i];
            }

            var index = (int) newStamp - 1;
            newStamps[index] = newStamp;

            Stamps = newStamps;
        }

        public int GetStampCount()
        {
            var count = 0;

            for (int i = 0; i < Stamps.Length; i++)
            {
                if(Stamps[i] is not StampType.None) count++;
            }

            return count;
        }

        public bool CanLevelUp()
        {
            ExpThreshold = LevelUpFormula();
            
            if (Experience >= ExpThreshold)
            {
                Debug.Log("Passed threshold of " + ExpThreshold);
                //levelThreshold = (RenownLevel * 100) * (Mathf.Pow(1.15f, RenownLevel - 1)); // update the new threshold again
                return true; // Allows the level up screen when ready on the player's turn.
            }
            else
            {
                return false;
            }
        }

        public int LevelUpFormula()
        {
            // 100, 230, 396, 608, 874... Every level costs around 30% more (should be tuned in testing).
            return (int)((Level * 100) * (Mathf.Pow(1.15f, Level - 1)));
        }
        #region 'Stat Mod' Function
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
            CurrentStatsModifier = new EntityStatsModifiers();

            foreach (var item in ActiveEffects)
            {
                CurrentStatsModifier = item.OriginalItem.ApplyStatModChanges(CurrentStatsModifier, item.OriginalItem.Duration - item.TurnsRemaining);
            }

            //m_RefreshedActiveEffects.RaiseEvent(this);
        }
        public void TickDownActiveEffects()
        {
            for (int i = 0; i < ActiveEffects.Count; i++)
            {
                ActiveEffects[i].TurnsRemaining--;

                if ((ActiveEffects[i].TurnsRemaining < 1) && !ActiveEffects[i].IsPermanent)
                {
                    ActiveEffects.RemoveAt(i);
                    i--;
                }
            }
        }
        public void TickDownActiveEffect(ActiveEffect effect)
        {
            var effectIndex = ActiveEffects.FindIndex(activeEffect => activeEffect.OriginalItem == effect.OriginalItem);
            if (effectIndex != -1)
            {
                ActiveEffects[effectIndex].TurnsRemaining--;

                if (ActiveEffects[effectIndex].TurnsRemaining < 1 && !ActiveEffects[effectIndex].IsPermanent)
                {
                    ActiveEffects.RemoveAt(effectIndex);
                }
            }
            else
            {
                Debug.Log("Effect not found!");
            }
        }
        public void ApplyItemEffectsOnTurnStart()
        {
            // Regenerate health from active effects.
            MaxHealth = BaseMaxHealth * CurrentStatsModifier.maxHealthMultModifier + CurrentStatsModifier.maxHealthFlatModifier;
            Health = Mathf.Min(MaxHealth, Health + CurrentStatsModifier.healthRegen);

            if (Health < 1)
            {
                Health = 1;
            }

            //m_UpdatePlayerScore.RaiseEvent(p.id);

            // Warp player to specified destination.
            if (CurrentStatsModifier.warpDestination != null)
            {
                //WarpToMapNode(p, p.currentStatsModifier.warpDestination);
                //expectedPhase = GamePhase.InitialTurnMenu;
                //m_WarpByItem.RaiseEvent(p.CurrentStatsModifier.warpDestination);
            }
        }

        public void RemoveItemEffectOnUse(HashSet<string> itemNames)
        {
            List<ActiveEffect> effectsToRemove = new List<ActiveEffect>();

            foreach (var effect in ActiveEffects)
            {
                if (itemNames.Contains(effect.OriginalItem.itemName))
                {
                    Debug.Log(effect.OriginalItem.name + "'s effect is removed!");
                    effectsToRemove.Add(effect);
                }
            }

            ActiveEffects.RemoveAll(effect => effectsToRemove.Contains(effect));
            RefreshStatModifiers();
        }

        /// <summary>
        /// Add a specified item to this player's list of active stat modifier effects
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="item"></param>
        public void AddItemToActiveEffects(int duration, ItemStats item)
        {

            var sameEffect = ActiveEffects.Find(activeEffect => activeEffect.OriginalItem == item);

            // Refresh effect if same item has been used before. Otherwise, add new effect.
            if (sameEffect != null && !item.CanStack)
            {
                sameEffect.TurnsRemaining = duration;
            }
            else
            {
                ActiveEffects.Add(new ActiveEffect
                {
                    OriginalItem = item,
                    TurnsRemaining = duration,
                    IsPermanent = item.IsPermanent,
                    ClearAfterCombat = item.ClearAfterCombat,
                });
            }
        }
        #endregion 'Stat Mod' Region
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

    public enum EntityType
    {
        Player,
        CPU,
        Enemy
    }

    [System.Serializable]
    public class ActiveEffect
    {
        public ItemStats OriginalItem;
        public int TurnsRemaining;
        public bool IsPermanent;
        public bool ClearAfterCombat;
    }
}