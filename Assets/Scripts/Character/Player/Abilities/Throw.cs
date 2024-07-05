using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Hierarchy;

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

    void InitProjectile(ref GameObject projectile, ref NetworkVariable<ulong> projectileId, GameObject prefab)
    {
        // instantiate/spawn our projectile we'll be using when this ability activates
        // and initially set to deactivated
        projectile = Instantiate(prefab);
        projectile.GetComponent<NetworkObject>().Spawn();
        projectileId.Value = projectile.GetComponent<NetworkObject>().NetworkObjectId;
        projectile.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            InitProjectile(ref m_throwProjectile, ref m_throwProjectileId, ThrowProjectilePrefab);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (m_throwProjectile != null) m_throwProjectile.GetComponent<NetworkObject>().Despawn();
    }

    void TryAddProjectile(ref GameObject projectile, ref NetworkVariable<ulong> projectileId)
    {
        if (projectile == null && projectileId.Value > 0)
        {
            projectile = NetworkManager.SpawnManager.SpawnedObjects[projectileId.Value].gameObject;
            projectile.SetActive(false);
        }
    }

    private void Update()
    {
        // ensure remote clients associate projectiles with local projectile variables
        TryAddProjectile(ref m_throwProjectile, ref m_throwProjectileId);
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
            ActivateProjectileClientRpc(ActivationWearableNameEnum, projectile.transform.position, direction, distance, duration);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ActivateProjectileClientRpc(Wearable.NameEnum activationWearable, Vector3 startPosition, Vector3 direction, float distance, float duration)
    {
        GameObject projectile = GetProjectileInstance(activationWearable);

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
