using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Dropt;
using Unity.Mathematics;

public class NetworkTimer_v2 : NetworkBehaviour
{
    public static NetworkTimer_v2 Instance;

    [Tooltip("WARNING: The TickRate here must be the same for the NetworkTimer_v2, NetworkManager AND the physics interval in Edit > Project Settings > Time (1 / TickRate)")]
    public float TickRate = 10f;
    [HideInInspector] public float TickInterval = 0.1f;

    [HideInInspector] public int TickCurrent;
    [HideInInspector] public float TickFraction;

    public int DroptNetworkTransformInterpolationDelayTicks = 6;

    public int ClientServerTickDelta = 0;

    private List<int> m_clientServerTickDeltas = new List<int>();

    private float m_timer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        TickInterval = 1 / TickRate;

        TickCurrent = 0;
        TickFraction = 0;
        m_timer = 0;

        TickRate = 1 / TickInterval;

        DroptNetworkTransformInterpolationDelayTicks = 6;
    }

    private void Update()
    {
        // update timers
        m_timer += Time.deltaTime;

        // check for tick time
        int loopBreakCount = 0;
        while (m_timer > TickInterval && loopBreakCount < 10)
        {
            // update timer and tick
            m_timer -= TickInterval;
            TickCurrent++;

            // handle all the functions that need to be called each tick in our codebase
            HandleTickFunctions();

            CheckTickDelta();

            // update the loopBreakerCount (to prevent getting stuck in any loops)
            loopBreakCount++;
        }

        // calc the new tick fraction
        TickFraction = m_timer / TickInterval;
    }

    void CheckTickDelta()
    {
        if (IsClient)
        {
            GetServerTickServerRpc(GetComponent<NetworkObject>().NetworkObjectId);
        }
    }

    [Rpc(SendTo.Server)]
    void GetServerTickServerRpc(ulong networkObjectId)
    {
        SendServerTickClientRpc(networkObjectId, TickCurrent);
    }

    [Rpc(SendTo.NotServer)]
    void SendServerTickClientRpc(ulong networkObjectId, int serverTick)
    {
        if (GetComponent<NetworkObject>().NetworkObjectId != networkObjectId) return;

        var newTickDelta = TickCurrent - serverTick;
        m_clientServerTickDeltas.Add(newTickDelta);
        if (m_clientServerTickDeltas.Count > 10) m_clientServerTickDeltas.RemoveAt(0);

        // get average
        float sum = 0;
        foreach (var delta in m_clientServerTickDeltas) sum += delta;

        //Debug.Log("new ClientServerTickDelta: " + newTickDelta + ", serverTick: " + serverTick + ", clientTick: " + TickCurrent);

        // if new remote client tick delta is 5 or more different from the old, replace the old delta
        int newClientServerTickDelta = (int)math.round(sum / m_clientServerTickDeltas.Count);
        if (math.abs(ClientServerTickDelta - newClientServerTickDelta) > 5)
        {
            ClientServerTickDelta = newClientServerTickDelta;
            Debug.Log("Set new ClientServerTickDelta: " + ClientServerTickDelta);
        }
    }

    public float GetTime()
    {
        return (TickCurrent + TickFraction) * TickInterval; 
    }

    private void HandleTickFunctions()
    {
        // insert all functions that need to be ticked throughout the codebase
        HandleTick_PlayerPrediction();
        HandleTick_PositionBuffers();
        HandleTick_DroptNetworkTransforms();
    }

    private void HandleTick_PlayerPrediction()
    {
        if (Game.Instance == null) return;

        var players = Game.Instance.playerControllers;
        foreach (var player in players)
        {
            player.GetComponent<PlayerPrediction>().Tick();
        }
    }

    private void HandleTick_PositionBuffers()
    {
        if (Game.Instance == null) return;

        var enemies = Game.Instance.enemyControllers;
        foreach (var enemyController in enemies)
        {
            var posBuffer = enemyController.GetComponent<PositionBuffer>();
            if (posBuffer != null)
            {
                posBuffer.Tick();
            }
        }
    }

    private void HandleTick_DroptNetworkTransforms()
    {
        if (Game.Instance == null) return;

        var enemies = Game.Instance.enemyControllers;
        foreach (var enemyController in enemies)
        {
            var droptNetworkTransform = enemyController.GetComponent<DroptNetworkTransform>();
            if (droptNetworkTransform != null)
            {
                droptNetworkTransform.Tick();
            }
        }
    }
}
