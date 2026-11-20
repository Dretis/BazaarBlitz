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
    #region 'UI' Commands
    public struct UICancelCommand : ICommand
    {
        // Basically pressing Back in a UI
    }
    public struct UIInventoryUseItemCommand : ICommand
    {
        public int Index; // Position in Inventory
        public ItemStats Item;

        public UIInventoryUseItemCommand(int index, ItemStats item)
        {
            Index = index;
            Item = item;
        }
    }

    public struct UIBuildStoreCommand : ICommand
    {
        public int ItemCount; // useless?

        public UIBuildStoreCommand(int itemCount)
        {
            ItemCount = itemCount;
        }
    }
    
    public struct UIRestockStoreCommand : ICommand
    {
        public Dictionary<int, ItemStats> ItemsToStock; // not useless

        public UIRestockStoreCommand(Dictionary<int, ItemStats> itemsToStock)
        {
            ItemsToStock = itemsToStock;
        }
    }
    public struct UIDiscardItemsCommand : ICommand
    {
        public Dictionary<int, ItemStats> ItemsToDiscard; // not useless

        public UIDiscardItemsCommand(Dictionary<int, ItemStats> itemsToDiscard)
        {
            ItemsToDiscard = itemsToDiscard;
        }
    }

    public struct UIConfirmRenovateCommand : ICommand
    {
        // prob blessing selected thingy here
    }

    // Level Up
    public struct UIAllocateStatDieCommand : ICommand
    {
        public Action.WeaponType DieType;
        public int FaceIndex; 
        public int DieNumber; 

        public UIAllocateStatDieCommand(Action.WeaponType dieType, int faceIndex, int dieNumber)
        {
            DieType = dieType;
            FaceIndex = faceIndex;
            DieNumber = dieNumber;
        }
    }
    public struct UIBlessingSelectedCommand : ICommand
    {
        // prob blessing selected thingy here
    }
    #endregion

    public struct FreeviewExitCommand : ICommand
    {
        // goo goo
    }
    
    public struct FreeviewExamineCommand : ICommand
    {
        // gaga
    }
    
    public struct ConfirmationYesCommand : ICommand
    {
        // ehn-na
    }
    
    public struct ConfirmationNoCommand : ICommand
    {
        // uhn-na
    }
    
    public struct ConfirmationChangeCommand : ICommand
    {
        // for speed die atm
    }

    public struct CombatActionCommand : ICommand
    {
        public int ActionId;
        public int EntityId;

        public CombatActionCommand(int actionId, int entityId)
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