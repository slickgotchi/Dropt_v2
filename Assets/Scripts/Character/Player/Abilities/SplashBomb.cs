using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.UIElements;

public class SplashBomb : PlayerAbility
{
    [Header("SplashBomb Parameters")]
    public float Projection = 1.5f;
    public float MaxDistance = 8f;
    public float Duration = 1f;
    public float ExplosionRadius = 1f;
    public float LobHeight = 2f;

    [Header("Projectile Prefab")]
    public GameObject Projectile;

    private float m_distance = 8f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
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
        SetLocalPosition(PlayerAbilityCentreOffset + ActivationInput.actionDirection * Projection);

        // play animation
        PlayAnimation("SplashLob");

        // adjust distance
        m_distance = math.min(ActivationInput.actionDistance, MaxDistance);

        // activate projectile
        ActivateProjectile(ActivationWearableNameEnum, ActivationInput.actionDirection, m_distance, Duration, 1f, ExplosionRadius);
    }

    void ActivateProjectile(Wearable.NameEnum wearableNameEnum, Vector3 direction, float distance, float duration,
        float scale, float explosionRadius)
    {
        var no_projectile = Projectile.GetComponent<SplashProjectile>();
        var playerCharacter = Player.GetComponent<NetworkCharacter>();
        var startPosition =
                Player.GetComponent<PlayerPrediction>().GetInterpPositionAtTick(ActivationInput.tick)
                + new Vector3(0, 0.5f, 0);
                //+ ActivationInput.actionDirection * Projection;

        // Local Client & Server
        if (Player.GetComponent<NetworkObject>().IsLocalPlayer || IsServer)
        {
            // init
            no_projectile.Init(
                startPosition, direction, distance, duration, scale, explosionRadius,

                IsServer ? PlayerAbility.NetworkRole.Server : PlayerAbility.NetworkRole.LocalClient,
                Wearable.WeaponTypeEnum.Splash, wearableNameEnum,

                Player,
                playerCharacter.currentStaticStats.AttackPower * ActivationWearable.RarityMultiplier * DamageMultiplier,
                playerCharacter.currentStaticStats.CriticalChance,
                playerCharacter.currentStaticStats.CriticalDamage,
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
                startPosition, direction, distance, duration, scale, explosionRadius, wearableNameEnum,
                playerId);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ActivateProjectileClientRpc(Vector3 startPosition, Vector3 direction, 
        float distance, float duration, float scale, float explosionRadius, Wearable.NameEnum wearableNameEnum,
        ulong playerNetworkObjectId)
    {
        // Remote Client
        Player = NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId].gameObject;
        if (!Player) return;

        // Remote Client
        if (!Player.GetComponent<NetworkObject>().IsLocalPlayer)
        {
            var no_projectile = Projectile.
                GetComponent<SplashProjectile>();

            // init
            no_projectile.Init(startPosition, direction, distance, duration, scale, explosionRadius,
                PlayerAbility.NetworkRole.RemoteClient,
                Wearable.WeaponTypeEnum.Splash, wearableNameEnum,

                Player,
                0, 0, 0,
                0, 0);

            // init
            no_projectile.Fire();
        }
    }

    public override void OnFinish()
    {
    }
}
