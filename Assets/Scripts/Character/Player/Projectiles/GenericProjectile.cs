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

    private int m_activationClientServerTickDelta = 0;

    public void Fire(int activationClientTick = 0)
    {
        gameObject.SetActive(true);

        // if we're on client this will just be 0
        m_activationClientServerTickDelta = activationClientTick -
            NetworkTimer_v2.Instance.TickCurrent;

        Debug.Log("activationClientTick: " + activationClientTick + ", currentTick: " +
            NetworkTimer_v2.Instance.TickCurrent);

        isFired = true;

        transform.localScale = new Vector3(Scale, Scale, 1f);

        m_timer = Duration;
        m_isSpawned = true;
        m_speed = Distance / Duration;

        m_collider = GetComponent<Collider2D>();

        if (VisualGameObject != null) VisualGameObject.SetActive(true);

        currentDeterminatePosition = transform.position;
        previousDeterminatePosition = currentDeterminatePosition - Direction * 0.01f;
    }

    bool isFired = false;
    Vector3 previousDeterminatePosition;
    Vector3 currentDeterminatePosition;

    void Tick()
    {
        if (!isFired) return;

        // roll back all colliders to a tick
        int targetTick = NetworkTimer_v2.Instance.TickCurrent;
            int interpolationLagTicks = 5;

        if (IsClient)
        {
            int tickDelta = 0;
            var playerPing = Player.GetComponent<PlayerPing>();
            if (playerPing != null)
            {
                tickDelta = playerPing.Client_ClientLocalServerReceived_TickDelta;
            }

            targetTick = 
                NetworkTimer_v2.Instance.TickCurrent -
                tickDelta -
                interpolationLagTicks + 1;

            //Debug.Log("Client targetTick: " + targetTick);
        }
        if (IsServer)
        {
            // we need to compare the activation tick client/server tick delta to the more
            // established PlayerPing client/server tick delta to see if we fired later or
            // earlier than usual and need to adjust for that
            int tickDelta = 0;
            var playerPing = Player.GetComponent<PlayerPing>();
            if (playerPing != null)
            {
                tickDelta = playerPing.Server_ClientReportingServerReceived_TickDelta;
            }


            targetTick = NetworkTimer_v2.Instance.TickCurrent +
                (m_activationClientServerTickDelta -
                tickDelta -
                interpolationLagTicks);

            //Debug.Log("Server targetTick: " + targetTick +
            //    ", currentTick: " + NetworkTimer_v2.Instance.TickCurrent +
            //    ", m_activationClientServerTickDelta: " + m_activationClientServerTickDelta +
            //    ", averageClientServerTickDelta: " + tickDelta);
        }

        // roll back all perfect lag compensated enemies
        var plces = FindObjectsByType<PerfectLagCompensation>(FindObjectsSortMode.None);
        foreach (var plce in plces)
        {
            plce.StashPosition();
            plce.SetPositionToTick(targetTick);
        }

        Physics2D.SyncTransforms();

        var dt = NetworkTimer_v2.Instance.TickInterval_s;
        var deltaPos = Direction * m_speed * dt;
        previousDeterminatePosition = currentDeterminatePosition;
        currentDeterminatePosition += deltaPos;

        var startPosition = previousDeterminatePosition;
        var radius = 0.2f;
        var direction = (currentDeterminatePosition - previousDeterminatePosition).normalized;
        var distance = math.distance(currentDeterminatePosition, previousDeterminatePosition);
        ContactFilter2D contactFilter = PlayerAbility.GetContactFilter(new string[] { "EnemyHurt", "Destructible", "EnvironmentWall" });
        RaycastHit2D hit2d = Physics2D.CircleCast(startPosition, radius, direction, distance, contactFilter.layerMask);
        if (hit2d.collider != null)
        {
            var hit = hit2d.collider;
            //Debug.Log("Hit: " + hit2d.collider.name);

            if (IsClient)
            {
                ReportClientHitServerRpc(targetTick, plces[0].transform.position);

            }

            if (IsServer && plces.Length > 0)
            {
                Debug.Log("Server Hit obj at targetTick: " + targetTick + ", pos: " + plces[0].transform.position);
            }

            if (hit.HasComponent<NetworkCharacter>())
            {
                var damage = PlayerAbility.GetRandomVariation(DamagePerHit);
                var isCritical = PlayerAbility.IsCriticalAttack(CriticalChance);
                damage = (int)(isCritical ? damage * CriticalDamage : damage);
                hit.GetComponent<NetworkCharacter>().TakeDamage(damage, isCritical, Player);

                //var enemyAI = hit.GetComponent<Dropt.EnemyAI>();
                //if (enemyAI != null)
                //{
                //    enemyAI.Knockback(castDirection, KnockbackDistance, KnockbackStunDuration);
                //}
            }
            else if (hit.HasComponent<Destructible>())
            {
                var destructible = hit.GetComponent<Destructible>();
                destructible.TakeDamage(WeaponType, Player.GetComponent<NetworkObject>().NetworkObjectId);
            }
            ExplodeAndDeactivate(hit2d.point);
        }


        // unroll colliders
        foreach (var plce in plces)
        {
            plce.RestorePositionFromStash();
        }
    }

    [Rpc(SendTo.Server)]
    void ReportClientHitServerRpc(int tickHit, Vector3 objPosition)
    {
        Debug.Log("Client Hit obj at targetTick: " + tickHit + ", pos: " + objPosition);
    }

    void RollBackPerfectLagCompsToTick(int tick)
    {

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

        //if (Role != PlayerAbility.NetworkRole.RemoteClient) CollisionCheck();
    }

    private void LateUpdate()
    {
        if (IsClient)
        {
            VisualGameObject.transform.position = transform.position;
            VisualGameObject.transform.rotation = transform.rotation;
        }
    }

    /*
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
    */


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
        isFired = false;

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
