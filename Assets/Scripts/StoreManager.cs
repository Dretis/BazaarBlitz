using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
    private EntityPiece playerOwner;
    public ItemStats item1;
    public ItemStats item2;
    public Dictionary<ItemStats, int> storeInventory = new Dictionary<ItemStats, int>();

    private void Awake()
    {
        playerOwner = GetComponent<MapNode>().playerOccupied;
        item1 = GameplayTest.instance.item1;
        item2 = GameplayTest.instance.item2;
        storeInventory.Add(item1, 1);
        storeInventory.Add(item2, 1);
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
