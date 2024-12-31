using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Dropt;
using Unity.Mathematics;

public class NetworkTimer_v2 : MonoBehaviour
{
    public static NetworkTimer_v2 Instance;

    public float TickRate = 10f;
    [HideInInspector] public float TickInterval = 0.1f;

    [HideInInspector] public int TickCurrent;
    [HideInInspector] public float TickFraction;

    private float m_timer;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of the AudioManager exists
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

            // update the loopBreakerCount (to prevent getting stuck in any loops)
            loopBreakCount++;
        }

        // calc the new tick fraction
        TickFraction = m_timer / TickInterval;
    }

    public float GetTime()
    {
        return (TickCurrent + TickFraction) * TickInterval; 
    }

    private void HandleTickFunctions()
    {
        // insert all functions that need to be ticked throughout the codebase
        HandleTick_PlayerPrediction();
        HandleTick_CustomNetworkTransforms();
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

    private void HandleTick_CustomNetworkTransforms()
    {
        if (Game.Instance == null) return;

        var enemies = Game.Instance.enemyControllers;
        foreach (var enemy in enemies)
        {
            var customNetworkTransform = enemy.GetComponent<CustomNetworkTransform>();
            if (customNetworkTransform != null)
            {
                customNetworkTransform.Tick();
            }
        }
    }
}
