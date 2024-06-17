using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;

public class EntityPiece : MonoBehaviour
{
    public string entityName;
    public int id;
    public Color playerColor; // idk man
    public SpriteRenderer playerSprite; // idk man
    public AnimatorController combatAnimatorController; // to use and send in combat
    public MapNode occupiedNode; // Node player is currently on
    public MapNode occupiedNodeCopy; // Node player is currently on
    public MapNode previousNode = null; // Node player just walked on last turn. They can't go back this way.
    public int unspentLevelUpPoints = 0;
    public List<MapNode> traveledNodes = new List<MapNode>(); // Tracks the nodes the player has gone to
    public ParticleSystem dustCloud;

    [Header("Overworld Stats")]
    public int movementTotal;
    public int movementLeft;
    public int heldPoints = 0;
    public int storeCount = 0;
    public List<Stamp.StampType> stamps = new List<Stamp.StampType>();
    public bool isInDeathsRow = false;

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
    public float levelThreshold = 100;
    public int RenownLevel = 1; // Used to calculate the next level threshold.

    private void OnEnable()
    {
        if (!isEnemy && combatSceneIndex > -1)
        {
            // In combat, dust cloud around players
            dustCloud.gameObject.SetActive(true);
            dustCloud.Play();
        }
    }

    [Serializable, Inspectable]
    public class ActiveEffect
    {
        public ItemStats originalItem;
        public int turnsRemaining;
    }

    public EntityStatsModifiers currentStatsModifier;

    public List<ItemStats> inventory = new();
    public List<int> lootOdds;

    [SerializeField]
    public List<ActiveEffect> activeEffects = new();

    /// <summary>
    /// Add a specified item to this player's list of active stat modifier effects
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="item"></param>
    public void AddItemToActiveEffects(int duration, ItemStats item)
    {

        var sameEffect = activeEffects.Find(activeEffect => (UnityEngine.Object) activeEffect.originalItem == item);

        // Refresh effect if same item has been used before. Otherwise, add new effect.
        if (sameEffect != null)
        {
            sameEffect.turnsRemaining = duration;
        }
        else
        {
            activeEffects.Add(new ActiveEffect
            {
                originalItem = item,
                turnsRemaining = duration
            });
        }
    }

    /// <summary>
    /// Tick down active effects by one turn and recalculate this player's stat modifiers
    /// </summary>
    public void UpdateStatModifiers()
    {
        TickDownActiveEffects();
        RefreshStatModifiers();
    }

    public void UpdateStatModifier(ActiveEffect effect)
    {
        TickDownActiveEffect(effect);
        RefreshStatModifiers();
    }

    public void RefreshStatModifiers()
    {
        currentStatsModifier = new EntityStatsModifiers();

        foreach (var item in activeEffects)
        {
            currentStatsModifier = item.originalItem.ApplyStatModChanges(currentStatsModifier, item.originalItem.Duration - item.turnsRemaining);
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

    private void TickDownActiveEffect(ActiveEffect effect)
    {
        var effectIndex = activeEffects.FindIndex(activeEffect => activeEffect.originalItem == effect.originalItem);
        if (effectIndex != -1)
        {
            activeEffects[effectIndex].turnsRemaining--;

            if (activeEffects[effectIndex].turnsRemaining < 0)
            {
                activeEffects.RemoveAt(effectIndex);
            }
        }
        else
        {
            Debug.Log("Effect not found");
        }
    }

    public void RemoveItemEffectOnUse(HashSet<string> itemNames)
    {
        List<EntityPiece.ActiveEffect> effectsToRemove = new List<EntityPiece.ActiveEffect>();

        foreach (var effect in activeEffects)
        {
            if (itemNames.Contains(effect.originalItem.name))
            {
                Debug.Log(effect.originalItem.name + "'s effect is removed!");
                effectsToRemove.Add(effect);
            }
        }

        activeEffects.RemoveAll(effect => effectsToRemove.Contains(effect));
        RefreshStatModifiers();
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
        levelThreshold = ( RenownLevel * 100 ) * ( Mathf.Pow(1.15f, RenownLevel-1) );
        // 100, 230, 396, 608, 874... Every level costs around 30% more (should be tuned in testing).
        if (ReputationPoints >= levelThreshold) {
            Debug.Log("Passed threshold of " + levelThreshold);
            levelThreshold = (RenownLevel * 100) * (Mathf.Pow(1.15f, RenownLevel - 1)); // update the new threshold again
            return true; // Allows the level up screen when ready on the player's turn.
        } else {
            return false;
        }
    }

    // for use on enemies
    public void resetStats() {

        for (int l = RenownLevel; l > 1; l--) {
            for (int i = 0; i < 6; i++) {
                strDie.dieFaces[i] -= 1;
            }
            for (int i = 0; i < 6; i++) {
                dexDie.dieFaces[i] -= 1;
            }
            for (int i = 0; i < 6; i++) {
                intDie.dieFaces[i] -= 1;
            }
        }
        
        RenownLevel = 1;

    }

    public void raiseAllStats() {



        for (int i = 0; i < 6; i++) {
            strDie.dieFaces[i] += 1;
        }
        for (int i = 0; i < 6; i++) {
            dexDie.dieFaces[i] += 1;
        }
        for (int i = 0; i < 6; i++) {
            intDie.dieFaces[i] += 1;
        }
        
    }
}
