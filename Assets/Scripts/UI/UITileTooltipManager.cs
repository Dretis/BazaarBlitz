using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Unity.VisualScripting;
using Cinemachine;
using UnityEditor.Experimental.GraphView;

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

    private void OnEnable()
    {
        m_EnterRaycastedTile.OnEventRaised += DisplayTileInformation;
        m_ExitRaycastedTile.OnEventRaised += HideTileInformation;
    }

    private void OnDisable()
    {
        m_EnterRaycastedTile.OnEventRaised -= DisplayTileInformation;
        m_ExitRaycastedTile.OnEventRaised -= HideTileInformation;
    }

    private void Start()
    {
        vcam = GetComponentInChildren<CinemachineVirtualCamera>();
        tileInfoGroup.alpha = 0;
        storestockGroup.alpha = 0;
    }

    public void DisplayTileInformation(MapNode node)
    {
        vcam.Follow = node.transform;

        FadeTo(tileInfoGroup, 1, 0.5f);
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
                var stampHexColor = stamp.stampColor.ToHexString();
                tileType.text = $"<color=#{stampHexColor}>{stamp.stampType} Stamp</color>";
                tileDescription.text = $"Upon passing, collect the <color=#{stampHexColor}>{stamp.stampType}</color> stamp.";
                tileDescription.text += "\n\nCan only collect one of this stamp at a time.";
            }
            // Pawn Shop
            else if (node.tag == "Castle") //please change this tag
            {
                tileType.text = "Pawn Shop";
                tileDescription.text = "Upon passing, exchange in all held stamps for <color=yellow>@</color>.";
                tileDescription.text += "\n\nYou get more <color=yellow>@</color> the more stamps exchanged at once.";
            }
            // Wild Tile
            else
            {
                tileType.text = "Wild Tile";
                tileDescription.text = "Upon landing, encounter an enemy.";
                tileDescription.text += "\n\nCan build a store on this space.";
            }
        }
    }

    public void HideTileInformation()
    {
        vcam.Follow = GameplayTest.instance.currentPlayer.transform; //wow this line is dogshit

        FadeTo(tileInfoGroup, 0, 0.25f);
        FadeTo(storestockGroup, 0, 0.25f);
        
    }

    public void DisplayStoreStock(StoreManager store)
    {
        storestockDisplayed = true;
        FadeTo(storestockGroup, 1, 0.5f);
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
                storestockNames[i].text = $"{storeItem.itemName}";
                storestockPrices[i].text = $"<color=yellow>@</color>{storeItem.basePrice}";

                storestockNames[i].GetComponentInParent<Button>().interactable = false;
            }
        }
    }

    public void FadeTo(CanvasGroup group, float alphaValue, float duration)
    {
        DOTween.To(() => group.alpha, x => group.alpha = x, alphaValue, duration);
    }
}
