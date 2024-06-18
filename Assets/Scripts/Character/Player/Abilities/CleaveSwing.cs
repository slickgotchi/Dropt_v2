using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class CleaveSwing : PlayerAbility
{
    [Header("Cleave Swing Parameters")]
    [SerializeField] float Projection = 1f;

    private Animator m_animator;
    private Collider2D m_collider;

    public override void OnNetworkSpawn()
    {
        m_animator = GetComponent<Animator>();
        m_collider = GetComponent<Collider2D>();
    }

    public override void OnStart()
    {
        // Play the attack animation
        if (IsClient)
        {
            m_animator.Play("CleaveSwing");
        }

        // set transform to activation rotation/position
        transform.rotation = GetRotationFromDirection(PlayerActivationInput.actionDirection);
        transform.position = PlayerActivationState.position + PlayerCenterOffset +
            PlayerActivationInput.actionDirection * Projection;

        // sync colliders to current transform
        Physics2D.SyncTransforms();

        // do a collision check
        List<Collider2D> hitColliders = new List<Collider2D>();
        m_collider.Overlap(GetEnemyHurtContactFilter(), hitColliders);
        DebugDraw.DrawColliderPolygon(m_collider, IsServer);
        foreach (var hit in hitColliders)
        {
            if (IsClient)
            {
                if (hit.HasComponent<SpriteFlash>())
                {
                    hit.GetComponent<SpriteFlash>().DamageFlash();
                    Player.GetComponent<PlayerCamera>().Shake(0.5f, 0.3f);
                }
            }
            if (IsServer)
            {
                hit.GetComponent<NetworkCharacter>().HpCurrent.Value -= 20;
            }
        }

        hitColliders.Clear();
    }

    private void Update()
    {
        transform.position = GetPlayerCentrePosition() + PlayerActivationInput.actionDirection * Projection;
    }

    public override void OnFinish()
    {
    }
}
