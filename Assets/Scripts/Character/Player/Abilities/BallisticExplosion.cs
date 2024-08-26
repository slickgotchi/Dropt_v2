using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Hierarchy;

public class BallisticExplosion : PlayerAbility
{
    [Header("BallisticExplosion Parameters")]
    public float Projection = 1.5f;
    public float Distance = 8f;
    public float Duration = 1f;
    public float DamageMultiplier = 3f;

    [Header("Projectile Prefab")]
    public GameObject BulletPrefab;
    public GameObject ArrowPrefab;
    public GameObject BasketballPrefab;

    // variables for keeping track of the spawned projectile
    private GameObject m_bulletProjectile;
    private NetworkVariable<ulong> m_bulletProjectileId = new NetworkVariable<ulong>(0);

    private GameObject m_arrowProjectile;
    private NetworkVariable<ulong> m_arrowProjectileId = new NetworkVariable<ulong>(0);

    private GameObject m_basketballProjectile;
    private NetworkVariable<ulong> m_basketballProjectileId = new NetworkVariable<ulong>(0);

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        GenericProjectile.InitSpawnProjectileOnServer(ref m_bulletProjectile, ref m_bulletProjectileId, BulletPrefab);
        GenericProjectile.InitSpawnProjectileOnServer(ref m_arrowProjectile, ref m_arrowProjectileId, ArrowPrefab);
        GenericProjectile.InitSpawnProjectileOnServer(ref m_basketballProjectile, ref m_basketballProjectileId, BasketballPrefab);
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

        if (m_bulletProjectile != null) m_bulletProjectile.GetComponent<NetworkObject>().Despawn();
        if (m_arrowProjectile != null) m_arrowProjectile.GetComponent<NetworkObject>().Despawn();
        if (m_basketballProjectile != null) m_basketballProjectile.GetComponent<NetworkObject>().Despawn();
    }

    private void Update()
    {
        if (IsClient)
        {
            // ensure remote clients associate projectiles with local projectile variables
            GenericProjectile.TryAddProjectileOnClient(ref m_bulletProjectile, ref m_bulletProjectileId, NetworkManager);
            GenericProjectile.TryAddProjectileOnClient(ref m_arrowProjectile, ref m_arrowProjectileId, NetworkManager);
            GenericProjectile.TryAddProjectileOnClient(ref m_basketballProjectile, ref m_basketballProjectileId, NetworkManager);
        }
    }

    public override void OnStart()
    {
        // set rotatin/local position
        SetRotationToActionDirection();
        SetLocalPosition(PlayerAbilityCentreOffset);

        // play animation
        PlayAnimation("BallisticShot");

        // activate projectile
        ActivateProjectile(ActivationWearableNameEnum, ActivationInput.actionDirection, Distance, Duration);
    }

    ref GameObject GetProjectileInstance(Wearable.NameEnum activationWearable)
    {
        switch (activationWearable)
        {
            case Wearable.NameEnum.AagentPistol: return ref m_bulletProjectile;
            case Wearable.NameEnum.NailGun: return ref m_bulletProjectile;
            case Wearable.NameEnum.Basketball: return ref m_basketballProjectile;
            default: return ref m_arrowProjectile;
        }
    }

    void ActivateProjectile(Wearable.NameEnum activationWearable, Vector3 direction, float distance, float duration)
    {
        GameObject projectile = GetProjectileInstance(activationWearable);
        var no_projectile = projectile.GetComponent<GenericProjectile>();
        var no_projectileId = no_projectile.GetComponent<NetworkObject>().NetworkObjectId;
        var playerCharacter = Player.GetComponent<NetworkCharacter>();
        var startPosition =
                Player.GetComponent<PlayerPrediction>().GetInterpPositionAtTick(ActivationInput.tick)
                + new Vector3(0, 0.5f, 0)
                + ActivationInput.actionDirection * Projection;

        // Local Client & Server
        if (Player.GetComponent<NetworkObject>().IsLocalPlayer || IsServer)
        {
            // init
            no_projectile.Init(startPosition, direction, distance, duration, 1,
                IsServer ? PlayerAbility.NetworkRole.Server : PlayerAbility.NetworkRole.LocalClient,
                Wearable.WeaponTypeEnum.Ballistic, Player,
                playerCharacter.AttackPower.Value * ActivationWearable.RarityMultiplier * DamageMultiplier,
                playerCharacter.CriticalChance.Value,
                playerCharacter.CriticalDamage.Value);

            // fire
            no_projectile.Fire();
        }

        // Server Only
        if (IsServer)
        {
            ulong playerId = Player.GetComponent<NetworkObject>().NetworkObjectId;
            ActivateProjectileClientRpc(startPosition,
                direction, distance, duration,
                playerId, no_projectileId);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ActivateProjectileClientRpc(Vector3 startPosition, Vector3 direction, 
        float distance, float duration, ulong playerNetworkObjectId, ulong projectileNetworkObjectId)
    {
        // Remote Client
        Player = NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId].gameObject;
        if (!Player) return;


        // Remote Client
        if (!Player.GetComponent<NetworkObject>().IsLocalPlayer)
        {
            var no_projectile = NetworkManager.SpawnManager.SpawnedObjects[projectileNetworkObjectId].
                GetComponent<GenericProjectile>();

            // init
            no_projectile.Init(startPosition, direction, distance, duration, 1,
                PlayerAbility.NetworkRole.RemoteClient,
                Wearable.WeaponTypeEnum.Ballistic, Player,
                0, 0, 0);

            // init
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
