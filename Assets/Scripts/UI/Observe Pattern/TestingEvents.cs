using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingEvents : MonoBehaviour
{
    public ItemStats testItem;
    [Header("Broadcast on Event Channel")]
    [SerializeField] private VoidEventChannelSO testBroadcastChannel1;
    [SerializeField] private VoidEventChannelSO testBroadcastChannel2;
    [SerializeField] private ItemEventChannelSO testBroadcastChannel3;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            RaiseChannel1();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            RaiseChannel2();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            RaiseChannelItem(testItem);
        }
    }

    public void RaiseChannel1()
    {
        testBroadcastChannel1.RaiseEvent();
    }

    public void RaiseChannel2()
    {
        testBroadcastChannel2.RaiseEvent();
    }

    public void RaiseChannelItem(ItemStats item)
    {
        testBroadcastChannel3.RaiseEvent(item);
    }
}
