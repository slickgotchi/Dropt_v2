using Language.Lua;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CleaveCycloneProjectile : NetworkBehaviour
{
    [HideInInspector] public Vector3 Direction;
    [HideInInspector] public float Distance;
    [HideInInspector] public float Duration;
    [HideInInspector] public float Scale;

    [HideInInspector] public float DamageMutiplierPerHit = 0.5f;
    [HideInInspector] public int NumberHits = 4;

    [HideInInspector] public float DamagePerHit = 1f;
    [HideInInspector] public float CriticalChance = 0.1f;
    [HideInInspector] public float CriticalDamage = 1.5f;

    [HideInInspector] public float KnockbackDistance;
    [HideInInspector] public float KnockbackStunDuration;

    [HideInInspector] public GameObject LocalPlayer;

    [HideInInspector] public PlayerAbility.NetworkRole Role = PlayerAbility.NetworkRole.LocalClient;

    public float GrowShrinkTime = 0.5f; // Time for scaling up and down

    private float m_timer = 0;
    private bool m_isSpawned = false;
    private float m_speed = 1;
    private Vector3 initialScale;
    private ParticleSystem particles;

    private List<Collider2D> m_hitColliders = new List<Collider2D>();
    private Collider2D m_collider;

    private float m_hitClearTimer = 0;
    private float m_hitClearInterval = 1;

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
        PlayerAbility.NetworkRole role,
        int numberHits,

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
        NumberHits = numberHits;

        // server & local only
        LocalPlayer = player;
        DamagePerHit = damagePerHit;
        CriticalChance = criticalChance;
        CriticalDamage = criticalDamage;

        // knockback
        KnockbackDistance = knockbackDistance;
        KnockbackStunDuration = knockbackStunDuration;

        // reset hit clear interval
        m_hitClearInterval = Duration / NumberHits;
    }

    public void Fire()
    {
        gameObject.SetActive(true);
        m_hitColliders.Clear();

        // ensure grow shrink time is not too large
        if (Duration < 2 * GrowShrinkTime)
        {
            GrowShrinkTime = Duration / 2;
        }

        m_timer = Duration;
        m_isSpawned = true;
        m_speed = Distance / Duration;

        GetComponent<Animator>().Play("CleaveCycloneProjectile");

        m_collider = GetComponent<Collider2D>();

        initialScale = new Vector3(Scale, Scale, 1);
        transform.localScale = Vector3.zero;

        particles = GetComponentInChildren<ParticleSystem>();
        particles.transform.localScale = transform.localScale;

        m_hitClearTimer = 0;
        m_hitClearInterval = Duration / NumberHits;

        // start decrementing NumberHits
        //NumberHits--;
    }

    private void Update()
    {
        if (!m_isSpawned) return;

        m_timer -= Time.deltaTime;

        float elapsedTime = Duration - m_timer;

        if (elapsedTime <= GrowShrinkTime)
        {
            float scaleFactor = elapsedTime / GrowShrinkTime;
            transform.localScale = initialScale * scaleFactor;
        }
        else if (elapsedTime >= Duration - GrowShrinkTime)
        {
            float scaleFactor = (Duration - elapsedTime) / GrowShrinkTime;
            transform.localScale = initialScale * scaleFactor;
        }
        else
        {
            transform.localScale = initialScale;
        }

        particles.transform.localScale = transform.localScale;

        if (m_timer < 0)
        {
            gameObject.SetActive(false);
        }


        transform.position += Direction * m_speed * Time.deltaTime;

        if (Role != PlayerAbility.NetworkRole.RemoteClient)
        {
            CollisionCheck();
        }

        m_hitClearTimer += Time.deltaTime;
        if (m_hitClearTimer > m_hitClearInterval)
        {
            m_hitClearTimer -= m_hitClearInterval;
            m_hitColliders.Clear();
        }
    }

    public void CollisionCheck()
    {
        // if (IsServer && !IsHost) PlayerAbility.RollbackEnemies(LocalPlayer);

        // each frame do our collision checks
        Physics2D.SyncTransforms();

        // do a collision check
        List<Collider2D> enemyHitColliders = new List<Collider2D>();
        m_collider.OverlapCollider(PlayerAbility.GetContactFilter(new string[] { "EnemyHurt", "Destructible" }), enemyHitColliders);
        foreach (var hit in enemyHitColliders)
        {
            bool isAlreadyHit = false;
            foreach (var hitCheck in m_hitColliders)
            {
                if (hitCheck == hit) isAlreadyHit = true;
            }
            if (!isAlreadyHit)
            {
                m_hitColliders.Add(hit);

                if (hit.HasComponent<NetworkCharacter>())
                {
                    var damage = DamagePerHit * DamageMutiplierPerHit;
                    damage = PlayerAbility.GetRandomVariation(damage);
                    var isCritical = PlayerAbility.IsCriticalAttack(CriticalChance);
                    damage = (int)(isCritical ? damage * CriticalDamage : damage);
                    hit.GetComponent<NetworkCharacter>().TakeDamage(damage, isCritical, LocalPlayer);
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
                    destructible.TakeDamage(Wearable.WeaponTypeEnum.Cleave, LocalPlayer.GetComponent<NetworkObject>().NetworkObjectId);
                }
            }
        }
        // clear out colliders
        enemyHitColliders.Clear();

        // if (IsServer && !IsHost) PlayerAbility.UnrollEnemies();
    }
}

