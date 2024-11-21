using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OnDestroySpawnHpAp : NetworkBehaviour
{
    public float SmallHpChance = 0.4f;
    public float BigHpChance = 0.1f;
    public float SmallApChance = 0.4f;
    public float BigApChance = 0.1f;

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        if (!GetComponent<OnDestroySpawnHpAp>().enabled) return;

        var rand = UnityEngine.Random.Range(0f, 1f);

        if (rand < SmallHpChance)
        {
            PickupItemManager.Instance.SpawnHpOrb(PickupItemManager.Size.Small, transform.position);
        }
        else if (rand < SmallHpChance + BigHpChance)
        {
            PickupItemManager.Instance.SpawnHpOrb(PickupItemManager.Size.Large, transform.position);
        }
        else if (rand < SmallHpChance + BigHpChance + SmallApChance)
        {
            PickupItemManager.Instance.SpawnApOrb(PickupItemManager.Size.Small, transform.position);
        }
        else
        {
            PickupItemManager.Instance.SpawnApOrb(PickupItemManager.Size.Large, transform.position);
        }

        base.OnNetworkDespawn();
    }
}
