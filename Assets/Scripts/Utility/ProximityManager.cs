using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Tilemaps;

public class ProximityManager : MonoBehaviour
{
    public static ProximityManager Instance { get; private set; }

    float k_xDist = 16*1.2f;
    float k_yDist = 10f*1.2f;

    // handle agent enable/disable based on distance for large levels
    float k_updateInterval = 0.2f;
    float m_updateTimer = 0f;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of the AudioManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        HandleProximityChecks();
    }

    void HandleProximityChecks()
    {
        m_updateTimer -= Time.deltaTime;
        if (m_updateTimer > 0) return;
        m_updateTimer = k_updateInterval;

        var players = Game.Instance.playerControllers;
        var culledObjects = ProximityCulling.culledObjects;

        int numPlayers = players.Count;
        int culledObjectCount = culledObjects.Count;

        // Temporary hashset to track already activated objects
        HashSet<ProximityCulling> activatedObjects = new HashSet<ProximityCulling>();

        // Loop through players first
        for (int j = 0; j < numPlayers; j++)
        {
            var player = players[j];
            var playerPos = player.transform.position;

            float xLow = playerPos.x - k_xDist;
            float xHigh = playerPos.x + k_xDist;
            float yLow = playerPos.y - k_yDist;
            float yHigh = playerPos.y + k_yDist;

            // Loop through culled objects
            for (int i = 0; i < culledObjectCount; i++)
            {
                var culledObject = culledObjects[i];
                if (culledObject == null || activatedObjects.Contains(culledObject)) continue;

                var culledObjectPosition = culledObject.transform.position;

                // Check if in range
                if (culledObjectPosition.x > xLow && culledObjectPosition.x < xHigh &&
                    culledObjectPosition.y > yLow && culledObjectPosition.y < yHigh)
                {
                    culledObject.gameObject.SetActive(true);
                    activatedObjects.Add(culledObject); // Mark as activated
                }
            }
        }

        // Deactivate remaining culled objects
        for (int i = 0; i < culledObjectCount; i++)
        {
            var culledObject = culledObjects[i];
            if (culledObject == null || activatedObjects.Contains(culledObject)) continue;

            culledObject.gameObject.SetActive(false);
        }
    }

    /*
    void HandleProximityChecks()
    {
        m_updateTimer -= Time.deltaTime;
        if (m_updateTimer > 0) return;
        m_updateTimer = k_updateInterval;

        var players = Game.Instance.playerControllers;
        var culledObjects = ProximityCulling.culledObjects;

        int culledObjectCount = culledObjects.Count;
        int numPlayers = players.Count;
        for (int i = 0; i < culledObjectCount; i++)
        {
            var culledObject = culledObjects[i];
            if (culledObject == null) continue;

            var culledObjectPosition = culledObject.transform.position;

            // don't activate/deactivate if not spawned
            var networkObject = culledObject.networkObject;
            if (networkObject != null && !networkObject.IsSpawned) continue;

            // if culling is off, don't cull
            if (!culledObject.IsCulled) continue;

            // check for pooled objects that are not in use
            var pooledObject = culledObject.pooledObject;
            if (pooledObject != null && !pooledObject.IsInUse) continue;

            // check if in range
            bool inRange = false;

            for (int j = 0; j < numPlayers; j++)
            {
                var player = players[j];
                var playerPos = player.transform.position;

                float xLow = playerPos.x - k_xDist;
                float xHigh = playerPos.x + k_xDist;
                float yLow = playerPos.y - k_yDist;
                float yHigh = playerPos.y + k_yDist;

                if (culledObjectPosition.x > xLow && culledObjectPosition.x < xHigh && culledObjectPosition.y > yLow && culledObjectPosition.y < yHigh)
                {
                    inRange = true;
                }
            }


            // set enabled state
            if (inRange)
            {
                culledObject.gameObject.SetActive(true);
            }
            else
            {
                culledObject.gameObject.SetActive(false);
            }
        }

    }
    */
}
