using Unity.Netcode;
using UnityEngine;
using PickupItems.Orb;

public class PlayerPickupItemMagnet : NetworkBehaviour
{
    public float Radius = 5f;
    public Collider2D magnetCollider;

    public PlayerOffchainData PlayerDungeonData;
    private PickupItem m_currentPickupItem;

    private void Start()
    {
        // Create and configure the magnet collider
        //magnetCollider = gameObject.AddComponent<CircleCollider2D>();
        magnetCollider.isTrigger = true;
        ((CircleCollider2D)magnetCollider).radius = Radius;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        m_currentPickupItem = other.GetComponent<PickupItem>();

        if (m_currentPickupItem != null)
        {
            //m_currentPickupItem.Pick(gameObject.GetComponent<PlayerPickupItemMagnet>());
            m_currentPickupItem.TryGoTo(gameObject);
        }
    }

    // Collect method to be called when the item is picked up
    public void Collect(PickupItem pickupItem)
    {
        if (!IsServer) return;

        if (pickupItem.gameObject.HasComponent<GltrOrb>())
        {
            PlayerDungeonData.AddDungeonDust(pickupItem.GetComponent<GltrOrb>().GetValue());
        }

        if (pickupItem.gameObject.HasComponent<CGHSTOrb>())
        {
            PlayerDungeonData.AddDungeonEcto(pickupItem.GetComponent<CGHSTOrb>().GetValue());
        }

        if (pickupItem.gameObject.HasComponent<ApOrb>())
        {
            PlayerDungeonData.GetComponent<NetworkCharacter>().ApCurrent.Value +=
                pickupItem.GetComponent<ApOrb>().GetValue();
        }

        if (pickupItem.gameObject.HasComponent<HpOrb>())
        {
            NetworkCharacter networkCharacter = PlayerDungeonData.GetComponent<NetworkCharacter>();
            networkCharacter.AddHp(pickupItem.GetComponent<HpOrb>().GetValue());
        }

        if (pickupItem.gameObject.HasComponent<HpCannister>())
        {
            Debug.Log("HpCannister");
            PlayerCharacter networkCharacter = PlayerDungeonData.GetComponent<PlayerCharacter>();
            int amount = pickupItem.GetComponent<HpCannister>().GetValue();
            networkCharacter.AddHp(amount);
            networkCharacter.SpawnHpCannistaerEffect();
            PopupTextClientRpc(amount);
            NetworkObject networkObj = pickupItem.GetComponent<NetworkObject>();
            networkObj.Despawn();
            return;
        }

        if (pickupItem.gameObject.HasComponent<EssenceCannister>())
        {
            Debug.Log("EssenceCannister");
            PlayerCharacter networkCharacter = PlayerDungeonData.GetComponent<PlayerCharacter>();
            int amount = pickupItem.GetComponent<EssenceCannister>().GetValue();
            networkCharacter.AddEssenceValue(amount);
            networkCharacter.SpawnEssenceCannisterEffect();
            PopupTextClientRpc(amount);
            NetworkObject networkObj = pickupItem.GetComponent<NetworkObject>();
            networkObj.Despawn();
            return;
        }

        PickupItemManager.Instance.ReturnToPool(pickupItem);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PopupTextClientRpc(float amount)
    {
        PopupTextManager.Instance.PopupText(
            $"+{amount:F0}",
            transform.position + new Vector3(0, 1.5f, 0),
            16,
            new Color32(153, 230, 95, 255),
            0.2f);
    }
}