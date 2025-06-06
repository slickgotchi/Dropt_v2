using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Level;

/// <summary>
/// This class iterates over all LevelSpawn objects to manage when they are active/spawned
/// </summary>
public class LevelSpawnManager : MonoBehaviour
{
    public static LevelSpawnManager Instance { get; private set; }

    private float k_updateInterval = 0.1f;
    private float m_updateTimer = 0f;

    private List<LevelSpawn> m_levelSpawns = new List<LevelSpawn>();

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
        m_updateTimer -= Time.deltaTime;
        if (m_updateTimer > 0) return;
        m_updateTimer = k_updateInterval;

        // get all levelspawns
        var levelSpawns = m_levelSpawns.ToArray();
        var numLevelSpawns = levelSpawns.Length;

        // get a list of all the active spawn Id's
        var activeLevelSpawnIds = new List<int>();
        for (int i = 0; i < numLevelSpawns; i++)
        {
            var levelSpawn = levelSpawns[i];
            if (!activeLevelSpawnIds.Contains(levelSpawn.spawnerId))
            {
                activeLevelSpawnIds.Add(levelSpawn.spawnerId);
            }
        }

        // get a list of all the level spawns that have been touched by a player
        var touchedByPlayerLevelSpawnIds = new List<int>();
        for (int i = 0; i < numLevelSpawns; i++)
        {
            var levelSpawn = levelSpawns[i];
            if (levelSpawn.isTouchedByPlayer && !touchedByPlayerLevelSpawnIds.Contains(levelSpawn.spawnerId))
            {
                touchedByPlayerLevelSpawnIds.Add(levelSpawn.spawnerId);
            }

        }

        // iterate over each
        for (int i = 0; i < numLevelSpawns; i++)
        {
            var levelSpawn = levelSpawns[i];
            if (levelSpawn.isSpawned) continue;

            bool isSpawnTime = false;

            switch (levelSpawn.spawnCondition)
            {
                case LevelSpawn.SpawnCondition.ElapsedTime:
                    levelSpawn.elapsedTime -= k_updateInterval;
                    if (levelSpawn.elapsedTime <= 0) isSpawnTime = true;
                    break;
                case LevelSpawn.SpawnCondition.PlayerDestroyAllWithSpawnerId:
                    // check if there are no spawns with the id of destroyall
                    if (!activeLevelSpawnIds.Contains(levelSpawn.destroyAllWithSpawnerId))
                    {
                        levelSpawn.spawnCondition = LevelSpawn.SpawnCondition.ElapsedTime;
                        levelSpawn.elapsedTime = levelSpawn.spawnTimeAfterDestroyAll;
                    }
                    break;
                case LevelSpawn.SpawnCondition.PlayerTouchTriggerWithSpawnerId:
                    if (touchedByPlayerLevelSpawnIds.Contains(levelSpawn.touchTriggerWithSpawnerId))
                    {
                        // switch to elapsed time spawn
                        levelSpawn.spawnCondition = LevelSpawn.SpawnCondition.ElapsedTime;
                        levelSpawn.elapsedTime = levelSpawn.spawnTimeAfterTrigger;
                    }
                    break;
                default: break;
            }

            if (isSpawnTime)
            {
                // init enemyAI and networkcharacter
                var enemyAI = levelSpawn.GetComponent<Dropt.EnemyAI>();
                var networkCharacter = levelSpawn.GetComponent<NetworkCharacter>();
                var statBarCanvas = levelSpawn.GetComponentInChildren<StatBarCanvas>();
                if (enemyAI != null && networkCharacter != null && statBarCanvas)
                {
                    enemyAI.Init();
                    networkCharacter.Init();
                    statBarCanvas.Init();
                }

                // we defer spawning to next frame to give navmesh enough time to be picked up by level spawns that need it
                DeferredSpawner.SpawnNextFrame(levelSpawn.GetComponent<NetworkObject>());
                levelSpawn.isSpawned = true;
            }
        }
    }

    public void AddLevelSpawnComponent(GameObject gObject, int spawnerId, GameObject prefab,
        Spawner_SpawnCondition spawnCondition = null)
    {
        // Add the LevelSpawn component to the instantiated object if does not have it
        LevelSpawn levelSpawn = gObject.GetComponent<LevelSpawn>();
        if (levelSpawn == null)
        {
            levelSpawn = gObject.AddComponent<LevelSpawn>();
        }

        // set basic instant spawn if no condition was passed
        if (spawnCondition == null)
        {
            levelSpawn.Set(
                spawnerId,
                LevelSpawn.SpawnCondition.ElapsedTime,
                0, 0, 0, 0, 0, prefab);
        }
        // set a more detailed spawn condition
        else
        {
            levelSpawn.Set(
                spawnerId,
                spawnCondition.spawnCondition,
                spawnCondition.elapsedTime,
                spawnCondition.destroyAllWithSpawnerId,
                spawnCondition.spawnTimeAfterDestroyAll,
                spawnCondition.touchTriggerWithSpawnerId,
                spawnCondition.spawnTimeAfterTrigger,
                prefab);
        }

        // add levelspawn to list
        m_levelSpawns.Add(levelSpawn);

        // deactivate the gameobject
        gObject.SetActive(false);
    }

    public void RemoveLevelSpawnComponent(LevelSpawn levelSpawn)
    {
        if (m_levelSpawns.Contains(levelSpawn))
        {
            m_levelSpawns.Remove(levelSpawn);
        }
    }
}
