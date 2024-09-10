using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Unity.Mathematics;

public class EnemyController : NetworkBehaviour
{
    [Header("Rendering Parameters")]
    public SpriteRenderer SpriteToFlip;
    public enum Facing { Left, Right }
    [HideInInspector] public Facing FacingDirection;

    private NavMeshAgent m_navMeshAgent;

    // private parameters for updating facing position
    private LocalVelocity m_localVelocity;
    private float m_facingTimer = 0f;

    // spawn parameters
    [Header("Spawn Parameters")]
    [HideInInspector] public bool IsSpawning = true;
    public float SpawnDuration = 0f;

    [HideInInspector] public bool IsArmed = false;

    [Header("Dynamic HP")]
    public float DynamicHpMultiplier = 1f;

    private void Awake()
    {
        m_localVelocity = GetComponent<LocalVelocity>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient && !IsHost)
        {
            Debug.Log("Remove NavMeshAgent from client side enemy");
            Destroy(GetComponent<NavMeshAgent>());
        } else
        {
            m_navMeshAgent = GetComponent<NavMeshAgent>();
            m_navMeshAgent.updateRotation = false;
            m_navMeshAgent.updateUpAxis = false;
            m_navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            m_navMeshAgent.enabled = true;

        }

        ApplyDynamicHp();
    }

    public override void OnNetworkDespawn()
    {
    }

    void ApplyDynamicHp()
    {
        if (!IsServer) return;

        var playerCharacters = FindObjectsByType<PlayerCharacter>(FindObjectsSortMode.None);
        var netAttackPowers = new List<float>();
        foreach (var pc in playerCharacters)
        {
            netAttackPowers.Add(GetPlayerNetAttackPower(pc));
        }

        netAttackPowers.Sort((a, b) => b.CompareTo(a));

        // now assign dynamic HP
        float dynamicHp = 0;
        if (netAttackPowers.Count == 1)
        {
            dynamicHp = netAttackPowers[0];
        } else if (netAttackPowers.Count == 2)
        {
            dynamicHp = netAttackPowers[0] + netAttackPowers[1] * 0.5f;
        } else if (netAttackPowers.Count == 3)
        {
            dynamicHp = netAttackPowers[0] + netAttackPowers[1] * 0.5f + netAttackPowers[2] * 0.25f;
        }

        dynamicHp *= DynamicHpMultiplier;

        GetComponent<NetworkCharacter>().HpMax.Value += dynamicHp;
        GetComponent<NetworkCharacter>().HpCurrent.Value += dynamicHp;
    }

    float GetPlayerNetAttackPower(PlayerCharacter playerCharacter)
    {
        var playerEquipment = playerCharacter.GetComponent<PlayerEquipment>();
        var lhWearableNameEnum = playerEquipment.LeftHand.Value;
        var rhWearableNameEnum = playerEquipment.RightHand.Value;

        var lhWearable = WearableManager.Instance.GetWearable(lhWearableNameEnum);
        var rhWearable = WearableManager.Instance.GetWearable(rhWearableNameEnum);

        float lhRarityMultiplier = lhWearable == null ? 1 : lhWearable.RarityMultiplier;
        float rhRarityMultiplier = rhWearable == null ? 1 : rhWearable.RarityMultiplier;

        float rarityMultiplier = math.max(lhRarityMultiplier, rhRarityMultiplier);

        var baseAttack = playerCharacter.AttackPower.Value * rarityMultiplier;
        var baseCrit = playerCharacter.CriticalChance.Value;

        float netAttackPower = (baseAttack * (1 - baseCrit) + (baseAttack * 2 * baseCrit));

        return netAttackPower;
    }

    // Update is called once per frame
    void Update()
    {
        HandleFacing();
    }

    public void SetFacingFromDirection(Vector3 direction, float facingTimer)
    {
        SetFacing(direction.x > 0 ? Facing.Right : Facing.Left, facingTimer);
    }

    public void SetFacing(Facing facingDirection, float facingTimer = 0.5f)
    {
        m_facingTimer = facingTimer;
        FacingDirection = facingDirection;
        SpriteToFlip.flipX = FacingDirection == Facing.Left ? true : false;
    }

    public void HandleFacing()
    {
        if (SpriteToFlip == null) return;
        if (m_navMeshAgent == null) return;
        if (math.abs(m_navMeshAgent.velocity.x) < 0.02f) return;

        m_facingTimer -= Time.deltaTime;
        if (m_facingTimer > 0f) return;

        FacingDirection = m_navMeshAgent.velocity.x < 0 ? Facing.Left : Facing.Right;
        SpriteToFlip.flipX = FacingDirection == Facing.Left ? true : false;
    }
}
