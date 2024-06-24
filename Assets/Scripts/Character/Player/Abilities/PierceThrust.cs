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

        OneFrameCollisionDamageCheck(m_collider, Wearable.WeaponTypeEnum.Pierce);

        // animation
        Animator.Play("PierceThrust");
        DebugDraw.DrawColliderPolygon(m_collider, IsServer);
        PlayAnimRemoteServerRpc("PierceThrust", AbilityOffset, AbilityRotation);
    }

    public override void OnUpdate()
    {
        TrackPlayerPosition();
    }

    public override void OnFinish()
    {
    }
}
