using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
    public List<ItemStats> storeInventory;

    private EntityPiece playerOwner;
    private int storeCapacity = 4;

    private void Awake()
    {
        playerOwner = GetComponent<MapNode>().playerOccupied;

        storeInventory = Enumerable.Repeat<ItemStats>(null, storeCapacity).ToList();
    }

    public void BuyItem(EntityPiece buyer, ItemStats item, int index)
    {
        // Check if buyer has enough money to buy items
        // Subject to change - inventory system
        buyer.combatStats.inventory.Add(item);
        // subtract value of item from buyer buyer.heldPoints
        // add value of item to playerOwner

        // Remove from store inventory
        storeInventory[index] = null;
    }

    public void SellItem(EntityPiece seller, ItemStats item)
    {
        // Check if store has enough money to buy items
        int indexToAdd = storeInventory.FindIndex(x => x == null);

        if (indexToAdd != -1) 
        {
            // Remove item from seller's inventory and add it to store's inventory.
            seller.combatStats.inventory.Remove(item);
            storeInventory[indexToAdd] = item;
            // subtract value of item from playerOwner
            // add value of item to seller
        }
        else
        {
            Debug.Log("Inventory is full.");
        }      
    }
}
