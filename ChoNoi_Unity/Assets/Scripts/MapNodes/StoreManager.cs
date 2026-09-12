using System.Collections.Generic;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
    private MapNode node;
    public List<ItemStats> storeInventory;

    public EntityPiece playerOwner;
    private int storeCapacity = 3;

    [Header("Store Stats")]
    public int storeLevel = 1;
    public float storePriceMultiplier = 1;
    public float storeBaseMult = 0.2f;
    public int storeHeldCoins = 0;
    
    private void Awake()
    {
        //playerOwner = GetComponent<MapNode>().playerOccupied;
        node = GetComponent<MapNode>();

        storeInventory = new List<ItemStats>();
        node.storeLevelIndicator.text = "*";

        for (int i = 0; i < storeCapacity; i++)
        {
            storeInventory.Add(null);
        }
        // storeInventory = Enumerable.Repeat<ItemStats>(null, storeCapacity).ToList();
    }

    public void LevelUpStore()
    {
        storeLevel += 1;
        node.storeLevelIndicator.text += "*";
        //storePriceMultiplier += 0.2f;
        storePriceMultiplier = 1 + storeBaseMult * storeLevel;
    }

    public void EnhanceStore()
    {
        // make the star color cool
        storeBaseMult = 0.3f;
        storePriceMultiplier = storeBaseMult * storeLevel;
    }

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
        storeInventory[index] = null;
    }

    public void AddItem(ItemStats item)
    {
        // Check if store has enough money to buy items
        int indexToAdd = storeInventory.FindIndex(x => x == null);

        if (indexToAdd != -1) 
        {
            storeInventory[indexToAdd] = item;
        }
        else
        {
            Debug.Log("Inventory is full.");
        }      
    }
}
