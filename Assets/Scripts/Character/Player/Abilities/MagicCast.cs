using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class MagicCast : PlayerAbility
{
    [Header("MagicCast Parameters")]
    public float Projection = 1.5f;
    public float Distance = 8f;
    public float Duration = 1f;

    [Header("Projectile Prefab")]
    public GameObject MagicOrbPrefab;

    // variables for keeping track of the spawned projectile
    private GameObject m_orbProjectile;
    private NetworkVariable<ulong> m_orbProjectileId = new NetworkVariable<ulong>(0);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer) return;

        GenericProjectile.InitSpawnProjectileOnServer(ref m_orbProjectile, ref m_orbProjectileId, MagicOrbPrefab);
    }

    public override void OnNetworkDespawn()
    {
        if (m_orbProjectile != null)
        {
            if (IsServer) m_orbProjectile.GetComponent<NetworkObject>().Despawn();
        }

        base.OnNetworkDespawn();
    }

    protected override void Update()
    {
        base.Update();

        if (IsClient)
        {
            GenericProjectile.TryAddProjectileOnClient(ref m_orbProjectile, ref m_orbProjectileId, NetworkManager);
        }
    }

    public override void OnStart()
    {
        // set rotatin/local position
        SetRotationToActionDirection();
        SetLocalPosition(PlayerAbilityCentreOffset);

        // play animation
        //PlayAnimation("MagicCast");
        PlayAnimationWithDuration("MagicCast", ExecutionDuration);

        // activate projectile
        ActivateProjectile(ActivationWearableNameEnum, ActivationInput.actionDirection, Distance, Duration);
    }

    ref GameObject GetProjectileInstance(Wearable.NameEnum activationWearable)
    {
        return ref m_orbProjectile;
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
                Wearable.WeaponTypeEnum.Magic, Player,
                playerCharacter.AttackPower.Value * ActivationWearable.RarityMultiplier * DamageMultiplier,
                playerCharacter.CriticalChance.Value,
                playerCharacter.CriticalDamage.Value,
                ActivationInput.actionDirection,
                KnockbackDistance,
                KnockbackStunDuration);

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
                Wearable.WeaponTypeEnum.Magic, Player,
                0, 0, 0,
                Vector3.right, 0, 0);

            // init
            no_projectile.Fire();
        }
    }

    public override void OnFinish()
    {
    }
}
