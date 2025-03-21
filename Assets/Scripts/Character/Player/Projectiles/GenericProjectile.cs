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

    [HideInInspector] public GameObject Player;

    [HideInInspector] public PlayerAbility.NetworkRole Role = PlayerAbility.NetworkRole.LocalClient;

    [HideInInspector] public GameObject VisualGameObject;

    public GameObject SpawnOnHitPrefab;

    public enum HitObjectType { Destructible, EnemyCharacter, PlayerCharacter }

    private float m_timer = 0;
    private bool m_isSpawned = false;
    private float m_speed = 1;

    // private Collider2D m_collider;

    bool m_isFired = false;
    Vector3 m_previousDeterminatePosition;
    Vector3 m_currentDeterminatePosition;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        transform.parent = null;

        if (VisualGameObject != null) VisualGameObject.SetActive(false);

        NetworkTimer_v2.Instance.OnTick += Tick;
    }

    public override void OnNetworkDespawn()
    {
        NetworkTimer_v2.Instance.OnTick -= Tick;

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
        Player = player;
        DamagePerHit = damagePerHit;
        CriticalChance = criticalChance;
        CriticalDamage = criticalDamage;

        // knockback
        KnockbackDirection = knockbackDirection;
        KnockbackDistance = knockbackDistance;
        KnockbackStunDuration = knockbackStunDuration;

    }

    private int m_clientServerActivationTickDelta = 0;

    public void Fire(int activationClientTick = 0)
    {
        gameObject.SetActive(true);

        // if we're on client this will just be 0
        m_clientServerActivationTickDelta = activationClientTick -
            NetworkTimer_v2.Instance.TickCurrent;

        m_isFired = true;

        transform.localScale = new Vector3(Scale, Scale, 1f);

        m_timer = Duration;
        m_isSpawned = true;
        m_speed = Distance / Duration;

        if (VisualGameObject != null) VisualGameObject.SetActive(true);

        m_currentDeterminatePosition = transform.position;
        m_previousDeterminatePosition = m_currentDeterminatePosition - Direction * 0.01f;
    }



    void Tick()
    {
        if (!m_isFired) return;

        // we need to find the historic tick that we can reliably use for lag compensation
        int rollbackTargetTick = PerfectLagCompensation.GetRollbackTargetTickForPlayer(Player, 
            m_clientServerActivationTickDelta);

        // roll back all perfect lag compensated enemies and resync physics transforms
        PerfectLagCompensation.RollbackAllEntities(rollbackTargetTick);
        Physics2D.SyncTransforms();

        // move projectile to next determinate position
        m_previousDeterminatePosition = m_currentDeterminatePosition;
        m_currentDeterminatePosition += Direction * m_speed * NetworkTimer_v2.Instance.TickInterval_s;

        // circle cast
        CircleCastCollisionCheck();

        // unroll colliders
        PerfectLagCompensation.UnrollAllEntities();
    }

    void CircleCastCollisionCheck() {
        var radius = 0.2f;
        var direction = (m_currentDeterminatePosition - m_previousDeterminatePosition).normalized;
        var distance = math.distance(m_currentDeterminatePosition, m_previousDeterminatePosition);
        ContactFilter2D contactFilter = PlayerAbility.GetContactFilter(new string[] { "EnemyHurt", "Destructible", "EnvironmentWall" });

        // do circle cast
        RaycastHit2D hit2d = Physics2D.CircleCast(m_previousDeterminatePosition, radius, direction, distance, contactFilter.layerMask);
        if (hit2d.collider != null)
        {
            var hit = hit2d.collider;

            if (hit.HasComponent<NetworkCharacter>())
            {
                var damage = PlayerAbility.GetRandomVariation(DamagePerHit);
                var isCritical = PlayerAbility.IsCriticalAttack(CriticalChance);
                damage = (int)(isCritical ? damage * CriticalDamage : damage);
                hit.GetComponent<NetworkCharacter>().TakeDamage(damage, isCritical, Player);
            }
            else if (hit.HasComponent<Destructible>())
            {
                var destructible = hit.GetComponent<Destructible>();
                destructible.TakeDamage(WeaponType, Player.GetComponent<NetworkObject>().NetworkObjectId);
            }
            ExplodeAndDeactivate(hit2d.point);
        }
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
    }

    private void LateUpdate()
    {
        if (IsClient)
        {
            VisualGameObject.transform.position = transform.position;
            VisualGameObject.transform.rotation = transform.rotation;
        }
    }

    void ExplodeAndDeactivate(Vector3 hitPosition)
    {
        m_isFired = false;

        if (VisualGameObject != null) Destroy(VisualGameObject);

        VisualEffectsManager.Instance.Spawn_VFX_AttackHit(hitPosition);

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
            VisualEffectsManager.Instance.Spawn_VFX_AttackHit(hitPosition);

            if (VisualGameObject != null) VisualGameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}
