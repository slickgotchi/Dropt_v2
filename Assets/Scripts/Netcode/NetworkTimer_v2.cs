using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkTimer_v2 : MonoBehaviour
{
    public static NetworkTimer_v2 Instance;

    public float TickInterval = 0.1f;

    [HideInInspector] public int TickCurrent;
    [HideInInspector] public float TickFraction;

    private float m_timer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        TickCurrent = 0;
        TickFraction = 0;
        m_timer = 0;
    }

    public void Update(float dt)
    {
        // update timers
        m_timer += dt;

        // check for tick time
        int loopBreakCount = 0;
        while (m_timer >= TickInterval && loopBreakCount < 10)
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

    private void HandleTickFunctions()
    {
        // insert all functions that need to be ticked throughout the codebase
        HandleTick_PlayerPrediction();
    }

    private void HandleTick_PlayerPrediction()
    {
        var playerPredictions = FindObjectsByType<PlayerPrediction>(FindObjectsSortMode.None);
        foreach (var playerPrediction in playerPredictions)
        {
            playerPrediction.Tick();
        }
    }
}
