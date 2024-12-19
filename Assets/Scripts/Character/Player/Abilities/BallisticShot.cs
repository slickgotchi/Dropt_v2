using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class BallisticShot : PlayerAbility
{
    [Header("BallisticShot Parameters")]
    public float Projection = 1.5f;
    public float Distance = 8f;
    public float Duration = 1f;

    [Header("Projectile Prefab")]
    public GameObject Projectile;

    [Header("Visual Projectile Prefabs")]
    public GameObject ApplePrefab;
    public GameObject ArrowPrefab;
    public GameObject BasketballPrefab;
    public GameObject BulletPrefab;
    public GameObject CorkPrefab;
    public GameObject DrankPrefab;
    public GameObject MilkPrefab;
    public GameObject NailTrioPrefab;
    public GameObject SeedPrefab;

    private GameObject m_instantiatedVisualProjectile;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // set our projectiles visual game object
        Projectile.GetComponent<GenericProjectile>().VisualGameObject =
            InstantiateVisualProjectile(Wearable.NameEnum.AagentPistol);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void OnStart()
    {
        // set rotatin/local position
        SetRotationToActionDirection();
        SetLocalPosition(PlayerAbilityCentreOffset);

        // play animation
        PlayAnimationWithDuration("BallisticShot", ExecutionDuration);

        // activate projectile
        ActivateProjectile(ActivationWearableNameEnum, ActivationInput.actionDirection, Distance, Duration);
    }

    GameObject InstantiateVisualProjectile(Wearable.NameEnum activationWearable)
    {
        switch (activationWearable)
        {
            case Wearable.NameEnum.LinkBubbly: return GameObject.Instantiate(CorkPrefab);
            case Wearable.NameEnum.AagentPistol: return GameObject.Instantiate(BulletPrefab);
            case Wearable.NameEnum.BabyBottle: return GameObject.Instantiate(MilkPrefab);
            case Wearable.NameEnum.AppleJuice: return GameObject.Instantiate(ApplePrefab);
            case Wearable.NameEnum.LilPumpDrank: return GameObject.Instantiate(DrankPrefab);
            case Wearable.NameEnum.Basketball: return GameObject.Instantiate(BasketballPrefab);
            case Wearable.NameEnum.BowandArrow: return GameObject.Instantiate(ArrowPrefab);
            case Wearable.NameEnum.Longbow: return GameObject.Instantiate(ArrowPrefab);
            case Wearable.NameEnum.NailGun: return GameObject.Instantiate(NailTrioPrefab);
            case Wearable.NameEnum.GMSeeds: return GameObject.Instantiate(SeedPrefab);
            default: return GameObject.Instantiate(BulletPrefab); 
        }
    }

    void ActivateProjectile(Wearable.NameEnum activationWearable, Vector3 direction, float distance, float duration)
    {
        var no_projectile = Projectile.GetComponent<GenericProjectile>();
        //var no_projectileId = no_projectile.GetComponent<NetworkObject>().NetworkObjectId;
        var playerCharacter = Player.GetComponent<NetworkCharacter>();
        var startPosition =
                Player.GetComponent<PlayerPrediction>().GetInterpPositionAtTick(ActivationInput.tick)
                + new Vector3(0, 0.5f, 0)
                + ActivationInput.actionDirection * Projection;

        // Local Client & Server
        if (Player.GetComponent<NetworkObject>().IsLocalPlayer || IsServer)
        {
            if (IsClient)
            {
                m_instantiatedVisualProjectile = InstantiateVisualProjectile(activationWearable);
                m_instantiatedVisualProjectile.transform.position = startPosition;
                if (no_projectile.VisualGameObject != null) Destroy(no_projectile.VisualGameObject);
                no_projectile.VisualGameObject = m_instantiatedVisualProjectile;
            }

            // init
            no_projectile.Init(startPosition, direction, distance, duration, 1,
                IsServer ? PlayerAbility.NetworkRole.Server : PlayerAbility.NetworkRole.LocalClient, 
                Wearable.WeaponTypeEnum.Ballistic, Player,
                playerCharacter.currentStaticStats.AttackPower * ActivationWearable.RarityMultiplier * DamageMultiplier,
                playerCharacter.currentStaticStats.CriticalChance,
                playerCharacter.currentStaticStats.CriticalDamage,
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
            ActivateProjectileClientRpc(
                activationWearable, startPosition, 
                direction, distance, duration,
                playerId);
        }

        // set hand wearable

    }

    [Rpc(SendTo.ClientsAndHost)]
    void ActivateProjectileClientRpc(Wearable.NameEnum activationWearable, Vector3 startPosition, Vector3 direction, 
        float distance, float duration, ulong playerNetworkObjectId)
    {
        // Remote Client
        Player = NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId].gameObject;
        if (!Player) return;

        // Remote Client
        if (!Player.GetComponent<NetworkObject>().IsLocalPlayer)
        {
            var no_projectile = Projectile.GetComponent<GenericProjectile>();

            m_instantiatedVisualProjectile = InstantiateVisualProjectile(activationWearable);
            m_instantiatedVisualProjectile.transform.position = startPosition;
            if (no_projectile.VisualGameObject != null) Destroy(no_projectile.VisualGameObject);
            no_projectile.VisualGameObject = m_instantiatedVisualProjectile;

            // init
            no_projectile.Init(startPosition, direction, distance, duration, 1,
                PlayerAbility.NetworkRole.RemoteClient,
                Wearable.WeaponTypeEnum.Ballistic, Player,
                0, 0, 0,
                Vector3.up, 0, 0);

            // init
            no_projectile.Fire();
        }
    }
}
