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
        Player = player;
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

    [Rpc(SendTo.ClientsAndHost)]
    void LogFireDetailsClientRpc(Vector3 pos, Vector3 dir, float duration)
    {
        Debug.Log("Server Projectile: ");
        Debug.Log("Position: " + pos);
        Debug.Log("Direction: " + dir);
        Debug.Log("Duration: " + duration);
    }

    [Rpc(SendTo.Server)]
    void LogFireDetailsServerRpc(Vector3 pos, Vector3 dir, float duration)
    {
        Debug.Log("Client Projectile: ");
        Debug.Log("Position: " + pos);
        Debug.Log("Direction: " + dir);
        Debug.Log("Duration: " + duration);
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

        if (Role == PlayerAbility.NetworkRole.LocalClient)
        {

        }

        if (Role == PlayerAbility.NetworkRole.Server)
        {
            //if (!IsHost) PlayerAbility.RollbackEnemies(Player);
        }









        if (IsServer && !IsHost) PlayerAbility.RollbackEnemies(Player);

        // resync transforms
        Physics2D.SyncTransforms();

        // If we're on the server we need to do two passes of the collision check
        //  1. the first pass is the actual projectile radius and does a normal check for collision
        //  2. the second pass is a "tolerance" larger radius check. if the client thinks they
        //     got a hit they can ask the server to double check and if we got a tolerance hit it
        //      will register a hit

        // common variables for each cast
        Vector2 castDirection = Direction.normalized;
        float castDistance = m_speed * Time.deltaTime;
        RaycastHit2D[] hits = new RaycastHit2D[1];

        var firstCastRadius = m_collider.GetComponent<CircleCollider2D>().radius;
        var secondCastRadius = 3 * firstCastRadius;

        // FIRST CAST - Normal Hit Check
        // Use ColliderCast to perform continuous collision detection
        m_collider.GetComponent<CircleCollider2D>().radius = firstCastRadius;
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
                hit.GetComponent<NetworkCharacter>().TakeDamage(damage, isCritical, Player);

                var enemyAI = hit.GetComponent<Dropt.EnemyAI>();
                if (enemyAI != null)
                {
                    enemyAI.Knockback(castDirection, KnockbackDistance, KnockbackStunDuration);
                }
            }
            else if (hit.HasComponent<Destructible>())
            {
                var destructible = hit.GetComponent<Destructible>();
                destructible.TakeDamage(WeaponType, Player.GetComponent<NetworkObject>().NetworkObjectId);
            }
            ExplodeAndDeactivate(hitInfo.point);


            if (Player != null)
            {
                //Player.GetComponent<PlayerCamera>().Shake();
            }
        }

        // SECOND CAST - Tolerance Hit Check
        if (IsServer)
        {
            m_collider.GetComponent<CircleCollider2D>().radius = secondCastRadius;
            hitCount = m_collider.Cast(castDirection,
                PlayerAbility.GetContactFilter(new string[] { "EnemyHurt", "Destructible", "EnvironmentWall" }),
                hits, castDistance);
        }

        // reset to original radius
        m_collider.GetComponent<CircleCollider2D>().radius = firstCastRadius;

        if (IsServer && !IsHost) PlayerAbility.UnrollEnemies();
    }

    [Rpc(SendTo.Server)]
    void LogHitPointServerRpc(Vector2 hitPoint)
    {
        Debug.Log("Client Hit: " + hitPoint);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void LogHitPointClientRpc(Vector2 hitPoint)
    {
        Debug.Log("Server Hit: " + hitPoint);
    }

    void ExplodeAndDeactivate(Vector3 hitPosition)
    {
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
