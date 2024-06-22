using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class PierceThrust : PlayerAbility
{
    [Header("Pierce Thrust Parameters")]
    [SerializeField] float Projection = 1f;

    private Collider2D m_collider;

    public override void OnNetworkSpawn()
    {
        Animator = GetComponent<Animator>();
        m_collider = GetComponent<Collider2D>();
    }

    public override void OnStart(bool isServer = false)
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
        List<Collider2D> hitColliders = new List<Collider2D>();
        m_collider.Overlap(GetEnemyHurtContactFilter(), hitColliders);
        foreach (var hit in hitColliders)
        {
            if (hit.HasComponent<NetworkCharacter>())
            {
                var playerCharacter = Player.GetComponent<NetworkCharacter>();
                var damage = playerCharacter.GetAttackPower();
                var isCritical = playerCharacter.IsCriticalAttack();
                damage = (int)(isCritical ? damage * playerCharacter.CriticalDamage.Value : damage);
                hit.GetComponent<NetworkCharacter>().TakeDamage(damage, isCritical, isServer);
                Player.GetComponent<PlayerCamera>().Shake(1.5f, 0.3f);
            }
        }
        hitColliders.Clear();

        // do client side effects/visuals etc.
        if (!isServer)
        {
            if (Player.GetComponent<NetworkObject>().IsLocalPlayer)
            {
                Animator.Play("PierceThrust");
                DebugDraw.DrawColliderPolygon(m_collider, IsServer);
                PlayAnimRemoteServerRpc("PierceThrust", AbilityOffset, AbilityRotation);
            }
        }
    }

    private void Update()
    {
        TrackPlayerPosition();
    }

    public override void OnFinish(bool isServer = false)
    {
    }
}
