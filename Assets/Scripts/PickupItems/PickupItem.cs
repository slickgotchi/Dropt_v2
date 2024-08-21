using Unity.Netcode;
using UnityEngine;

public sealed class PickupItem : NetworkBehaviour
{
    private readonly float distanceForMagnet = 0.3f;
    public float speed = 5f;
    private GameObject target;

    private void Update()
    {
        if (target != null)
        {
            // Move towards the target
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, step);

            if (IsServer)
            {
                if (GetDistanceTo(target) < distanceForMagnet)
                {
                    // Notify the player's magnet that the item has been collected
                    PlayerPickupItemMagnet magnet = target.GetComponent<PlayerPickupItemMagnet>();
                    if (magnet != null)
                    {
                        magnet.Collect(this);
                    }
                }
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        gameObject.SetActive(true);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        target = null;
    }

    private float GetDistanceTo(GameObject target)
    {
        return Vector2.Distance(transform.position, target.transform.position);
    }

    public bool TryGoTo(GameObject target)
    {
        if (this.target == null)
        {
            this.target = target;
            return true;
        }

        if (GetDistanceTo(target) > GetDistanceTo(this.target))
        {
            return false;
        }

        this.target = target;
        return true;
    }
}