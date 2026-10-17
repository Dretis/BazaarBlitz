namespace Core.Events
{
    using UnityEngine;
    using UnityEngine.Analytics;

    public struct FrameGameState
    {
        public GameState PrevState;
        public GameState NewState;
        
        
        public bool DidChangePhases()
        {
            return PrevState.GamePhase != NewState.GamePhase;
        }

        public bool UseItemCutsceneStarted()
        {
            return NewState.GamePhase is GameplayTest.GamePhase.IncidentHappening 
                && PrevState.GamePhase is GameplayTest.GamePhase.Inventory;
        }
    }

    [CreateAssetMenu(fileName = "GameStateEventChannel", menuName = "Events/GameState EventChannelSO")]
    public class GameStateEventChannelSO : GenericEventChannelSO<FrameGameState>
    {

    }

}