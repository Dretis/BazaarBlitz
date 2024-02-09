using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
    private EntityPiece playerOwner;
    private Dictionary<ItemStats, int> storeInventory = new Dictionary<ItemStats, int>();

    private void Awake()
    {
        playerOwner = GetComponent<MapNode>().playerOccupied;
    }

    public void BuyItem(EntityPiece buyer, ItemStats item, int quantity)
    {
        buyer.combatStats.inventory.AddRange(Enumerable.Repeat(item, quantity));
        removeFromInventory(item, quantity);
    }

    public void SellItem(EntityPiece seller, ItemStats item, int quantity)
    {
        for (int i = 0; i < quantity; i++) 
            seller.combatStats.inventory.Remove(item);
        addToInventory(item, quantity);
    }

    private void addToInventory(ItemStats item, int quantity)
    {
        if (storeInventory.ContainsKey(item))
        {
            storeInventory[item] += quantity;
        }
        else
        {
            storeInventory.Add(item, quantity);
        }
        
    }

    private void removeFromInventory(ItemStats item, int quantity)
    {
        storeInventory[item] -= quantity; 
    }
}
