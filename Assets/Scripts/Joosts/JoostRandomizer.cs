using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Mathematics;

public class JoostRandomizer : NetworkBehaviour
{
    public List<GameObject> joostInteractablePrefabs = new List<GameObject>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            Dropt.Utils.List.Shuffle(joostInteractablePrefabs);

            var count = math.min(transform.childCount, joostInteractablePrefabs.Count);
            for (int i = 0; i < count; i++)
            {
                var child = transform.GetChild(i);
                var obj = Instantiate(joostInteractablePrefabs[i], child.position, Quaternion.identity);
                var networkObject = obj.GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    networkObject.Spawn();
                }
                else
                {
                    Debug.LogWarning("Invalid interactable added to joostInteractablePrefabs");
                }
            }
        }

        var spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in spriteRenderers)
        {
            sr.enabled = false;
        }
    }
}
