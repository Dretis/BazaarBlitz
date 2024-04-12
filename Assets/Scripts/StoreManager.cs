using System.Collections.Generic;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
    public List<ItemStats> storeInventory;

    public EntityPiece playerOwner;
    private int storeCapacity = 3;

    private void Awake()
    {
        playerOwner = GetComponent<MapNode>().playerOccupied;

        storeInventory = new List<ItemStats>();

        for (int i = 0; i < storeCapacity; i++)
        {
            storeInventory.Add(null);
        }
        // storeInventory = Enumerable.Repeat<ItemStats>(null, storeCapacity).ToList();
    }

    public void BuyItem(EntityPiece buyer, ItemStats item, int index)
    {
        buyer.ReputationPoints += 20 + (item.basePrice / 100);
        // Check if buyer has enough money to buy items
        // Subject to change - inventory system
        buyer.inventory.Add(item);
        // subtract value of item from buyer buyer.heldPoints
        // add value of item to playerOwner

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
