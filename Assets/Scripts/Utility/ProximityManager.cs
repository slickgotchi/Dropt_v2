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

    // handle agent enable/disable based on distance for large levels
    float k_updateInterval = 0.1f;
    float m_updateTimer = 0f;
    void HandleProximityChecks()
    {
        m_updateTimer -= Time.deltaTime;
        if (m_updateTimer > 0) return;
        m_updateTimer = k_updateInterval;

        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        var culledObjects = FindObjectsByType<ProximityCulling>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        int culledObjectCount = culledObjects.Length;
        int numPlayers = players.Length;
        for (int i = 0; i < culledObjectCount; i++)
        {
            var culledObject = culledObjects[i];
            var culledObjectPosition = culledObject.transform.position;

            // don't activate/deactivate if not spawned
            var networkObject = culledObject.GetComponent<NetworkObject>();
            if (networkObject != null && !networkObject.IsSpawned) continue;

            // if culling is off, don't cull
            if (!culledObject.IsCulled) continue;

            // check for pooled objects that are not in use
            var pooledObject = networkObject.GetComponent<PooledObject>();
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
}
