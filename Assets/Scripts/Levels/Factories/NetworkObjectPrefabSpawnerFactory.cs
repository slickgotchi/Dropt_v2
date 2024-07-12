using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Level
{
    public static class NetworkObjectPrefabSpawnerFactory
    {
        public static void CreateNetworkObjectPrefabSpawners(GameObject parent)
        {
            var no_prefabSpawners = new List<NetworkObjectPrefabSpawner>(parent.GetComponentsInChildren<NetworkObjectPrefabSpawner>());

            for (int i = 0; i < no_prefabSpawners.Count; i++)
            {
                var no_object = Object.Instantiate(no_prefabSpawners[i].NetworkObjectPrefab);
                no_object.transform.position = no_prefabSpawners[i].transform.position;
                no_object.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}
