using System.Collections.Generic;
using UnityEngine;

public class EntityPiece : MonoBehaviour
{
    public string entityName;
    public int id;
    public Color playerColor; // idk man
    public SpriteRenderer playerSprite; // idk man
    public MapNode occupiedNode; // Node player is currently on
    public MapNode occupiedNodeCopy; // Node player is currently on
    public List<MapNode> traveledNodes = new List<MapNode>(); // Tracks the nodes the player has gone to

    [Header("Overworld Stats")]
    public int movementTotal;
    public int movementLeft;
    public int finalPoints = 1;
    public int heldPoints = 0;
    public int storeCount = 0;
    public List<Stamp.StampType> stamps = new List<Stamp.StampType>();
    public bool isTurnSkipped = false;

    [Header("Combat Stats")]
    public int health = 75;
    public int maxHealth = 75;

    // Dice faces initialization.
    public DieConfig strDie => entityStats.dieConfigs[(int)EntityBaseStats.DieTypes.Strength];
    public DieConfig dexDie => entityStats.dieConfigs[(int)EntityBaseStats.DieTypes.Dex];
    public DieConfig intDie => entityStats.dieConfigs[(int)EntityBaseStats.DieTypes.Int];

    public int combatSceneIndex = -1; // -1 indicates player is not in battle
    public bool isEnemy;
    public int favoredAttack;

    public EntityBaseStats entityStats = new();

    public List<Action> attackActions;
    public List<Action> defendActions;
    public CombatUIManager.FightingPosition fightingPosition; // Just for the combat, will change

    public float ReputationPoints = 0; // For enemies: how much rep they give on kill. For players: they're total exp

    public int RenownLevel = 1; // Used to calculate the next level threshold.


    public class ActiveEffect
    {
        public IStatModifierChanger statMod;
        public int turnsRemaining;
    }

    public EntityStatsModifiers currentStatsModifier;

    public List<ItemStats> inventory = new();

    private List<ActiveEffect> activeEffects = new();

    /// <summary>
    /// Add a specified item to this player's list of active stat modifier effects
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="item"></param>
    public void AddItemToActiveEffects(int duration, ItemStats item)
    {
        activeEffects.Add(new ActiveEffect
        {
            statMod = item,
            turnsRemaining = duration
        });
    }

    /// <summary>
    /// Tick down active effects by one turn and recalculate this player's stat modifiers
    /// </summary>
    public void UpdateStatModifiers()
    {
        TickDownActiveEffects();

        currentStatsModifier = new EntityStatsModifiers();

        foreach (var item in activeEffects)
        {
            currentStatsModifier = item.statMod.ApplyStatModChanges(currentStatsModifier);
        }
    }

    private void TickDownActiveEffects()
    {
        for (int i = 0; i < activeEffects.Count; i++)
        {
            activeEffects[i].turnsRemaining--;

            if (activeEffects[i].turnsRemaining < 0)
            {
                activeEffects.RemoveAt(i);
                i--;
            }
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        if (playerSprite != null)
            playerSprite.color = playerColor;
        
        if (occupiedNode != null)
        {
            transform.position = occupiedNode.transform.position;
            occupiedNodeCopy = occupiedNode;
            traveledNodes.Add(occupiedNode);
        }
    }

    public bool canLevelUp() {
        float threshold = ( RenownLevel * 100 ) * ( Mathf.Pow(1.15f, RenownLevel-1) );
        // 100, 230, 396, 608, 874... Every level costs around 30% more (should be tuned in testing).
        if (ReputationPoints >= threshold) {
            Debug.Log("Passed threshold of " + threshold);
            return true; // Allows the level up screen when ready on the player's turn.
        } else {
            return false;
        }
    }
}
