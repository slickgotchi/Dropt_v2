using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnHitSpawnGameObject : MonoBehaviour
{
    public GameObject Prefab;
    public float AliveTime = 1f;
    public Vector3 offset = Vector3.zero;

    public void Hit(Vector3 position)
    {
        // we don't want to spawn if the level is changing so check isDestroying
        var destroyAtLevelChange = GetComponent<DestroyAtLevelChange>();
        if (destroyAtLevelChange != null && destroyAtLevelChange.isDestroying) return;

        var go = Instantiate(Prefab, position, Quaternion.identity);
        var dot = go.AddComponent<DestroyOnTimer>();
        dot.duration = AliveTime;
    }
}
