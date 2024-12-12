using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

// going to rename this MagicChain
public class MagicBeam : PlayerAbility
{
    [Header("MagicBeam Parameters")]
    [SerializeField] float Projection = 0f;
    [SerializeField] float HoldStartDamageMultiplier = 0.5f;
    [SerializeField] float HoldFinishDamageMultiplier = 2.5f;
    [SerializeField] float m_chainRange = 10f;

    // hold charge variables
    private int m_holdStartTargets = 1;
    private int m_holdFinishTargets = 5;

    public GameObject ElectrictyEffectPrefab;

    // we want a ref to the player og visualizer for the colors
    private AttackPathVisualizer m_attackPathVisualizer;

    private AttackCentre m_attackCentre;

    // these are the individual visualizers of the chain
    [SerializeField]
    private List<AttackPathVisualizer> m_chainAttackPathVisualizers =
        new List<AttackPathVisualizer>();

    private List<GameObject> m_targets = new List<GameObject>();

    private Collider2D m_collider;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        m_collider = GetComponent<Collider2D>();
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
        SetRotationToActionDirection();

        PlayAnimationWithDuration("MagicCast", ExecutionDuration);

        // show visual electricity effect
        if (IsClient)
        {
            for (int i = 0; i < m_targets.Count; i++)
            {
                Vector3 start;
                Vector3 finish;

                // first path is between player and target
                if (i == 0)
                {
                    start = m_attackCentre.transform.position;
                    finish = m_targets[0].transform.position + new Vector3(0, 0.5f, 0);
                }
                else
                {
                    start = m_targets[i - 1].transform.position + new Vector3(0, 0.5f, 0);
                    finish = m_targets[i].transform.position + new Vector3(0, 0.5f, 0);
                }

                // create an electric object
                var electricEffect = Instantiate(ElectrictyEffectPrefab);

                // scale it up
                var length = math.distance(start, finish);
                electricEffect.transform.localScale = new Vector3(length, 2, 1);

                // position it
                electricEffect.transform.position = math.lerp(start, finish, 0.5f);

                // rotate it
                var rotation = PlayerAbility.GetRotationFromDirection(finish - start);
                electricEffect.transform.rotation = rotation;
            }
        }

        // deal damage to all targets if server
        //if (IsServer)
        {
            var playerCharacter = Player.GetComponent<NetworkCharacter>();

            for (int i = 0; i < m_targets.Count; i++)
            {
                var damage = playerCharacter.GetAttackPower() * DamageMultiplier * ActivationWearable.RarityMultiplier;
                bool isCritical = playerCharacter.IsCriticalAttack();
                damage = (int)(isCritical ? damage * playerCharacter.CriticalDamage.Value : damage);

                var networkCharacter = m_targets[i].GetComponent<NetworkCharacter>();
                var destructible = m_targets[i].GetComponent<Destructible>();

                if (networkCharacter != null)
                {
                    networkCharacter.TakeDamage(damage, isCritical, Player);

                    var enemyAI = networkCharacter.GetComponent<Dropt.EnemyAI>();
                    if (enemyAI != null)
                    {
                        enemyAI.Knockback(Vector3.zero, KnockbackDistance, KnockbackStunDuration);
                    }
                }

                if (destructible != null)
                {
                    destructible.TakeDamage(Wearable.WeaponTypeEnum.Magic, Player.GetComponent<NetworkObject>().NetworkObjectId);
                }
            }
        }
    }

    public override void OnUpdate()
    {

    }

    public override void OnFinish()
    {
        foreach (var sapv in m_chainAttackPathVisualizers) sapv.SetMeshVisible(false);
    }

    public override void OnHoldStart()
    {
        base.OnHoldStart();

        if (Player == null) return;

        // we use the players attack visualizer to set our snipers visualisers color etc.
        m_attackPathVisualizer = Player.GetComponentInChildren<AttackPathVisualizer>();
        if (m_attackPathVisualizer == null) return;
        foreach (var capv in m_chainAttackPathVisualizers)
        {
            capv.borderThickness = m_attackPathVisualizer.borderThickness;
            capv.fillMeshRenderer.material = m_attackPathVisualizer.fillMeshRenderer.material;
            capv.borderMeshRenderer.material = m_attackPathVisualizer.borderMeshRenderer.material;
            capv.useCircle = false;
            capv.width = 0.1f;
            capv.useParentPositionAsStart = false;
            capv.SetMeshVisible(false);
        }
    }



    public override void OnHoldUpdate()
    {
        base.OnHoldUpdate();

        if (Player == null || m_attackCentre == null) return;

        // 1. calc number of targets we can lock on to based on hold percentage
        int numTargetsAllowed = (int)math.ceil(math.lerp(
            (float)m_holdStartTargets - 1,
            (float)m_holdFinishTargets,
            GetHoldPercentage()));

        // 2. find number of targets in range
        var attackCentrePos = m_attackCentre.transform.position;
        m_targets = FindTargetChain(attackCentrePos,
            numTargetsAllowed, m_chainRange);

        if (IsClient)
        {
            // 3. set all attack paths invisible
            foreach (var capv in m_chainAttackPathVisualizers) capv.SetMeshVisible(false);

            // 4. draw a attack path between each target
            for (int i = 0; i < m_targets.Count; i++)
            {
                Vector3 start;
                Vector3 finish;

                // first path is between player and target
                if (i == 0)
                {
                    start = m_attackCentre.transform.position;
                    finish = m_targets[0].transform.position + new Vector3(0, 0.5f, 0);
                }
                else
                {
                    start = m_targets[i - 1].transform.position + new Vector3(0, 0.5f, 0);
                    finish = m_targets[i].transform.position + new Vector3(0, 0.5f, 0);
                }

                var capv = m_chainAttackPathVisualizers[i];
                capv.SetMeshVisible(true);
                var dir = (finish - start).normalized;
                capv.forwardDirection = new Vector2(dir.x, dir.y);
                capv.innerStartPoint = 0f;
                capv.outerFinishPoint = math.distance(finish, start);
                capv.customStartPosition = start;
            }
        }

    }

    public override void OnHoldCancel()
    {
        base.OnHoldCancel();

        foreach (var sapv in m_chainAttackPathVisualizers) sapv.SetMeshVisible(false);
    }

    public override void OnHoldFinish()
    {
        base.OnHoldFinish();

        foreach (var sapv in m_chainAttackPathVisualizers) sapv.SetMeshVisible(false);
    }

    public List<GameObject> FindTargetChain(Vector3 attackCentrePosition, int numberTargets, float maxRange)
    {
        // 1. Find all GameObjects with EnemyCharacter or Destructible
        EnemyCharacter[] enemies = FindObjectsOfType<EnemyCharacter>();
        Destructible[] destructibles = FindObjectsOfType<Destructible>();

        // Combine all potential targets into a single list
        List<GameObject> potentialTargets = new List<GameObject>();
        foreach (var enemy in enemies)
        {
            potentialTargets.Add(enemy.gameObject);
        }
        foreach (var destructible in destructibles)
        {
            potentialTargets.Add(destructible.gameObject);
        }

        // 2. Initialize the result chain and a set to track used targets
        List<GameObject> targetChain = new List<GameObject>();
        HashSet<GameObject> usedTargets = new HashSet<GameObject>();

        // 3. Find the first target closest to the attackCentrePosition
        GameObject currentTarget = null;
        float closestDistance = float.MaxValue;

        foreach (var target in potentialTargets)
        {
            float distance = Vector3.Distance(attackCentrePosition, target.transform.position);
            if (distance <= maxRange && distance < closestDistance)
            {
                closestDistance = distance;
                currentTarget = target;
            }
        }

        // If no initial target is found, return an empty list
        if (currentTarget == null)
        {
            return targetChain;
        }

        // Add the first target to the chain and mark it as used
        targetChain.Add(currentTarget);
        usedTargets.Add(currentTarget);

        // 4. Find the subsequent targets in the chain
        while (targetChain.Count < numberTargets)
        {
            GameObject nextTarget = null;
            closestDistance = float.MaxValue;

            foreach (var target in potentialTargets)
            {
                // Skip targets that have already been used
                if (usedTargets.Contains(target)) continue;

                float distance = Vector3.Distance(targetChain[targetChain.Count - 1].transform.position, target.transform.position);
                if (distance <= maxRange && distance < closestDistance)
                {
                    closestDistance = distance;
                    nextTarget = target;
                }
            }

            // If no more valid targets are found, break out of the loop
            if (nextTarget == null)
            {
                break;
            }

            // Add the next target to the chain and mark it as used
            targetChain.Add(nextTarget);
            usedTargets.Add(nextTarget);
        }

        // 5. Return the target chain
        return targetChain;
    }
}
