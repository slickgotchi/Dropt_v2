using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Hierarchy;

public class BallisticShot : PlayerAbility
{
    [Header("BallisticShot Parameters")]
    public float Projection = 1.5f;
    public float Distance = 8f;
    public float Duration = 1f;

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
            InitProjectile(ref m_bulletProjectile, ref m_bulletProjectileId, BulletPrefab);
            InitProjectile(ref m_arrowProjectile, ref m_arrowProjectileId, ArrowPrefab);
            InitProjectile(ref m_basketballProjectile, ref m_basketballProjectileId, BasketballPrefab);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (m_bulletProjectile != null) m_bulletProjectile.GetComponent<NetworkObject>().Despawn();
        if (m_arrowProjectile != null) m_arrowProjectile.GetComponent<NetworkObject>().Despawn();
        if (m_basketballProjectile != null) m_basketballProjectile.GetComponent<NetworkObject>().Despawn();
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
        TryAddProjectile(ref m_bulletProjectile, ref m_bulletProjectileId);
        TryAddProjectile(ref m_arrowProjectile, ref m_arrowProjectileId);
        TryAddProjectile(ref m_basketballProjectile, ref m_basketballProjectileId);
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
        if (activationWearable == Wearable.NameEnum.AagentPistol || activationWearable == Wearable.NameEnum.NailGun)
        {
            return ref m_bulletProjectile;
        }
        else if (activationWearable == Wearable.NameEnum.Basketball)
        {
            return ref m_basketballProjectile;
        }
        else
        {
            return ref m_arrowProjectile;
        }
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
                + new Vector3(0,0.5f,0)
                + ActivationInput.actionDirection * Projection;
            var no_projectile = projectile.GetComponent<GenericProjectile>();
            no_projectile.Direction = direction;
            no_projectile.Distance = distance;
            no_projectile.Duration = duration;
            no_projectile.Scale = 1;
            no_projectile.LocalPlayer = Player;
            no_projectile.WeaponType = Wearable.WeaponTypeEnum.Ballistic;

            var playerCharacter = Player.GetComponent<NetworkCharacter>();
            no_projectile.DamagePerHit = playerCharacter.AttackPower.Value * ActivationWearable.RarityMultiplier;
            no_projectile.CriticalChance = playerCharacter.CriticalChance.Value;
            no_projectile.CriticalDamage = playerCharacter.CriticalDamage.Value;
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
            var no_projectile = projectile.GetComponent<GenericProjectile>();
            no_projectile.Direction = direction;
            no_projectile.Distance = distance;
            no_projectile.Duration = duration;
            no_projectile.Scale = 1;
            no_projectile.WeaponType = Wearable.WeaponTypeEnum.Ballistic;

            var playerCharacter = Player.GetComponent<NetworkCharacter>();
            no_projectile.DamagePerHit = playerCharacter.AttackPower.Value * ActivationWearable.RarityMultiplier;
            no_projectile.CriticalChance = playerCharacter.CriticalChance.Value;
            no_projectile.CriticalDamage = playerCharacter.CriticalDamage.Value;
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
