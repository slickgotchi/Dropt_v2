using CarlosLab.UtilityIntelligence;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class GasBag_Explode : EnemyAbility
{
    [Header("GasBag_Explode Parameters")]
    public float ExplosionDuration = 1f;
    public float ExplosionRadius = 3f;
    public float PoisonDuration = 5f;
    public float PoisonDamagePerSecond = 4f;

    [SerializeField] private Transform SpriteTransform;
    [SerializeField] private Collider2D Collider;

    private float m_explosionGrowFadeTimer = 0;

    private void Awake()
    {
        SpriteTransform.localScale = Vector3.one;
    }

    public override void OnTelegraphStart()
    {
    }

    public override void OnExecutionStart()
    {
        // reset explosion fade timer & set fade out duration
        m_explosionGrowFadeTimer = 0f;
        GetComponentInChildren<FadeOut>().duration = ExplosionDuration;

        // set position of explosion and initial scale
        transform.position = Parent.transform.position + new Vector3(0, 0.6f, 0);
        SpriteTransform.localScale = new Vector3(0f, 0f, 1);

        // resize explosion collider and check collisions
        Collider.GetComponent<CircleCollider2D>().radius = ExplosionRadius;
        HandleCollisions(Collider);

        // destroy the parent object
        if (Parent != null)
        {
            transform.parent = null;
            Parent.GetComponent<UtilityAgentFacade>().Destroy();
        }
    }

    private void HandleCollisions(Collider2D collider)
    {
        // sync colliders to current transform
        Physics2D.SyncTransforms();

        // do a collision check
        List<Collider2D> playerHitColliders = new List<Collider2D>();
        collider.Overlap(PlayerAbility.GetContactFilter(new string[] { "PlayerHurt" }), playerHitColliders);
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

    public override void OnUpdate()
    {
        m_explosionGrowFadeTimer += Time.deltaTime;
        if (m_explosionGrowFadeTimer > ExplosionDuration)
        {
            if (IsServer) GetComponent<NetworkObject>().Despawn();
            return;
        }

        m_explosionGrowFadeTimer = math.min(m_explosionGrowFadeTimer, ExplosionDuration);
        var alpha = m_explosionGrowFadeTimer / ExplosionDuration;

        SpriteTransform.localScale = new Vector3(alpha * ExplosionRadius * 2, alpha * ExplosionRadius * 2, 1);
    }
}
