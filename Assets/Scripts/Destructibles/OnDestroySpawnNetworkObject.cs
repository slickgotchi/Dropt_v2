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
        if (IsServer)
        {
            if (SpawnPrefab == null) return;

            var newSpawn = Instantiate(SpawnPrefab, transform);
            newSpawn.transform.position += Offset;
            newSpawn.GetComponent<NetworkObject>().Spawn();
        }
    }

}
