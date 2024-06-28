using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Hierarchy;

public class CleaveCyclone : PlayerAbility
{
    [Header("CleaveCyclone Parameters")]
    public float Projection = 1.5f;
    public float Distance = 10f;
    public float Duration = 3f;
    public float Scale = 2f;
    public float DamageMultiplierPerHit = 0.5f;
    public int NumberHits = 6;

    public GameObject ProjectilePrefab;

    private GameObject m_projectile;
    private NetworkVariable<ulong> m_projectileId = new NetworkVariable<ulong>(0);

    public override void OnNetworkSpawn()
    {
        Animator = GetComponent<Animator>();

        if (IsServer)
        {
            m_projectile = Instantiate(ProjectilePrefab);
            m_projectile.GetComponent<NetworkObject>().Spawn();
            m_projectileId.Value = m_projectile.GetComponent<NetworkObject>().NetworkObjectId;
            m_projectile.SetActive(false);
        }
    }

    private void Update()
    {
        if (m_projectile == null && m_projectileId.Value > 0)
        {
            m_projectile = NetworkManager.SpawnManager.SpawnedObjects[m_projectileId.Value].gameObject;
            m_projectile.SetActive(false);
        }
    }

    public override void OnStart()
    {
        // set rotatin/local position
        SetRotation(quaternion.identity);
        SetLocalPosition(PlayerAbilityCentreOffset + ActivationInput.actionDirection * Projection);

        ActivateProjectile(ActivationInput.actionDirection, Distance, Duration, Scale);
    }

    void ActivateProjectile(Vector3 direction, float distance, float duration, float scale)
    {
        // Local Client or Server
        if (Player.GetComponent<NetworkObject>().IsLocalPlayer || IsServer)
        {
            m_projectile.SetActive(true);
            m_projectile.transform.position = transform.position;
            var no_projectile = m_projectile.GetComponent<CleaveCycloneProjectile>();
            no_projectile.Direction = direction;
            no_projectile.Distance = distance;
            no_projectile.Duration = duration;
            no_projectile.Scale = scale;

            var playerCharacter = Player.GetComponent<NetworkCharacter>();
            no_projectile.DamagePerHit = playerCharacter.AttackPower.Value;
            no_projectile.CriticalChance = playerCharacter.CriticalChance.Value;
            no_projectile.CriticalDamage = playerCharacter.CriticalDamage.Value;
            no_projectile.Role = IsServer ? AbilityRole.Server : AbilityRole.LocalClient;

            no_projectile.Fire();
        }

        // Server Only
        if (IsServer)
        {
            ActivateProjectileClientRpc(direction, distance, duration, scale);
        }

        //if (!Player.GetComponent<NetworkObject>().IsLocalPlayer) return;

        //ActivateProjectileServerRpc(direction, distance, duration, scale);
    }

    //[Rpc(SendTo.Server)]
    //void ActivateProjectileServerRpc(Vector3 direction, float distance, float duration, float scale)
    //{
    //    ActivateProjectileClientRpc(direction, distance, duration, scale);
    //}

    [Rpc(SendTo.ClientsAndHost)]
    void ActivateProjectileClientRpc(Vector3 direction, float distance, float duration, float scale)
    {
        // Remote Client
        if (!Player.GetComponent<NetworkObject>().IsLocalPlayer)
        {
            m_projectile.SetActive(true);
            m_projectile.transform.position = transform.position;
            var no_projectile = m_projectile.GetComponent<CleaveCycloneProjectile>();
            no_projectile.Direction = direction;
            no_projectile.Distance = distance;
            no_projectile.Duration = duration;
            no_projectile.Scale = scale;

            var playerCharacter = Player.GetComponent<NetworkCharacter>();
            no_projectile.DamagePerHit = playerCharacter.AttackPower.Value;
            no_projectile.CriticalChance = playerCharacter.CriticalChance.Value;
            no_projectile.CriticalDamage = playerCharacter.CriticalDamage.Value;
            no_projectile.Role = AbilityRole.RemoteClient;

            no_projectile.Fire();
        }
    }

    public override void OnUpdate()
    {

    }

    public override void OnFinish()
    {
    }
}
