using System.Collections.Generic;
using UnityEngine;

namespace Core
{


    /// <summary>
    /// Will need to be serializable over network (so no unity objects that are not network objects)
    /// </summary>
    public class GameCommand
    {
        public GameCommandType Type;
        public int SourcePlayerId;

        public GameCommand(GameCommandType type)
        {
            Type = type;

        }
    }

    public enum GameCommandType
    {
        NoCommand,

        InitialTurnMenuRollMoveDie, // change to InitialTurnMenuMove
        InitialTurnMenuInv,
        InitialTurnMenuBuild,
        InitialTurnMenuView,

        MovingMove,

        ConfirmationYes,
        ConfirmationNo,
        ConfirmationChange,

        InventoryExit,
    }

    public interface ICommand
    {
        //public CommandType CommandType { get; }
    }

    public struct ITMCommand : ICommand
    {
        public enum ITMAction
        {
            Roll,
            View,
            Inv,
            Build
        }

        public ITMAction Action;

        public ITMCommand(ITMAction action)
        {
            Action = action;
        }
    }

    public struct MovingCommand : ICommand
    {
        public Vector2 Direction;

        public MovingCommand(Vector2 direction)
        {
            Direction = direction;
        }
    }

    public struct UICancelCommand : ICommand
    {
        // Basically pressing Back in a UI
    }
    public struct InventoryUseItemCommand : ICommand
    {
        public int Index; // Position in Inventory
        public ItemStats Item;

        public InventoryUseItemCommand(int index, ItemStats item)
        {
            Index = index;
            Item = item;
        }
    }

    public struct BuildStoreCommand : ICommand
    {
        public int ItemCount; // useless?

        public BuildStoreCommand(int itemCount)
        {
            ItemCount = itemCount;
        }
    }
    
    public struct RestockStoreCommand : ICommand
    {
        public Dictionary<int, ItemStats> ItemsToStock; // not useless

        public RestockStoreCommand(Dictionary<int, ItemStats> itemsToStock)
        {
            ItemsToStock = itemsToStock;
        }
    }

    public struct FreeviewExitCommand : ICommand
    {
        
    }
    
    public struct FreeviewExamineCommand : ICommand
    {
        
    }
    
    public struct ConfirmationYesCommand : ICommand
    {
        
    }
    
    public struct ConfirmationNoCommand : ICommand
    {
        
    }
    
    public struct ConfirmationChangeCommand : ICommand
    {
        // for speed die atm
    }

    public struct CombatActionCommand : ICommand
    {
        public int ActionId;
        public int EntityId;

        public CombatActionCommand(int entityId, int actionId)
        {
            ActionId = actionId;
            EntityId = entityId;
        }
    }

    public struct CombatRollCommand : ICommand
    {
        // Does this need anything specific?
        public int EntityId;

        public CombatRollCommand(int entityId)
        {
            EntityId = entityId;
        }
    }
}