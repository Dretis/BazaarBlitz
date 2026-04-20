using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
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

    public SpriteRenderer flowerTrapVisual;
    public Sprite[] flowerTrapSprites;

    public SpriteRenderer storefrontVisual;
    public CanvasGroup stockGroup;
    public Image[] stockItems;
    public TMP_Text soldoutText;

    [Header("Nearby Nodes")] 
    public MapNode north;
    public MapNode east;
    public MapNode south;
    public MapNode west;

    [Header("Modifiers / Trap")]
    public Modifier modifier = Modifier.None;
    public EntityPiece modifierOwner;

    public enum Modifier
    {
        None,
        Marigold,
        Rafflesia,
        Bomb
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
        Debug.Log($"Landed on MapNode - {name}");
    }

    public virtual void PassByThisNode(EntityPiece p)
    {
        // Current player passed this node
        if (p.movementLeft == 0) return;

        Debug.Log($"Passed MapNode - {name}");
    }

    public virtual void UndoPassByThisNode(EntityPiece p)
    {
        // Current player is undo'd passing this node

        Debug.Log($"Undo Passing MapNode - {name}");
    }

    public virtual string GetTileTypeString()
    {
        return tileTypeString.GetLocalizedString();
    }

    public virtual string GetTileAboutString()
    {
        return tileAboutString.GetLocalizedString();
    }
}
