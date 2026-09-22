namespace Core
{
    /// <summary>
    /// Will need to be serializable over network (so no unity objects that are not network objects)
    /// </summary>
    public class GameCommand
    {
        public GameCommandType Type;

        public GameCommand(GameCommandType type)
        {
            Type = type;
        }
    }

    public enum GameCommandType
    {
        InitialTurnMenuRollMoveDie
    }
}