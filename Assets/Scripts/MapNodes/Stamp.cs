using UnityEngine;

public class Stamp : MonoBehaviour
{
    public GameObject spawnNode;
    public Color stampColor;
    public StampType stampType;
    public enum StampType
    {
        Green,
        Red,
        Blue,
        Orange
    }

    void Awake()
    { 
        spawnNode = this.gameObject;
        spawnNode.GetComponent<SpriteRenderer>().color = stampColor;
    }
}
