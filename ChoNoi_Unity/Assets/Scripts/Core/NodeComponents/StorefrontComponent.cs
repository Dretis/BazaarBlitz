using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;
using TMPro;
using UnityEngine.Playables;
using UnityEngine.UI;
using System;
using Core.Events;

public class StorefrontComponent : MonoBehaviour
{
    private BoardNode _boardNode; // og
    //[SerializeField] private StorefrontData _storefrontData;

    [Header("Storefront Variables")]
    [SerializeField] private Color storefrontColor;
    [SerializeField] private SpriteRenderer storefrontBoatVisual;
    [SerializeField] private CanvasGroup stockGroup;
    [SerializeField] private Image[] stockItems;
    [SerializeField] private TMP_Text soldoutText;
    [SerializeField] private TMP_Text storeLevelIndicator;
    [Space]
    [SerializeField] private PlayableDirector pd_nUpgradeStore;
    [Space]
    [SerializeField] private GameStateEventChannelSO _gameStateEvent;

    private void OnEnable()
    {
        _gameStateEvent.OnEventRaised += UpdateFromGameState;
    }

    private void OnDisable()
    {
        _gameStateEvent.OnEventRaised -= UpdateFromGameState;
    }

    private void UpdateFromGameState(FrameGameState gameState)
    {
        if (gameState.NewState.SelectedBoardNodeId != _boardNode.Id) return;
        if (!gameState.DidChangePhases()) return;

        if (gameState.PrevState.GamePhase is GameplayTest.GamePhase.PrepStockStore &&
            gameState.NewState.GamePhase is GameplayTest.GamePhase.InitialTurnMenu)
        {
            var selectedStore = gameState.NewState.BoardNodes[_boardNode.Id].StorefrontData;
            UpdateStockVisual(selectedStore);
        }

        if (gameState.BuildStoreCutsceneStarted())
        {
            SetStoreColor(gameState.NewState.CurrentPlayer());
        }
    }

    public void Initialize(BoardNode bn)
    {
        _boardNode = bn;
        //_boardNode.BoardNodeData.StorefrontData = new StorefrontData(ps);
        //storefrontColor = ps.EntityData.MainColor;
        //UpdateStoreVisual();
    }

    private void SetStoreColor(PlayerState ps)
    {
        storefrontColor = ps.EntityData.MainColor;
    }

    #region Additional Functions
    public void UpdateStoreVisual()
    {
        ref var sd = ref _boardNode.BoardNodeData.StorefrontData;

        _boardNode.nodeVisual.color = storefrontColor;

        //storefrontBoatVisual.enabled = true;
        storefrontBoatVisual.color = storefrontColor;

        //stockGroup.alpha = 1;
        stockGroup.GetComponent<Image>().color = storefrontColor;
        stockGroup.GetComponent<Image>().color -= new Color(0, 0, 0, .25f);

        storeLevelIndicator.text = "";
        for (int i = 0; i < sd.StoreLevel; i++)
        {
            storeLevelIndicator.text += "*";
        }
    }

    private void UpdateStockVisual(StorefrontData sd)
    {
        //var sd = _boardNode.BoardNodeData.StorefrontData;

        for(int i = 0; i < StorefrontData.STORESTOCK_CAP; i++)
        {
            if (sd.Storestock[i] is null) 
            {
                stockItems[i].gameObject.SetActive(false);
                //stockItems[i].enabled = false;
            }
            else
            {
                //Debug.Log($"{sd.Storestock[i]}");
                //stockItems[i].enabled = true;
                stockItems[i].gameObject.SetActive(true);
                stockItems[i].sprite = sd.Storestock[i].itemSprite;
            }
        }

        if (sd.IsEmpty())
        {
            soldoutText.text = "SOLDOUT";
        }
        else
        {
            soldoutText.text = "";
        }
    }

    public void ShowStoreVisual()
    {
        storeLevelIndicator.enabled = true;
        storefrontBoatVisual.enabled = true;
        stockGroup.alpha = 1;
    }

    public void HideStoreVisual()
    {
        storeLevelIndicator.enabled = false;
        storefrontBoatVisual.enabled = false;
        stockGroup.alpha = 0;
    }

    public void CopyStore(StorefrontData oldSd)
    {
        ref var sd = ref _boardNode.BoardNodeData.StorefrontData;

        storeLevelIndicator.text = "";

        for (int i = 0; i < sd.StoreLevel; i++)
        {
            storeLevelIndicator.text += "*";
        }

        sd.OwnerId = oldSd.OwnerId;
        sd.Storestock = oldSd.Storestock;

        sd.StorePriceMultiplier = 1 + oldSd.StoreBaseMult * (oldSd.StoreLevel - 1);
    }

    public void LevelUpStore()
    {
        ref var sd = ref _boardNode.BoardNodeData.StorefrontData;

        sd.StoreLevel += 1;
        sd.StorePriceMultiplier = 1 + sd.StoreBaseMult * (sd.StoreLevel - 1);

        storeLevelIndicator.text += "*";
    }
    /*
    public void BuyItem(EntityPiece buyer, ItemStats item, int index)
    {
        Debug.Log("StoreManager | BuyItem");
        int finalItemPrice = (int)(item.basePrice * storePriceMultiplier);
        storeHeldCoins += finalItemPrice;

        buyer.heldPoints -= finalItemPrice;
        buyer.ReputationPoints += 20 + (finalItemPrice / 10);
        buyer.inventory.Add(item);
        playerOwner.CalculateStorestockTotal();
        // subtract value of item from buyer buyer.heldPoints
        // add value of item to playerOwner
        playerOwner.heldPoints += finalItemPrice;

        // Remove from store inventory
        _storefrontData.Storestock[index] = null;
    }*/

    public void AddItem(ItemStats item)
    {
        //_storefrontData.Storestock
    }
    #endregion
}
