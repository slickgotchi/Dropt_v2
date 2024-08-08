using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyAbility : NetworkBehaviour
{
    [Header("Base EnemyAbility Parameters")]
    public float TelegraphDuration = 1f;
    public float ExecutionDuration = 1f;
    public float CooldownDuration = 1f;
    [HideInInspector] public GameObject Parent;
    [HideInInspector] public GameObject Target;
    [HideInInspector] public Vector3 AttackDirection;

    public bool isStartRotationAlignedWithParentDirection = false;
    public float axialOffsetWhenAlignedWithParentDirection = 0;

    private float m_timer = 0;
    private bool m_isActive = false;

    public enum State
    {
        None, Telegraph, Execution, Cooldown,
    }
    [HideInInspector] public State EnemyAbilityState = State.None;

    public override void OnNetworkSpawn()
    {
    }

    public void Activate()
    {
        m_isActive = true;
        m_timer = TelegraphDuration;
        EnemyAbilityState = State.Telegraph;
        if (IsServer) OnTelegraphStart();
    }

    private void Update()
    {
        m_timer -= Time.deltaTime;

        switch (EnemyAbilityState)
        {
            case State.Telegraph:
                if (m_timer <= 0)
                {
                    m_timer = ExecutionDuration;
                    EnemyAbilityState = State.Execution;
                    if (IsServer) OnExecutionStart();
                }
                break;
            case State.Execution:
                if (m_timer <= 0)
                {
                    m_timer = CooldownDuration;
                    EnemyAbilityState = State.Cooldown;
                    if (IsServer) OnCooldownStart();
                }
                break;
            case State.Cooldown:
                if (m_timer <= 0)
                {
                    m_timer = 0;
                    EnemyAbilityState = State.None;
                    if (IsServer) OnFinish();
                    if (IsServer) GetComponent<NetworkObject>().Despawn();
                    m_isActive = false;
                }
                break;
            case State.None: break;
            default: break;
        }

        if (m_isActive && IsServer) OnUpdate();
    }

    public virtual void OnTelegraphStart() { }
    public virtual void OnExecutionStart() { }
    public virtual void OnCooldownStart() { }
    public virtual void OnFinish() { }
    public virtual void OnUpdate() { }

    [Rpc(SendTo.ClientsAndHost)]
    protected void SpawnBasicCircleClientRpc(Vector3 position, Color color, float explosionRadius)
    {
        VisualEffectsManager.Singleton.SpawnBasicCircle(position, color, explosionRadius);
    }

    public static void PlayerCollisionCheckAndDamage(Collider2D collider, float damage, 
        bool isCritical = false, GameObject damageDealer = null)
    {
        // sync colliders to current transform
        Physics2D.SyncTransforms();

        // do a collision check
        List<Collider2D> playerHitColliders = ListPool<Collider2D>.Get();

        collider.Overlap(PlayerAbility.GetContactFilter(new string[] { "PlayerHurt" }), playerHitColliders);
        foreach (var hit in playerHitColliders)
        {
            var player = hit.transform.parent;
            if (player.HasComponent<NetworkCharacter>())
            {
                player.GetComponent<NetworkCharacter>().TakeDamage(damage, isCritical, damageDealer);
            }
        }

        // clear out colliders
        ListPool<Collider2D>.Release(playerHitColliders);
    }
}