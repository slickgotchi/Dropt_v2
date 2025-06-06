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
    public List<GameObject> Projectiles = new List<GameObject>();

    private float m_distance = 8f;

    private List<ScheduledProjectile> m_scheduledProjectiles =
        new List<ScheduledProjectile>();

    private AttackPathVisualizer m_attackPathVisualizer;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        foreach (var projectile in Projectiles)
        {
            projectile.SetActive(false);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }

    protected override void Update()
    {
        base.Update();
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
        //SetLocalPosition(PlayerAbilityCentreOffset + ActivationInput.actionDirection * Projection);
        SetLocalPosition(PlayerAbilityCentreOffset);


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
        //GameObject projectile = GetProjectileInstance(index);
        var no_projectile = Projectiles[index].GetComponent<SplashProjectile>();
        var playerCharacter = Player.GetComponent<NetworkCharacter>();
        var startPosition =
                Player.GetComponent<PlayerPrediction>().GetInterpPositionAtTick(ActivationInput.tick)
                + new Vector3(0, 0.5f, 0);
        //+ ActivationInput.actionDirection * Projection;

        // Local Client & Server
        if (Player.GetComponent<NetworkObject>().IsLocalPlayer || IsServer)
        {
            Projectiles[index].SetActive(true);
            //Projectiles[index].transform.position = startPosition;

            no_projectile.Init(
                startPosition, direction, distance, duration, scale,
                explosionRadius,
                IsServer ? PlayerAbility.NetworkRole.Server : PlayerAbility.NetworkRole.LocalClient,
                Wearable.WeaponTypeEnum.Splash, activationWearable,
                Player,
                playerCharacter.currentStaticStats.AttackPower * ActivationWearable.RarityMultiplier * DamageMultiplier,
                playerCharacter.currentStaticStats.CriticalChance,
                playerCharacter.currentStaticStats.CriticalDamage,
                KnockbackDistance,
                KnockbackStunDuration);


            no_projectile.Fire();
        }

        // Server Only
        if (IsServer)
        {
            var playerNetworkObjectId = Player.GetComponent<NetworkObject>().NetworkObjectId;
            //var projectileNetworkObjectId = projectile.GetComponent<NetworkObject>().NetworkObjectId;
            ActivateProjectileClientRpc(index,
                activationWearable, startPosition, 
                direction, distance, duration, scale, explosionRadius,
                playerNetworkObjectId);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ActivateProjectileClientRpc(int index,Wearable.NameEnum activationWearable, Vector3 startPosition, 
        Vector3 direction, float distance, float duration, float scale, float explosionRadius,
        ulong playerNetworkObjectId)
    {
        // Remote Client
        Player = NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId].gameObject;
        if (!Player) return;

        //GameObject projectile = GetProjectileInstance(index);
        //GameObject projectile = NetworkManager.SpawnManager.SpawnedObjects[projectileNetworkObjectId].gameObject;

        

        // Remote Client
        if (!Player.GetComponent<NetworkObject>().IsLocalPlayer)
        {
            var no_projectile = Projectiles[index].GetComponent<SplashProjectile>();

            //Projectiles[index].SetActive(true);
            //Projectiles[index].transform.position = startPosition;

            // iniy
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
        if (m_attackPathVisualizer == null) return;
        if (!IsHolding()) m_attackPathVisualizer.SetMeshVisible(false);
    }

    public override void OnHoldStart()
    {
        base.OnHoldStart();

        if (Player == null) return;

        m_attackPathVisualizer = Player.GetComponentInChildren<AttackPathVisualizer>();
        if (m_attackPathVisualizer == null) return;

        m_attackPathVisualizer.SetMeshVisible(true);

        m_attackPathVisualizer.useCircle = true;
        m_attackPathVisualizer.angle = 90;

        int numTargetsAllowed = 1;

        var playerPrediction = Player.GetComponent<PlayerPrediction>();
        if (playerPrediction == null) return;

        var holdDistance = playerPrediction.GetHoldDistanceFromPlayerAttackCentre();
        holdDistance = holdDistance > MaxDistance ? MaxDistance : holdDistance;
        var outerRadius = holdDistance + ExplosionRadius;
        m_attackPathVisualizer.outerRadius = outerRadius;

        var innerRadius = outerRadius - 2 * ExplosionRadius;
        m_attackPathVisualizer.innerRadius = innerRadius < 0 ? 0 : innerRadius;

        var angleDegrees = math.degrees(math.atan(ExplosionRadius / holdDistance));
        m_attackPathVisualizer.angle = numTargetsAllowed * angleDegrees;

        m_attackPathVisualizer.forwardDirection = playerPrediction.GetHoldActionDirection();
    }

    public override void OnHoldUpdate()
    {
        base.OnHoldUpdate();

        if (Player == null) return;
        if (m_attackPathVisualizer == null) return;

        int numTargetsAllowed = (int)math.ceil(math.lerp(0, 5,
            GetHoldPercentage()));

        var playerPrediction = Player.GetComponent<PlayerPrediction>();
        if (playerPrediction == null) return;

        var holdDistance = playerPrediction.GetHoldDistanceFromPlayerAttackCentre();
        holdDistance = holdDistance > MaxDistance ? MaxDistance : holdDistance;
        var outerRadius = holdDistance + ExplosionRadius;
        m_attackPathVisualizer.outerRadius = outerRadius;

        var innerRadius = outerRadius - 2 * ExplosionRadius;
        m_attackPathVisualizer.innerRadius = innerRadius < 0 ? 0 : innerRadius;

        var angleDegrees = math.degrees(math.atan(ExplosionRadius / holdDistance));
        m_attackPathVisualizer.angle = numTargetsAllowed * angleDegrees;

        m_attackPathVisualizer.forwardDirection = playerPrediction.GetHoldActionDirection();
    }

    public override void OnHoldCancel()
    {
        base.OnHoldCancel();

        if (m_attackPathVisualizer == null) return;
        m_attackPathVisualizer.SetMeshVisible(false);
    }

    public override void OnHoldFinish()
    {
        base.OnHoldFinish();

        if (m_attackPathVisualizer == null) return;
        m_attackPathVisualizer.SetMeshVisible(false);
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
