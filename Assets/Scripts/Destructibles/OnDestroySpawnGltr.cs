using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OnDestroySpawnGltr : NetworkBehaviour
{
    public int GltrValue;

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        if (!GetComponent<OnDestroySpawnGltr>().enabled) return;

        // we don't want to spawn if the level is changing so check isDestroying
        var destroyAtLevelChange = GetComponent<DestroyAtLevelChange>();
        if (destroyAtLevelChange != null && destroyAtLevelChange.isDestroying) return;

        PickupItemManager.Instance.SpawnGltr(GltrValue, transform.position);

        base.OnNetworkDespawn();
    }
}
