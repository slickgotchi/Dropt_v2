using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;
using System.Linq;
using Random = UnityEngine.Random;

public class PetController : NetworkBehaviour
{
    [SerializeField] private PetSettings m_petSettings;

    private PetStateMachine m_petStateMachine;

    private PetView m_petView;
    private PetMeter m_petMeter;
    private EnemyDetactor m_enemyDetactor;

    private NavMeshAgent m_agent;

    private Transform m_petOwner;
    private Transform m_transform;

    private ulong m_ownerObjectId;

    private readonly NetworkVariable<Vector3> m_petPosition = new(Vector3.zero);

    private float m_damageMultiplier;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        m_petView = GetComponent<PetView>();
        m_transform = transform;
        if (!IsServer && !IsHost)
        {
            m_petPosition.OnValueChanged += OnPetPositionChange;
            return;
        }
        m_petPosition.Value = transform.position;
        InitializeNavmeshAgent();
        m_petMeter = GetComponent<PetMeter>();
        m_petOwner = NetworkManager.SpawnManager.SpawnedObjects[m_ownerObjectId].transform;
        m_enemyDetactor = m_petOwner.GetComponent<EnemyDetactor>();
        m_petStateMachine = new PetStateMachine(this);
        m_petStateMachine.ChangeState(m_petStateMachine.PetFollowOwnerState);
        ActivatePetMeterViewClientRpc(m_ownerObjectId);
    }

    private void OnPetPositionChange(Vector3 previousValue, Vector3 newValue)
    {
        if (Vector3.Distance(m_transform.position, newValue) > 4.0f)
        {
            m_transform.position = newValue;
        }
    }

    public ulong GetPetOwnerNetworkObjectId()
    {
        if (m_petOwner != null)
        {
            return m_petOwner.GetComponent<NetworkObject>().NetworkObjectId;
        }

        return 0;
    }

    public bool IsSummonDurationOver()
    {
        return m_petMeter.IsSummonDurationOver();
    }

    public void DrainSummonDuration()
    {
        m_petMeter.DrainSummonDuration();
    }

    public void RemoveDestination()
    {
        m_agent.ResetPath();
    }

    [ClientRpc]
    public void ActivatePetMeterViewClientRpc(ulong id)
    {
        if (IsOwnerOfPet(id))
        {
            PlayerHUDCanvas.Instance.ActivatePetMeter(m_petView.m_downSprite);
        }
    }

    public bool IsDeactivated()
    {
        return m_petView.IsActivated();
    }

    private bool IsOwnerOfPet(ulong id)
    {
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController player in players)
        {
            if (player.IsLocalPlayer && player.NetworkObjectId == id)
            {
                return true;
            }
        }
        return false;
    }

    public float GetMeterValue()
    {
        return m_petMeter.GetMeterValue();
    }

    private void InitializeNavmeshAgent()
    {
        m_agent = gameObject.AddComponent<NavMeshAgent>();
        m_agent.updateRotation = false;
        m_agent.updateUpAxis = false;
        m_agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        m_agent.speed = m_petSettings.Speed;
        m_agent.stoppingDistance = m_petSettings.OffsetDistance;
        m_agent.enabled = true;
    }

    private Vector3 m_velocity = Vector3.zero;

    private void Update()
    {
        //if (m_petOwner == null) return;

        if (IsServer && m_petOwner != null)
        {
            m_petStateMachine.Update();
            m_petPosition.Value = m_transform.position;
            return;
        }

        //m_transform.position = Vector3.Lerp(m_transform.position, m_petPosition.Value, m_petSettings.Speed * Time.deltaTime);
        m_transform.position = Vector3.SmoothDamp(m_transform.position, m_petPosition.Value, ref m_velocity, 0.3f);
    }

    public void FollowOwner()
    {
        _ = m_agent.SetDestination(m_petOwner.position);
    }

    public bool IsPetReachToDestination()
    {
        return m_agent.remainingDistance <= m_agent.stoppingDistance;
    }

    public bool IsPlayerOutOfTeleportRange()
    {
        float distanceToPlayer = Vector3.Distance(m_transform.position, m_petOwner.position);
        return distanceToPlayer > m_petSettings.TeleportDistance;
    }

    public void TeleportCloseToPlayer()
    {
        Vector3 randomDirection = Random.insideUnitSphere * m_petSettings.OffsetDistance;
        randomDirection += m_petOwner.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, m_petSettings.OffsetDistance, NavMesh.AllAreas))
        {
            m_agent.Warp(navHit.position);
        }
    }

    public void SetFacingDirection()
    {
        Vector3 velocity = m_agent.velocity;
        if (velocity.magnitude > 0.1f)
        {
            ChangeSprite(Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y) ?
               (velocity.x > 0 ? Direction.Right : Direction.Left)
               : (velocity.y > 0 ? Direction.Up : Direction.Down));
        }
    }

    public void SetFacingDirectionToOwnner()
    {
        SetFacingDirection(m_petOwner);
    }

    public void SetFacingDirection(Transform facingTransform)
    {
        Vector3 direction = (transform.position - facingTransform.position).normalized;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            ChangeSprite(direction.x > 0 ? Direction.Left : Direction.Right);
        }
        else
        {
            ChangeSprite(direction.y > 0 ? Direction.Down : Direction.Up);
        }
    }

    private void ChangeSprite(Direction direction)
    {
        m_petView.SetSprite(direction);
        SetFacingSpriteClientRpc(direction);
    }

    [ClientRpc]
    public void SetFacingSpriteClientRpc(Direction direction)
    {
        if (IsServer)
        {
            return;
        }

        m_petView.SetSprite(direction);
    }

    public void InitializePet(ulong ownerObjectId, float damageMulitplier)
    {
        m_ownerObjectId = ownerObjectId;
        m_damageMultiplier = damageMulitplier;
    }

    public ulong GetPlayerNetworkObjectId()
    {
        return m_ownerObjectId;
    }

    public void DeactivatePet()
    {
        if (IsServer && !IsHost)
        {
            m_petView.Hide();
        }
        DeactivatePetViewClientRpc();
    }

    [ClientRpc]
    public void DeactivatePetViewClientRpc()
    {
        m_petView.Hide();
    }

    public void ActivatePet()
    {
        if (IsServer && !IsHost)
        {
            m_petView.Show();
        }

        ActivatePetViewClientRpc();
    }

    [ClientRpc]
    public void ActivatePetViewClientRpc()
    {
        m_petView.Show();
    }

    public void ActivateAgent()
    {
        m_agent.enabled = true;
    }

    public void DeactivateAgent()
    {
        m_agent.enabled = false;
    }

    public void SubscribeOnEnemyGetDamage()
    {
        NetworkCharacter networkCharacter = m_petOwner.GetComponent<NetworkCharacter>();
        EventManager.Instance.StartListeningWithParam("OnEnemyGetDamage", OnEnemyGetDamage);
    }

    public void UnsubscribeOnEnemyGetDamage()
    {
        NetworkCharacter networkCharacter = m_petOwner.GetComponent<NetworkCharacter>();
        EventManager.Instance.StopListeningWithParam("OnEnemyGetDamage", OnEnemyGetDamage);
    }

    private void OnEnemyGetDamage(object delearId)
    {
        if ((ulong)delearId != m_ownerObjectId)
        {
            return;
        }
        m_petMeter.Recharge();
    }

    public void DrainPetMeter()
    {
        m_petMeter.Drain();
    }

    public void SetPetMeterProgress()
    {
        SetPetMeterProgressClientRpc(m_ownerObjectId, m_petMeter.GetProgress());
    }

    public void SetSummonProgress()
    {
        SetPetMeterProgressClientRpc(m_ownerObjectId, m_petMeter.GetSummonProgress());
    }

    [ClientRpc]
    public void SetPetMeterProgressClientRpc(ulong id, float progress)
    {
        if (IsOwnerOfPet(id))
        {
            PlayerHUDCanvas.Instance.SetPetMeterProgress(progress);
        }
    }

    public bool IsEnemyInPlayerRange()
    {
        return m_enemyDetactor.DetectEnemies().Length > 0;
    }

    public Transform GetEnemyInPlayerRange(Transform previousEnemy)
    {
        Collider2D[] enemiesCollider = m_enemyDetactor.DetectEnemies();
        if (enemiesCollider.Length == 0)
        {
            return null;
        }

        if (enemiesCollider.Length == 1)
        {
            return enemiesCollider[0].transform;
        }

        Transform[] validEnemies = enemiesCollider
        .Select(collider => collider.transform) // Select enemy transforms
        .Where(transform => transform != previousEnemy) // Filter out the previous enemy
        .ToArray();

        return validEnemies.Length == 0
               ? null
               : validEnemies[Random.Range(0, validEnemies.Length)];
    }

    public bool IsPetMeterFullyCharged()
    {
        return m_petMeter.IsFullyCharged();
    }

    [ClientRpc]
    public void CloudExplosionClientRpc()
    {
        GameObject cloud = VisualEffectsManager.Instance.SpawnCloudExplosion(transform.position);
        cloud.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }

    public void SpawnAttackAnimation(Transform enemyTransform)
    {
        AttackCentre attackCentre = enemyTransform.GetComponentInChildren<AttackCentre>();
        attackCentre.SpawnAttackEffect();
    }

    public float GetAttackInterval()
    {
        return m_petMeter.attackInterval;
    }

    public void TeleportCloseToEnemy(Transform enemy)
    {
        Teleport teleport = new(m_agent, enemy, 2.0f);
        teleport.Start();
    }

    public float GetSummonDuration()
    {
        return m_petMeter.summonDuration;
    }

    public void ResetSummonDuration()
    {
        m_petMeter.ResetSummonDuration();
    }

    public void ApplyDamageToEnemy(Transform enemyTransform)
    {
        float damage = m_petMeter.basePetAttackMultiplier * m_damageMultiplier * m_petOwner.GetComponent<NetworkCharacter>().currentStaticStats.AttackPower;
        NetworkCharacter networkCharacter = enemyTransform.GetComponent<NetworkCharacter>();
        networkCharacter.TakeDamage(damage, false, m_petOwner.gameObject);
    }

    public void ResetPetMeter()
    {
        m_petMeter.ResetCharge();
    }
}