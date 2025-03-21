using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Dropt;
using Unity.Mathematics;
using System;

public class NetworkTimer_v2 : NetworkBehaviour
{
    public static NetworkTimer_v2 Instance;

    [Tooltip("WARNING: The TickRate here must be the same for the NetworkTimer_v2, NetworkManager AND the physics interval in Edit > Project Settings > Time (1 / TickRate)")]
    public float TickRate = 10f;
    [HideInInspector] public float TickInterval_s = 0.1f;

    [HideInInspector] public int TickCurrent;
    [HideInInspector] public float TickFraction;

    [HideInInspector] public int DroptNetworkTransformInterpolationDelayTicks = 6;

    public int ClientServerTickDelta = 0;
    public int InterpolationLagTicks = 5;

    private List<int> m_clientServerTickDeltas = new List<int>();

    private float m_timer;

    public event Action OnTick;

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
        TickInterval_s = 1 / TickRate;

        TickCurrent = 0;
        TickFraction = 0;
        m_timer = 0;

        TickRate = 1 / TickInterval_s;

        DroptNetworkTransformInterpolationDelayTicks = 6;
    }

    private void Update()
    {
        // update timers
        m_timer += Time.deltaTime;

        // check for tick time
        int loopBreakCount = 0;
        while (m_timer > TickInterval_s && loopBreakCount < 10)
        {
            // update timer and tick
            m_timer -= TickInterval_s;
            TickCurrent++;

            // tell all isteners a tick happened
            OnTick?.Invoke();

            // handle all the functions that need to be called each tick in our codebase
            HandleTickFunctions();

            CheckTickDelta();

            // update the loopBreakerCount (to prevent getting stuck in any loops)
            loopBreakCount++;
        }

        // calc the new tick fraction
        TickFraction = m_timer / TickInterval_s;
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

    [Rpc(SendTo.ClientsAndHost)]
    void SendServerTickClientRpc(ulong networkObjectId, int serverTick)
    {
        if (GetComponent<NetworkObject>().NetworkObjectId != networkObjectId) return;

        var newTickDelta = TickCurrent - serverTick;
        m_clientServerTickDeltas.Add(newTickDelta);
        if (m_clientServerTickDeltas.Count > 10) m_clientServerTickDeltas.RemoveAt(0);

        //Debug.Log("Client Tick: " + NetworkTimer_v2.Instance.TickCurrent +
        //    ", Server Tick: " + serverTick);

        // get average
        float sum = 0;
        foreach (var delta in m_clientServerTickDeltas) sum += delta;

        // if new remote client tick delta is 5 or more different from the old, replace the old delta
        int newClientServerTickDelta = (int)math.round(sum / m_clientServerTickDeltas.Count);
        if (math.abs(ClientServerTickDelta - newClientServerTickDelta) > 5)
        {
            ClientServerTickDelta = newClientServerTickDelta;
        }
    }

    public float GetTime()
    {
        return (TickCurrent + TickFraction) * TickInterval_s; 
    }

    private void HandleTickFunctions()
    {
        // insert all functions that need to be ticked throughout the codebase
        //HandleTick_PlayerPrediction();
        HandleTick_PositionBuffers();
        HandleTick_DroptNetworkTransforms();
        // HandleTick_PerfectLagCompensation();
    }

    //private void HandleTick_PlayerPrediction()
    //{
    //    if (Game.Instance == null) return;

    //    var players = Game.Instance.playerControllers;
    //    foreach (var player in players)
    //    {
    //        player.GetComponent<PlayerPrediction>().Tick();
    //    }
    //}

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

    // private void HandleTick_PerfectLagCompensation()
    // {
    //     if (Game.Instance == null) return;

    //     var enemies = Game.Instance.enemyControllers;
    //     foreach (var enemyController in enemies)
    //     {
    //         var perfLagComp = enemyController.GetComponent<PerfectLagCompensation>();
    //         if (perfLagComp != null)
    //         {
    //             perfLagComp.Tick();
    //         }
    //     }
    // }
}
