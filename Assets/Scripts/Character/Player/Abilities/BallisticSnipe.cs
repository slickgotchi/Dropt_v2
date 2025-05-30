using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class BallisticSnipe : PlayerAbility
{
    [Header("BallisticSnipe Parameters")]
    public float Projection = 1.5f;
    public float Distance = 8f;
    public float Duration = 1f;
    private int m_holdStartTargets = 1;
    private int m_holdFinishTargets = 5;
    [SerializeField]
    private List<AttackPathVisualizer> m_snipeAttackPathVisualizers =
        new List<AttackPathVisualizer>();

    [Header("Projectile Prefab")]
    public List<GameObject> Projectiles = new List<GameObject>();
    

    [Header("Visual Projectile Prefab")]
    public GameObject ApplePrefab;
    public GameObject ArrowPrefab;
    public GameObject BasketballPrefab;
    public GameObject BulletPrefab;
    public GameObject CorkPrefab;
    public GameObject DrankPrefab;
    public GameObject MilkPrefab;
    public GameObject NailTrioPrefab;
    public GameObject SeedPrefab;


    // we want a ref to the player og visualizer for the colors
    private AttackPathVisualizer m_attackPathVisualizer;

    private AttackCentre m_attackCentre;
    private List<Vector3> m_attackDirections = new List<Vector3>(new Vector3[5]);
    private List<GameObject> m_targets = new List<GameObject>();
    private int m_numTargetsAllowed = 0;

    private List<GameObject> m_visualProjectiles =
        new List<GameObject>(new GameObject[5]);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        foreach (var projectile in Projectiles)
        {
            projectile.GetComponent<GenericProjectile>().VisualGameObject =
                InstantiateVisualProjectile(Wearable.NameEnum.AagentPistol);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }

    protected override void Update()
    {
        base.Update();

        if (m_attackCentre == null && Player != null)
        {
            m_attackCentre = Player.GetComponentInChildren<AttackCentre>();
        }
    }

    public override void OnStart()
    {
        // set rotatin/local position
        SetRotationToActionDirection();
        SetLocalPosition(PlayerAbilityCentreOffset);
        var holdScale = math.min(1 + (m_holdTimer / HoldChargeTime), 2f);

        // play animation
        //PlayAnimation("BallisticShot");
        PlayAnimationWithDuration("BallisticShot", ExecutionDuration);

        // activate projectiles
        for (int i = 0; i < m_targets.Count; i++)
        {
            ActivateProjectile(i, ActivationWearableNameEnum, m_attackDirections[i],
                Distance, Duration, holdScale);
        }

    }



    public override void OnHoldStart()
    {
        base.OnHoldStart();

        if (Player == null) return;

        // we use the players attack visualizer to set our snipers visualisers color etc.
        m_attackPathVisualizer = Player.GetComponentInChildren<AttackPathVisualizer>();
        if (m_attackPathVisualizer == null) return;

        foreach (var sapv in m_snipeAttackPathVisualizers)
        {
            sapv.borderThickness = m_attackPathVisualizer.borderThickness;
            sapv.fillMeshRenderer.material = m_attackPathVisualizer.fillMeshRenderer.material;
            sapv.borderMeshRenderer.material = m_attackPathVisualizer.borderMeshRenderer.material;
            sapv.useCircle = false;
            sapv.width = 0.1f;
            sapv.SetMeshVisible(false);
        }
    }



    public override void OnHoldUpdate()
    {
        base.OnHoldUpdate();

        if (Player == null || m_attackCentre == null) return;
        
        // 1. calc number of targets we can lock on to based on hold percentage
        int numTargetsAllowed = (int)math.ceil(math.lerp(
            (float)m_holdStartTargets-1,
            (float)m_holdFinishTargets,
            GetHoldPercentage()));

        // 2. find number of targets in range
        var attackCentrePos = m_attackCentre.transform.position;
        m_targets = FindClosestTargets(attackCentrePos,
            numTargetsAllowed, Projection + Distance);

        // 3. get attack directions
        for (int i = 0; i < m_targets.Count; i++)
        {
            var targetPos = m_targets[i].transform.position + new Vector3(0, 0.5f, 0);
            m_attackDirections[i] = (targetPos - attackCentrePos).normalized;
        }

        if (IsClient)
        {
            // 3. set all attack paths invisible
            foreach (var sapv in m_snipeAttackPathVisualizers) sapv.SetMeshVisible(false);

            // 4. draw a attack path to each target
            for (int i = 0; i < m_targets.Count; i++)
            {
                var sapv = m_snipeAttackPathVisualizers[i];
                sapv.SetMeshVisible(true);

                var dir = m_attackDirections[i];
                sapv.forwardDirection = new Vector2(dir.x, dir.y);

                var targetPos = m_targets[i].transform.position + new Vector3(0, 0.5f, 0);
                sapv.innerStartPoint = 1f;
                sapv.outerFinishPoint = math.distance(targetPos, attackCentrePos);
            }
        }
        
    }

    public override void OnHoldCancel()
    {
        base.OnHoldCancel();

        foreach (var sapv in m_snipeAttackPathVisualizers) sapv.SetMeshVisible(false);
    }

    public override void OnHoldFinish()
    {
        base.OnHoldFinish();

        foreach (var sapv in m_snipeAttackPathVisualizers) sapv.SetMeshVisible(false);

        if (Player == null) return;
        var playerPrediction = Player.GetComponent<PlayerPrediction>();
        if (playerPrediction == null) return;

        var dir = playerPrediction.GetHoldActionDirection();
        var startAngle = GetAngleFromDirection(dir);

        // do a gotchi spin
        Player.GetComponent<PlayerGotchi>().PlayFacingSpin(1, 0.4f,
            PlayerGotchi.SpinDirection.AntiClockwise, startAngle);
    }

    public List<GameObject> FindClosestTargets(Vector3 position, int numberTargets, float maxRange)
    {
        // Find all GameObjects with EnemyCharacter or Destructible
        EnemyCharacter[] enemies = FindObjectsOfType<EnemyCharacter>();
        Destructible[] destructibles = FindObjectsOfType<Destructible>();

        // Create a list to store all potential targets within range
        List<GameObject> potentialTargets = new List<GameObject>();

        // Add GameObjects to the potential targets list if within maxRange
        foreach (var enemy in enemies)
        {
            if (Vector3.Distance(position, enemy.transform.position) <= maxRange)
            {
                potentialTargets.Add(enemy.gameObject);
            }
        }

        foreach (var destructible in destructibles)
        {
            if (Vector3.Distance(position, destructible.transform.position) <= maxRange)
            {
                potentialTargets.Add(destructible.gameObject);
            }
        }

        // Sort the potential targets by distance to the specified position
        potentialTargets.Sort((a, b) =>
        {
            float distA = Vector3.Distance(position, a.transform.position);
            float distB = Vector3.Distance(position, b.transform.position);
            return distA.CompareTo(distB);
        });

        // Ensure the final list only contains as many targets as are available
        int countToReturn = Mathf.Min(numberTargets, potentialTargets.Count);

        // If fewer targets exist than requested, only the available ones are returned
        return potentialTargets.GetRange(0, countToReturn);
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

    void ActivateProjectile(int i, Wearable.NameEnum activationWearable, Vector3 direction, float distance, float duration, 
        float scale)
    {
        var no_projectile = Projectiles[i].GetComponent<GenericProjectile>();
        //var no_projectileId = no_projectile.GetComponent<NetworkObject>().NetworkObjectId;
        var playerCharacter = Player.GetComponent<NetworkCharacter>();
        var startPosition =
                Player.GetComponent<PlayerPrediction>().GetInterpPositionAtTick(ActivationInput.tick)
                + new Vector3(0, 0.5f, 0)
                + direction * Projection;

        // Local Client & Server
        if (Player.GetComponent<NetworkObject>().IsLocalPlayer || IsServer)
        {
            if (IsClient)
            {
                m_visualProjectiles[i] = InstantiateVisualProjectile(activationWearable);
                m_visualProjectiles[i].transform.position = startPosition;
                if (no_projectile.VisualGameObject != null) Destroy(no_projectile.VisualGameObject);
                no_projectile.VisualGameObject = m_visualProjectiles[i];
            }

            // init
            no_projectile.Init(startPosition, direction, distance, duration, scale,
                IsServer ? PlayerAbility.NetworkRole.Server : PlayerAbility.NetworkRole.LocalClient,
                Wearable.WeaponTypeEnum.Ballistic, Player,
                playerCharacter.currentStaticStats.AttackPower * ActivationWearable.RarityMultiplier * DamageMultiplier,
                playerCharacter.currentStaticStats.CriticalChance,
                playerCharacter.currentStaticStats.CriticalDamage,
                direction,
                KnockbackDistance,
                KnockbackStunDuration);

            // fire
            no_projectile.Fire();
        }

        // Server Only
        if (IsServer)
        {
            ulong playerId = Player.GetComponent<NetworkObject>().NetworkObjectId;
            ActivateProjectileClientRpc(i, activationWearable, startPosition,
                direction, distance, duration, scale,
                playerId);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ActivateProjectileClientRpc(int i, Wearable.NameEnum activationWearable, Vector3 startPosition, Vector3 direction, float distance, float duration, 
        float scale, ulong playerNetworkObjectId)
    {
        // Remote Client
        Player = NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId].gameObject;
        if (!Player) return;

        // Remote Client
        if (!Player.GetComponent<NetworkObject>().IsLocalPlayer)
        {
            var no_projectile = Projectiles[i].
                GetComponent<GenericProjectile>();

            m_visualProjectiles[i] = InstantiateVisualProjectile(activationWearable);
            m_visualProjectiles[i].transform.position = startPosition;
            if (no_projectile.VisualGameObject != null) Destroy(no_projectile.VisualGameObject);
            no_projectile.VisualGameObject = m_visualProjectiles[i];

            // init
            no_projectile.Init(startPosition, direction, distance, duration, scale,
                PlayerAbility.NetworkRole.RemoteClient,
                Wearable.WeaponTypeEnum.Ballistic, Player,
                0, 0, 0,
                Vector3.right, 0, 0);

            // init
            no_projectile.Fire();
        }
    }

    public override void OnFinish()
    {
        if (IsHolding()) return;
        foreach (var sapv in m_snipeAttackPathVisualizers) sapv.SetMeshVisible(false);
    }
}
