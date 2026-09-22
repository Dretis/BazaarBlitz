namespace Core.Events
{
    using UnityEngine;

    public struct FrameGameState
    {
        public GameState PrevState;
        public GameState NewState;
        
        
        public bool DidChangePhases()
        {
            return PrevState.GamePhase != NewState.GamePhase;
        }
    }

    [CreateAssetMenu(fileName = "GameStateEventChannel", menuName = "Events/GameState EventChannelSO")]
    public class GameStateEventChannelSO : GenericEventChannelSO<FrameGameState>
    {

    }

}