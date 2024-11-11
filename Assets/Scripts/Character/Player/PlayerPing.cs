using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Transports.UTP;

public class PlayerPing : NetworkBehaviour
{
    // ping tracking
    private float m_pingInterval = 0.1f;
    private float m_pingTimer = 0f;
    private List<float> m_pingBuffer = new List<float>();
    private int m_pingBufferSize = 100;

    public float pingLive;
    public float pingMedian;
    public float pingMean;
    public float pingHigh;
    public float pingLow;

    //UnityTransport m_transport;

    NetworkObject m_networkObject;

    public NetworkVariable<ulong> RTT = new NetworkVariable<ulong>(0);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        m_networkObject = GetComponent<NetworkObject>();

        //m_transport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
    }

    private void Update()
    {
        if (IsServer)
        {
            //RTT.Value = m_transport.GetCurrentRtt(m_networkObject.OwnerClientId);
        }

        if (IsClient)
        {
            TrackPing();
            DebugCanvas.Instance.SetPing((int)pingMedian);
            SetRTTServerRpc((int)pingMedian);
        }
    }

    [Rpc(SendTo.Server)]
    void SetRTTServerRpc(int ping)
    {
        RTT.Value = (ulong)ping;
    }

    void TrackPing()
    {
        if (!IsClient) return;

        m_pingTimer -= Time.deltaTime;
        if (m_pingTimer > 0) return;
        m_pingTimer = m_pingInterval;

        PingServerRpc(Time.time);
    }

    [Rpc(SendTo.Server)]
    void PingServerRpc(float clientTime)
    {
        PingClientRpc(clientTime);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void PingClientRpc(float originalClientTime)
    {
        var deltaTime = (Time.time - originalClientTime) * 1000;
        m_pingBuffer.Add(deltaTime);
        if (m_pingBuffer.Count > m_pingBufferSize)
        {
            m_pingBuffer.RemoveAt(0);
        }

        pingLive = deltaTime;
        pingMean = MathExtensions.CalculateMean(m_pingBuffer);
        pingMedian = MathExtensions.CalculateMedian(m_pingBuffer);

        if (pingLive < pingLow) pingLow = pingLive;
        if (pingLive > pingHigh) pingHigh = pingLive;
    }
}
