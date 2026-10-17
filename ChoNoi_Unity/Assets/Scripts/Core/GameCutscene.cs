
using System;
using UnityEngine;
using UnityEngine.Playables;

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
        //public PlayableDirector PlayableDirector { get; set; }
        //public float timePassed { set; }

        public void Play(GameState prev, GameState curr);
        public bool IsPlaying();
        public double CurrentTime();
        public double TimeRemaining();
        public ICommand CommandsToRun();
    }

    public struct UseItemCutscene : ICutscene
    {
        public ItemStats Item;
        private PlayableDirector _playableDirector;

        public UseItemCutscene(ItemStats item, PlayableDirector pd)
        {
            Item = item;
            _playableDirector = pd;
            //timePassed = 0;
        }

        public void Play(GameState prev, GameState curr)
        {
            // could do some extra stuff visual stuff with this?
            //var statMods = curr.CurrentPlayer().EntityData.CurrentStatsModifier;
            _playableDirector.Play();
        }

        public bool IsPlaying()
        {
            //_playableDirector.
            return TimeRemaining() > 0.05;
        }

        public double CurrentTime()
        {
            return _playableDirector.time;
        }

        public double TimeRemaining()
        {
            Debug.Log($"UseItemCutscene | TimeRemaining: {_playableDirector.playableAsset.duration - _playableDirector.time}s");
            return _playableDirector.playableAsset.duration - _playableDirector.time;
        }
        public ICommand CommandsToRun()
        {
            throw new System.NotImplementedException();
        }
    }
}
