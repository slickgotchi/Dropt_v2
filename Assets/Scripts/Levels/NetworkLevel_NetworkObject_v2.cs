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
                NetworkObject networkObject = null;

                // handle enemies and destructibles
                //if (destructible != null || enemyController != null)
                //{
                //    networkObject = NetworkObjectPool.Instance.GetNetworkObject(
                //        randPrefab, spawners[i].transform.position, Quaternion.identity);
                //}
                // handle instantiating all other objects
                //else
                //{
                    var gameObject = Object.Instantiate(
                        randPrefab, spawners[i].transform.position, Quaternion.identity);
                    networkObject = gameObject.GetComponent<NetworkObject>();
                //}
                
                // if we got networkobject, set levelspawn
                if (networkObject != null)
                {
                    LevelSpawnManager.Instance.AddLevelSpawnComponent(
                        networkObject.gameObject,
                        spawners[i].spawnerId,
                        randPrefab,
                        spawners[i].GetComponent<Spawner_SpawnCondition>());

                    // add networkObject ref to original spawner
                    spawners[i].spawnedNetworkObject = networkObject.gameObject;

                    // start the object off inactive
                    networkObject.gameObject.SetActive(false);
                }
            }
        }
    }
}
