using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Febucci.UI.Core;
using LitMotion;
using Core;

public class RestockBoatSelectionHandler : MonoBehaviour, ISubmitHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    private UIRenovateSelectorManager _uiRenovateManager;
    private BoardNodeData _boardNodeData;
    private StorefrontData _storeData;
    [Header("Store Details")]
    [SerializeField] private int nodeId;
    [SerializeField] private MapNode storeLocation;
    [SerializeField] private StoreManager storeManager;
    [SerializeField] private EntityPiece owner;
    private int storestock = 0;
    private string storestockIndicator = "";

    [Header("UI Elements")]
    [SerializeField] private Image boatIcon;
    [SerializeField] private TextMeshProUGUI boatNumText;
    [SerializeField] private TextMeshProUGUI boatLvlText;
    [SerializeField] private TextMeshProUGUI boatStockText;
    [SerializeField] private Color ownerColor;

    private MotionHandle currentMotion;

    [Header("Broadcast on Event Channel")]
    //public NodeEventChannelSO m_RestockStore;
    public NodeEventChannelSO m_FocusOnNode;

    public NodeEventChannelSO m_PromptRenovateOptions;

    public void Initialize(int index, BoardNodeData bnd, UIRenovateSelectorManager uiRnvManager)
    {
        _uiRenovateManager = uiRnvManager;
        _boardNodeData = bnd;
        _storeData = bnd.StorefrontData;

        //UpdateStoreBoatVisuals();
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
        // NEW: Show prompt to let player pick Restock or Upgrade!
        //m_PromptRenovateOptions.RaiseEvent(storeLocation);
        _uiRenovateManager.PromptRenovateOptions(_boardNodeData);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (currentMotion.IsPlaying()) currentMotion.Cancel();

        currentMotion = LMotion.Create(GetComponent<RectTransform>().localScale, Vector3.one * 1.4f, 0.2f)
            .WithEase(Ease.OutBack)
            .Bind(x => GetComponent<RectTransform>().localScale = x);

        boatStockText.text = storestockIndicator;
        // Focus the camera on that node's position 
        //m_FocusOnNode.RaiseEvent(storeLocation);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (currentMotion.IsPlaying()) currentMotion.Cancel();

        currentMotion = LMotion.Create(GetComponent<RectTransform>().localScale, Vector3.one, 0.2f)
            .WithEase(Ease.OutBack)
            .Bind(x => GetComponent<RectTransform>().localScale = x);

        boatStockText.text = "";
    }

    public void UpdateStoreBoatVisual(int index, StorefrontData store, Color ownerColor)
    {
        //storeLocation = store.GetComponent<MapNode>();

        //storeManager = store;
        //owner = store.playerOwner;
        //ownerColor = owner.playerColor;

        foreach(ItemStats item in store.Storestock)
        {
            if(item !=  null) storestock++;
        }

        if (storestock >= 3) storestockIndicator = "FULL";
        else if (storestock == 0) storestockIndicator = "EMPTY";


        //boatNumText.text = boatIndex.ToString();
        //boatNumText.color = ownerColor;
        boatStockText.text = "";
        boatStockText.color = ownerColor;
        boatIcon.color = ownerColor;
        boatLvlText.text = "";
        for (int i = 0; i < store.StoreLevel; i++)
        {
            boatLvlText.text += "*";
        }
    }

    private void OnDestroy()
    {
        if (currentMotion.IsPlaying()) currentMotion.Cancel();
    }
}
