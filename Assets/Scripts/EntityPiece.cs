using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPiece : MonoBehaviour
{
    public string nickname;
    public int id;
    public Color playerColor; // idk man
    public SpriteRenderer playerSprite; // idk man
    public MapNode occupiedNode; // Node player is currently on
    public MapNode occupiedNodeCopy; // Node player is currently on
    public List<MapNode> traveledNodes = new List<MapNode>(); // Tracks the nodes the player has gone to

    [Header("Stats")]
    public int movementTotal;
    public int movementLeft;
    public int finalPoints = 1;
    public int heldPoints = 0;

    // Start is called before the first frame update
    void Start()
    {
        playerSprite.color = playerColor;
        transform.position = occupiedNode.transform.position;
        occupiedNodeCopy = occupiedNode;
        traveledNodes.Add(occupiedNode);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
