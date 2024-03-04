using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stamp : MonoBehaviour
{
    public GameObject spawnNode;
    public Color stampColor;
    public bool isCollected = false;

    void Awake()
    { 
        spawnNode = this.gameObject;
        spawnNode.GetComponent<SpriteRenderer>().color = stampColor;
    }
}
