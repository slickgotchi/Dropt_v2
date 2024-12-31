using System.Collections.Generic;
using System.Data;
using Unity.Netcode;
using UnityEngine;
using Unity.Mathematics;

public class GenericProjectile : NetworkBehaviour
{
    [HideInInspector] public Vector3 Direction;
    [HideInInspector] public float Distance;
    [HideInInspector] public float Duration;
    [HideInInspector] public float Scale = 1f;

    [HideInInspector] public float DamagePerHit = 1f;
    [HideInInspector] public float CriticalChance = 0.1f;
    [HideInInspector] public float CriticalDamage = 1.5f;
    [HideInInspector] public Wearable.WeaponTypeEnum WeaponType;

    [HideInInspector] public Vector3 KnockbackDirection;
    [HideInInspector] public float KnockbackDistance;
    [HideInInspector] public float KnockbackStunDuration;

    [HideInInspector] public GameObject LocalPlayer;

    [HideInInspector] public PlayerAbility.NetworkRole Role = PlayerAbility.NetworkRole.LocalClient;

    [HideInInspector] public GameObject VisualGameObject;

    private float m_timer = 0;
    private bool m_isSpawned = false;
    private float m_speed = 1;

    private Collider2D m_collider;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        transform.parent = null;

        if (VisualGameObject != null) VisualGameObject.SetActive(false);

    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }

    public void Init(
        // server, local & remote
        Vector3 position,
        Vector3 direction,
        float distance,
        float duration,
        float scale,
        PlayerAbility.NetworkRole role,
        Wearable.WeaponTypeEnum weaponType,

        // server & local only
        GameObject player,
        float damagePerHit,
        float criticalChance,
        float criticalDamage,

        // knockback
        Vector3 knockbackDirection,
        float knockbackDistance,
        float knockbackStunDuration
        )
    {
        // server, local & remote
        gameObject.SetActive(true);
        transform.position = position;
        Direction = direction;
        Distance = distance;
        Duration = duration;
        Scale = scale;
        Role = role;
        WeaponType = weaponType;

        // server & local only
        LocalPlayer = player;
        DamagePerHit = damagePerHit;
        CriticalChance = criticalChance;
        CriticalDamage = criticalDamage;

        // knockback
        KnockbackDirection = knockbackDirection;
        KnockbackDistance = knockbackDistance;
        KnockbackStunDuration = knockbackStunDuration;

    }

    public void Fire()
    {
        gameObject.SetActive(true);

        transform.localScale = new Vector3(Scale, Scale, 1f);

        m_timer = Duration;
        m_isSpawned = true;
        m_speed = Distance / Duration;

        m_collider = GetComponent<Collider2D>();

        if (VisualGameObject != null) VisualGameObject.SetActive(true);

    }

    private void Update()
    {
        if (!m_isSpawned) return;

        m_timer -= Time.deltaTime;

        if (m_timer < 0)
        {
            if (VisualGameObject != null) VisualGameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        transform.position += Direction * m_speed * Time.deltaTime;
        transform.rotation = PlayerAbility.GetRotationFromDirection(Direction);

        if (Role != PlayerAbility.NetworkRole.RemoteClient) CollisionCheck();
    }

    private void LateUpdate()
    {
        if (IsClient)
        {
            VisualGameObject.transform.position = transform.position;
            VisualGameObject.transform.rotation = transform.rotation;
        }
    }

    public void CollisionCheck()
    {
        if (IsServer && !IsHost) PlayerAbility.RollbackEnemies(LocalPlayer);

        // resync transforms
        Physics2D.SyncTransforms();

        // Use ColliderCast to perform continuous collision detection
        Vector2 castDirection = Direction.normalized;
        float castDistance = m_speed * Time.deltaTime;
        RaycastHit2D[] hits = new RaycastHit2D[1];
        int hitCount = m_collider.Cast(castDirection,
            PlayerAbility.GetContactFilter(new string[] { "EnemyHurt", "Destructible", "EnvironmentWall" }),
            hits, castDistance);

        if (hitCount > 0)
        {
            RaycastHit2D hitInfo = hits[0];
            Collider2D hit = hitInfo.collider;
            if (hit.HasComponent<NetworkCharacter>())
            {
                var damage = PlayerAbility.GetRandomVariation(DamagePerHit);
                var isCritical = PlayerAbility.IsCriticalAttack(CriticalChance);
                damage = (int)(isCritical ? damage * CriticalDamage : damage);
                hit.GetComponent<NetworkCharacter>().TakeDamage(damage, isCritical, LocalPlayer);

                var enemyAI = hit.GetComponent<Dropt.EnemyAI>();
                if (enemyAI != null)
                {
                    enemyAI.Knockback(KnockbackDirection, KnockbackDistance, KnockbackStunDuration);
                }
            }
            else if (hit.HasComponent<Destructible>())
            {
                var destructible = hit.GetComponent<Destructible>();
                destructible.TakeDamage(WeaponType, LocalPlayer.GetComponent<NetworkObject>().NetworkObjectId);
            }
            ExplodeAndDeactivate(hitInfo.point);

            if (LocalPlayer != null)
            {
                LocalPlayer.GetComponent<PlayerCamera>().Shake();
            }
        }

        if (IsServer && !IsHost) PlayerAbility.UnrollEnemies();
    }

    void ExplodeAndDeactivate(Vector3 hitPosition)
    {
        if (VisualGameObject != null) Destroy(VisualGameObject);

        VisualEffectsManager.Instance.SpawnBulletExplosion(hitPosition);
        if (VisualGameObject != null) VisualGameObject.SetActive(false);
        gameObject.SetActive(false);

        if (Role == PlayerAbility.NetworkRole.Server)
        {
            ExplodeAndDeactivateClientRpc(hitPosition);
        }
    }



    [Rpc(SendTo.ClientsAndHost)]
    void ExplodeAndDeactivateClientRpc(Vector3 hitPosition)
    {
        if (Role == PlayerAbility.NetworkRole.RemoteClient)
        {
            VisualEffectsManager.Instance.SpawnBulletExplosion(hitPosition);
            if (VisualGameObject != null) VisualGameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}
