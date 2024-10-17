using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Transports.UTP;

public class PlayerPing : NetworkBehaviour
{
    //public float Ping = 0;

    //float m_timer = 0;

    UnityTransport m_transport;

    NetworkObject m_networkObject;

    public NetworkVariable<ulong> RTT = new NetworkVariable<ulong>(0);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        m_networkObject = GetComponent<NetworkObject>();

        m_transport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;

        //if (IsLocalPlayer && !IsHost)
        //{
        //    PingServerRpc(m_timer);
        //}
    }

    private void Update()
    {
        //m_timer += Time.deltaTime;

        if (IsServer)
        {
            RTT.Value = m_transport.GetCurrentRtt(m_networkObject.OwnerClientId);
        }

        if (IsClient)
        {
            DebugCanvas.Instance.SetPing((int)RTT.Value);
        }
    }


    //[Rpc(SendTo.Server)]
    //void PingServerRpc(float prevTime)
    //{
    //    PingClientRpc(prevTime);
    //}

    //[Rpc(SendTo.ClientsAndHost)]
    //void PingClientRpc(float prevTime)
    //{
    //    if (IsLocalPlayer)
    //    {
    //        float currTime = m_timer;
    //        Ping = currTime - prevTime;
    //        //DebugCanvas.Instance.SetPing((int)(Ping * 1000));
    //        DebugCanvas.Instance.SetPing((int)RTT.Value * 2);
    //        PingServerRpc(m_timer);
    //    }
    //}
}
