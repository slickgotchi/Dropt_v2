using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class PlayerPickupItemMagnet : NetworkBehaviour
{
    public float Radius = 5f;
    private Collider2D magnetCollider;
    private HashSet<PickupItem> attractedItems;

    public PlayerDungeonData PlayerDungeonData;

    void Start()
    {
        // Create and configure the magnet collider
        magnetCollider = gameObject.AddComponent<CircleCollider2D>();
        magnetCollider.isTrigger = true;
        ((CircleCollider2D)magnetCollider).radius = Radius;

        // Initialize the set of attracted items
        attractedItems = new HashSet<PickupItem>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the other collider is a PickupItem
        PickupItem pickupItem = other.GetComponent<PickupItem>();
        if (pickupItem != null && !attractedItems.Contains(pickupItem))
        {
            attractedItems.Add(pickupItem);
            pickupItem.GoTo(gameObject);
        }
    }

    // Collect method to be called when the item is picked up
    public void Collect(PickupItem pickupItem)
    {
        if (!IsServer) return;

        // check for gltr
        if (pickupItem.gameObject.HasComponent<GltrOrb>())
        {
            PlayerDungeonData.AddGltr(pickupItem.GetComponent<GltrOrb>().GetValue());
        }

        // Remove the item from the set of attracted items
        attractedItems.Remove(pickupItem);

        // Optionally, destroy the item or perform other actions
        pickupItem.GetComponent<NetworkObject>().Despawn();
    }
}
