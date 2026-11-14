using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameplayTest;

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
        public GameplayRules GameRuleset;

        public int Round;
        public float AveragePlayerLevel;
        public GameplayTest.GamePhase GamePhase;
        [Space]
        public GameplayTest.GamePhase LastGamePhase;
        public GameplayTest.GamePhase ExpectedPhase; // goes to this phase after a cutscene
        [Space]
        public int MoveDieRoll; // originally diceRoll | aka MovementTotal
        public int MovementLeft; // originally in EntityPiece
        [Space]

        public CombatInstance CurrentCombatInstance;
        public PlayerState[] Players; // Num of players should never change
        public PlayerState[] Enemies; // Dynamically changes based off avg lvl
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

        public void UpdateAveragePlayerLevel()
        {
            float levelSum = 0.0f;
            for(int i = 0; i < Players.Length; i++)
            {
                levelSum += Players[i].EntityData.Level;
            }
            AveragePlayerLevel = levelSum / Players.Length;
        }

        public void Add<T>(ref T[] array, T thing)
        {
            Array.Resize(ref array, array.Length + 1);
            array[^1] = thing;
        }

        public ref PlayerState FindEntityById(int entityId)
        {
            //ref PlayerState ps = ref Players[0];
            if (entityId >= 0)
            {
                return ref Players[entityId];
            }
            else
            {
                var index = 0;
                for (int i = 0; i < Enemies.Length; i++)
                {
                    if (Enemies[i].Id == entityId)
                    {
                        index = i;
                        break;
                    }
                }
                return ref Enemies[index];
            }
            //return ref ps;
            //ref PlayerState ps = ref Players[0];
            //if (entityId >= 0)
            //{
            //    ps = Array.Find(Players, p => p.Id == entityId);
            //}
            //else // is an Enemy
            //{
            //    ps = Array.Find(Enemies, e => e.Id == entityId);
            //}
        }
    }

    /// <summary>
    /// State of an ongoing combat instance
    /// </summary>
    [System.Serializable]
    public struct CombatInstance
    {
        // asdfasdf
        public GameplayTest.CombatPhase CombatPhase;
        public GameplayTest.CombatPhase ExpectedPhase;
        //public FighterState[] Fighters;
        public TeamSide[] Teams;
        //public TeamSide LeftSide;
        //public TeamSide RightSide;
        public List<CombatSequenceData> CombatSequenceDatas;

        public int NumberOfPlayers;
        public int NumberOfPlayersActed;
        public int NumberOfEnemies;
        public int PhaseSwapCount;
        //public int DamageDealt; // is this needed?
        //public int WinnerId;
        //public int LoserId;
        public CombatInstance(List<int> leftIds, List<int> rightIds)
        {
            CombatPhase = CombatPhase.None;
            ExpectedPhase = CombatPhase.None;

            NumberOfPlayers = 0;
            NumberOfPlayersActed = 0;
            NumberOfEnemies = 0;
            PhaseSwapCount = 0;

            Teams = new TeamSide[2]; // should always be 2 (Left/Right)

            CombatSequenceDatas = new List<CombatSequenceData>();

            SetupTeamFighters(FighterSide.Left, leftIds);
            SetupTeamFighters(FighterSide.Right, rightIds);
        }
        
        public CombatInstance(List<PlayerState> leftEntities, List<PlayerState> rightEntities)
        {
            CombatPhase = CombatPhase.None;
            ExpectedPhase = CombatPhase.None;

            NumberOfPlayers = 0;
            NumberOfPlayersActed = 0;
            NumberOfEnemies = 0;
            PhaseSwapCount = 0;

            Teams = new TeamSide[2]; // should always be 2 (Left/Right)

            CombatSequenceDatas = new List<CombatSequenceData>();

            SetupTeamFighters(FighterSide.Left, leftEntities);
            SetupTeamFighters(FighterSide.Right, rightEntities);
        }

        private void SetupTeamFighters(GameplayTest.FighterSide side, List<int> ids)
        {
            var s = (int)side;
            Teams[s].FighterSide = side;
            Teams[s].ActionPhase = (Action.ActionPhase) s;

            Teams[s].Fighters = new FighterState[ids.Count];
            for (int i = 0; i < ids.Count; i++)
            {
                Teams[s].Fighters[i] = new FighterState(ids[i]);
                if (ids[i] >= 0) NumberOfPlayers++;
                else NumberOfEnemies++;
            }
        }
        
        private void SetupTeamFighters(GameplayTest.FighterSide side, List<PlayerState> entities)
        {
            var s = (int)side;
            Teams[s].FighterSide = side;
            Teams[s].ActionPhase = (Action.ActionPhase) s;

            Teams[s].Fighters = new FighterState[entities.Count];
            for (int i = 0; i < entities.Count; i++)
            {
                Teams[s].Fighters[i] = new FighterState(entities[i]);
                if (entities[i].Id >= 0) NumberOfPlayers++;
                else NumberOfEnemies++;
            }
        }

        public void ResetCombatInstance()
        {
            CombatPhase = GameplayTest.CombatPhase.None;
            //Fighters = new FighterState[0];

            NumberOfPlayers = 0;
            NumberOfEnemies = 0;
            PhaseSwapCount = 0;
        }

        //public FighterState[] FindAllFightersByPhase(Action.ActionPhase phase)
        //{
        //    return Array.FindAll(Fighters, f => f.ActionPhase == phase);
        //}
        public TeamSide FindTeamByPhase(Action.ActionPhase phase)
        {
            return Array.Find(Teams, f => f.ActionPhase == phase);
        }

        [System.Serializable]
        public struct TeamSide
        {
            public GameplayTest.FighterSide FighterSide; // shouldnt change in a CombatInstance
            public Action.ActionPhase ActionPhase;
            public FighterState[] Fighters;

            public void SwapActionPhase()
            {
                if (ActionPhase is Action.ActionPhase.Attack)
                    ActionPhase = Action.ActionPhase.Defend;
                else if (ActionPhase is Action.ActionPhase.Defend)
                    ActionPhase = Action.ActionPhase.Attack;
            }
        }

        /// <summary>
        /// Container that holds data meant for value changes (DMG and Heals) and Combat Cutscenes/Sequences 
        /// </summary>
        [System.Serializable]
        public struct CombatSequenceData
        {
            public int AttackerId;
            public int DefenderId; // may change to multiple
            [Space]
            public TypeAdvantage TypeAdvantage;
            public float DmgTypeMultiplier;
            //public int BaseAttackRoll;
            //public int BaseDefendRoll;
            public int FinalDamageDealt;
            public bool IsDamageLethal; // the atk kills the defender

            [Header("Reward Info")]
            public int MoneyGain;
            public int ExpGain;
            public ItemStats[] ItemDrops;

            public void CalculateDamage(FighterState attacker, FighterState defender)
            {
                float totalAttackRoll = 0;
                float totalDefendRoll = 0;

                for (int i = 0; i < attacker.CombatDieRolls.Length; i++)
                {
                    totalAttackRoll += attacker.CombatDieRolls[i].FinalDieRoll;
                }

                for (int i = 0; i < defender.CombatDieRolls.Length; i++)
                {
                    totalDefendRoll += defender.CombatDieRolls[i].FinalDieRoll;
                }

                var dmgCalculation = (int)(((totalAttackRoll * 10.0f) * (1.0f - (0.1f * totalDefendRoll))) * DmgTypeMultiplier);

                Debug.Log($"<color=white>Final DMG = {dmgCalculation}</color>" +
                    $" | <color=orange>[Base DMG: {totalAttackRoll} x 10]</color> x" +
                    $" <color=cyan>[DEF Value: {(1 - (0.1f * totalDefendRoll)) * 100}%]</color> " +
                    //$" (1 - (0.1f x <color=cyan>[DEF Roll: {totalDefendRoll}]</color>)) " +
                    $" x <color=yellow>[Type Mult: {DmgTypeMultiplier}]</color>");
                FinalDamageDealt = dmgCalculation;
            }

            public void DetermineTypeAdvantage(Action.WeaponType attack, Action.WeaponType defend)
            {
                if (attack == defend)
                {
                    TypeAdvantage = TypeAdvantage.Neutral;
                }
                else if ((attack == Action.WeaponType.Melee && defend == Action.WeaponType.Gun)
                       || (attack == Action.WeaponType.Gun && defend == Action.WeaponType.Magic)
                       || (attack == Action.WeaponType.Magic && defend == Action.WeaponType.Melee))
                {
                    TypeAdvantage = TypeAdvantage.Resist;
                }
                else
                {
                    TypeAdvantage = TypeAdvantage.Strong;
                }
            }

            public void DetermineTypeAdvantageMultiplier(EntityData attackerData, EntityData defenderData)
            {
                DmgTypeMultiplier = 1f; // Attacker deals this much times more damage
                switch (TypeAdvantage)
                {
                    case TypeAdvantage.Strong:
                        DmgTypeMultiplier = 2f;
                        DmgTypeMultiplier += attackerData.CurrentStatsModifier.strongMultModifier
                                     - defenderData.CurrentStatsModifier.resistMultModifier;
                        break;
                    case TypeAdvantage.Resist:
                        DmgTypeMultiplier = 1f;
                        DmgTypeMultiplier += attackerData.CurrentStatsModifier.resistMultModifier
                                     - defenderData.CurrentStatsModifier.strongMultModifier;
                        break;
                    default: // Neutral
                        DmgTypeMultiplier = 1.5f;
                        DmgTypeMultiplier += attackerData.CurrentStatsModifier.neutralMultModifier
                                     - defenderData.CurrentStatsModifier.neutralMultModifier;
                        break;
                }
            }
        }


        public enum TypeAdvantage
        {
            Resist,
            Neutral,
            Strong
        }

        //public float DetermineTypeAdvantageMultiplier(TypeAdvantage dmgTypeAdv, EntityData attacker, EntityData defender)
        //{
        //    float dmgTypeMult = 1f; // Attacker deals this much times more damage
        //    switch (dmgTypeAdv)
        //    {
        //        case TypeAdvantage.Strong:
        //            dmgTypeMult = 2f;
        //            dmgTypeMult += attacker.CurrentStatsModifier.strongMultModifier 
        //                         - defender.CurrentStatsModifier.resistMultModifier;
        //            break;
        //        case TypeAdvantage.Resist:
        //            dmgTypeMult = 1f;
        //            dmgTypeMult += attacker.CurrentStatsModifier.resistMultModifier
        //                         - defender.CurrentStatsModifier.strongMultModifier;
        //            break;
        //        default: // Neutral
        //            dmgTypeMult = 1.5f;
        //            dmgTypeMult += attacker.CurrentStatsModifier.neutralMultModifier 
        //                         - defender.CurrentStatsModifier.neutralMultModifier;
        //            break;
        //    }

        //    return dmgTypeMult;
        //}
    }

    [System.Serializable]
    public struct FighterState
    {
        [SerializeField] private string _name;
        public int EntityId;
        //public GameplayTest.FighterSide FighterSide; // shouldnt change in a CombatInstance
        public GameplayTest.FighterPhase FighterPhase;
        //public Action.ActionPhase ActionPhase;

        [Header("Selected Action Info")]
        public int SelectedActionId; // 1 = Left, 2 = Up, 3 = Right
        public int WeaponActionId; // for the animator to know what to play
        //public EntityBaseStats.DieTypes ActionType;
        public Action.WeaponType DieType;
        [Space]
        //[Range(0, 5)] public int[] DieFaceNumber; // [0] to [5]
        //public int[] BaseDieRoll; // Base number of that die face
        //public int[] FinalDieRoll; // Final die number value after all modifiers are applied
        public CombatDieRoll[] CombatDieRolls;

        public FighterState(int id)
        {
            _name = $"ID: {id}";
            EntityId = id;
            //FighterSide = side;
            FighterPhase = FighterPhase.ActionSelect;
            //ActionPhase = actionPhase;

            SelectedActionId = -1;
            WeaponActionId = -1;
            DieType = Action.WeaponType.None;
            //DieFaceNumber = new int[1];
            //BaseDieRoll = new int[1];
            //FinalDieRoll = new int[1];

            CombatDieRolls = new CombatDieRoll[1];
        }
        
        public FighterState(PlayerState ps)
        {
            _name = $"{ps.PlayerName}";
            EntityId = ps.Id;
            FighterPhase = FighterPhase.ActionSelect;

            SelectedActionId = -1;
            WeaponActionId = -1;
            DieType = Action.WeaponType.None;

            CombatDieRolls = new CombatDieRoll[1];
        }

        public void ResetSelectedAction()
        {
            SelectedActionId = -1;
            WeaponActionId = -1;
            DieType = Action.WeaponType.None;

            //DieFaceNumber = new int[1];
            //BaseDieRoll = new int[1];
            //FinalDieRoll = new int[1];

            CombatDieRolls = new CombatDieRoll[1];
        }

        [System.Serializable]
        public struct CombatDieRoll
        {
            [Range(0, 5)] public int DieFaceNumber; // [0] to [5]
            public int BaseDieRoll; // Base number of that die face
            public int FinalDieRoll; // Final die number value after all modifiers are applied
        }
    }


    public interface IEntityState
    {
        EntityData EntityData { get; set; }
    }

    [System.Serializable]
    public struct PlayerState // : IEntityState
    {
        public string PlayerName;
        public int Id;  
        public EntityData EntityData; // { get; set; }
        public CosmeticData CosmeticData; // { get; set; }
        public EnemyData EnemyData; // { get; set; 
        public PlayerState(PlayerState ps)
        {
            Id = ps.Id;
            PlayerName = ps.PlayerName;
            EntityData = new EntityData(ps.EntityData);
            CosmeticData = new CosmeticData(ps.CosmeticData);

            if(ps.EntityData.EntityType is EntityType.Player)
            {
                EnemyData = new EnemyData(ps.EnemyData);
            }
            else
                EnemyData = new EnemyData(ps.EnemyData);
        }
        /*
        public PlayerState(int id, PlayerColorPalettePreset palettePreset)
        {
            Id = id;
            PlayerName = palettePreset.presetName;
            EntityData = new EntityData(id, palettePreset);
            EnemyData = new();
        }
        */
        public PlayerState(int id, PlayerCosmeticManager pcm)
        {
            Id = id;
            PlayerName = pcm.playerName;
            EntityData = new EntityData(id, pcm);
            CosmeticData = new CosmeticData(pcm);

            EnemyData = new();
            EnemyData.ActionWeights = new int[3];
            EnemyData.LootPool = new();
        }
    }

    [System.Serializable]
    public struct CosmeticData
    {
        public RuntimeAnimatorController CombatAnimatorController;
        public List<Action> AttackActions;
        public List<Action> DefendActions;

        public List<Color> EntityPalette;
        public List<Color> BoatPalette;

        public CosmeticData(CosmeticData cd)
        {
            CombatAnimatorController = cd.CombatAnimatorController;
            AttackActions = new List<Action>(cd.AttackActions);
            DefendActions = new List<Action>(cd.DefendActions);

            EntityPalette = new List<Color>(cd.EntityPalette);
            BoatPalette = new List<Color>(cd.BoatPalette);
        }

        public CosmeticData(PlayerCosmeticManager pcm)
        {
            CombatAnimatorController = pcm.CombatAnimatorController;
            AttackActions = new List<Action>(pcm.AttackActions);
            DefendActions = new List<Action>(pcm.DefendActions);

            EntityPalette = new List<Color>(pcm.baggieColorPalette);
            BoatPalette = new List<Color>(pcm.boatColorPalette);
        }
    }

    /// <summary>
    /// Holds information on Enemy Entities specifically. Should be unused for Players.
    /// </summary>
    [System.Serializable]
    public struct EnemyData
    {
        public int SpawnWeight; // how much more likely this Enemy will appear compared to the rest 

        public CombatBehavior Behavior;
        public int[] ActionWeights; // Index 0 = Melee, 1 = Gun, 2 = Magic

        public List<ItemDrop> LootPool;

        public EnemyData(EnemyData wd)
        {
            SpawnWeight = wd.SpawnWeight;

            Behavior = wd.Behavior;
            ActionWeights = new int[3];
            wd.ActionWeights.CopyTo(ActionWeights, 0);
            //ActionWeights = new List<int>(wd.ActionWeights);
            LootPool = new List<ItemDrop>(wd.LootPool);
        }

        public enum CombatBehavior
        {
            Simple,
            Learner,
            Smart,
            Reactive,
            Cheat
        }

        public int DecideActionByBehavior(System.Random rng)//FighterState fs)
        {
            // fs is the opponent it is against
            var actionId = 0;
            switch (Behavior)
            {
                case CombatBehavior.Simple:
                    actionId = SimpleBehavior(rng);
                    break;
                case CombatBehavior.Learner:

                    break;
                case CombatBehavior.Smart:

                    break;
                case CombatBehavior.Reactive:

                    break;
                case CombatBehavior.Cheat:

                    break;
            }

            return actionId;
        }

        private int SimpleBehavior(System.Random rng)
        {
            var weightTotal = 0;
            foreach (var value in ActionWeights)
            {
                weightTotal += value;
            }

            var rngNumber = rng.Next(0, weightTotal);
            Debug.Log($"SimpleBehavior - {(Action.ActionPhase)rngNumber} | RNG Number = {rngNumber} | WeightTotal = {weightTotal}");

            //int selectedEnemyId = -1;
            for (int i = 0; i < ActionWeights.Length; i++)
            {
                if (rngNumber <= ActionWeights[i])
                {
                    return i;
                }
                else
                    rngNumber -= ActionWeights[i];
            }

            return 0; // 
        }
    }
    [System.Serializable]
    public struct ItemDrop
    {
        public ItemStats Item;
        public int Weight;
    }

    //[System.Serializable]
    //public struct EnemyState : IEntityState
    //{
    //    // May need a different Name indicator bc its name should be based on the local language
    //    // Name is different between clients!!!
    //    //public bool isEnemy;
    //    public int favoredAttack;
    //    public float spawnRarityModifier;

    //    public EntityData EntityData { get; set; }
    //}

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

        [Header("Overworld Info")] // NEW
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

        //[Header("Combat Actions")]
        //public List<Action> attackActions;
        //public List<Action> defendActions;

        //[Header("Cosmetic")]
        //public List<Color> EntityPalette;
        //public List<Color> BoatPalette;

        public DieConfig StrDie => EntityStats.dieConfigs[(int)EntityBaseStats.DieTypes.Strength];
        public DieConfig DexDie => EntityStats.dieConfigs[(int)EntityBaseStats.DieTypes.Dex];
        public DieConfig IntDie => EntityStats.dieConfigs[(int)EntityBaseStats.DieTypes.Int];
        public DieConfig SpdDie => EntityStats.dieConfigs[(int)EntityBaseStats.DieTypes.Speed];

        public EntityData(EntityData ed)
        {
            EntityName = ed.EntityName;

            MainColor = ed.MainColor;
            //EntityPalette = ed.EntityPalette;
            //BoatPalette = ed.BoatPalette;

            //

            Statuses = new List<Status>(ed.Statuses);
            ActiveEffects = new List<ActiveEffect>(ed.ActiveEffects);
            EntityType = ed.EntityType;
            IsEnemy = ed.IsEnemy;

            Money = ed.Money;
            Health = ed.Health;
            MaxHealth = ed.MaxHealth;
            BaseMaxHealth = MaxHealth;

            Stamps = ed.Stamps;
            Inventory = new List<ItemStats>(ed.Inventory);

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
        /*
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
        */
        public EntityData(int id, PlayerCosmeticManager pcm)
        {
            //Id = id;
            EntityName = pcm.playerName;

            MainColor = pcm.playerColor;
            //EntityPalette = pcm.baggieColorPalette;
            //BoatPalette = pcm.boatColorPalette;

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

        public DieConfig GetDie(Action.WeaponType type)
        {
            switch (type)
            {
                case Action.WeaponType.Melee:
                    return StrDie;
                case Action.WeaponType.Gun:
                    return DexDie;
                case Action.WeaponType.Magic:
                    return IntDie;
                case Action.WeaponType.Speed:
                    return SpdDie;
                default:
                    Debug.Log($"Cannot find Die Type {type}, defaulting to StrDie");
                    return StrDie;
            }
        }
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

    }

    [System.Serializable]
    public class ActiveEffect
    {
        public ItemStats OriginalItem;
        public int TurnsRemaining;
        public bool IsPermanent;
        public bool ClearAfterCombat;
    }
    #endregion 'Stat Mod' Region

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
}