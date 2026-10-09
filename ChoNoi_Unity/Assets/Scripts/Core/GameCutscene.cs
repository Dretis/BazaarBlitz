
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Interface for uninteractable visual sequences
    /// </summary>
    [System.Serializable]
    public struct GameCutscene
    {

    }

    public interface ICutscene
    {
        public void Play(GameState prev, GameState curr);
        public bool IsFinished();
        public float CurrentTime();
        public float TimeRemaining();
        public ICommand CommandsToRun();
    }
}
