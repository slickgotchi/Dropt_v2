using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyAbility : NetworkBehaviour
{
    [Header("Base EnemyAbility Parameters")]
    //public float TelegraphDuration = 1f;
    public float ExecutionDuration = 1f;
    //public float CooldownDuration = 1f;
    [HideInInspector] public GameObject Parent;
    [HideInInspector] public GameObject Target;
    [HideInInspector] public Vector3 AttackDirection;
    [HideInInspector] public Vector3 PositionToAttack;

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
        base.OnNetworkSpawn();
    }

    public void Activate(GameObject parent, GameObject target,
        Vector3 attackDirection, float executionDuration, Vector3 positionToAttack)
    {
        if (parent == null) return;
        if (target == null) return;

        Parent = parent;
        Target = target;
        AttackDirection = attackDirection.normalized;
        ExecutionDuration = executionDuration;
        PositionToAttack = positionToAttack;

        OnActivate();
        m_timer = ExecutionDuration;
        m_isActive = true;
    }

    public void Deactivate()
    {
        OnDeactivate();
        m_isActive = false;

        if (IsServer)
        {
            var networkObject = GetComponent<NetworkObject>();
            if (networkObject != null) GetComponent<NetworkObject>().Despawn();
        }
    }

    private void Update()
    {
        m_timer -= Time.deltaTime;
        if (m_timer < 0 && m_isActive)
        {
            Deactivate();
        } else
        {
            OnUpdate(Time.deltaTime);
        }
    }

    //public virtual void OnTelegraphStart() { }
    //public virtual void OnExecutionStart() { }
    //public virtual void OnCooldownStart() { }
    //public virtual void OnFinish() { }
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