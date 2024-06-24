using Language.Lua;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CleaveCycloneProjectile : MonoBehaviour
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

    [HideInInspector] public bool IsServer = false;

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

    //public override void OnNetworkSpawn()
    private void Start()
    {
        //if (!IsServer) return;

        if (Duration < 2 * GrowShrinkTime)
        {
            Duration = 2 * GrowShrinkTime;
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
        NumberHits--;
    }

    private void Update()
    {
        //if (!IsServer) return;
        if (!m_isSpawned) return;

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
            Destroy(gameObject);
        }

        m_timer -= Time.deltaTime;

        transform.position += Direction * m_speed * Time.deltaTime;

        // each frame do our collision checks
        Physics2D.SyncTransforms();

        // do a collision check
        List<Collider2D> enemyHitColliders = new List<Collider2D>();
        m_collider.Overlap(PlayerAbility.GetContactFilter(new string[] {"EnemyHurt", "Destructible"}), enemyHitColliders);
        foreach (var hit in enemyHitColliders)
        {
            if (hit.HasComponent<NetworkCharacter>())
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
                        damage = GetRandomVariation(damage);
                        var isCritical = IsCriticalAttack(CriticalChance);
                        damage = (int)(isCritical ? damage * CriticalDamage : damage);
                        hit.GetComponent<NetworkCharacter>().TakeDamage(damage, isCritical);
                    }

                    if (hit.HasComponent<Destructible>())
                    {
                        var destructible = hit.GetComponent<Destructible>();
                        destructible.TakeDamage(Wearable.WeaponTypeEnum.Cleave);
                    }
                }

            }
        }
        // clear out colliders
        enemyHitColliders.Clear();

        m_hitClearTimer += Time.deltaTime;
        if (m_hitClearTimer > m_hitClearInterval && NumberHits > 0)
        {
            m_hitClearTimer = 0;
            m_hitColliders.Clear();
            NumberHits--;
        }
    }

    public int GetRandomVariation(float baseValue, float randomVariation = 0.1f)
    {
        return (int)UnityEngine.Random.Range(
            baseValue * (1 - randomVariation),
            baseValue * (1 + randomVariation));
    }

    public bool IsCriticalAttack(float criticalChance)
    {
        var rand = UnityEngine.Random.Range(0f, 0.999f);
        return rand < criticalChance;
    }
}
