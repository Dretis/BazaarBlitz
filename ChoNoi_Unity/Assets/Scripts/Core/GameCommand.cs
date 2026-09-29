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

    public struct BuildStoreCommand : ICommand
    {
        public int ItemCount; // useless?

        public BuildStoreCommand(int itemCount)
        {
            ItemCount = itemCount;
        }
    }

    public struct FreeviewExitCommand : ICommand
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
}