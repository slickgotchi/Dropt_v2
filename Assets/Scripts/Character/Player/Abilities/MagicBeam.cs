using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class MagicBeam : PlayerAbility
{
    [Header("MagicBeam Parameters")]
    [SerializeField] float Projection = 0f;
    [SerializeField] float HoldStartDamageMultiplier = 0.5f;
    [SerializeField] float HoldFinishDamageMultiplier = 2.5f;

    private Collider2D m_collider;

    public override void OnNetworkSpawn()
    {
        m_collider = GetComponent<Collider2D>();
    }

    public override void OnStart()
    {
        // set local rotation/position.
        // IMPORTANT SetRotation(), SetRotationToActionDirection() and SetLocalPosition() must be used as
        // they call RPC's that sync remote clients
        SetRotationToActionDirection();
        SetLocalPosition(PlayerAbilityCentreOffset + ActivationInput.actionDirection * Projection);

        // determine hold damage multiplier
        var alpha = math.min(HoldDuration / 3f, 1f);
        var damageMultiplier = math.lerp(HoldStartDamageMultiplier, HoldFinishDamageMultiplier, alpha);

        // collision check (no RPC's are involved in this call)
        OneFrameCollisionDamageCheck(m_collider, Wearable.WeaponTypeEnum.Magic, damageMultiplier);

        // IMPORTANT use PlayAnimation which calls RPC's in the background that play the 
        // animation on remote clients
        //PlayAnimation("MagicBeam");
        PlayAnimationWithDuration("MagicBeam", ExecutionDuration);
    }

    public override void OnUpdate()
    {

    }

    public override void OnFinish()
    {

    }
}
