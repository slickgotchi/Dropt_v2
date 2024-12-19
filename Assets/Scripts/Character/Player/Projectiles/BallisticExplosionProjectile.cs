using System.Collections.Generic;
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

    [HideInInspector] public Vector3 KnockbackDirection;
    [HideInInspector] public float KnockbackDistance;
    [HideInInspector] public float KnockbackStunDuration;
    [HideInInspector] public float ExplosionKnockbackDistance;
    [HideInInspector] public float ExplosionKnockbackStunDuration;

    [HideInInspector] public float DamagePerHit = 1f;
    [HideInInspector] public float CriticalChance = 0.1f;
    [HideInInspector] public float CriticalDamage = 1.5f;
    [HideInInspector] public Wearable.WeaponTypeEnum WeaponType;

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
        // destroy the ExplosionCollider which was deparented
        Destroy(ExplosionCollider.gameObject);

        base.OnNetworkDespawn();
    }

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
        float explosionDamageMultiplier,

        // knockback
        Vector3 knockbackDirection,
        float knockbackDistance,
        float knockbackStunDuration,
        float explosionKnockbackDistance,
        float explosionKnockbackStunDuration
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

        KnockbackDirection = knockbackDirection;
        KnockbackDistance = knockbackDistance;
        KnockbackStunDuration = knockbackStunDuration;
        ExplosionKnockbackDistance = explosionKnockbackDistance;
        ExplosionKnockbackStunDuration = explosionKnockbackStunDuration;

   
    }

    public void Fire()
    {
        gameObject.SetActive(true);

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
            if (VisualGameObject != null) Destroy(VisualGameObject);
            Explode(transform.position);
            gameObject.SetActive(false);
        }

        transform.position += Direction * m_speed * Time.deltaTime;
        transform.rotation = PlayerAbility.GetRotationFromDirection(Direction);

        if (IsClient)
        {
            VisualGameObject.transform.position = transform.position;
            VisualGameObject.transform.rotation = transform.rotation;
        }

        if (Role != PlayerAbility.NetworkRole.RemoteClient) CollisionCheck();
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
                var damage = PlayerAbility.GetRandomVariation(DamagePerHit * ProjectileDamageMultiplier);
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
            Deactivate(hitInfo.point);

            if (LocalPlayer != null)
            {
                LocalPlayer.GetComponent<PlayerCamera>().Shake();

                // visual effect
                VisualEffectsManager.Instance.SpawnSplashExplosion(hitInfo.point, new Color(1, 0, 0, 0.5f), ExplosionRadius);
            }

            Explode(hitInfo.point);
        }

        if (IsServer && !IsHost) PlayerAbility.UnrollEnemies();
    }

    void Explode(Vector3 position)
    {
        if (LocalPlayer != null)
        {
            // visual effect
            //VisualEffectsManager.Instance.SpawnSplashExplosion(position, new Color(1, 0, 0, 0.5f), ExplosionRadius);
        }

        // do explosion collision check
        ExplosionCollider.transform.parent = null;
        ExplosionCollider.transform.position = position;
        ExplosionCollider.transform.localScale = new Vector3(ExplosionRadius * 2, ExplosionRadius * 2, 1f);
        ExplosionCollisionCheck(position);
    }

    private void ExplosionCollisionCheck(Vector3 position)
    {
        // sync colliders to current transform
        Physics2D.SyncTransforms();
        // do a collision check
        List<Collider2D> enemyHitColliders = new List<Collider2D>();
        ExplosionCollider.OverlapCollider(PlayerAbility.GetContactFilter(new string[] { "EnemyHurt", "Destructible" }), enemyHitColliders);
        foreach (var hit in enemyHitColliders)
        {
            if (hit.HasComponent<NetworkCharacter>())
            {
                var damage = PlayerAbility.GetRandomVariation(DamagePerHit * ExplosionDamageMultiplier);
                var isCritical = PlayerAbility.IsCriticalAttack(CriticalChance);
                damage = (int)(isCritical ? damage * CriticalDamage : damage);
                hit.GetComponent<NetworkCharacter>().TakeDamage(damage, isCritical, LocalPlayer);
                var knockbackDirection = (Dropt.Utils.Battle.GetAttackCentrePosition(hit.gameObject) - position).normalized;
                var enemyAI = hit.GetComponent<Dropt.EnemyAI>();
                if (enemyAI != null)
                {
                    enemyAI.Knockback(knockbackDirection, KnockbackDistance, KnockbackStunDuration);
                }
            }

            if (hit.HasComponent<Destructible>())
            {
                var destructible = hit.GetComponent<Destructible>();
                destructible.TakeDamage(WeaponType, LocalPlayer.GetComponent<NetworkObject>().NetworkObjectId);
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
        if (VisualGameObject != null) Destroy(VisualGameObject);
        VisualEffectsManager.Instance.SpawnSplashExplosion(hitPosition, new Color(1, 0, 0, 0.5f), ExplosionRadius);
        VisualEffectsManager.Instance.SpawnBulletExplosion(hitPosition);
        if (VisualGameObject != null) VisualGameObject.SetActive(false);

        gameObject.SetActive(false);

        if (Role == PlayerAbility.NetworkRole.Server)
        {
            DeactivateClientRpc(hitPosition, ExplosionRadius);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void DeactivateClientRpc(Vector3 hitPosition, float explosionRadius)
    {
        if (Role == PlayerAbility.NetworkRole.RemoteClient)
        {
            VisualEffectsManager.Instance.SpawnSplashExplosion(hitPosition, new Color(1, 0, 0, 0.5f), explosionRadius);
            VisualEffectsManager.Instance.SpawnBulletExplosion(hitPosition);
            if (VisualGameObject != null) VisualGameObject.SetActive(false);

            gameObject.SetActive(false);
        }
    }

    //public static void InitSpawnProjectileOnServer(ref GameObject projectile, ref NetworkVariable<ulong> projectileId, GameObject prefab)
    //{
    //    // instantiate/spawn our projectile we'll be using when this ability activates
    //    // and initially set to deactivated
    //    projectile = Instantiate(prefab);
    //    projectile.GetComponent<NetworkObject>().Spawn();
    //    projectileId.Value = projectile.GetComponent<NetworkObject>().NetworkObjectId;
    //    projectile.SetActive(false);
    //}

    //public static void TryAddProjectileOnClient(ref GameObject projectile,
    //    ref NetworkVariable<ulong> projectileId, NetworkManager networkManager)
    //{
    //    if (projectile == null && projectileId.Value > 0)
    //    {
    //        projectile = networkManager.SpawnManager.SpawnedObjects[projectileId.Value].gameObject;
    //        projectile.SetActive(false);
    //    }
    //}
}
