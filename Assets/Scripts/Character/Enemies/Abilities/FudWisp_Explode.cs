using CarlosLab.UtilityIntelligence;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class FudWisp_Explode : EnemyAbility
{
    [Header("FudWisp_Explode Parameters")]
    public float ExplosionDuration = 1f;
    public float ExplosionRadius = 3f;
    public float RootDuration = 3f;

    [SerializeField] private Collider2D Collider;

    private float m_explosionTimer = 0;

    private void Awake()
    {
        transform.localScale = new Vector3(ExplosionRadius * 2, ExplosionRadius * 2, 1);
    }

    public override void OnTelegraphStart()
    {
        Debug.Log("OnTelegraphStart()");
    }

    public override void OnExecutionStart()
    {
        Debug.Log("OnExecutionStart()");

        // set position of explosion and initial scale
        transform.position = Parent.transform.position + new Vector3(0, 0.6f, 0);

        // resize explosion collider and check collisions
        Collider.GetComponent<CircleCollider2D>().radius = ExplosionRadius;
        HandleCollisions(Collider);

        // destroy the parent object
        if (Parent != null)
        {
            transform.parent = null;
            Parent.GetComponent<UtilityAgentFacade>().Destroy();
        }

        // spawn visual effect
        //SpawnFudWispExplosionClientRpc(transform.position, ExplosionRadius);
        SpawnBasicCircleClientRpc(
            transform.position,
            Dropt.Utils.Color.HexToColor("#99e65f", 0.5f),
            ExplosionRadius);
    }

    //[Rpc(SendTo.ClientsAndHost)]
    //void SpawnFudWispExplosionClientRpc(Vector3 position, float explosionRadius)
    //{
    //    Color color;
    //    ColorUtility.TryParseHtmlString("#99e65f", out color);
    //    color.a = 0.5f;
    //    VisualEffectsManager.Singleton.SpawnFudWispExplosion(position, color, explosionRadius);
    //}

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
                // apply rooted
                player.GetComponent<CharacterStatus>().SetRooted(true, RootDuration);
            }
        }

        // clear out colliders
        playerHitColliders.Clear();
    }

    public override void OnUpdate()
    {
        m_explosionTimer += Time.deltaTime;
        if (m_explosionTimer > ExplosionDuration)
        {
            if (IsServer) GetComponent<NetworkObject>().Despawn();
            return;
        }
    }
}
