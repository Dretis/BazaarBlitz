namespace Core
{
    /// <summary>
    /// Ideally will be serializable over network (so no unity objects that are not network objects)
    /// For things like players you can just keep the index and do a lookup locally
    /// Or make players networkobjects idk either could work
    /// </summary>
    public struct GameState
    {
        public GameplayTest.GamePhase GamePhase;
        public EntityPiece CurrentPlayer;
    }
}