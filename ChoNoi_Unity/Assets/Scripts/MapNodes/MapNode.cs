using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Playables;
using UnityEngine.UI;

public class MapNode : MonoBehaviour
{

    public List<EntityPiece> playersOccupied;

    [Header("Hover Information")]
    public Color tileHoverColor;
    public LocalizedString tileTypeString;
    public LocalizedString tileAboutString;
    
    [Header("Visual Variables")]
    public GameObject focusPoint;
    [Space]
    public SpriteRenderer flowerTrapVisual;
    public Sprite[] flowerTrapSprites;
    [Space]
    public SpriteRenderer storeModVisual; // For Lucky Cat / Neko
    [Space]
    public SpriteRenderer storefrontVisual;
    public CanvasGroup stockGroup;
    public Image[] stockItems;
    public TMP_Text soldoutText;
    public TMP_Text storeLevelIndicator;
    [Space]
    public PlayableDirector pd_nUpgradeStore;

    [Header("Nearby Nodes")] 
    public MapNode north;
    public MapNode east;
    public MapNode south;
    public MapNode west;

    [Header("Modifiers / Trap")]
    public Modifier modifier = Modifier.None;
    public StoreModifier storeModifier = StoreModifier.Empty;
    public EntityPiece modifierOwner;

    public enum Modifier
    {
        None,
        Marigold,
        Rafflesia,
        Bomb
    }
    public enum StoreModifier
    {
        Empty,
        NekoWhite,
        NekoBlack,
        NekoGold
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, .15f);

        Gizmos.color = Color.green;
        if (north != null)
            Gizmos.DrawLine(transform.position, north.transform.position);

        Gizmos.color = Color.red;
        if (east != null)
            Gizmos.DrawLine(transform.position, east.transform.position);

        Gizmos.color = Color.blue;
        if (south != null)
            Gizmos.DrawLine(transform.position, south.transform.position);

        Gizmos.color = Color.yellow;
        if (west != null)
            Gizmos.DrawLine(transform.position, west.transform.position);
    }

    protected virtual void OnEnable()
    {
        //m_NextPlayerTurn.OnEventRaised += OnNextPlayerTurn;
    }

    protected virtual void OnDisable()
    {
        //m_NextPlayerTurn.OnEventRaised -= OnNextPlayerTurn;
    }

    public virtual void LandOnThisNode(EntityPiece p)
    {
        // Current player landed on this node
        Debug.Log($"Landed on MapNode - {name} | Tag: {tag}");
    }

    public virtual void PassByThisNode(EntityPiece p)
    {
        // Current player passed this node
        if (p.movementLeft == 0) return;

        Debug.Log($"Passed MapNode - {name} | Tag: {tag}");
    }

    public virtual void UndoPassByThisNode(EntityPiece p)
    {
        // Current player is undo'd passing this node

        Debug.Log($"Undo Passing MapNode - {name} | Tag: {tag}");
    }

    public virtual string GetTileTypeString()
    {
        return tileTypeString.GetLocalizedString();
    }

    public virtual string GetTileAboutString()
    {
        return tileAboutString.GetLocalizedString();
    }

    public void SignalUpgradeOver()
    {
        GameplayTest.instance.phase = GameplayTest.instance.expectedPhase;
        Debug.Log($"SignalUpgradeOver - {name} | ExpectedPhase = {GameplayTest.instance.expectedPhase}");
    }
}
