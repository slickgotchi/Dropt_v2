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

    public void Init(GameObject parent, GameObject target, float executionDuration)
    {
        if (parent == null) return;
        if (target == null) return;

        Parent = parent;
        Target = target;
        ExecutionDuration = executionDuration;
        AttackDirection = (target.transform.position - parent.transform.position).normalized;

        OnInit();
    }

    public void Activate()
    {
        OnActivate();
        m_timer = ExecutionDuration;
        m_isActive = true;
    }

    private void Update()
    {
        m_timer -= Time.deltaTime;

        if (m_timer < 0 && m_isActive)
        {
            OnDeactivate();
            m_isActive = false;
        } else
        {
            OnUpdate(Time.deltaTime);
        }


        //m_timer -= Time.deltaTime;

        //switch (EnemyAbilityState)
        //{
        //    case State.Telegraph:
        //        if (m_timer <= 0)
        //        {
        //            m_timer = ExecutionDuration;
        //            EnemyAbilityState = State.Execution;
        //            if (IsServer) OnExecutionStart();
        //        }
        //        break;
        //    case State.Execution:
        //        if (m_timer <= 0)
        //        {
        //            m_timer = CooldownDuration;
        //            EnemyAbilityState = State.Cooldown;
        //            if (IsServer) OnCooldownStart();
        //        }
        //        break;
        //    case State.Cooldown:
        //        if (m_timer <= 0)
        //        {
        //            m_timer = 0;
        //            EnemyAbilityState = State.None;
        //            if (IsServer) OnFinish();
        //            if (IsServer) GetComponent<NetworkObject>().Despawn();
        //            m_isActive = false;
        //        }
        //        break;
        //    case State.None: break;
        //    default: break;
        //}

        //if (m_isActive && IsServer) OnUpdate();
    }

    public virtual void OnTelegraphStart() { }
    public virtual void OnExecutionStart() { }
    public virtual void OnCooldownStart() { }
    public virtual void OnFinish() { }
    public virtual void OnUpdate(float dt) { }

    public virtual void OnInit() { }
    public virtual void OnActivate() { }
    public virtual void OnDeactivate() { }

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

        collider.OverlapCollider(PlayerAbility.GetContactFilter(new string[] { "PlayerHurt" }), playerHitColliders);
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