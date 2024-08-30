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

    //float k_xDist = 16f;
    //float k_yDist = 10f;

    private void Awake()
    {
        Instance = this;
        World = GetComponent<UtilityWorldController>();
    }

    private void Update()
    {
        //HandleAgentEnableDisable();
    }

    //// handle agent enable/disable based on distance for large levels
    //float enableDisableInterval = 0.1f;
    //float enableDisableTimer = 0f;
    //void HandleAgentEnableDisable()
    //{
    //    enableDisableTimer -= Time.deltaTime;
    //    if (enableDisableTimer > 0) return;
    //    enableDisableTimer = enableDisableInterval;

    //    var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
    //    var enemies = FindObjectsByType<EnemyController>(FindObjectsInactive.Include, FindObjectsSortMode.None);

    //    //float activationDistanceSq = 20 * 20;

        

    //    int numEnemies = enemies.Length;
    //    int numPlayers = players.Length;
    //    for (int i = 0; i < numEnemies; i++)
    //    {
    //        var enemy = enemies[i];
    //        var enemyPos = enemy.transform.position;

    //        // also ensure we don't deactivate anything that has not yet spawned
    //        if (!enemy.HasComponent<NetworkObject>()) continue;
    //        if (!enemy.GetComponent<NetworkObject>().IsSpawned) continue;

    //        bool inRange = false;

    //        for (int j = 0; j < numPlayers; j++)
    //        {
    //            var player = players[j];
    //            var playerPos = player.transform.position;

    //            float xLow = playerPos.x - k_xDist;
    //            float xHigh = playerPos.x + k_xDist;
    //            float yLow = playerPos.y - k_yDist;
    //            float yHigh = playerPos.y + k_yDist;

    //            if (enemyPos.x > xLow && enemyPos.x < xHigh && enemyPos.y > yLow && enemyPos.y < yHigh)
    //            {
    //                inRange = true;
    //            }

    //            //var distSq = math.distancesq(enemy.transform.position,
    //            //    player.transform.position);
    //            //if (distSq < activationDistanceSq)
    //            //{
    //            //    inRange = true;
    //            //}
    //        }


    //        // set enabled state
    //        if (inRange)
    //        {
    //            enemy.gameObject.SetActive(true);
    //        }
    //        else
    //        {
    //            enemy.gameObject.SetActive(false);
    //        }
    //    }

    //}
}
