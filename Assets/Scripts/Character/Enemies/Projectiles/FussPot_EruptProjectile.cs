using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class FussPot_EruptProjectile : NetworkBehaviour
{
    [Header("FussPot_EruptProjectile Parameters")]
    public float HitRadius = 3f;
    public GameObject TargetMarker;
    public LobArc LobArcBody;

    [HideInInspector] public Vector3 Direction;
    [HideInInspector] public float Distance;
    [HideInInspector] public float Duration;

    [HideInInspector] public float DamagePerHit = 1f;
    [HideInInspector] public float CriticalChance = 0.1f;
    [HideInInspector] public float CriticalDamage = 1.5f;

    [HideInInspector] public float Scale = 1f;

    private float m_timer = 0;
    private bool m_isSpawned = false;
    private float m_speed = 1;
    private bool m_isCollided = false;

    private Collider2D m_collider;

    private Vector3 m_finalPosition = Vector3.zero;

    private SoundFX_ProjectileHitGround m_soundFX_ProjectileHitGround;

    private void Awake()
    {
        m_collider = GetComponentInChildren<Collider2D>();
        m_soundFX_ProjectileHitGround = GetComponent<SoundFX_ProjectileHitGround>();
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            VisualEffectsManager.Instance.SpawnCloudExplosion(transform.position);
            m_soundFX_ProjectileHitGround.Play();
        }

        base.OnNetworkDespawn();
    }

    public void Init(Vector3 position, Quaternion rotation, Vector3 direction, float distance, float duration, float damagePerHit,
        float criticalChance, float criticalDamage)
    {
        transform.SetPositionAndRotation(position, rotation);
        Direction = direction.normalized;
        Distance = distance;
        Duration = duration;
        DamagePerHit = damagePerHit;
        CriticalChance = criticalChance;
        CriticalDamage = criticalDamage;

        m_finalPosition = transform.position + Direction * Distance;
    }

    public void Fire()
    {
        gameObject.SetActive(true);

        m_timer = Duration;
        m_isSpawned = true;
        m_speed = Distance / Duration;

        LobArcBody.Duration_s = Duration;
    }

    private void Update()
    {
        if (!m_isSpawned) return;

        m_timer -= Time.deltaTime;

        transform.position += m_speed * Time.deltaTime * Direction;

        TargetMarker.transform.position = m_finalPosition;

        // update target marker size
        float alpha = (Duration - m_timer) / Duration;
        float targetScale = alpha * HitRadius * 2;
        TargetMarker.transform.localScale = new Vector3(targetScale, targetScale * 0.7f, 1);

        // handle collision checks
        if (IsServer && m_timer < 0 && !m_isCollided)
        {
            m_isCollided = true;

            m_collider.GetComponent<CircleCollider2D>().radius = HitRadius;

            var isCritical = Dropt.Utils.Battle.IsCriticalAttack(CriticalChance);
            var damage = isCritical ? DamagePerHit * CriticalDamage : DamagePerHit;
            damage = Dropt.Utils.Battle.GetRandomVariation(damage);

            // we need to delay the collision check to account for lag
            // - player interp is 2 ticks back of current position (check player interp in PlayerPrediction)
            // - 

            EnemyAbility.PlayerCollisionCheckAndDamage(m_collider, damage, isCritical);

            gameObject.SetActive(false);
            gameObject.GetComponent<NetworkObject>().Despawn();
        }
    }
}
