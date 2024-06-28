using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerPing : NetworkBehaviour
{
    public float Ping = 0;

    public override void OnNetworkSpawn()
    {
        if (IsLocalPlayer && !IsHost)
        {
            PingServerRpc(Time.realtimeSinceStartup);
        }
    }

    [Rpc(SendTo.Server)]
    void PingServerRpc(float prevTime)
    {
        PingClientRpc(prevTime);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void PingClientRpc(float prevTime)
    {
        if (IsLocalPlayer)
        {
            float currTime = Time.realtimeSinceStartup;
            Ping = currTime - prevTime;
            DebugCanvas.Instance.SetPing((int)(Ping * 100));
            PingServerRpc(Time.realtimeSinceStartup);
        }
    }
}
