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

    UnityTransport m_unityTransport;

    NetworkObject m_networkObject;

    public NetworkVariable<ulong> RTT_ms = new NetworkVariable<ulong>(0);

    public NetworkVariable<int> serverFPS = new NetworkVariable<int>(0);
    private List<float> m_serverFPSArray = new List<float>();
    private int m_maxServerFPSArrayLength = 50;

    public float elapsedTimeSinceLastPing = 0;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        m_networkObject = GetComponent<NetworkObject>();
        m_unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();

    }

    private void Update()
    {
        if (IsHost) return;

        if (IsServer)
        {
            //if (m_unityTransport != null) RTT_ms.Value = m_unityTransport.GetCurrentRtt(m_networkObject.OwnerClientId);
            //Debug.Log("RTT_ms: " + RTT_ms.Value);

            m_serverFPSArray.Add(1 / Time.deltaTime);
            if (m_serverFPSArray.Count > m_maxServerFPSArrayLength)
            {
                m_serverFPSArray.RemoveAt(0);
            }

            // find average fps
            float sum = 0;
            for (int i = 0; i < m_serverFPSArray.Count; i++)
            {
                sum += m_serverFPSArray[i];
            }

            if (m_serverFPSArray.Count > 0)
            {
                serverFPS.Value = (int)(sum/m_serverFPSArray.Count);
            }
            
        }

        if (IsLocalPlayer)
        {
            TrackPing();
            DebugCanvas.Instance.SetPing((int)pingMedian);
            DebugCanvas.Instance.SetServerFPS(((int)serverFPS.Value));
            UpdateServerRTT_msServerRpc((ulong)pingMedian);
        }

        elapsedTimeSinceLastPing += Time.deltaTime;
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
    void UpdateServerRTT_msServerRpc(ulong rtt_ms)
    {
        RTT_ms.Value = rtt_ms;
    }

    [Rpc(SendTo.Server)]
    void PingServerRpc(float clientTime)
    {
        ulong clientId = Dropt.Utils.Network.GetClientIdFromPlayer(
            NetworkManager.Singleton, m_networkObject);

        // send ping response back to only the client for this player
        PingClientRpc(clientTime, RpcTarget.Single(clientId, RpcTargetUse.Persistent));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void PingClientRpc(float originalClientTime, RpcParams rpcParams = default)
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

        elapsedTimeSinceLastPing = 0;
    }
}
