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

        #region Cutscenes
        public bool UseItemCutsceneStarted()
        {
            return NewState.GamePhase is GameplayTest.GamePhase.IncidentHappening 
                && PrevState.GamePhase is GameplayTest.GamePhase.Inventory;
        }
        
        public bool BuildStoreCutsceneStarted()
        {
            return NewState.GamePhase is GameplayTest.GamePhase.IncidentHappening 
                && PrevState.GamePhase is GameplayTest.GamePhase.BuildingStore;
        }
        #endregion Cutscenes

        public bool DidChangeStamps(int playerId)
        {
            var prevEd = PrevState.Players[playerId].EntityData;
            var newEd = NewState.Players[playerId].EntityData;

            for (int i = 0; i < newEd.Stamps.Length; i++)
            {
                if (newEd.Stamps[i] != prevEd.Stamps[i])
                {
                    return true;
                }
            }

            return false;
        }
    }

    [CreateAssetMenu(fileName = "GameStateEventChannel", menuName = "Events/GameState EventChannelSO")]
    public class GameStateEventChannelSO : GenericEventChannelSO<FrameGameState>
    {

    }

}