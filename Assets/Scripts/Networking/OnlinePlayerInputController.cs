using HeathenEngineering.SteamworksIntegration;
using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;

// Inherit from NetworkBehaviour instead of MonoBehaviour
public class OnlinePlayerInputController : NetworkIdentity
{
    [SerializeField] private NetworkIdentity _networkIdentity;

    protected override void OnSpawned(bool asServer)
    {
        base.OnSpawned(asServer);

        if (!asServer) return;


    }
}