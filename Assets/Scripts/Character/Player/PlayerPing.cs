using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerPing : NetworkBehaviour
{
    public float Ping = 0;

    float m_timer = 0;

    private void Update()
    {
        m_timer += Time.deltaTime;
    }

    public override void OnNetworkSpawn()
    {
        if (IsLocalPlayer && !IsHost)
        {
            PingServerRpc(m_timer);
            Debug.Log("Ping: " + m_timer);
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
            float currTime = m_timer;
            Ping = currTime - prevTime;
            DebugCanvas.Instance.SetPing((int)(Ping * 1000));
            PingServerRpc(m_timer);
            Debug.Log("Pong: " + m_timer);
        }
    }
}
