using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GenericEnemyProjectile : NetworkBehaviour
{
    [HideInInspector] public Vector3 Direction;
    [HideInInspector] public float Distance;
    [HideInInspector] public float Duration;

    [HideInInspector] public float DamagePerHit = 1f;
    [HideInInspector] public float CriticalChance = 0.1f;
    [HideInInspector] public float CriticalDamage = 1.5f;
    [HideInInspector] public GameObject Parent;

    [HideInInspector] public float Scale = 1f;

    private float m_timer = 0;
    private bool m_isSpawned = false;
    private float m_speed = 1;

    private Collider2D m_collider;

    public void Init(Vector3 position, Quaternion rotation, Vector3 direction, float distance, float duration, float damagePerHit,
        float criticalChance, float criticalDamage, GameObject parent, float scale = 1f)
    {
        transform.position = position;
        transform.rotation = rotation;
        Direction = direction;
        Distance = distance;
        Duration = duration;
        DamagePerHit = damagePerHit;
        CriticalChance = criticalChance;
        CriticalDamage = criticalDamage;
        Parent = parent;
        Scale = scale;
    }

    public void Fire()
    {
        gameObject.SetActive(true);

        transform.localScale = new Vector3(Scale, Scale, 1f);

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
            if (IsServer) gameObject.GetComponent<NetworkObject>().Despawn();
        }

        transform.position += Direction * m_speed * Time.deltaTime;
        transform.rotation = PlayerAbility.GetRotationFromDirection(Direction);

        CollisionCheck();
    }

    public void CollisionCheck()
    {
        // Use ColliderCast to perform continuous collision detection
        Vector2 castDirection = Direction.normalized;
        float castDistance = m_speed * Time.deltaTime;
        RaycastHit2D[] hits = new RaycastHit2D[1];
        int hitCount = m_collider.Cast(castDirection,
            PlayerAbility.GetContactFilter(new string[] { "PlayerHurt", "Destructible", "EnvironmentWall" }),
            hits, castDistance);

        if (hitCount > 0)
        {
            RaycastHit2D hitInfo = hits[0];
            Collider2D hit = hitInfo.collider;
            if (hit.transform.parent != null && hit.transform.parent.HasComponent<NetworkCharacter>())
            {
                var damage = PlayerAbility.GetRandomVariation(DamagePerHit);
                var isCritical = PlayerAbility.IsCriticalAttack(CriticalChance);
                damage = (int)(isCritical ? damage * CriticalDamage : damage);
                hit.transform.parent.GetComponent<NetworkCharacter>().TakeDamage(damage, isCritical, Parent);
            }
            gameObject.SetActive(false);
            if (IsServer) gameObject.GetComponent<NetworkObject>().Despawn();
        }
    }
}
