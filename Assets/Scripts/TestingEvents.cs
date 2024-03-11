using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingEvents : MonoBehaviour
{
    public ItemStats testItem;
    [Header("Broadcast on Event Channel")]
    [SerializeField] private VoidEventChannelSO voidBroadcastChannel1;
    [SerializeField] private VoidEventChannelSO voidBroadcastChannel2;
    [SerializeField] private ItemEventChannelSO itemBroadcastChannel1;

    [SerializeField] private ItemEventChannelSO itemBoughtChannel;
    [SerializeField] private IntEventChannelSO removeItemBroadcastChannel;
    [SerializeField] private IntEventChannelSO buyingItemBroadcastChannel;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            RaiseChannelVoid(voidBroadcastChannel1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            RaiseChannelVoid(voidBroadcastChannel2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            RaiseChannelItem(itemBroadcastChannel1, testItem);
        }
    }

    public void RaiseChannelVoid(VoidEventChannelSO voidEvent)
    {
        voidEvent.RaiseEvent();
    }

    public void RaiseChannelItem(ItemEventChannelSO itemEvent, ItemStats item)
    {
        itemEvent.RaiseEvent(item);
    }

    public void RaiseChannelItem(ItemStats item)
    {
        itemBroadcastChannel1.RaiseEvent(item);
    }

    public void RaiseChannelItemList(ItemListEventChannelSO itemListEvent, List<ItemStats> items)
    {
        itemListEvent.RaiseEvent(items);
    }

    public void RaiseBuyItem(ItemStats item)
    {
        // Upon buying from store, it should:
        //  Remove item sprite visually in the store
        //  Make the item null in the list position
        //  Add that item into the player's inventory
        //  Deduct currency based on item's price
        Debug.Log("This " + item + " has been bought!");

        // Tell listeners that this item is bought
        itemBoughtChannel.RaiseEvent(item);
    }
    public void RaiseBuyItemAt(int i)
    {
        // Upon buying from store, it should:
        //  Remove item sprite visually in the store
        //  Make the item null in the list position
        //  Add that item into the player's inventory
        //  Deduct currency based on item's price

        // Tell listeners that the item at this i position is being bought 
        buyingItemBroadcastChannel.RaiseEvent(i);
    }

    public void RaiseRemoveItem(int i)
    {
        Debug.Log("YO REMOVE THAT ITEM AT " + i);
        removeItemBroadcastChannel.RaiseEvent(i);
    }
}
