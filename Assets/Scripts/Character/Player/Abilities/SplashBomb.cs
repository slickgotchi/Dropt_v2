using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Hierarchy;
using UnityEngine.UIElements;

public class SplashBomb : PlayerAbility
{
    [Header("SplashBomb Parameters")]
    public float Projection = 1.5f;
    public float Distance = 8f;
    public float Duration = 1f;
    public float ExplosionRadius = 1f;
    public float LobHeight = 2f;

    [Header("Projectile Prefab")]
    public GameObject SplashProjectilePrefab;

    // variables for keeping track of the spawned projectile
    private GameObject m_splashProjectile;
    private NetworkVariable<ulong> m_splashProjectileId = new NetworkVariable<ulong>(0);

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        GenericProjectile.InitSpawnProjectileOnServer(ref m_splashProjectile, ref m_splashProjectileId, SplashProjectilePrefab);
    }

    public override void OnNetworkDespawn()
    {
        if (m_splashProjectile != null)
        {
            if (IsServer) m_splashProjectile.GetComponent<NetworkObject>().Despawn();
        }
    }

    private void Update()
    {
        if (IsClient)
        {
            GenericProjectile.TryAddProjectileOnClient(ref m_splashProjectile, ref m_splashProjectileId, NetworkManager);
        }
    }

    public override void OnStart()
    {
        // set rotatin/local position
        SetRotationToActionDirection();
        SetLocalPosition(PlayerAbilityCentreOffset + ActivationInput.actionDirection * Projection);

        // play animation
        PlayAnimation("SplashLob");

        // activate projectile
        ActivateProjectile(ActivationWearableNameEnum, ActivationInput.actionDirection, Distance, Duration, 1f, ExplosionRadius);
    }

    ref GameObject GetProjectileInstance(Wearable.NameEnum activationWearable)
    {
        return ref m_splashProjectile;
    }

    void ActivateProjectile(Wearable.NameEnum wearableNameEnum, Vector3 direction, float distance, float duration,
        float scale, float explosionRadius)
    {
        GameObject projectile = GetProjectileInstance(wearableNameEnum);
        var no_projectile = projectile.GetComponent<SplashProjectile>();
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
            no_projectile.Init(
                startPosition, direction, distance, duration, scale, explosionRadius,

                IsServer ? PlayerAbility.NetworkRole.Server : PlayerAbility.NetworkRole.LocalClient,
                Wearable.WeaponTypeEnum.Splash, wearableNameEnum,

                Player,
                playerCharacter.AttackPower.Value * ActivationWearable.RarityMultiplier,
                playerCharacter.CriticalChance.Value,
                playerCharacter.CriticalDamage.Value);

            // fire
            no_projectile.Fire();
        }

        // Server Only
        if (IsServer)
        {
            ulong playerId = Player.GetComponent<NetworkObject>().NetworkObjectId;
            ActivateProjectileClientRpc(
                startPosition, direction, distance, duration, scale, explosionRadius, wearableNameEnum,
                playerId, no_projectileId);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ActivateProjectileClientRpc(Vector3 startPosition, Vector3 direction, 
        float distance, float duration, float scale, float explosionRadius, Wearable.NameEnum wearableNameEnum,
        ulong playerNetworkObjectId, ulong projectileNetworkObjectId)
    {
        // Remote Client
        Player = NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId].gameObject;
        if (!Player) return;

        // Remote Client
        if (!Player.GetComponent<NetworkObject>().IsLocalPlayer)
        {
            var no_projectile = NetworkManager.SpawnManager.SpawnedObjects[projectileNetworkObjectId].
                GetComponent<SplashProjectile>();

            // init
            no_projectile.Init(startPosition, direction, distance, duration, scale, explosionRadius,
                PlayerAbility.NetworkRole.RemoteClient,
                Wearable.WeaponTypeEnum.Splash, wearableNameEnum,

                Player,
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
