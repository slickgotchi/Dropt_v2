using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Level
{
    // Spawner_NetworkObject_v2
    public partial class NetworkLevel : NetworkBehaviour
    {
        public void CreateSpawners_NetworkObject_v2()
        {
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
                var no_object = Object.Instantiate(randPrefab);
                no_object.transform.position = spawners[i].transform.position;

                AddLevelSpawnComponent(no_object, spawners[i].spawnerId, spawners[i].GetComponent<Spawner_SpawnCondition>());

                // add no_object ref to original spawner
                spawners[i].spawnedNetworkObject = no_object;
            }

        }

        public void AddLevelSpawnComponent(GameObject no_object, int spawnerId, Spawner_SpawnCondition spawnCondition = null)
        {
            // Add the LevelSpawn component to the instantiated object
            var levelSpawn = no_object.AddComponent<LevelSpawn>();

            // Set the spawn details
            levelSpawn.spawnerId = spawnerId;

            // if no spawn condition, just do a basic elapsed time spawn
            if (spawnCondition == null)
            {
                levelSpawn.spawnCondition = LevelSpawn.SpawnCondition.ElapsedTime;
                levelSpawn.elapsedTime = 0f;
            }
            else
            {
                Debug.Log("Setting spawn condition to: " + spawnCondition.spawnCondition + " with elapsedTime: " + spawnCondition.elapsedTime);
                levelSpawn.spawnCondition = spawnCondition.spawnCondition;
                levelSpawn.elapsedTime = spawnCondition.elapsedTime;
                levelSpawn.destroyAllWithSpawnerId = spawnCondition.destroyAllWithSpawnerId;
                levelSpawn.touchTriggerWithSpawnerId = spawnCondition.touchTriggerWithSpawnerId;
            }

            // start object off inactive
            no_object.SetActive(false);
        }
    }
}
