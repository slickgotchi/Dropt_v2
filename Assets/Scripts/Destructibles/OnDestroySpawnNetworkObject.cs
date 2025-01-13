using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OnDestroySpawnNetworkObject : NetworkBehaviour
{
    public Vector3 Offset;
    public GameObject SpawnPrefab;

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        if (!GetComponent<OnDestroySpawnNetworkObject>().enabled) return;
        if (SpawnPrefab == null) return;

        // we don't want to spawn if the level is changing so check isDestroying
        var destroyAtLevelChange = GetComponent<DestroyAtLevelChange>();
        if (destroyAtLevelChange != null && destroyAtLevelChange.isDestroying) return;

        // spawn 
        var newSpawn = Instantiate(SpawnPrefab, transform);
        newSpawn.transform.position += Offset;
        newSpawn.GetComponent<NetworkObject>().Spawn();

        base.OnNetworkDespawn();
    }

}
