using UnityEngine;

public class MapNode : MonoBehaviour
{
    public EntityPiece playerOccupied;
    [Header("Visual Variables")]
    public SpriteRenderer flowerTrapVisual;
    public SpriteRenderer storefrontVisual;

    [Header("Nearby Nodes")] 
    public MapNode north;
    public MapNode east;
    public MapNode south;
    public MapNode west;
    public Modifier modifier = Modifier.None;
    public EntityPiece modifierOwner;

    public enum Modifier
    {
        None,
        Marigold,
        Rafflesia,
        Bomb
    }

    private void OnDrawGizmos()
    {
        /*
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
        */
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
}
