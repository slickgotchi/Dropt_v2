using System.Collections.Generic;
using System.Data;
using Unity.Netcode;
using UnityEngine;

public class BallisticExplosionProjectile : NetworkBehaviour
{
    [HideInInspector] public Vector3 Direction;
    [HideInInspector] public float Distance;
    [HideInInspector] public float Duration;
    [HideInInspector] public float ExplosionRadius = 2f;
    public Collider2D ExplosionCollider;
    [HideInInspector] public float ProjectileDamageMultiplier = 1f;
    [HideInInspector] public float ExplosionDamageMultiplier = 0.5f;

    [HideInInspector] public float DamagePerHit = 1f;
    [HideInInspector] public float CriticalChance = 0.1f;
    [HideInInspector] public float CriticalDamage = 1.5f;
    [HideInInspector] public Wearable.WeaponTypeEnum WeaponType;

    [HideInInspector] public GameObject LocalPlayer;

    [HideInInspector] public PlayerAbility.NetworkRole Role = PlayerAbility.NetworkRole.LocalClient;

    private float m_timer = 0;
    private bool m_isSpawned = false;
    private float m_speed = 1;

    private Collider2D m_collider;

    public void Init(
        // server, local & remote
        Vector3 position,
        Vector3 direction,
        float distance,
        float duration,
        float explosionRadius,
        PlayerAbility.NetworkRole role,
        Wearable.WeaponTypeEnum weaponType,

        // server & local only
        GameObject player,
        float damagePerHit,
        float criticalChance,
        float criticalDamage,
        float projectileDamageMultiplier,
        float explosionDamageMultiplier
        )
    {
        // server, local & remote
        gameObject.SetActive(true);
        transform.position = position;
        Direction = direction;
        Distance = distance;
        Duration = duration;
        ExplosionRadius = explosionRadius;
        Role = role;
        WeaponType = weaponType;

        // server & local only
        LocalPlayer = player;
        DamagePerHit = damagePerHit;
        CriticalChance = criticalChance;
        CriticalDamage = criticalDamage;
        ProjectileDamageMultiplier = projectileDamageMultiplier;
        ExplosionDamageMultiplier = explosionDamageMultiplier;
    }

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
            Explode(transform.position);
            gameObject.SetActive(false);
        }

        transform.position += Direction * m_speed * Time.deltaTime;
        transform.rotation = PlayerAbility.GetRotationFromDirection(Direction);

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
            RaycastHit2D hitInfo = hits[0];
            Collider2D hit = hitInfo.collider;
            if (hit.HasComponent<NetworkCharacter>())
            {
                var damage = PlayerAbility.GetRandomVariation(DamagePerHit * ProjectileDamageMultiplier);
                var isCritical = PlayerAbility.IsCriticalAttack(CriticalChance);
                damage = (int)(isCritical ? damage * CriticalDamage : damage);
                hit.GetComponent<NetworkCharacter>().TakeDamage(damage, isCritical, LocalPlayer);
            }
            else if (hit.HasComponent<Destructible>())
            {
                var destructible = hit.GetComponent<Destructible>();
                destructible.TakeDamage(WeaponType);
            }
            Deactivate(hitInfo.point);

            if (LocalPlayer != null)
            {
                LocalPlayer.GetComponent<PlayerCamera>().Shake();

                // visual effect
                VisualEffectsManager.Singleton.SpawnSplashExplosion(hitInfo.point, new Color(1, 0, 0, 0.5f), ExplosionRadius);
            }

            Explode(hitInfo.point);
        }
    }

    void Explode(Vector3 position)
    {
        if (LocalPlayer != null)
        {
            // visual effect
            VisualEffectsManager.Singleton.SpawnSplashExplosion(position, new Color(1, 0, 0, 0.5f), ExplosionRadius);
        }

        // do explosion collision check
        ExplosionCollider.transform.position = position;
        ExplosionCollider.transform.localScale = new Vector3(ExplosionRadius * 2, ExplosionRadius * 2, 1f);
        ExplosionCollisionCheck();
    }

    private void ExplosionCollisionCheck()
    {
        // sync colliders to current transform
        Physics2D.SyncTransforms();

        // do a collision check
        List<Collider2D> enemyHitColliders = new List<Collider2D>();
        ExplosionCollider.Overlap(PlayerAbility.GetContactFilter(new string[] { "EnemyHurt", "Destructible" }), enemyHitColliders);
        foreach (var hit in enemyHitColliders)
        {
            if (hit.HasComponent<NetworkCharacter>())
            {
                var damage = PlayerAbility.GetRandomVariation(DamagePerHit * ExplosionDamageMultiplier);
                var isCritical = PlayerAbility.IsCriticalAttack(CriticalChance);
                damage = (int)(isCritical ? damage * CriticalDamage : damage);
                hit.GetComponent<NetworkCharacter>().TakeDamage(damage, isCritical);
            }

            if (hit.HasComponent<Destructible>())
            {
                var destructible = hit.GetComponent<Destructible>();
                destructible.TakeDamage(WeaponType);
            }
        }

        // screen shake
        var playerCameras = FindObjectsByType<PlayerCamera>(FindObjectsSortMode.None);
        foreach (var playerCamera in playerCameras)
        {
            if (playerCamera.GetComponent<NetworkObject>().IsLocalPlayer)
            {
                playerCamera.Shake(1.5f, 0.3f);
            }
        }

        // clear out colliders
        enemyHitColliders.Clear();
    }

    void Deactivate(Vector3 hitPosition)
    {
        VisualEffectsManager.Singleton.SpawnBulletExplosion(hitPosition);
        gameObject.SetActive(false);

        if (Role == PlayerAbility.NetworkRole.Server)
        {
            DeactivateClientRpc(hitPosition);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void DeactivateClientRpc(Vector3 hitPosition)
    {
        if (Role == PlayerAbility.NetworkRole.RemoteClient)
        {
            VisualEffectsManager.Singleton.SpawnBulletExplosion(hitPosition);
            gameObject.SetActive(false);
        }
    }

    public static void InitSpawnProjectileOnServer(ref GameObject projectile, ref NetworkVariable<ulong> projectileId, GameObject prefab)
    {
        // instantiate/spawn our projectile we'll be using when this ability activates
        // and initially set to deactivated
        projectile = Instantiate(prefab);
        projectile.GetComponent<NetworkObject>().Spawn();
        projectileId.Value = projectile.GetComponent<NetworkObject>().NetworkObjectId;
        projectile.SetActive(false);
    }

    public static void TryAddProjectileOnClient(ref GameObject projectile,
        ref NetworkVariable<ulong> projectileId, NetworkManager networkManager)
    {
        if (projectile == null && projectileId.Value > 0)
        {
            projectile = networkManager.SpawnManager.SpawnedObjects[projectileId.Value].gameObject;
            projectile.SetActive(false);
        }
    }
}
