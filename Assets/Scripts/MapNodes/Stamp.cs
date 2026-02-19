using UnityEngine;
using UnityEngine.Playables;

public class Stamp : MonoBehaviour
{
    public GameObject spawnNode;
    public Color stampColor;
    public StampType stampType;

    [SerializeField] private PlayableDirector pd_CollectStamp;
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

    public void PlayCollectStamp()
    {
        pd_CollectStamp.Play();
    }
}
