using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class CleaveSwing : PlayerAbility
{
    [Header("CleaveSwing Parameters")]
    [SerializeField] float Projection = 1f;

    private Collider2D m_collider;

    public override void OnNetworkSpawn()
    {
        Animator = GetComponent<Animator>();
        m_collider = GetComponent<Collider2D>();
    }

    public override void OnStart()
    {
        AbilityOffset = PlayerCenterOffset + PlayerActivationInput.actionDirection * Projection;
        AbilityRotation = GetRotationFromDirection(PlayerActivationInput.actionDirection);

        // set transform to activation rotation/position
        transform.rotation = AbilityRotation;
        transform.position = PlayerActivationState.position + AbilityOffset;

        OneFrameCollisionDamageCheck(m_collider, Wearable.WeaponTypeEnum.Cleave);

        // animation
        Animator.Play("CleaveSwing");
        DebugDraw.DrawColliderPolygon(m_collider, IsServer);
        PlayAnimRemoteServerRpc("CleaveSwing", AbilityOffset, AbilityRotation);
    }



    public override void OnUpdate()
    {
        TrackPlayerPosition();
    }

    public override void OnFinish()
    {
    }
}
