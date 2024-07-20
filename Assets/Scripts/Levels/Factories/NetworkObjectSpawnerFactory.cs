using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Level
{
    public struct NetworkObjectSpawn
    {
        public string Name;
        public float Chance;
    }

    public static class NetworkObjectSpawnerFactory
    {
        public static void CreateNetworkObjectSpawners(GameObject parent, ref List<SpawnerActivator> spawnerActivators)
        {
            var no_spawners = new List<NetworkObjectSpawner>(parent.GetComponentsInChildren<NetworkObjectSpawner>());

            for (int i = 0; i < no_spawners.Count; i++)
            {
                no_spawners[i] = NormalizeNetworkObjectSpawner(no_spawners[i]);

                if (no_spawners[i].transform.childCount <= 0)
                {
                    Debug.Log("No spawn points attached as children to NetworkObjectSpawner");
                    continue;
                }

                var spawns = GetNetworkObjectSpawns(no_spawners[i]);
                var spawnedObjects = SpawnObjects(no_spawners[i], spawns);

                HandleOnDestroySpawnNetworkObject(no_spawners[i], spawnedObjects);

                var spawnerActivator = new SpawnerActivator();
                foreach (var spawnedObject in spawnedObjects)
                {
                    spawnedObject.SetActive(false);
                    spawnerActivator.spawnedObjects.Add(spawnedObject);
                    spawnerActivator.Type = SpawnerActivator.ActivationType.ElapsedTime;
                }
                spawnerActivators.Add(spawnerActivator);
            }

            AssignSpawnerActivators(no_spawners, spawnerActivators);
            CleanupFactory.DestroySpawnerObjects<NetworkObjectSpawner>(parent);
        }

        private static List<NetworkObjectSpawn> GetNetworkObjectSpawns(NetworkObjectSpawner no_spawner)
        {
            var spawns = new List<NetworkObjectSpawn>();

            foreach (var spawnEnemy in no_spawner.SpawnEnemies)
            {
                spawns.Add(new NetworkObjectSpawn { Name = spawnEnemy.EnemyPrefabType.ToString(), Chance = spawnEnemy.SpawnChance });
            }

            foreach (var spawnDestructible in no_spawner.SpawnDestructibles)
            {
                spawns.Add(new NetworkObjectSpawn { Name = spawnDestructible.DestructiblePrefabType.ToString(), Chance = spawnDestructible.SpawnChance });
            }

            foreach (var spawnInteractable in no_spawner.SpawnInteractables)
            {
                spawns.Add(new NetworkObjectSpawn { Name = spawnInteractable.InteractablePrefabType.ToString(), Chance = spawnInteractable.SpawnChance });
            }

            foreach (var spawnProp in no_spawner.SpawnProps)
            {
                spawns.Add(new NetworkObjectSpawn { Name = spawnProp.PropPrefabType.ToString(), Chance = spawnProp.SpawnChance });
            }

            return spawns;
        }

        private static List<GameObject> SpawnObjects(NetworkObjectSpawner no_spawner, List<NetworkObjectSpawn> spawns)
        {
            var spawnedObjects = new List<GameObject>();

            foreach (Transform spawnPoint in no_spawner.transform)
            {
                var randValue = UnityEngine.Random.Range(0f, 1f);

                foreach (var spawn in spawns)
                {
                    if (randValue < spawn.Chance)
                    {
                        var spawnObject = Prefabs_NetworkObject.Instance.GetNetworkObjectByName(spawn.Name);
                        if (spawnObject != null)
                        {
                            var no_object = Object.Instantiate(spawnObject);
                            no_object.transform.position = spawnPoint.position;
                            spawnedObjects.Add(no_object);
                        }
                        else
                        {
                            Debug.Log(spawn.Name + " does not exist in Prefabs_NetworkObject");
                        }
                        break;
                    }
                    else
                    {
                        randValue -= spawn.Chance;
                    }
                }
            }

            return spawnedObjects;
        }

        private static void HandleOnDestroySpawnNetworkObject(NetworkObjectSpawner no_spawner, List<GameObject> spawnedObjects)
        {
            var onDestroySpawner = no_spawner.GetComponent<SpawnerOnDestroySpawnNetworkObject>();
            if (onDestroySpawner != null)
            {
                if (onDestroySpawner.Type == SpawnerOnDestroySpawnNetworkObject.EnumType.Chance)
                {
                    foreach (var spawnedObject in spawnedObjects)
                    {
                        var rand = UnityEngine.Random.Range(0, 0.9999f);
                        if (rand < onDestroySpawner.Chance)
                        {
                            var odsno = spawnedObject.GetComponent<OnDestroySpawnNetworkObject>();
                            odsno.SpawnPrefab = onDestroySpawner.NetworkObjectPrefab;
                        }
                    }
                }
                else if (onDestroySpawner.Type == SpawnerOnDestroySpawnNetworkObject.EnumType.Range)
                {
                    if (onDestroySpawner.MaxRange < onDestroySpawner.MinRange) onDestroySpawner.MaxRange = onDestroySpawner.MinRange;
                    var numberSpawns = UnityEngine.Random.Range(onDestroySpawner.MinRange, onDestroySpawner.MaxRange);
                    var randIndices = GetRandomSamples(spawnedObjects.Count, numberSpawns);
                    foreach (var rand in randIndices)
                    {
                        var odsno = spawnedObjects[rand].GetComponent<OnDestroySpawnNetworkObject>();
                        odsno.SpawnPrefab = onDestroySpawner.NetworkObjectPrefab;
                    }
                }
            }
        }

        private static List<int> GetRandomSamples(int count, int numberSamples)
        {
            if (numberSamples > count) numberSamples = count;

            var samples = new HashSet<int>();
            int justInCase = 0;
            while (samples.Count < numberSamples)
            {
                var randomNumber = UnityEngine.Random.Range(0, count);
                samples.Add(randomNumber);
                justInCase++;

                if (justInCase > 1000)
                {
                    Debug.Log("Exceeded 1000 loops in GetRandomSamples. Check input parameters");
                    break;
                }
            }

            return new List<int>(samples);
        }

        private static void AssignSpawnerActivators(List<NetworkObjectSpawner> no_spawners, List<SpawnerActivator> spawnerActivators)
        {
            for (int i = 0; i < no_spawners.Count; i++)
            {
                if (no_spawners[i].activationType == NetworkObjectSpawner.ActivationType.OtherSpawnerCleared)
                {
                    for (int j = 0; j < no_spawners.Count; j++)
                    {
                        if (no_spawners[i].activateOnOtherSpawnerCleared == no_spawners[j])
                        {
                            spawnerActivators[i].Type = SpawnerActivator.ActivationType.OtherSpawnerCleared;
                            spawnerActivators[i].otherSpawnerActivator = spawnerActivators[j];
                        }
                    }
                }
            }
        }

        private static NetworkObjectSpawner NormalizeNetworkObjectSpawner(NetworkObjectSpawner no_spawner)
        {
            var sum = no_spawner.NoSpawnChance;

            foreach (var spawnPrefab in no_spawner.SpawnEnemies)
            {
                sum += spawnPrefab.SpawnChance;
            }

            foreach (var spawnPrefab in no_spawner.SpawnDestructibles)
            {
                sum += spawnPrefab.SpawnChance;
            }

            foreach (var spawnPrefab in no_spawner.SpawnInteractables)
            {
                sum += spawnPrefab.SpawnChance;
            }

            foreach (var spawnPrefab in no_spawner.SpawnProps)
            {
                sum += spawnPrefab.SpawnChance;
            }

            if (sum <= 0) return no_spawner;

            no_spawner.NoSpawnChance /= sum;

            foreach (var spawnPrefab in no_spawner.SpawnEnemies)
            {
                spawnPrefab.SpawnChance /= sum;
            }

            foreach (var spawnPrefab in no_spawner.SpawnDestructibles)
            {
                spawnPrefab.SpawnChance /= sum;
            }

            foreach (var spawnPrefab in no_spawner.SpawnInteractables)
            {
                spawnPrefab.SpawnChance /= sum;
            }

            foreach (var spawnPrefab in no_spawner.SpawnProps)
            {
                spawnPrefab.SpawnChance /= sum;
            }

            return no_spawner;
        }
    }
}
