using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class FudWisp_Explode : EnemyAbility
{
    [Header("FudWisp_Explode Parameters")]
    public float ExplosionDuration = 1f;
    public float RootDuration = 3f;

    [SerializeField] private Collider2D Collider;

    private float m_explosionTimer = 0;


    public override void OnActivate()
    {
        base.OnActivate();

        if (Parent == null) return;

        // set position
        transform.position = Parent.transform.position;

        // resize explosion collider and check collisions
        HandleCollisions(Collider);

        // destroy the parent object
        if (Parent != null)
        {
            transform.parent = null;
            Parent.GetComponent<NetworkObject>().Despawn();
        }
    }

    private void HandleCollisions(Collider2D collider)
    {
        // sync colliders to current transform
        Physics2D.SyncTransforms();

        // create a collision check using OverlapCollider
        List<Collider2D> playerHitColliders = new List<Collider2D>();
        ContactFilter2D contactFilter = PlayerAbility.GetContactFilter(new string[] { "PlayerHurt" });
        collider.OverlapCollider(contactFilter, playerHitColliders); // Replacing Overlap method

        foreach (var hit in playerHitColliders)
        {
            var player = hit.transform.parent;
            var networkCharacter = player.GetComponent<NetworkCharacter>();
            var characterStatus = player.GetComponent<CharacterStatus>();

            if (networkCharacter != null && characterStatus != null && !characterStatus.IsRooted())
            {
                // apply rooted
                characterStatus.SetRooted(true, RootDuration);
            }
        }

        // clear out colliders
        playerHitColliders.Clear();
    }

    public override void OnUpdate(float dt)
    {
        m_explosionTimer += Time.deltaTime;
        if (m_explosionTimer > ExplosionDuration)
        {
            if (IsServer) GetComponent<NetworkObject>().Despawn();
            return;
        }
    }
}
