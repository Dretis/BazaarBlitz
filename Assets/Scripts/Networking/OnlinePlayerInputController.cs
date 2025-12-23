using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.InputSystem;

// Inherit from NetworkBehaviour instead of MonoBehaviour
public class OnlinePlayerInputController : NetworkBehaviour
{
    public static OnlinePlayerInputController Instance { get; private set; }
    [SerializeField] private PlayerInputController pic;
    
    public override void OnStartClient()
    {
        if (!IsOwner) return;

        GetComponent<PlayerInput>().enabled = true;
        Instance = this;
    }
}