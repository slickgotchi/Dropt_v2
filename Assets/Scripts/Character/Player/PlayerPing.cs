using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using Unity.Mathematics;

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

    private List<int> m_clientServerTickDeltas = new List<int>();

    //public int ClientServerTickDelta;

    public int Client_LocalTick;
    public int Client_ReceivedServerTick;
    public int Client_ClientLocalServerReceived_TickDelta;

    public int Server_LocalTick;
    public int Server_ReportingClientTick;
    public int Server_ClientReportingServerReceived_TickDelta;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        m_networkObject = GetComponent<NetworkObject>();
        m_unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        NetworkTimer_v2.Instance.OnTick += Tick;
    }

    public override void OnNetworkDespawn()
    {
        NetworkTimer_v2.Instance.OnTick -= Tick;

        base.OnNetworkDespawn();
    }

    void Tick()
    {
        if (IsServer)
        {
            Server_LocalTick = NetworkTimer_v2.Instance.TickCurrent;
            SendServerTickToClientRpc(Server_LocalTick);

            ServerTickRTTClientRpc(Server_LocalTick);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ServerTickRTTClientRpc(int serverTick)
    {
        if (IsLocalPlayer)
        {
            ServerTickRTTServerRpc(serverTick);
        }
    }

    [Rpc(SendTo.Server)]
    void ServerTickRTTServerRpc(int ogServerTick)
    {
        //Debug.Log("OG serverTick: " + ogServerTick +
        //    ", Current serverTick" + NetworkTimer_v2.Instance.TickCurrent +
        //    ", tick delta: " + (NetworkTimer_v2.Instance.TickCurrent - ogServerTick));
    }

    [Rpc(SendTo.ClientsAndHost)]
    void SendServerTickToClientRpc(int serverTick)
    {
        if (IsLocalPlayer)
        {
            Client_LocalTick = NetworkTimer_v2.Instance.TickCurrent;
            Client_ReceivedServerTick = serverTick;

            CompareClientTickServerRpc(Client_ReceivedServerTick, Client_LocalTick);

            // update the clients version of the client/server tick delta
            UpdateClientServerTickDelta(Client_ReceivedServerTick, Client_LocalTick);
        }
    }

    [Rpc(SendTo.Server)]
    void CompareClientTickServerRpc(int serverTick, int clientTick)
    {
        // update the servers version of the client/server tick delta
        UpdateClientServerTickDelta(serverTick, clientTick);
    }

    void UpdateClientServerTickDelta(int serverTick, int clientTick)
    {
        m_clientServerTickDeltas.Add(clientTick - serverTick);
        if (m_clientServerTickDeltas.Count > 20) m_clientServerTickDeltas.RemoveAt(0);

        // get average
        float sum = 0;
        foreach (var delta in m_clientServerTickDeltas) sum += delta;

        // if new remote client tick delta is 5 or more different from the old, replace the old delta
        int newClientServerTickDelta = (int)math.round(
            sum / m_clientServerTickDeltas.Count);

        if (IsServer)
        {
            if (math.abs(Server_ClientReportingServerReceived_TickDelta -
                newClientServerTickDelta) > 5)
            {
                Server_ClientReportingServerReceived_TickDelta =
                    newClientServerTickDelta;
            }
        }
        else if (IsClient)
        {
            if (math.abs(Client_ClientLocalServerReceived_TickDelta -
                newClientServerTickDelta) > 5)
            {
                Client_ClientLocalServerReceived_TickDelta = newClientServerTickDelta;
            }
        }
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
