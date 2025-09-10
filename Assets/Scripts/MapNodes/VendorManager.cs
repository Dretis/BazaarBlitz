using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using LitMotion;
using static UnityEngine.Rendering.DebugUI;

public class VendorManager : MonoBehaviour
{
    [Header("Vendor Stuff")]
    public Vendor vendor;
    public ItemStats itemForSale;
    public int salePrice;
    public List<MapNode> associatedNodes; // what nodes is this vendor related to
    public List<EntityPiece> seatedPlayers; // players currently in the vendor
    private bool currentlyVisiting = false;
    

    [Header("Visual Variables")]
    public GameObject vendorObject;

    public CanvasGroup vendorPromptGroup;
    public TextMeshProUGUI vendorDialogue;

    public List<GameObject> seatedPlayerSprites;

    [Header("Broadcast on Event Channels")]
    public IntItemEventChannelSO m_RecieveVendorItem;

    [Header("Listen on Event Channels")]
    public NodeEventChannelSO m_LandedOnVendor;
    public PlayerEventChannelSO m_BuyFromVendor;
    public VoidEventChannelSO m_ExitVendor; // also broadcasting

    public PlayerEventChannelSO m_NextPlayerTurn;

    private void OnEnable()
    {
        m_LandedOnVendor.OnEventRaised += OnLandedOnVendor;
        m_BuyFromVendor.OnEventRaised += OnBuyFromVendor;
        m_ExitVendor.OnEventRaised += OnExitVendor;

        m_NextPlayerTurn.OnEventRaised += OnNextPlayerTurn;
    }

    private void OnDisable()
    {
        m_LandedOnVendor.OnEventRaised -= OnLandedOnVendor;
        m_BuyFromVendor.OnEventRaised -= OnBuyFromVendor;
        m_ExitVendor.OnEventRaised -= OnExitVendor;

        m_NextPlayerTurn.OnEventRaised -= OnNextPlayerTurn;
    }

    private void OnLandedOnVendor(MapNode node)
    {
        if(associatedNodes.Contains(node))
        {
            LMotion.Create(0f, 1f, 0.25f)
                .Bind(x => vendorPromptGroup.alpha = x);

            itemForSale = vendor.itemsForSale[0];
            salePrice = (int)(itemForSale.basePrice * 0.6f);

            vendorDialogue.text = vendor.enterDialogue;

            currentlyVisiting = true;
            //vendorPromptGroup.interactable = true;
            //EventSystem.current.SetSelectedGameObject(vendorButtonHolders[0]);
            //m_FocusOnVendor.RaiseEvent();
        }
    }
    private void OnBuyFromVendor(EntityPiece player)
    {
        if (!currentlyVisiting) return;
        //seatedPlayers.Add(player);
        if (player.heldPoints < salePrice)
        {
            //wtf dont let them buy this shit
            vendorDialogue.text = vendor.leaveDialogue;
            m_ExitVendor.RaiseEvent();
            return;
        }

        // DO the do
        var newestPlayerID = seatedPlayers.Count - 1;
        for (int i = 0; i < seatedPlayers.Count; i++)
        {
            if (seatedPlayers[i] == null)
            {
                seatedPlayers[i] = player;
                newestPlayerID = i;
                break;
            }
        }

        // Change state for the Player
        player.playerSprite.enabled = false;
        player.currentStates.Add(EntityPiece.State.InsideVendor);

        // Make it so the player looks like they're in the vendor
        seatedPlayerSprites[newestPlayerID].GetComponent<SpriteRenderer>().color = Color.white;

        seatedPlayerSprites[newestPlayerID].GetComponent<PlayerPaletteLoader>().
                SetInspectorPalette(player.GetComponent<PlayerPaletteLoader>().GetInspectorPalette());

        m_RecieveVendorItem.RaiseEvent(salePrice, itemForSale);

        vendorDialogue.text = vendor.purchaseDialogue;
    }

    private void OnExitVendor()
    {
        if (!currentlyVisiting) return;
        // just get rid of prompt
        LMotion.Create(1f, 0f, 0.25f)
                .Bind(x => vendorPromptGroup.alpha = x);

        currentlyVisiting = false;
    }

    private void OnNextPlayerTurn(EntityPiece player)
    {
        OnExitVendor();

        if (seatedPlayers.Contains(player))
        {
            var id = seatedPlayers.IndexOf(player);
            //seatedPlayers.Remove(player);
            seatedPlayers[id] = null;

            seatedPlayerSprites[id].GetComponent<SpriteRenderer>().color = new Color32(0,0,0,0);
        }
        else
        {
            Debug.Log($"wtf {player.entityName} isn't here");
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        vendorPromptGroup.alpha = 0.0f;
        //vendorPromptGroup.interactable = false;
    }
}
