using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Level
{
    // NetworkObjectPrefabSpawners
    public partial class NetworkLevel : NetworkBehaviour
    {
        //public void CreateNetworkObjectPrefabSpawners()
        //{
        //    var no_prefabSpawners = new List<NetworkObjectPrefabSpawner>(GetComponentsInChildren<NetworkObjectPrefabSpawner>());

        //    for (int i = 0; i < no_prefabSpawners.Count; i++)
        //    {
        //        var no_object = Object.Instantiate(no_prefabSpawners[i].NetworkObjectPrefab);
        //        no_object.transform.position = no_prefabSpawners[i].transform.position;
        //        LevelManager.Instance.AddToSpawnList(no_object.GetComponent<NetworkObject>());

        //        //DestroyAllChildren(no_prefabSpawners[i].transform);
        //        Debug.Log("added to spawn list");
        //    }
        //}
    }
}
