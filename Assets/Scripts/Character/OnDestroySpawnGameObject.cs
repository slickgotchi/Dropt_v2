using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class OnDestroySpawnGameObject : NetworkBehaviour
{
    public GameObject Prefab;
    public float AliveTime = 1f;
    public Vector3 offset = Vector3.zero;

    public override void OnNetworkDespawn()
    {
        // we don't want to spawn if the level is changing so check isDestroying
        var destroyAtLevelChange = GetComponent<DestroyAtLevelChange>();
        if (destroyAtLevelChange != null && destroyAtLevelChange.isDestroying) return;

        var go = Instantiate(Prefab, transform.position + offset, Quaternion.identity);
        //var dot = go.AddComponent<DestroyOnTimer>();
        //dot.duration = AliveTime;

        //var particleSystem = go.GetComponent<ParticleSystem>();
        //var main = particleSystem.main;
        //main.loop = false;

        //particleSystem.Play();
    }
}
