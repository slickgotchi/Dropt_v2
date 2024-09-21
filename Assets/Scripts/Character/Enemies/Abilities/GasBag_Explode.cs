using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class GasBag_Explode : EnemyAbility
{
    [Header("GasBag_Explode Parameters")]
    public float ExplosionRadius = 3f;
    public float PoisonDuration = 5f;
    public float PoisonDamagePerSecond = 4f;

    [SerializeField] private Collider2D Collider;

    bool m_isExploded = false;

    private void Awake()
    {
    }

    public override void OnActivate()
    {
        if (Parent == null) return;
        if (m_isExploded) return;

        // set position
        transform.position = Dropt.Utils.Battle.GetAttackCentrePosition(Parent);

        // resize explosion collider and check collisions
        Collider.GetComponent<CircleCollider2D>().radius = ExplosionRadius;
        HandleCollisions(Collider);

        // do visual explosion
        SpawnBasicCircleClientRpc(
            transform.position,
            Dropt.Utils.Color.HexToColor("#7a09fa", 0.5f),
            ExplosionRadius);

        Parent.GetComponent<NetworkObject>().Despawn();

        m_isExploded = true;
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
                PoisonStack.ApplyPoisonStack(player.gameObject, 3, 10, 5);
            }
        }

        // clear out colliders
        playerHitColliders.Clear();
    }
}
