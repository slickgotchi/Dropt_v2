using System.Collections;
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
        OnTelegraphStart();
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
                    OnExecutionStart();
                }
                break;
            case State.Execution:
                if (m_timer <= 0)
                {
                    m_timer = CooldownDuration;
                    EnemyAbilityState = State.Cooldown;
                    OnCooldownStart();
                }
                break;
            case State.Cooldown:
                if (m_timer <= 0)
                {
                    m_timer = 0;
                    EnemyAbilityState = State.None;
                    OnFinish();
                    GetComponent<NetworkObject>().Despawn();
                    m_isActive = false;
                }
                break;
            case State.None: break;
            default: break;
        }

        if (m_isActive) OnUpdate();
    }

    public virtual void OnTelegraphStart() { }
    public virtual void OnExecutionStart() { }
    public virtual void OnCooldownStart() { }
    public virtual void OnFinish() { }
    public virtual void OnUpdate() { }

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

    public static void FillPlayerCollisionCheckAndDamage(List<NetworkCharacter> result, Collider2D collider, float damage,
        bool isCritical = false, GameObject damageDealer = null)
    {
        // sync colliders to current transform
        Physics2D.SyncTransforms();

        // do a collision check
        List<Collider2D> playerHitColliders = ListPool<Collider2D>.Get();
        // clear out colliders
        playerHitColliders.Clear();

        collider.Overlap(PlayerAbility.GetContactFilter(new string[] { "PlayerHurt" }), playerHitColliders);
        foreach (var hit in playerHitColliders)
        {
            var player = hit.transform.parent;
            if (player.HasComponent<NetworkCharacter>())
            {
                player.GetComponent<NetworkCharacter>().TakeDamage(damage, isCritical, damageDealer);
                result.Add(player.GetComponent<NetworkCharacter>());
            }
        }

        ListPool<Collider2D>.Release(playerHitColliders);
    }
}
