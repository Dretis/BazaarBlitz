using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingEvents : MonoBehaviour
{
    [Header("Broadcast on Event Channel")]
    [SerializeField] private VoidEventChannelSO testBroadcastChannel1;
    [SerializeField] private VoidEventChannelSO testBroadcastChannel2;
    [SerializeField] private VoidEventChannelSO testBroadcastChannel3;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            testBroadcastChannel1.RaiseEvent();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            testBroadcastChannel2.RaiseEvent();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            testBroadcastChannel3.RaiseEvent();
        }
    }
}
