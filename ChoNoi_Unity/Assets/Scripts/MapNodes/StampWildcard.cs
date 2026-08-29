using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.Playables;
using static GameplayTest;

/// <summary>
/// Script for the Stamp Node
/// </summary>
public class StampWildcard : MapNode
{
    [Header("Stamp Wildcard Specific")]
    [SerializeField] private List<LocalizedString> localizedStampColors;

    public GameObject spawnNode;
    public List<Color> stampColors;
    [SerializeField] private SpriteRenderer stampVisual;
    public List<Sprite> stampSprites; // Green - Red - Blue - Orange
    public List<Stamp.StampType> stampTypes; // Green - Red - Blue - Orange

    public Stamp.StampType currentStampType;
    public Color currentStampColor;
    public int currentStampIndex; 

    [SerializeField] private PlayableDirector pd_CollectStamp;

    [Header("Current Player Tracking")]
    [SerializeField] private List<List<Stamp.StampType>> snapshotStamps = new List<List<Stamp.StampType>>();

    [Header("Broadcast on Event Channels")]
    public PlayerEventChannelSO m_PassByStamp;
    public StampEventChannelSO m_UndoPassByStamp;

    public PlayerEventChannelSO m_AskRenovateStore;

    [Header("Listen on Event Channels")]
    public PlayerEventChannelSO m_NextPlayerTurn;

    protected override void OnEnable()
    {
        m_NextPlayerTurn.OnEventRaised += OnNextPlayerTurn;
    }

    protected override void OnDisable()
    {
        m_NextPlayerTurn.OnEventRaised -= OnNextPlayerTurn;
    }

    void Awake()
    {
        spawnNode = this.gameObject;
        //spawnNode.GetComponent<SpriteRenderer>().color = stampColor;
    }

    public override void LandOnThisNode(EntityPiece p)
    {
        // Current player landed on this node
        Debug.Log($"Landed on MapNode - {name}");
        Debug.Log($"Reseting {p.entityName}'s direction!");
        p.previousNode = null;

        //GameplayTest.instance.phase = GamePhase.EndTurn;
        if(p.storeCount > 0)
        {
            m_AskRenovateStore.RaiseEvent(p);
            GameplayTest.instance.expectedPhase = GamePhase.EndTurn;
        }
        else
        {
            GameplayTest.instance.phase = GamePhase.EndTurn;
        }
    }

    public override void PassByThisNode(EntityPiece p)
    {
        // Current player passed this node
        //if (p.movementLeft == 0) return;

        Debug.Log($"Passed Stamp MapNode - {name}");
        if (p.stamps.Count == 0)
        {
            Debug.Log($"Snapshoted player having 0 stamps.");
            snapshotStamps.Add(null);
        }
        else
        {
            snapshotStamps.Add(new List<Stamp.StampType>(p.stamps));
            //Debug.Log($"Added player's stamps into snapshotStamps | # of stamps = {snapshotStamps[^1].Count}");
            //DebugShowSnapshotStamps(snapshotStamps[^1]);
        }

        if (!p.stamps.Contains(stampTypes[currentStampIndex]))
        {
            Debug.Log($"{p.entityName} collected the {stampTypes[currentStampIndex]} stamp!");

            p.stamps.Add(stampTypes[currentStampIndex]);

            PlayCollectStamp(); //plays timeline to show you picked this up
            m_PassByStamp.RaiseEvent(p);
        }
        // Changes the stamp type anyway
        IncrementStampType();
    }

    public override void UndoPassByThisNode(EntityPiece p)
    {
        base.UndoPassByThisNode(p);

        if (snapshotStamps == null) return;

        if (snapshotStamps[^1] == null)
        {
            //Debug.Log($"Player had 0 stamps before.");
            p.stamps.Clear();
            snapshotStamps.RemoveAt(snapshotStamps.Count - 1);
        }
        else
        {
            //Debug.Log($"Removed last player's stamps from snapshotStamps | # of stamps = {snapshotStamps[^1]}");
            //DebugShowSnapshotStamps(snapshotStamps[^1]);

            p.stamps = new List<Stamp.StampType>(snapshotStamps[^1]);
            snapshotStamps.RemoveAt(snapshotStamps.Count - 1);
        }
        DecrementStampType();
        m_UndoPassByStamp.RaiseEvent(stampTypes[currentStampIndex]);
    }

    private void DecrementStampType()
    {
        currentStampIndex--;

        if(currentStampIndex < 0)
        {
            currentStampIndex = stampTypes.Count - 1;
        }

        ChangeStampType(0);
    }

    private void IncrementStampType()
    {
        currentStampIndex++;

        if (currentStampIndex >= stampTypes.Count)
        {
            currentStampIndex = 0;
        }

        ChangeStampType(0.25f);
    }

    public void ChangeStampType(float delay)
    {
        var i = currentStampIndex;

        currentStampColor = stampColors[i];
        currentStampType = stampTypes[i];

        //stampVisual.sprite = stampSprites[i];
        StartCoroutine(DelayChangeStampSprite(delay));
    }

    private IEnumerator DelayChangeStampSprite(float delay)
    {
        yield return new WaitForSeconds(delay);
        var i = currentStampIndex;
        stampVisual.sprite = stampSprites[i];
    }

    public void PlayCollectStamp()
    {
        pd_CollectStamp.Play();
    }

    public virtual void OnNextPlayerTurn(EntityPiece entity)
    {
        snapshotStamps.Clear();
    }

    public void DebugShowSnapshotStamps(List<Stamp.StampType> stamps)
    {
        var i = 1;
        foreach (Stamp.StampType stamp in stamps)
        {
            Debug.Log($"#{i} - {stamp}");
            i++;
        }
    }

    #region Localization Hover Info
    public override string GetTileTypeString()
    {
        //(tileTypeString["COLOR"] as StringVariable).Value = GetLocalizedStampColor();
        //(tileTypeString["HEX_COLOR"] as StringVariable).Value = stampColor.ToHexString();

        //SetupLocalVariableStampColor(tileTypeString);
        //SetupLocalVariableHexColor(tileTypeString);
        //tileTypeString.RefreshString();
        return tileTypeString.GetLocalizedString();
    }

    public override string GetTileAboutString()
    {
        //(tileAboutString["COLOR"] as StringVariable).Value = GetLocalizedStampColor();
        //(tileAboutString["HEX_COLOR"] as StringVariable).Value = currentStampColor.ToHexString();

        //SetupLocalVariableStampColor(tileAboutString);
        //SetupLocalVariableHexColor(tileAboutString);
        //tileAboutString.RefreshString();
        return tileAboutString.GetLocalizedString();
    }

    private string GetLocalizedStampColor()
    {
        return localizedStampColors[(int)currentStampType].GetLocalizedString();
    }

    private void SetupLocalVariableStampColor(LocalizedString ls)
    {
        var variable = ls["COLOR"] as StringVariable;
        Debug.Log($"The COLOR value is {variable.Value}");
        variable.Value = GetLocalizedStampColor();

        Debug.Log($"The COLOR value now is {variable.Value}");
    }

    private void SetupLocalVariableHexColor(LocalizedString ls)
    {
        var variable = ls["HEX_COLOR"] as StringVariable;
        Debug.Log($"The HEX_COLOR value is {variable.Value}");
        variable.Value = currentStampColor.ToHexString();

        Debug.Log($"The HEX_COLOR value now is {variable.Value}");
    }
    #endregion Localization Hover Info
}
