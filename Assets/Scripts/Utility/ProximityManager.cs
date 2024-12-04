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
        var levelObjects = FindObjectsByType<DestroyAtLevelChange>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        int numLevelObjects = levelObjects.Length;
        int numPlayers = players.Length;
        for (int i = 0; i < numLevelObjects; i++)
        {
            var levelObject = levelObjects[i];
            var levelObjectPos = levelObject.transform.position;

            if (levelObject.HasComponent<Dropt.EnemyAI_RoamShade>()) continue;

            // also ensure we don't deactivate anything that has not yet spawned
            if (!levelObject.HasComponent<NetworkObject>()) continue;
            if (!levelObject.GetComponent<NetworkObject>().IsSpawned) continue;

            // don't deactivate grid maps
            if (levelObject.HasComponent<Grid>()) continue;

            // continue if component ignores proximity
            if (levelObject.HasComponent<IgnoreProximity>()) continue;

            bool inRange = false;

            for (int j = 0; j < numPlayers; j++)
            {
                var player = players[j];
                var playerPos = player.transform.position;

                float xLow = playerPos.x - k_xDist;
                float xHigh = playerPos.x + k_xDist;
                float yLow = playerPos.y - k_yDist;
                float yHigh = playerPos.y + k_yDist;

                if (levelObjectPos.x > xLow && levelObjectPos.x < xHigh && levelObjectPos.y > yLow && levelObjectPos.y < yHigh)
                {
                    inRange = true;
                }
            }


            // set enabled state
            if (inRange)
            {
                levelObject.gameObject.SetActive(true);
            }
            else
            {
                levelObject.gameObject.SetActive(false);
            }
        }

    }
}
