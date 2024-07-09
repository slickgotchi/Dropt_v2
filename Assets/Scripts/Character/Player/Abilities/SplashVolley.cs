using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class SplashVolley : PlayerAbility
{
    [Header("SplashVolley Parameters")]
    public float Projection = 1.5f;
    public float Distance = 8f;
    public float Duration = 1f;
    public float ExplosionRadius = 1f;
    public float LobHeight = 2f;
    public float DistanceDelta = 0.5f; // New: Distance variation
    public float ReleaseTimeDelta = 0.1f; // New: Release time variation

    [Header("Projectile Prefab")]
    public GameObject SplashProjectilePrefab;

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
                projectile.GetComponent<NetworkObject>().Despawn();
        }
    }

    void TryAddProjectile(int index)
    {
        var projectileId = GetSplashProjectileId(index).Value;
        if (m_splashProjectiles[index] == null && projectileId > 0)
        {
            var projectile = NetworkManager.SpawnManager.SpawnedObjects[projectileId].gameObject;
            projectile.SetActive(false);
            m_splashProjectiles[index] = projectile;
        }
    }

    private void Update()
    {
        // Ensure remote clients associate projectiles with local projectile variables
        for (int i = 0; i < m_splashProjectiles.Count; i++)
        {
            TryAddProjectile(i);
        }

        // Check scheduled projectiles for activation
        float currentTime = Time.time;
        for (int i = m_scheduledProjectiles.Count - 1; i >= 0; i--)
        {
            if (currentTime >= m_scheduledProjectiles[i].ActivationTime)
            {
                var scheduledProjectile = m_scheduledProjectiles[i];
                ActivateProjectile(scheduledProjectile.Index, scheduledProjectile.Wearable, scheduledProjectile.Direction, scheduledProjectile.Distance, scheduledProjectile.Duration);
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

        // Activate projectiles
        var holdChargePercentage = math.min(HoldDuration / HoldChargeTime, 1);
        ActivateMultipleProjectiles(ActivationWearableNameEnum, ActivationInput.actionDirection, Distance, Duration, holdChargePercentage);
    }

    GameObject GetProjectileInstance(int index)
    {
        return m_splashProjectiles[index];
    }

    void ActivateMultipleProjectiles(Wearable.NameEnum activationWearable, Vector3 direction, float distance, float duration, float holdChargePercentage)
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

            ScheduleProjectile(i, activationWearable, newDirection, randomDistance, duration, Time.time + releaseDelay);
        }
    }

    void ScheduleProjectile(int index, Wearable.NameEnum activationWearable, Vector3 direction, float distance, float duration, float activationTime)
    {
        m_scheduledProjectiles.Add(new ScheduledProjectile
        {
            Index = index,
            Wearable = activationWearable,
            Direction = direction,
            Distance = distance,
            Duration = duration,
            ActivationTime = activationTime
        });
    }

    void ActivateProjectile(int index, Wearable.NameEnum activationWearable, Vector3 direction, float distance, float duration)
    {
        GameObject projectile = GetProjectileInstance(index);

        // Local Client & Server
        if (Player.GetComponent<NetworkObject>().IsLocalPlayer || IsServer)
        {
            projectile.SetActive(true);
            projectile.transform.position =
                Player.GetComponent<PlayerPrediction>().GetInterpPositionAtTick(ActivationInput.tick)
                + new Vector3(0, 0.5f, 0)
                + ActivationInput.actionDirection * Projection;
            var no_projectile = projectile.GetComponent<SplashProjectile>();
            no_projectile.Direction = direction;
            no_projectile.Distance = distance;
            no_projectile.Duration = duration;
            no_projectile.ExplosionRadius = 1;
            no_projectile.LocalPlayer = Player;
            no_projectile.WeaponType = Wearable.WeaponTypeEnum.Magic;
            no_projectile.ExplosionRadius = ExplosionRadius;
            no_projectile.WearableNameEnum = activationWearable;

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
            ActivateProjectileClientRpc(index, activationWearable, projectile.transform.position, direction, distance, duration);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ActivateProjectileClientRpc(int index,Wearable.NameEnum activationWearable, Vector3 startPosition, Vector3 direction, float distance, float duration)
    {
        GameObject projectile = GetProjectileInstance(index);

        // Remote Client
        if (!Player.GetComponent<NetworkObject>().IsLocalPlayer)
        {
            projectile.SetActive(true);
            projectile.transform.position = startPosition;
            var no_projectile = projectile.GetComponent<SplashProjectile>();
            no_projectile.Direction = direction;
            no_projectile.Distance = distance;
            no_projectile.Duration = duration;
            no_projectile.ExplosionRadius = 1;
            no_projectile.WeaponType = Wearable.WeaponTypeEnum.Magic;
            no_projectile.ExplosionRadius = ExplosionRadius;
            no_projectile.WearableNameEnum = activationWearable;

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
        // Custom update logic if needed
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
        public float ActivationTime;
    }
}
