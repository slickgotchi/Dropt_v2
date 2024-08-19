using CarlosLab.UtilityIntelligence;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Netcode;

public class UtilityWorldSingleton : MonoBehaviour
{
    public static UtilityWorldSingleton Instance {  get; private set; }

    public UtilityWorldController World;

    private void Awake()
    {
        Instance = this;
        World = GetComponent<UtilityWorldController>();
    }

    private void Update()
    {
        HandleAgentEnableDisable();
    }

    // handle agent enable/disable based on distance for large levels
    float enableDisableInterval = 0.1f;
    float enableDisableTimer = 0f;
    void HandleAgentEnableDisable()
    {
        enableDisableTimer -= Time.deltaTime;
        if (enableDisableTimer > 0) return;
        enableDisableTimer = enableDisableInterval;

        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        var enemies = FindObjectsByType<EnemyController>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        float activationDistanceSq = 15 * 15;

        for (int i = 0; i < enemies.Length; i++)
        {
            var enemy = enemies[i];

            // IMPORTANT: FILTER OUT PREFABS
            //if (!enemy.gameObject.scene.isLoaded) continue;

            // also ensure we don't deactivate anything that has not yet spawned
            if (!enemy.HasComponent<NetworkObject>()) continue;
            if (!enemy.GetComponent<NetworkObject>().IsSpawned) continue;

            bool inRange = false;

            for (int j = 0; j < players.Length; j++)
            {
                var player = players[j];

                //if (!player.gameObject.scene.isLoaded) continue;

                var distSq = math.distancesq(enemy.transform.position,
                    player.transform.position);
                if (distSq < activationDistanceSq)
                {
                    inRange = true;
                }
            }


            // set enabled state
            if (inRange)
            {
                enemy.gameObject.SetActive(true);
            }
            else
            {
                enemy.gameObject.SetActive(false);
            }
        }

    }
}
