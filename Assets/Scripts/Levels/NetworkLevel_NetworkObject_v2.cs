using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Core.Pool;

namespace Level
{
    // Spawner_NetworkObject_v2
    public partial class NetworkLevel : NetworkBehaviour
    {
        public void CreateSpawners_NetworkObject_v2()
        {
            if (!IsServer) return;

            var spawners = new List<Spawner_NetworkObject_v2>(GetComponentsInChildren<Spawner_NetworkObject_v2>());

            for (int i = 0; i < spawners.Count; i++)
            {
                // instantiate new object and set start position
                var randPrefab = spawners[i].GetRandom();
                if (randPrefab == null)
                {
                    spawners[i].spawnedNetworkObject = null;
                    continue;
                }

                // now instantiate
                var enemyController = randPrefab.GetComponent<EnemyController>();
                var destructible = randPrefab.GetComponent<Destructible>();
                NetworkObject no_object = null;
                if (destructible != null || enemyController != null)
                {
                    Debug.Log("CreateSpawners_NetworkObject_v2 - GetNetworkObject() for Destructible or Enemy");
                    no_object = NetworkObjectPool.Instance.GetNetworkObject(
                        randPrefab, spawners[i].transform.position, Quaternion.identity);
                }
                else
                {
                    Debug.Log("CreateSpawners_NetworkObject_v2 - Instantiating network object");
                    var gameObject = Object.Instantiate(
                        randPrefab, spawners[i].transform.position, Quaternion.identity);
                    no_object = gameObject.GetComponent<NetworkObject>();
                }
                
                //no_object.transform.position = spawners[i].transform.position;

                if (no_object != null)
                {
                    AddLevelSpawnComponent(no_object.gameObject,
                        spawners[i].spawnerId,
                        randPrefab,
                        spawners[i].GetComponent<Spawner_SpawnCondition>());

                    // add no_object ref to original spawner
                    spawners[i].spawnedNetworkObject = no_object.gameObject;
                }
            }

        }

        public void AddLevelSpawnComponent(GameObject no_object, int spawnerId, GameObject prefab,
            Spawner_SpawnCondition spawnCondition = null)
        {
            // Add the LevelSpawn component to the instantiated object
            var levelSpawn = no_object.AddComponent<LevelSpawn>();

            // Set the spawn details
            levelSpawn.spawnerId = spawnerId;
            levelSpawn.prefab = prefab;

            // if no spawn condition, just do a basic elapsed time spawn
            if (spawnCondition == null)
            {
                levelSpawn.spawnCondition = LevelSpawn.SpawnCondition.ElapsedTime;
                levelSpawn.elapsedTime = 0f;
            }
            else
            {
                //Debug.Log("Setting spawn condition to: " + spawnCondition.spawnCondition + " with elapsedTime: " + spawnCondition.elapsedTime);
                levelSpawn.spawnCondition = spawnCondition.spawnCondition;
                levelSpawn.elapsedTime = spawnCondition.elapsedTime;
                levelSpawn.destroyAllWithSpawnerId = spawnCondition.destroyAllWithSpawnerId;
                levelSpawn.spawnTimeAfterDestroyAll = spawnCondition.spawnTimeAfterDestroyAll;
                levelSpawn.touchTriggerWithSpawnerId = spawnCondition.touchTriggerWithSpawnerId;
                levelSpawn.spawnTimeAfterTrigger = spawnCondition.spawnTimeAfterTrigger;
            }

            // start object off inactive
            no_object.SetActive(false);
        }
    }
}
