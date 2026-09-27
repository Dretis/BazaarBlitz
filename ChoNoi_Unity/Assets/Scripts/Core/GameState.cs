using System.Collections.Generic;
using UnityEngine;

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
        public List<CombatState> CurrentCombats;
        public List<PlayerState> Players; // player state
    }

    /// <summary>
    /// State of an ongoing combat instance
    /// </summary>
    public struct CombatState
    {
        // asdfasdf
    }

    public struct PlayerState
    {
        public EntityPieceThing EntityPiece;
        public Color playerColor; // idk man
    }

    public enum PlayerAnims
    {
        Idle,
        Moving,
        Rolling
    }

    public struct WildCreature
    {
        public EntityPieceThing EntityPiece;
    }

    public struct EntityPieceThing
    {
        public bool isEnemy;
        public int favoredAttack;
        public float spawnRarityModifier;
        public EntityBaseStats entityStats;
        public int heldPoints;
        public int health;
        public int maxHealth;
        public Sprite entityIcon;

        public string entitySpecies;

        public Color defaultColor;

        [TextArea(3, 10)]
        public string flavorText;
    }
}