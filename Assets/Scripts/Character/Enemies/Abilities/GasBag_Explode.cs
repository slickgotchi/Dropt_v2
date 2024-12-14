using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class GasBag_Explode : EnemyAbility
{
    [Header("GasBag_Explode Parameters")]
    //public float ExplosionRadius = 3f;
    public float PoisonDuration = 5f;
    public float PoisonDamagePerSecond = 4f;

    [SerializeField] private Collider2D Collider;

    bool m_isExploded = false;

    private float m_stackTimer = 0f;

    private float m_durationTimer = 0f;

    public override void OnActivate()
    {
        if (Parent == null) return;
        if (m_isExploded) return;

        // set position
        //transform.position = Dropt.Utils.Battle.GetAttackCentrePosition(Parent);
        transform.position = Parent.GetComponent<Dropt.EnemyAI>().GetKnockbackPosition() + new Vector3(0, 0.5f, 0);
        //Debug.Log("OnActivate position: " + transform.position);

        m_isExploded = true;

        m_stackTimer = 0.5f;
        m_durationTimer = PoisonDuration;
    }

    public override void OnUpdate(float dt)
    {
        if (!m_isExploded) return;
        base.OnUpdate(dt);

        m_stackTimer -= dt;

        if (m_stackTimer < 0f)
        {
            HandleCollisions(Collider);
            m_stackTimer = 0.5f;
        }

    }

    private void HandleCollisions(Collider2D collider)
    {
        // sync colliders to current transform
        Physics2D.SyncTransforms();

        // do a collision check
        List<Collider2D> playerHitColliders = new List<Collider2D>();
        collider.OverlapCollider(PlayerAbility.GetContactFilter(new string[] { "PlayerHurt" }), playerHitColliders);
        foreach (var hit in playerHitColliders)
        {
            var player = hit.transform.parent;
            if (player.HasComponent<NetworkCharacter>())
            {
                // apply a stack of poison
                PoisonStack.ApplyPoisonStack(player.gameObject, PoisonDamagePerSecond, PoisonDuration, 5);
            }
        }

        // clear out colliders
        playerHitColliders.Clear();
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();

        if (Parent != null)
        {
            var parentNetworkObject = Parent.GetComponent<NetworkObject>();
            if (parentNetworkObject != null) parentNetworkObject.Despawn();
        }

        var networkObject = GetComponent<NetworkObject>();
        if (networkObject != null) networkObject.Despawn();
    }
}
