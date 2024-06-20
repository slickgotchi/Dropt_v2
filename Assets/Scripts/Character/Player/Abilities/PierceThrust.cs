using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class PierceThrust : PlayerAbility
{
    [Header("PierceThrust Parameters")]
    [SerializeField] float Projection = 1f;

    private Collider2D m_collider;

    public override void OnNetworkSpawn()
    {
        Animator = GetComponent<Animator>();
        m_collider = GetComponent<Collider2D>();
    }

    public override void OnStart()
    {
        // setup offset and rotation for tracking
        AbilityOffset = PlayerCenterOffset + PlayerActivationInput.actionDirection * Projection;
        AbilityRotation = GetRotationFromDirection(PlayerActivationInput.actionDirection);

        // set transform to activation rotation/position
        transform.rotation = AbilityRotation;
        transform.position = PlayerActivationState.position + AbilityOffset;

        // sync colliders to current transform
        Physics2D.SyncTransforms();

        // do a collision check
        List<Collider2D> enemyHitColliders = new List<Collider2D>();
        m_collider.Overlap(GetContactFilter("EnemyHurt"), enemyHitColliders);
        bool isLocalPlayer = Player.GetComponent<NetworkObject>().IsLocalPlayer;
        foreach (var hit in enemyHitColliders)
        {
            if (hit.HasComponent<NetworkCharacter>())
            {
                var playerCharacter = Player.GetComponent<NetworkCharacter>();
                var damage = playerCharacter.GetAttackPower();
                var isCritical = playerCharacter.IsCriticalAttack();
                damage = (int)(isCritical ? damage * playerCharacter.CriticalDamage.Value : damage);
                hit.GetComponent<NetworkCharacter>().TakeDamage(damage, isCritical);
            }
        }

        // screen shake
        if (isLocalPlayer && enemyHitColliders.Count > 0)
        {
            Player.GetComponent<PlayerCamera>().Shake(1.5f, 0.3f);
        }

        // clear out colliders
        enemyHitColliders.Clear();

        // animation
        if (isLocalPlayer)
        {
            Animator.Play("PierceThrust");
            DebugDraw.DrawColliderPolygon(m_collider, IsServer);
            PlayAnimRemoteServerRpc("PierceThrust", AbilityOffset, AbilityRotation);
        }
    }

    public override void OnUpdate()
    {
        TrackPlayerPosition();
    }

    public override void OnFinish()
    {
    }
}
