using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ProximityCulling : NetworkBehaviour
{
    //public bool IsCulled = true;

    public NetworkObject networkObject;
    public PooledObject pooledObject;

    public static List<ProximityCulling> culledObjects = new List<ProximityCulling>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        networkObject = GetComponent<NetworkObject>();
        pooledObject = GetComponent<PooledObject>();

        if (!culledObjects.Contains(this))
        {
            culledObjects.Add(this);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (culledObjects.Contains(this))
        {
            culledObjects.Remove(this);
        }

        base.OnNetworkDespawn();
    }
}
