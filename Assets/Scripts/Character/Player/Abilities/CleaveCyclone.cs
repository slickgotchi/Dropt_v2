using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class CleaveCyclone : PlayerAbility
{
    [Header("CleaveCyclone Parameters")]
    public float Projection = 1.5f;
    public float Distance = 10f;
    public float Duration = 3f;
    public float Scale = 2f;
    //public float DamageMultiplierPerHit = 0.5f;
    public int NumberHits = 6;

    [Header("Projectile Prefab")]
    public GameObject ProjectilePrefab;

    // variables for keeping track of the spawned projectile on both local and remote client
    private GameObject m_projectile;
    private NetworkVariable<ulong> m_projectileId = new NetworkVariable<ulong>(0);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            GenericProjectile.InitSpawnProjectileOnServer(ref m_projectile, ref m_projectileId, ProjectilePrefab);
        }
    }

    public override void OnNetworkDespawn()
    {

        if (m_projectile != null)
        {
            if (IsServer) m_projectile.GetComponent<NetworkObject>().Despawn();
        }

        base.OnNetworkDespawn();

    }

    protected override void Update()
    {
        base.Update();

        if (IsClient)
        {
            GenericProjectile.TryAddProjectileOnClient(ref m_projectile, ref m_projectileId, NetworkManager);
        }
    }

    public override void OnStart()
    {
        // set rotatin/local position
        SetRotation(quaternion.identity);
        SetLocalPosition(PlayerAbilityCentreOffset + ActivationInput.actionDirection * Projection);

        // activate projectile
        ActivateProjectile(ActivationInput.actionDirection, Distance, Duration, Scale);
    }

    void ActivateProjectile(Vector3 direction, float distance, float duration, float scale)
    {
        var no_projectile = m_projectile.GetComponent<CleaveCycloneProjectile>();
        var projectileId = no_projectile.GetComponent<NetworkObject>().NetworkObjectId;
        var playerCharacter = Player.GetComponent<NetworkCharacter>();

        // Local Client & Server
        if (Player.GetComponent<NetworkObject>().IsLocalPlayer || IsServer)
        {
            // init projectile
            no_projectile.Init(
                transform.position,
                direction, 
                distance,
                duration,
                scale,
                IsServer ? PlayerAbility.NetworkRole.Server : PlayerAbility.NetworkRole.LocalClient,
                NumberHits,
                Player,
                playerCharacter.currentStaticStats.AttackPower * ActivationWearable.RarityMultiplier * DamageMultiplier,
                playerCharacter.currentStaticStats.CriticalChance,
                playerCharacter.currentStaticStats.CriticalDamage,
                KnockbackDistance,
                KnockbackStunDuration
                );

            // fire projectile
            no_projectile.Fire();
        }

        // Server Only
        if (IsServer)
        {
            ulong playerNetworkObjectid = Player.GetComponent<NetworkObject>().NetworkObjectId;
            ActivateProjectileClientRpc(direction, distance, duration, scale, NumberHits, playerNetworkObjectid, projectileId);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ActivateProjectileClientRpc(Vector3 direction, float distance, float duration, float scale, int numberHits,
        ulong playerNetworkObjectId, ulong projectileId)
    {
        // Remote Client
        Player = NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId].gameObject;
        if (!Player) return;

        if (!Player.GetComponent<NetworkObject>().IsLocalPlayer)
        {
            //var no_projectile = m_projectile.GetComponent<CleaveCycloneProjectile>();
            var no_projectile = NetworkManager.SpawnManager.SpawnedObjects[projectileId].GetComponent<CleaveCycloneProjectile>();

            // init
            no_projectile.Init(
                transform.position,
                direction,
                distance,
                duration,
                scale,
                PlayerAbility.NetworkRole.RemoteClient,
                numberHits,
                Player,
                0,
                0,
                0,
                0, 0);

            // fire
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
