using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputPreparation : MonoBehaviour
{
    PlayerInputManager manager;
    // Start is called before the first frame update
    void Start()
    {
        manager = GetComponent<PlayerInputManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
