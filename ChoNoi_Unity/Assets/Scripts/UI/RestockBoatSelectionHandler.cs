using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Febucci.UI.Core;
using LitMotion;

public class RestockBoatSelectionHandler : MonoBehaviour, ISubmitHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    private int boatIndex;
    private RectTransform boatRect;

    [Header("Store Details")]
    [SerializeField] private MapNode storeLocation;
    [SerializeField] private StoreManager storeManager;
    [SerializeField] private EntityPiece owner;
    private int storestock = 0;
    private string storestockIndicator = "";

    [Header("UI Elements")]
    [SerializeField] private Image boatIcon;
    [SerializeField] private TextMeshProUGUI boatNumText;
    [SerializeField] private TextMeshProUGUI boatStockText;
    [SerializeField] private Color ownerColor;

    private MotionHandle currentMotion;

    [Header("Broadcast on Event Channel")]
    public NodeEventChannelSO m_RestockStore;
    public NodeEventChannelSO m_FocusOnNode;

    private void Awake()
    {
        boatIcon = GetComponent<Image>();
        boatRect = GetComponent<RectTransform>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        eventData.selectedObject = gameObject;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        eventData.selectedObject = null;
    }
    public void OnSubmit(BaseEventData eventData)
    {
        Debug.Log("BoatSelector - Submitted | " + eventData);
        if (storestock >= 3)
        {
            // Play some goofa noise with this
            Debug.Log("you can't restock a full store!!");
        }
        else
            m_RestockStore.RaiseEvent(storeLocation);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (currentMotion.IsPlaying()) currentMotion.Cancel();

        currentMotion = LMotion.Create(GetComponent<RectTransform>().localScale, Vector3.one * 1.4f, 0.2f)
            .WithEase(Ease.OutBack)
            .Bind(x => GetComponent<RectTransform>().localScale = x);

        boatStockText.text = storestockIndicator;
        // Focus the camera on that node's position
        m_FocusOnNode.RaiseEvent(storeLocation);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (currentMotion.IsPlaying()) currentMotion.Cancel();

        currentMotion = LMotion.Create(GetComponent<RectTransform>().localScale, Vector3.one, 0.2f)
            .WithEase(Ease.OutBack)
            .Bind(x => GetComponent<RectTransform>().localScale = x);

        boatStockText.text = "";
    }

    public void UpdateBoatInfo(int index, StoreManager store)
    {
        storeLocation = store.GetComponent<MapNode>();

        storeManager = store;
        owner = store.playerOwner;
        ownerColor = owner.playerColor;

        foreach(ItemStats item in store.storeInventory)
        {
            if(item !=  null) storestock++;
        }

        if (storestock >= 3) storestockIndicator = "FULL";
        else if (storestock == 0) storestockIndicator = "EMPTY";

        boatIndex = index;

        boatNumText.text = boatIndex.ToString();
        boatNumText.color = ownerColor;
        boatStockText.text = "";
        boatStockText.color = ownerColor;
    }

    private void OnDestroy()
    {
        if (currentMotion.IsPlaying()) currentMotion.Cancel();
    }
}
