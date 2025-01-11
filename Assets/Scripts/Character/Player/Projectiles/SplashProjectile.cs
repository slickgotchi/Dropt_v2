using System.Collections.Generic;
using System.Data;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class SplashProjectile : NetworkBehaviour
{
    [HideInInspector] public Vector3 Direction;
    [HideInInspector] public float Distance;
    [HideInInspector] public float Duration;
    [HideInInspector] public float ExplosionRadius = 1f;
    [HideInInspector] public float LobHeight = 2f;
    [HideInInspector] public float Scale = 1f;

    [HideInInspector] public float KnockbackDistance;
    [HideInInspector] public float KnockbackStunDuration;

    [HideInInspector] public float DamagePerHit = 1f;
    [HideInInspector] public float CriticalChance = 0.1f;
    [HideInInspector] public float CriticalDamage = 1.5f;
    [HideInInspector] public Wearable.WeaponTypeEnum WeaponType;

    [HideInInspector] public Wearable.NameEnum WearableNameEnum;

    [HideInInspector] public GameObject LocalPlayer;

    [HideInInspector] public PlayerAbility.NetworkRole Role = PlayerAbility.NetworkRole.LocalClient;

    public SpriteRenderer bodySpriteRenderer;
    public SpriteRenderer shadowSpriteRenderer;
    public CircleCollider2D Collider;

    private float m_timer = 0;
    private bool m_isSpawned = false;
    private float m_speed = 1;
    private Vector3 m_finalPosition;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        transform.parent = null;
        gameObject.SetActive(false);
    }

    public void Init(
        // server, local & remote
        Vector3 position,
        Vector3 direction,
        float distance,
        float duration,
        float scale,
        float explosionRadius,

        PlayerAbility.NetworkRole role,
        Wearable.WeaponTypeEnum weaponType,
        Wearable.NameEnum wearableNameEnum,

        // server & local only
        GameObject player,
        float damagePerHit,
        float criticalChance,
        float criticalDamage,

        // knockback
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
        ExplosionRadius = explosionRadius;
        WearableNameEnum = wearableNameEnum;

        // server & local only
        LocalPlayer = player;
        DamagePerHit = damagePerHit;
        CriticalChance = criticalChance;
        CriticalDamage = criticalDamage;

        // knockback
        KnockbackDistance = knockbackDistance;
        KnockbackStunDuration = knockbackStunDuration;
    }

    public void Fire()
    {
        gameObject.SetActive(true);

        transform.parent = null;

        transform.rotation = Quaternion.identity;

        transform.localScale = new Vector3(Scale, Scale, 1f);

        m_timer = Duration;
        m_isSpawned = true;
        m_speed = Distance / Duration;

        m_finalPosition = transform.position + Direction * Distance;

        Collider.radius = ExplosionRadius;

        GetComponent<LobArc>().Reset();
        GetComponent<LobArc>().Duration_s = Duration;
        GetComponent<LobArc>().MaxHeight = LobHeight;

        var wearable = WearableManager.Instance.GetWearable(WearableNameEnum);
        var wearablesSprite = WeaponSpriteManager.Instance.GetSprite(WearableNameEnum, wearable.AttackView);
        bodySpriteRenderer.sprite = wearablesSprite;
        bodySpriteRenderer.enabled = true;
        shadowSpriteRenderer.enabled = true;
    }

    private void Update()
    {
        if (!m_isSpawned) return;

        m_timer -= Time.deltaTime;

        if (m_timer < 0)
        {
            transform.position = m_finalPosition;
            if (Role != PlayerAbility.NetworkRole.RemoteClient) CollisionCheck();
            gameObject.SetActive(false);
            VisualEffectsManager.Instance.SpawnSplashExplosion(m_finalPosition, new Color(1, 0, 0, 0.5f), ExplosionRadius);

            bodySpriteRenderer.enabled = false;
            shadowSpriteRenderer.enabled = false;
        }

        transform.position += Direction * m_speed * Time.deltaTime;
    }

    public void CollisionCheck()
    {
        if (IsServer && !IsHost) PlayerAbility.RollbackEnemies(LocalPlayer);

        // resync transforms
        Physics2D.SyncTransforms();

        // do a collision check
        List<Collider2D> enemyHitColliders = new List<Collider2D>();
        Collider.OverlapCollider(PlayerAbility.GetContactFilter(new string[] { "EnemyHurt", "Destructible" }), enemyHitColliders);
        foreach (var hit in enemyHitColliders)
        {
            if (hit.HasComponent<NetworkCharacter>())
            {
                var isCritical = PlayerAbility.IsCriticalAttack(CriticalChance);
                var damage = (int)(isCritical ? DamagePerHit * CriticalDamage : DamagePerHit);
                hit.GetComponent<NetworkCharacter>().TakeDamage(damage, isCritical);
                var knockbackDirection = (Dropt.Utils.Battle.GetAttackCentrePosition(hit.gameObject) - transform.position).normalized;
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
        var playerControllers = Game.Instance.playerControllers;
        foreach (var pc in playerControllers)
        {
            var playerCamera = pc.GetComponent<PlayerCamera>();
            if (playerCamera.GetComponent<NetworkObject>().IsLocalPlayer)
            {
                playerCamera.Shake(1.5f, 0.3f);
            }
        }

        // clear out colliders
        enemyHitColliders.Clear();

        if (IsServer && !IsHost) PlayerAbility.UnrollEnemies();
    }

    void Deactivate(Vector3 hitPosition)
    {
        VisualEffectsManager.Instance.SpawnBulletExplosion(hitPosition);
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
            VisualEffectsManager.Instance.SpawnBulletExplosion(hitPosition);
            gameObject.SetActive(false);
        }
    }
}
