using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Unity.VisualScripting;
using Cinemachine;

public class UITileTooltipManager : MonoBehaviour
{
    private CinemachineVirtualCamera vcam;
    private bool storestockDisplayed = false;

    [Header("Selected Tile Information")]
    [SerializeField] private CanvasGroup tileInfoGroup;
    [SerializeField] private TextMeshProUGUI tileType;
    [SerializeField] private TextMeshProUGUI tileDescription;


    [Header("Storestock on Tile")]
    [SerializeField] private CanvasGroup storestockGroup;
    [SerializeField] private List<Image> storestockIcons;
    [SerializeField] private List<TextMeshProUGUI> storestockNames;
    [SerializeField] private List<TextMeshProUGUI> storestockPrices;



    [Header("Listen on Event Channels")]
    public NodeEventChannelSO m_EnterRaycastedTile;
    public VoidEventChannelSO m_ExitRaycastedTile;
    public VoidEventChannelSO m_DisableFreeview;

    private void OnEnable()
    {
        m_EnterRaycastedTile.OnEventRaised += DisplayTileInformation;
        m_ExitRaycastedTile.OnEventRaised += HideTileInformation;
        m_DisableFreeview.OnEventRaised += HideTileInformation;
    }

    private void OnDisable()
    {
        m_EnterRaycastedTile.OnEventRaised -= DisplayTileInformation;
        m_ExitRaycastedTile.OnEventRaised -= HideTileInformation;
        m_DisableFreeview.OnEventRaised -= HideTileInformation;
    }

    private void Start()
    {
        vcam = GetComponentInChildren<CinemachineVirtualCamera>();
        tileInfoGroup.alpha = 0;
        storestockGroup.alpha = 0;
    }

    public void DisplayTileInformation(MapNode node)
    {
        Debug.Log(node.gameObject);
        vcam.Follow = node.transform;

        FadeTo(tileInfoGroup, 1, 0.25f);
        Debug.Log(node.gameObject);

        // Storefront Tile
        if (node.TryGetComponent<StoreManager>(out StoreManager store))
        {
            var playerHexColor = store.playerOwner.playerColor.ToHexString();

            tileType.text = $"<color=#{playerHexColor}>{store.playerOwner.entityName}'s Storefront</color>";
            tileDescription.text = "Upon landing, buy an item from the store.";
            tileDescription.text += $"\n\nIf <color=#{playerHexColor}>";
            tileDescription.text += $"{store.playerOwner.entityName}</color> lands, restock items from inventory.";

            DisplayStoreStock(store);
        }
        else
        {
            if (storestockDisplayed)
            {
                FadeTo(storestockGroup, 0, 0.15f);
                storestockDisplayed = false;
            }

            // Stamp Tile
            if (node.TryGetComponent<Stamp>(out Stamp stamp))
            {
                //var stampHexColor = stamp.stampColor.ToHexString();
                //tileType.text = $"<color=#{stampHexColor}>{stamp.stampType} Stamp</color>";
                //tileDescription.text = $"Upon passing, collect the <color=#{stampHexColor}>{stamp.stampType}</color> stamp.";
                //tileDescription.text += $"\n\nCan only hold one <color=#{stampHexColor}>{stamp.stampType}</color> stamp at a time.";
                //tileDescription.text += "\n\nFights cannot happen on this tile.";
                tileType.color = node.tileHoverColor;
                tileType.text = node.GetTileTypeString();
                tileDescription.text = node.GetTileAboutString();
            }
            // Pawn Shop
            else if (node.tag == "Castle") //please change this tag
            {
                //tileType.text = "<color=yellow>Central Market</color>";
                //tileDescription.text = "Upon passing, heal <color=#4DCF56>33% HP</color> and exchange all currently held stamps. " +
                //    "\n\nGain <sprite=\"Coin Icon\" index=0> and <color=#7AFFF7>EXP</color> relative to the number of exchanged stamps.";
                //tileDescription.text += "\n\nThe first player to pass with <color=#FDECAD>4000</color><sprite=\"Coin Icon\" index=0> in hand wins!";
                //tileDescription.text += "\n\n<sprite=\"Coin Icon\" index=0> recieved is based on the number of colored stamps exchanged at once.";

                tileType.color = node.tileHoverColor;
                tileType.text = node.GetTileTypeString();
                tileDescription.text = node.GetTileAboutString();
            }
            else if(node.tag == "Vendor")
            {
                //tileType.text = "<color=#5AA6DC>Vendor</color>";
                //tileDescription.text = "Upon landing, choose to enter and buy the item being sold.";
                //tileDescription.text += "\n\nBecome safe from fights and damage if entered.";

                tileType.color = node.tileHoverColor;
                tileType.text = node.GetTileTypeString();
                tileDescription.text = node.GetTileAboutString();
            }
            else if(node.tag == "MoveAgain")
            {
                //tileType.text = "<color=#2AA583>Once More</color>";
                //tileDescription.text = "Upon landing, allow the current player to take their turn again.";
                //tileDescription.text += "\n\nFights cannot happen on this tile.";

                tileType.color = node.tileHoverColor;
                tileType.text = node.GetTileTypeString();
                tileDescription.text = node.GetTileAboutString();
            }
            else if (node.tag == "CoconutTree")
            {
                //tileType.text = "<color=#65D252>Coconut Tree</color>";
                //tileDescription.text = "Upon landing, a Coconut may fall on your head.";
                //tileDescription.text += "\n\nTake damage and recieve a Coconut.";

                tileType.color = node.tileHoverColor;
                tileType.text = node.GetTileTypeString();
                tileDescription.text = node.GetTileAboutString();
            }
            else if (node.tag == "WaterCoconut")
            {
                tileType.text = "<color=#65D252>Water Coconut Bushel</color>";
                tileDescription.text = "Upon landing, roll any dice of your choice to reach the given requirement.";
                tileDescription.text += "\n\nIf successful, harvest a piece of Water Coconut.";
            }
            // Wild Tile
            else
            {
                //tileType.text = "Wild Tile";
                //tileDescription.text = "Upon landing, encounter an enemy.";
                //tileDescription.text += "\n\nCan set up shop on this space.";
                tileType.color = node.tileHoverColor;
                tileType.text = node.GetTileTypeString();
                tileDescription.text = node.GetTileAboutString();
            }
        }
    }

    public void HideTileInformation()
    {
        FadeTo(tileInfoGroup, 0, 0.25f);
        FadeTo(storestockGroup, 0, 0.25f);      
    }

    public void DisplayStoreStock(StoreManager store)
    {
        storestockDisplayed = true;
        FadeTo(storestockGroup, 1, 0.25f);
        var storeInventory = store.storeInventory;

        ItemStats storeItem = null;

        for (int i = 0; i < storestockNames.Count; i++)
        {
            if (i < storeInventory.Count)
            {
                storeItem = storeInventory[i];
            }

            if (storeItem == null || i >= storeInventory.Count)
            {
                storestockIcons[i].sprite = null;
                storestockIcons[i].enabled = false;
                storestockNames[i].text = "";
                storestockPrices[i].text = "";

                storestockNames[i].GetComponentInParent<Button>().interactable = false;
            }
            else
            {
                storestockIcons[i].sprite = storeItem.itemSprite;
                storestockIcons[i].enabled = true;
                storestockNames[i].text = $"{storeItem.l_itemName.GetLocalizedString()}";
                storestockPrices[i].text = $"<sprite=\"Coin Icon\" index=0>{storeItem.basePrice}";

                storestockNames[i].GetComponentInParent<Button>().interactable = false;
            }
        }
    }

    public void FadeTo(CanvasGroup group, float alphaValue, float duration)
    {
        DOTween.Kill(group.gameObject);
        DOTween.To(() => group.alpha, x => group.alpha = x, alphaValue, duration);
    }
}
