using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BallisticShotProjectile : NetworkBehaviour
{
    [HideInInspector] public Vector3 Direction;
    [HideInInspector] public float Distance;
    [HideInInspector] public float Duration;

    [HideInInspector] public float DamagePerHit = 1f;
    [HideInInspector] public float CriticalChance = 0.1f;
    [HideInInspector] public float CriticalDamage = 1.5f;

    [HideInInspector] public PlayerAbility.NetworkRole Role = PlayerAbility.NetworkRole.LocalClient;

    private float m_timer = 0;
    private bool m_isSpawned = false;
    private float m_speed = 1;

    private Collider2D m_collider;

    public void Fire()
    {
        gameObject.SetActive(true);

        m_timer = Duration;
        m_isSpawned = true;
        m_speed = Distance / Duration;

        m_collider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (!m_isSpawned) return;

        m_timer -= Time.deltaTime;

        if (m_timer < 0)
        {
            gameObject.SetActive(false);
        }

        transform.position += Direction * m_speed * Time.deltaTime;

        if (Role != PlayerAbility.NetworkRole.RemoteClient) CollisionCheck();
    }

    public void CollisionCheck()
    {
        // Use ColliderCast to perform continuous collision detection
        Vector2 castDirection = Direction.normalized;
        float castDistance = m_speed * Time.deltaTime;
        RaycastHit2D[] hits = new RaycastHit2D[1];
        int hitCount = m_collider.Cast(castDirection, 
            PlayerAbility.GetContactFilter(new string[] { "EnemyHurt", "Destructible", "EnvironmentWall" }), 
            hits, castDistance);

        if (hitCount > 0)
        {
            Collider2D hit = hits[0].collider;
            if (hit.HasComponent<NetworkCharacter>())
            {
                var damage = PlayerAbility.GetRandomVariation(DamagePerHit);
                var isCritical = PlayerAbility.IsCriticalAttack(CriticalChance);
                damage = (int)(isCritical ? damage * CriticalDamage : damage);
                hit.GetComponent<NetworkCharacter>().TakeDamage(damage, isCritical);
            }
            else if (hit.HasComponent<Destructible>())
            {
                var destructible = hit.GetComponent<Destructible>();
                destructible.TakeDamage(Wearable.WeaponTypeEnum.Ballistic);
            }
            Deactivate();
        }
    }

    void Deactivate()
    {
        gameObject.SetActive(false);

        if (Role == PlayerAbility.NetworkRole.Server)
        {
            DeactivateClientRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void DeactivateClientRpc()
    {
        if (Role == PlayerAbility.NetworkRole.RemoteClient)
        {
            gameObject.SetActive(false);
        }
    }
}
