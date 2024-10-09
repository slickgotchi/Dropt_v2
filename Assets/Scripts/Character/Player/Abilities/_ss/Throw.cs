using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class Throw : PlayerAbility
{
    [Header("Throw Parameters")]
    public float Projection = 0.5f;
    public float Distance = 6f;
    public float Duration = 1f;
    public float LobHeight = 2f;

    [Header("Projectile Prefab")]
    public GameObject ThrowProjectilePrefab;

    // variables for keeping track of the spawned projectile
    private GameObject m_throwProjectile;
    private NetworkVariable<ulong> m_throwProjectileId = new NetworkVariable<ulong>(0);

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        GenericProjectile.InitSpawnProjectileOnServer(ref m_throwProjectile, ref m_throwProjectileId, ThrowProjectilePrefab);
    }

    public override void OnNetworkDespawn()
    {
        if (m_throwProjectile != null)
        {
            if (IsServer) m_throwProjectile.GetComponent<NetworkObject>().Despawn();
        }
    }

    protected override void Update()
    {
        base.Update();

        // ensure remote clients associate projectiles with local projectile variables
        GenericProjectile.TryAddProjectileOnClient(ref m_throwProjectile, ref m_throwProjectileId, NetworkManager);
    }

    public override void OnStart()
    {
        // set rotatin/local position
        SetRotationToActionDirection();
        SetLocalPosition(PlayerAbilityCentreOffset + ActivationInput.actionDirection * Projection);

        // play animation (its the same as the SplashLob...)
        PlayAnimation("SplashLob");

        // activate projectile
        ActivateProjectile(ActivationWearableNameEnum, ActivationInput.actionDirection, Distance, Duration);
    }

    ref GameObject GetProjectileInstance(Wearable.NameEnum activationWearable)
    {
        return ref m_throwProjectile;
    }

    void ActivateProjectile(Wearable.NameEnum activationWearable, Vector3 direction, float distance, float duration)
    {
        GameObject projectile = GetProjectileInstance(activationWearable);

        // Local Client & Server
        if (Player.GetComponent<NetworkObject>().IsLocalPlayer || IsServer)
        {
            projectile.SetActive(true);
            projectile.transform.position =
                Player.GetComponent<PlayerPrediction>().GetInterpPositionAtTick(ActivationInput.tick)
                + new Vector3(0, 0.5f, 0)
                + ActivationInput.actionDirection * Projection;
            var no_projectile = projectile.GetComponent<ThrowProjectile>();
            no_projectile.Direction = direction;
            no_projectile.Distance = distance;
            no_projectile.Duration = duration;
            no_projectile.LocalPlayer = Player;
            no_projectile.WearableNameEnum = activationWearable;
            no_projectile.NetworkRole = IsServer ? PlayerAbility.NetworkRole.Server : PlayerAbility.NetworkRole.LocalClient;

            no_projectile.Fire();
        }

        // Server Only
        if (IsServer)
        {
            var playerNetworkObjectId = Player.GetComponent<NetworkObject>().NetworkObjectId;
            var projectileNetworkObjectId = projectile.GetComponent<NetworkObject>().NetworkObjectId;
            ActivateProjectileClientRpc(activationWearable, projectile.transform.position, 
                direction, distance, duration, playerNetworkObjectId, projectileNetworkObjectId);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ActivateProjectileClientRpc(Wearable.NameEnum activationWearable, Vector3 startPosition, Vector3 direction, float distance, float duration,
        ulong playerNetworkObjectId, ulong projectileNetworkObjectId)
    {
        Player = NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId].gameObject;
        if (!Player) return;

        GameObject projectile = NetworkManager.SpawnManager.SpawnedObjects[projectileNetworkObjectId].gameObject;

        // Remote Client
        if (!Player.GetComponent<NetworkObject>().IsLocalPlayer)
        {
            projectile.SetActive(true);
            projectile.transform.position = startPosition;
            var no_projectile = projectile.GetComponent<ThrowProjectile>();
            no_projectile.Direction = direction;
            no_projectile.Distance = distance;
            no_projectile.Duration = duration;
            no_projectile.WearableNameEnum= activationWearable;
            no_projectile.NetworkRole = PlayerAbility.NetworkRole.RemoteClient;

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
