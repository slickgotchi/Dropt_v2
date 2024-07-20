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

        PickupItemManager.Instance.SpawnGltr(GltrValue, transform.position);
    }
}
