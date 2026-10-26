
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
        PlayableDirector PlayableDirector { get; set; }
        public virtual void Play(GameState prev, GameState curr)
        {
            // could do some extra stuff visual stuff with this?
            PlayableDirector.Play();
        }
        public virtual bool IsPlaying()
        {
            return TimeRemaining() > 0.05;
        }

        public virtual double CurrentTime()
        {
            return PlayableDirector.time;
        }

        public virtual double TimeRemaining()
        {
            //Debug.Log($"Cutscene | TimeRemaining: {_playableDirector.playableAsset.duration - _playableDirector.time}s");
            return PlayableDirector.playableAsset.duration - PlayableDirector.time;
        }

        public ICommand CommandsToRun();
    }

    public struct UseItemCutscene : ICutscene
    {
        public ItemStats Item;
        public PlayableDirector PlayableDirector { get; set; }

        public UseItemCutscene(ItemStats item, PlayableDirector pd)
        {
            Item = item;
            PlayableDirector = pd;
        }

        public ICommand CommandsToRun()
        {
            throw new System.NotImplementedException();
        }
    }
    
    public struct BuildStoreCutscene : ICutscene
    {
        public PlayableDirector PlayableDirector { get; set; }

        public BuildStoreCutscene(PlayableDirector pd)
        {
            PlayableDirector = pd;
        }

        public ICommand CommandsToRun()
        {
            throw new System.NotImplementedException();
        }
    }
}
