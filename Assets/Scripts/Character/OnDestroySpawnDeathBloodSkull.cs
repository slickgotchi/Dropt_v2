using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class OnDeathSpawnBloodSkull : NetworkBehaviour
{
    public float Scale;

    public Vector3 offset = Vector3.zero;

    public override void OnNetworkDespawn()
    {
        // we don't want to spawn if the level is changing so check isDestroying
        var destroyAtLevelChange = GetComponent<DestroyAtLevelChange>();
        if (destroyAtLevelChange != null && destroyAtLevelChange.isDestroying) return;

        VisualEffectsManager.Instance.SpawnVFX_DeathBloodSkull(transform.position + offset,
            Scale);
    }
}
