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
    public GameObject ProjectilePrefab;

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

    private AttackPathVisualizer m_attackPathVisualizer;

    // all the ballistic projectiles
    private List<GameObject> m_networkProjectiles
        = new List<GameObject>(new GameObject[5]);

    private NetworkVariable<ulong> m_networkProjectileId_0 = new NetworkVariable<ulong>();
    private NetworkVariable<ulong> m_networkProjectileId_1 = new NetworkVariable<ulong>();
    private NetworkVariable<ulong> m_networkProjectileId_2 = new NetworkVariable<ulong>();
    private NetworkVariable<ulong> m_networkProjectileId_3 = new NetworkVariable<ulong>();
    private NetworkVariable<ulong> m_networkProjectileId_4 = new NetworkVariable<ulong>();

    private List<GameObject> m_visualProjectiles
        = new List<GameObject>(new GameObject[5]);

    ref NetworkVariable<ulong> GetNetworkProjectileId(int index)
    {
        if (index == 0) return ref m_networkProjectileId_0;
        else if (index == 1) return ref m_networkProjectileId_1;
        else if (index == 2) return ref m_networkProjectileId_2;
        else if (index == 3) return ref m_networkProjectileId_3;
        else return ref m_networkProjectileId_4;
    }

    void InitNetworkProjectile(int index, GameObject prefab)
    {
        var projectile = Instantiate(prefab);
        projectile.GetComponent<NetworkObject>().Spawn();
        projectile.SetActive(false);
        m_networkProjectiles[index] = projectile;

        GetNetworkProjectileId(index).Value = projectile.GetComponent<NetworkObject>().NetworkObjectId;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            for (int i = 0; i < 5; i++)
            {
                InitNetworkProjectile(i, ProjectilePrefab);
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            for (int i = 0; i < 5; i++)
            {
                m_networkProjectiles[i].GetComponent<NetworkObject>().Despawn();
            }
        }

        base.OnNetworkDespawn();
    }

    void TryAddProjectileOnClient(int index)
    {
        var projectileId = GetNetworkProjectileId(index).Value;
        if (m_networkProjectiles[index] == null && projectileId > 0)
        {
            var projectile = NetworkManager.SpawnManager.SpawnedObjects[projectileId].gameObject;
            projectile.SetActive(false);
            m_networkProjectiles[index] = projectile;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (IsClient)
        {
            for (int i = 0; i < 5; i++)
            {
                TryAddProjectileOnClient(i);
            }
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
            ActivateProjectile(i, ActivationWearableNameEnum, m_directions[i],
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
            sapv.width = 0.3f;
            sapv.SetMeshVisible(false);
        }
    }

    private List<Vector3> m_directions = new List<Vector3>(new Vector3[5]);
    private List<GameObject> m_targets = new List<GameObject>();

    public override void OnHoldUpdate()
    {
        base.OnHoldUpdate();

        if (Player == null) return;
        
        // 1. calc number of targets we can lock on to based on hold percentage
        int numTargetsAllowed = (int)math.ceil(math.lerp(
            (float)m_holdStartTargets-1,
            (float)m_holdFinishTargets,
            GetHoldPercentage()));

        // 2. find number of targets in range
        var attackCentre = Player.GetComponentInChildren<AttackCentre>();
        var attackCentrePos = attackCentre.transform.position;
        //Debug.Log("attackCentre: " + attackCentrePos);
        var targets = FindClosestTargets(attackCentre.transform.position,
            numTargetsAllowed, Projection + Distance);
        m_targets = targets;

        // 3. set all attack paths invisible
        foreach (var sapv in m_snipeAttackPathVisualizers) sapv.SetMeshVisible(false);

        // 4. draw a attack path to each target
        for (int i = 0; i < targets.Count; i++)
        {
            var sapv = m_snipeAttackPathVisualizers[i];
            sapv.SetMeshVisible(true);
            var targetPos = targets[i].transform.position + new Vector3(0, 0.5f, 0);
            //Debug.Log("targetPos: " + targetPos);

            var dir = (targetPos - attackCentrePos).normalized;
            m_directions[i] = dir;
            //Debug.Log("dir: " + dir);
            sapv.forwardDirection = new Vector2(dir.x, dir.y);
            sapv.innerStartPoint = 1f;
            sapv.outerFinishPoint = math.distance(targetPos, attackCentrePos);
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
        var no_projectile = m_networkProjectiles[i].GetComponent<GenericProjectile>();
        var no_projectileId = no_projectile.GetComponent<NetworkObject>().NetworkObjectId;
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
                playerCharacter.AttackPower.Value * ActivationWearable.RarityMultiplier * DamageMultiplier,
                playerCharacter.CriticalChance.Value,
                playerCharacter.CriticalDamage.Value,
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
                playerId, no_projectileId);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ActivateProjectileClientRpc(int i, Wearable.NameEnum activationWearable, Vector3 startPosition, Vector3 direction, float distance, float duration, 
        float scale, ulong playerNetworkObjectId, ulong projectileNetworkObjectId)
    {
        // Remote Client
        Player = NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId].gameObject;
        if (!Player) return;

        // Remote Client
        if (!Player.GetComponent<NetworkObject>().IsLocalPlayer)
        {
            var no_projectile = NetworkManager.SpawnManager.SpawnedObjects[projectileNetworkObjectId].
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
    }
}
