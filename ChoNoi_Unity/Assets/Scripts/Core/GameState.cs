using System.Collections.Generic;
using UnityEngine;
using static EntityPiece;

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

        public GameplayTest.GamePhase LastGamePhase;
        public GameplayTest.GamePhase GamePhase;
        [Space]
        public int MoveDieRoll; // originally diceRoll | aka MovementTotal
        public int MovementLeft; // originally in EntityPiece
        [Space]

        public List<CombatState> CurrentCombats;
        public PlayerState[] Players; // player state
        public List<PlayerState> NextPlayers; // Tracks remaining players that havent gotten a turn

        public EntityPiece CurrentPlayerObj;
        //public PlayerState CurrentPlayer;
        public int CurrPlayerId;
        public bool CurrentPlayerHasUsedItem;
        public bool UsingSpeedDieToMove;

        public ref PlayerState CurrentPlayer()
        {
            return ref (Players[CurrPlayerId]);
        }
    }

    /// <summary>
    /// State of an ongoing combat instance
    /// </summary>
    public struct CombatState
    {
        // asdfasdf
    }

    [System.Serializable]
    public struct PlayerState
    {
        public int Id;
        public EntityData EntityData;

        public PlayerState(int id, PlayerColorPalettePreset palettePreset)
        {
            Id = id;
            EntityData = new EntityData(id, palettePreset);

            //MainColor = palettePreset.mainColor;
            //EntityPalette = palettePreset.baggieColorPalette;
            //BoatPalette = palettePreset.boatColorPalette;
        }

        public PlayerState(int id, PlayerCosmeticManager pcm)
        {
            Id = id;
            EntityData = new EntityData(id, pcm);

            //MainColor = pcm.playerColor;
            //EntityPalette = pcm.baggieColorPalette;
            //BoatPalette = pcm.boatColorPalette;
        }

        public void SetEntityData(EntityData entityData)
        {
            EntityData = entityData;
        }
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

        public EntityData EntityData;
    }

    // Change into a class instead of struct?
    [System.Serializable]
    public class EntityData
    {
        public string EntityName; // may be unneeded
        //public int Id;

        public Color MainColor; // idk man
        public List<Color> EntityPalette;
        public List<Color> BoatPalette;

        public List<Status> Statuses; // originally 'currentStates'
        public List<ActiveEffect> ActiveEffects; // Item Buffs/Debuffs

        [Header("Categorization")]
        public EntityType EntityType;
        public bool IsEnemy;

        [Header("Score Stats")]
        public int Money; // originally 'heldPoints'
        public int Health;
        public int BaseMaxHealth;
        public int MaxHealth;
        public List<Stamp.StampType> Stamps;
        public List<ItemStats> Inventory;

        [Header("Overworld Info")] // move to GameplayTest or GameState?
        public MapNode OccupiedNode;     // Node player is currently on
        public MapNode InitialNode; // Node player's initial node at the start of the turn
        public MapNode PreviousNode;     // Node player just walked on last turn. They can't go back this way.
        public List<MapNode> TraveledNodes; // Tracks the nodes the player has gone to

        [Header("Storefront Info")]
        [Range(1, 10)] public int InventoryLimit; // Default = 8
        [Range(0, 6)] public int StoreCount; // Defaults - 2P = 8, 3P = 6, 4P = 4,
        public int StorestockTotal;
        public List<StoreManager> OwnedStores;

        [Header("Augment/Die Info")]
        [Range(1, 99)] public int Level; // Default = 1
        public float Experience; // Default = 0
        public float ExpThreshold;// Default = 100, originally 'levelThreshold'
        public int UnspentSP; // originally 'unspentLevelUpPoints'

        public EntityBaseStats EntityStats;
        public EntityStatsModifiers CurrentStatsModifier;

        public DieConfig StrDie => EntityStats.dieConfigs[(int)EntityBaseStats.DieTypes.Strength];
        public DieConfig DexDie => EntityStats.dieConfigs[(int)EntityBaseStats.DieTypes.Dex];
        public DieConfig IntDie => EntityStats.dieConfigs[(int)EntityBaseStats.DieTypes.Int];
        public DieConfig SpdDie => EntityStats.dieConfigs[(int)EntityBaseStats.DieTypes.Speed];
        //public Sprite entityIcon;

        //public string entitySpecies;

        //public Color defaultColor;

        //[TextArea(3, 10)]
        //public string flavorText;
        public EntityData(int id, PlayerColorPalettePreset palettePreset)
        {
            //Id = id;
            EntityName = palettePreset.presetName;

            MainColor = palettePreset.mainColor;
            EntityPalette = palettePreset.baggieColorPalette;
            BoatPalette = palettePreset.boatColorPalette;

            SetDefaultData();
        }
        public EntityData(int id, PlayerCosmeticManager pcm)
        {
            //Id = id;
            EntityName = pcm.playerName;

            MainColor = pcm.playerColor;
            EntityPalette = pcm.baggieColorPalette;
            BoatPalette = pcm.boatColorPalette;

            SetDefaultData();
        }

        private void SetDefaultData()
        {
            Statuses = new();
            ActiveEffects = new();

            Money = 1000;
            Health = 75;
            MaxHealth = 75;
            BaseMaxHealth = MaxHealth;

            Stamps = new();
            Inventory = new();

            OccupiedNode = null;
            InitialNode = null;
            PreviousNode = null;
            TraveledNodes = new();

            InventoryLimit = 8;
            StoreCount = 4;
            StorestockTotal = 0;

            Level = 1;
            Experience = 0;
            ExpThreshold = 100;
            UnspentSP = 0;

            EntityStats = new();
            CurrentStatsModifier = new();
        }

        public bool CanLevelUp()
        {
            ExpThreshold = (Level * 100) * (Mathf.Pow(1.15f, Level - 1));
            // 100, 230, 396, 608, 874... Every level costs around 30% more (should be tuned in testing).
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