using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class BallisticExplosion : PlayerAbility
{
    [Header("BallisticExplosion Parameters")]
    public float Projection = 1.5f;
    public float Distance = 8f;
    public float Duration = 1f;
    public float ExplosionRadius = 2f;
    public float ExplosionDamageMultiplier = 0.5f;
    public float ExplosionKnockbackDistance = 0.5f;
    public float ExplosionKnockbackStunDuration = 0.5f;

    [Header("Projectile Prefab")]
    public GameObject ApplePrefab;
    public GameObject ArrowPrefab;
    public GameObject BasketballPrefab;
    public GameObject BulletPrefab;
    public GameObject CorkPrefab;
    public GameObject DrankPrefab;
    public GameObject MilkPrefab;
    public GameObject NailTrioPrefab;
    public GameObject SeedPrefab;

    // variables for keeping track of the spawned projectile
    private GameObject m_appleProjectile;
    private NetworkVariable<ulong> m_appleProjectileId = new NetworkVariable<ulong>(0);

    private GameObject m_arrowProjectile;
    private NetworkVariable<ulong> m_arrowProjectileId = new NetworkVariable<ulong>(0);

    private GameObject m_basketballProjectile;
    private NetworkVariable<ulong> m_basketballProjectileId = new NetworkVariable<ulong>(0);

    private GameObject m_bulletProjectile;
    private NetworkVariable<ulong> m_bulletProjectileId = new NetworkVariable<ulong>(0);

    private GameObject m_corkProjectile;
    private NetworkVariable<ulong> m_corkProjectileId = new NetworkVariable<ulong>(0);

    private GameObject m_drankProjectile;
    private NetworkVariable<ulong> m_drankProjectileId = new NetworkVariable<ulong>(0);

    private GameObject m_milkProjectile;
    private NetworkVariable<ulong> m_milkProjectileId = new NetworkVariable<ulong>(0);

    private GameObject m_nailTrioProjectile;
    private NetworkVariable<ulong> m_nailTrioProjectileId = new NetworkVariable<ulong>(0);

    private GameObject m_seedProjectile;
    private NetworkVariable<ulong> m_seedProjectileId = new NetworkVariable<ulong>(0);

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        GenericProjectile.InitSpawnProjectileOnServer(ref m_appleProjectile, ref m_appleProjectileId, ApplePrefab);
        GenericProjectile.InitSpawnProjectileOnServer(ref m_arrowProjectile, ref m_arrowProjectileId, ArrowPrefab);
        GenericProjectile.InitSpawnProjectileOnServer(ref m_basketballProjectile, ref m_basketballProjectileId, BasketballPrefab);
        GenericProjectile.InitSpawnProjectileOnServer(ref m_bulletProjectile, ref m_bulletProjectileId, BulletPrefab);
        GenericProjectile.InitSpawnProjectileOnServer(ref m_corkProjectile, ref m_corkProjectileId, CorkPrefab);
        GenericProjectile.InitSpawnProjectileOnServer(ref m_drankProjectile, ref m_drankProjectileId, DrankPrefab);
        GenericProjectile.InitSpawnProjectileOnServer(ref m_milkProjectile, ref m_milkProjectileId, MilkPrefab);
        GenericProjectile.InitSpawnProjectileOnServer(ref m_nailTrioProjectile, ref m_nailTrioProjectileId, ArrowPrefab);
        GenericProjectile.InitSpawnProjectileOnServer(ref m_seedProjectile, ref m_seedProjectileId, SeedPrefab);
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

        if (m_appleProjectile != null) m_appleProjectile.GetComponent<NetworkObject>().Despawn();
        if (m_arrowProjectile != null) m_arrowProjectile.GetComponent<NetworkObject>().Despawn();
        if (m_basketballProjectile != null) m_basketballProjectile.GetComponent<NetworkObject>().Despawn();
        if (m_bulletProjectile != null) m_bulletProjectile.GetComponent<NetworkObject>().Despawn();
        if (m_corkProjectile != null) m_corkProjectile.GetComponent<NetworkObject>().Despawn();
        if (m_drankProjectile != null) m_drankProjectile.GetComponent<NetworkObject>().Despawn();
        if (m_milkProjectile != null) m_milkProjectile.GetComponent<NetworkObject>().Despawn();
        if (m_nailTrioProjectile != null) m_nailTrioProjectile.GetComponent<NetworkObject>().Despawn();
        if (m_seedProjectile != null) m_seedProjectile.GetComponent<NetworkObject>().Despawn();
    }

    private void Update()
    {
        if (IsClient)
        {
            // ensure remote clients associate projectiles with local projectile variables
            GenericProjectile.TryAddProjectileOnClient(ref m_appleProjectile, ref m_appleProjectileId, NetworkManager);
            GenericProjectile.TryAddProjectileOnClient(ref m_arrowProjectile, ref m_arrowProjectileId, NetworkManager);
            GenericProjectile.TryAddProjectileOnClient(ref m_basketballProjectile, ref m_basketballProjectileId, NetworkManager);
            GenericProjectile.TryAddProjectileOnClient(ref m_bulletProjectile, ref m_bulletProjectileId, NetworkManager);
            GenericProjectile.TryAddProjectileOnClient(ref m_corkProjectile, ref m_corkProjectileId, NetworkManager);
            GenericProjectile.TryAddProjectileOnClient(ref m_drankProjectile, ref m_drankProjectileId, NetworkManager);
            GenericProjectile.TryAddProjectileOnClient(ref m_milkProjectile, ref m_milkProjectileId, NetworkManager);
            GenericProjectile.TryAddProjectileOnClient(ref m_nailTrioProjectile, ref m_nailTrioProjectileId, NetworkManager);
            GenericProjectile.TryAddProjectileOnClient(ref m_seedProjectile, ref m_seedProjectileId, NetworkManager);
        }
    }

    public override void OnStart()
    {
        // set rotatin/local position
        SetRotationToActionDirection();
        SetLocalPosition(PlayerAbilityCentreOffset);

        // play animation
        //PlayAnimation("BallisticShot");
        PlayAnimationWithDuration("BallisticShot", ExecutionDuration);



        // activate projectile
        ActivateProjectile(ActivationWearableNameEnum, ActivationInput.actionDirection, Distance, Duration);
    }

    ref GameObject GetProjectileInstance(Wearable.NameEnum activationWearable)
    {
        switch (activationWearable)
        {
            case Wearable.NameEnum.LinkBubbly: return ref m_corkProjectile;
            case Wearable.NameEnum.AagentPistol: return ref m_bulletProjectile;
            case Wearable.NameEnum.BabyBottle: return ref m_milkProjectile;
            case Wearable.NameEnum.AppleJuice: return ref m_appleProjectile;
            case Wearable.NameEnum.LilPumpDrank: return ref m_drankProjectile;
            case Wearable.NameEnum.Basketball: return ref m_basketballProjectile;
            case Wearable.NameEnum.BowandArrow: return ref m_arrowProjectile;
            case Wearable.NameEnum.Longbow: return ref m_arrowProjectile;
            case Wearable.NameEnum.NailGun: return ref m_nailTrioProjectile;
            case Wearable.NameEnum.GMSeeds: return ref m_seedProjectile;
            default: return ref m_bulletProjectile;
        }
    }

    void ActivateProjectile(Wearable.NameEnum activationWearable, Vector3 direction, float distance, float duration)
    {
        GameObject projectile = GetProjectileInstance(activationWearable);
        var no_projectile = projectile.GetComponent<BallisticExplosionProjectile>();
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
            no_projectile.Init(startPosition, direction, distance, duration, ExplosionRadius,
                IsServer ? PlayerAbility.NetworkRole.Server : PlayerAbility.NetworkRole.LocalClient,
                Wearable.WeaponTypeEnum.Ballistic, Player,
                playerCharacter.AttackPower.Value * ActivationWearable.RarityMultiplier * DamageMultiplier,
                playerCharacter.CriticalChance.Value,
                playerCharacter.CriticalDamage.Value,
                DamageMultiplier,
                ExplosionDamageMultiplier,

                ActivationInput.actionDirection,
                KnockbackDistance,
                KnockbackStunDuration,
                ExplosionKnockbackDistance,
                ExplosionKnockbackStunDuration);

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
                GetComponent<BallisticExplosionProjectile>();

            // init
            no_projectile.Init(startPosition, direction, distance, duration, 1,
                PlayerAbility.NetworkRole.RemoteClient,
                Wearable.WeaponTypeEnum.Ballistic, Player,
                0, 0, 0, 0, 0,
                Vector3.right, 0, 0, 0, 0);

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
