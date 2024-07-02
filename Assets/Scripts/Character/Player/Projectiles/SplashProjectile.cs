using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SplashProjectile : NetworkBehaviour
{
    [HideInInspector] public Vector3 Direction;
    [HideInInspector] public float Distance;
    [HideInInspector] public float Duration;
    [HideInInspector] public float ExplosionRadius = 1f;
    [HideInInspector] public float LobHeight = 2f;

    [HideInInspector] public float DamagePerHit = 1f;
    [HideInInspector] public float CriticalChance = 0.1f;
    [HideInInspector] public float CriticalDamage = 1.5f;
    [HideInInspector] public Wearable.WeaponTypeEnum WeaponType;

    [HideInInspector] public GameObject LocalPlayer;

    [HideInInspector] public PlayerAbility.NetworkRole NetworkRole = PlayerAbility.NetworkRole.LocalClient;

    public CircleCollider2D Collider;

    private float m_timer = 0;
    private bool m_isSpawned = false;
    private float m_speed = 1;
    private Vector3 m_finalPosition;

    public void Fire()
    {
        gameObject.SetActive(true);

        transform.rotation = Quaternion.identity;

        m_timer = Duration;
        m_isSpawned = true;
        m_speed = Distance / Duration;

        m_finalPosition = transform.position + Direction * Distance;

        Collider.radius = ExplosionRadius;

        GetComponent<LobArc>().Reset();
        GetComponent<LobArc>().Duration_s = Duration;
        GetComponent<LobArc>().MaxHeight = LobHeight;
    }

    private void Update()
    {
        if (!m_isSpawned) return;

        m_timer -= Time.deltaTime;

        if (m_timer < 0)
        {
            transform.position = m_finalPosition;
            if (NetworkRole != PlayerAbility.NetworkRole.RemoteClient) CollisionCheck();
            gameObject.SetActive(false);
            VisualEffectsManager.Singleton.SpawnSplashExplosion(m_finalPosition, new Color(1, 0, 0, 0.5f), ExplosionRadius);
        }

        transform.position += Direction * m_speed * Time.deltaTime;
    }

    public void CollisionCheck()
    {
        // sync colliders to current transform
        Physics2D.SyncTransforms();

        // do a collision check
        List<Collider2D> enemyHitColliders = new List<Collider2D>();
        Collider.Overlap(PlayerAbility.GetContactFilter(new string[] { "EnemyHurt", "Destructible" }), enemyHitColliders);
        foreach (var hit in enemyHitColliders)
        {
            if (hit.HasComponent<NetworkCharacter>())
            {
                var isCritical = PlayerAbility.IsCriticalAttack(CriticalChance);
                var damage = (int)(isCritical ? DamagePerHit * CriticalDamage : DamagePerHit);
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

        if (NetworkRole == PlayerAbility.NetworkRole.Server)
        {
            DeactivateClientRpc(hitPosition);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void DeactivateClientRpc(Vector3 hitPosition)
    {
        if (NetworkRole == PlayerAbility.NetworkRole.RemoteClient)
        {
            VisualEffectsManager.Singleton.SpawnBulletExplosion(hitPosition);
            gameObject.SetActive(false);
        }
    }
}
