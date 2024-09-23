using Unity.Netcode;
using UnityEngine;
using PickupItems.Orb;

public class PlayerPickupItemMagnet : NetworkBehaviour
{
    public float Radius = 5f;
    public Collider2D magnetCollider;

    public PlayerDungeonData PlayerDungeonData;
    private PickupItem m_currentPickupItem;

    void Start()
    {
        // Create and configure the magnet collider
        //magnetCollider = gameObject.AddComponent<CircleCollider2D>();
        magnetCollider.isTrigger = true;
        ((CircleCollider2D)magnetCollider).radius = Radius;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        m_currentPickupItem = other.GetComponent<PickupItem>();

        if (m_currentPickupItem != null)
        {
            m_currentPickupItem.Pick(gameObject.GetComponent<PlayerPickupItemMagnet>());
        }
    }

    // Collect method to be called when the item is picked up
    public void Collect(PickupItem pickupItem)
    {
        if (!IsServer) return;

        if (pickupItem.gameObject.HasComponent<GltrOrb>())
        {
            PlayerDungeonData.AddGltr(pickupItem.GetComponent<GltrOrb>().GetValue());
        }

        if (pickupItem.gameObject.HasComponent<CGHSTOrb>())
        {
            PlayerDungeonData.AddCGHST(pickupItem.GetComponent<CGHSTOrb>().GetValue());
        }

        PickupItemManager.Instance.ReturnToPool(pickupItem);
    }
}