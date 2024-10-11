using Unity.Netcode;
using UnityEngine;
using DG.Tweening;

public sealed class PickupItem : NetworkBehaviour
{
    private readonly float distanceForMagnet = 0.3f;
    public float speed = 5f;
    private GameObject target;
    //private PlayerPickupItemMagnet m_playerPickupItemMagnet;
    public NetworkVariable<bool> IsItemPicked = new NetworkVariable<bool>(false);

    private void Update()
    {
        if (target != null)
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, step);

            if (IsServer)
            {
                if (GetDistanceTo(target) < distanceForMagnet)
                {
                    // Notify the player's magnet that the item has been collected
                    PlayerPickupItemMagnet magnet = target.GetComponentInChildren<PlayerPickupItemMagnet>();
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
        IsItemPicked.Value = false;
        base.OnNetworkSpawn();
        gameObject.SetActive(true);
    }

    public override void OnNetworkDespawn()
    {
        gameObject.SetActive(false);
        base.OnNetworkDespawn();
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

    public void Pick(PlayerPickupItemMagnet playerPickupItemMagnet)
    {
        if (playerPickupItemMagnet == null)
        {
            Debug.Log("no valid PlayerPickupItemMagent");
        }

        if (IsServer) IsItemPicked.Value = true;
        Vector3 position = playerPickupItemMagnet.transform.position;
        var tween = transform.DOMove(position, 10)
                           .SetSpeedBased()
                           .SetEase(Ease.Linear);
        tween.OnComplete(() =>
        {
            playerPickupItemMagnet?.Collect(this);
        });
        GotoClientRpc(position);
    }

    [ClientRpc]
    public void GotoClientRpc(Vector3 position)
    {
        if (IsServer) return;
        _ = transform.DOMove(position, 10)
                             .SetSpeedBased()
                             .SetEase(Ease.Linear);
    }

    public bool AllowToPick()
    {
        return !IsItemPicked.Value;
    }
}