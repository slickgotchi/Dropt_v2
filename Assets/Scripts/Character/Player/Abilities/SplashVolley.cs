using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class SplashVolley : PlayerAbility
{
    [Header("SplashVolley Parameters")]
    public float Projection = 1.5f;
    public float MaxDistance = 8f;
    public float Duration = 1f;
    public float ExplosionRadius = 1f;
    public float LobHeight = 2f;
    public float DistanceDelta = 0.5f; // New: Distance variation
    public float ReleaseTimeDelta = 0.1f; // New: Release time variation
    public float Scale = 1f;

    [Header("Projectile Prefab")]
    public GameObject SplashProjectilePrefab;

    private float m_distance = 8f;

    // variables for keeping track of the spawned projectiles
    private List<GameObject> m_splashProjectiles = new List<GameObject>(new GameObject[5]);

    private NetworkVariable<ulong> m_splashProjectileId_0 = new NetworkVariable<ulong>();
    private NetworkVariable<ulong> m_splashProjectileId_1 = new NetworkVariable<ulong>();
    private NetworkVariable<ulong> m_splashProjectileId_2 = new NetworkVariable<ulong>();
    private NetworkVariable<ulong> m_splashProjectileId_3 = new NetworkVariable<ulong>();
    private NetworkVariable<ulong> m_splashProjectileId_4 = new NetworkVariable<ulong>();

    //private List<NetworkVariable<ulong>> m_splashProjectileIds = new List<NetworkVariable<ulong>>();
    //private NetworkList<ulong> m_splashProjectileIds = new NetworkList<ulong>();
    private List<ScheduledProjectile> m_scheduledProjectiles = new List<ScheduledProjectile>();

    ref NetworkVariable<ulong> GetSplashProjectileId(int index)
    {
        if (index == 0) return ref m_splashProjectileId_0;
        else if (index == 1) return ref m_splashProjectileId_1;
        else if (index == 2) return ref m_splashProjectileId_2;
        else if (index == 3) return ref m_splashProjectileId_3;
        else return ref m_splashProjectileId_4;
    }

    void InitProjectile(int index, GameObject prefab)
    {
        var projectile = Instantiate(prefab);
        projectile.GetComponent<NetworkObject>().Spawn();
        projectile.SetActive(false);
        m_splashProjectiles[index] = projectile;

        GetSplashProjectileId(index).Value = projectile.GetComponent<NetworkObject>().NetworkObjectId;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            for (int i = 0; i < 5; i++)
            {
                InitProjectile(i, SplashProjectilePrefab);
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        foreach (var projectile in m_splashProjectiles)
        {
            if (projectile != null)
            {
                if (IsServer) projectile.GetComponent<NetworkObject>().Despawn();
            }
        }

        base.OnNetworkDespawn();
    }

    void TryAddProjectileOnClient(int index)
    {
        var projectileId = GetSplashProjectileId(index).Value;
        if (m_splashProjectiles[index] == null && projectileId > 0)
        {
            var projectile = NetworkManager.SpawnManager.SpawnedObjects[projectileId].gameObject;
            projectile.SetActive(false);
            m_splashProjectiles[index] = projectile;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (IsClient)
        {
            // Ensure remote clients associate projectiles with local projectile variables
            for (int i = 0; i < m_splashProjectiles.Count; i++)
            {
                TryAddProjectileOnClient(i);
            }
        }
    }

    public override void OnUpdate()
    {
        // Check scheduled projectiles for activation
        float currentTime = Time.time;
        for (int i = m_scheduledProjectiles.Count - 1; i >= 0; i--)
        {
            if (currentTime >= m_scheduledProjectiles[i].ActivationTime)
            {
                var scheduledProjectile = m_scheduledProjectiles[i];
                ActivateProjectile(scheduledProjectile.Index, scheduledProjectile.Wearable, 
                    scheduledProjectile.Direction, scheduledProjectile.Distance, scheduledProjectile.Duration,
                    scheduledProjectile.Scale, scheduledProjectile.ExplosionRadius);
                m_scheduledProjectiles.RemoveAt(i);
            }
        }
    }

    public override void OnStart()
    {
        // Set rotation/local position
        SetRotationToActionDirection();
        SetLocalPosition(PlayerAbilityCentreOffset + ActivationInput.actionDirection * Projection);

        // Play animation
        PlayAnimation("SplashLob");

        // adjust distance
        m_distance = math.min(ActivationInput.actionDistance, MaxDistance);

        // Activate projectiles
        var holdChargePercentage = math.min(m_holdTimer / HoldChargeTime, 1);
        ActivateMultipleProjectiles(ActivationWearableNameEnum, 
            ActivationInput.actionDirection, m_distance, Duration, Scale, ExplosionRadius,
            holdChargePercentage);
    }

    GameObject GetProjectileInstance(int index)
    {
        return m_splashProjectiles[index];
    }

    void ActivateMultipleProjectiles(Wearable.NameEnum activationWearable, 
        Vector3 direction, float distance, float duration, float scale, float explosionRadius,
        float holdChargePercentage)
    {
        int numProjectiles = Mathf.Clamp(Mathf.CeilToInt(holdChargePercentage * 5), 1, 5);

        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float startAngle = baseAngle - (numProjectiles - 1) * 10f; // Centering projectiles around the initial direction

        for (int i = 0; i < numProjectiles; i++)
        {
            float angle = startAngle + i * 15f;
            Vector3 newDirection = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0).normalized;

            float randomDistance = distance + UnityEngine.Random.Range(-DistanceDelta, DistanceDelta);
            float releaseDelay = UnityEngine.Random.Range(0, ReleaseTimeDelta);

            ScheduleProjectile(i, activationWearable, 
                newDirection, randomDistance, duration, scale, explosionRadius,
                Time.time + releaseDelay);
        }
    }

    void ScheduleProjectile(int index, Wearable.NameEnum activationWearable, 
        Vector3 direction, float distance, float duration, float scale, float explosionRadius, float activationTime)
    {
        m_scheduledProjectiles.Add(new ScheduledProjectile
        {
            Index = index,
            Wearable = activationWearable,
            Direction = direction,
            Distance = distance,
            Duration = duration,
            Scale = scale,
            ExplosionRadius = explosionRadius,
            ActivationTime = activationTime
        });
    }

    void ActivateProjectile(int index, Wearable.NameEnum activationWearable, 
        Vector3 direction, float distance, float duration, float scale, float explosionRadius)
    {
        GameObject projectile = GetProjectileInstance(index);
        var no_projectile = projectile.GetComponent<SplashProjectile>();
        var playerCharacter = Player.GetComponent<NetworkCharacter>();

        // Local Client & Server
        if (Player.GetComponent<NetworkObject>().IsLocalPlayer || IsServer)
        {
            var position =
                Player.GetComponent<PlayerPrediction>().GetInterpPositionAtTick(ActivationInput.tick)
                + new Vector3(0, 0.5f, 0)
                + ActivationInput.actionDirection * Projection;

            no_projectile.Init(
                position, direction, distance, duration, scale,
                explosionRadius, IsServer ? PlayerAbility.NetworkRole.Server : PlayerAbility.NetworkRole.LocalClient,
                Wearable.WeaponTypeEnum.Splash, activationWearable,
                Player,
                playerCharacter.AttackPower.Value * ActivationWearable.RarityMultiplier * DamageMultiplier,
                playerCharacter.CriticalChance.Value,
                playerCharacter.CriticalDamage.Value,
                KnockbackDistance,
                KnockbackStunDuration);


            no_projectile.Fire();
        }

        // Server Only
        if (IsServer)
        {
            var playerNetworkObjectId = Player.GetComponent<NetworkObject>().NetworkObjectId;
            var projectileNetworkObjectId = projectile.GetComponent<NetworkObject>().NetworkObjectId;
            ActivateProjectileClientRpc(index, activationWearable, projectile.transform.position, 
                direction, distance, duration, scale, explosionRadius,
                playerNetworkObjectId, projectileNetworkObjectId);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ActivateProjectileClientRpc(int index,Wearable.NameEnum activationWearable, Vector3 startPosition, 
        Vector3 direction, float distance, float duration, float scale, float explosionRadius,
        ulong playerNetworkObjectId, ulong projectileNetworkObjectId)
    {
        // Remote Client
        Player = NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId].gameObject;
        if (!Player) return;

        //GameObject projectile = GetProjectileInstance(index);
        GameObject projectile = NetworkManager.SpawnManager.SpawnedObjects[projectileNetworkObjectId].gameObject;

        // Remote Client
        if (!Player.GetComponent<NetworkObject>().IsLocalPlayer)
        {
            projectile.SetActive(true);
            projectile.transform.position = startPosition;
            var no_projectile = projectile.GetComponent<SplashProjectile>();
            no_projectile.Init(
                startPosition, direction, distance, duration, scale, explosionRadius,
                NetworkRole.RemoteClient, Wearable.WeaponTypeEnum.Splash, activationWearable,
                Player, 0, 0, 0,
                0, 0);


            no_projectile.Fire();
        }
    }

    public override void OnFinish()
    {
        // Custom finish logic if needed
    }

    private class ScheduledProjectile
    {
        public int Index;
        public Wearable.NameEnum Wearable;
        public Vector3 Direction;
        public float Distance;
        public float Duration;
        public float Scale;
        public float ExplosionRadius;
        public float ActivationTime;
    }
}
